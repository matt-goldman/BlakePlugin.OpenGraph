# BlakePlugin.OpenGraph

BlakePlugin.OpenGraph is a C# .NET 9.0 plugin for the Blake static site generator framework that generates OpenGraph metadata for statically generated Razor pages to improve social media integration.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Install .NET 9.0 SDK (REQUIRED)
The project requires .NET 9.0 SDK which is not available by default. Install it EVERY time:

```bash
wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0 --install-dir ./dotnet
export PATH="$(pwd)/dotnet:$PATH"
export DOTNET_ROOT="$(pwd)/dotnet"
```

NEVER CANCEL the .NET installation -- it takes 2-3 minutes to download and extract. Set timeout to 10+ minutes.

### Bootstrap, Build, and Validate the Project

Run these commands in sequence. NEVER CANCEL long-running operations:

```bash
# Set .NET 9.0 environment (run after every new session)
export PATH="$(pwd)/dotnet:$PATH"
export DOTNET_ROOT="$(pwd)/dotnet"

# Restore packages -- takes 4-5 seconds
dotnet restore

# Build the project -- takes 5-6 seconds. NEVER CANCEL. Set timeout to 30+ seconds.
dotnet build

# Verify formatting issues exist
dotnet format --verify-no-changes  # Expected to fail with whitespace errors

# Create NuGet package (requires Release build)
dotnet build -c Release
dotnet pack -c Release
```

### Testing
- NO unit tests exist in this project
- `dotnet test` will run but finds no tests to execute
- Plugin functionality testing requires integration with Blake framework

## Validation

### Code Formatting Requirements
ALWAYS run code formatting before committing changes:

```bash
# Fix all formatting issues
dotnet format

# Verify no formatting issues remain
dotnet format --verify-no-changes
```

The formatting check MUST pass (exit code 0) before any code changes are committed.

### Build Validation
ALWAYS run full build validation after making changes:

```bash
# Clean build
dotnet clean
dotnet restore
dotnet build
dotnet build -c Release
```

Build MUST succeed for both Debug and Release configurations.

## Plugin Architecture and Functionality

### What This Plugin Does
- Implements `IBlakePlugin` interface to hook into Blake's build process
- Generates OpenGraph meta tags (`og:title`, `og:description`, `og:url`, `og:image`, etc.)
- Processes generated pages and creates individual HTML files with metadata
- Handles base URL configuration for different build environments
- Strips existing SEO tags and replaces them with generated ones

### Key Files
- `src/Plugin.cs` - Main plugin logic implementing `IBlakePlugin.AfterBakeAsync`
- `src/StringUtils.cs` - URL processing utilities
- `src/BlakePlugin.OpenGraph.csproj` - Project configuration targeting .NET 9.0

### Plugin Dependencies
- `Blake.BuildTools` version 1.0.18 - Core Blake framework dependency
- Targets .NET 9.0 framework
- Uses `Microsoft.Extensions.Logging` for logging

### Plugin Configuration
The plugin expects:
- `--social:baseurl=<URL>` argument for base URL configuration
- `wwwroot/index.html` template file for metadata injection
- Release builds require base URL, Debug builds default to "/"

## Common Tasks

### Repository Structure
```
.
├── BlakePlugin.OpenGraph.sln    # Visual Studio solution
├── README.md                    # Basic project description
├── LICENSE                      # MIT license
├── .gitignore                   # Excludes build artifacts and dotnet/ directory
├── assets/
│   └── icon.png                 # Package icon
└── src/
    ├── BlakePlugin.OpenGraph.csproj  # Project file with NuGet package configuration
    ├── Plugin.cs                     # Main plugin implementation
    └── StringUtils.cs                # URL processing utilities
```

### Package Information
```json
{
  "PackageId": "BlakePlugin.OpenGraph",
  "Version": "1.0.0",
  "Authors": "Matt Goldman",
  "Description": "Blake plugin that generates OpenGraph metadata for statically generated Razor pages",
  "PackageTags": "blazor; ssg; razor; opengraph;"
}
```

### Expected Build Times
- .NET 9.0 SDK installation: 2-3 minutes (NEVER CANCEL)
- `dotnet restore`: 1 second
- `dotnet build`: 1-2 seconds
- `dotnet build -c Release`: 1-2 seconds
- `dotnet format`: <1 second
- `dotnet pack -c Release`: 1-2 seconds (after Release build)

### Validation Workflow
When making changes, ALWAYS follow this validation sequence:

1. **Install .NET 9.0 SDK** (if not already available)
2. **Format code**: `dotnet format`
3. **Build Debug**: `dotnet build`
4. **Build Release**: `dotnet build -c Release`
5. **Create package**: `dotnet pack -c Release`
6. **Verify formatting**: `dotnet format --verify-no-changes` (must pass)

### Common Scenarios

#### Adding New OpenGraph Meta Tags
1. Modify `GenerateMetaTags` method in `src/Plugin.cs`
2. Test with Debug build: `dotnet build`
3. Validate formatting: `dotnet format --verify-no-changes`

#### Updating Blake Framework Dependency
1. Edit `PackageReference` in `src/BlakePlugin.OpenGraph.csproj`
2. Run: `dotnet restore`
3. Test: `dotnet build`

#### Creating Release Package
1. Ensure code is formatted: `dotnet format`
2. Build Release: `dotnet build -c Release`
3. Create package: `dotnet pack -c Release`
4. Package location: `src/bin/Release/BlakePlugin.OpenGraph.1.0.0.nupkg`

### Plugin Functionality Testing

To verify the plugin works correctly with the Blake framework, create a test scenario:

```bash
# Create test project structure
mkdir -p /tmp/blake-test/wwwroot
echo '<html><head></head><body><h1>Test Page</h1></body></html>' > /tmp/blake-test/wwwroot/index.html

# The plugin expects Blake.BuildTools context which requires full Blake integration
# Standalone testing is limited without Blake framework installation
```

The plugin processes Blake's `BlakeContext` and `GeneratedPages` collection, requiring full Blake framework for complete functional testing.

### Troubleshooting

**Build Error: "The current .NET SDK does not support targeting .NET 9.0"**
- Solution: Install .NET 9.0 SDK using the commands in the "Install .NET 9.0 SDK" section above

**Format Check Failures**
- Solution: Run `dotnet format` to fix whitespace and style issues automatically

**Package Creation Fails with "file was not found"**
- Solution: Build Release configuration first: `dotnet build -c Release`

### Environment Notes
- The `dotnet/` directory is ignored in `.gitignore` and should not be committed
- Always set `PATH` and `DOTNET_ROOT` environment variables when working with the local .NET SDK
- Plugin requires Blake framework to test full functionality - standalone testing is limited