---
id: 'cms-web-table-of-contents'
order: 8
title: TableOfContents
permalink: /components/table-of-contents
description: "TableOfContents reads the headings of an HTML string, such as a rendered CMS item, and renders links to them as a nested list."
author: Dejan Demonjić
date: "2024-12-29"
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-table-of-contents.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- table-of-contents
- headings
published: true
slug: table-of-contents
lang: en
redirect_from:
- /docs/components/cms-web/navigation/table-of-contents
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'TableOfContents: a heading outline for Blazor'
  description: 'Build a table of contents from the headings of rendered Markdown or any HTML string, with heading level limits and no script.'
  image: 'https://media.getosirion.com/og/components-table-of-contents.jpg'
  canonical: 'https://getosirion.com/components/table-of-contents'
  lang: en
  robots: index, follow
  og_title: 'TableOfContents - Osirion.Blazor'
  og_description: 'A nested list of links to the headings of an HTML string, with the section being read marked as you scroll.'
  og_image_url: 'https://media.getosirion.com/og/components-table-of-contents.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'TableOfContents - Osirion.Blazor'
  twitter_description: 'A nested list of links to the headings of an HTML string, with the section being read marked as you scroll.'
  twitter_image_url: 'https://media.getosirion.com/og/components-table-of-contents.jpg'
  meta_tags: {}
---

# TableOfContents

`TableOfContents` reads the `h1` to `h6` headings of an HTML string and renders a nested list of links to them. Pass
it the rendered content of a CMS item to give long articles and documentation pages an outline.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/TableOfContents.razor).

```demo
table-of-contents
```

The demo builds the outline from a small sample HTML string and renders that HTML below it, so the links have headings
to go to.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<TableOfContents Content="@Item?.Content" />
```

A sidebar outline limited to the second and third heading levels:

```razor
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Web.Components

<aside>
    <h2>On this page</h2>
    <TableOfContents Content="@Item?.Content"
                     MinLevel="2"
                     MaxLevel="3"
                     EmptyText="This page has no sections." />
</aside>

@code {
    [Parameter]
    public ContentItem? Item { get; set; }
}
```

The entries are numbered by default, `1.` and `1.2`. Set `ShowNumbers="false"` for a plain list, for example when
your headings already carry their own numbers:

```razor
<TableOfContents Content="@Item?.Content" ShowNumbers="false" />
```

The built-in `ArticlePage` and `DocumentPage` render it the same way, under their own `TableOfContentsTitle`
heading.

## Parameters

<!-- osirion:parameters TableOfContents -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Content` | `string?` | `null` | The HTML to read the headings from, usually an item's rendered content. Nothing is listed when it is empty. |
| `MinLevel` | `int` | `2` | The lowest heading level included, so the page's `h1` is left out by default. |
| `MaxLevel` | `int` | `6` | The highest heading level included. |
| `MaxDepth` | `int` | `3` | The nesting limit. `1` lists only top-level headings; the list renders at most two levels, so a value above `2` changes nothing. |
| `EmptyText` | `string` | `"No headings found."` | The text shown when no heading matches. |
| `TrackActiveHeading` | `bool` | `true` | Whether the link to the section being read is marked as it scrolls by: it gets the `osirion-toc-link-active` class and `aria-current="location"`. A script does it, so without script the contents work as plain links. |
| `ShowNumbers` | `bool` | `true` | Whether the entries are numbered, `1.` for a top-level heading and `1.2` for one under it. When on, the root element gets the `osirion-toc-numbered` class that the numbering rules hang on; `false` renders a plain list. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the `nav`. `Style` and `Theme` are accepted but not written, and other attributes on the tag are
captured but not rendered.

## Behavior

- Each link points to `{current path}#{heading id}`. The CMS Markdown renderer gives every heading an `id` and the
  HTML sanitizer keeps it, so the links work for CMS content. For a heading without an `id`, the component makes one
  from the text (lower case, letters and digits, spaces turned into hyphens), and the link only works if your page
  has an element with that id.
- The heading's `id` is read wherever it sits among the attributes of the tag, quoted or bare. A heading whose text
  runs over several lines is found.
- The search for headings stops after one second. On HTML that makes it take longer, such as thousands of unclosed
  heading tags, the outline is empty and a warning is logged.
- Headings are nested by level: an `h3` goes under the `h2` before it. The list shows two levels, the top-level
  headings and the headings directly under them. Deeper headings are not shown.
- Tags inside a heading are stripped and the remaining text is written as text, so it is HTML-encoded. Character
  references are not decoded first, so a heading such as `Q&A`, which the Markdown renderer writes as `Q&amp;A`,
  shows as `Q&amp;A` in the outline.
- The component always renders a `nav` with the accessible name "Table of Contents" (fixed English text), and the
  `EmptyText` paragraph when there is nothing to list. Headings are read on every parameter change, so the outline
  is part of the static SSR response.
- With `TrackActiveHeading` on and at least one heading listed, a small script (loaded through BlazorJSComponents,
  which `AddOsirion` registers) marks the link of the section being read as the page scrolls:
  the last heading whose top has passed a line a quarter of the viewport below the top (plus the
  `--osirion-anchor-offset` of a sticky header). At the end of the page the last heading wins. Without script the
  links work as they are.

## Styling

| Class | Element |
| --- | --- |
| `osirion-table-of-contents` | The `nav`, plus your `Class` |
| `osirion-toc-numbered` | Also on the `nav` while `ShowNumbers` is on; the counters and number markers apply only under it |
| `osirion-toc-list` | The top-level `ol` |
| `osirion-toc-sublist` | The nested `ol` |
| `osirion-toc-item` | Each `li`, together with `osirion-toc-level-{n}` for the heading level |
| `osirion-toc-link` | Each link; the script adds `osirion-toc-link-active` to the one being read |
| `osirion-toc-empty` | The empty-state paragraph |

Links use `--osirion-text-secondary` and `--osirion-action-primary`, with `--osirion-focus-ring-color` for keyboard
focus. The marked link reads `--osirion-toc-active-color`, `--osirion-toc-active-accent` and
`--osirion-toc-active-background`. The stylesheet also has print rules.

## Related

- [OsirionStickySidebar](/components/sticky-sidebar), to keep the outline in view while reading
- [Content View](/components/content-view)
- [CMS navigation components](/components/cms-navigation)
