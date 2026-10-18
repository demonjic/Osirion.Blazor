---
order: 4
title: ContentPage
permalink: /components/content-page
description: "Load a content item by URL and lay it out with breadcrumbs, translation links, previous and next links, a sidebar, share links and related items."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Components
- CMS Web Components
- Core Components
tags:
- content-page
- cms
- article
- related-content
- static-ssr
slug: content-page
lang: en
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/content-page
custom_fields:
  group: cms-pages
seo_properties:
  title: 'ContentPage: an article page by URL for Blazor'
  description: 'ContentPage loads a content item by URL and lays out breadcrumbs, the article, previous and next links, a sidebar, share links and related items.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/components/content-page'
  lang: en
  robots: index, follow
  og_title: 'ContentPage - Osirion.Blazor'
  og_description: 'A whole article page loaded by URL: breadcrumbs, sidebar, share links and related items, rendered without script.'
  og_image_url: '/assets/images/blazor-cover.png'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ContentPage - Osirion.Blazor'
  twitter_description: 'A whole article page loaded by URL: breadcrumbs, sidebar, share links and related items.'
  twitter_image_url: '/assets/images/blazor-cover.png'
  meta_tags: {}
---

# ContentPage

`ContentPage` loads one content item by its URL and lays out a whole article: breadcrumbs, the item through
[LocalizedContentView](/components/localized-content-view), previous and next links, a sidebar of
categories and tags, optional share links and optional related items. Use it for a quick article page without
assembling those parts yourself.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/ContentPage.razor).

```demo
content-page
```

The demo loads this site's own page for the 1.5 release notes by its URL (`blog/osirion-blazor-v1-5-release-notes`)
and shows it in a scrolling frame. The formatters point the category, tag and related links at this site's routes.
This documentation page has its own `h1`, so the demo sets `HeadingLevel="2"` and the article title is an `h2`.

## Usage

```razor
@page "/blog/{Slug}"
@using Osirion.Blazor.Cms.Web.Components

<ContentPage Path="@($"blog/{Slug}")" />

@code {
    [Parameter] public string? Slug { get; set; }
}
```

`Path` is the item's `Url` exactly as the provider stores it: no leading slash, no file extension, and for localized
content no locale folder (`blog/my-post`). The match is exact, including case. Without a `Path`, the page shows the
item you pass in `ContentItem` and loads no neighbors, related items or default share links, so pass `PreviousItem`,
`NextItem` and `RelatedItems` yourself in that case.

`ContentPage` writes nothing to the page head. To add SEO metadata, keep the loaded item and render a
[SeoMetadataRenderer](/components/seo-metadata-renderer) for it:

```razor
@page "/blog/{Slug}"
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Web.Components

@if (current is not null)
{
    <HeadContent>
        <SeoMetadataRenderer Content="@current" SchemaTypes="@(new[] { SchemaType.BlogPosting })" />
    </HeadContent>
}

<ContentPage Path="@($"blog/{Slug}")"
             ShowRelatedContent
             ShowShareLinks
             TagUrlFormatter="@(tag => $"/tag/{Uri.EscapeDataString(tag)}")"
             OnContentLoaded="@((ContentItem item) => current = item)" />

@code {
    [Parameter] public string? Slug { get; set; }

    private ContentItem? current;
}
```

## Parameters

<!-- osirion:parameters ContentPage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Path` | `string?` | `null` | The URL of the item to load from the default content provider, as the provider stores it: no leading slash, no file extension and, for localized content, no locale folder (`blog/my-post`). The match is exact, including case. `IgnorePathSegment` and `PathReplacePattern` are applied first. Without a path, the page shows the item in `ContentItem`, or the not-found state when that is null too. |
| `ContentItem` | `ContentItem?` | `null` | The item shown. When `Path` loads, the page sets this property and replaces a value you passed; it is not two-way bound, so use `OnContentLoaded` to receive the item. Without a path the page shows the item you pass and loads no neighbors, related items or default share links. |
| `PreviousItem` | `ContentItem?` | `null` | The item the previous link points to. When `ShowNavigationLinks` is on and this or `NextItem` is null, the page looks both up with a query for the item's directory and locale, newest first, and replaces your values with what it finds. |
| `NextItem` | `ContentItem?` | `null` | The item the next link points to. See `PreviousItem` for how a missing neighbor is looked up. |
| `RelatedItems` | `IReadOnlyList<ContentItem>?` | `null` | The items shown in the related section. When `ShowRelatedContent` is on and this is null or empty, the page picks up to three items of the same locale that share a tag or a category with the item, most shared first, and sets this property. |
| `LoadingText` | `string` | `"Loading content..."` | The text of the loading state, shown while the page waits for the item. It is not part of a statically rendered response, which is sent once the item has loaded. |
| `NotFoundTitle` | `string` | `"Content Not Found"` | The heading of the not-found state, shown when no item matches or the provider fails. |
| `NotFoundText` | `string` | `"The requested content could not be found."` | The message of the not-found state. |
| `ShowBackLink` | `bool` | `true` | Whether the not-found state has a link back, to `BackLinkUrl` with the text `BackLinkText`. It is not shown when the URL is empty. |
| `BackLinkUrl` | `string` | `"/"` | The address of the back link in the not-found state. |
| `BackLinkText` | `string` | `"Back to Home"` | The text of the back link in the not-found state. |
| `ShowBreadcrumbs` | `bool` | `true` | Whether to render `ContentBreadcrumbs` for the item above the article. |
| `ShowBreadcrumbHome` | `bool` | `true` | Whether the breadcrumbs start with a home link. |
| `BreadcrumbHomeUrl` | `string` | `"/"` | The address of the breadcrumbs' home link. |
| `BreadcrumbHomeText` | `string` | `"Home"` | The text of the breadcrumbs' home link. |
| `ShowMetadata` | `bool` | `true` | Has no effect: the markup does not read it. The header with the author, date and reading time always renders. |
| `FeaturedImageCaption` | `string?` | `null` | Has no effect: the markup does not read it. |
| `ShowFooter` | `bool` | `true` | Has no effect: the markup does not read it. |
| `ShowCategories` | `bool` | `true` | Whether the sidebar lists categories. The list renders only when the item has categories, and it holds every category of the site, not only the item's, without counts. |
| `CategoriesTitle` | `string` | `"Categories:"` | The heading of the sidebar's category list. |
| `ShowTags` | `bool` | `true` | Whether the sidebar shows a tag cloud. It renders only when the item has tags, and it holds every tag of the site, not only the item's, without counts. |
| `TagsTitle` | `string` | `"Tags:"` | The heading of the sidebar's tag cloud. |
| `ShowNavigationLinks` | `bool` | `true` | Whether to show the previous and next links under the article. Missing neighbors are looked up by the page, see `PreviousItem`. |
| `PreviousItemLabel` | `string` | `"Previous"` | Has no effect: the markup does not read it. The previous link is labeled `Previous`. |
| `NextItemLabel` | `string` | `"Next"` | Has no effect: the markup does not read it. The next link is labeled `Next`. |
| `ShowShareLinks` | `bool` | `false` | Whether to show a share section. Without `ShareLinks` it offers Twitter, Facebook and LinkedIn links that carry the page's absolute address without query string or fragment; those defaults are created only when the page loads by `Path`. |
| `ShareLinksTitle` | `string` | `"Share:"` | The heading of the share section. |
| `ShareLinks` | `List<ShareLink>` | `[]` | The share links to show, each with `Name` (also the suffix of its CSS class), `Label`, `Url` and `Icon`. `Icon` is written as raw HTML without sanitizing, so never fill it from user or content input. The links open in a new tab with `rel="noopener noreferrer"`. |
| `FooterContent` | `string?` | `null` | HTML written after the article and the share links as raw markup, without sanitizing. Use trusted markup only. Nothing renders when it is null or white space. |
| `ShowRelatedContent` | `bool` | `false` | Whether to show related items, from `RelatedItems` or, when that is empty, found by the page. |
| `RelatedContentTitle` | `string` | `"Related Content"` | The heading of the related section. |
| `SchemaType` | `string` | `"Article"` | The schema.org type written into the article's `itemtype` attribute (`https://schema.org/Article` by default). ContentPage writes no head metadata; render a `SeoMetadataRenderer` for that. |
| `HeadingLevel` | `int` | `1` | The level of the heading that holds the article title, `1` to `6` (`h1` to `h6`), passed to the `LocalizedContentView` inside. It defaults to `1`, the page's own heading; set `2` for a page embedded in another that has its own `h1`, so that page keeps a single one. A value outside the range is moved to the nearest level. The title keeps its look at every level. |
| `DirectoryUrlFormatter` | `Func<DirectoryItem, string>?` | `null` | Has no effect: the page renders no directory links. |
| `CategoryUrlFormatter` | `Func<string, string>?` | `null` | Builds a category link from the category name, for the links under the article and the sidebar list. Without it the links under the article are `/{locale}/category/{slug}` and the sidebar list uses its own default. |
| `TagUrlFormatter` | `Func<string, string>?` | `null` | Builds a tag link from the tag name, for the links under the article and the tag cloud. Without it the links under the article are `/{locale}/tag/{slug}` and the tag cloud uses its own default. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the links of the previous, next and related items from the item. Without it a previous or next link is `/{item.Path}` and a related link is the item's source path with `IgnorePathSegment` removed, `PathReplacePattern` applied and the `.md` or `.markdown` extension dropped. |
| `IgnorePathSegment` | `string?` | `null` | A path segment that the public address leaves out. It is put in front of `Path` (as `segment/path`) when the path does not already contain it, before the lookup, and it is removed from the default links of related items. |
| `PathReplacePattern` | `string?` | `null` | A regular expression and its replacement, separated by one vertical bar (`pattern\|replacement`), applied to `Path` before the lookup and to the default links of related items. A value without exactly one bar is ignored, so the pattern cannot contain a bar, and a pattern that runs longer than 250 milliseconds is ignored too. |
| `OnContentLoaded` | `EventCallback<ContentItem>` | - | Raised with the item after the page loaded it by `Path`. It is not raised when nothing is found or when you passed `ContentItem` yourself. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`ShareLinks` takes `ShareLink` objects (namespace `Osirion.Blazor.Cms.Domain.Models`) with `Name`, `Label`, `Url` and
`Icon`, all strings that default to empty. The `Theme` and `Style` rows come from the base class. The page does not write them, or unmatched attributes, so only `Class` reaches the root element.

## Behavior

- **Rendering.** Static SSR renders the finished article on the server; nothing needs script. A new `Path` clears the
  neighbors, related items and default share links the component loaded for the previous one.
- **Body.** The item is rendered by `LocalizedContentView`, so the body is sanitized, a header shows the title,
  author, date, reading time and featured image, and translation links appear when the item has translations.
- **Sidebar.** `CategoriesList` and `TagCloud` list every category and tag of the site, not only the item's, without
  counts.
- **Share links.** They open in a new tab with `rel="noopener noreferrer"`. The default links carry the page's
  absolute URL without query string or fragment.
- **Related items.** Each shows its featured image (when it has one, with the `featured_image_alt` custom field of its front matter as alt text, or an empty one), its title as a
  link and its date, written in the language of that item's locale. The default link is the item's source path with `.md` removed; set `ContentUrlFormatter` when
  your routes differ.
- **Security.** `FooterContent` and each `ShareLink.Icon` are written as raw HTML without sanitizing. Never put
  user-supplied or content-supplied text in them.
- **Not found.** When no item matches, or the provider fails (the error is logged), the page shows the not-found
  state: a heading, a message and a back link. Without a `Path` and without a `ContentItem` it shows the same state.
  The page also signals 404, as `ArticlePage` does: `NavigationManager.NotFound()` on .NET 10, the status code of the
  request on .NET 9, while the response has not started. It has no effect in an interactive render, where the not-found
  state is all that can be shown.

## Styling

The root element has the class `osirion-content-page-container`; the article is `osirion-content-page` with
`osirion-content-main`, `osirion-content-primary` and `osirion-content-sidebar`. The other parts use
`osirion-content-share` (`osirion-content-share-link`, plus `osirion-content-share-{name}` per link),
`osirion-content-custom-footer`, `osirion-content-related` with `osirion-content-related-grid`, and
`osirion-content-not-found`. Colors and spacing come from theme tokens such as `--osirion-text-primary`,
`--osirion-text-secondary` and `--osirion-border-color`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [LocalizedContentView](/components/localized-content-view): the article inside the page
- [ContentView](/components/content-view): a single item without the page layout
- [ContentBreadcrumbs](/components/content-breadcrumbs): the breadcrumb trail
- [SeoMetadataRenderer](/components/seo-metadata-renderer): head metadata for the item
