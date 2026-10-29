---
id: 'card-grid'
order: 6
layout: docs
title: OsirionCardGrid
permalink: /components/card-grid
description: OsirionCardGrid lays cards out in equal columns that collapse on tablets and phones, rendered without script.
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-card-grid.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- blazor
- card-grid
- cards
- layout
- static-ssr
is_featured: false
published: true
slug: card-grid
lang: en
custom_fields:
  group: cards
seo_properties:
  title: 'OsirionCardGrid: a responsive card grid for Blazor'
  description: 'A responsive grid for Blazor cards: up to four equal columns that collapse to two and then one, rendered without script under static SSR.'
  image: 'https://media.getosirion.com/og/components-card-grid.jpg'
  canonical: 'https://getosirion.com/components/card-grid'
  lang: en
  robots: index, follow
  og_title: 'OsirionCardGrid - Osirion.Blazor'
  og_description: 'Up to four equal columns of cards that collapse on narrow screens, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-card-grid.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionCardGrid - Osirion.Blazor'
  twitter_description: 'Up to four equal columns of cards that collapse on narrow screens, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-card-grid.jpg'
  meta_tags: {}
---

# OsirionCardGrid

`OsirionCardGrid` lays cards out in equal columns: the number you ask for on wide screens, at most two on tablets and
one on phones. It renders a plain `<div>` grid without script and takes any content, such as
[feature cards](/components/feature-card) with icons.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionCardGrid.razor). Available from 4.3.

```demo
card-grid
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionCardGrid Columns="3">
    <OsirionFeatureCard Title="Fast pages" Description="HTML on the first request." />
    <OsirionFeatureCard Title="Themed by tokens" Description="One set of colors for every framework." />
    <OsirionFeatureCard Title="Accessible markup" Description="Landmarks, headings and labels in place." />
</OsirionCardGrid>
```

## Parameters

<!-- osirion:parameters OsirionCardGrid -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The cards, or any other blocks, one per grid cell. |
| `Columns` | `int` | `3` | The number of columns on wide screens, `1` to `4`; it defaults to `3`. A value outside the range is moved to the nearest one. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the grid `<div>`, so an `aria-label` or an `id` can go there.

## Behavior

- The component renders plain HTML and needs no script.
- `Columns` takes `1` to `4`; a value outside the range moves to the nearest one. The grid shows that many columns
  from 992px, at most two from 640px, and one below.
- The columns are set by a class, not an inline style, so the grid writes no `style` attribute for a Content Security
  Policy to block.
- Each cell can shrink below its content's width, so a wide card cannot widen the page.

## Styling

Classes: `osirion-card-grid` and `osirion-card-grid-columns-1` to `osirion-card-grid-columns-4`. The gap between cells
is `--osirion-spacing-6`; set another in `Class` or `Style`.

## Related

- [Feature card](/components/feature-card), with an `Icon` and `Items` for a grid of features
- [Cards](/components/cards)
- [Metric grid](/components/metric-grid), for figures
