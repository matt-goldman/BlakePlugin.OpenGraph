using Blake.BuildTools;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BlakePlugin.OpenGraph;

public partial class Plugin : IBlakePlugin
{
    public async Task AfterBakeAsync(BlakeContext context, ILogger? logger = null)
    {
        logger?.LogInformation("OpenGraph: AfterBakeAsync called.");

        var baseUrl = context.Arguments.FirstOrDefault(arg => arg.StartsWith("--social:baseurl="))?.Split('=')[1];

        var configuration = ParseConfigurationFlag(context.Arguments);

        if (string.IsNullOrEmpty(baseUrl))
        {
            if (configuration.Equals("release", StringComparison.InvariantCultureIgnoreCase))
            {
                logger?.LogError("OpenGraph: Base URL not provided. Skipping social meta generation.");
                throw new ArgumentException("Base URL is required for social meta generation.");
            }

            baseUrl = "/"; // Default to root for debug configuration
        }

        var wwwrootpath = $"{context.ProjectPath.TrimEnd('/')}/wwwroot";

        if (!Directory.Exists(wwwrootpath))
        {
            logger?.LogWarning("OpenGraph: wwwroot directory does not exist. Skipping social meta generation.");
            return;
        }

        var indexFilePath = Path.Combine(wwwrootpath, "index.html");

        if (!File.Exists(indexFilePath))
        {
            logger?.LogWarning("OpenGraph: index.html file does not exist in wwwroot. Skipping social meta generation.");
            return;
        }

        var indexFileContent = await File.ReadAllTextAsync(indexFilePath);

        var blankIndexContent = StripExistingSeoTags(indexFileContent);

        foreach (var page in context.GeneratedPages)
        {
            if (page.Page.Slug == "index")
            {
                // Skip the index page as it is already handled
                continue;
            }

            var metaTags = GenerateMetaTags(page, baseUrl);
            
            // Insert meta tags into the head section of the index.html file
            var localIndexContent = blankIndexContent
                .Replace("<base href=\"/\" />", $"<base href=\"{baseUrl.TrimEnd('/')}/\" />")
                .Replace("</head>", $"{metaTags}\n</head>")
                .RootAllURLs();

            var pathParts = page.Page.Slug.TrimStart('/').TrimEnd('/');

            var outputPath = Path.Combine(wwwrootpath, pathParts, "index.html");

            var outputDirectory = Path.GetDirectoryName(outputPath);

            if (outputDirectory != null && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            await File.WriteAllTextAsync(outputPath, localIndexContent);
        }
    }

    private static string ParseConfigurationFlag(IReadOnlyList<string> arguments)
    {
        var args = arguments.ToArray();

        var config = "debug"; // default to debug configuration

        var i = Array.FindIndex(args, a => a is "-c" or "--configuration");

        if (i >= 0 && i + 1 < args.Length)
        {
            config = args[i + 1].ToLowerInvariant();
        }

        return config;
    }

    private static string GenerateMetaTags(GeneratedPage page, string baseUrl)
    {
        static string esc(string s) => System.Net.WebUtility.HtmlEncode(s);
        var metaTags = new List<string>
        {
            $"<meta name=\"description\" content=\"{esc(page.Page.Description)}\" />",
            $"<link rel=\"canonical\" href=\"{baseUrl.TrimEnd('/')}/{page.Page.Slug.TrimStart('/').TrimEnd('/')}/\" />",
            $"<title>{esc(page.Page.Title)}</title>",
            $"<meta property=\"og:type\" content=\"article\" />",
            $"<meta property=\"og:title\" content=\"{esc(page.Page.Title)}\" />",
            $"<meta property=\"og:description\" content=\"{esc(page.Page.Description)}\" />",
            $"<meta property=\"og:url\" content=\"{baseUrl.TrimEnd('/')}/{page.Page.Slug.TrimStart('/').TrimEnd('/')}/\" />"
        };

        if (!string.IsNullOrEmpty(page.Page.Image))
        {
            metaTags.Add($"<meta property=\"og:image\" content=\"{baseUrl.TrimEnd('/')}/{page.Page.Image.TrimStart('/')}\" />");
        }

        return string.Join("\n", metaTags);
    }

    private static string StripExistingSeoTags(string html)
    {
        var rx = new[]
        {
            // <meta property="og:*" ...>
            MetaOGRegex(),
            //// <meta name="twitter:*" ...>
            //MetaTWRegex(),
            // <meta name="description" ...>
            MetaNameRegex(),
            // <link rel="canonical" ...>
            MetaCanonicalRegex(),
            // <title>...</title>
            MetaTitleRegex()
        };

        foreach (var r in rx) html = r.Replace(html, "");
        
        return html;
    }

    [GeneratedRegex("""<meta\s+[^>]*\bproperty\s*=\s*['""]og:[^'""]+['"'][^>]*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-AU")]
    private static partial Regex MetaOGRegex();
    
    [GeneratedRegex("""<meta\s+[^>]*\bname\s*=\s*['""]twitter:[^'""]+['"'][^>]*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-AU")]
    private static partial Regex MetaTWRegex();
    
    [GeneratedRegex("""<meta\s+[^>]*\bname\s*=\s*['""]description['""][^>]*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-AU")]
    private static partial Regex MetaNameRegex();
    
    [GeneratedRegex("""<link\s+[^>]*\brel\s*=\s*['""]canonical['""][^>]*>""", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-AU")]
    private static partial Regex MetaCanonicalRegex();

    [GeneratedRegex("""<title>.*?</title>""", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-AU")]
    private static partial Regex MetaTitleRegex();
}
