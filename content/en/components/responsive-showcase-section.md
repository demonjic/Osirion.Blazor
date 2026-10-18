---
id: 'responsive-showcase-section'
order: 6
layout: docs
title: OsirionResponsiveShowcaseSection
permalink: /components/responsive-showcase-section
description: OsirionResponsiveShowcaseSection previews content in a frame that switches between desktop, tablet and mobile widths, for demo and docs pages.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-responsive-showcase-section.jpg'
categories:
- Components
- Core Components
- Sections
- Responsive Design
tags:
- blazor
- responsive
- showcase
- preview
- interactive
is_featured: true
published: true
slug: responsive-showcase-section
lang: en
redirect_from:
- /docs/components/core/sections/responsive-showcase-section
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionResponsiveShowcaseSection: preview for Blazor'
  description: 'Preview Blazor content in a frame that switches between desktop, tablet and mobile widths, with an optional code sample.'
  image: 'https://media.getosirion.com/og/components-responsive-showcase-section.jpg'
  canonical: 'https://getosirion.com/components/responsive-showcase-section'
  lang: en
  robots: index, follow
  og_title: 'OsirionResponsiveShowcaseSection - Osirion.Blazor'
  og_description: 'A preview frame that switches between desktop, tablet and mobile widths.'
  og_image_url: 'https://media.getosirion.com/og/components-responsive-showcase-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionResponsiveShowcaseSection - Osirion.Blazor'
  twitter_description: 'A preview frame that switches between desktop, tablet and mobile widths.'
  twitter_image_url: 'https://media.getosirion.com/og/components-responsive-showcase-section.jpg'
  meta_tags: {}
---

# OsirionResponsiveShowcaseSection

`OsirionResponsiveShowcaseSection` shows content inside a browser-like frame with buttons that switch the frame
between desktop, tablet and mobile widths, and an optional code sample under it. It is a tool for component demos
and design reviews, not for a production page.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Core.Components.Sections`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionResponsiveShowcaseSection.razor).

This page has no live demo. The width buttons and the code toggle are Blazor event handlers, so the component needs an interactive render mode, and this site renders its pages as static HTML, where the buttons would do nothing.

## Usage

The component is in its own namespace, and switching widths needs an interactive render mode on the page. Without one, the frame renders in its `InitialViewport` and the buttons do nothing:

```razor
@page "/demo/cards"
@rendermode InteractiveServer
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Components.Sections

<OsirionResponsiveShowcaseSection Title="Feature card">
    <OsirionFeatureCard Title="Static SSR" Description="Every component renders on the server." />
</OsirionResponsiveShowcaseSection>
```

Start in the mobile width and offer the markup under the preview:

```razor
<OsirionResponsiveShowcaseSection Title="Hero"
                                  InitialViewport="OsirionResponsiveShowcaseSection.ViewportMode.Mobile"
                                  CodeSnippet="@heroMarkup">
    <HeroSection Title="Release notes" Subtitle="What changed in each version" />
</OsirionResponsiveShowcaseSection>

@code {
    private const string heroMarkup =
        "<HeroSection Title=\"Release notes\" Subtitle=\"What changed in each version\" />";
}
```

## Parameters

<!-- osirion:parameters OsirionResponsiveShowcaseSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `"Component Preview"` | The heading above the frame, rendered as an `h3`. |
| `ChildContent` | `RenderFragment?` | `null` | The content to preview inside the frame; without it a placeholder is shown. |
| `ShowDimensions` | `bool` | `true` | Whether the current width is described under the frame. |
| `ShowCode` | `bool` | `true` | Whether a Show Code button appears; it needs `CodeSnippet`. |
| `CodeSnippet` | `string?` | `null` | The code shown under the preview when the button is pressed, rendered as encoded text. |
| `InitialViewport` | `ViewportMode` | `ViewportMode.Desktop` | The width the frame starts in: `Desktop` (1200px), `Tablet` (768px) or `Mobile` (375px), each capped at the available width. |
| `ShowBrowserChrome` | `bool` | `true` | Whether the frame shows the browser bar (dots and address) above the content; `false` leaves it out and the frame loses the `with-chrome` class. |
| `Class` | `string?` | `null` | Extra CSS classes on the component's root element. |
| `Style` | `string?` | `null` | An inline style for the frame, added after its width declarations. |

<!-- /osirion:parameters -->

`ViewportMode` is nested in the component: write `OsirionResponsiveShowcaseSection.ViewportMode.Mobile`. `Class` and unmatched attributes are written to the root element. The component does not inherit `OsirionComponentBase`, so it has no `Theme` parameter.

## Behavior

- The frame widths are 1200px (desktop), 768px (tablet) and 375px (mobile), each capped at 100% of the available
  width.
- The frame is a `<div>`, not an `<iframe>`. Layouts that depend on the width of their container (wrapping flex
  rows, `auto-fit` grids, container queries) change with the frame; CSS media queries still follow the browser
  window.
- Switching widths and showing the code are Blazor event handlers. Under static SSR the component renders in its
  `InitialViewport` with the code hidden, and the buttons do nothing. Because `ChildContent` cannot cross a render
  mode boundary, make the whole page interactive rather than setting `@rendermode` on this component.
- The frame's address bar shows fixed text (`example.com - Desktop View`, and the same for tablet and mobile), not your page's address.
- The browser bar (dots and address) is shown unless `ShowBrowserChrome` is `false`, which leaves it out of the markup
  and the `with-chrome` class off the root. On screens 600px wide or narrower the bar is hidden by CSS.
- The width buttons report their state with `aria-pressed` and are named "Desktop", "Tablet" and "Mobile" with
  `aria-label`; their icons are hidden from assistive technology.

## Styling

The root is `osirion-responsive-showcase`, with `showcase-header`, `showcase-title`, `viewport-controls` and
`viewport-btn` (`active` on the current width) for the header, and `showcase-body`, `viewport-container`
(`viewport-desktop`, `viewport-tablet` or `viewport-mobile`), `viewport-chrome`, `component-wrapper`,
`dimensions-info` and `code-section` for the rest.

## Related

- [Core section components](/components/sections)
- [Feature card](/components/feature-card)
- [Hero section](/components/hero-section)
