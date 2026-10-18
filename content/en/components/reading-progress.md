---
id: 'osirion-reading-progress'
order: 7
layout: docs
title: OsirionReadingProgress
permalink: /components/reading-progress
description: "OsirionReadingProgress shows a bar at the top of the window that fills as the reader scrolls, driven by a CSS scroll-driven animation with no script."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-reading-progress.jpg'
categories:
- Components
- Core Components
- Content
tags:
- reading-progress
- scroll
- css
- static-ssr
slug: reading-progress
lang: en
published: true
redirect_from:
- /docs/components/core/content/reading-progress
custom_fields:
  group: content
seo_properties:
  title: 'OsirionReadingProgress: reading progress bar for Blazor'
  description: 'OsirionReadingProgress shows a bar at the top of the window that fills as the reader scrolls, driven by a CSS scroll-driven animation with no script.'
  image: 'https://media.getosirion.com/og/components-reading-progress.jpg'
  canonical: 'https://getosirion.com/components/reading-progress'
  lang: en
  robots: index, follow
  og_title: 'OsirionReadingProgress - Osirion.Blazor'
  og_description: 'A bar at the top of the window that fills as the reader scrolls, with no script.'
  og_image_url: 'https://media.getosirion.com/og/components-reading-progress.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionReadingProgress - Osirion.Blazor'
  twitter_description: 'A bar at the top of the window that fills as the reader scrolls, with no script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-reading-progress.jpg'
  meta_tags: {}
---

# OsirionReadingProgress

`OsirionReadingProgress` shows a thin bar at the top of the window that fills from left to right as the reader
scrolls through the page. Use it on long articles. It is a CSS scroll-driven animation, so it needs no script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionReadingProgress.razor). Available from 4.0.

```demo
reading-progress
```

## Usage

Place it once on the page, anywhere in the markup:

```razor
@using Osirion.Blazor.Components

<OsirionReadingProgress />
```

To change its color and thickness, set its two custom properties in your stylesheet:

```css
:root {
    --osirion-reading-progress-color: #16a34a;
    --osirion-reading-progress-height: 4px;
}
```

## Parameters

The component has no parameters of its own. It takes the ones every Osirion component inherits:

<!-- osirion:parameters OsirionReadingProgress -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes and `Class` go on the bar `<div>`. `Style` is accepted but the component does not write it.

## Behavior

- The bar is `position: fixed` at the top of the viewport and its width follows the scroll position of the page's
  root scroller (`animation-timeline: scroll(root block)`). It does not follow a scrolling container inside the page.
- Browsers without scroll-driven animations do not show the bar at all, so it never sits empty or stuck. The CSS
  checks support with `@supports (animation-timeline: scroll())`.
- The bar is decorative: it has `aria-hidden="true"`, because the scroll position is already available to assistive
  technology, and it ignores the pointer, so it never blocks a click on a sticky header beneath it.
- It does not check `prefers-reduced-motion`. The bar follows the scroll position rather than playing on its own, so
  it does not animate by itself.
- It renders the same in static SSR and in interactive render modes.

## Styling

| Class or property | Purpose |
| --- | --- |
| `osirion-reading-progress` | The bar |
| `--osirion-reading-progress-color` | Bar color. Default: `--osirion-action-primary`, falling back to `#2563eb`. |
| `--osirion-reading-progress-height` | Bar thickness. Default: `3px`. |

The bar sits at `z-index: 1100`. If a fixed header in your layout uses a higher value, pass a class
(`Class="article-progress"`) and raise the bar's `z-index` with a selector such as
`body .osirion-reading-progress.article-progress`; the component's own styles are scoped (a class plus an attribute),
so a single class selector is not specific enough to override them.

## Related

- [Article metadata](/components/article-metadata)
- [Content components](/components/content)
