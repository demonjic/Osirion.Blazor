---
id: 'getting-started'
order: 2
layout: docs
title: "Getting started"
permalink: /docs/getting-started
description: "Create a Markdown blog with dotnet new osirion-blog and run it in three commands, or add Osirion.Blazor to an existing Blazor Web App."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-18
featured_image: 'https://media.getosirion.com/og/docs-getting-started.jpg'
categories:
- Documentation
- Getting Started
tags:
- getting-started
- templates
- installation
- configuration
- markdown
- github
published: true
slug: getting-started
lang: en
custom_fields:
  group: getting-started
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-getting-started.jpg'
  title: 'Getting started with Osirion.Blazor'
  description: 'Start a Markdown blog with the osirion-blog template in three commands, or add Osirion.Blazor to an existing Blazor Web App.'
  image: 'https://media.getosirion.com/og/docs-getting-started.jpg'
  canonical: 'https://getosirion.com/docs/getting-started'
  robots: index, follow
---

# Getting started

There are two ways in: start from the `osirion-blog` template, which gives you a running blog in three commands, or
add Osirion to a Blazor app you already have.

## Start from the template

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

### Install the templates

```bash
dotnet new install Osirion.Blazor.Templates
```

### Create the blog

```bash
dotnet new osirion-blog -n MyBlog --SiteName "My Blog" --SiteUrl "https://blog.example.com"
cd MyBlog
```

`--SiteName` goes into the menu, the footer, the page titles and the feeds; `--SiteUrl` is the public address the feeds,
the sitemap and `robots.txt` link to. Both can be changed later in `appsettings.json`. The packages are restored as
the project is created.

### Run it

```bash
dotnet run
```

The browser opens on the home page. In a run with an empty NuGet package cache, the three steps took between 24 and
35 seconds, most of it the first restore and build.

![The blog the getting started commands create, running on localhost](https://media.getosirion.com/shots/docs-getting-started/first-blog-dark.webp){width=1600 height=1000}

### What you have

| Address | Page |
| --- | --- |
| `/` | The three newest posts |
| `/blog` | Every post, newest first, with a link to the RSS feed |
| `/blog/hello-world` | A post: title, date, reading time, table of contents, previous and next links |
| `/about` | `content/about.md` |
| `/feed.xml`, `/atom.xml`, `/sitemap.xml` | RSS, Atom and the sitemap, from the same posts |
| any other address | The not-found page, with status 404 |

Every page is rendered on the server as complete HTML; no interactive render mode is used. The menu, the footer and
the light and dark theme come from [`OsirionSiteLayout`](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/components/OsirionSiteLayout.md),
configured in `appsettings.json`.

### Make it yours

| To change | Edit |
| --- | --- |
| Posts | Add Markdown files to `content/blog`; the folder is watched, so a new post appears on reload |
| The About page | `content/about.md` |
| Menu, footer, social links | `Osirion:Site` in `appsettings.json` |
| Colors | `Osirion:Theming:DefaultPalette`: one of the [palettes](/components/palettes) |
| Your own styles | `wwwroot/app.css` |
| The public address | `Feeds:SiteUrl` in `appsettings.json` and the `Sitemap` line in `wwwroot/robots.txt` |

`dotnet publish` copies the `content` folder with the app. The post "Write your first post", which the template
includes, shows the front matter a post needs.

For documentation instead of a blog, use `dotnet new osirion-docs -n MyDocs`: pages in `content/docs` with side
navigation, a table of contents and previous and next links. For a product or company site, use
`dotnet new osirion-site -n MySite`: a home page with a hero, steps and the latest news, plus News, About and Contact.

## Add Osirion to an existing Blazor app

This guide takes a new Blazor Web App to a running blog: a list page and an article page rendered from Markdown
files, read from a local folder or from a GitHub repository. It follows the Bootstrap example in the repository and
is written for Osirion.Blazor 4.0.0, which targets .NET 9 and .NET 10. The steps were checked with the .NET 10
Blazor Web App template.

### Create the project and add the package

```bash
dotnet new blazor -n MySite
cd MySite
dotnet add package Osirion.Blazor
```

`Osirion.Blazor` brings in every module (Core, Cms, Navigation, Analytics and Theming) and the `AddOsirion`
registration method. You can reference single modules instead, but `AddOsirion` is only in this package.

The content pages in this guide render with static server-side rendering, so they need no interactive render mode.
The template's interactive setup can stay as it is.

### Register the services

In `Program.cs`, add the `using` and one `builder.Services` line after `AddRazorComponents()`:

```csharp
using Osirion.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOsirion(builder.Configuration);
```

`AddOsirion(builder.Configuration)` reads the `Osirion` section of your configuration. It registers a content
provider for each source under `Osirion:Cms`, and configures theming, analytics, navigation and SEO metadata when
their sections (`Osirion:Theming`, `Osirion:Analytics`, `Osirion:Navigation`, `Osirion:Seo`) exist. It always
registers enhanced navigation, scroll to top, the HTML sanitizer that cleans rendered Markdown and the services some
Osirion components use to load their own small scripts, including `OsirionStyles`. You do not call `AddJSComponents()`
yourself; a call you already have does no harm.

### Point the provider at your content

Choose a local folder or a GitHub repository. Both go in `appsettings.json` under `Osirion:Cms:Web`.

#### A local folder

```json
{
  "Osirion": {
    "Theming": {
      "Framework": "Bootstrap"
    },
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true
        }
      }
    }
  }
}
```

`BasePath` is used as given, so a relative path is resolved against the working directory of the process. Running
`dotnet run` from the project folder finds a `content` folder next to `Program.cs`; elsewhere, use an absolute path.
The provider watches the folder for changed files (`WatchForChanges`, on by default).

`dotnet publish` does not copy Markdown files. To ship the folder with the app, add this to the project file:

```xml
<ItemGroup>
  <Content Include="content\**" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

#### A GitHub repository

```json
{
  "Osirion": {
    "Theming": {
      "Framework": "Bootstrap"
    },
    "Cms": {
      "Web": {
        "GitHub": {
          "Site": {
            "Owner": "your-account",
            "Repository": "your-content-repository",
            "Branch": "main",
            "ContentPath": "content",
            "IsDefault": true
          }
        }
      }
    }
  }
}
```

Each child of `Osirion:Cms:Web:GitHub` registers one provider; `Site` is only its name. `ContentPath` is the folder
inside the repository, and `Branch` defaults to `main`.

The provider needs an access token, even for a public repository: without one it logs "Access token is required for
GitHub API requests" and loads no content. Use a token that can read the repository's contents, and keep it out of
`appsettings.json`. In development, store it with user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Site:ApiToken" "<your token>"
```

In production, set it as the environment variable `Osirion__Cms__Web__GitHub__Site__ApiToken` or in your secret
store.

Content is loaded once and cached. To refresh it when you push to the branch, register the webhook endpoint:

```csharp
using Osirion.Blazor.Cms.Infrastructure.Extensions;

builder.Services.AddGitHubWebhookAndPolling();

// after builder.Build()
app.UseGitHubWebhook();
```

Then set `WebhookSecret` on the provider (as a secret, like the token) and add a webhook in the repository settings
that sends `push` events to `https://your-site/api/github/webhook` with content type `application/json` and the same
secret. Deliveries are verified with the `X-Hub-Signature-256` header and rejected while no provider has a secret.
Register the webhook only together with a GitHub provider: with just the local folder provider, the webhook handler
cannot be built and the app fails at startup in Development.

If your files are split into language folders such as `content/en/blog`, also set `EnableLocalization` to `true`,
`DefaultLocale` and `SupportedLocales`. The same keys work for the local folder provider.

### Add the styles and imports

Add the Osirion namespaces to `Components/_Imports.razor`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Cms.Components
@using Osirion.Blazor.Cms.Web.Components
@using Osirion.Blazor.Theming.Components
```

Render `OsirionStyles` once in `Components/Layout/MainLayout.razor`, as the examples do:

```razor
@inherits LayoutComponentBase

<div class="page">
    @* the template's layout markup *@
    @Body
</div>

<OsirionStyles />
```

`OsirionStyles` loads the Osirion stylesheets and, for the `Framework` you configured (`Bootstrap`, `FluentUI`,
`MudBlazor` or `Radzen`), the adapter that maps the framework's colors onto Osirion's. It does not load the framework
itself; the Blazor template already loads Bootstrap. Leave `Framework` out to use Osirion's own palette. See
[OsirionStyles](/components/osirion-styles) for its parameters.

### Add a list page and an article page

Create `Components/Pages/Blog.razor`:

```razor
@page "/blog"
@page "/blog/{Slug}"

@if (string.IsNullOrWhiteSpace(Slug))
{
    <ContentList Directory="blog" ShowPagination />
}
else
{
    <ArticlePage DirectoryName="blog"
                 ItemSlug="@Slug"
                 SchemaTypes="new[] { SchemaType.BlogPosting }" />
}

@code {
    [Parameter]
    public string? Slug { get; set; }
}
```

`ContentList` lists the items in the `blog` folder, newest first, ten per page, and links each one to `blog/<slug>`.
`ArticlePage` finds the item by folder and slug and renders it with a hero, breadcrumbs, a table of contents, tags
and the page's SEO tags and structured data. An unknown slug answers with HTTP 404.

`ArticlePage` also lists related items under the article: other posts in the same folder that share a tag or a
category. When there are none, for example while the folder holds a single post, that section shows a "Page Not
Found" block; set `ShowRelatedContent="false"` until you have more posts.

### Write the first post

Create `content/blog/hello-world.md` (in the folder, or under `ContentPath` in the repository):

```markdown
---
title: "Hello, world"
description: "The first post on the new site."
author: "Your Name"
date: 2026-10-11
slug: hello-world
categories:
- News
tags:
- blazor
---

This post is a Markdown file in `content/blog`.
```

Front matter keys are snake_case (`featured_image`, `is_featured`, `last_modified`). A slug may contain only
lowercase letters, digits and hyphens. A file without a `published` key is published; add `published: false` to
keep it hidden.

### Run

```bash
dotnet run
```

Open `/blog` to see the list, then select the post.

## Next steps

- Browse the [components overview](/components) and the [CMS web components](/docs/packages/cms-web).
- Wrap the site in [OsirionPageLayout](/components/page-layout) for a header that hides while you
  scroll down and a footer that stays at the bottom of short pages.
- Pick a [palette](/components/palettes) and let visitors choose one with
  [ThemePicker](/components/theme-picker).
- Add analytics from the [analytics components](/docs/packages/analytics).
- Compare with the complete
  [Bootstrap example](https://github.com/demonjic/Osirion.Blazor/tree/master/examples/Osirion.Blazor.Example.Bootstrap),
  which also adds the home page, search, tags, categories and cookie consent.
