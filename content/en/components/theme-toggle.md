---
id: 'theme-toggle-component'
order: 2
layout: docs
title: ThemeToggle
permalink: /components/theme-toggle
description: ThemeToggle renders a button that switches the site between light and dark in the browser and remembers the choice in a cookie.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-theme-toggle.jpg'
categories:
- Components
- Theming Components
- UI Controls
tags:
- blazor
- theme-toggle
- dark-mode
- light-mode
- accessibility
is_featured: true
published: true
slug: theme-toggle
lang: en
redirect_from:
- /docs/components/theming/theme-toggle
custom_fields:
  group: theming
seo_properties:
  title: 'ThemeToggle: light and dark mode button for Blazor'
  description: 'A light and dark mode button for Blazor that works under static SSR and remembers the visitor choice in a cookie.'
  image: 'https://media.getosirion.com/og/components-theme-toggle.jpg'
  canonical: 'https://getosirion.com/components/theme-toggle'
  lang: en
  robots: index, follow
  og_title: 'ThemeToggle Component - Osirion.Blazor'
  og_description: 'A light and dark mode button that works under static SSR and remembers the choice in a cookie.'
  og_image_url: 'https://media.getosirion.com/og/components-theme-toggle.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ThemeToggle Component - Osirion.Blazor'
  twitter_description: 'A light and dark mode button for Blazor that works under static SSR.'
  twitter_image_url: 'https://media.getosirion.com/og/components-theme-toggle.jpg'
  meta_tags: {}
---

# ThemeToggle

`ThemeToggle` renders a round button with a sun or moon icon that switches the site between the light and the dark
theme. The switch happens in the browser, so it works on statically rendered pages, and the choice is kept in a cookie
for the next visit. Use it in your header or menu, together with
[`OsirionStyles`](/components/osirion-styles) in the layout. The demo below changes this site's own mode, which is the
point of the component, and your choice stays in your browser.

Package `Osirion.Blazor.Theming`, namespace `Osirion.Blazor.Theming.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Theming/Components/ThemeToggle.razor).

```demo
theme-toggle
```

## Usage

```razor
@using Osirion.Blazor.Theming.Components

<ThemeToggle />
```

In a navigation menu, after the links:

```razor
@using Osirion.Blazor.Navigation.Components
@using Osirion.Blazor.Theming.Components

<Menu AriaLabel="Main navigation" BrandText="My site" Alignment="MenuAlignment.Right">
    <MenuItem Text="Home" Href="/" Icon="home" />
    <MenuItem Text="Blog" Href="/blog" Icon="blog" />
    <MenuDivider />
    <ThemeToggle />
</Menu>
```

The toggle needs theming registered: `AddOsirion`, or `AddOsirionTheming` together with
`builder.Services.AddJSComponents()`, which loads its script. See [Theming](/docs/packages/theming) for the setup.

## Parameters

<!-- osirion:parameters ThemeToggle -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` | `string?` | `null` | The id of the toggle button. No id is written when it is `null`, so several toggles can share a page without repeating an id; set it only when a host stylesheet or script needs to address one. |
| `OnThemeChanged` | `EventCallback<string>` | - | A callback raised with `light` or `dark` after the visitor switches the theme. It runs only when the toggle itself is interactive (Interactive Server, WebAssembly or Auto): under Static SSR the browser switches the theme and the server is not notified, so the callback is never raised there. Listen for the `osirion-theme-update` browser event on `window` to react in every render mode. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The last three rows, `Theme`, `Class` and `Style`, are inherited from the component base class. They are not written to
the markup, and neither are extra attributes.

## Behavior

The button has `type="button"`, and a click listener in `_content/Osirion.Blazor.Theming/js/theme-toggle.js` handles
the click. It needs JavaScript, but no interactive render mode; there is no fallback without JavaScript. One click:

1. switches `data-osirion-theme` on `<html>` between `light` and `dark`, and updates the framework's dark-mode marker
   (`data-bs-theme`, `mud-theme-dark`, `fluent-dark-theme` and `fluent-light-theme`, or `rz-dark-theme`);
2. stores the choice in the `osirion-preferred-theme` cookie (path `/`, one year, `SameSite=Lax`);
3. raises the `osirion-theme-update` event on `window`, which `OsirionStyles` and your own scripts can listen to;
   when the toggle is interactive and has an `OnThemeChanged` callback, that callback is raised too, with `light` or
   `dark`;
4. updates every toggle on the page to show the new state.

The toggle has two states, light and dark. Before the visitor clicks, the site follows the configured `DefaultMode`,
which can be `System`; after a click, the stored choice wins over the operating system preference.

On the server the button renders from `IThemeService.CurrentMode`, which reads the same cookie, so a returning visitor
gets the right icon in the first response. When the mode is `System`, the server renders the moon icon and the script
corrects it once the page loads. The script also refreshes the button after enhanced navigation.

The change fades through a short full-screen overlay. With `prefers-reduced-motion: reduce`, the theme changes at once
without the fade. The scroll position is kept.

The toggle writes no inline script and no event handler attribute. Its settings (`data-default-theme` and
`data-framework`) travel on the button, and a click listener in `theme-toggle.js` does the work, so the button runs
under a Content Security Policy without `'unsafe-inline'`. `OsirionStyles`, which the page also needs, writes a short
inline script; pass it a `Nonce` so the policy can allow that script, as [its page](/components/osirion-styles#security)
describes.

### Accessibility

- The button's `aria-label` and `title` state the current theme and the one a click switches to, for example "Toggle
  theme. Currently using light mode. Click to switch to dark mode."
- A visually hidden text (`osirion-visually-hidden`) repeats the current theme; the icons are `aria-hidden`.
- Keyboard focus (`:focus-visible`) shows the focus ring every Osirion component draws: `--osirion-focus-ring-width`
  solid `--osirion-focus-ring-color`, offset by `--osirion-focus-ring-offset`. Mouse focus does not. The ring is drawn
  at once; the button's transitions leave the outline out, so it does not fade in from another color. Just inside it
  the button's own 2px ring shows in `--osirion-primary-color` at full strength, so focus stays visible when a site sets
  the token to a faint color.
- The button is 44 by 44 pixels, 40 by 40 at 768px and below, and 36 by 36 at 576px and below.

## Styling

| Class | Element |
| --- | --- |
| `theme-switcher` | The wrapper `div`, padded by `--osirion-theme-toggle-padding` (10px). |
| `theme-toggle-btn` | The round button. |
| `theme-icon` | The sun or moon icon. |
| `theme-text` | The visually hidden text. |

The button takes its light and dark colors from `[data-osirion-theme]`. The hover ring uses
`--osirion-primary-color` and `--osirion-primary-color-hover`. The theme does not set these, so the fallbacks
`#2563eb` and `#1d4ed8` apply until you define them. The focus ring follows the shared focus ring tokens, which the
theme defines, so one setting restyles it together with the other components:

```css
:root {
    --osirion-primary-color: #0b5ed7;
    --osirion-primary-color-hover: #0a58ca;
    --osirion-focus-ring-color: #0b5ed7;
    --osirion-focus-ring-width: 2px;
}
```

`--osirion-theme-toggle-padding` sets the wrapper's padding (10px by default). The `Menu` drawer sets it to `0` on its
actions, so a toggle there lines up with the menu items; set it in any other container that spaces its children itself.

The component's own styles are scoped CSS, and several of them use `!important` (the border and the shadow, never the
focus outline). To restyle the button, use a selector that is at least as specific and add `!important` where the
original has it.

## Related

- [Theming](/docs/packages/theming): setup, options and how the theme is applied.
- [OsirionStyles](/components/osirion-styles): the component that applies the stored theme.
- [Navigation](/docs/packages/navigation): the `Menu` component used above.
