---
id: 'split-section'
order: 13
layout: docs
title: OsirionSplitSection
permalink: /components/split-section
description: OsirionSplitSection places copy beside an aside, such as an image or a sample record, and stacks the aside under the copy on narrow screens.
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-split-section.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- split-section
- sections
- layout
- static-ssr
is_featured: false
published: true
slug: split-section
lang: en
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionSplitSection: copy beside an aside for Blazor'
  description: 'Copy beside an image, a card or a sample in two columns that stack on narrow screens, with the aside on either side, rendered without script.'
  image: 'https://media.getosirion.com/og/components-split-section.jpg'
  canonical: 'https://getosirion.com/components/split-section'
  lang: en
  robots: index, follow
  og_title: 'OsirionSplitSection - Osirion.Blazor'
  og_description: 'Copy beside an aside in two columns that stack on narrow screens, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-split-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionSplitSection - Osirion.Blazor'
  twitter_description: 'Copy beside an aside in two columns that stack on narrow screens, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-split-section.jpg'
  meta_tags: {}
---

# OsirionSplitSection

`OsirionSplitSection` places copy beside an aside, such as a paragraph and a list next to an image, a card or a sample
record. The two sit in equal columns from 768px, and below that the aside stacks under the copy. It renders without
script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionSplitSection.razor). Available from 4.3.

```demo
split-section
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionBaseSection Id="how-it-works" Eyebrow="How it works" Title="Every decision keeps its reasons">
    <OsirionSplitSection AsidePosition="SplitAsidePosition.Start">
        <Content>
            <p>Each request is checked against your rules.</p>
        </Content>
        <Aside>
            <img src="/images/rules.png" alt="The rules editor" width="640" height="400" />
        </Aside>
    </OsirionSplitSection>
</OsirionBaseSection>
```

The component has no heading or band of its own: put it in an [OsirionBaseSection](/components/base-section) for the
heading, the eyebrow and the band.

## Parameters

<!-- osirion:parameters OsirionSplitSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Content` | `RenderFragment?` | `null` | The copy: text, lists and buttons. It comes first in reading order. |
| `Aside` | `RenderFragment?` | `null` | The block beside the copy, such as an image, a card or a code sample. It comes after the copy in reading order and stacks under it on narrow screens. |
| `AsidePosition` | `SplitAsidePosition` | `SplitAsidePosition.End` | The side the aside sits on when both columns show: `End` (the default, on the right in a left-to-right page) or `Start`. The reading order stays copy first either way. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the root `<div>`.

## Behavior

- The component renders plain HTML and needs no script.
- The copy always comes first in the markup, so reading and keyboard order run copy then aside on every screen.
  `AsidePosition` moves only the aside's column: `End` (the default) puts it on the right in a left-to-right page,
  `Start` on the left.
- Below 768px the columns stack, the aside under the copy.
- With only one slot filled, it spans both columns.

## Styling

Classes: `osirion-split-section` (with `osirion-split-section-aside-start` when the aside is at the start),
`osirion-split-section-content` and `osirion-split-section-aside`. The columns are vertically centered, with a gap of
`--osirion-spacing-8` when stacked and `--osirion-spacing-12` side by side.

## Related

- [Base section](/components/base-section), the band and heading around it
- [Record card](/components/record-card), a sample record for the aside
- [Checklist](/components/checklist), short points for the copy
- [Sections](/components/sections)
