---
id: 'osirion-scroll-to-top'
order: 4
layout: docs
title: ScrollToTop
permalink: /components/scroll-to-top
description: ScrollToTop renders a fixed back to top button that appears after a scroll threshold. ScrollToTopProvider renders it from settings registered once.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-scroll-to-top.jpg'
categories:
- Components
- Navigation Components
- Navigation
tags:
- blazor
- scroll-to-top
- navigation
- static-ssr
- osirion-blazor-navigation
is_featured: true
published: true
slug: scroll-to-top
lang: en
custom_fields:
  group: navigation
seo_properties:
  title: 'ScrollToTop: a back to top button for Blazor'
  description: 'ScrollToTop renders a fixed back to top button that appears after a scroll threshold. ScrollToTopProvider renders it from settings registered once.'
  image: 'https://media.getosirion.com/og/components-scroll-to-top.jpg'
  canonical: 'https://getosirion.com/components/scroll-to-top'
  lang: en
  robots: index, follow
  og_title: 'ScrollToTop - Osirion.Blazor'
  og_description: 'A fixed back to top button that appears after a scroll threshold, configured once or per layout.'
  og_image_url: 'https://media.getosirion.com/og/components-scroll-to-top.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ScrollToTop - Osirion.Blazor'
  twitter_description: 'A fixed back to top button that appears after a scroll threshold, configured once or per layout.'
  twitter_image_url: 'https://media.getosirion.com/og/components-scroll-to-top.jpg'
  meta_tags: {}
---

# ScrollToTop

`ScrollToTop` renders a fixed "back to top" button that appears once the visitor has scrolled past a threshold and
scrolls the window back to the top when clicked. `ScrollToTopProvider` renders the same button from the settings you
register once at startup, so a layout needs only one line.

Package `Osirion.Blazor.Navigation`, namespace `Osirion.Blazor.Navigation.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Navigation/Components/ScrollToTop.razor).

There is no demo on this page. The button is fixed to the viewport and appears only after a scroll of 300 pixels, so an
inline example would add a second floating button to the page you are reading.

## Usage

Put `ScrollToTopProvider` in a layout. It reads the registered settings (see the
[package page](/docs/packages/navigation) for `AddScrollToTop` and the `Osirion:Navigation:ScrollToTop` configuration
section):

```razor
@inherits LayoutComponentBase

@Body

<ScrollToTopProvider />
```

To set the button per layout instead, use the component directly:

```razor
<ScrollToTop Position="Position.BottomLeft" Text="Top" IconCssClass="bi bi-arrow-up" />
```

`Position` and `ScrollBehavior` are enums in `Osirion.Blazor.Components`. `OsirionSiteLayout` renders
`<ScrollToTopProvider />` itself when `ShowScrollToTop` is `true`, which is the default.

## Parameters

### ScrollToTop

A parameter left at its default value lets the configured value apply: `ScrollToTop` treats `BottomRight`, `Smooth`,
`300` and `"Scroll to top"` as unset and uses the registered `ScrollToTopOptions` value instead. `Enabled` and `Text`
are nullable and fall back the same way.

<!-- osirion:parameters ScrollToTop -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Position` | `Position` | `Position.BottomRight` | The corner the button is fixed to: `TopLeft`, `TopRight`, `BottomLeft` or `BottomRight`. Left at its default, the configured `Position` applies. |
| `Behavior` | `ScrollBehavior` | `ScrollBehavior.Smooth` | How the page scrolls up: `Auto`, `Instant` or `Smooth`. Left at its default, the configured `Behavior` applies. |
| `VisibilityThreshold` | `int` | `300` | How many pixels the page must be scrolled before the button appears. Left at its default, the configured `VisibilityThreshold` applies. |
| `Text` | `string?` | `null` | Visible text shown next to the icon. It is hidden at 768px and below. When unset, the configured `Text` applies. |
| `Title` | `string` | `"Scroll to top"` | The accessible name of the button, written as `aria-label`. Left at its default, the configured `Title` applies. |
| `CustomIcon` | `string?` | `null` | Icon markup. It is sanitized before it is rendered, which removes `svg` elements, so an inline SVG is dropped: use `IconCssClass` instead. Without a registered HTML sanitizer the markup is ignored. |
| `IconCssClass` | `string?` | `null` | The class of an `aria-hidden` span rendered inside the button, for example an icon font class. It is used when `CustomIcon` yields no markup; with neither, the button shows a built-in arrow. |
| `Enabled` | `bool?` | `null` | Whether the button is rendered; `false` renders nothing. When unset, the configured `Enabled` applies. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

### ScrollToTopProvider

`ScrollToTopProvider` has no parameters of its own. `Class`, `Style` and `Theme` come from the component base, and the
provider's markup uses none of them. All settings come from the registered `ScrollToTopManager`: `IsEnabled`,
`Position`, `Behavior`, `VisibilityThreshold`, `Text`, `Title`, `CssClass`, `CustomIcon` and `IconCssClass`. The
provider passes `CssClass` to the button as its `Class`.

<!-- osirion:parameters ScrollToTopProvider -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- The button is rendered hidden (`style="display: none;"`) and an inline script shows it when `window.scrollY` is
  greater than the threshold. The button therefore needs JavaScript, but no interactive render mode, so it works under
  static SSR.
- Each button gets a unique `id` and registers its scroll listener once, so a second render or an enhanced navigation
  does not stack listeners. Two buttons on one page do not clash.
- A click calls `window.scrollTo` to the top with the configured `Behavior`.
- `Enabled="false"` renders nothing. The provider also renders nothing while `ScrollToTopManager.IsEnabled` is `false`,
  so the button shows only when both the option and the manager allow it.
- `ScrollToTopProvider` injects the scoped `ScrollToTopManager` that `AddScrollToTop` registers. It re-renders when the
  manager's settings change, and unsubscribes when it is disposed.
- `Title` is the button's `aria-label`. `Text` is shown next to the icon and hidden at 768px and below, where the
  button shrinks to a round icon.
- `IconCssClass` renders an `aria-hidden` span with your class, for an icon font. `CustomIcon` is passed through the
  Osirion HTML sanitizer, which removes inline SVG, so prefer `IconCssClass`; without a registered sanitizer
  `CustomIcon` is ignored. With neither, the button shows a built-in arrow.

## Styling

The button is a `<button type="button">` with the class `osirion-scroll-to-top`, a position class (`top-left`,
`top-right`, `bottom-left` or `bottom-right`) and the configured CSS framework's button class: `btn btn-primary` with
Bootstrap, `osirion-fluent-button` with Fluent UI, `mud-button mud-button-filled` with MudBlazor, `rz-button
rz-button-primary` with Radzen, and `osirion-search-button` with none. `Class` is added to them.

`--osirion-scroll-size` sets the size: `45px` by default, `40px` at 768px and below. With `Text` set, the button widens
into a pill above 768px.

## Related

- [EnhancedNavigation](/components/enhanced-navigation), which resets the scroll position after enhanced navigation
- [Menu](/components/menu)
- [Navigation package](/docs/packages/navigation)
