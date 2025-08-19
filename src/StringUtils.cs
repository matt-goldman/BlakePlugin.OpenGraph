using System.Text.RegularExpressions;

namespace BlakePlugin.OpenGraph;

internal static class StringUtils
{
    internal static string RootAllURLs(this string indexSource) =>
        // Prepend any URLs (src=" or href=") that are not absolute (i.e. do not start with https) with "/" if they don't already start with "/"
        Regex.Replace(indexSource, @"(src|href)=""(?!https?://)([^""]+?)""", m => $"{m.Groups[1].Value}=\"/{m.Groups[2].Value.TrimStart('/')}\"");
}
