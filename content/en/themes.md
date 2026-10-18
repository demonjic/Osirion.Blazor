---
id: 'themes'
order: 0
layout: docs
title: Themes
permalink: /themes
description: "Six color palettes for Osirion.Blazor, in light and dark. Try them on this site, see every color token they set, and make your own."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-18
featured_image: 'https://media.getosirion.com/og/themes.jpg'
categories:
- Documentation
- Theming
tags:
- palettes
- dark-mode
- css-custom-properties
- accessibility
published: true
slug: themes
lang: en
custom_fields:
  group: theming
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/themes.jpg'
  title: 'Blazor themes and color palettes'
  description: 'Light and dark palettes for Osirion.Blazor components, checked against WCAG 2.2 AA, with every color token they set and how to make your own.'
---

# Themes

## The palettes

| Palette | Variants | Colors |
| --- | --- | --- |
| Framework | follows the mode | Your CSS framework's, through its adapter (Bootstrap, Fluent UI, MudBlazor, Radzen), or Osirion's theme files without one. The default. |
| Osirion | light, dark | Osirion's own, kept even when a framework adapter is active |
| Commit | light, dark | Inspired by GitHub's Primer colors |
| Editor | light, dark | Inspired by Visual Studio Code's Light Modern and Dark Modern themes |
| Studio | light, dark | Approximates Visual Studio's Light and Dark themes |
| Studio Blue | light; dark uses Studio's | Approximates Visual Studio's Blue theme: a light page in a dark blue menu frame |
| Contrast | light, dark | High contrast, inspired by Visual Studio Code's High Contrast themes |

The library's tests check every variant against WCAG 2.2: text reaches 4.5:1 on its background and the focus
indicator 3:1, and the Contrast palette holds text to 7:1. Code blocks follow the palette too. The
[Palettes](/components/palettes) page covers setup, how a palette is applied and where the colors come from; the
[ThemePicker](/components/theme-picker) is the list at the top of this page.

The getting started page in each palette, in the dark theme and then the light one:

![The getting started page of getosirion.com in the six palettes: Osirion, Commit, Editor, Studio, Studio Blue and Contrast, in the dark theme](https://media.getosirion.com/shots/themes/palettes-dark.webp){width=1600 height=774}

![The getting started page of getosirion.com in the six palettes: Osirion, Commit, Editor, Studio, Studio Blue and Contrast, in the light theme](https://media.getosirion.com/shots/themes/palettes-light.webp){width=1600 height=774}

## Make your own

A palette is a set of values for the tokens in the table below. To change one token of one palette, write a rule at
least as specific as the palette's own in your stylesheet:

```css
html:root[data-osirion-palette="commit"][data-osirion-theme="light"] {
    --osirion-action-primary: #0969da;
}
```

A palette of your own follows the same pattern: set the tokens under
`:root[data-osirion-palette="your-name"][data-osirion-theme="light"]` and `...="dark"]`, and put the
`data-osirion-palette` attribute on `<html>` yourself, since `ThemePalette` only knows the palettes Osirion ships.

Keep the pairs in the table's last column apart: each text token at 4.5:1 on the backgrounds it is read on, and the
focus colors at 3:1 on the page. Without a palette, the theme files and the framework adapter set these tokens;
[OsirionStyles](/components/osirion-styles) describes the other ways to override them.
