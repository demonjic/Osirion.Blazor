---
id: 'hero-section'
order: 1
layout: docs
title: HeroSection
permalink: /components/hero-section
description: HeroSection renders a page header with a title, subtitle, buttons, article metadata and a side image, background image or pattern.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-hero-section.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- hero
- landing-page
- header
- static-ssr
is_featured: true
published: true
slug: hero-section
lang: en
redirect_from:
- /docs/components/core/hero-section
- /docs/components/core/sections/hero-section
custom_fields:
  group: sections
seo_properties:
  title: 'HeroSection: page header and hero for Blazor'
  description: 'A Blazor page header with title, subtitle, buttons, article metadata and a side image, background image or pattern, rendered without script.'
  image: 'https://media.getosirion.com/og/components-hero-section.jpg'
  canonical: 'https://getosirion.com/components/hero-section'
  lang: en
  robots: index, follow
  og_title: 'HeroSection - Osirion.Blazor'
  og_description: 'A page header with title, subtitle, buttons and a side or background image, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-hero-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'HeroSection - Osirion.Blazor'
  twitter_description: 'A page header with title, subtitle, buttons and a side or background image, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-hero-section.jpg'
  meta_tags: {}
---

# HeroSection

`HeroSection` renders the header of a page: a title, a subtitle, a summary, up to two buttons and an image, either
beside the text or behind it. The CMS page components (`ArticlePage`, `HomePage`, `LandingPage`) use it for their
headers, and you can use it on your own Razor pages.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/HeroSection.razor).

```demo
hero-section
```

## Usage

```razor
<HeroSection Title="Release notes" Subtitle="What changed in each version" />
```

A landing page header with a side image, two buttons and a background pattern:

```razor
<HeroSection Title="Osirion.Blazor"
             Subtitle="SSR-first Blazor components for content sites"
             ImageUrl="/assets/images/blazor-cover.png"
             ImageAlt=""
             BackgroundPattern="BackgroundPatternType.Grid"
             PrimaryButtonText="Read the docs"
             PrimaryButtonUrl="/docs"
             SecondaryButtonText="Source on GitHub"
             SecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor" />
```

An article header with the image behind the text and the author, date and reading time:

```razor
<HeroSection Title="Static SSR with Blazor"
             Subtitle="What renders on the server, and what needs interactivity"
             ImageUrl="/assets/images/blazor-cover.png"
             UseBackgroundImage="true"
             HasOverlay="true"
             Alignment="Alignment.Center"
             ShowMetadata="true"
             Author="Dejan Demonjić"
             PublishDate="@(new DateTime(2026, 10, 11))"
             ReadTime="6"
             MinHeight="40vh" />
```

## Parameters

<!-- osirion:parameters HeroSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The main heading, rendered as an `h1` with the class `osirion-hero-title`. Ignored when `TitleContent` is set. |
| `TitleContent` | `RenderFragment?` | `null` | Your own heading markup in place of `Title`, for when the page needs another heading level. |
| `Subtitle` | `string?` | `null` | The line under the title, rendered as a paragraph. Ignored when `SubtitleContent` is set. |
| `SubtitleContent` | `RenderFragment?` | `null` | Your own markup in place of `Subtitle`. |
| `Summary` | `string?` | `null` | A longer paragraph under the subtitle. Ignored when `SummaryContent` is set. |
| `SummaryContent` | `RenderFragment?` | `null` | Your own markup in place of `Summary`. |
| `ImageUrl` | `string?` | `null` | The image, shown beside the text or, with `UseBackgroundImage`, behind it. As a background it is CSS-escaped before it is written to the inline style. |
| `ImageAlt` | `string?` | `null` | The side image's alternative text; when it is not set, `Title` is used, then `Hero image`. Pass an empty string for a decorative image. A background image has no alternative text. |
| `UseBackgroundImage` | `bool` | `false` | Whether `ImageUrl` is the section's background image instead of a side image. |
| `HasOverlay` | `bool` | `false` | Whether a 40% black overlay is drawn over a background image and the text turns white. Has no effect without `UseBackgroundImage` and `ImageUrl`. |
| `BackgroundPattern` | `BackgroundPatternType?` | `null` | A decorative pattern behind the content, drawn unless `UseBackgroundImage` is on. |
| `Alignment` | `Alignment` | `Alignment.Left` | The text alignment: `Left`, `Center` or `Right`. `Justify` is treated as `Left`. |
| `ShowPrimaryButton` | `bool` | `true` | Whether the primary button is rendered; it also needs `PrimaryButtonText` and `PrimaryButtonUrl`. |
| `ShowSecondaryButton` | `bool` | `true` | Whether the secondary button is rendered; it also needs `SecondaryButtonText` and `SecondaryButtonUrl`. |
| `PrimaryButtonText` | `string?` | `null` | The primary button's text. |
| `PrimaryButtonUrl` | `string?` | `null` | The primary button's address, written as given, so pass an address your application controls. |
| `SecondaryButtonText` | `string?` | `null` | The secondary button's text. |
| `SecondaryButtonUrl` | `string?` | `null` | The secondary button's address, written as given, so pass an address your application controls. |
| `BackgroundColor` | `string?` | `null` | A CSS color for the section's background, written to the inline style as given. |
| `TextColor` | `string?` | `null` | A CSS color for the section's text, written to the inline style as given. |
| `MinHeight` | `string` | `"60vh"` | The section's minimum height as a CSS length, written to the inline style as given. |
| `Author` | `string?` | `null` | The author name shown in the metadata line when `ShowMetadata` is on. |
| `PublishDate` | `DateTime?` | `null` | The publication date shown in the metadata line when `ShowMetadata` is on. |
| `Culture` | `CultureInfo?` | `null` | The culture `PublishDate` is written in, normally the language of the page. When `null`, the current culture is used. |
| `ReadTime` | `string?` | `null` | The reading time in minutes, shown in the metadata line when `ShowMetadata` is on: `6` is shown as `6 min`. |
| `ShowMetadata` | `bool` | `false` | Whether `Author`, `PublishDate` and `ReadTime` are shown under the text. |
| `ImagePosition` | `Alignment` | `Alignment.Right` | The side of the text the side image sits on. `Left` and `Justify` are treated as `Right`, so the image is always on the right. |
| `Variant` | `HeroVariant` | `HeroVariant.Hero` | `Hero`, `Jumbotron` or `Minimal`: changes the spacing, the text sizes and the side image's size. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content under the buttons, in a `div` with the class `osirion-hero-additional`. |
| `HasDivider` | `bool` | `true` | Whether a shadow in the primary color is drawn along the section's top edge. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `<section>`.

## Behavior

- The component renders plain HTML and needs no script.
- `Title` is an `<h1>`. A page should have one, so when the page already has its main heading, pass `TitleContent`
  with the level you need.
- Text parameters are HTML-encoded. Button addresses are written as given, so pass addresses you control.
- `ImageUrl` is CSS-escaped when it becomes a background image (since 4.0), so a front matter `featured_image`
  cannot close the `url()` and add declarations.
- The side image carries `width` and `height` attributes for its variant (600 by 315 for `Hero`, 600 by 600 for
  `Jumbotron`, 315 by 315 for `Minimal`) to avoid layout shift, and `fetchpriority="high"` because it is usually the
  largest element in view.
- `Culture` sets the language of the date in the metadata line; without it the current culture is used.
- A button is rendered only when it is shown and has both its text and its address.
- The buttons take the configured CSS framework's primary and secondary button classes in the large size
  (`btn btn-primary btn-lg` and `btn btn-secondary btn-lg` under Bootstrap). The secondary classes come from
  `ButtonClasses.Secondary` in `Osirion.Blazor.Core.Utils`, like the secondary button of
  [OsirionCallToActionSection](/components/call-to-action-section), so the two sections' buttons look the same.
  Up to 4.2.0 the secondary button had the outlined primary classes.
- A background image has no alternative text, so use it for decoration only. The side image has `ImageAlt`.
- A background pattern is drawn unless `UseBackgroundImage` is on.

## Styling

The `<section>` has `osirion-hero-section`, `osirion-hero-variant-{hero|jumbotron|minimal}` and
`osirion-hero-align-{left|center|right}`, plus `osirion-hero-with-background` and `osirion-hero-with-overlay`, or
`osirion-hero-with-side-image`, depending on the image. Inside are `osirion-hero-title`, `osirion-hero-subtitle`,
`osirion-hero-summary`, `osirion-hero-buttons`, `osirion-hero-additional` and `osirion-hero-image-container`. Both
buttons carry `osirion-hero-button`; the primary button adds `osirion-hero-button-primary` and the secondary button
`osirion-hero-button-secondary`. Up to 4.2.0 the secondary button carried `osirion-hero-button-primary` too, so a
rule written for both buttons through that class should select `osirion-hero-button` instead.

The text sizes scale with the window between a minimum and a maximum, set as custom properties on the section:
`--hero-title-min`, `--hero-title-pref`, `--hero-title-max` and the same three for `subtitle` and `summary`. Set them
through `Style`:

```razor
<HeroSection Title="Changelog" Style="--hero-title-max: 3rem; --hero-subtitle-max: 1.125rem;" />
```

Colors come from `--osirion-background-primary` and `--osirion-text-primary`, the divider from
`--osirion-color-primary-600`.

## Related

- [Base section](/components/base-section)
- [Article metadata](/components/article-metadata)
- [Background pattern](/components/background-pattern)
- [Core section components](/components/sections)
