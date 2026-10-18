---
id: 'cms-web-components-overview'
order: 2
layout: docs
title: Osirion.Blazor.Cms.Web
permalink: /docs/packages/cms-web
description: Set up Osirion.Blazor.Cms.Web and render Markdown content from GitHub or the file system as lists, articles, navigation, SEO metadata and feeds.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-11
featured_image: 'https://media.getosirion.com/og/docs-packages-cms-web.jpg'
categories:
- Components
- CMS Web Components
- Content Management
tags:
- cms
- markdown
- github
- content-provider
- seo
is_featured: true
published: true
slug: cms-web
lang: en
redirect_from:
- /docs/components/cms-web
custom_fields:
  group: packages
seo_properties:
  title: 'CMS Web Components | Osirion.Blazor'
  description: 'Set up Osirion.Blazor.Cms.Web and render Markdown content from GitHub or the file system in Blazor.'
  image: 'https://media.getosirion.com/og/docs-packages-cms-web.jpg'
  canonical: 'https://getosirion.com/docs/packages/cms-web'
  lang: en
  robots: index, follow
  og_title: 'CMS Web Components | Osirion.Blazor'
  og_description: 'Render Markdown content from GitHub or the file system in Blazor.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages-cms-web.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'CMS Web Components | Osirion.Blazor'
  twitter_description: 'Render Markdown content from GitHub or the file system in Blazor.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages-cms-web.jpg'
  meta_tags: {}
---

# Osirion.Blazor.Cms.Web
`Osirion.Blazor.Cms.Web` renders Markdown content for visitors: lists, articles, navigation, head metadata, a sitemap
and feeds. The content comes from a provider that reads Markdown files with YAML front matter from a GitHub
repository or a local folder. Every component renders under static SSR, so pages work without script and are fully
indexable. This site is built with them.

## Install

The `Osirion.Blazor` package includes the CMS modules and the `AddOsirion` setup methods:

```bash
dotnet add package Osirion.Blazor
```

To take only the content components, install
[Osirion.Blazor.Cms.Web](https://www.nuget.org/packages/Osirion.Blazor.Cms.Web) and register content with
`AddOsirionContent` from `Osirion.Blazor.Cms.Web.DependencyInjection`. Both target .NET 9 and .NET 10.

## Register a content provider

Content from a folder in your project, configured in code:

```csharp
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(osirion => osirion
    .UseContent(content => content.AddFileSystem(options =>
    {
        options.BasePath = Path.Combine(builder.Environment.ContentRootPath, "content");
        options.EnableLocalization = true;
        options.SupportedLocales = ["en", "sr"];
    })));
```

Content from a GitHub repository, configured in `appsettings.json` and read with
`builder.Services.AddOsirion(builder.Configuration)`:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Blog": {
            "Owner": "your-account",
            "Repository": "your-content-repo",
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

Each child of `Osirion:Cms:Web:GitHub` registers one provider. Keep `ApiToken` and `WebhookSecret` out of
`appsettings.json`; set them with user secrets or environment variables. A file system provider can be configured the
same way under `Osirion:Cms:Web:FileSystem` (with `BasePath`).

Then import the components, for example in `_Imports.razor`:

```razor
@using Osirion.Blazor.Cms.Web.Components
```

## Write content

Each Markdown file starts with front matter in snake_case:

```markdown
---
title: "Getting started"
description: "Install the package and render your first article."
author: "Jane Developer"
date: 2026-10-01
last_modified: 2026-10-08
featured_image: /images/getting-started.png
categories:
- Guides
tags:
- setup
slug: getting-started
published: true
---

# Getting started
```

An item's URL is its folder path below the content root plus its `slug`, without the locale folder when localization
is on. Other keys the
providers read include `id` (pairs translations), `order`, `is_featured`, `lang`, `custom_fields` and
`seo_properties` (title, description, canonical, robots, Open Graph and Twitter values, `json_ld`). From 4.0,
`series` and `series_order` group items into a series. A folder's `_index.md` describes the folder itself.

Markdown is rendered to HTML and sanitized against an allow-list when it is loaded, and the article components
sanitize again when they write it. Script, event handler attributes, `javascript:` URLs, forms, iframes and inline
SVG are removed. To allow an element your content needs, extend the list:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options => options.AllowElement("chart", "type", "data"));
```

## Components

**Core:** [ContentList](/components/content-list),
[ContentView](/components/content-view),
[LocalizedContentView](/components/localized-content-view),
[ContentPage](/components/content-page),
[ContentRenderer](/components/content-renderer),
[SeoMetadataRenderer](/components/seo-metadata-renderer) and
[feeds and sitemap](/components/feeds-and-sitemap) (from 4.0). See the
[core overview](/components/cms-pages).

**Navigation:** [CategoriesList](/components/categories-list),
[ContentBreadcrumbs](/components/content-breadcrumbs),
[DirectoryNavigation](/components/directory-navigation),
[LocalizedNavigation](/components/localized-navigation),
[TagCloud](/components/tag-cloud),
[SearchBox](/components/search-box),
[TableOfContents](/components/table-of-contents),
[OsirionContentNavigation](/components/osirion-content-navigation) and, from 4.0,
[ContentSeriesNavigation](/components/series-navigation). See the
[navigation overview](/components/cms-navigation).

**Pages and sections:** `ArticlePage`, `DocumentPage`, `HomePage` and `LandingPage` render complete pages from an
item, and `ContentSection`, `FeaturedPostsSection` and `OsirionContentListSection` render blocks of content. Except
`ArticlePage`, these are in the `Osirion.Blazor.Cms.Components` namespace.

The editing interface is a separate package, `Osirion.Blazor.Cms.Admin`.

## Related

- [GitHub as a CMS for Blazor](/blog/github-cms-with-blazor)
- [Structuring content repositories](/blog/structuring-content-repositories)
- [Source on GitHub](https://github.com/demonjic/Osirion.Blazor)
