---
title: "Build a Blog with GitHub as the CMS"
author: "Dejan Demonjić"
date: "2025-04-15"
last_modified: 2026-10-18
description: "Step by step: a statically rendered Blazor blog whose posts are Markdown files in a GitHub repository, with tags, categories, search and refresh on push."
tags: [blazor, github, cms, static-ssr, tutorial]
categories: [tutorials, blazor]
slug: "github-cms-with-blazor"
is_featured: true
featured_image: 'https://media.getosirion.com/og/blog-github-cms-with-blazor.jpg'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-github-cms-with-blazor.jpg'
---

# Build a Blog with GitHub as the CMS

In this tutorial you build a Blazor blog whose posts are Markdown files in a GitHub repository. Writing a post means
committing a file; the site picks it up from GitHub and renders it on the server. There is no database, the content has
Git history and pull requests for free, and every page is plain HTML that search engines and readers get in the first
response.

The site uses static server rendering throughout, so it needs no WebAssembly download and no open connection per
visitor.

## What you need

- The .NET 9 or .NET 10 SDK (Osirion.Blazor 4.0 targets both)
- A GitHub account, a repository for the content, and a personal access token that can read it
- Basic Blazor knowledge

## Create the project

Create a Blazor Web App without interactivity, which gives you static server rendering:

```bash
dotnet new blazor --interactivity None -o MyBlog
cd MyBlog
dotnet add package Osirion.Blazor
```

`Osirion.Blazor` is the meta-package: it brings the CMS, theming, navigation and analytics packages.

## Add content to GitHub

In your content repository, create `content/blog/hello-blazor.md`:

```markdown
---
title: "Hello, Blazor"
description: "The first post on the new blog."
author: "Jane Developer"
date: 2026-10-01
slug: hello-blazor
categories: [Getting Started]
tags: [blazor, markdown]
---

This post lives in a GitHub repository and is rendered by Blazor on the server.

## A heading

Markdown works as you would expect, including code blocks.
```

The front matter keys are snake_case (`featured_image`, `is_featured`, `last_modified`). A file without `published` is
published; add `published: false` to keep a draft off the site. Write multi-word tags with hyphens instead of spaces:
tag links turn spaces into hyphens, and the tag pages under [The pages](#the-pages) compare the URL value with each whole tag (case does not
matter, spaces do).

## Point the site at the repository

Add the provider to `appsettings.json`. The key under `GitHub` (`Blog` here) names the provider:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Blog": {
            "Owner": "your-github-user",
            "Repository": "your-content-repo",
            "Branch": "main",
            "ContentPath": "content",
            "IsDefault": true
          }
        }
      }
    },
    "Theming": {
      "Framework": "Bootstrap"
    }
  }
}
```

`Framework` tells the Osirion components to take their colors and button styles from Bootstrap, which the template
already includes.

The provider reads the repository through the GitHub API and sends no request without a token, even for a public
repository. Create a fine-grained personal access token with read-only access to the repository's contents, and keep it
out of source control:

```bash
dotnet user-secrets init
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Blog:ApiToken" "<your token>"
```

Then register the services in `Program.cs`:

```csharp
using Osirion.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddOsirion(builder.Configuration);
builder.Services.AddJSComponents();
```

`AddOsirion` reads the `Osirion` section and registers what it finds: the GitHub provider, the theming options, and the
navigation defaults. `AddJSComponents` registers the script loader that `OsirionStyles` and the article renderer use;
without it those components fail to render.

## Imports and styles

Add the namespaces to `Components/_Imports.razor`:

```razor
@using Osirion.Blazor.Cms.Web.Components
@using Osirion.Blazor.Theming.Components
```

Then render the Osirion stylesheets and a search box in `Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<OsirionStyles />

<div class="container py-3">
    <SearchBox />
    @Body
</div>
```

## The pages

One component serves both the post list and the posts. Create `Components/Pages/Blog.razor`:

```razor
@page "/blog"
@page "/blog/{Slug}"

@if (string.IsNullOrWhiteSpace(Slug))
{
    <PageTitle>Blog</PageTitle>
    <ContentList Directory="blog" ShowPagination ItemsPerPage="12" />
}
else
{
    <ArticlePage DirectoryName="blog" ItemSlug="@Slug" />
}

@code {
    [Parameter]
    public string? Slug { get; set; }
}
```

`ContentList` renders the posts in `content/blog` as cards, newest first, with page links that work without script.
The cards link to each post's URL, its folder below `ContentPath` plus its slug (`blog/hello-blazor`), which is the
route above. `ArticlePage` renders the post with a hero, breadcrumbs, a table of contents, the categories and tags,
and links to the previous and next post. When no post matches the slug, it answers with HTTP 404. Both write the page
title, description, canonical URL and structured data into the head.

The blog list, as getosirion.com renders it:

![A blog list rendered from Markdown files in a GitHub repository](https://media.getosirion.com/shots/blog-github-cms-with-blazor/blog-from-github-dark.webp){width=1600 height=1125}

Add pages for tags and categories, which is where `ArticlePage` and the cards link to. `Components/Pages/Tag.razor`:

```razor
@page "/tag/{TagSlug}"

<PageTitle>Tag: @TagSlug</PageTitle>
<ContentList Directory="blog" Tag="@TagSlug" ShowPagination />

@code {
    [Parameter]
    public string? TagSlug { get; set; }
}
```

`Components/Pages/Category.razor` is the same with `@page "/category/{CategorySlug}"` and `Category="@CategorySlug"`.
The category filter turns hyphens back into spaces, so `/category/getting-started` finds "Getting Started".

Finally, the search page that `SearchBox` submits to, `Components/Pages/Search.razor`:

```razor
@page "/search"

<PageTitle>Search</PageTitle>

@if (!string.IsNullOrWhiteSpace(Query))
{
    <ContentList SearchQuery="@Query" ShowPagination NoContentText="Nothing matched your search." />
}

@code {
    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }
}
```

The search matches any of the words in a post's title, description, content, categories or tags.

## Run it

```bash
dotnet run
```

Open `/blog`. The first request waits while the provider reads the repository; after that, the content is served
from memory.

## Refresh on push

To pick up a new commit as soon as you push it, let GitHub tell the site. Register the webhook handler in
`Program.cs`:

```csharp
using Osirion.Blazor.Cms.Infrastructure.Extensions;

builder.Services.AddGitHubWebhookAndPolling();

var app = builder.Build();

app.UseGitHubWebhook();
```

Store a random secret as `Osirion:Cms:Web:GitHub:Blog:WebhookSecret` (user secrets locally, your host's secret store in
production). In the content repository, add a webhook with the payload URL `https://your-site/api/github/webhook`,
content type `application/json`, the same secret, and the push event. Each delivery's signature is checked against the
secret, deliveries are rejected while no provider has a secret, and a push refreshes only the provider whose owner,
repository and branch match it. The refresh downloads only the files that changed.

## Where to go next

- Available from 4.0: `MapOsirionFeeds` in `Osirion.Blazor.Cms.Web.Feeds` maps `/sitemap.xml`, an RSS feed at
  `/feed.xml` and an Atom feed at `/atom.xml` from the published content.
- [The Osirion.Blazor CMS components](/blog/github-cms-components) describes each component used here and the ones
  that were not.
- [Hero sections in Blazor](/blog/modern-hero-sections-blazor) covers the hero `ArticlePage` renders.
- The [CMS web components](/docs/packages/cms-web) reference lists every parameter.
