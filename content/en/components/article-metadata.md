---
id: 'osirion-article-metadata'
order: 1
layout: "component"
title: OsirionArticleMetdata
permalink: /components/article-metadata
description: "OsirionArticleMetdata shows an article's author, publish date and read time in one line with icons, and renders nothing when all three are empty."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Components
- Core Components
- Content
tags:
- metadata
- article
- author
- date
- read-time
- blog
slug: article-metadata
lang: en
published: true
redirect_from:
- /docs/components/core/article-metadata
- /docs/components/core/content/article-metadata
custom_fields:
  group: content
seo_properties:
  title: 'OsirionArticleMetdata: author, date and read time for Blazor'
  description: 'OsirionArticleMetdata shows an article''s author, publish date and read time in one line with icons, and renders nothing when all three are empty.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/components/article-metadata'
  lang: en
  robots: index, follow
  og_title: 'OsirionArticleMetdata - Osirion.Blazor'
  og_description: 'Author, publish date and read time on one line, each with an icon. Renders without script.'
  og_image_url: '/assets/images/blazor-cover.png'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionArticleMetdata - Osirion.Blazor'
  twitter_description: 'Author, publish date and read time on one line, each with an icon. Renders without script.'
  twitter_image_url: '/assets/images/blazor-cover.png'
  meta_tags: {}
---

# OsirionArticleMetdata

`OsirionArticleMetdata` shows an article's author, publish date and read time on one line, each with a small icon.
Use it under an article title or in a list of posts. It renders only the fields you set, and nothing at all when all
three are empty.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/OsirionArticleMetdata.razor).

```demo
article-metadata
```

> [!NOTE]
> The component name is spelled `OsirionArticleMetdata` (without the second "a") in the library. Use that spelling in
> Razor.

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionArticleMetdata Author="Dejan Demonjić" PublishDate="@publishDate" ReadTime="5" />

@code {
    private readonly DateTime publishDate = new(2026, 10, 11);
}
```

This renders the author, the date in the current culture's short date format, and "5 min".

A realistic case is a post header fed from your own model, with a long date format:

```razor
<h1>@post.Title</h1>
<OsirionArticleMetdata Author="@post.Author"
                       PublishDate="@post.PublishedOn"
                       ReadTime="@post.ReadMinutes.ToString(CultureInfo.InvariantCulture)"
                       DateFormat="MMMM d, yyyy" />
```

`post` stands for your own type here; `PublishedOn` is a `DateTime?` and `ReadMinutes` an `int`. Add
`@using System.Globalization` for `CultureInfo`.

## Parameters

<!-- osirion:parameters OsirionArticleMetdata -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Author` | `string?` | `null` | The author's name, shown with a person icon. Hidden when empty or white space. |
| `PublishDate` | `DateTime?` | `null` | The publication date, shown with a calendar icon in the format of `DateFormat`. Hidden when `null`. |
| `DateFormat` | `string?` | `"d"` | The .NET date format string for `PublishDate`, applied with `Culture`. |
| `Culture` | `CultureInfo?` | `null` | The culture `PublishDate` is written in, normally the language of the article. When `null`, the current culture is used. |
| `ReadTime` | `string?` | `null` | The read time in minutes, shown with a clock icon. The component appends " min", so pass the number only (`5`, not `5 min`). Hidden when empty or white space. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class`, `Style`, `Theme` and unmatched attributes (for example `id` or `data-*`) are written on the root `<div>`.
The component's own styles are scoped (a class plus an attribute), so a single class selector loses to them; a parent
class matches their specificity, and your rule wins when your stylesheet loads after the library's.

## Behavior

- Renders plain HTML under static SSR and needs no script.
- The date is formatted on the server with `Culture`, or with the request's current culture when it is null. The CMS
  components pass the culture of the content's locale, so dates follow the content's language.
- Values are written as text, so Razor encodes them; HTML in `Author` shows as text.
- The date is a `<time>` element whose `datetime` attribute holds the date as `yyyy-MM-dd`.
- The icons are inline SVG without a text alternative. The text next to each icon carries the meaning.
- `HeroSection` uses this component when its `ShowMetadata` parameter is `true`, passing its own `Author`,
  `PublishDate`, `Culture` and `ReadTime`.

## Styling

The markup is a flex row:

| Class | Element |
| --- | --- |
| `osirion-article-metadata` | The wrapping `<div>` (wraps onto several lines when needed, 80% opacity) |
| `osirion-article-metadata-author` | The author item |
| `osirion-article-metadata-date` | The date item |
| `osirion-article-metadata-read-time` | The read time item |
| `osirion-article-metadata-icon` | The three icons (no shrinking, 70% opacity) |

The author's icon also keeps the earlier class `osirion-article-meta-icon`, which no stylesheet defines, so rules a site
wrote for it still match. All three icons are 16 pixels square.

The styles are scoped to the component and use the theme tokens `--osirion-spacing-2`, `--osirion-spacing-4`,
`--osirion-spacing-6` and `--osirion-font-size-sm`, so a theme or framework adapter that sets them changes the spacing
and size. Icons draw with `currentColor` and follow the text color.

## Related

- [Hero section](/components/hero-section)
- [Content components](/components/content)
