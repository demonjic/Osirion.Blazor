---
id: 'theming-components-overview'
order: 5
layout: docs
title: Osirion.Blazor.Theming
permalink: /docs/packages/theming
description: Light and dark themes for Osirion components, adapters for Bootstrap, Fluent UI, MudBlazor and Radzen, and a theme toggle that works under static SSR.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-16
featured_image: 'https://media.getosirion.com/og/docs-packages-theming.jpg'
categories:
- Components
- Theming Components
- Design Systems
tags:
- blazor
- theming
- dark-mode
- light-mode
- css-custom-properties
- bootstrap
- fluent-ui
- mudblazor
- radzen
is_featured: true
published: true
slug: theming
lang: en
redirect_from:
- /docs/components/theming
custom_fields:
  group: packages
seo_properties:
  title: 'Theming for Blazor - Osirion.Blazor'
  description: 'Light and dark themes, CSS framework adapters and a theme toggle for Osirion.Blazor components.'
  image: 'https://media.getosirion.com/og/docs-packages-theming.jpg'
  canonical: 'https://getosirion.com/docs/packages/theming'
  lang: en
  robots: index, follow
  og_title: 'Theming for Blazor - Osirion.Blazor'
  og_description: 'Light and dark themes, adapters for Bootstrap, Fluent UI, MudBlazor and Radzen, and a theme toggle.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages-theming.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Theming for Blazor - Osirion.Blazor'
  twitter_description: 'Light and dark themes, CSS framework adapters and a theme toggle for Osirion.Blazor.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages-theming.jpg'
  meta_tags: {}
---

# Osirion.Blazor.Theming
`Osirion.Blazor.Theming` gives Osirion components a light and a dark theme, maps their colors onto Bootstrap,
Fluent UI, MudBlazor or Radzen, and lets visitors switch between the themes. It has two components:

- [`OsirionStyles`](/components/osirion-styles) links the stylesheets and applies the visitor's theme.
  Render it once, in your main layout.
- [`ThemeToggle`](/components/theme-toggle) is a button that switches between light and dark.

On top of the mode, a [palette](/components/palettes) can set the colors: Osirion, Commit, Editor,
Studio, Studio Blue or Contrast, each with a light and a dark variant.
[`ThemePicker`](/components/theme-picker) lets visitors choose one.

Neither needs an interactive render mode. Both load a small script, so the theme switch needs JavaScript.

## Setup

Set the framework and the default theme in `appsettings.json`:

```json
{
  "Osirion": {
    "Theming": {
      "Framework": "Bootstrap",
      "DefaultMode": "System"
    }
  }
}
```

Register theming in `Program.cs`. `AddOsirion(builder.Configuration)` from the meta-package reads the same section and
also registers the script loader. With the `Osirion.Blazor.Theming` package alone, call `AddOsirionTheming` and
`AddJSComponents`, because `AddOsirionTheming` does not register the loader:

```csharp
using Osirion.Blazor.Theming.Extensions;

builder.Services.AddOsirionTheming(builder.Configuration);
builder.Services.AddJSComponents();
```

`AddJSComponents` registers the loader that `OsirionStyles` and other components use for their scripts. The same
setup in code:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Theming.Extensions;

builder.Services.AddOsirionTheming(theming => theming
    .UseFramework(CssFramework.Bootstrap)
    .Configure(options => options.DefaultMode = ThemeMode.System));
```

Then render the components in your layout:

```razor
@using Osirion.Blazor.Theming.Components
@inherits LayoutComponentBase

<header>
    <ThemeToggle />
</header>

<main>
    @Body
</main>

<OsirionStyles />
```

`OsirionStyles` does not load the CSS framework itself. Reference Bootstrap, Fluent UI, MudBlazor or Radzen the way
that framework documents.

## Options

These are the `ThemingOptions` the components read, bound from `Osirion:Theming`:

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| `Framework` | `CssFramework` | `None` | `None`, `Bootstrap`, `FluentUI`, `MudBlazor` or `Radzen`. Selects the adapter. |
| `DefaultMode` | `ThemeMode` | `System` | The theme before the visitor picks one: `Light`, `Dark` or `System` (follow the operating system). |
| `UseDefaultStyles` | `bool` | `false` | When `true`, only the Core stylesheets are linked; the adapter and the theme stylesheet are left out. |
| `CustomVariables` | `string?` | `null` | CSS declarations written into a `:root` block, for example `--osirion-font-family-base: Inter, sans-serif;`. They do not override the theme colors or an adapter's values; see Styling. |
| `DefaultPalette` | `ThemePalette` | `Framework` | The [palette](/components/palettes) before the visitor picks one. `Framework` applies none. |
| `AllowPaletteSelection` | `bool` | `false` | Links the palette stylesheet on every page, so visitors can switch palettes without a flash. |

`EnableDarkMode` only affects `IThemeService.SetThemeMode` on the server; `ThemeToggle` switches in the browser and
ignores it. `FollowSystemPreference` is bound but not read by the components; set `DefaultMode` to `System` instead.
The parameterless `AddOsirionTheming()` registers different defaults: `UseDefaultStyles` on and `DefaultMode` set to
`Light`.

## How the theme is applied

The current theme lives on the `<html>` element:

- `data-osirion-theme` is `light` or `dark`, and `data-osirion-framework` names the framework.
- The framework's own marker is set too: `data-bs-theme` for Bootstrap, the `mud-theme-dark` class for MudBlazor,
  `fluent-dark-theme` or `fluent-light-theme` for Fluent UI, and `rz-dark-theme` for Radzen. With Fluent UI Blazor v5,
  the mode is also passed to `Blazor.theme.setThemeMode`, so Fluent components follow the toggle.
- The visitor's choice is stored in the `osirion-preferred-theme` cookie for one year. `OsirionStyles` applies it
  while the page is parsed, and the server reads it so `ThemeToggle` renders with the right icon.
- With `DefaultMode` set to `System` and no stored choice, the theme follows `prefers-color-scheme`, including
  changes while the page is open.

## Framework adapters

An adapter maps Osirion's design tokens onto the framework's variables: `--bs-*` for Bootstrap, `--mud-palette-*` for
MudBlazor, the Fluent UI v5 tokens (`--colorNeutralBackground1` and so on, with the v4 tokens as fallback) and
`--rz-*` for Radzen. Its rules apply under a class on `<html>` that comes from the configured framework:
`osirion-bootstrap-integration`, `osirion-fluent-integration`, `osirion-mudblazor-integration` or
`osirion-radzen-integration`. Osirion components then take their colors from your framework theme in both modes.

The repository has one example site per framework (`examples/Osirion.Blazor.Example.Bootstrap`, `.FluentUI` and
`.Mud`). The MudBlazor example shows a MudBlazor theme that follows `ThemeToggle` without a round trip.

## Styling

Osirion components read CSS custom properties such as `--osirion-background-primary`, `--osirion-text-primary`,
`--osirion-border-color`, `--osirion-action-primary` and `--osirion-link-color`. The theme stylesheet sets them under
`:root[data-osirion-theme=light]` and `:root[data-osirion-theme=dark]`, which are more specific than a plain `:root`
block, so `CustomVariables` cannot change them.

Without an adapter, override them per theme in your own stylesheet. Putting `html` in front of the theme's selector
makes your rule win whatever order the stylesheets load in:

```css
html:root[data-osirion-theme="light"] {
    --osirion-action-primary: #0b5ed7;
}

html:root[data-osirion-theme="dark"] {
    --osirion-action-primary: #6ea8fe;
}
```

With an adapter, change the framework's variables instead (for Bootstrap, variables such as `--bs-primary`): the adapter
derives Osirion's tokens from them under `html.osirion-bootstrap-integration` and similar selectors. Every adapter
also sets `--osirion-font-family-base` from the framework's font, so with an adapter the font comes from the framework.

## Related

- [OsirionStyles](/components/osirion-styles)
- [ThemeToggle](/components/theme-toggle)
- [Palettes](/components/palettes)
- [ThemePicker](/components/theme-picker)
- [Navigation](/docs/packages/navigation): place the toggle in a `Menu`.
- [Osirion.Blazor.Theming on NuGet](https://www.nuget.org/packages/Osirion.Blazor.Theming)
