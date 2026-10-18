---
id: 'osirion-enhanced-navigation'
order: 5
layout: docs
title: EnhancedNavigation
permalink: /components/enhanced-navigation
description: EnhancedNavigation scrolls to the top of the new page after a Blazor enhanced navigation. It renders only an inline script.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-enhanced-navigation.jpg'
categories:
- Components
- Navigation Components
- Navigation
tags:
- blazor
- enhanced-navigation
- scroll
- navigation
- osirion-blazor-navigation
is_featured: true
published: true
slug: enhanced-navigation
lang: en
custom_fields:
  group: navigation
seo_properties:
  title: 'EnhancedNavigation: scroll control for Blazor'
  description: 'EnhancedNavigation scrolls to the top of the new page after a Blazor enhanced navigation. It renders only an inline script.'
  image: 'https://media.getosirion.com/og/components-enhanced-navigation.jpg'
  canonical: 'https://getosirion.com/components/enhanced-navigation'
  lang: en
  robots: index, follow
  og_title: 'EnhancedNavigation - Osirion.Blazor'
  og_description: 'Scroll to the top after a Blazor enhanced navigation, with options for behavior and same-page updates.'
  og_image_url: 'https://media.getosirion.com/og/components-enhanced-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'EnhancedNavigation - Osirion.Blazor'
  twitter_description: 'Scroll to the top after a Blazor enhanced navigation, with options for behavior and same-page updates.'
  twitter_image_url: 'https://media.getosirion.com/og/components-enhanced-navigation.jpg'
  meta_tags: {}
---

# EnhancedNavigation

Blazor enhanced navigation replaces the page content without a full load, and the browser keeps the old scroll
position. `EnhancedNavigation` scrolls to the top of the new page after each enhanced navigation. It renders no visible
output, only an inline script, so it has no demo.

Package `Osirion.Blazor.Navigation`, namespace `Osirion.Blazor.Navigation.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Navigation/Components/EnhancedNavigation.razor).

## Usage

Place it in `App.razor` after the Blazor script, because the script looks for the `Blazor` object when it runs:

```razor
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
    <EnhancedNavigation />
</body>
```

`AddOsirion()` registers the defaults; to change them for the whole site, see the
[package page](/docs/packages/navigation). Parameters override the configured values for one place:

```razor
<EnhancedNavigation Behavior="ScrollBehavior.Smooth" ResetScrollOnNavigation="true" />
```

`ScrollBehavior` is an enum in `Osirion.Blazor.Components`.

## Parameters

Each parameter is nullable. When it is not set, the configured value from `EnhancedNavigationOptions` applies
(`Osirion:Navigation:Enhanced` in configuration).

<!-- osirion:parameters EnhancedNavigation -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Behavior` | `ScrollBehavior?` | `null` | How the page scrolls to the top: `Auto`, `Instant` or `Smooth`. When not set, the configured `EnhancedNavigationOptions.Behavior` applies, which is `Auto` by default. |
| `ResetScrollOnNavigation` | `bool?` | `null` | Whether the page scrolls to the top after an enhanced navigation. When not set, the configured `EnhancedNavigationOptions.ResetScrollOnNavigation` applies, which is `true` by default. |
| `PreserveScrollForSamePageNavigation` | `bool?` | `null` | Whether an enhanced update that leaves the URL unchanged keeps the scroll position. It matters only while `ResetScrollOnNavigation` is on. When not set, the configured `EnhancedNavigationOptions.PreserveScrollForSamePageNavigation` applies, which is `true` by default. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- The component writes one inline script and needs no interactive render mode.
- The script listens for the `enhancedload` event of the `Blazor` object. When the `Blazor` object is missing, which is
  the case if the component comes before the Blazor script, it does nothing.
- With `ResetScrollOnNavigation` on, each enhanced load scrolls to the top (`top: 0, left: 0`) with the configured
  `Behavior`. With `PreserveScrollForSamePageNavigation` also on, it does so only when the URL changed since the last
  load, so an enhanced update that leaves the URL unchanged keeps the scroll position.
- With `ResetScrollOnNavigation` off, the script never scrolls.
- A form that navigates with a result in the query string, such as the subscription card, can read better with
  `ResetScrollOnNavigation="false"`, so the visitor stays at the form.

## Related

- [ScrollToTop](/components/scroll-to-top)
- [Menu](/components/menu)
- [Navigation package](/docs/packages/navigation)
