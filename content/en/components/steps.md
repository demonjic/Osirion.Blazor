---
id: 'steps'
order: 5
layout: docs
title: OsirionSteps
permalink: /components/steps
description: OsirionSteps renders numbered how it works steps as an ordered list, side by side or stacked with a line joining the numbers.
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-steps.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- steps
- how-it-works
- accessibility
- static-ssr
is_featured: true
published: true
slug: steps
lang: en
redirect_from:
- /docs/components/core/sections/steps
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionSteps: numbered steps for Blazor'
  description: 'Numbered how it works steps for Blazor as an ordered list, side by side or stacked, rendered without script under static SSR.'
  image: 'https://media.getosirion.com/og/components-steps.jpg'
  canonical: 'https://getosirion.com/components/steps'
  lang: en
  robots: index, follow
  og_title: 'OsirionSteps - Osirion.Blazor'
  og_description: 'Numbered steps as an ordered list, side by side or stacked, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-steps.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionSteps - Osirion.Blazor'
  twitter_description: 'Numbered steps as an ordered list, side by side or stacked, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-steps.jpg'
  meta_tags: {}
---

# OsirionSteps

`OsirionSteps` renders numbered steps, each with a title and an optional description, for a "how it works" or
"getting started" section. The steps sit side by side on wide screens and stack on narrow ones, or always stack with
a line joining their numbers.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionSteps.razor). Available from 4.0.

```demo
steps
```

## Usage

Steps are `StepItem` records from `Osirion.Blazor.Core.Models`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionSteps Items="@steps" />

@code {
    private readonly StepItem[] steps =
    [
        new("Install the package", "dotnet add package Osirion.Blazor"),
        new("Point it at your content", "A GitHub repository or a local folder of Markdown files."),
        new("Publish", "Static SSR pages that need no client script."),
    ];
}
```

For a longer process, or a narrow column, stack them:

```razor
<OsirionSteps Items="@steps" Vertical="true" />
```

## Parameters

<!-- osirion:parameters OsirionSteps -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Items` | `IReadOnlyList<StepItem>` | `[]` | The steps, in order. Required; with no steps nothing is rendered. |
| `Vertical` | `bool` | `false` | Whether the steps are always stacked, with a line joining their numbers; otherwise they sit side by side on wide screens. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `<ol>`.

`StepItem(string Title, string? Description = null)` is a record: `Title` names the step and `Description` adds a
sentence or two of plain text.

## Behavior

- The component renders plain HTML and needs no script.
- The steps are an `<ol>`, so screen readers announce how many steps there are and the position of each. The visible
  numbers are drawn from a CSS counter and hidden from assistive technology, so they always match the list order
  and are not read twice.
- The title and description are paragraphs, not headings, so the steps do not add entries to the page outline. Put
  a heading for the whole section above the list.
- Title and description are HTML-encoded.

## Styling

Side by side, the list is a grid of columns at least 12rem wide that wraps as the width shrinks. Classes:
`osirion-steps` (with `osirion-steps-vertical` when stacked), `osirion-step` for each item, `osirion-step-marker`
for the number, and `osirion-step-body`, `osirion-step-title` and `osirion-step-description`.

The number is a 2.5rem circle in `--osirion-action-primary` with text in `--osirion-action-primary-text`. The title
uses `--osirion-text-primary`, the description `--osirion-text-secondary`, and the joining line
`--osirion-border-color`.

## Related

- [Pricing table and feature comparison](/components/pricing-table)
- [Base section](/components/base-section)
- [Core section components](/components/sections)
