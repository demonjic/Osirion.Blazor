---
id: 'page-heading'
order: 11
layout: docs
title: OsirionPageHeading
permalink: /components/page-heading
description: OsirionPageHeading renders a page title as the h1 with an optional sentence below it. Parameters, behavior and styling.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-page-heading.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- heading
- sections
- osirion-blazor-core
- static-ssr
is_featured: true
published: true
slug: page-heading
lang: en
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionPageHeading: page title heading for Blazor'
  description: 'OsirionPageHeading renders a page title as the h1 with an optional sentence below it. Parameters, behavior and styling.'
  image: 'https://media.getosirion.com/og/components-page-heading.jpg'
  canonical: 'https://getosirion.com/components/page-heading'
  lang: en
  robots: index, follow
  og_title: 'OsirionPageHeading - Osirion.Blazor'
  og_description: 'OsirionPageHeading renders a page title as the h1 with an optional sentence below it. Parameters, behavior and styling.'
  og_image_url: 'https://media.getosirion.com/og/components-page-heading.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionPageHeading - Osirion.Blazor'
  twitter_description: 'OsirionPageHeading renders a page title as the h1 with an optional sentence below it. Parameters, behavior and styling.'
  twitter_image_url: 'https://media.getosirion.com/og/components-page-heading.jpg'
  meta_tags: {}
---

# OsirionPageHeading

`OsirionPageHeading` renders the title of a page as its `h1`, with an optional sentence below it, in the same readable width as the
page body. Use it for pages that have no [hero](/components/hero-section), such as a privacy policy.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionPageHeading.razor).

This page has no live demo. The component always renders an `h1`, and a copy on this page would give it a second one.

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionPageHeading Title="Privacy policy" Description="What this site stores and why." />
```

Content after the description, such as a date:

```razor
<OsirionPageHeading Title="Release notes" Description="What changed in each version.">
    <p>Updated 17 October 2026</p>
</OsirionPageHeading>
```

## Parameters

<!-- osirion:parameters OsirionPageHeading -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | The page title, rendered as the page's `h1`. Required. |
| `Description` | `string?` | `null` | The sentence under the title; nothing is rendered when it is empty. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content after the description, for example badges or a date. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `header`.

## Behavior

- The component renders a `header` with the `h1` and, when `Description` is set, a paragraph. It needs no script.
- A page made of [base sections](/components/base-section) has only `h2` headings, so it needs a heading like this one for its `h1`.
- The CMS pages `AboutPage` and `ContactPage` render it when `ShowHero` is off.
- `Title` and `Description` are HTML-encoded.

## Styling

The `header` has `osirion-page-heading` and is centered with a maximum width of `--osirion-container-lg` (default `960px`). Inside are
`osirion-page-heading-title` and `osirion-page-heading-description`, which is at most `65ch` wide. Colors come from
`--osirion-text-primary` and `--osirion-text-secondary`.

## Related

- [Hero section](/components/hero-section)
- [Base section](/components/base-section)
- [Sections](/components/sections)
