---
id: 'context-card-grid'
order: 10
layout: docs
title: OsirionContextCardGrid
permalink: /components/context-card-grid
description: OsirionContextCardGrid renders a heading and a grid of labeled detail cards with an accent color. Parameters, behavior and styling.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-context-card-grid.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- cards
- sections
- osirion-blazor-core
- static-ssr
is_featured: true
published: true
slug: context-card-grid
lang: en
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionContextCardGrid: detail card grid for Blazor'
  description: 'OsirionContextCardGrid renders a heading and a grid of labeled detail cards with an accent color. Parameters, behavior and styling.'
  image: 'https://media.getosirion.com/og/components-context-card-grid.jpg'
  canonical: 'https://getosirion.com/components/context-card-grid'
  lang: en
  robots: index, follow
  og_title: 'OsirionContextCardGrid - Osirion.Blazor'
  og_description: 'OsirionContextCardGrid renders a heading and a grid of labeled detail cards with an accent color. Parameters, behavior and styling.'
  og_image_url: 'https://media.getosirion.com/og/components-context-card-grid.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContextCardGrid - Osirion.Blazor'
  twitter_description: 'OsirionContextCardGrid renders a heading and a grid of labeled detail cards with an accent color. Parameters, behavior and styling.'
  twitter_image_url: 'https://media.getosirion.com/og/components-context-card-grid.jpg'
  meta_tags: {}
---

# OsirionContextCardGrid

`OsirionContextCardGrid` renders a section with a heading, optional intro text and a grid of labeled detail cards. Each card has a
label, a short description and an accent color on its top border. Use it to set several short points side by side, such as one
point per role or category.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionContextCardGrid.razor).

```demo
context-card-grid
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionContextCardGrid Title="Context by role"
                        Description="Each role sees the same data through its own priorities."
                        Id="roles"
                        HeadingId="roles-heading"
                        Items="roleItems" />

@code {
    private readonly OsirionContextCardGrid.ContextCardItem[] roleItems =
    [
        new("Finance", "Cost variance and forecast predictability.", "blue"),
        new("Operations", "Coverage reliability and exception response."),
        new("Security", "Access reviews and audit evidence.", "purple")
    ];
}
```

The heading is an `h2` and each card label an `h3`. Under a heading of the page that is already an `h2`, set
`HeadingLevel="3"`: the heading becomes an `h3` and the labels `h4`, so the outline stays in order.

## Parameters

<!-- osirion:parameters OsirionContextCardGrid -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `"Context by category"` | The section's heading, rendered at `HeadingLevel`; with an empty value no heading is rendered and the section has no `aria-labelledby`. |
| `HeadingLevel` | `int` | `2` | The level of the section's heading, `1` to `6` (`h1` to `h6`). It defaults to `2`, a section under the page's own `h1`. Each card's label is a heading one level below, up to `h6`. A value outside the range is moved to the nearest level, and the headings keep their look at every level. |
| `Description` | `string?` | `null` | Supporting text under the heading; nothing is rendered when it is empty. |
| `Items` | `ContextCardItem[]` | `[]` | The cards. With none, only the heading is rendered. |
| `HeadingId` | `string?` | `null` | The `id` of the heading, which the section's `aria-labelledby` points at. Without a value each grid generates its own, so several grids on a page never repeat an id. |
| `Id` | `string?` | `null` | The section's `id`, for in-page links. |
| `SectionClass` | `string?` | `null` | Extra CSS classes on the section. |
| `ContainerClass` | `string` | `"container"` | CSS classes of the inner container; `container` is a Bootstrap class, so set another value for other frameworks. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`ContextCardItem` is a record nested in the component: `OsirionContextCardGrid.ContextCardItem(string Label, string Description, string Accent = "green")`.
The accent is a string written into a class name, `osirion-context-card-grid-item-{accent}`, and colors the card's top
border. The stylesheet defines `blue`, `purple`, `indigo`, `teal` and `amber`; any other value, including the default
`green`, uses `--osirion-context-card-grid-accent` (default `#176b45`).

## Behavior

- The component is plain markup and CSS and needs no script.
- The section is a landmark named by its heading: `aria-labelledby` points at the heading, an `h2` unless you set
  `HeadingLevel`. Each grid generates its own heading id unless you set `HeadingId`, so several grids on a page never
  repeat an id. Set the parameter when you need a stable id to link to.
- Each card is an `article` with the label as a heading one level below the section's (`h3` by default, never past
  `h6`) and the description as a paragraph, so the labels nest under the section's heading. A `HeadingLevel` outside
  1 to 6 moves to the nearest level, and the headings keep their look at every level.
- With an empty `Title` no heading is rendered and the section has no `aria-labelledby`. With no items, only the heading
  is rendered.
- `Class` and `Style` are not applied; use `SectionClass` for the section and `ContainerClass` for the container. Unmatched
  attributes are written to the `section`.
- `ContainerClass` defaults to `container`, a Bootstrap class. Under another framework set your own value.
- The cards fill the width with `repeat(auto-fit, minmax(min(100%, 15rem), 1fr))`.

## Styling

The section has `osirion-context-card-grid`; inside are `osirion-context-card-grid-container`, `osirion-context-card-grid-heading`,
`osirion-context-card-grid-items` and one `osirion-context-card-grid-item` per card. Override the look with
`--osirion-context-card-grid-padding`, `--osirion-context-card-grid-background`, `--osirion-context-card-grid-heading-color`,
`--osirion-context-card-grid-description-color`, `--osirion-context-card-grid-accent`,
`--osirion-context-card-grid-card-background`, `--osirion-context-card-grid-card-shadow`,
`--osirion-context-card-grid-item-color` and `--osirion-context-card-grid-item-heading-color`. The heading and
description fall back to the theme's text colors (`--osirion-text-primary` and `--osirion-text-secondary`). The
component has no animation, so reduced motion changes nothing.

`--osirion-context-card-grid-heading-align` aligns the heading and description: `start`, `center` (the default) or
`end`. It moves both the text and its lines, which are at most 52rem wide, so `start` lines them up with the left edge
of the cards:

```css
.roles-section {
    --osirion-context-card-grid-heading-align: start;
}
```

Each card label takes the card's text color: `--osirion-context-card-grid-item-heading-color`, which falls back to
`--osirion-context-card-grid-item-color` (default `#555`). On the default white card that is 7.46:1. Up to 4.2.0 the
label took the accent color: on the white card every accent passed 4.5:1 (the lowest, amber, at 4.87:1), but on a card
set to the dark background of any shipped palette every accent failed, from 2.16:1 to 4.31:1. When you change
`--osirion-context-card-grid-card-background`, set `--osirion-context-card-grid-item-color` with it, as the description
already needs, and the label follows. Set `--osirion-context-card-grid-item-heading-color` only for a label color of
its own, and check it reaches 4.5:1 on your card.

## Related

- [Cards](/components/cards)
- [OsirionMetricGrid](/components/metric-grid)
- [Sections](/components/sections)
