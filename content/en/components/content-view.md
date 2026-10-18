---
order: 2
title: ContentView
permalink: /components/content-view
description: "Render one content item as an article: title, author, date, reading time, featured image, sanitized body, categories, tags and previous and next links."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Components
- CMS Web Components
- Core Components
tags:
- content-view
- cms
- article
- sanitization
- static-ssr
slug: content-view
lang: en
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/content-view
custom_fields:
  group: cms-pages
seo_properties:
  title: 'ContentView: one content item as an article for Blazor'
  description: 'ContentView renders one CMS content item as an article with a header, a sanitized body, categories, tags and previous and next links.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/components/content-view'
  lang: en
  robots: index, follow
  og_title: 'ContentView - Osirion.Blazor'
  og_description: 'One content item as an article: header, sanitized body, categories, tags and previous and next links, rendered without script.'
  og_image_url: '/assets/images/blazor-cover.png'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ContentView - Osirion.Blazor'
  twitter_description: 'One content item as an article: header, sanitized body, categories, tags and previous and next links.'
  twitter_image_url: '/assets/images/blazor-cover.png'
  meta_tags: {}
---

# ContentView

`ContentView` renders one content item as an article: a header with the title, author, date, reading time, tags and
featured image, the sanitized HTML body, and a footer with categories, tags and optional previous and next links. Use
it when you build your own page around a single item.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/ContentView.razor).

```demo
content-view
```

The demo loads this site's own page for the 1.5 release notes and shows it in a scrolling frame. The neighbors come
from the same folder, newest first, and the formatters point the links at this site's routes. `ContentView` writes
the title as an `h1` unless you set `HeadingLevel`; the demo sets it to `2`, because this documentation page has
its own `h1`.

## Usage

Pass an item you already loaded:

```razor
@using Osirion.Blazor.Cms.Web.Components

<ContentView Item="@item" />
```

Or let the component load it by the item's source path (`ContentItem.Path`, as the provider reports it):

```razor
<ContentView Path="blog/getting-started.md"
             ShowNavigationLinks="true"
             ContentUrlFormatter="@(item => "/" + item.Url)"
             TagUrlFormatter="@(tag => $"/blog/tag/{Uri.EscapeDataString(tag)}")"
             AuthorUrlFormatter="@(name => $"/authors/{Uri.EscapeDataString(name.ToLowerInvariant().Replace(' ', '-'))}")" />
```

## Parameters

<!-- osirion:parameters ContentView -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Path` | `string` | `""` | The source path of the item to load from the default provider (`ContentItem.Path`, for example `blog/getting-started.md`) while `Item` is not set. A null or white-space path loads nothing. |
| `LoadingText` | `string` | `"Loading content..."` | The text shown while the item loads, in interactive render modes only. A statically rendered response is sent once the item has loaded. |
| `NotFoundText` | `string` | `"Content not found."` | The message of the not-found block, shown under a fixed `Content Not Found` heading and above a link back to `/`. The block appears when no item is found, when loading fails (the error is logged) and when neither `Item` nor `Path` is set. |
| `CategoryUrlFormatter` | `Func<string, string>?` | `null` | Builds a category link from the category name. Without it the link is `/category/{slug}`. |
| `TagUrlFormatter` | `Func<string, string>?` | `null` | Builds a tag link from the tag name. Without it the link is `/tag/{slug}`. |
| `AuthorUrlFormatter` | `Func<string, string>?` | `null` | How the author's page URL is built from the author's name. When set, the author's name links there with `rel="author"`. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the links of the previous and next items from the item. Without it a link is `/{item.Path}`, the source path, which is usually not the public address, so set it whenever navigation links are shown. |
| `Item` | `ContentItem?` | `null` | The item to render: header, sanitized body, categories, tags and navigation. An item the parent sets is never replaced by loading by `Path`. |
| `PreviousItem` | `ContentItem?` | `null` | The item the previous link points to. Loading by `Path` with `ShowNavigationLinks` on fills it, and clears what it filled when the path changes; a value you set is left alone until then. |
| `NextItem` | `ContentItem?` | `null` | The item the next link points to. See `PreviousItem`. |
| `ShowNavigationLinks` | `bool` | `false` | Whether the footer shows previous and next links. When the component loads by `Path` it also looks the neighbors up with a directory query built from the folder part of the path, newest first. For an item you pass in `Item`, set `PreviousItem` and `NextItem` yourself. |
| `HeadingLevel` | `int` | `1` | The level of the heading that holds the title, `1` to `6` (`h1` to `h6`). It defaults to `1`, the page's own heading; set `2` for a view embedded in a page that has its own `h1`, so the page keeps a single one. A value outside the range is moved to the nearest level. The title keeps its look at every level. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`AuthorUrlFormatter` is available from 4.1. The `Theme` and `Style` rows come from the base class. `ContentView` does not write them, or unmatched attributes, so only `Class` reaches the root element.

The default previous and next link is the item's source path, which usually differs from its public URL. When you
show navigation links, set `ContentUrlFormatter`, for example `item => "/" + item.Url`.

## Behavior

- **Rendering.** Everything renders on the server under static SSR; there is no script. The loading text appears only
  in interactive render modes. When `Path` changes, the previous item and the neighbors the component loaded are
  cleared before the new one loads, so an old article never shows under a new address.
- **Not found.** Without an item the component renders a "Content Not Found" heading, `NotFoundText` and a link
  back to `/`. The heading and the link text are fixed. The component also signals 404, as `ArticlePage` does:
  `NavigationManager.NotFound()` on .NET 10, the status code of the request on .NET 9, while the response has not
  started. A view you embed in a page that must stay at 200 without its item should not be rendered without one.
- **Security.** The body is written as HTML after it passes through the registered `IHtmlContentSanitizer` (the
  default allow-list when none is registered), even when the item comes from a custom provider or from your code.
  Script, event handler attributes, `javascript:`, `vbscript:` and `data:` URLs, forms, iframes, inline SVG and
  inline styles (apart from table cell alignment) are removed. The result is cached until the content changes. To allow a custom element, register it with
  `AddOsirionHtmlSanitizer(options => options.AllowElement("chart", "type", "data"))`. Title, author and tags are
  encoded as text.
- **Headings.** The title is an `h1` by default, the page's own heading. Set `HeadingLevel` (1 to 6) to write another
  level, `2` for a view embedded in a page that has its own `h1`; the title keeps its look. The body's own leading
  `h1` (the `# Title` line of the Markdown) is left out, so the title appears once; a top-level heading further down is
  kept.
- **Images.** The featured image takes its `alt` text from the `featured_image_alt` entry of the front matter's `custom_fields`, for example `custom_fields:` with `featured_image_alt: A bridge at dusk` below it. The front matter model has no property of its own for it, and a `featured_image_alt` key outside `custom_fields` is ignored. Without the entry the `alt` is empty, which marks the image as decorative, instead of repeating the title that the heading beside it already shows. From 4.0 the image loads eagerly with
  `fetchpriority="high"` and async decoding, because it is usually the largest element on an article page.
- **Dates.** The date is `DateCreated` (the `date` front matter), formatted as `MMM d, yyyy` in the language of the
  item's locale, or in the current culture when the locale is missing or not a culture name, inside a `<time>` element.
- **SEO.** `ContentView` writes nothing to the page head. Render a
  [SeoMetadataRenderer](/components/seo-metadata-renderer) inside `<HeadContent>` next to it.

## Styling

The root element has the class `osirion-content-view`; the article inside is `osirion-content-article`. The header
uses `osirion-content-header`, `osirion-content-title` and `osirion-content-meta` (items `osirion-content-author`,
`osirion-content-date`, `osirion-content-read-time`), the body `osirion-content-body`, and the footer
`osirion-content-footer`, `osirion-content-category`, `osirion-content-tag` and `osirion-content-navigation`
(`osirion-content-previous`, `osirion-content-next`). Tags are rendered twice, in the header
(`osirion-content-tags-inline`) and in the footer (`osirion-content-tags-mobile`), and the library styles neither
class, so both lists show at every width; hide one in your own CSS if you want only one.

The body is markup the component writes itself, outside `OsirionHtmlRenderer`, so the library gives its headings,
paragraphs, lists and tables no typography: they take the style of your CSS framework or your own CSS (the rules
`osirion-content-body h2`, `p` and the like that older builds carried never matched and are gone). Two rules do reach
it, and keep a wide image or code block inside the column: `img` is at most as wide as the body, and `pre` scrolls
sideways. For a body with the library's typography, render it with [ContentRenderer](/components/content-renderer).
Colors and spacing
come from theme tokens such as `--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-border-color` and
`--osirion-background-secondary`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [LocalizedContentView](/components/localized-content-view): the same with translation links
- [ContentRenderer](/components/content-renderer): the body only, with code highlighting
- [ContentPage](/components/content-page): a full page layout around an item
- [ContentList](/components/content-list): lists, including `Author` for author pages
- [OsirionAuthorCard](/components/author-card): the author introduction on an author page
