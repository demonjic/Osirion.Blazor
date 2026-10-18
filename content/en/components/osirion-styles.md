---
id: 'osirion-styles-component'
order: 1
layout: docs
title: OsirionStyles
permalink: /components/osirion-styles
description: OsirionStyles links the Osirion stylesheets and the CSS framework adapter, and applies the visitor's light or dark theme as the page loads.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Components
- Theming Components
- Styling
tags:
- blazor
- osirion-styles
- theming
- dark-mode
- css-custom-properties
- css-framework
is_featured: true
published: true
slug: osirion-styles
lang: en
redirect_from:
- /docs/components/theming/osirion-styles
custom_fields:
  group: theming
seo_properties:
  title: 'OsirionStyles: stylesheets and theme setup for Blazor'
  description: 'Link the Osirion stylesheets and framework adapter and apply the stored light or dark theme with OsirionStyles.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/components/osirion-styles'
  lang: en
  robots: index, follow
  og_title: 'OsirionStyles Component - Osirion.Blazor'
  og_description: 'Links the Osirion stylesheets and the framework adapter, and applies the visitor theme as the page loads.'
  og_image_url: '/assets/images/blazor-cover.png'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionStyles Component - Osirion.Blazor'
  twitter_description: 'Links the Osirion stylesheets and the framework adapter, and applies the visitor theme.'
  twitter_image_url: '/assets/images/blazor-cover.png'
  meta_tags: {}
---

# OsirionStyles

`OsirionStyles` links the stylesheets that Osirion components need, adds the adapter for your CSS framework, and
applies the visitor's light or dark theme while the page loads. Render it once per page, in your main layout. It writes
stylesheet links and a script where you place it, not visible markup, so this page has no live demo.

Package `Osirion.Blazor.Theming`, namespace `Osirion.Blazor.Theming.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Theming/Components/OsirionStyles.razor).

## Usage

With theming configured (see [Theming](/docs/packages/theming)), the component needs no parameters:

```razor
@using Osirion.Blazor.Theming.Components
@inherits LayoutComponentBase

@Body

<OsirionStyles />
```

A layout that makes dark the default theme and sets the base font:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Theming.Components
@inherits LayoutComponentBase

@Body

<OsirionStyles Mode="ThemeMode.Dark"
               CustomVariables="--osirion-font-family-base: Inter, system-ui, sans-serif;" />
```

`Mode` only sets the default. A visitor who already picked a theme with
[`ThemeToggle`](/components/theme-toggle) keeps it. The font variable takes effect when no framework
adapter is used: every adapter sets `--osirion-font-family-base` from the framework's own font.

## Parameters

`Mode`, `Framework` and `UseStyles` override the matching `ThemingOptions` value for this component; left unset, the
configured value applies. `CustomVariables` does not replace the configured value: both are written (see Behavior).

<!-- osirion:parameters OsirionStyles -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `UseStyles` | `bool?` | `null` | Whether only the Core stylesheet is linked. When `true`, the framework adapter and the theme stylesheet are left out. When `null`, `ThemingOptions.UseDefaultStyles` applies (`false` unless configured). |
| `CustomVariables` | `string?` | `null` | CSS declarations written into a `:root` block. Every `<` is replaced by the CSS escape `\3C `, so a value cannot end the `<style>` element or add markup; a valid declaration, which never contains `<` outside a string, is written unchanged. The configured `ThemingOptions.CustomVariables` value is written the same way in a second block after this one, so for the same property the configured value wins. Still pass only values from your own code or configuration. |
| `Framework` | `CssFramework?` | `null` | Which framework adapter stylesheet is linked: `Bootstrap`, `FluentUI`, `MudBlazor` or `Radzen`; `None` links none. When `null`, `ThemingOptions.Framework` applies. The framework class on `<html>` comes from the configured framework, so set the framework in configuration as well. |
| `Mode` | `ThemeMode?` | `null` | The theme used when the visitor has not picked one: `Light`, `Dark` or `System`. `Inverted` is not handled by the page script, which then follows the operating system. When `null`, `ThemingOptions.DefaultMode` applies (`System` unless configured). |
| `Nonce` | `string?` | `null` | The Content Security Policy nonce written as the `nonce` attribute of the inline `<script>` that sets the theme before first paint and of each inline `<style>` element. Pass the nonce your policy header carries for this response, so `script-src` and `style-src` can use `'nonce-...'` instead of `'unsafe-inline'`. It must be a new random value for every response; a fixed value defeats the policy. When `null` or empty, no attribute is written. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The last three rows, `Theme`, `Class` and `Style`, are inherited from the component base class and have no effect
here, nor do extra attributes: the component renders no wrapper element.

Set the framework in configuration rather than only through the `Framework` parameter. The adapter's rules apply under
a class on `<html>` (such as `osirion-bootstrap-integration`) that comes from the configured framework, so the
parameter on its own links the adapter stylesheet without the class it needs.

## Behavior

The component renders, in this order:

1. `_content/Osirion.Blazor.Core/css/index.min.css`. The scoped component styles come from your app's
   `{App}.styles.css`, which imports every referenced package's bundle.
2. Unless `UseStyles` (or `UseDefaultStyles`) is `true`, the framework adapter from
   `_content/Osirion.Blazor.Theming/css/adapters/` and `_content/Osirion.Blazor.Theming/css/index.min.css`.
3. `_content/Osirion.Blazor.Theming/css/palettes.min.css` when a [palette](/components/palettes) is in use
   or `AllowPaletteSelection` is set.

   The links in steps 1 to 3 go through the component's `Assets`, so an app that serves static files with
   `MapStaticAssets` gets fingerprinted addresses (`index.min.or0ndbmg4q.css`), which browsers may cache for a year
   and which change when the package does. With `UseStaticFiles` the paths are written as shown.
4. A `<style>` block with the `CustomVariables` parameter, then one with the configured `CustomVariables`. Both are
   `:root` rules, so for the same property the configured value, written last, wins.
5. `osirion-styles.js`, loaded through the BlazorJSComponents script loader, which `AddOsirion` registers (a site
   that registers theming alone with `AddOsirionTheming` calls `builder.Services.AddJSComponents()` itself).
6. A short inline script that sets `data-osirion-theme`, `data-osirion-palette`, `data-osirion-framework`, the
   framework class and the framework's own dark-mode marker on `<html>` while the page is parsed.

The theme is resolved in this order: the `osirion-preferred-theme` cookie, then `Mode` or `DefaultMode`. `System`
follows the operating system's `prefers-color-scheme` and reacts when it changes, as long as the visitor has not
picked a theme. `ThemeMode.Inverted` is not handled by the script: the page takes the operating system's theme, as
with `System`, but does not react when it changes. On the server, `IThemeService` reads and writes `inverted` in the
cookie like `light`, `dark` and `system`, so `CurrentMode` reports `Inverted` for such a visitor.

The script applies the theme again after every Blazor enhanced navigation. Under Static SSR this is all that runs; no
interactive render mode is needed. When `IThemeService` raises `ThemeChanged`, the component renders again, and it
unsubscribes when disposed.

With Fluent UI Blazor v5, the component also passes the mode to `Blazor.theme.setThemeMode`, so Fluent components
follow the Osirion theme.

### Security

`CustomVariables`, and the configured `ThemingOptions.CustomVariables`, are written into a `<style>` element. Every
`<` in the value is replaced by the CSS escape `\3C `, so a value such as `</style><script>` cannot end the element
or add markup; a valid declaration never contains `<` outside a string and is written unchanged. The value is not
validated as CSS otherwise, so pass only values from your code or configuration, never text a visitor supplied. 
The component writes an inline script that sets the theme attributes before first paint, and an inline `<style>`
element for `CustomVariables` and for generated theme variables. Under a Content Security Policy without
`'unsafe-inline'`, pass the nonce of the response in `Nonce`: it is written as the `nonce` attribute of the script and
of every `<style>` element, and `script-src 'nonce-...'` and `style-src 'nonce-...'` then allow them while the first
paint still applies the theme without a flash. Use a new random value per response; a fixed value does not protect
anything. `osirion-styles.js` and the stylesheets are files from your own origin, covered by `'self'`. The
[security guide](/docs/security#4-write-a-content-security-policy) shows the middleware and the layout.

## Styling

After the script runs, `<html>` carries:

| Marker | Values |
| --- | --- |
| `data-osirion-theme` | `light` or `dark` |
| `data-osirion-palette` | `osirion`, `commit`, `editor`, `studio`, `studio-blue` or `contrast`; absent for `Framework` |
| `data-osirion-framework` | `None`, `Bootstrap`, `FluentUI`, `MudBlazor` or `Radzen` |
| Framework class | `osirion-bootstrap-integration`, `osirion-fluent-integration`, `osirion-mudblazor-integration` or `osirion-radzen-integration` |
| Dark-mode marker | `data-bs-theme` (Bootstrap), `mud-theme-dark` (MudBlazor), `fluent-dark-theme` or `fluent-light-theme` (Fluent UI), `rz-dark-theme` (Radzen) |

Use these in your own CSS to style per theme or per framework.

The `CustomVariables` blocks are plain `:root` rules. They come after the Core stylesheet, so they change the tokens it
defines in `:root`, such as `--osirion-font-family-base`. They do not change the theme colors, which the theme
stylesheet sets under the more specific `:root[data-osirion-theme=light]` and `:root[data-osirion-theme=dark]`, nor
anything an adapter sets under `html.osirion-*-integration`. To change a theme color, use a more specific selector in
your own stylesheet; to change a color an adapter provides, change the framework's variable. The
[Theming](/docs/packages/theming) page shows both and lists the main tokens.

## Related

- [Theming](/docs/packages/theming): setup, options and adapters.
- [ThemeToggle](/components/theme-toggle): the button that switches the theme.
- [Palettes](/components/palettes): named color sets with light and dark variants.
