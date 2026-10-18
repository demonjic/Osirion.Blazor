---
id: 'metric-grid'
order: 9
layout: docs
title: OsirionMetricGrid
permalink: /components/metric-grid
description: OsirionMetricGrid lays out metric cards as a responsive list, described by records. Parameters, behavior and styling.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-metric-grid.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- metrics
- cards
- osirion-blazor-core
- static-ssr
is_featured: true
published: true
slug: metric-grid
lang: en
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionMetricGrid: metric card grid for Blazor'
  description: 'OsirionMetricGrid lays out metric cards as a responsive list, described by records. Parameters, behavior and styling.'
  image: 'https://media.getosirion.com/og/components-metric-grid.jpg'
  canonical: 'https://getosirion.com/components/metric-grid'
  lang: en
  robots: index, follow
  og_title: 'OsirionMetricGrid - Osirion.Blazor'
  og_description: 'OsirionMetricGrid lays out metric cards as a responsive list, described by records. Parameters, behavior and styling.'
  og_image_url: 'https://media.getosirion.com/og/components-metric-grid.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionMetricGrid - Osirion.Blazor'
  twitter_description: 'OsirionMetricGrid lays out metric cards as a responsive list, described by records. Parameters, behavior and styling.'
  twitter_image_url: 'https://media.getosirion.com/og/components-metric-grid.jpg'
  meta_tags: {}
---

# OsirionMetricGrid

`OsirionMetricGrid` lays out several [metric cards](/components/metric-card) as a responsive list: one column on phones, two
from 576px wide, and two, three or four from 992px. Describe each metric with a `MetricGridItem`; the grid needs no child markup.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionMetricGrid.razor).

```demo
metric-grid
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionMetricGrid Columns="MetricGridColumns.Three"
                   AriaLabel="Service reliability metrics"
                   Metrics="metrics" />

@code {
    private readonly IReadOnlyList<OsirionMetricGrid.MetricGridItem> metrics =
    [
        new("99.9%", "Availability", "Measured over twelve months", MetricCardAccent.Teal, Animate: true),
        new("24/7", "Monitoring", Kicker: "Operations"),
        new("15 min", "Response target", Accent: MetricCardAccent.Amber, Elevated: true)
    ];
}
```

The figures above are placeholders; show only numbers you can stand behind.

## Parameters

<!-- osirion:parameters OsirionMetricGrid -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Metrics` | `IReadOnlyList<MetricGridItem>` | `[]` | The metrics, one `OsirionMetricCard` each; with none, nothing is rendered. |
| `Columns` | `MetricGridColumns` | `MetricGridColumns.Four` | The number of columns from 992px wide: `Two`, `Three` or `Four`. The grid has one column on phones and two from 576px. |
| `AriaLabel` | `string` | `"Key metrics"` | The accessible name of the metric list. |
| `CssClass` | `string?` | `null` | Extra CSS classes on the list, added before `OsirionComponentBase.Class`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`MetricGridItem` is a record nested in the component:
`OsirionMetricGrid.MetricGridItem(string Value, string? Label = null, string? Description = null, MetricCardAccent Accent = MetricCardAccent.Green, string? Kicker = null, bool Elevated = false, bool Animate = false)`.
Each member goes to the parameter of the same name on `OsirionMetricCard`; the card's `Border` and `Shadow` keep their defaults.
`MetricGridColumns` has `Two`, `Three` and `Four`; `Two` stays at two columns on wide screens.

## Behavior

- The grid is plain markup and CSS. Only a card with `Animate` loads the count-up script, through BlazorJSComponents, which
  `AddOsirion` registers; see [OsirionMetricCard](/components/metric-card).
- The grid is a `ul` named by `AriaLabel`, and each metric is an `li` holding a card, so screen readers announce the number of
  metrics. With no metrics nothing is rendered.
- `Class`, `CssClass` and `Style` are applied to the `ul`, as are unmatched attributes. The list has `role="list"`, which
  keeps its list semantics where the browser drops them for an unstyled list.
- Each card fills its cell with its own CSS, not a framework class, so the cards of a row are equally tall with or without Bootstrap.

## Styling

The list has `osirion-metric-grid` and `osirion-metric-grid-columns-{2|3|4}`. The gap between cards is
`--osirion-metric-grid-gap` (default `1rem`). The cards keep their own classes and tokens; see the
[metric card](/components/metric-card) page.

## Related

- [OsirionMetricCard](/components/metric-card)
- [Cards](/components/cards)
- [Sections](/components/sections)
