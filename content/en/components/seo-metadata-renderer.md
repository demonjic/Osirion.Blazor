---
order: 1
title: SeoMetadataRenderer
permalink: /components/seo-metadata-renderer
description: "Write the page title, meta description, canonical URL, robots, Open Graph, Twitter Card and JSON-LD tags for a content item or a list of items."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-24
featured_image: 'https://media.getosirion.com/og/components-seo-metadata-renderer.jpg'
categories:
- Components
- CMS Web Components
- Core Components
tags:
- seo
- metadata
- open-graph
- json-ld
- structured-data
slug: seo-metadata-renderer
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/seo-metadata-renderer
custom_fields:
  group: seo-and-feeds
seo_properties:
  title: 'SeoMetadataRenderer: head metadata and JSON-LD for Blazor'
  description: 'Write the title, description, canonical URL, robots, Open Graph, Twitter Card and JSON-LD tags for a Blazor content page with SeoMetadataRenderer.'
  image: 'https://media.getosirion.com/og/components-seo-metadata-renderer.jpg'
  canonical: 'https://getosirion.com/components/seo-metadata-renderer'
  lang: en
  robots: index, follow
  og_title: 'SeoMetadataRenderer - Osirion.Blazor'
  og_description: 'Writes the head metadata of a content page: title, canonical URL, robots, Open Graph, Twitter Card and JSON-LD.'
  og_image_url: 'https://media.getosirion.com/og/components-seo-metadata-renderer.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'SeoMetadataRenderer - Osirion.Blazor'
  twitter_description: 'Writes the head metadata of a content page: title, canonical URL, robots, Open Graph, Twitter Card and JSON-LD.'
  twitter_image_url: 'https://media.getosirion.com/og/components-seo-metadata-renderer.jpg'
  meta_tags: {}
---

# SeoMetadataRenderer

`SeoMetadataRenderer` writes the head metadata for a content page: the `<title>`, description, canonical URL,
robots directives, Open Graph and Twitter Card tags, and schema.org JSON-LD. Give it one item (`Content`) for an
article or several (`ContentItems`) for a list page. `ArticlePage` and `ContentList` already render it; add it
yourself when you build a page from `ContentView` or `ContentPage`. It writes into the page head and renders nothing
visible, and a live demo would replace the title and tags of this page, so this page has no demo.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/SeoMetadataRenderer.razor).

## Usage

Place it inside `<HeadContent>`, and keep `<HeadOutlet />` in your `App.razor`:

```razor
@using Osirion.Blazor.Cms.Web.Components

@if (item is not null)
{
    <HeadContent>
        <SeoMetadataRenderer Content="@item"
                             SchemaTypes="@(new[] { SchemaType.BlogPosting })" />
    </HeadContent>
}
```

Render it only when you have content: with neither `Content` nor `ContentItems` it writes only an empty title.

Site-wide defaults come from `SeoMetadataOptions`, either in code:

```csharp
using Osirion.Blazor.Cms.Web.DependencyInjection;

builder.Services.AddOsirionSeoMetadata(seo =>
{
    seo.SiteName = "Contoso Docs";
    seo.OrganizationName = "Contoso";
    seo.SiteLogoUrl = "/logo.png";
    seo.DefaultImageUrl = "/assets/images/cover.png";
    seo.TwitterSite = "@contoso";
});
```

or in `appsettings.json` under `Osirion:Seo`, which `AddOsirion(builder.Configuration)` reads:

```json
{
  "Osirion": {
    "Seo": {
      "SiteName": "Contoso Docs",
      "SiteUrl": "https://docs.contoso.com",
      "SiteLogoUrl": "/logo.png",
      "DefaultImageUrl": "/assets/images/cover.png"
    }
  }
}
```

## Parameters

A parameter you leave unset takes the matching `SeoMetadataOptions` value (see Behavior). The component renders no
element of its own, so the `Theme`, `Class` and `Style` parameters it inherits from the component base class have no
effect.

<!-- osirion:parameters SeoMetadataRenderer -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Content` | `ContentItem?` | `null` | The item a single content page writes its metadata for. Set this or `ContentItems`, not both. With neither (or an empty list), the component writes only an empty page title and no other tag. |
| `ContentItems` | `IReadOnlyList<ContentItem>?` | `null` | The items of a list page. The title becomes `{CollectionTitle} \| {site name}`, `og:type` is `website`, and the JSON-LD holds an article per item, after a `CollectionPage` and a `BreadcrumbList` when `SchemaTypes` lists them. Each item's article is the item's front matter `type` when it is `Article`, `BlogPosting` or `TechArticle`, then the first of those three in `SchemaTypes`, then `Article`. An empty list counts as not set. |
| `CollectionTitle` | `string?` | `null` | The title of a list page, used in the page title as `{CollectionTitle} \| {site name}` and, cut to 60 characters, as `og:title`. Read only with `ContentItems`. |
| `CollectionDescription` | `string?` | `null` | The description of a list page: the meta description (cut to 160 characters), `og:description` (cut to 200) and the source of the summary tags. Read only with `ContentItems`. |
| `SiteNameOverride` | `string?` | `null` | The site name written after the title and as `og:site_name`. When empty, the configured `SiteName` applies, then `OrganizationName`, then the host name of the site address. |
| `SiteDescription` | `string?` | `null` | A site description. The component accepts it but writes nothing from it. `MapOsirionFeeds` uses the configured `SiteDescription` as the feed description when the feed options set none. |
| `SiteLogoUrl` | `string?` | `null` | The address of the publisher logo in structured data, absolute or relative to the site address. When empty, the configured value applies, then `/logo.png` on the site address. |
| `TwitterSite` | `string?` | `null` | The handle of the site for `twitter:site`, for example `@contoso`. Nothing is written when empty. |
| `TwitterCreator` | `string?` | `null` | The handle of the author for `twitter:creator`, for example `@author`. Nothing is written when empty. |
| `FacebookAppId` | `string?` | `null` | The Facebook app ID written as `fb:app_id`. Nothing is written when empty. |
| `SchemaTypes` | `SchemaType[]?` | `null` | The JSON-LD types to generate, such as `Article`, `BlogPosting`, `TechArticle`, `WebPage`, `HowTo`, `BreadcrumbList` or `CollectionPage`. When empty, the configured `DefaultSchemaTypes` apply, then the front matter `type`, then `FallbackSchemaType`, then `WebPage`. On a list page only `CollectionPage` and `BreadcrumbList` write a schema of their own, and an article type names the type of the listed items. The first type also sets `og:type` when the front matter has no `og_type`. `FAQ` generates no data: use `EnableGeneratedFaq`. A single type gets a `BreadcrumbList` added when `GenerateMultipleSchemas` is on. |
| `AllowAiDiscovery` | `bool` | `true` | Whether the crawler opt-in meta tags are written (`true`) or the opt-out tags (`false`). With `false`, the training tag says disallow whatever `AllowAiTraining` is. These meta names are not a standard; use `robots.txt` to control crawlers. When not set, the configured value applies, then `true`. |
| `AllowAiTraining` | `bool` | `true` | Whether the training tags (`ai-training` and `Google-Extended`) opt in (`true`) or out (`false`). Ignored when `AllowAiDiscovery` is `false`. When not set, the configured value applies, then `true`. |
| `AllowSearchIndexing` | `bool` | `true` | Whether search engines may index the page. `false` writes `noindex, nofollow` as `robots`, `googlebot` and `bingbot`; `true` writes the front matter `robots` value, or `index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1` when it has none. When not set, the configured value applies, then `true`. |
| `EnableGeoOptimization` | `bool` | `true` | Whether the summary meta tags are written (`entity-summary`, `content-type`, `primary-category`, `content-author`, `content-expertise-level` and `content-freshness`). They are written only when the item or list has a description. When not set, the configured value applies, then `true`. |
| `EnableAeoOptimization` | `bool` | `true` | Whether the generated answer tags are allowed. It must be on for `EnableGeneratedFaq` to have an effect. When not set, the configured value applies, then `true`. |
| `GenerateMultipleSchemas` | `bool` | `true` | Whether a `BreadcrumbList` is added when exactly one schema type is generated for an item with a path. Several types given in `SchemaTypes` are used as listed. When not set, the configured value applies, then `true`. |
| `AuthorUrl` | `string?` | `null` | A page that identifies the author, such as an About page, named as the author `url` in structured data. It is meant for a site with one author: every article and every posting in a list names it. Without one the author is named alone, because a guessed address that answers 404 is worse than none. |
| `DefaultImageUrl` | `string?` | `null` | The social image (`og:image` and `twitter:image`). A relative address becomes absolute with the site address. On a content page it is used when the item has no `og_image_url`, no `image` and no featured image. On a list page (`ContentItems`) an image passed here is the list's own image and comes first; then the first item's featured image; then the configured `SeoMetadataOptions.DefaultImageUrl`, which as a site-wide default never replaces an item's image. |
| `DefaultImageWidth` | `int?` | `null` | The width written as `og:image:width`, in pixels. When not set, the configured width applies, then `1200`. |
| `DefaultImageHeight` | `int?` | `null` | The height written as `og:image:height`, in pixels. When not set, the configured height applies, then `630`. |
| `OrganizationName` | `string?` | `null` | The publisher name in structured data. It is also the site name when no site name is set. |
| `ApplicationCategory` | `string` | `"DeveloperApplication"` | The `applicationCategory` of the `SchemaType.SoftwareApplication` schema, one of the app types search engines support, such as `DeveloperApplication` or `BusinessApplication`. |
| `AlternateLanguageUrls` | `List<string>?` | `null` | The `hreflang` links of other language versions, each entry a language code and an address joined by a vertical bar, such as `de\|https://example.com/de/page`. Malformed entries are skipped with a logged warning. |
| `PrevPageUrl` | `string?` | `null` | The address written as `rel="prev"`. Nothing is written when empty. |
| `NextPageUrl` | `string?` | `null` | The address written as `rel="next"`. Nothing is written when empty. |
| `BreadcrumbHomeText` | `string` | `"Home"` | The name of the first `BreadcrumbList` entry. When empty, the configured value applies. |
| `CustomBreadcrumbs` | `List<ValueTuple<string, string>>?` | `null` | The breadcrumb entries after the home entry, as name and address pairs, in place of the ones built from the item path. |
| `CanonicalQueryParameters` | `IReadOnlyCollection<string>?` | `null` | The query parameters kept in the canonical URL, `og:url` and structured data addresses (for example `page` for paginated lists, where each page is its own document). All other parameters and the fragment are dropped. A canonical address declared in the front matter always wins. |
| `EmitMobileMetaTags` | `bool` | `false` | Whether the viewport, theme-color and mobile web app meta tags are written. Off by default: the host document owns these, and writing them again duplicates its tags. When not set, the configured value applies, then `false`; a value set on the page wins over the configured one in both directions. |
| `EnableGeneratedFaq` | `bool` | `false` | Whether FAQ structured data and answer meta tags are built from the title and description (requires `EnableAeoOptimization`). Off by default because the question is derived, not authored. When not set, the configured value applies, then `false`; a value set on the page wins over the configured one in both directions. |
| `ReferrerPolicy` | `string?` | `null` | The referrer policy written as `<meta name="referrer">`, such as `strict-origin`. When empty, the configured `SeoMetadataOptions.ReferrerPolicy` applies; when that is empty too, no tag is written, so the site's `Referrer-Policy` response header (or the browser default, `strict-origin-when-cross-origin`) applies. A referrer meta tag replaces the header's policy for the whole document. A value that is not one of the policy tokens of the Referrer Policy specification writes nothing. |
| `FallbackSchemaType` | `SchemaType?` | `null` | The schema type of a content page when nothing else names one: `SchemaTypes`, the configured `DefaultSchemaTypes` and the front matter `type` all come first. When not set, `WebPage` applies. `ArticlePage` sets it to `SchemaType.Article`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The crawler tags controlled by `AllowAiDiscovery` and `AllowAiTraining` use meta names that are not a standard; use
`robots.txt` to control crawlers reliably.

## What is read from the item

| Output | Source, first match wins |
| --- | --- |
| `<title>` | `seo_properties.title`, then `title`; the site name follows after a vertical bar. The title is not cut |
| description | `seo_properties.description`, then `description`, cut to 160 characters |
| canonical | `seo_properties.canonical`, then the current URL without query string and fragment, on `SiteUrl` when it is set |
| robots | `AllowSearchIndexing`, then `seo_properties.robots`; an item that sets none gets `index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1` |
| `og:title`, `og:description` | `seo_properties.og_title` and `og_description`, then the title and description above; cut to 60 and 200 characters |
| `og:type` | `seo_properties.og_type`, then `article` when the first schema type is Article, BlogPosting or TechArticle, `product` for Product, `video.other` for VideoObject, `profile` for ProfilePage, otherwise `website` |
| `og:image` | `seo_properties.og_image_url`, then `seo_properties.image`, then `featured_image`, then `DefaultImageUrl`; the width, height and alt tags are written only when there is an image |
| Twitter Card | `seo_properties.twitter_card`, `twitter_title`, `twitter_description`, `twitter_image_url`, then Open Graph; the title is cut to 70 characters and the description to 200, and `twitter:image:alt` repeats the Twitter title |
| keywords | `tags` |
| `og:locale` | `seo_properties.lang`, then the item's locale, written with an underscore (`de-DE` becomes `de_DE`); `en_US` when neither is set |
| extra meta elements | `seo_properties.meta_tags`, one `meta` element per entry. Names start with a letter and hold only letters, digits and `- _ . :`; others are skipped with a logged warning, as are the names the component writes itself (`description`, `keywords`, `author`, `generator`, `language`, `robots`, `googlebot`, `bingbot`, `referrer`). Values are HTML encoded |
| `article:*` tags | Written only when `og:type` is `article`: `article:published_time`, `article:modified_time`, `article:author`, `article:section` (the first of `categories`) and one `article:tag` per tag |
| JSON-LD | `seo_properties.json_ld`, then `SchemaTypes`, then the configured `DefaultSchemaTypes`, then `seo_properties.type`, then `FallbackSchemaType` (`Article` on `ArticlePage`), then `WebPage` |

Relative image URLs become absolute with the site's base URL.

## Page types

| Type | Use it for | What it holds |
| --- | --- | --- |
| `TechArticle` | Documentation and technical guides | The Article schema with the more specific type. |
| `SoftwareApplication` | A product page about an app or a library | Name, description, image, `ApplicationCategory`, last modified date and keywords. Search engines show software rich results only with a price and a rating or review; put the full schema in `json_ld` when you have them. |
| `ProfilePage` | An author's page | The person as `mainEntity`: the `author` (or the title), description and image, with tags that are absolute links as `sameAs`. |
| `AboutPage` | The about page | A WebPage with the site's organization as `mainEntity`. |
| `ContactPage` | The contact page | A WebPage with the more specific type. |

`FAQ` generates no data by itself, and Google no longer shows FAQ or HowTo rich results.

## Behavior

- **Rendering.** It renders on the server as part of the head, with no script, and recomputes everything when its
  parameters change, so a reused instance never keeps the previous page's canonical URL or structured data.
- **List pages.** With `ContentItems`, the title is `{CollectionTitle} | {site name}` (the site name alone when there
  is no `CollectionTitle`), `og:type` is `website`, and JSON-LD holds the CollectionPage and the BreadcrumbList that
  `SchemaTypes` lists, in its order, then an article per item. A list holds case studies and documentation as well as
  posts, so an item is an `Article` unless its front matter `type` is `Article`, `BlogPosting` or `TechArticle`, or
  `SchemaTypes` names one of those three for the whole list (`SchemaTypes="[SchemaType.CollectionPage,
  SchemaType.BreadcrumbList, SchemaType.BlogPosting]"` on a blog). A property without a value (keywords of an item
  without tags, an image, an author) is left out rather than written as `null`. The list shares its own image first: a `DefaultImageUrl` the page passes as a
  parameter. Without one it shares the first item's featured image, then the site-wide `DefaultImageUrl` of
  `AddOsirionSeoMetadata` or `Osirion:Seo`, so a site-wide default never replaces an item's image. `ContentList` passes
  its `PageImageUrl`, or the image of the section's own `_index.md` when the list is shown at the section's address.
- **Breadcrumbs.** A single schema type for an item with a path gets a BreadcrumbList added when
  `GenerateMultipleSchemas` is on. Several types in `SchemaTypes` are used as listed. On a list page the
  BreadcrumbList, and the `breadcrumb` of the CollectionPage, hold the home page and the list (`CollectionTitle` at the canonical address), or `CustomBreadcrumbs`
  when the page passes them.
- **Security.** Meta values are HTML-encoded. JSON-LD is serialized with HTML-sensitive characters escaped; custom
  `json_ld` from front matter is parsed and re-serialized the same way, and ignored with a logged warning when it is
  not a JSON object or array. When custom `json_ld` is used, `SchemaTypes` generates nothing.
- **Referrer policy.** No `referrer` meta tag is written unless `ReferrerPolicy` (parameter or option) names one of
  the policies of the [Referrer Policy specification](https://w3c.github.io/webappsec-referrer-policy/#referrer-policies),
  such as `strict-origin-when-cross-origin`; another value writes nothing and logs a warning. A referrer meta tag sets
  the document's policy after the `Referrer-Policy` response header did
  ([HTML, `referrer` metadata name](https://html.spec.whatwg.org/multipage/semantics.html#meta-referrer)), so it
  replaces the header's policy. Without one the header applies, or the browser default,
  `strict-origin-when-cross-origin`.
- **Fixed values.** It also writes `generator`, `language` and `content-language`, the robots value
  again as `googlebot` and `bingbot`, and publish date, last-modified and reading time meta tags. It writes no
  `dns-prefetch` or `preconnect` hint for the site's own address: the browser is already connected to it. The Article
  schema gives the author a `url` only when `AuthorUrl` is set.
- **Crawler tags.** With `AllowAiDiscovery` on, it writes `ai-crawling`, `ai-training` and a tag per named crawler;
  with it off, the opt-out versions. The component makes no AI content declaration of its own: write
  `ai-content-declaration` (or `ai-content-detection`, `ai-content-attribution`) under `meta_tags` in the front matter, or under `AdditionalMetaTags` in `SeoMetadataOptions` for every page. These names are not a standard either.
- **Options.** `SeoMetadataOptions` fills the parameters you leave unset: `SiteName` (as `SiteNameOverride`),
  `SiteDescription`, `OrganizationName`, `SiteLogoUrl`, `DefaultImageUrl`, `DefaultImageWidth`, `DefaultImageHeight`,
  `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `AuthorUrl`, `ReferrerPolicy` and
  `DefaultSchemaTypes`. The switches `AllowSearchIndexing`, `AllowAiDiscovery`, `AllowAiTraining`, `EmitMobileMetaTags`, `EnableGeneratedFaq`,
  `EnableGeoOptimization`, `EnableAeoOptimization` and `GenerateMultipleSchemas` come from the options unless the
  page sets them as parameters, `true` or `false`. `BreadcrumbHomeText` applies when the parameter is empty. `DefaultImageWidth` and
  `DefaultImageHeight` are nullable: the option applies when the parameter is not set, and an explicit `1200` or `630` stands.
- **Public address.** `SiteUrl` (for example `https://example.com`) makes the canonical URL, `og:url` and the
  addresses in structured data use the site's address with the page's path, whatever host served the request: a
  proxy, a preview host or a second domain. Only an absolute `http` or `https` address is used; otherwise they follow
  the request. Set it on a site published as static files: the export renders from a local address, and without
  `SiteUrl` every address in the head and the JSON-LD names that local host. `OsirionFeedOptions.SiteUrl` covers the
  feeds and the sitemap only.

## Related

- [ContentList](/components/content-list): renders this for list pages
- [Feeds and sitemap](/components/feeds-and-sitemap): `sitemap.xml`, RSS and Atom
- [ContentView](/components/content-view): the article body to pair it with
- [Blazor SEO checklist for SSR and SSG](/blog/blazor-seo-ssr-ssg-checklist)
