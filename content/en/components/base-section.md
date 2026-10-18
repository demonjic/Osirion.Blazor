---
id: 'base-section'
order: 2
layout: docs
title: OsirionBaseSection
permalink: /components/base-section
description: OsirionBaseSection renders a page section with an id, heading, description, framework container, padding and an optional background image or pattern.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-base-section.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- section
- layout
- background
- static-ssr
is_featured: true
published: true
slug: base-section
lang: en
redirect_from:
- /docs/components/core/sections/base-section
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionBaseSection: content section for Blazor'
  description: 'A Blazor page section with heading, description, framework container, padding and an optional background image or pattern.'
  image: 'https://media.getosirion.com/og/components-base-section.jpg'
  canonical: 'https://getosirion.com/components/base-section'
  lang: en
  robots: index, follow
  og_title: 'OsirionBaseSection - Osirion.Blazor'
  og_description: 'A page section with heading, container, padding and an optional background image or pattern.'
  og_image_url: 'https://media.getosirion.com/og/components-base-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionBaseSection - Osirion.Blazor'
  twitter_description: 'A page section with heading, container, padding and an optional background image or pattern.'
  twitter_image_url: 'https://media.getosirion.com/og/components-base-section.jpg'
  meta_tags: {}
---

# OsirionBaseSection

`OsirionBaseSection` renders a `<section>` with an id, an optional heading and description, your content inside the
CSS framework's container, and consistent vertical padding. Use it for the bands of a landing page so they share
spacing and alignment.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionBaseSection.razor).

```demo
base-section
```

## Usage

```razor
<OsirionBaseSection Id="why" Title="Why static SSR" Description="Pages arrive as HTML and work without script.">
    <p>Search engines and readers get the full page on the first request.</p>
</OsirionBaseSection>
```

A left-aligned section with more padding, a background image and a shadow underneath:

```razor
<OsirionBaseSection Id="newsletter"
                    Title="Release notes by email"
                    TextAlignment="Alignment.Left"
                    Padding="SectionPadding.Large"
                    BackgroundImageUrl="/assets/images/blazor-cover.png"
                    HasDivider="true">
    <a href="/subscribe">Subscribe</a>
</OsirionBaseSection>
```

With an image, the text turns white and a dark overlay (`ShowOverlay`, on by default) keeps it readable.

## Parameters

<!-- osirion:parameters OsirionBaseSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The section's content, inside the container below the heading. |
| `Id` | `string?` | `null` | The section's `id`, for in-page links; the section keeps a 3rem scroll margin so a link does not land under a sticky header. |
| `Title` | `string?` | `null` | The heading, rendered as an `h2`. Written as raw HTML without sanitizing, so pass only text you control; use `TitleContent` for text you do not control. Ignored when `TitleContent` is set. |
| `TitleContent` | `RenderFragment?` | `null` | Your own heading markup in place of `Title`. Blazor encodes any text in it. |
| `Description` | `string?` | `null` | The paragraph under the heading. Written as raw HTML without sanitizing, so pass only text you control. Shown only together with a title, and ignored when `DescriptionContent` is set. |
| `DescriptionContent` | `RenderFragment?` | `null` | Your own markup in place of `Description`. Shown only together with a title. |
| `ContainerClass` | `string?` | `null` | The class of the inner container; when it is empty the configured CSS framework's container class is used. |
| `TextAlignment` | `Alignment` | `Alignment.Center` | The alignment of the heading and the content: `Left`, `Center`, `Right` or `Justify`. |
| `BackgroundImageUrl` | `string?` | `null` | The background image, CSS-escaped and written as a `url()` to the inline style. The text turns white while it is set. |
| `BackgroundColor` | `string?` | `null` | A CSS color for the background, written to the inline style as given. |
| `TextColor` | `string?` | `null` | A CSS color for the text, written to the inline style as given. |
| `ShowOverlay` | `bool` | `true` | Whether a dark overlay is drawn over the background image so the text stays readable. Has no effect without `BackgroundImageUrl`. |
| `BackgroundPattern` | `BackgroundPatternType?` | `null` | A decorative pattern, drawn when `ShowPattern` is on. |
| `ShowPattern` | `bool` | `false` | Whether `BackgroundPattern` is drawn behind the content. |
| `Padding` | `SectionPadding` | `SectionPadding.Medium` | The vertical padding: `None`, `Small` (2rem), `Medium` (3rem) or `Large` (4rem); smaller on narrow screens. |
| `HasDivider` | `bool` | `false` | Whether a shadow under the section separates it from the next. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `<section>`. The pattern values are `Dots`, `Grid`, `Diagonal`,
`Honeycomb`, `GradientMesh`, `AnimatedGrid`, `TechWave`, `Circuit` and `DotsFade`; see
[background pattern](/components/background-pattern).

## Behavior

- The component renders plain HTML and needs no script.
- The heading block appears only when `Title` or `TitleContent` is set; a `Description` on its own is not shown.
- `Title` and `Description` are written as raw HTML without sanitizing, so you can use inline markup such as
  `<em>`. Pass only text you control; render user or content-provided text with `TitleContent` and
  `DescriptionContent`, where Blazor encodes it.
- `BackgroundImageUrl` is CSS-escaped before it is written (since 4.0), so a value cannot close the `url()` and add
  declarations. `BackgroundColor` and `TextColor` are written as given: use your own values.
- The section has `scroll-margin-top: 3rem`, so a link to `#id` does not land under a sticky header.

## Styling

The `<section>` has `osirion-section`, `osirion-section-align-{left|center|right|justify}` and
`osirion-section-padding-{none|small|medium|large}`, plus `osirion-section-with-background`,
`osirion-section-with-pattern` and `osirion-section-divider` when those options are on. Inside are
`osirion-section-overlay`, the container, `osirion-section-header` (with `osirion-section-header-{alignment}`),
`osirion-section-title`, `osirion-section-description` and `osirion-section-content`.

The container class follows the configured CSS framework:

| Framework | Container class |
| --- | --- |
| Bootstrap | `container-xxl` |
| Fluent UI | `osirion-container` |
| MudBlazor | `mud-container` |
| Radzen | `rz-container` |
| None | `osirion-container` |

Colors come from `--osirion-background-primary` and `--osirion-text-primary`, and spacing from the
`--osirion-spacing-*` tokens, so a framework adapter or your theme restyles every section at once.

## Related

- [Hero section](/components/hero-section)
- [Background pattern](/components/background-pattern)
- [Core section components](/components/sections)
