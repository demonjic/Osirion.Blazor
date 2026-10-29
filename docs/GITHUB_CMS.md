# GitHub CMS Guide

[![Documentation](https://img.shields.io/badge/Documentation-GitHub_CMS-blue)](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/GITHUB_CMS.md)
[![Version](https://img.shields.io/nuget/v/Osirion.Blazor.Cms)](https://www.nuget.org/packages/Osirion.Blazor.Cms)

Comprehensive guide for using GitHub as a content management system with Osirion.Blazor.

## Table of Contents

1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Repository Setup](#repository-setup)
4. [Content Structure](#content-structure)
5. [Frontmatter Reference](#frontmatter-reference)
6. [Content Components](#content-components)
7. [Advanced Features](#advanced-features)
8. [SEO and Metadata](#seo-and-metadata)
9. [Multi-language Support](#multi-language-support)
10. [Admin Interface](#admin-interface)
11. [Webhooks and Automation](#webhooks-and-automation)
12. [Best Practices](#best-practices)

## Overview

The GitHub CMS provider transforms any GitHub repository into a powerful content management system for your Blazor application. This approach offers several advantages:

### Benefits of Git-Based CMS

- **Version Control**: Complete history of all content changes
- **Collaboration**: Multiple authors can contribute using Git workflows
- **Review Process**: Content reviews through pull requests
- **Backup**: Content is automatically backed up and distributed
- **Developer-Friendly**: Use familiar Git tools and workflows
- **Cost-Effective**: Leverage existing GitHub infrastructure

### Key Features

- **Markdown Support**: Write content in Markdown with frontmatter
- **Automatic Discovery**: Automatically discovers and indexes content
- **Search and Filtering**: Full-text search and metadata filtering
- **Category and Tag Support**: Organize content with categories and tags
- **SEO Optimization**: Automatic sitemap and metadata generation
- **Caching**: Built-in caching for optimal performance
- **Multi-language**: Support for localized content

## Getting Started

### Installation

Install the required packages:

```bash
# Core CMS functionality
dotnet add package Osirion.Blazor.Cms

# Or install the complete package
dotnet add package Osirion.Blazor
```

### Basic Configuration

Configure the GitHub CMS provider in your `Program.cs`:

```csharp
using Osirion.Blazor.Cms.Extensions;

builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.Owner = "your-username";
        options.Repository = "your-content-repo";
        options.ContentPath = "content";
        options.Branch = "main";
        options.ApiToken = "your-github-token"; // Optional for public repos
    });
});
```

### Add Component Imports

Add to your `_Imports.razor`:

```razor
@using Osirion.Blazor.Cms.Components
```

## Repository Setup

### Create Content Repository

1. **Create a new GitHub repository** for your content
2. **Add a content directory** (default: `content`)
3. **Create initial content structure**

Example repository structure:

```
your-content-repo/
??? content/
?   ??? blog/                  # Blog posts directory
?   ?   ??? 2024/              # Year-based organization
?   ?   ?   ??? 01/            # Month subdirectories
?   ?   ?   ?   ??? getting-started.md
?   ?   ?   ?   ??? advanced-tips.md
?   ?   ?   ??? 02/
?   ?   ?       ??? new-features.md
?   ?   ??? categories/        # Category definitions
?   ?       ??? tutorials.md
?   ?       ??? announcements.md
?   ??? pages/                 # Static pages
?   ?   ??? about.md
?   ?   ??? contact.md
?   ?   ??? privacy-policy.md
?   ??? docs/                  # Documentation
?   ?   ??? getting-started.md
?   ?   ??? api-reference.md
?   ??? tags/                  # Tag definitions
?       ??? blazor.md
?       ??? web-development.md
??? assets/                    # Static assets
?   ??? images/
?   ?   ??? featured/          # Featured images
?   ?   ??? blog/              # Blog images
?   ??? documents/
??? README.md
```

### Authentication Setup

For private repositories or higher API limits, create a GitHub Personal Access Token:

1. Go to GitHub Settings ? Developer settings ? Personal access tokens
2. Generate a new token with `repo` scope
3. Add the token to your configuration:

```csharp
options.ApiToken = "ghp_your_token_here";
```

Or use environment variables:

```bash
export GITHUB_TOKEN="ghp_your_token_here"
```

```csharp
options.ApiToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
```

## Content Structure

### File Naming Conventions

- Use lowercase with hyphens: `getting-started.md`
- Include dates for blog posts: `2024-01-15-new-features.md`
- Use descriptive, SEO-friendly names
- Keep file names under 50 characters

### Directory Organization

#### By Category
```
content/
??? tutorials/
??? announcements/
??? case-studies/
??? product-updates/
```

#### By Date (for blogs)
```
content/blog/
??? 2024/
?   ??? 01/
?   ??? 02/
?   ??? 03/
??? 2023/
    ??? 11/
    ??? 12/
```

#### Hybrid Approach
```
content/
??? blog/
?   ??? 2024/01/
?   ??? 2024/02/
??? docs/
?   ??? getting-started/
?   ??? advanced/
??? pages/
    ??? legal/
    ??? company/
```

### Section Pages

An `_index.md` file in a folder is that folder's page: its front matter names and describes the folder (the title,
description, order and featured image that directory navigation shows), and its body is the overview page served at
the folder's URL. `content/en/docs/components/core/_index.md` is the page at `/docs/components/core`. The GitHub and
file system providers both serve it.

A `ContentList` leaves out the page it is shown on, so the list on `/blog` never shows a card for `blog/_index.md`.

## Frontmatter Reference

Frontmatter is YAML metadata at the top of your Markdown files:

### Required Fields

```yaml
---
title: "Your Article Title"
date: "2024-01-15"
---
```

### Complete Example

```yaml
---
# Basic Information
title: "Complete Guide to Osirion.Blazor"
subtitle: "Building Modern Web Applications"
description: "Learn how to build powerful Blazor applications with Osirion.Blazor components and GitHub CMS."

# Publishing Information
date: "2024-01-15T10:00:00Z"
lastModified: "2024-01-20T15:30:00Z"
author: "Jane Developer"
authorEmail: "jane@example.com"
authorBio: "Senior Developer at Example Corp"

# Content Organization
categories: [tutorials, web-development]
tags: [blazor, cms, github, web]
series: "Osirion.Blazor Tutorial Series"
seriesOrder: 1

# Publishing Status
published: true
featured: true
draft: false

# SEO and Social
slug: "complete-guide-osirion-blazor"
excerpt: "A comprehensive guide covering everything you need to know about building applications with Osirion.Blazor."
keywords: [blazor, cms, github, tutorial]
canonicalUrl: "https://mysite.com/blog/complete-guide-osirion-blazor"

# Media
featuredImage: "/assets/images/featured/osirion-guide.jpg"
featuredImageAlt: "Osirion.Blazor Components Diagram"
socialImage: "/assets/images/social/osirion-guide-social.jpg"

# Content Metadata
readingTime: "15 min"
difficulty: "intermediate"
audience: "developers"

# Custom Fields
customField: "custom value"
metadata:
  version: "1.0"
  lastReviewed: "2024-01-20"
  reviewedBy: "Tech Team"
---
```

### Field Descriptions

| Field | Type | Description | Required |
|-------|------|-------------|----------|
| `title` | String | Article title | ? |
| `date` | DateTime | Publication date | ? |
| `last_modified` | DateTime | Last change, used by the sitemap and feeds | Optional |
| `series` | String | Series the item belongs to, for `ContentSeriesNavigation` | Optional |
| `series_order` | Number | Position in the series, starting at 1 | Optional |
| `redirect_from` | List | Earlier URLs of the item; requests for them redirect here (`UseOsirionRedirects`) | Optional |
| `description` | String | Meta description | Recommended |
| `author` | String | Author name | Recommended |
| `categories` | Array | Content categories | Optional |
| `tags` | Array | Content tags | Optional |
| `published` | Boolean | Publishing status | Optional (default: true) |
| `featured` | Boolean | Featured content flag | Optional |
| `draft` | Boolean | Draft status | Optional |
| `slug` | String | URL slug | Optional (auto-generated) |
| `excerpt` | String | Content summary | Optional |
| `featuredImage` | String | Featured image URL | Optional |
| `readingTime` | String | Estimated reading time | Optional |

## Content Components

### Displaying Content Lists

#### Basic Content List
```razor
<ContentList Directory="blog" Count="10" />
```

#### Filtered Content List
```razor
<ContentList 
    Directory="blog"
    Category="tutorials"
    Count="5"
    SortBy="SortField.Date"
    SortDirection="SortDirection.Descending"
    ShowExcerpt="true"
    ShowMetadata="true" />
```

#### Featured Content
```razor
<ContentList 
    FeaturedCount="3"
    ShowExcerpt="true"
    ShowMetadata="true" />
```

### Displaying Individual Content

#### Content View
```razor
<ContentView Path="blog/getting-started.md" />
```

#### Content with Custom Layout
```razor
<ContentView Path="blog/getting-started.md">
    <HeaderTemplate Context="content">
        <div class="custom-header">
            <h1>@content.Title</h1>
            <p class="lead">@content.Excerpt</p>
        </div>
    </HeaderTemplate>
    
    <FooterTemplate Context="content">
        <div class="custom-footer">
            <p>Published on @content.PublishDate?.ToString("MMMM dd, yyyy")</p>
        </div>
    </FooterTemplate>
</ContentView>
```

### Navigation and Discovery

#### Categories List
```razor
<CategoriesList />

<!-- With custom formatting -->
<CategoriesList>
    <CategoryTemplate Context="category">
        <div class="category-card">
            <h5>@category.Name</h5>
            <span class="badge">@category.Count</span>
        </div>
    </CategoryTemplate>
</CategoriesList>
```

#### Tag Cloud
```razor
<TagCloud MaxTags="20" />

<!-- With size variation -->
<TagCloud 
    MaxTags="30"
    ShowCounts="true"
    SizeVariation="true" />
```

#### Search Box
```razor
<SearchBox 
    Placeholder="Search articles..."
    MinSearchLength="3"
    MaxResults="10"
    OnSearch="@HandleSearch" />

@code {
    private void HandleSearch(string query)
    {
        // Handle search results
        NavigationManager.NavigateTo($"/search?q={Uri.EscapeDataString(query)}");
    }
}
```

#### Series Navigation

Give the parts of a multi-part article the same `series` and number them with `series_order`:

```yaml
---
title: "Deploying the blog"
series: "Build a blog with Osirion"
series_order: 3
---
```

`ContentSeriesNavigation` shows "Part 3 of 5", lists the parts and links the previous and next one. Place it in the
article page, above or below the content:

```razor
<ContentSeriesNavigation Item="@item" />
```

Parts are read from the item's locale and ordered by `series_order`; parts without a number follow, oldest first.
Nothing renders for an item outside a series or a series with one published part. `ContentUrlFormatter`,
`PositionFormat` (`Part {0} of {1}`), `PreviousText`, `NextText` and `ShowPartList` adjust it.

#### Author Pages

Every item's `author` front matter field can drive an author page: list that author's items with `ContentList`
and introduce them with `OsirionAuthorCard`. Link bylines to the page with `ContentView`'s `AuthorUrlFormatter`.

```razor
@page "/authors/{Slug}"

<OsirionAuthorCard Name="Jane Developer" Role="Principal engineer" Bio="Writes about Blazor." />
<ContentList Author="Jane Developer" Directory="blog" />

@code {
    [Parameter] public string Slug { get; set; } = "";
}
```

```razor
<ContentView Path="@Path" AuthorUrlFormatter="@(name => $"/authors/{Uri.EscapeDataString(name.ToLowerInvariant().Replace(' ', '-'))}")" />
```

The author filter matches the whole name, ignoring case. Map the slug in the URL to the author's name and details
(from configuration or a small lookup) in the page.

#### Directory Navigation
```razor
<DirectoryNavigation CurrentDirectory="blog" />

<!-- With custom structure -->
<DirectoryNavigation CurrentDirectory="docs">
    <DirectoryTemplate Context="directory">
        <div class="nav-directory">
            <i class="fas fa-folder"></i>
            <a href="/docs/@directory.Path">@directory.Name</a>
            <span class="count">(@directory.ItemCount)</span>
        </div>
    </DirectoryTemplate>
</DirectoryNavigation>
```

## Advanced Features

### Content Querying

#### Programmatic Content Access
```csharp
@inject IContentProvider ContentProvider

@code {
    private List<ContentItem> articles = new();
    
    protected override async Task OnInitializedAsync()
    {
        var query = new ContentQuery
        {
            Directory = "blog",
            Category = "tutorials",
            Tag = "blazor",
            SearchQuery = "getting started",
            FromDate = new DateTime(2024, 1, 1),
            ToDate = DateTime.Now,
            Count = 10,
            Skip = 0,
            SortBy = SortField.Date,
            SortDirection = SortDirection.Descending,
            IsFeatured = true
        };
        
        articles = (await ContentProvider.GetItemsByQueryAsync(query)).ToList();
    }
}
```

#### Complex Queries
```csharp
// Get recent articles in multiple categories
var query = new ContentQuery
{
    Categories = new[] { "tutorials", "announcements" },
    FromDate = DateTime.Now.AddDays(-30),
    SortBy = SortField.Date,
    SortDirection = SortDirection.Descending
};

// Get popular articles by tags
var popularQuery = new ContentQuery
{
    Tags = new[] { "blazor", "popular" },
    IsFeatured = true,
    Count = 5
};
```

### Content Transformation

#### Custom Content Processors
```csharp
public class CodeHighlightProcessor : IContentTransformer
{
    public async Task<string> TransformAsync(string content, ContentItem item)
    {
        // Add syntax highlighting to code blocks
        return content.Replace("```csharp", "```csharp class=\"language-csharp\"");
    }
}

// Register the processor
builder.Services.AddScoped<IContentTransformer, CodeHighlightProcessor>();
```

#### Link Processing
```csharp
public class InternalLinkProcessor : IContentTransformer
{
    public async Task<string> TransformAsync(string content, ContentItem item)
    {
        // Convert relative links to absolute
        var pattern = @"\[([^\]]+)\]\((?!http)([^)]+)\)";
        return Regex.Replace(content, pattern, 
            match => $"[{match.Groups[1].Value}](/content/{match.Groups[2].Value})");
    }
}
```

### Caching Strategies

#### Configure Caching
```csharp
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.UseCache = true;
        options.CacheExpirationMinutes = 60;
        options.SlidingExpiration = TimeSpan.FromMinutes(15);
        options.CacheSize = 100; // Number of items
    });
});
```

#### Custom Cache Provider
```csharp
public class RedisCacheProvider : IContentCacheProvider
{
    private readonly IDistributedCache cache;
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var json = await cache.GetStringAsync(key);
        return json != null ? JsonSerializer.Deserialize<T>(json) : null;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }
}
```

## SEO and Metadata

### Automatic SEO Metadata

```razor
<SeoMetadataRenderer 
    Content="@currentContent"
    SiteName="My Website"
    BaseUrl="https://mysite.com"
    DefaultImage="/images/default-social.jpg"
    TwitterHandle="@myhandle" />
```

### Custom Meta Tags

```csharp
public class CustomSeoProcessor : ISeoProcessor
{
    public void ProcessSeoData(ContentItem content, SeoData seoData)
    {
        // Add custom OpenGraph tags
        seoData.OpenGraph["article:author"] = content.Author;
        seoData.OpenGraph["article:published_time"] = content.PublishDate?.ToString("yyyy-MM-dd");
        
        // Add structured data
        seoData.StructuredData = new
        {
            Type = "Article",
            Headline = content.Title,
            Author = new { Name = content.Author },
            DatePublished = content.PublishDate,
            Image = content.FeaturedImage
        };
    }
}
```

### Live Demos in Content

Register Razor components as demos and embed them in Markdown with a fenced `demo` block:

````markdown
```demo
install-tabs
```
````

```csharp
builder.Services.AddOsirionContentDemos(demos => demos
    .Add<InstallTabsDemo>("install-tabs", source: ContentDemoRegistry.ReadEmbeddedSource(
        typeof(InstallTabsDemo).Assembly, "MySite.Components.Demos.InstallTabsDemo.razor")));
```

The block renders the component, with Preview and Code tabs when the demo has source code. See
`docs/components/core/OsirionContentDemo.md`.

### Redirects

When a page moves, list its earlier URLs in `redirect_from` and add the middleware before the page endpoints:

```yaml
---
title: "Tabs"
redirect_from:
  - /docs/components/core/content/tabs
---
```

```csharp
using Osirion.Blazor.Cms.Web.Redirects;

app.UseOsirionRedirects(redirects =>
{
    // Pages that are not content items
    redirects.Redirects["/docs/components/core"] = "/components";
});
```

- GET and HEAD requests for an old URL get a 301 to the item's current URL (`/{item.Url}`, or `ContentUrlFormatter`);
  `Permanent = false` answers 302. The query string is kept; the app's path base is added.
- Paths match ignoring case and a trailing slash. A published page at the same address wins over a redirect, and the
  first item to claim an old URL keeps it; both cases are logged as warnings.
- The map is built from published content and reused for `CacheDuration` (5 minutes by default). If the content
  cannot be read, the fixed redirects keep working and the failure is not cached.
- Fixed redirects are checked at startup: sources must be site paths, targets site paths or absolute http(s) URLs.

### Sitemap and Feeds

`MapOsirionFeeds` maps three endpoints built from the published content:

| Path | Document | Content |
|------|----------|---------|
| `/sitemap.xml` | sitemaps.org XML sitemap | Every published item (up to 50,000 URLs), plus `AdditionalSitemapUrls` |
| `/feed.xml` | RSS 2.0 | The newest `FeedItemCount` items of `FeedDirectory` and `FeedLocale` |
| `/atom.xml` | Atom 1.0 | The same items as the RSS feed |

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://mysite.com";   // absolute http(s) URL; checked at startup
    feeds.FeedDirectory = "blog";           // default: all content
    feeds.FeedLocale = "en";                // default: every locale
    feeds.FeedItemCount = 20;               // default: 20
    feeds.AdditionalSitemapUrls = ["/", "/blog"];
});
```

- Set `SiteUrl` in production. Without it, URLs are built from the request's scheme, host and path base, so they
  follow whatever `Host` header a request sends.
- The title and description default to `Osirion:Seo:SiteName` and `SiteDescription`; set `Title` and `Description`
  to override them.
- Item links are `/{item.Url}`, the same path the content lists use. When the site routes content elsewhere, set
  `ContentUrlFormatter` (it may return a path or an absolute URL).
- `lastmod` and Atom `updated` come from the front matter's `last_modified`, then the file's write time (file system
  provider), then `date`. Set `last_modified` when dates must survive a fresh checkout or deployment.
- Items whose `seo.robots` contains `noindex` are left out of the sitemap.
- With locale routing (`AddOsirionLocalization`), each `AdditionalSitemapUrls` path is listed in every culture, each
  item at its address in its own culture, and every `url` carries its language versions as
  `<xhtml:link rel="alternate" hreflang="...">` plus `x-default` (translations share a `ContentId`). Without locale
  routing the sitemap is unchanged.
- Responses carry `Cache-Control: public, max-age=600`; change it with `CacheDuration`. Turn single endpoints off
  with `EnableSitemap`, `EnableRss` or `EnableAtom`.
- Point crawlers at the sitemap from `robots.txt` with `Sitemap: https://mysite.com/sitemap.xml`, and let feed readers
  find the feeds with `<link rel="alternate" type="application/rss+xml" href="/feed.xml">` in the page head.

## Multi-language Support

### Enable Localization

```csharp
builder.Services.AddOsirionBlazor(osirion => {
    osirion.AddGitHubCms(options => {
        options.EnableLocalization = true;
        options.DefaultLocale = "en";
        options.SupportedLocales = new[] { "en", "es", "fr", "de" };
    });
});
```

### Content Structure for Localization

```
content/
??? en/                    # English content
?   ??? blog/
?   ?   ??? getting-started.md
?   ??? pages/
?       ??? about.md
??? es/                    # Spanish content
?   ??? blog/
?   ?   ??? comenzando.md
?   ??? pages/
?       ??? acerca-de.md
??? fr/                    # French content
    ??? blog/
    ?   ??? commencer.md
    ??? pages/
        ??? a-propos.md
```

### Derived Locales (Transliteration)

A locale can be made from another by transliteration when content loads, so content written once in Serbian Cyrillic
(`sr`) is also served in Latin script (`sr-Latn`). The FileSystem and GitHub providers share the setting:

```json
"DerivedLocales": [
  { "Locale": "sr-Latn", "From": "sr", "Transliteration": "serbian-cyrillic-to-latin" }
]
```

`From` must be one of `SupportedLocales`; the derived locale is added to them. Every `From` item without a file of the
same localization id in the derived locale gets a read-only derived item (`IsDerived`, `DerivedFromLocale`) with the
same localization id and address, its text and rendered HTML transliterated (code, autolinks and addresses are kept;
links to the site's own pages under `/sr/` move under `/sr-Latn/`). A real file always wins. Derived items are never
saved back, and the admin, which edits files, does not list them. The rules, the validation and what is not derived are
in the [localization guide](../content/en/docs/localization.md#serve-a-second-script-without-a-second-file).

### Localized Content Components

```razor
<LocalizedContentView 
    Path="@contentPath"
    CurrentLocale="@currentLocale"
    FallbackLocale="en" />

<LocalizedNavigation 
    CurrentLocale="@currentLocale"
    OnLocaleChanged="@HandleLocaleChange" />

@code {
    private string currentLocale = "en";
    
    private async Task HandleLocaleChange(string newLocale)
    {
        currentLocale = newLocale;
        await LocalStorage.SetItemAsync("locale", newLocale);
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }
}
```

## Admin Interface

### Enable Admin Interface

```csharp
// Add admin services
builder.Services.AddOsirionBlazor(osirion => {
    osirion
        .AddGitHubCms(options => { ... })
        .AddCmsAdmin(options => {
            options.EnableAdmin = true;
            options.AdminPath = "/admin";
            options.RequireAuthentication = true;
        });
});
```

### Admin Components

#### Markdown Editor
```razor
<MarkdownEditorPreview 
    Content="@content"
    ContentChanged="@OnContentChanged"
    ShowToolbar="true"
    SyncScroll="true"
    AutoSave="true"
    AutoSaveInterval="30000" />

@code {
    private string content = "";
    
    private async Task OnContentChanged(string newContent)
    {
        content = newContent;
        // Auto-save logic
        await SaveDraftAsync(newContent);
    }
}
```

#### Content Manager
```razor
<ContentManager 
    AllowCreate="true"
    AllowEdit="true"
    AllowDelete="true"
    ShowDrafts="true">
    
    <ContentListTemplate Context="items">
        @foreach (var item in items)
        {
            <ContentManagerCard 
                Item="@item"
                OnEdit="@(() => EditContent(item))"
                OnDelete="@(() => DeleteContent(item))" />
        }
    </ContentListTemplate>
</ContentManager>
```

### Authentication Integration

```csharp
// Configure authentication
builder.Services.AddAuthentication("GitHub")
    .AddGitHub(options => {
        options.ClientId = "your-github-app-id";
        options.ClientSecret = "your-github-app-secret";
    });

// Authorize admin area
app.MapRazorPages().RequireAuthorization();
app.MapBlazorHub().RequireAuthorization();
```

## Webhooks and Automation

### Setup GitHub Webhooks

A webhook delivery refreshes the content cache of every GitHub provider that tracks the pushed branch, so new content appears without a restart.

1. **Register the services and the middleware**:

```csharp
builder.Services.AddOsirion(builder.Configuration);
builder.Services.AddGitHubWebhookAndPolling();

var app = builder.Build();

app.UseGitHubWebhook(); // handles POST /api/github/webhook
```

2. **Give each provider a webhook secret**. Use a long random value and keep it out of source control (user secrets, environment variables or a key vault):

```bash
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Blog:WebhookSecret" "<long random value>"
```

3. **Create the webhook in GitHub** (repository Settings, Webhooks):
   - **Payload URL**: `https://<your host>/api/github/webhook`. Use the canonical host and the exact path, so no redirect sits in between.
   - **Content type**: `application/json`
   - **Secret**: the same value as `WebhookSecret`
   - **Events**: just the push event

### How Deliveries Are Verified

- Every delivery must carry an `X-Hub-Signature-256` header that verifies against the `WebhookSecret` of at least one configured provider. The HMAC-SHA256 signature is checked over the raw request body with a constant-time comparison.
- While no provider has a `WebhookSecret`, every delivery is rejected. Such deliveries, and deliveries whose signature header is missing or malformed, are rejected before the body is read.
- Bodies larger than 5 MiB are rejected: 413 when the declared length is too large, 400 otherwise. Chunked bodies up to 5 MiB are accepted; the middleware lowers the server's request body limit for the webhook path to 10 MiB to leave room for chunked framing, and keeps a stricter limit the host has set.
- A verified push refreshes each provider whose `Owner` and `Repository` match the push, whose `Branch` matches the pushed branch exactly, and whose own secret verified the signature. The cache is found by the provider's exact `ProviderId`, or `github-{Owner}-{Repository}` when no `ProviderId` is set.
- Pushes to tags, deleted branches and branches no provider tracks are acknowledged with 202 and refresh nothing. `ping` and other events are acknowledged once verified.
- Responses are 202 (accepted), 400 (rejected), 413 (too large) or 500 (unexpected error), each with a short generic body. Check GitHub's "Recent Deliveries" view after a change of secret or URL.

Provider options are read once at startup, so a new secret takes effect after the application restarts. Set it in GitHub and in the application at the same time.

### Custom Webhook Handler

You can register your own `IGitHubWebhookHandler` in place of the built-in one. A custom handler must verify the `X-Hub-Signature-256` header against your secret before it acts on the payload, and reject the request when it does not verify. `GitHubWebhookHandler` is the reference implementation.

### Automated Workflows

#### GitHub Actions for Content Processing

```yaml
# .github/workflows/content-processing.yml
name: Process Content Updates

on:
  push:
    paths:
      - 'content/**'

jobs:
  process-content:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Optimize Images
        run: |
          # Optimize images in content
          find content -name "*.jpg" -o -name "*.png" | xargs -I {} sh -c 'imagemin {} --out-dir={}'
      
      - name: Generate Thumbnails
        run: |
          # Generate thumbnails for featured images
          ./scripts/generate-thumbnails.sh
      
      - name: Trigger Cache Refresh
        run: |
          curl -X POST "https://mysite.com/api/refresh-cache" \
            -H "Authorization: Bearer ${{ secrets.API_TOKEN }}"
```

## Best Practices

### Content Organization

1. **Use Consistent Naming Conventions**
   - Lowercase with hyphens for files and directories
   - Date prefixes for time-sensitive content
   - Descriptive, SEO-friendly names

2. **Organize by Purpose and Audience**
   - Separate blogs, documentation, and pages
   - Use categories and tags consistently
   - Create clear content hierarchies

3. **Optimize for Performance**
   - Keep images optimized and appropriately sized
   - Use efficient directory structures
   - Implement appropriate caching strategies

### Content Writing

1. **Write Good Frontmatter**
   - Include all relevant metadata
   - Use consistent field names
   - Validate YAML syntax

2. **Structure Content Well**
   - Use clear headings and sections
   - Include table of contents for long articles
   - Add relevant internal and external links

3. **Optimize for SEO**
   - Write descriptive titles and meta descriptions
   - Use relevant keywords naturally
   - Include alt text for images

### Development Workflow

1. **Use Branching Strategy**
   - Create feature branches for content changes
   - Use pull requests for content review
   - Maintain staging and production branches

2. **Implement Content Review Process**
   - Require reviews for content changes
   - Use GitHub's review features
   - Test content in staging environment

3. **Monitor Performance**
   - Track content loading times
   - Monitor API rate limits
   - Set up alerts for failures

### Security Considerations

1. **Protect API Tokens**
   - Store tokens as environment variables
   - Use minimal required permissions
   - Rotate tokens regularly

2. **Validate Webhook Payloads**
   - Verify webhook signatures
   - Validate payload structure
   - Log security events

3. **Sanitize Content**
   - HTML sanitization is on by default; extend the allow-list with `AddOsirionHtmlSanitizer` rather than setting `SanitizeHtml="false"` for repository content
   - Validate frontmatter data
   - Escape user-generated content

This comprehensive guide covers all aspects of using GitHub as a CMS with Osirion.Blazor. For additional help or advanced use cases, consult the [API Reference](API_REFERENCE.md) or reach out to the community.