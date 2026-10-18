---
id: 'theme-picker-component'
order: 4
layout: docs
title: ThemePicker
permalink: /components/theme-picker
description: ThemePicker renders a labelled list from which visitors pick a color palette; the choice applies at once and is remembered in a cookie.
author: Dejan Demonjić
date: 2026-10-16
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-theme-picker.jpg'
categories:
- Components
- Theming Components
- Styling
tags:
- blazor
- theme-picker
- theming
- palettes
- accessibility
is_featured: false
published: true
slug: theme-picker
lang: en
redirect_from:
- /docs/components/theming/theme-picker
custom_fields:
  group: theming
seo_properties:
  title: 'ThemePicker: color palette picker for Blazor'
  description: 'Let visitors pick a color palette with a labelled list that works under static SSR and on static sites.'
  image: 'https://media.getosirion.com/og/components-theme-picker.jpg'
  canonical: 'https://getosirion.com/components/theme-picker'
  lang: en
  robots: index, follow
  og_title: 'ThemePicker Component - Osirion.Blazor'
  og_description: 'A labelled list from which visitors pick a color palette, applied at once and remembered.'
  og_image_url: 'https://media.getosirion.com/og/components-theme-picker.jpg'
  og_type: article
  json_ld: ''
  type: TechArticle
  twitter_card: summary_large_image
  twitter_title: 'ThemePicker Component - Osirion.Blazor'
  twitter_description: 'A labelled list from which visitors pick a color palette, applied at once and remembered.'
  twitter_image_url: 'https://media.getosirion.com/og/components-theme-picker.jpg'
  meta_tags: {}
---

# ThemePicker

`ThemePicker` renders a labelled list of [palettes](/components/palettes). Choosing one applies it at once
and keeps it in the `osirion-preferred-palette` cookie, so the next page, and the next visit, use it too. Light and dark
stay with [`ThemeToggle`](/components/theme-toggle); the two sit well side by side. The demo below changes this
site's own palette, which is the point of the component, and your choice stays in your browser.

Package `Osirion.Blazor.Theming`, namespace `Osirion.Blazor.Theming.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Theming/Components/ThemePicker.razor).

```demo
theme-picker
```

## Usage

Allow palette selection, so every page links the palette stylesheet and a chosen palette is painted with the first
frame:

```json
{
  "Osirion": {
    "Theming": {
      "AllowPaletteSelection": true
    }
  }
}
```

Then place the picker, for example next to the toggle in a menu:

```razor
@using Osirion.Blazor.Theming.Components

<ThemePicker />
<ThemeToggle />
```

Offer fewer palettes, in your order, and rename the option that applies none:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Theming.Components

<ThemePicker Label="Colors"
             FrameworkLabel="Site colors"
             Palettes="@([ThemePalette.Framework, ThemePalette.Editor, ThemePalette.Contrast])" />
```

## Parameters

<!-- osirion:parameters ThemePicker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Label` | `string` | `"Palette"` | The visible text of the list's label. |
| `Palettes` | `IReadOnlyList<ThemePalette>?` | `null` | The palettes offered, in this order. When `null`, every palette is offered. Duplicates and values outside `ThemePalette` are dropped. The palette the visitor already uses is offered even when it is not listed (first, when it is missing), so the list always shows the palette in effect. |
| `FrameworkLabel` | `string` | `"Default"` | The text of the `ThemePalette.Framework` option, which applies no palette and shows the site's own colors. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class`, `Style` and any extra attributes go on the outer `<label>`. `Theme`, inherited from the component base class,
is not written to the markup.

## Behavior

- The server renders the list with the visitor's palette selected (`IThemeService.CurrentPalette`). On a page exported to
  static files, or one cached before the visitor chose, the script selects the palette from the cookie when the page
  loads and after every enhanced navigation.
- Choosing an option sets `data-osirion-palette` on `<html>` (or removes it for `Framework`), writes the cookie for one
  year (`SameSite=Lax`, `Secure` on HTTPS) and keeps every picker on the page in step. When the page has no palette
  stylesheet (`AllowPaletteSelection` off), the script adds it.
- No interactive render mode is needed. The script, `theme-picker.js`, is loaded once through the BlazorJSComponents
  script loader (registered by `AddOsirion`, or by `builder.Services.AddJSComponents()` next to `AddOsirionTheming`),
  and listens on the document, so pickers that enhanced navigation adds
  work without further setup. It writes no inline script and no event handler attributes.
- Without JavaScript the list renders but choosing does nothing, as with `ThemeToggle`.

### Accessibility

The list is a native `<select>` inside its `<label>`, so it has an accessible name, works with the keyboard (arrow keys
change the palette) and needs no id, which keeps several pickers on one page valid. Its focus outline uses
`--osirion-focus-ring-color`. The label takes the color of its container, so it stays readable in a menu with its own
background, such as the Studio Blue frame.

## Styling

| Class | Element |
| --- | --- |
| `osirion-theme-picker` | The outer `<label>` |
| `osirion-theme-picker-label` | The label text |
| `osirion-theme-picker-select` | The `<select>`; it reads `--osirion-input-background`, `--osirion-input-text`, `--osirion-input-border` and `--osirion-input-radius` |

To hide the label visually but keep it for screen readers, pass a `Class` and give
`.your-class .osirion-theme-picker-label` the same rules as Osirion's `osirion-visually-hidden` class in your stylesheet.

## Related

- [Palettes](/components/palettes): the palettes, their contrast checks and how they are applied.
- [ThemeToggle](/components/theme-toggle): switches between light and dark.
- [OsirionStyles](/components/osirion-styles): links the palette stylesheet.
