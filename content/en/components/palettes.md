---
id: 'theming-palettes'
order: 3
layout: docs
title: Palettes
permalink: /components/palettes
description: Named color palettes for Osirion components, each with a light and a dark variant that meets WCAG 2.2 AA, chosen in configuration or by the visitor, applied before the first paint.
author: Dejan Demonjić
date: 2026-10-16
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-palettes.jpg'
categories:
- Components
- Theming Components
- Design Systems
tags:
- blazor
- theming
- palettes
- dark-mode
- accessibility
- css-custom-properties
is_featured: false
published: true
slug: palettes
lang: en
redirect_from:
- /docs/components/theming/palettes
custom_fields:
  group: theming
seo_properties:
  title: 'Palettes: named color palettes for Blazor'
  description: 'Six named palettes with light and dark variants, checked for WCAG 2.2 contrast and applied before the first paint.'
  image: 'https://media.getosirion.com/og/components-palettes.jpg'
  canonical: 'https://getosirion.com/components/palettes'
  lang: en
  robots: index, follow
  og_title: 'Color palettes for Blazor - Osirion.Blazor'
  og_description: 'Named palettes with light and dark variants for Osirion components, checked for contrast.'
  og_image_url: 'https://media.getosirion.com/og/components-palettes.jpg'
  og_type: article
  json_ld: ''
  type: TechArticle
  twitter_card: summary_large_image
  twitter_title: 'Color palettes for Blazor - Osirion.Blazor'
  twitter_description: 'Named palettes with light and dark variants for Osirion components, checked for contrast.'
  twitter_image_url: 'https://media.getosirion.com/og/components-palettes.jpg'
  meta_tags: {}
---

# Palettes

A palette is a named set of colors for Osirion components. The mode (light, dark or system) stays separate: every
palette has a light and a dark variant, and the [theme toggle](/components/theme-toggle) switches between
them. To try the palettes on this site, see [Themes](/themes).

Package `Osirion.Blazor.Theming`; the `ThemePalette` enum is in `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Enums/ThemePalette.cs).

## The palettes

| Palette | `ThemePalette` | Light and dark | Colors |
| --- | --- | --- | --- |
| Framework | `Framework` | follows the mode | No palette: the theme files and the active framework adapter decide. The default. |
| Osirion | `Osirion` | yes | Osirion's own colors, kept even when a framework adapter is active. |
| Commit | `Commit` | yes | Inspired by GitHub's Primer colors. |
| Editor | `Editor` | yes | Inspired by Visual Studio Code's Light Modern and Dark Modern themes. |
| Studio | `Studio` | yes | Approximates Visual Studio's Light and Dark themes. |
| Studio Blue | `StudioBlue` | light; dark uses Studio's | Approximates Visual Studio's Blue theme: a light page in a dark blue menu frame. |
| Contrast | `Contrast` | yes | High contrast, inspired by Visual Studio Code's High Contrast themes. |

The names are neutral on purpose: "inspired by" describes where the colors come from, and none of those projects
endorses Osirion.

## Setup

Pick the palette visitors see in `appsettings.json`:

```json
{
  "Osirion": {
    "Theming": {
      "DefaultPalette": "Editor",
      "DefaultMode": "System"
    }
  }
}
```

Or in code:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Theming.Extensions;

builder.Services.AddOsirionTheming(theming => theming.UsePalette(ThemePalette.Editor));
```

Nothing else changes in the layout: [`OsirionStyles`](/components/osirion-styles) links the palette
stylesheet and applies the palette.

To let visitors choose, set `AllowPaletteSelection` to `true` (or call `AllowPaletteSelection()` on the builder) and
place a [`ThemePicker`](/components/theme-picker); on the server, `IThemeService.SetPalette` stores a
choice too. With the option set, every page links the palette stylesheet, so a palette chosen on one page is painted on
the next without a flash.

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `DefaultPalette` | `ThemePalette` | `Framework` | The palette before the visitor picks one. `Framework` applies none. |
| `AllowPaletteSelection` | `bool` | `false` | Links the palette stylesheet on every page. Without it, the stylesheet is linked only while a palette other than `Framework` is in use. |

## How a palette is applied

- The palette lives on `<html>` as `data-osirion-palette` (`osirion`, `commit`, `editor`, `studio`, `studio-blue` or
  `contrast`). `Framework` removes the attribute.
- The visitor's choice is kept in the `osirion-preferred-palette` cookie for one year. `OsirionStyles` reads it while
  the page is parsed, before the first paint, and again after every enhanced navigation; the server reads it through
  `IThemeService.CurrentPalette`. An unknown value is ignored and the default applies.
- The stylesheet is `_content/Osirion.Blazor.Theming/css/palettes.min.css` (about 6 KB compressed). Its selectors,
  `:root[data-osirion-palette="editor"][data-osirion-theme="dark"]` and so on, are more specific than the theme files
  and every framework adapter, so the palette's tokens win in any load order.
- A site exported to static files works the same way: the cookie is read in the browser, not only on the server.

## Contrast

Every palette variant is checked in the library's tests, against WCAG 2.2:

- Text and its background reach 4.5:1 (AA): body, secondary and muted text on the page and on raised surfaces, links,
  button labels on their buttons (also on hover), the active navigation item, status messages, form fields, and every
  syntax highlighting color on the code background.
- The Contrast palette holds all of these to 7:1 (AAA).
- The focus indicator reaches 3:1 against the page.

Where a source color misses its target, the palette keeps its hue and moves its lightness until it reaches it, so a
color can differ slightly from the theme it was taken from. Osirion's own light and dark themes are held to the same
checks.

## Code highlighting

Code blocks follow the palette. Each palette sets `--osirion-code-background` and a color per Prism token group
(`--osirion-code-keyword`, `--osirion-code-string`, `--osirion-code-comment` and so on), and its rules outrank Prism's
themes, including the accessible theme `OsirionHtmlRenderer` uses by default. Without a palette, code blocks keep the
renderer's theme.

## With a CSS framework

A palette recolors Osirion's components through the `--osirion-*` tokens. The framework still paints what it owns:
the page body under Bootstrap, the framework's own buttons and inputs, and the components an adapter styles with the
framework's variables. Under Bootstrap, the primary buttons (`.btn-primary` and `.btn-outline-primary`) follow the
palette's action colors while a palette is active. Use `Framework` when the site's colors should come from the
framework theme.

## Your own colors

To change one token of one palette, write a rule with a higher specificity in your own stylesheet:

```css
html:root[data-osirion-palette="commit"][data-osirion-theme="light"] {
    --osirion-action-primary: #0969da;
}
```

Check the contrast of anything you change. A palette of your own can follow the same pattern: set the tokens under
`:root[data-osirion-palette="your-name"][data-osirion-theme="light"]` and `...="dark"]`, and set the attribute
yourself; `ThemePalette` only knows the palettes Osirion ships.

## Sources

The colors of Commit come from Primer primitives 11.10.0 (GitHub, MIT License); those of Editor and Contrast from
Visual Studio Code's default themes (Microsoft, MIT License). The notices are in `THIRD-PARTY-NOTICES.md` in the
Osirion.Blazor.Theming package. Studio approximates Visual Studio's themes, which are not published as source.

## Related

- [Themes](/themes): try the palettes and see every color token.
- [Theming](/docs/packages/theming): setup, options and adapters.
- [OsirionStyles](/components/osirion-styles): links the stylesheets and applies mode and palette.
- [ThemePicker](/components/theme-picker): lets visitors pick a palette.
- [ThemeToggle](/components/theme-toggle): switches between light and dark.
