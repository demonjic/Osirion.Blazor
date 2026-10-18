---
id: 'cms-web-series-navigation'
order: 7
title: ContentSeriesNavigation
permalink: /components/series-navigation
description: "ContentSeriesNavigation shows an item's place in a series, lists the parts and links the previous and next part, from series front matter."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-series-navigation.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- series
- front-matter
- previous-next
published: true
slug: series-navigation
lang: en
redirect_from:
- /docs/components/cms-web/navigation/series-navigation
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'ContentSeriesNavigation: series links for Blazor'
  description: 'Link the parts of a multi-part article with series and series_order front matter: Part 2 of 5, a list of parts and previous and next links.'
  image: 'https://media.getosirion.com/og/components-series-navigation.jpg'
  canonical: 'https://getosirion.com/components/series-navigation'
  lang: en
  robots: index, follow
  og_title: 'ContentSeriesNavigation - Osirion.Blazor'
  og_description: 'Shows where an item stands in a series, lists the parts and links the previous and next part.'
  og_image_url: 'https://media.getosirion.com/og/components-series-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ContentSeriesNavigation - Osirion.Blazor'
  twitter_description: 'Shows where an item stands in a series, lists the parts and links the previous and next part.'
  twitter_image_url: 'https://media.getosirion.com/og/components-series-navigation.jpg'
  meta_tags: {}
---

# ContentSeriesNavigation

`ContentSeriesNavigation` shows where an item stands in a series ("Part 2 of 5"), lists every part and links the
previous and next part. Use it on multi-part tutorials and article series. The series comes from front matter, so
adding a part is a matter of writing its Markdown file.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/ContentSeriesNavigation.razor).

This page has no live demo. The component reads the parts of a series from the content provider, and no content on
this site has `series` front matter, so it would render nothing.

## Usage

Give every part the same `series` name and number it with `series_order`:

```yaml
---
title: "Deploy the blog"
series: "Build a blog with Osirion"
series_order: 3
---
```

Then render the component with the item being shown:

```razor
@using Osirion.Blazor.Cms.Web.Components

<ContentSeriesNavigation Item="@Item" />
```

With three published parts of that series, on the third part it renders like this (abridged):

```html
<nav class="osirion-series-nav" aria-label="Build a blog with Osirion: Part 3 of 3">
  <p class="osirion-series-nav-heading">
    <span class="osirion-series-nav-position">Part 3 of 3</span>
    <span class="osirion-series-nav-name">Build a blog with Osirion</span>
  </p>
  <ol class="osirion-series-nav-parts"> ...one li per part, the current one with aria-current="page"... </ol>
  <div class="osirion-series-nav-pager">
    <a class="osirion-series-nav-previous" href="/blog/part-two" rel="prev">...</a>
  </div>
</nav>
```

A tutorial page that shows the position and part list at the top and only the previous and next links at the
bottom, with its own wording:

```razor
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Web.Components

<ContentSeriesNavigation Item="@Item" PositionFormat="Lesson {0} of {1}" />

@* the article body *@

<ContentSeriesNavigation Item="@Item"
                         ShowPartList="false"
                         PositionFormat="Lesson {0} of {1}"
                         PreviousText="Previous lesson"
                         NextText="Next lesson" />

@code {
    [Parameter]
    public ContentItem? Item { get; set; }
}
```

## Parameters

<!-- osirion:parameters ContentSeriesNavigation -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Item` | `ContentItem?` | `null` | The item being shown. Required: nothing renders without it, nor when the item has no `series`, when the series has fewer than two published parts or when the item is not among them. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | A function that builds each part's link. Without it, links go to `/{Url}` of the item, the path the content lists link to. |
| `ShowPartList` | `bool` | `true` | Whether every part is listed, not only the previous and next one. |
| `PositionFormat` | `string` | `"Part {0} of {1}"` | The position text, a format string where `{0}` is the part number and `{1}` the number of parts. Formatted with the current culture. Defaults to `Part {0} of {1}`. |
| `PreviousText` | `string` | `"Previous"` | The label of the previous-part link. |
| `NextText` | `string` | `"Next"` | The label of the next-part link. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the `nav`, and other attributes on the tag are added to it, `Theme` included. `Style` is accepted but
not written.

## Behavior

- The parts are the published items in the item's locale whose `series` matches, ignoring case. They are ordered
  by `series_order`; parts without a number come after the numbered ones, oldest first.
- Nothing renders when the item has no `series`, when the series has only one published part, or when the item
  itself is not among the parts. The content is queried only for items with a series, and only again when the item
  or its series changes.
- If the content provider fails, the error is logged through `ILogger` and the component renders nothing, so the
  article still shows.
- The `nav` is labelled with the series name and position, for example "Build a blog with Osirion: Part 3 of 5".
  The current part in the list carries `aria-current="page"`, and the previous and next links carry `rel="prev"`
  and `rel="next"`.
- Titles and the series name are written as text, so they are HTML-encoded. The links are plain anchors, so it
  works under static SSR without script.
- The built-in `ArticlePage` renders it when its `ShowSeriesNavigation` is on, which is the default.

## Styling

| Class | Element |
| --- | --- |
| `osirion-series-nav` | The `nav`, plus your `Class` |
| `osirion-series-nav-heading` | The paragraph with the position and series name |
| `osirion-series-nav-position`, `osirion-series-nav-name` | The position text and the series name |
| `osirion-series-nav-parts` | The `ol` of parts |
| `osirion-series-nav-current` | The current part's `li` |
| `osirion-series-nav-pager` | The `div` around the previous and next links |
| `osirion-series-nav-previous`, `osirion-series-nav-next` | The previous and next links |
| `osirion-series-nav-direction`, `osirion-series-nav-title` | A link's label and the part's title |

The box uses `--osirion-border-color` and `--osirion-background-secondary`; text uses `--osirion-text-primary` and
`--osirion-text-secondary`.

## Related

- [OsirionContentNavigation](/components/osirion-content-navigation), previous and next links by directory order
- [Content List](/components/content-list)
- [CMS navigation components](/components/cms-navigation)
