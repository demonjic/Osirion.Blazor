---
title: "Blazor SEO Checklist for Static SSR Content Sites"
author: "Osirion Team"
date: "2025-01-25"
last_modified: 2026-10-11
description: "Check rendering, titles, canonical URLs, robots rules, structured data, sitemaps and feeds on a Blazor content site built with Osirion.Blazor."
slug: "blazor-seo-ssr-ssg-checklist"
categories: [SEO]
tags: [SEO, Static SSR, Canonical URL, Structured Data, Sitemap]
featured_image: 'https://media.getosirion.com/og/blog-blazor-seo-ssr-ssg-checklist.jpg'
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-blazor-seo-ssr-ssg-checklist.jpg'
  title: "Blazor SEO Checklist for Static SSR"
  description: "What Osirion.Blazor writes into the page head and what you configure yourself: canonical URLs, robots rules, JSON-LD, sitemap and feeds."
  type: "Article"
---

# Blazor SEO checklist for static SSR content sites

Search engines and link previews read the HTML your server sends. This checklist walks through a Blazor content
site built with Osirion.Blazor: what the library writes into each page for you, and what you still set up yourself.
Work through it top to bottom on a new site, or use it to audit one that is already live.

## Send the article in the first response

Blazor Web Apps render pages with static server-side rendering (static SSR) unless a page or component asks for an
interactive render mode. Keep content pages static. Osirion's page components (`ArticlePage`, `DocumentPage` and
`HomePage`) load the item on the server and, since 4.0, no longer use streaming rendering, so the first response
already contains the title and the body.

A blog route from the example app:

```razor
@page "/blog/{slug}"
@using Osirion.Blazor.Cms.Web.Components

<ArticlePage DirectoryName="blog" ItemSlug="@Slug" SchemaTypes="new[] { SchemaType.BlogPosting }" />

@code {
    [Parameter]
    public string? Slug { get; set; }
}
```

Missing content answers with HTTP 404: `NavigationManager.NotFound()` on .NET 10 and the response status on .NET 9.
On .NET 10, give the router a not-found page (`<Router NotFoundPage="typeof(Pages.NotFound)">`); without one the 404
response has an empty body.

To check, fetch a page without a browser and look for the heading:

```bash
curl -s https://example.com/blog/my-post | grep "<h1"
```

Osirion.Blazor renders pages on request; it does not export a static copy of the site. If you need static files,
crawl the running site with a tool of your choice: it saves the same HTML.

## Give every page its own title, description and preview image

`ArticlePage`, `HomePage`, `LandingPage` and `ContentList` place `SeoMetadataRenderer` in the head through
`HeadContent`, so your `App.razor` needs `<HeadOutlet />`. The renderer writes the `<title>` (the page title, a
separator and the site name, shortened to 60 characters), the meta description, Open Graph and Twitter card tags,
robots directives, the canonical link and JSON-LD.

The values come from the front matter. `seo_properties` overrides the plain keys where search results or social
previews need different text:

```yaml
---
title: "Structuring content repositories"
description: "Folders, slugs and front matter for a Markdown content repository."
featured_image: /assets/images/structure.png
seo_properties:
  title: "Structure a Markdown content repository"
  description: "How to lay out folders, slugs and front matter so URLs stay stable."
  og_image_url: /assets/images/structure-social.png
---
```

The preview image is `og_image_url`, then `featured_image`, then the site default. Site-wide values live in the
`Osirion:Seo` configuration section, which `AddOsirion(builder.Configuration)` binds:

```json
{
  "Osirion": {
    "Seo": {
      "SiteName": "My Site",
      "SiteDescription": "Guides and release notes for My Product.",
      "OrganizationName": "My Company",
      "DefaultImageUrl": "/assets/images/social-default.png",
      "TwitterSite": "@mysite"
    }
  }
}
```

Checklist: every published file has a `title` and a `description` of about 150 characters, and pages that people
share have an image of 1200 by 630 pixels.

## Declare one canonical URL per page

Since 4.0 the canonical URL, `og:url` and the URLs in structured data are the current address without its query
string and fragment, so tracking parameters do not create duplicate pages. `ContentList` keeps the `page` parameter
on page 2 and later through `CanonicalQueryParameters`. To point a page somewhere else, for example an article first
published on another site, set the canonical URL in the front matter:

```yaml
seo_properties:
  canonical: "https://example.com/blog/original-post"
```

The renderer builds absolute URLs from the address of the request, so serve the site under one scheme and one host.
The example app redirects everything else outside Development:

```csharp
using Microsoft.AspNetCore.Rewrite;

var rewriteOptions = new RewriteOptions()
    .AddRedirectToHttpsPermanent()
    .AddRedirectToNonWwwPermanent();
app.UseRewriter(rewriteOptions);
```

## Control what gets indexed

- **One page.** `seo_properties.robots` replaces the default robots directive. A value that contains `noindex` also
  keeps the page out of the sitemap.

  ```yaml
  seo_properties:
    robots: "noindex, follow"
  ```

- **Drafts.** `published: false` hides a file: lists, feeds and the sitemap leave it out, and its page answers 404.
  Since 4.0 a file without a `published` key is published.
- **robots.txt.** The simplest option is a static `wwwroot/robots.txt` that points to your sitemap:

  ```text
  User-agent: *
  Disallow: /search/
  Sitemap: https://example.com/sitemap.xml
  ```

  Alternatively, `app.UseOsirionRobotsGenerator()` answers `/robots.txt` with the content item whose URL is `robots`
  (a `robots.txt` at the root of the content folder, when the provider loads `.txt` files) and otherwise returns a
  default that disallows `/search/`. From the release after 4.1.0 it serves the item's source text, as plain text. Use one approach, not both: registered before the static files, as in the
  example app, the middleware answers and the static file is never served.

## Describe the page with structured data

`SchemaTypes` chooses the schema.org types the renderer generates, for example `BlogPosting` for posts, or `Article`
with `BreadcrumbList` for documentation. Without it, the renderer uses the site default from
`Osirion:Seo:DefaultSchemaTypes`, then `seo_properties.type` when that names a `SchemaType` value, then `WebPage`.

For data the generator does not cover, write the JSON-LD yourself:

```yaml
seo_properties:
  json_ld: |
    {
      "@context": "https://schema.org",
      "@type": "HowTo",
      "name": "Publish your first post"
    }
```

Valid JSON-LD replaces the generated data. It is parsed and written again with `<`, `>` and `&` escaped, so it cannot
close the script element; invalid JSON-LD is dropped with a logged warning, and the generated data is used instead.

Available from 4.0: `OsirionFaq` renders questions and answers you write and the matching `FAQPage` data. Prefer it
to the FAQ data the renderer can derive from titles, which is off by default (`EnableGeneratedFaq`). Google no longer
shows FAQ rich results (since May 2026) or HowTo rich results (since 2023), so this data describes the page without
changing how it looks in Google results.

## Publish a sitemap and feeds

Available from 4.1. `MapOsirionFeeds` maps `/sitemap.xml`, an RSS 2.0 feed at `/feed.xml` and an Atom feed at
`/atom.xml`, built from the published content:

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://example.com";
    feeds.FeedDirectory = "blog";
    feeds.AdditionalSitemapUrls = ["/", "/blog"];
});
```

Set `SiteUrl` in production, so a forged `Host` header cannot change the URLs that crawlers store. The sitemap's
`lastmod` reads the `last_modified` front matter key, then the file's date, then the publication date, so update
`last_modified` when a page changes in a meaningful way. Items marked `noindex` stay out of the sitemap.

Point feed readers to the feed from the head in `App.razor`:

```html
<link rel="alternate" type="application/rss+xml" title="My Site" href="/feed.xml" />
```

## Link translations to each other

On a multilingual site, tell search engines which pages translate each other. `ArticlePage` passes
`AlternateLanguageUrls` to the renderer, one `language|url` entry per translation:

```razor
<ArticlePage DirectoryName="blog" ItemSlug="@Slug" Locale="@Locale" AlternateLanguageUrls="alternates" />

@code {
    [Parameter]
    public string? Locale { get; set; } = "en";

    [Parameter]
    public string? Slug { get; set; }

    private readonly List<string> alternates =
    [
        "en|https://example.com/blog/release-notes",
        "sr|https://example.com/sr/blog/release-notes"
    ];
}
```

The renderer writes one `<link rel="alternate" hreflang="...">` per entry and skips malformed entries with a
warning. [Localization for Blazor content sites](/blog/localization-for-blazor-content-sites) covers the rest of a
multilingual setup.

## The short version

- Content pages render with static SSR and answer 404 when content is missing.
- Every file has a `title`, a `description` and, for shared pages, an image.
- One scheme and one host; canonical URLs without tracking parameters.
- `noindex` and `published: false` where pages must stay out of search.
- `SchemaTypes` or `json_ld` for structured data.
- A sitemap and feeds with `SiteUrl` set (from 4.0).
- `hreflang` links between translations.

## Related

- [SeoMetadataRenderer](/components/seo-metadata-renderer)
- [Sitemap and feeds](/components/feeds-and-sitemap)
- [Structuring content repositories](/blog/structuring-content-repositories)
- [Performance tuning for content-heavy Blazor sites](/blog/performance-tuning-content-heavy-blazor-sites)
