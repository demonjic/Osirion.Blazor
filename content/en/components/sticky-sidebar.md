---
id: 'osirion-sticky-sidebar'
order: 4
layout: docs
title: OsirionStickySidebar
permalink: /components/sticky-sidebar
description: An aside that stays in view while the page scrolls, with its own scroll area, for a table of contents or docs navigation. Pure CSS.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-sticky-sidebar.jpg'
categories:
- Components
- Core Components
- Layout
tags:
- blazor
- sidebar
- sticky
- layout
- table-of-contents
- css
is_featured: true
published: true
slug: sticky-sidebar
lang: en
redirect_from:
- /docs/components/core/layout/sticky-sidebar
custom_fields:
  group: layout
seo_properties:
  title: 'OsirionStickySidebar: sticky sidebar for Blazor'
  description: 'A Blazor aside that stays in view while the page scrolls, for a table of contents or docs navigation. Pure CSS, no script.'
  image: 'https://media.getosirion.com/og/components-sticky-sidebar.jpg'
  canonical: 'https://getosirion.com/components/sticky-sidebar'
  lang: en
  robots: index, follow
  og_title: 'OsirionStickySidebar - Osirion.Blazor'
  og_description: 'A sticky aside with its own scroll area for a table of contents or docs navigation.'
  og_image_url: 'https://media.getosirion.com/og/components-sticky-sidebar.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionStickySidebar - Osirion.Blazor'
  twitter_description: 'A sticky aside for a table of contents or docs navigation.'
  twitter_image_url: 'https://media.getosirion.com/og/components-sticky-sidebar.jpg'
  meta_tags: {}
---

# OsirionStickySidebar

`OsirionStickySidebar` renders an `aside` that sticks below your header while the page scrolls and scrolls on its
own when its content is taller than the viewport. Use it for a table of contents or documentation navigation next to
long content. It is pure CSS, so it works under static SSR without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Layout/OsirionStickySidebar.razor).

```demo
sticky-sidebar
```

Scroll inside the frame to see the sidebar stay in view. The frame stands in for the viewport, so the demo caps the
sidebar's height at 11 rem; on a page it fills the viewport below the header. At 768 pixels wide or less the sidebar
is static and flows with the page, so on a phone it scrolls away with the text.

## Usage

```razor
@using Osirion.Blazor.Components

<div class="docs-grid">
    <article>
        <h1>Installation</h1>
    </article>

    <OsirionStickySidebar>
        <nav aria-label="Documentation">
            <a href="/docs/installation">Installation</a>
            <a href="/docs/configuration">Configuration</a>
        </nav>
    </OsirionStickySidebar>
</div>
```

The CMS article page uses it the same way for its table of contents. Give the sidebar a column in a grid or flex
layout; `position: sticky` only takes effect while the parent is taller than the sidebar and no ancestor between
them sets `overflow: hidden`.

When your header is taller than 2.5rem, set `TopOffset` in pixels (`<OsirionStickySidebar TopOffset="64">`), or set the
custom property the stylesheet reads, which applies while `TopOffset` is left at its default:

```css
.docs-grid {
    --osirion-header-height: 4rem;
}
```

## Parameters

<!-- osirion:parameters OsirionStickySidebar -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The content of the `aside` element. |
| `TopOffset` | `int` | `40` | The offset from the top of the viewport, in pixels (default 40, which is 2.5rem). A value other than the default is written to the `aside` as `--osirion-header-height`; at the default the stylesheet decides, so a site that sets the custom property itself keeps working. |
| `HideScrollbar` | `bool` | `true` | Whether the scrollbar is hidden (class `osirion-no-scrollbar`). The sidebar still scrolls with the wheel, touch and keyboard. |
| `IsSticky` | `bool` | `true` | Whether the sidebar sticks below the header. `false` adds `osirion-non-sticky`, which renders a normal, static `aside` with its natural height. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

A `TopOffset` other than the default is written to the `aside` as an inline `--osirion-header-height`, which wins over
a value set in CSS. `Style`, `Theme` and unmatched attributes are not applied to the markup.

## Behavior

- **Sticky area.** The `aside` is `position: sticky` with `top` and `height` derived from `--osirion-header-height`
  (default `2.5rem`), so it fills the viewport below the header and scrolls internally (`overflow-y: auto`).
- **Small screens.** At 768 pixels wide or less the sidebar is static, with its natural height and no scroll area of
  its own, so it flows with the page. Hide or move it with your own CSS if a mobile layout needs that.
- **Motion.** Smooth scrolling inside the sidebar is turned off when the visitor prefers reduced motion.
- **Accessibility.** The element is an `aside`, a complementary landmark. Put a labelled `nav` inside it when it holds
  navigation, as in the example, so screen reader users can tell several landmarks apart. With `HideScrollbar`, make
  sure the content is reachable by keyboard (links are) because there is no visible scrollbar to drag.

## Styling

| Class or property | Purpose |
| --- | --- |
| `osirion-sticky-sidebar` | The `aside`. |
| `osirion-no-scrollbar` | Added by `HideScrollbar`. |
| `osirion-non-sticky` | Added when `IsSticky` is `false`. |
| `--osirion-header-height` | Top offset and height reduction, default `2.5rem`. |
| `--osirion-scrollbar-thumb`, `--osirion-scrollbar-thumb-hover` | Scrollbar thumb colors in WebKit browsers when the scrollbar is shown. |

## Related

- [OsirionPageLayout](/components/page-layout)
- [Layout overview](/components/layout)
