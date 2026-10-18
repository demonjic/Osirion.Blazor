---
order: 1
title: ContentList
permalink: /components/content-list
description: "List content items from the default provider as a grid of cards, filtered by directory, category, tag, author or search, with query-string pagination."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-content-list.jpg'
categories:
- Components
- CMS Web Components
- Core Components
tags:
- content-list
- cms
- pagination
- filtering
- blog
slug: content-list
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/content-list
custom_fields:
  group: cms-pages
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/components-content-list.jpg'
---

# ContentList

`ContentList` queries the default content provider and renders the matching items as a grid of cards, with optional
pagination and SEO metadata for the list page. Use it for blog indexes and for category, tag, author and search
result pages.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<ContentList Directory="blog" />
```

A paginated category page, the way the example site builds one:

```razor
@page "/category/{CategorySlug}"
@using Osirion.Blazor.Cms.Web.Components

<ContentList Directory="blog"
             Category="@CategorySlug"
             ShowPagination
             NotFoundWhenFilterMatchesNothing
             ItemsPerPage="12" />

@code {
    [Parameter] public string? CategorySlug { get; set; }
}
```

A search page passes the `q` query-string value through:

```razor
@page "/search"
@using Osirion.Blazor.Cms.Web.Components

<ContentList SearchQuery="@Query" ShowPagination ItemsPerPage="12"
             NoContentText="No results found for your search query." />

@code {
    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }
}
```

## Parameters

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Directory` | `string?` | `null` | Items directly in this content folder, for example `blog`. It is compared with the directory's name, which the `title` of the folder's `_index.md` replaces when present. |
| `Category` | `string?` | `null` | Items with a category that contains this text, ignoring case. Hyphens are read as spaces, so a URL slug works. |
| `Tag` | `string?` | `null` | Items with this tag, ignoring case. |
| `Author` | `string?` | `null` | Items whose `author` front matter matches the whole name, ignoring case. Available from 4.1. |
| `NotFoundWhenFilterMatchesNothing` | `bool` | `false` | When `Category`, `Tag` or `Author` is set and no item matches, the response is HTTP 404, so an address such as `/tag/no-such-tag` is not indexed as an empty page. On .NET 10 the router renders its not-found page, if it has one. Without a filter, or when loading fails, an empty list stays a normal page. `ContentListPage` turns it on. |
| `SearchQuery` | `string?` | `null` | Search terms, matched against title, description, body, categories and tags. Also bound to the `q` query-string parameter. |
| `Locale` | `string?` | `null` | Items in this locale only. |
| `DirectoryId` | `string?` | `null` | Items in the directory with this id. |
| `OnlyFeatured` | `bool?` | `null` | `true` lists only items with `is_featured: true`. |
| `FeaturedCount` | `int?` | `null` | Lists only featured items, at most this many, without pagination. |
| `SortBy` | `SortField` | `Date` | `Date`, `Title`, `Author`, `LastModified`, `Order`, `Created`, `PublishDate`, `Slug` or `ReadTime` (namespace `Osirion.Blazor.Cms.Domain.Enums`). |
| `SortDirection` | `SortDirection` | `Descending` | Sort direction. |
| `ShowPagination` | `bool` | `false` | Splits the list into pages and renders page links. |
| `ItemsPerPage` | `int` | `10` | Items per page; values below 1 count as 1. |
| `PageChanged` | `EventCallback<int>` | none | In interactive render modes, page links call it instead of navigating. Under static SSR the links always navigate. |
| `PaginationUrlFormatter` | `Func<int, string>?` | `null` | Builds page link URLs; by default the current URL with `page` set. |
| `LoadingText` | `string` | `"Loading content..."` | Shown while loading, in interactive render modes only. |
| `NoContentText` | `string` | `"No content available."` | Shown when nothing matches. |
| `EnableSeoMetadata` | `bool` | `true` | Renders `SeoMetadataRenderer` for the list into the page head. |
| `PageTitle` | `string?` | `null` | Title for the head and the CollectionPage data. |
| `PageDescription` | `string?` | `null` | Description for the head. |
| `WebsiteName` | `string?` | `null` | Site name for the title, in place of the configured one. |
| `SchemaTypes` | `SchemaType[]?` | `CollectionPage`, `BreadcrumbList` | Structured data types for the list page. |
| `Class` | `string?` | `null` | Extra CSS classes on the root element. |

`ContentUrlFormatter`, `ReadMoreText`, `OnItemSelected`, `IgnorePathSegment` and `PathReplacePattern` are declared
but have no effect in the current markup: each card links to the item's own URL. `Style`, `Theme` and unmatched
attributes from the base class are not written to the markup either.

## Behavior

- **Rendering.** The list renders on the server under static SSR and needs no script. The loading text appears only
  in interactive render modes. With the built-in providers, `Category` or `Tag` replaces the `Directory` filter
  instead of narrowing it.
- **Pagination.** The current page comes from the `page` query-string parameter and is clamped to the valid range,
  so `?page=0` shows the first page and an oversized number shows the last. Page links are ordinary links that keep
  every other query parameter (for example `q`); the link to page 1 drops `page`. The list is fetched once and the
  page is sliced from it.
- **Cards.** Each card shows the featured image (lazy loaded) or a placeholder, the title linked to the item, the
  description (or an excerpt of about 160 characters of the body), author, date, reading time and categories.
  Category links default to `/category/{slug}`.
- **SEO.** With `EnableSeoMetadata` and at least one item, the head gets a title (from `PageTitle`, otherwise
  "Category: {Category}", "Tag: {Tag}", the directory name in title case or "Blog", with " - Page N" after the first
  page), a description, a canonical URL that keeps only `page` (from page 2 on),
  `rel="prev"` and `rel="next"` links, CollectionPage JSON-LD and a BlogPosting entry per item. Turn it off when the
  page renders its own `SeoMetadataRenderer`.
- **Accessibility.** The previous and next page links are icons without a text label.
- **Errors.** A provider failure is logged and the empty state is shown.

## Styling

The root element has the class `osirion-content-list`. Pagination uses `osirion-content-pagination`,
`osirion-pagination-link` (with `osirion-active` on the current page) and `osirion-pagination-ellipsis`; the empty
state uses `osirion-no-content`. The cards are rendered by `OsirionContentListSection` (`osirion-featured-posts`,
`osirion-featured-post`). Colors, borders and spacing come from theme tokens such as `--osirion-border-color`,
`--osirion-action-primary`, `--osirion-action-primary-text` and `--osirion-text-secondary`, so a framework adapter
restyles the list with the rest of the site.

## Related

- [ContentView](/components/content-view): one item with its body
- [SeoMetadataRenderer](/components/seo-metadata-renderer): what the list writes to the head
- [CategoriesList](/components/categories-list): links to category pages
- [CMS Web core components](/components/cms-pages)
