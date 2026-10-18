# Getting Started Guide

[![Documentation](https://img.shields.io/badge/Documentation-Getting_Started-green)](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/GETTING_STARTED.md)
[![Version](https://img.shields.io/nuget/v/Osirion.Blazor)](https://www.nuget.org/packages/Osirion.Blazor)

This comprehensive guide will help you get started with Osirion.Blazor, from initial setup to building your first application.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [Quick Setup](#quick-setup)
4. [Your First Components](#your-first-components)
5. [Configuration](#configuration)
6. [Building Your First Application](#building-your-first-application)
7. [Next Steps](#next-steps)

## Prerequisites

Before getting started with Osirion.Blazor, ensure you have:

### Development Environment
- **.NET 8.0 SDK** or higher (**.NET 9.0** recommended)
- **Visual Studio 2022**, **VS Code**, or **JetBrains Rider**
- **Git** (for content management features)

### Blazor Knowledge
- Basic understanding of **Blazor components**
- Familiarity with **Razor syntax**
- Understanding of **dependency injection** in .NET

### Optional but Recommended
- **GitHub account** (for content management)
- **CSS framework** experience (Bootstrap, Fluent UI, etc.)
- **Modern web development** concepts

## Installation

Osirion.Blazor provides multiple installation options depending on your needs.

### Option 1: Complete Package (Recommended)

Install the complete Osirion.Blazor package that includes all modules:

```bash
dotnet add package Osirion.Blazor
```

### Option 2: Individual Modules

Install only the modules you need:

```bash
# Core components (required for other modules)
dotnet add package Osirion.Blazor.Core

# Analytics integration
dotnet add package Osirion.Blazor.Analytics

# Enhanced navigation
dotnet add package Osirion.Blazor.Navigation

# Content management system
dotnet add package Osirion.Blazor.Cms

# Theming and styling
dotnet add package Osirion.Blazor.Theming
```

### Option 3: Package Manager Console

Using Package Manager Console in Visual Studio:

```powershell
Install-Package Osirion.Blazor
```

## Quick Setup

### 1. Create a New Blazor Project

Create a new Blazor project if you haven't already:

```bash
# Blazor Server App
dotnet new blazorserver -n MyOsirionApp

# Blazor WebAssembly App
dotnet new blazorwasm -n MyOsirionApp

# Blazor Web App (.NET 8+)
dotnet new blazor -n MyOsirionApp
```

### 2. Install Osirion.Blazor

Navigate to your project directory and install the package:

```bash
cd MyOsirionApp
dotnet add package Osirion.Blazor
```

### 3. Register Services

Add Osirion services to your `Program.cs`:

```csharp
using Osirion.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // Add this for Blazor Server

// Register Osirion.Blazor services
builder.Services.AddOsirionBlazor(osirion => {
    osirion
        .AddGitHubCms(options => {
            options.Owner = "your-username";
            options.Repository = "your-content-repo";
            options.ContentPath = "content";
        })
        .AddScrollToTop()
        .AddClarityTracker(options => {
            options.SiteId = "your-clarity-id";
        })
        .AddOsirionStyle(CssFramework.Bootstrap);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### 4. Add Component Imports

Add the following to your `_Imports.razor` file:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Analytics.Components
@using Osirion.Blazor.Navigation.Components
@using Osirion.Blazor.Cms.Components
```

### 5. Include Styles

Add Osirion styles to your layout. In your main layout file (`Components/Layout/MainLayout.razor` or similar):

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - MyOsirionApp</title>
    
    <!-- Bootstrap CSS (if using Bootstrap) -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Osirion Styles -->
    <OsirionStyles Framework="CssFramework.Bootstrap" />
    
    <!-- Your custom styles -->
    <link href="css/app.css" rel="stylesheet" />
    
    @await RenderSectionAsync("Styles", required: false)
</head>
<body>
    <!-- Enhanced Navigation -->
    <EnhancedNavigation Behavior="ScrollBehavior.Smooth" />
    
    <!-- Your layout content -->
    @Body
    
    <!-- Scroll to Top button -->
    <ScrollToTop Position="Position.BottomRight" />
    
    <!-- Analytics tracking -->
    <ClarityTracker />
    
    <!-- Cookie consent -->
    <OsirionCookieConsent PolicyLink="/privacy-policy" />
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

## Your First Components

Let's create your first page using Osirion.Blazor components.

### 1. Create a Landing Page

Create a new page `Components/Pages/Home.razor`:

```razor
@page "/"
@using Osirion.Blazor.Components

<PageTitle>Welcome - MyOsirionApp</PageTitle>

<!-- Hero Section -->
<HeroSection 
    Title="Welcome to MyOsirionApp"
    Subtitle="Build Amazing Blazor Applications"
    Summary="Experience the power of modern web development with Osirion.Blazor components."
    ImageUrl="/images/hero-image.jpg"
    UseBackgroundImage="true"
    PrimaryButtonText="Get Started"
    PrimaryButtonUrl="/docs"
    SecondaryButtonText="View Examples"
    SecondaryButtonUrl="/examples"
    Variant="HeroVariant.Hero"
    Alignment="Alignment.Center" />

<!-- Features Section -->
<div class="container my-5">
    <div class="row">
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">?? SSR-First</h5>
                    <p class="card-text">Built for Server-Side Rendering with progressive enhancement.</p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">?? Framework Integration</h5>
                    <p class="card-text">Seamless integration with Bootstrap, Fluent UI, MudBlazor, and Radzen.</p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card h-100">
                <div class="card-body">
                    <h5 class="card-title">?? Content Management</h5>
                    <p class="card-text">Git-based content management with GitHub integration.</p>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Logo Carousel -->
<InfiniteLogoCarousel 
    Title="Trusted by Developers Worldwide"
    CustomLogos="@logoList"
    AnimationDuration="60"
    PauseOnHover="true" />

@code {
    private List<LogoItem> logoList = new()
    {
        new LogoItem { Name = "Microsoft", ImageUrl = "/images/logos/microsoft.png", Link = "https://microsoft.com" },
        new LogoItem { Name = "GitHub", ImageUrl = "/images/logos/github.png", Link = "https://github.com" },
        new LogoItem { Name = "Azure", ImageUrl = "/images/logos/azure.png", Link = "https://azure.com" },
        // Add more logos as needed
    };
}
```

### 2. Create a Blog Page

Create `Components/Pages/Blog.razor`:

```razor
@page "/blog"
@page "/blog/{category?}"
@using Osirion.Blazor.Components
@using Osirion.Blazor.Cms.Components

<PageTitle>Blog - MyOsirionApp</PageTitle>

<!-- Breadcrumb Navigation -->
<div class="container mt-3">
    <OsirionBreadcrumbs 
        Path="@Navigation.Uri"
        ShowHome="true"
        HomeText="Home"
        HomeUrl="/"
        SegmentFormatter="@FormatBreadcrumbSegment" />
</div>

<!-- Page Header -->
<HeroSection 
    Title="@GetPageTitle()"
    Subtitle="@GetPageSubtitle()"
    Variant="HeroVariant.Minimal"
    Alignment="Alignment.Center" />

<!-- Content Area -->
<div class="container my-5">
    <div class="row">
        <!-- Main Content -->
        <div class="col-lg-8">
            <ContentList 
                Directory="blog"
                Category="@Category"
                Count="10"
                SortBy="SortField.Date"
                SortDirection="SortDirection.Descending"
                ShowExcerpt="true"
                ShowMetadata="true" />
        </div>
        
        <!-- Sidebar -->
        <div class="col-lg-4">
            <div class="card mb-4">
                <div class="card-header">
                    <h5>Search</h5>
                </div>
                <div class="card-body">
                    <SearchBox 
                        Placeholder="Search articles..."
                        OnSearch="@HandleSearch" />
                </div>
            </div>
            
            <div class="card mb-4">
                <div class="card-header">
                    <h5>Categories</h5>
                </div>
                <div class="card-body">
                    <CategoriesList />
                </div>
            </div>
            
            <div class="card mb-4">
                <div class="card-header">
                    <h5>Popular Tags</h5>
                </div>
                <div class="card-body">
                    <TagCloud MaxTags="20" />
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public string? Category { get; set; }
    [Inject] NavigationManager Navigation { get; set; } = default!;
    
    private string searchQuery = string.Empty;
    
    private string GetPageTitle()
    {
        return string.IsNullOrWhiteSpace(Category) ? "Blog" : $"Blog - {Category}";
    }
    
    private string GetPageSubtitle()
    {
        return string.IsNullOrWhiteSpace(Category) 
            ? "Latest articles and insights" 
            : $"Articles in {Category}";
    }
    
    private void HandleSearch(string query)
    {
        searchQuery = query;
        // Navigate to search results or update the content list
        Navigation.NavigateTo($"/blog?search={Uri.EscapeDataString(query)}");
    }
    
    private string FormatBreadcrumbSegment(string segment)
    {
        return segment.Replace("-", " ").ToTitleCase();
    }
}

@functions {
    public static class StringExtensions
    {
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
                
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}
```

## Configuration

### Configuration via appsettings.json

For more advanced scenarios, configure Osirion.Blazor via `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Osirion": {
    "GitHubCms": {
      "Owner": "your-username",
      "Repository": "your-content-repo",
      "ContentPath": "content",
      "Branch": "main",
      "CacheExpirationMinutes": 60,
      "EnableLocalization": false,
      "DefaultLocale": "en"
    },
    "Analytics": {
      "Clarity": {
        "SiteId": "your-clarity-id",
        "Enabled": true
      },
      "Matomo": {
        "SiteId": "1",
        "TrackerUrl": "//analytics.example.com/",
        "Enabled": false
      }
    },
    "Navigation": {
      "ScrollToTop": {
        "Position": "BottomRight",
        "VisibilityThreshold": 300
      }
    },
    "Theming": {
      "Framework": "Bootstrap"
    }
  }
}
```

Then update your `Program.cs`:

```csharp
// Register services from configuration
builder.Services.AddOsirionBlazor(builder.Configuration);
```

### Environment-Specific Configuration

Create environment-specific configuration files:

**appsettings.Development.json:**
```json
{
  "Osirion": {
    "Analytics": {
      "Clarity": {
        "Enabled": false
      }
    }
  }
}
```

**appsettings.Production.json:**
```json
{
  "Osirion": {
    "GitHubCms": {
      "ApiToken": "your-production-token"
    },
    "Analytics": {
      "Clarity": {
        "Enabled": true
      }
    }
  }
}
```

## Building Your First Application

Let's build a complete blog application using Osirion.Blazor.

### 1. Set Up Your Content Repository

Create a GitHub repository for your content with this structure:

```
your-content-repo/
??? content/
?   ??? blog/
?   ?   ??? 2024/
?   ?   ?   ??? 01/
?   ?   ?   ?   ??? getting-started-with-osirion.md
?   ?   ?   ??? 02/
?   ?   ?       ??? advanced-features.md
?   ?   ??? categories/
?   ?       ??? tutorials.md
?   ?       ??? announcements.md
?   ??? pages/
?   ?   ??? about.md
?   ?   ??? contact.md
?   ??? tags/
?       ??? blazor.md
?       ??? dotnet.md
??? assets/
    ??? images/
        ??? featured/
        ??? logos/
```

### 2. Create Content Files

**content/blog/2024/01/getting-started-with-osirion.md:**
```markdown
---
title: "Getting Started with Osirion.Blazor"
date: "2024-01-15"
author: "Your Name"
description: "Learn how to build modern Blazor applications with Osirion.Blazor"
categories: [tutorials]
tags: [blazor, getting-started, web-development]
featured_image: "/assets/images/featured/getting-started.jpg"
is_featured: true
---

# Getting Started with Osirion.Blazor

Welcome to the world of modern Blazor development! In this tutorial, we'll explore how to build beautiful, functional web applications using Osirion.Blazor.

## What You'll Learn

- Setting up your development environment
- Creating your first components
- Building responsive layouts
- Integrating with content management

## Prerequisites

Before we begin, make sure you have...

[Rest of your content]
```

### 3. Create Application Pages

**Components/Pages/Article.razor:**
```razor
@page "/blog/{slug}"
@using Osirion.Blazor.Components
@using Osirion.Blazor.Cms.Components

<PageTitle>@GetPageTitle()</PageTitle>

<!-- Breadcrumb Navigation -->
<div class="container mt-3">
    <OsirionBreadcrumbs 
        Path="@Navigation.Uri"
        ShowHome="true" />
</div>

<!-- Article Content -->
@if (article != null)
{
    <!-- Article Hero -->
    <HeroSection 
        Title="@article.Title"
        Subtitle="@article.Excerpt"
        Author="@article.Author"
        PublishDate="@article.PublishDate"
        ReadTime="@CalculateReadTime(article.Content)"
        ShowMetadata="true"
        Variant="HeroVariant.Minimal"
        FeaturedImage="@article.FeaturedImage" />
    
    <!-- Article Body -->
    <div class="container my-5">
        <div class="row">
            <div class="col-lg-8 mx-auto">
                <ContentView Path="@GetContentPath()" />
                
                <!-- Tags -->
                @if (article.Tags.Any())
                {
                    <div class="mt-4">
                        <h6>Tags:</h6>
                        @foreach (var tag in article.Tags)
                        {
                            <a href="/blog/tag/@tag" class="badge bg-secondary me-1">@tag</a>
                        }
                    </div>
                }
            </div>
        </div>
    </div>
}
else
{
    <OsirionContentNotFound 
        Title="Article Not Found"
        Message="The article you're looking for doesn't exist."
        ShowBackButton="true"
        BackButtonUrl="/blog" />
}

@code {
    [Parameter] public string Slug { get; set; } = string.Empty;
    [Inject] NavigationManager Navigation { get; set; } = default!;
    [Inject] IContentProvider ContentProvider { get; set; } = default!;
    
    private ContentItem? article;
    
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Slug))
        {
            article = await ContentProvider.GetItemAsync($"blog/{Slug}.md");
        }
    }
    
    private string GetPageTitle()
    {
        return article?.Title ?? "Article Not Found";
    }
    
    private string GetContentPath()
    {
        return $"blog/{Slug}.md";
    }
    
    private string CalculateReadTime(string content)
    {
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var readTimeMinutes = Math.Ceiling(wordCount / 200.0); // Assuming 200 words per minute
        return $"{readTimeMinutes} min read";
    }
}
```

### 4. Add Error Handling

**Components/Pages/Error.razor:**
```razor
@page "/error"
@using Osirion.Blazor.Components

<PageTitle>Error - MyOsirionApp</PageTitle>

<OsirionContentNotFound 
    Title="Oops! Something went wrong"
    Message="We're sorry, but an unexpected error occurred. Please try again later."
    ShowBackButton="true"
    BackButtonText="Go Home"
    BackButtonUrl="/" />
```

## Next Steps

Congratulations! You now have a basic Osirion.Blazor application up and running. Here are some next steps to enhance your application:

### 1. Add More Components
- Implement contact forms
- Add newsletter subscriptions
- Create photo galleries
- Build testimonial sections

### 2. Customize Styling
- Review the [Styling Guide](STYLING.md)
- Implement your brand colors
- Add custom CSS variables
- Create responsive designs

### 3. Enhance Content Management
- Set up GitHub webhooks for automatic updates
- Add content localization
- Implement content scheduling
- Create custom content transformers

### 4. Add Analytics and SEO
- Configure additional analytics providers
- Implement structured data
- Add social media integration
- Set up sitemap generation

### 5. Performance Optimization
- Review the [Performance Guide](PERFORMANCE.md)
- Implement caching strategies
- Optimize images and assets
- Monitor application performance

### 6. Testing and Deployment
- Write unit tests for your components
- Set up integration tests
- Configure CI/CD pipelines
- Deploy to your preferred platform

## Learning Resources

- [Quick Reference](QUICK_REFERENCE.md) - Overview of all components
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Architecture Overview](ARCHITECTURE.md) - Understanding the system design
- [Examples Repository](https://github.com/demonjic/Osirion.Blazor/tree/master/examples) - Real-world examples
- [Contributing Guide](../CONTRIBUTING.md) - How to contribute to the project

## Getting Help

If you need help or have questions:

1. Check the [documentation](../README.md)
2. Browse [existing issues](https://github.com/demonjic/Osirion.Blazor/issues)
3. Create a [new issue](https://github.com/demonjic/Osirion.Blazor/issues/new); a question is welcome there too

Welcome to the Osirion.Blazor community! We're excited to see what you'll build.