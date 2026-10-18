---
title: "Scroll Position After Enhanced Navigation in Blazor"
author: "Dejan Demonjić"
date: "2025-01-20"
last_modified: 2026-10-11
description: "Control where the page scrolls after Blazor enhanced navigation with the EnhancedNavigation component from Osirion.Blazor.Navigation."
tags: [Navigation, Enhanced Navigation, Scrolling, SSR, Blazor]
categories: [Components, Navigation]
slug: "enhanced-navigation-interceptor"
is_featured: true
featured_image: 'https://media.getosirion.com/og/blog-enhanced-navigation-interceptor.jpg'
seo_properties:
  title: "Scroll Position After Enhanced Navigation in Blazor"
  description: "Use EnhancedNavigation to scroll to the top after each Blazor enhanced navigation, with smooth, instant or browser-default scrolling."
  image: 'https://media.getosirion.com/og/blog-enhanced-navigation-interceptor.jpg'
  og_image_url: 'https://media.getosirion.com/og/blog-enhanced-navigation-interceptor.jpg'
  type: "Article"
---

# Scroll Position After Enhanced Navigation in Blazor

Blazor Web Apps use enhanced navigation: when a visitor follows a link, Blazor fetches the next page and patches it into
the current document instead of loading a new one. Pages feel faster, but the browser's normal page-load behavior no
longer decides where the window ends up. `EnhancedNavigation` from `Osirion.Blazor.Navigation` makes that decision
explicit: after each enhanced navigation it scrolls the window to the top, unless you tell it not to.

## Add it to App.razor

Place the component in `App.razor`, after the `blazor.web.js` script tag:

```razor
@using Osirion.Blazor.Navigation.Components

<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>

    <EnhancedNavigation />
</body>
```

The order matters. The component renders a small inline script that subscribes to Blazor's `enhancedload` event, and it
checks for the `Blazor` object when it runs. Placed before `blazor.web.js`, it finds nothing and does nothing.

The component needs no service registration. It reads `EnhancedNavigationOptions` when they are configured and falls
back to the defaults otherwise.

## What the script does

On every `enhancedload` event, the script compares the new URL with the previous one and then:

1. does nothing if `ResetScrollOnNavigation` is `false`;
2. does nothing if `PreserveScrollForSamePageNavigation` is `true` and the URL did not change;
3. otherwise calls `window.scrollTo` to the top of the page with the configured behavior.

The comparison uses the full URL, including the query string, so moving from `?page=1` to `?page=2` counts as a new
page, while a link to the page the reader is already on keeps their position as long as
`PreserveScrollForSamePageNavigation` is on.

The script runs on every `enhancedload`, so it does not treat the browser's back and forward buttons differently from
link clicks. Full page loads are left to the browser, and navigation inside an interactive router does not raise
`enhancedload`, so the script does not run for it.

## Parameters

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Behavior` | `ScrollBehavior?` | `null` (options: `Auto`) | How to scroll: `Auto` lets the browser decide, `Instant` jumps, `Smooth` animates. |
| `ResetScrollOnNavigation` | `bool?` | `null` (options: `true`) | Scroll to the top after an enhanced navigation. |
| `PreserveScrollForSamePageNavigation` | `bool?` | `null` (options: `true`) | Skip the scroll when the URL did not change. |

A parameter you leave unset takes its value from `EnhancedNavigationOptions`, so you can configure the behavior once
and keep the markup bare. `ScrollBehavior` lives in the `Osirion.Blazor.Components` namespace:

```razor
@using Osirion.Blazor.Components

<EnhancedNavigation Behavior="ScrollBehavior.Smooth" />
```

## Configure it once

With the meta-package, `AddOsirion` registers enhanced navigation with its defaults. If you call
`AddOsirion(builder.Configuration)`, change them in the `Osirion:Navigation:Enhanced` section:

```json
{
  "Osirion": {
    "Navigation": {
      "Enhanced": {
        "Behavior": "Smooth",
        "ResetScrollOnNavigation": true,
        "PreserveScrollForSamePageNavigation": false
      }
    }
  }
}
```

or register it in code with the navigation package alone:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Navigation.Extensions;

builder.Services.AddOsirionNavigation(navigation =>
{
    navigation.AddEnhancedNavigation(options => options.Behavior = ScrollBehavior.Instant);
});
```

## The same event, other uses

`enhancedload` is also what the Matomo and Yandex Metrica trackers listen for to report page views after enhanced
navigation, through the static `osirion-analytics.js` script. See
[Analytics in Blazor with Osirion.Blazor](/blog/analytics-integration-in-blazor).

## Related

- [Navigation components](/docs/packages/navigation)
- [Navigation in Osirion.Blazor: menus, scroll and breadcrumbs](/blog/navigation-components)
- [Osirion.Blazor.Navigation on NuGet](https://www.nuget.org/packages/Osirion.Blazor.Navigation)
