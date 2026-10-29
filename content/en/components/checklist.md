---
id: 'checklist'
order: 11
layout: docs
title: OsirionChecklist
permalink: /components/checklist
description: OsirionChecklist renders short phrases behind check marks, with optional one-line descriptions, as a plain list under an optional heading.
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-checklist.jpg'
categories:
- Components
- Core Components
- Content
tags:
- blazor
- checklist
- lists
- accessibility
- static-ssr
is_featured: false
published: true
slug: checklist
lang: en
custom_fields:
  group: content
seo_properties:
  title: 'OsirionChecklist: a check mark list for Blazor'
  description: 'A list of short phrases behind check marks for Blazor pages, with optional descriptions and a heading, rendered without script under static SSR.'
  image: 'https://media.getosirion.com/og/components-checklist.jpg'
  canonical: 'https://getosirion.com/components/checklist'
  lang: en
  robots: index, follow
  og_title: 'OsirionChecklist - Osirion.Blazor'
  og_description: 'Short phrases behind check marks, with optional descriptions, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-checklist.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionChecklist - Osirion.Blazor'
  twitter_description: 'Short phrases behind check marks, with optional descriptions, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-checklist.jpg'
  meta_tags: {}
---

# OsirionChecklist

`OsirionChecklist` renders short phrases, each behind a check mark and with an optional line of description, such as
what a plan includes or what a product does. It renders a plain `<ul>` without script, under an optional heading that
names the list.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionChecklist.razor). Available from 4.3.

```demo
checklist
```

## Usage

Items are `ChecklistItem` records from `Osirion.Blazor.Core.Models`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionChecklist Title="Included in every plan" Items="@items" />

@code {
    private readonly ChecklistItem[] items =
    [
        new("Static SSR", "Every page arrives as complete HTML."),
        new("Light and dark themes"),
    ];
}
```

The heading is an `h3`, which fits a list inside a section that has its own `h2`. Set `HeadingLevel` to keep the
outline in order elsewhere. [HeroSection](/components/hero-section) shows its `Highlights` and
[OsirionFeatureCard](/components/feature-card) its `Items` through this component.

## Parameters

<!-- osirion:parameters OsirionChecklist -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading above the list, rendered at `HeadingLevel`; it also names the list for screen readers. With an empty value no heading is rendered. |
| `HeadingLevel` | `int` | `3` | The level of the heading, `1` to `6` (`h1` to `h6`). It defaults to `3`, a list inside a section that has its own `h2`. A value outside the range is moved to the nearest level. The heading keeps its look at every level. |
| `Items` | `IReadOnlyList<ChecklistItem>` | `[]` | The items, in order. With none, only the heading is rendered. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the root `<div>`.

`ChecklistItem(string Title, string? Description = null)` is a record: `Title` is the phrase and `Description` an
optional line of plain text under it.

## Behavior

- The component renders plain HTML and needs no script.
- The items are a `<ul>` with `role="list"`, which keeps the list semantics that some browsers drop from a list
  without bullets, so screen readers announce how many items there are.
- With a `Title`, the list has `aria-labelledby` pointing at the heading. Each instance generates the heading's `id`,
  so two lists on a page never repeat one.
- Each check mark is an inline `<svg>` with `aria-hidden="true"` and `focusable="false"`: the phrase alone carries
  the meaning.
- `HeadingLevel` sets `h1` to `h6`. A value outside 1 to 6 moves to the nearest level, and the heading looks the same
  at every level.
- With no title and no items, the component renders nothing. Title, phrases and descriptions are HTML-encoded.

## Styling

Classes: `osirion-checklist` (the root), `osirion-checklist-title`, `osirion-checklist-list`, `osirion-checklist-item`,
`osirion-checklist-mark`, `osirion-checklist-text`, `osirion-checklist-item-title` and
`osirion-checklist-item-description`.

The check mark is drawn in `--osirion-link-color` (`CanvasText` in forced colors mode), the phrase inherits the
text color around it and the description uses `--osirion-text-secondary`. The items stay left-aligned inside a
centered block.

## Related

- [Hero section](/components/hero-section), whose `Highlights` render as a checklist
- [Feature card](/components/feature-card), whose `Items` render as a checklist
- [Steps](/components/steps), for numbered steps
