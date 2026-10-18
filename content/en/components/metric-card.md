---
id: 'osirion-metric-card'
order: 5
layout: docs
title: OsirionMetricCard
permalink: /components/metric-card
description: "OsirionMetricCard shows one figure with a label and supporting text, and can count its number up when the card scrolls into view."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-metric-card.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- cards
- metrics
- static-ssr
published: true
slug: metric-card
lang: en
custom_fields:
  group: cards
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/components-metric-card.jpg'
  title: 'OsirionMetricCard: metric cards for Blazor'
  description: 'A Blazor card for one figure, its label and supporting text, with an optional count-up animation that respects reduced motion.'
---

# OsirionMetricCard

`OsirionMetricCard` shows one figure in large type, with an optional line above it, a label and supporting text. Use a
row of them for the numbers a page wants to stand out. The server renders the final value; a small script can count
the number up when the card scrolls into view.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionMetricCard.razor).

```demo
metric-card
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionMetricCard Value="250" Label="Pages" />
```

`Value` is the text as it is shown. With `Animate` on, the card finds the number in it, or the two numbers of a range,
and counts from zero to them; the text before and after stays as written:

```razor
<OsirionMetricCard Value="2-4 days" Label="Delivery time" Animate="true" />
```

Pick the color with `Accent` and the frame with `Border` and `Shadow`:

```razor
<OsirionMetricCard Value="98%"
                   Label="Delivered on time"
                   Accent="MetricCardAccent.Purple"
                   Border="MetricCardBorder.Default"
                   Shadow="MetricCardShadow.SmallAccent" />
```

## Parameters

<!-- osirion:parameters OsirionMetricCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Value` | `string` | `""` | The value as it is shown, such as `99.99%` or `18-32%`. Its number, or a range's two numbers, are what `Animate` counts up; the text around them stays. Required. |
| `Label` | `string?` | `null` | The label under the value. |
| `Description` | `string?` | `null` | Supporting text under the label. |
| `Accent` | `MetricCardAccent` | `MetricCardAccent.Green` | The accent color: a theme color (`Primary`, `Secondary`, `Success`, `Warning`, `Danger`, `Info`) or a fixed one (`Blue`, `Teal`, `Green`, `Amber`, `Purple`, `Indigo`). |
| `Kicker` | `string?` | `null` | Short text above the value. |
| `Border` | `MetricCardBorder` | `MetricCardBorder.Accent` | The border: `None`, `Default` (the theme border) or `Accent`. |
| `Shadow` | `MetricCardShadow` | `MetricCardShadow.Medium` | The shadow: `None`, `Small`, `Medium` or `Large`, or the same sizes tinted with the accent (`SmallAccent`, `MediumAccent`, `LargeAccent`). A card with a shadow lifts on hover. |
| `Elevated` | `bool` | `false` | Whether the shadow is made large: `Large`, or `LargeAccent` for an accent shadow, whatever `Shadow` says. |
| `CssClass` | `string?` | `null` | Extra CSS classes on the card, as `Class` does. |
| `Animate` | `bool` | `false` | Whether the numbers count up from zero, once, when the card scrolls into view. The server renders the final value, so the card reads correctly before and without script; with reduced motion requested, nothing moves. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- The card renders complete HTML under static SSR: the value, as written, is in the page before any script runs.
- The card's script loads through BlazorJSComponents, which `AddOsirion` registers (or
  `builder.Services.AddJSComponents()` without it). With
  `Animate` on, it counts up once, over about a second, when at least 30% of the value is in view. With reduced
  motion requested in the operating system it leaves the value as it is.
- A number with a thousands separator, such as `1,250`, is not counted up; write `1250` or leave `Animate` off.
- The card is an `article` with the value, label and description as text, so screen readers read it in that order.

## Styling

| Class | Element |
| --- | --- |
| `osirion-metric-card` | The card (`article`) |
| `osirion-metric-card-kicker` | The line above the value |
| `osirion-metric-card-value` | The value, in the accent color |
| `osirion-metric-card-label` | The label |
| `osirion-metric-card-description` | The supporting text |

The card also carries one class per setting, such as `osirion-metric-card-purple`, `osirion-metric-card-border-accent`
and `osirion-metric-card-shadow-medium`. Each accent class sets `--osirion-metric-card-accent`; set it yourself on the
card, through `Style` or your stylesheet, for a color of your own. The theme accents follow the palette and the
framework adapter; the fixed ones (`Blue` to `Indigo`) stay the same.

## Related

- [Cards](/components/cards), the other card components
- [Sections](/components/sections), which hold rows of cards
