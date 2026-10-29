---
id: 'call-to-action-section'
order: 12
layout: docs
title: OsirionCallToActionSection
permalink: /components/call-to-action-section
description: OsirionCallToActionSection closes a page with a heading, a short text and up to two buttons, in a labelled section rendered without script.
author: Dejan Demonjić
date: 2026-10-23
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-call-to-action-section.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- call-to-action
- sections
- accessibility
- static-ssr
is_featured: true
published: true
slug: call-to-action-section
lang: en
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionCallToActionSection: a closing call to action for Blazor'
  description: 'A closing call to action for Blazor pages: a heading, a short text and up to two buttons in your CSS framework, rendered without script under static SSR.'
  image: 'https://media.getosirion.com/og/components-call-to-action-section.jpg'
  canonical: 'https://getosirion.com/components/call-to-action-section'
  lang: en
  robots: index, follow
  og_title: 'OsirionCallToActionSection - Osirion.Blazor'
  og_description: 'A heading, a short text and up to two buttons that close a page, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-call-to-action-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionCallToActionSection - Osirion.Blazor'
  twitter_description: 'A heading, a short text and up to two buttons that close a page, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-call-to-action-section.jpg'
  meta_tags: {}
---

# OsirionCallToActionSection

`OsirionCallToActionSection` closes a page with a heading, a short text and up to two links styled as buttons, such
as "Get started" and "Browse components". It renders a labelled `<section>` as plain HTML without script, and its
buttons take the classes of the configured CSS framework.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionCallToActionSection.razor). Available from 4.3.

```demo
call-to-action-section
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionCallToActionSection Title="Ready to build your site?"
                            Description="Install the package and publish your first page."
                            PrimaryButtonText="Get started"
                            PrimaryButtonUrl="/docs/getting-started"
                            SecondaryButtonText="Browse components"
                            SecondaryButtonUrl="/components" />
```

Content inside the tag goes between the text and the buttons, for example a short note:

```razor
<OsirionCallToActionSection Title="Questions before you start?"
                            PrimaryButtonText="Write to us"
                            PrimaryButtonUrl="mailto:hello@example.com">
    <p>No account needed.</p>
</OsirionCallToActionSection>
```

The heading is an `h2`, which fits a section under the page's `h1`. Inside a section that has its own `h2`, set
`HeadingLevel="3"` so the outline stays in order.

## Parameters

<!-- osirion:parameters OsirionCallToActionSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading, rendered at `HeadingLevel`; it also names the section for screen readers. With an empty value no heading is rendered and the section has no accessible name. |
| `Description` | `string?` | `null` | The short text under the heading, as a paragraph; nothing is rendered when it is empty. |
| `PrimaryButtonText` | `string?` | `null` | The label of the primary button. The button is rendered only with a label and a safe `PrimaryButtonUrl`. |
| `PrimaryButtonUrl` | `string?` | `null` | Where the primary button goes: a path on the site or an http, https, mailto or tel address. Any other address, such as `javascript:`, leaves the button out. |
| `SecondaryButtonText` | `string?` | `null` | The label of the secondary button, rendered after the primary one. The button is rendered only with a label and a safe `SecondaryButtonUrl`. |
| `SecondaryButtonUrl` | `string?` | `null` | Where the secondary button goes, under the same rules as `PrimaryButtonUrl`. |
| `HeadingLevel` | `int` | `2` | The level of the heading, `1` to `6` (`h1` to `h6`). It defaults to `2`, a section under the page's own `h1`. A value outside the range is moved to the nearest level. The heading keeps its look at every level. |
| `ChildContent` | `RenderFragment?` | `null` | Other content, rendered after the description and before the buttons. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the `<section>`, so an `id` set there makes it a target for in-page links.

## Behavior

- The component renders plain HTML and needs no script.
- With a `Title`, the `<section>` has `aria-labelledby` pointing at the heading, so screen readers list it as a
  region named by its heading. Each instance generates the heading's `id`, so two sections on a page never repeat
  one. Without a title there is no heading and the section has no accessible name.
- `HeadingLevel` sets `h1` to `h6`. A value outside 1 to 6 moves to the nearest level, and the heading looks the
  same at every level.
- Each button is a plain `<a>`, rendered only when it has a label and a safe address: a path starting with `/` (not
  `//` or `/\`), or an `http`, `https` or `mailto` address. Anything else, such as a `javascript:` URL, leaves that
  button out. The primary button gets the CSS framework's primary button class and the secondary button its
  secondary class, `btn btn-primary` and `btn btn-secondary` under Bootstrap. They come from `ButtonClasses.Primary`
  and `ButtonClasses.Secondary` in `Osirion.Blazor.Core.Utils`, like the buttons of
  [HeroSection](/components/hero-section), so both sections' buttons look the same.
- `ChildContent` is rendered after the description and before the buttons.
- When there is nothing to show (no title, no description, no child content and no button that passes the rules
  above), the component renders nothing.
- `Title`, `Description` and the button labels are HTML-encoded.

## Styling

The section is a panel in `--osirion-background-secondary` with a 1px `--osirion-border-color` border,
`--osirion-radius-lg` corners and centered text. It sets no outer margin, so space it with the page layout or
`Class`. Classes: `osirion-call-to-action-section`, `osirion-call-to-action-title`,
`osirion-call-to-action-description`, `osirion-call-to-action-content` (the wrapper of `ChildContent`),
`osirion-call-to-action-buttons`, and on each button `osirion-call-to-action-button` with
`osirion-call-to-action-button-primary` or `osirion-call-to-action-button-secondary`.

The heading uses `--osirion-text-primary` at `--osirion-font-size-3xl` and the description `--osirion-text-secondary`.
The Osirion light and dark themes and every palette hold both to at least 4.5:1 on the secondary background. The
buttons keep the look of the CSS framework's buttons. Below 640px the padding and the heading get smaller.

## Related

- [Hero section](/components/hero-section), which opens a page
- [Pricing table and feature comparison](/components/pricing-table)
- [Sections](/components/sections)
