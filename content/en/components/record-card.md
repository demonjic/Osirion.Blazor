---
id: 'record-card'
order: 7
layout: docs
title: OsirionRecordCard
permalink: /components/record-card
description: OsirionRecordCard frames a sample record, such as an order, a ticket or a decision, with a status, labelled rows and a caption.
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-record-card.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- blazor
- record-card
- cards
- accessibility
- static-ssr
is_featured: false
published: true
slug: record-card
lang: en
custom_fields:
  group: cards
seo_properties:
  title: 'OsirionRecordCard: a sample record card for Blazor'
  description: 'A framed sample record for Blazor product pages: a title, a status badge with a tone, labelled rows and a caption, rendered without script.'
  image: 'https://media.getosirion.com/og/components-record-card.jpg'
  canonical: 'https://getosirion.com/components/record-card'
  lang: en
  robots: index, follow
  og_title: 'OsirionRecordCard - Osirion.Blazor'
  og_description: 'A framed sample record with a status, labelled rows and a caption, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-record-card.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionRecordCard - Osirion.Blazor'
  twitter_description: 'A framed sample record with a status, labelled rows and a caption, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-record-card.jpg'
  meta_tags: {}
---

# OsirionRecordCard

`OsirionRecordCard` frames a sample record, such as an order, a ticket or a decision, for a page that shows what a
product produces: a title, a status badge with a tone, labelled rows and a caption such as "Sample data". It renders
a `<figure>` with a description list, without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionRecordCard.razor). Available from 4.3.

```demo
record-card
```

## Usage

Rows are `RecordCardRow` records from `Osirion.Blazor.Core.Models`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionRecordCard Title="Order A-1042"
                   Status="Approved"
                   StatusTone="RecordStatusTone.Positive"
                   Rows="@rows"
                   Footnote="Sample data" />

@code {
    private readonly RecordCardRow[] rows =
    [
        new("Customer", "Example Ltd"),
        new("Amount", "1,250.00"),
    ];
}
```

The title is an `h3`, which fits a card inside a section that has its own `h2`. Set `HeadingLevel` to keep the
outline in order elsewhere.

## Parameters

<!-- osirion:parameters OsirionRecordCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The record's name, for example `Order A-1042`, as a heading at `HeadingLevel`. |
| `HeadingLevel` | `int` | `3` | The level of the title, `1` to `6` (`h1` to `h6`). It defaults to `3`, a card inside a section that has its own `h2`. A value outside the range is moved to the nearest level. The title keeps its look at every level. |
| `Status` | `string?` | `null` | The record's status as text, for example `Approved`, shown as a badge beside the title. The text carries the meaning; `StatusTone` only colors it. |
| `StatusTone` | `RecordStatusTone` | `RecordStatusTone.Neutral` | The color of the status: `Neutral` (the default), `Positive`, `Negative` or `Warning`. |
| `Rows` | `IReadOnlyList<RecordCardRow>` | `[]` | The labelled values, in order, rendered as a description list. |
| `Footnote` | `string?` | `null` | A short note under the rows, for example `Sample data`, rendered as the figure's caption. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `<figure>`.

`RecordCardRow(string Label, string Value)` is a record: the label is written as a `<dt>` and the value as its `<dd>`.

## Behavior

- The component renders plain HTML and needs no script.
- The card is a `<figure>`; `Footnote` is its `<figcaption>`, which names the figure for screen readers.
- The rows are a `<dl>`, each label and value grouped in a `<div>`, so screen readers announce them as terms and
  their descriptions.
- The status is text in a badge. `StatusTone` (`Neutral`, `Positive`, `Negative` or `Warning`) only colors the badge,
  so the status never rests on color alone; the dot in the badge is hidden from assistive technology.
- `HeadingLevel` sets `h1` to `h6`. A value outside 1 to 6 moves to the nearest level, and the title looks the same at
  every level.
- With no title, status, rows or footnote, the component renders nothing. Every text is HTML-encoded.

## Styling

The card uses `--osirion-card-background`, a 1px `--osirion-card-border-color` border,
`--osirion-card-border-radius` corners and the `--osirion-shadow-md` shadow. A positive status uses the
`--osirion-status-success-*` tokens, a negative one `--osirion-status-danger-*` and a warning `--osirion-status-warning-*`
(background, border and text); a neutral one `--osirion-background-tertiary` and `--osirion-text-secondary`. Below
480px each label sits above its value.

Classes: `osirion-record-card`, `osirion-record-card-header`, `osirion-record-card-title`,
`osirion-record-card-status` with `osirion-record-card-status-neutral`, `-positive`, `-negative` or `-warning`,
`osirion-record-card-status-mark`, `osirion-record-card-rows`, `osirion-record-card-row`, `osirion-record-card-label`,
`osirion-record-card-value` and `osirion-record-card-footnote`.

## Related

- [Cards](/components/cards)
