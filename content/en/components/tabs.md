---
id: 'osirion-tabs'
order: 3
layout: docs
title: OsirionTabs
permalink: /components/tabs
description: "OsirionTabs and OsirionTab show alternative content behind tabs that switch without script and work with the keyboard, under static SSR."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-tabs.jpg'
categories:
- Components
- Core Components
- Content
tags:
- tabs
- static-ssr
- accessibility
- code-samples
slug: tabs
lang: en
published: true
redirect_from:
- /docs/components/core/content/tabs
custom_fields:
  group: content
seo_properties:
  title: 'OsirionTabs: tabs without JavaScript for Blazor'
  description: 'OsirionTabs and OsirionTab show alternative content behind tabs that switch without script and work with the keyboard, under static SSR.'
  image: 'https://media.getosirion.com/og/components-tabs.jpg'
  canonical: 'https://getosirion.com/components/tabs'
  lang: en
  robots: index, follow
  og_title: 'OsirionTabs - Osirion.Blazor'
  og_description: 'Tabs that switch without script, with keyboard support, under static SSR.'
  og_image_url: 'https://media.getosirion.com/og/components-tabs.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionTabs - Osirion.Blazor'
  twitter_description: 'Tabs that switch without script, with keyboard support, under static SSR.'
  twitter_image_url: 'https://media.getosirion.com/og/components-tabs.jpg'
  meta_tags: {}
---

# OsirionTabs

`OsirionTabs` shows one of several panels at a time, with a row of tabs to switch between them. Use it for
alternative versions of the same content, such as one install command per package manager. The tabs switch without
script, so they work under static server-side rendering.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionTabs.razor). Available from 4.0.

```demo
install-tabs
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionTabs AriaLabel="Install command">
    <OsirionTab Title=".NET CLI">
        <pre><code>dotnet add package Osirion.Blazor</code></pre>
    </OsirionTab>
    <OsirionTab Title="Package Manager">
        <pre><code>Install-Package Osirion.Blazor</code></pre>
    </OsirionTab>
</OsirionTabs>
```

The first tab is selected when the page renders. To start on another one and give the ids a stable prefix:

```razor
<OsirionTabs AriaLabel="Install command" Id="install" SelectedIndex="2">
    <OsirionTab Title=".NET CLI">
        <pre><code>dotnet add package Osirion.Blazor</code></pre>
    </OsirionTab>
    <OsirionTab Title="Package Manager">
        <pre><code>Install-Package Osirion.Blazor</code></pre>
    </OsirionTab>
    <OsirionTab Title="PackageReference">
        <pre><code>&lt;PackageReference Include="Osirion.Blazor" Version="4.0.0" /&gt;</code></pre>
    </OsirionTab>
</OsirionTabs>
```

With `Id="install"`, the third tab's radio button is `install-2`, its label `install-2-label` and its panel
`install-2-panel`.

## Parameters

### OsirionTabs

<!-- osirion:parameters OsirionTabs -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The `OsirionTab` components. Each tab takes its position from the order in which the tabs render. |
| `SelectedIndex` | `int` | `0` | The zero-based index of the tab selected when the page renders. An index with no tab leaves every panel hidden. It sets only the starting choice: the visitor's later choices are not written back. |
| `Id` | `string?` | `null` | The radio group name, also the prefix of each tab's ids: `{Id}-0` for the radio button, `{Id}-0-label` for the label and `{Id}-0-panel` for the panel (then `-1` and so on). Defaults to a value generated for each instance; set it for stable ids, for example to link to a tab, and use a different value for each tab set on a page. |
| `AriaLabel` | `string?` | `null` | The accessible name of the tab group, for example "Package manager". Without it the group has no name. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the container `<div>`. `Class` is added to the container. `Style` is accepted but the
component does not write it.

### OsirionTab

<!-- osirion:parameters OsirionTab -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | The tab's label, shown in the tab row. Required. |
| `ChildContent` | `RenderFragment?` | `null` | The panel content, shown while this tab is selected. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes and `Class` go on the panel `<div>`. `Style` is accepted but the component does not write it.

## Behavior

- Each `OsirionTab` renders a radio button, its `<label>` and its panel. The radio buttons share one group name,
  and CSS shows the panel that follows the checked one. No script runs, in static SSR or in an interactive render
  mode.
- Keyboard: Tab moves focus into the group and the arrow keys move between tabs, which is the native radio button
  behavior. The radio button itself is visually hidden; its label shows a focus ring.
- Screen readers announce each tab as a radio button with its position ("2 of 3"). Each panel has `role="region"`
  and is labelled by its tab's label. The container has `role="group"` with `AriaLabel` as its name, so set
  `AriaLabel` to describe the choice.
- Tabs take their index in the order they first render. Ids stay the same when the parent renders again.
- Two `OsirionTabs` on one page get different generated group names. If you set `Id` yourself, use a different value
  for each instance.
- Printed pages show every panel under the tab row, because paper cannot switch tabs.
- An `OsirionTab` placed outside an `OsirionTabs` throws `InvalidOperationException`.
- The browser holds the visitor's choice. The component keeps no state of its own and never reports which tab is
  selected: `SelectedIndex` is only the starting choice.

## Styling

| Class | Element |
| --- | --- |
| `osirion-tabs` | The container, a wrapping flex row |
| `osirion-tabs-input` | The visually hidden radio button |
| `osirion-tabs-label` | The visible tab |
| `osirion-tabs-panel` | A panel |

The selected tab's label is underlined in `--osirion-action-primary`. Labels use `--osirion-text-secondary`, and
`--osirion-text-primary` when selected or hovered; the line under the tab row is `--osirion-border-color`, and the
focus ring uses `--osirion-focus-ring-width` and `--osirion-focus-ring-color`. Every token has a fallback value, and
a CSS framework adapter that sets them restyles the tabs with the rest of the site. Color transitions are turned off
under `prefers-reduced-motion: reduce`.

## Related

- [Accordion and FAQ](/components/accordion-and-faq)
- [Callouts](/components/callout)
- [Content components](/components/content)
