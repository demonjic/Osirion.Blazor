# Troubleshooting Guide

[![Documentation](https://img.shields.io/badge/Documentation-Troubleshooting-red)](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/TROUBLESHOOTING.md)
[![Version](https://img.shields.io/nuget/v/Osirion.Blazor)](https://www.nuget.org/packages/Osirion.Blazor)

This guide helps you diagnose and resolve common issues when working with Osirion.Blazor.

## Table of Contents

1. [Common Issues](#common-issues)
2. [Installation Problems](#installation-problems)
3. [Configuration Issues](#configuration-issues)
4. [Content Management Problems](#content-management-problems)
5. [Component Issues](#component-issues)
6. [Performance Problems](#performance-problems)
7. [Debugging Techniques](#debugging-techniques)
8. [Getting Help](#getting-help)

## Common Issues

### Issue: Components Not Rendering

**Symptoms:**
- Components appear blank or don't render at all
- No error messages in console

**Possible Causes:**
1. Missing service registration
2. Incorrect component imports
3. Missing CSS styles

**Solutions:**

1. **Verify Service Registration:**
```csharp
// ? Ensure services are registered in Program.cs
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => { ... });
});
```

2. **Check Component Imports:**
```razor
@* ? Add to _Imports.razor *@
@using Osirion.Blazor.Components
@using Osirion.Blazor.Cms.Components
```

3. **Include Required Styles:**
```razor
@* ? Add to layout *@
<OsirionStyles Framework="CssFramework.Bootstrap" />
```

### Issue: GitHub API Rate Limiting

**Symptoms:**
- Content not loading
- API error messages in logs
- Intermittent failures

**Solutions:**

1. **Add Authentication Token:**
```json
// appsettings.json
{
  "Osirion": {
    "GitHubCms": {
      "ApiToken": "your-github-token",
      "Owner": "username",
      "Repository": "repo"
    }
  }
}
```

2. **Implement Caching:**
```csharp
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.CacheExpirationMinutes = 60; // Cache for 1 hour
        options.UseCache = true;
    });
});
```

3. **Check Rate Limit Status:**
```csharp
[Inject] IGitHubApiClient GitHubClient { get; set; }

private async Task CheckRateLimit()
{
    var rateLimit = await GitHubClient.GetRateLimitAsync();
    logger.LogInformation("Rate limit: {Remaining}/{Limit}, resets at {Reset}",
        rateLimit.Remaining, rateLimit.Limit, rateLimit.Reset);
}
```

### Issue: Content Not Found

**Symptoms:**
- 404 errors for content
- Empty content lists
- "Content not found" messages

**Possible Causes:**
1. Incorrect repository/path configuration
2. Content not in expected format
3. Authentication issues

**Solutions:**

1. **Verify Repository Configuration:**
```csharp
// ? Check these settings
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.Owner = "correct-username";        // ? Verify username
        options.Repository = "correct-repo-name";  // ? Verify repo name
        options.ContentPath = "content";           // ? Verify path exists
        options.Branch = "main";                   // ? Verify branch name
    });
});
```

2. **Check Content Structure:**
```
your-repo/
??? content/              # ? This directory must exist
?   ??? blog/
?   ?   ??? post1.md     # ? Content files
?   ??? pages/
?       ??? about.md
```

3. **Validate Content Format:**
```markdown
---
title: "Your Title"          # ? Required frontmatter
date: "2024-01-01"
author: "Author Name"
---

# Your Content

Your markdown content here... # ? Content body
```

## Installation Problems

### Issue: Package Version Conflicts

**Symptoms:**
- Build errors about conflicting package versions
- Runtime errors about missing assemblies

**Solutions:**

1. **Check Package Compatibility:**
```xml
<!-- ? Ensure compatible versions -->
<PackageReference Include="Osirion.Blazor" Version="1.5.0" />
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.0" />
```

2. **Clear Package Cache:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore
```

3. **Update All Packages:**
```bash
# Update to latest compatible versions
dotnet add package Osirion.Blazor
dotnet add package Microsoft.AspNetCore.Components.Web
```

### Issue: Missing Dependencies

**Symptoms:**
- Compile-time errors about missing types
- Runtime exceptions about unregistered services

**Solutions:**

1. **Install Missing Packages:**
```bash
# Core dependencies
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Options
dotnet add package Microsoft.Extensions.Caching.Memory
```

2. **Verify Framework Target:**
```xml
<!-- ? Ensure correct target framework -->
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

## Configuration Issues

### Issue: Configuration Not Loading

**Symptoms:**
- Services using default configuration
- Settings from appsettings.json ignored

**Solutions:**

1. **Verify Configuration Binding:**
```csharp
// ? Ensure configuration is properly bound
builder.Services.AddOsirionBlazor(builder.Configuration);

// Or manually:
builder.Services.Configure<GitHubCmsOptions>(
    builder.Configuration.GetSection("Osirion:GitHubCms"));
```

2. **Check Configuration Structure:**
```json
{
  "Osirion": {                    // ? Correct nesting
    "GitHubCms": {
      "Owner": "username",
      "Repository": "repo"
    }
  }
}
```

3. **Validate Environment Configuration:**
```csharp
// ? Check if environment-specific config exists
var environment = builder.Environment.EnvironmentName;
logger.LogInformation("Loading configuration for environment: {Environment}", environment);
```

### Issue: Invalid Configuration Values

**Symptoms:**
- ArgumentException during startup
- Services failing to initialize

**Solutions:**

1. **Add Configuration Validation:**
```csharp
builder.Services.Configure<GitHubCmsOptions>(options =>
{
    // ? Validate required fields
    if (string.IsNullOrWhiteSpace(options.Owner))
        throw new ArgumentException("GitHub owner must be specified");
    
    if (string.IsNullOrWhiteSpace(options.Repository))
        throw new ArgumentException("GitHub repository must be specified");
});
```

2. **Use Options Validation:**
```csharp
builder.Services.AddOptions<GitHubCmsOptions>()
    .Bind(builder.Configuration.GetSection("Osirion:GitHubCms"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## Content Management Problems

### Issue: Markdown Not Rendering

**Symptoms:**
- Raw markdown text displayed instead of formatted content
- Missing HTML elements

**Solutions:**

1. **Check Markdown Processor:**
```csharp
// ? Ensure markdown processor is registered
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.EnableMarkdownExtensions = true;
        options.UseSmartyPants = true;
        options.UseEmphasisExtras = true;
    });
});
```

2. **Verify Content Format:**
```markdown
<!-- ? Ensure proper markdown syntax -->
# Heading 1
## Heading 2

**Bold text**
*Italic text*

- List item 1
- List item 2
```

3. **Check for HTML Sanitization:**
Content HTML is sanitized by default with an allow-list (see [HTML Sanitization](../src/Osirion.Blazor.Core/README.md#html-sanitization) in the Core README). If markup you need is removed, extend the allow-list instead of turning sanitization off:
```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options =>
{
    options.AllowedAttributes.Add("style");

    // List every attribute your client script reads from the element; an attribute that is not listed is removed
    options.AllowElement("chart", "type", "title", "caption", "data", "colors");
});
```

### Issue: Frontmatter Not Parsing

**Symptoms:**
- Metadata not appearing in content
- Frontmatter displayed as content

**Solutions:**

1. **Verify Frontmatter Format:**
```markdown
---
title: "Article Title"        # ? Use quotes for strings
date: "2024-01-01"           # ? Date in ISO format
categories: [web, blazor]     # ? Array format
featured: true               # ? Boolean values
---

Content starts here...
```

2. **Check YAML Syntax:**
```markdown
---
# ? Correct YAML syntax
title: "My Article"
author: "John Doe"
tags:
  - blazor
  - web-dev
metadata:
  reading_time: 5
  difficulty: beginner
---
```

### Issue: Images Not Loading

**Symptoms:**
- Broken image links
- 404 errors for image resources

**Solutions:**

1. **Check Image Paths:**
```markdown
<!-- ? Use correct relative paths -->
![Alt text](../assets/images/my-image.jpg)

<!-- ? Or absolute URLs -->
![Alt text](https://example.com/images/my-image.jpg)
```

2. **Configure Static File Serving:**
```csharp
// ? Ensure static files are served
app.UseStaticFiles();

// ? Configure additional paths if needed
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "assets")),
    RequestPath = "/assets"
});
```

## Component Issues

### Issue: CSS Styles Not Applied

**Symptoms:**
- Components appear unstyled
- Default browser styles applied

**Solutions:**

1. **Include Component Styles:**
```razor
@* ? Ensure OsirionStyles is included *@
<OsirionStyles 
    Framework="CssFramework.Bootstrap" />
```

2. **Check CSS Framework Integration:**
```html
<!-- ? Include framework CSS before Osirion styles -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<OsirionStyles Framework="CssFramework.Bootstrap" />
```

3. **Verify CSS Variables:**
```css
/* ? Check if CSS variables are defined */
:root {
    --osirion-primary-color: #2563eb;
    --osirion-border-radius: 0.5rem;
    /* Other variables... */
}
```

### Issue: JavaScript Interop Errors

**Symptoms:**
- JavaScript errors in browser console
- Interactive features not working

**Solutions:**

1. **Check Browser Compatibility:**
```javascript
// ? Ensure modern browser features are available
if ('IntersectionObserver' in window) {
    // Use intersection observer
} else {
    // Fallback implementation
}
```

2. **Verify Script Loading Order:**
```html
<!-- ? Load Blazor scripts before component scripts -->
<script src="_framework/blazor.server.js"></script>
<script src="_content/Osirion.Blazor/js/osirion.js"></script>
```

## Performance Problems

### Issue: Slow Initial Page Load

**Symptoms:**
- Long time to first paint
- Poor Core Web Vitals scores

**Solutions:**

1. **Enable Response Compression:**
```csharp
// ? Add compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
```

2. **Implement Caching:**
```csharp
// ? Add response caching
builder.Services.AddResponseCaching();

app.UseResponseCaching();
```

3. **Optimize Critical Resources:**
```html
<!-- ? Preload critical resources -->
<link rel="preload" href="/css/app.css" as="style">
<link rel="preload" href="/_framework/blazor.server.js" as="script">
```

### Issue: High Memory Usage

**Symptoms:**
- Application consuming excessive memory
- Out of memory exceptions

**Solutions:**

1. **Configure Cache Limits:**
```csharp
builder.Services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 100; // Limit cache size
});
```

2. **Implement Proper Disposal:**
```csharp
public class MyComponent : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource cts = new();
    
    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
    }
}
```

## Debugging Techniques

### Enable Detailed Logging

```csharp
// ? Enable detailed logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ? Configure specific logger levels
builder.Logging.AddFilter("Osirion.Blazor", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
```

### Use Development Tools

1. **Browser Developer Tools:**
   - Check Network tab for failed requests
   - Use Console for JavaScript errors
   - Monitor Performance tab for bottlenecks

2. **Blazor Developer Tools:**
   - Install Blazor DevTools extension
   - Inspect component hierarchy
   - Monitor component lifecycle

### Component Debugging

```csharp
public partial class DebugComponent : ComponentBase
{
    [Inject] ILogger<DebugComponent> Logger { get; set; } = default!;
    
    protected override void OnInitialized()
    {
        Logger.LogDebug("Component initialized with parameters: {@Parameters}", Parameters);
    }
    
    protected override void OnParametersSet()
    {
        Logger.LogDebug("Parameters set: {@Parameters}", Parameters);
    }
    
    protected override void OnAfterRender(bool firstRender)
    {
        Logger.LogDebug("Component rendered, first render: {FirstRender}", firstRender);
    }
}
```

### Network Debugging

```csharp
// ? Add HTTP logging
builder.Services.AddHttpClient<IGitHubApiClient, GitHubApiClient>()
    .AddLogger(logger =>
    {
        logger.LogRequestsAndResponses = true;
        logger.LogLevel = LogLevel.Debug;
    });
```

### Memory Debugging

```csharp
#if DEBUG
public static class MemoryDebugger
{
    public static void LogMemoryUsage(string operation)
    {
        var gcMemory = GC.GetTotalMemory(false);
        var workingSet = Process.GetCurrentProcess().WorkingSet64;
        
        Console.WriteLine($"[{operation}] GC Memory: {gcMemory:N0}, Working Set: {workingSet:N0}");
    }
}
#endif
```

## Getting Help

### Before Asking for Help

1. **Check Documentation:**
   - Review relevant documentation sections
   - Check API reference for parameter requirements
   - Look at example implementations

2. **Search Existing Issues:**
   - Browse [GitHub Issues](https://github.com/demonjic/Osirion.Blazor/issues)
   - Check closed issues for solutions

3. **Create Minimal Reproduction:**
   - Isolate the problem in a small example
   - Remove unnecessary code and dependencies
   - Document exact steps to reproduce

### Where to Get Help

1. **GitHub Issues:**
   - For bugs and feature requests
   - Include detailed reproduction steps
   - Provide relevant code samples

2. **Stack Overflow:**
   - Tag questions with `osirion-blazor`
   - Follow Stack Overflow guidelines
   - Provide complete, minimal examples

### Information to Include

When asking for help, include:

1. **Osirion.Blazor Version:**
```bash
dotnet list package | grep Osirion
```

2. **Environment Details:**
   - .NET version
   - Operating system
   - Browser (if relevant)
   - Hosting environment

3. **Complete Error Messages:**
```csharp
// Include full exception details
try
{
    // Your code
}
catch (Exception ex)
{
    logger.LogError(ex, "Complete error context");
    // Include this in your issue
}
```

4. **Relevant Configuration:**
```json
// Include relevant appsettings.json sections
{
  "Osirion": {
    "GitHubCms": {
      // Your configuration
    }
  }
}
```

5. **Code Samples:**
```csharp
// Provide minimal, complete code that reproduces the issue
public class ReproductionExample : ComponentBase
{
    // Minimal code to demonstrate the problem
}
```

By following this troubleshooting guide, you should be able to resolve most common issues with Osirion.Blazor. If you continue to experience problems, don't hesitate to reach out to the community for help.