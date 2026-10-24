---
id: 'osirion-background-pattern'
order: 5
layout: docs
title: OsirionBackgroundPattern
permalink: /components/background-pattern
description: A decorative CSS background layer (dots, grid, diagonal lines, honeycomb and more) drawn in the theme's primary color behind a section.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-background-pattern.jpg'
categories:
- Components
- Core Components
- Layout
tags:
- blazor
- background
- pattern
- css
- layout
is_featured: true
published: true
slug: background-pattern
lang: en
redirect_from:
- /docs/components/core/layout/background-pattern
custom_fields:
  group: layout
seo_properties:
  title: 'OsirionBackgroundPattern: CSS patterns for Blazor'
  description: 'A decorative CSS background layer for Blazor sections: dots, grid, diagonal lines, honeycomb, circuit and more, in the theme color.'
  image: 'https://media.getosirion.com/og/components-background-pattern.jpg'
  canonical: 'https://getosirion.com/components/background-pattern'
  lang: en
  robots: index, follow
  og_title: 'OsirionBackgroundPattern - Osirion.Blazor'
  og_description: 'Decorative CSS background patterns drawn in the theme primary color.'
  og_image_url: 'https://media.getosirion.com/og/components-background-pattern.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionBackgroundPattern - Osirion.Blazor'
  twitter_description: 'Decorative CSS background patterns for Blazor sections.'
  twitter_image_url: 'https://media.getosirion.com/og/components-background-pattern.jpg'
  meta_tags: {}
---

# OsirionBackgroundPattern

`OsirionBackgroundPattern` renders an empty, absolutely positioned `div` that fills its parent with a decorative
pattern made of CSS gradients. Use it behind a hero or section; it has no content and needs no images or script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Layout/OsirionBackgroundPattern.razor).

```demo
background-pattern
```

Most of the time you do not place it yourself: [HeroSection](/components/hero-section),
[OsirionBaseSection](/components/base-section) and `OsirionContentNotFound` take a
`BackgroundPattern` parameter and render it for you.

## Usage

Through a section:

```razor
@using Osirion.Blazor.Components

<OsirionBaseSection Title="Categories" ShowPattern="true" BackgroundPattern="BackgroundPatternType.Grid">
    <p>Browse the documentation by topic.</p>
</OsirionBaseSection>
```

`OsirionBaseSection` draws the pattern only when `ShowPattern` is `true` (its default is `false`).

On its own, inside a positioned container, with the content raised above it:

```razor
<div style="position: relative;">
    <OsirionBackgroundPattern BackgroundPattern="BackgroundPatternType.Dots" />
    <div style="position: relative; z-index: 1;">
        <h2>Release notes</h2>
    </div>
</div>
```

The pattern layer has `position: absolute; inset: 0; z-index: 0`, so its parent needs a `position` other than
`static`, and your content needs `position: relative` with a higher `z-index` to stay above it. The stylesheet's
`osirion-bg-content` class is scoped to the component, so it does not apply to markup in your own pages; set those
two properties yourself, as above.

## Parameters

<!-- osirion:parameters OsirionBackgroundPattern -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `BackgroundPattern` | `BackgroundPatternType?` | `null` | The pattern to draw, as the class `osirion-bg-{pattern}`. When it is `null`, nothing is rendered. |
| `MaskImage` | `bool` | `true` | Whether the class `osirion-bg-image-mask` is added, a vertical gradient mask that fades the pattern out toward the bottom edge. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Style`, `Theme` and unmatched attributes are not applied to the markup.

## Patterns

| `BackgroundPatternType` | CSS class | Look | Opacity |
| --- | --- | --- | --- |
| `Dots` | `osirion-bg-dots` | Dots on a 24 px grid. | 0.2 |
| `Grid` | `osirion-bg-grid` | Thin lines on a 24 px grid. | 0.2 |
| `Diagonal` | `osirion-bg-diagonal` | 45 degree lines, 24 px apart. | 0.15 |
| `Honeycomb` | `osirion-bg-honeycomb` | A honeycomb-style tiling, 40 by 70 px. | 0.15 |
| `GradientMesh` | `osirion-bg-gradient-mesh` | Color glows in the four corners, blurred by 60 px. | 0.1 |
| `AnimatedGrid` | `osirion-bg-grid-animated` | Grid that pulses between 24 and 28 px over 8 seconds. | 0.15, pulsing between 0.1 and 0.2 |
| `TechWave` | `osirion-bg-tech-wave` | Slowly rotating light overlay. | not set |
| `Circuit` | `osirion-bg-circuit` | Dots and short lines like a circuit board. | 0.3 |
| `DotsFade` | `osirion-bg-dots-fade` | Dots that fade out toward the edges. | 0.2 |

`TechWave` and `Circuit` mix the theme color into a transparent one with `color-mix()`, so they follow the theme like
the other patterns.

Under a dark theme, every pattern except `GradientMesh` and `TechWave` is drawn as dots at 0.15 opacity, and
`GradientMesh` is drawn at 0.15. The stylesheet takes a page or a parent element as dark when it carries
`data-osirion-theme="dark"`, which `OsirionStyles` sets on `<html>`, `data-bs-theme="dark"`, which it adds under
Bootstrap, or `data-theme="dark"`, which a component's `Theme` parameter writes outside Bootstrap. Up to 4.2.0 it looked
for `data-theme` alone, so on a site themed by `OsirionStyles` the patterns kept their light look in dark mode.

## Behavior

- **Rendering.** Pure CSS; the same markup under static SSR and interactive rendering.
- **Mask.** With `MaskImage` the layer fades out toward its bottom edge.
- **Accessibility.** The layer is an empty `div` with no text, so assistive technology has nothing to announce. Keep
  enough contrast between your text and the pattern; the patterns use low opacity for that reason. With
  `prefers-contrast: more` the layer's opacity drops to 0.05.
- **Motion.** `AnimatedGrid` and `TechWave` animate continuously, and stop under `prefers-reduced-motion: reduce`,
  leaving a still pattern.

## Styling

Patterns are drawn in `--osirion-action-primary`, so they follow the theme and the CSS framework adapter. Change the
color for one area by setting the token on a parent:

```css
.release-notes {
    --osirion-action-primary: #0e7490;
}
```

## Related

- [HeroSection](/components/hero-section)
- [OsirionBaseSection](/components/base-section)
- [Layout overview](/components/layout)
