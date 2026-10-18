---
id: 'navigation-components-overview'
order: 3
layout: docs
title: Osirion.Blazor.Navigation
permalink: /docs/packages/navigation
description: Scroll handling for Blazor enhanced navigation, a scroll-to-top button, and a responsive menu with submenus and mega menus that needs no JavaScript.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-packages-navigation.jpg'
categories:
- Components
- Navigation Components
- User Experience
tags:
- blazor
- navigation
- menu
- mega-menu
- enhanced-navigation
- scroll-to-top
- static-ssr
is_featured: true
published: true
slug: navigation
lang: en
redirect_from:
- /docs/components/navigation
custom_fields:
  group: packages
seo_properties:
  title: 'Navigation Components for Blazor - Osirion.Blazor'
  description: 'EnhancedNavigation, ScrollToTop and a responsive Menu with mega menus for Blazor, working under static SSR.'
  image: 'https://media.getosirion.com/og/docs-packages-navigation.jpg'
  canonical: 'https://getosirion.com/docs/packages/navigation'
  lang: en
  robots: index, follow
  og_title: 'Navigation Components for Blazor - Osirion.Blazor'
  og_description: 'Scroll handling for enhanced navigation, a scroll-to-top button and a responsive menu without JavaScript.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Navigation Components for Blazor - Osirion.Blazor'
  twitter_description: 'EnhancedNavigation, ScrollToTop and a responsive Menu for Blazor.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages-navigation.jpg'
  meta_tags: {}
---

# Osirion.Blazor.Navigation

`Osirion.Blazor.Navigation` holds three components for site navigation. Each has its own page with parameters,
behavior and styling:

| Component | What it does |
| --- | --- |
| [Menu](/components/menu) | A site menu with submenus, mega menus and a mobile drawer that open with CSS only, so it works under static SSR without JavaScript. Its parts are `MenuItem`, `MenuGroup`, `MenuDivider`, `MenuCallToAction`, `MenuPanel`, `MenuPanelLink` and `MenuImageCard`. |
| [ScrollToTop](/components/scroll-to-top) | A "back to top" button that appears once the visitor scrolls down. `ScrollToTopProvider` renders it from the registered settings. |
| [EnhancedNavigation](/components/enhanced-navigation) | Controls where the page scrolls after a Blazor enhanced navigation. |

The package also has `Osirion.Blazor.Navigation.Extensions` and `Osirion.Blazor.Navigation.Options` for registration and
configuration, described below. Breadcrumbs and read more links are in the Core package, see the
[navigation components](/components/navigation).

## Setup

```bash
dotnet add package Osirion.Blazor.Navigation
```

The `Osirion.Blazor` meta-package includes it, and `AddOsirion()` registers enhanced navigation and the scroll-to-top
button with their defaults. With this package alone:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Navigation.Extensions;

builder.Services.AddOsirionNavigation(navigation => navigation
    .AddEnhancedNavigation(options => options.Behavior = ScrollBehavior.Smooth)
    .AddScrollToTop(options => options.VisibilityThreshold = 400));
```

Add `@using Osirion.Blazor.Navigation.Components` to `_Imports.razor`. The enums `ScrollBehavior` and `Position`
are in `Osirion.Blazor.Components`. `Menu` and its parts need no registration. `ScrollToTopProvider` needs the
`ScrollToTopManager` that `AddScrollToTop` registers, so call it (or `AddOsirion()`) before you use the provider.

## Configuration

`AddOsirionNavigation(builder.Configuration)` and `AddOsirion(builder.Configuration)` read the settings from
configuration. `AddOsirionNavigation(configuration)` registers a component only when its section exists:

```json
{
  "Osirion": {
    "Navigation": {
      "Enhanced": {
        "Behavior": "Smooth",
        "ResetScrollOnNavigation": true,
        "PreserveScrollForSamePageNavigation": true
      },
      "ScrollToTop": {
        "Position": "BottomRight",
        "VisibilityThreshold": 300,
        "Title": "Back to top"
      }
    }
  }
}
```

`Osirion:Navigation:Enhanced` binds `EnhancedNavigationOptions`:

| Option | Type | Default |
| --- | --- | --- |
| `Behavior` | `ScrollBehavior` | `Auto` |
| `ResetScrollOnNavigation` | `bool` | `true` |
| `PreserveScrollForSamePageNavigation` | `bool` | `true` |

`Osirion:Navigation:ScrollToTop` binds `ScrollToTopOptions`:

| Option | Type | Default |
| --- | --- | --- |
| `Position` | `Position` | `BottomRight` |
| `Behavior` | `ScrollBehavior` | `Smooth` |
| `VisibilityThreshold` | `int` | `300` |
| `Text` | `string?` | `null` |
| `Title` | `string` | `"Scroll to top"` |
| `CssClass` | `string?` | `null` |
| `CustomIcon` | `string?` | `null` |
| `IconCssClass` | `string?` | `null` |
| `Enabled` | `bool` | `true` |

A parameter on the component overrides the configured value for that place. The meaning of each option is on the
[ScrollToTop](/components/scroll-to-top) and [EnhancedNavigation](/components/enhanced-navigation) pages.

## Related

- [Theming](/docs/packages/theming): put a [`ThemeToggle`](/components/theme-toggle) in the menu.
- [CMS navigation](/components/cms-navigation): menus, breadcrumbs and a table of contents built from your
  content.
- [Navigation components](/components/navigation): the group of navigation pages, including breadcrumbs and read-more
  links.
- [Osirion.Blazor.Navigation on NuGet](https://www.nuget.org/packages/Osirion.Blazor.Navigation)
