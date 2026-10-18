---
id: 'core-navigation-components-overview'
order: 1
layout: docs
title: Navigation
permalink: /components/navigation
description: The navigation components in Osirion.Blazor (breadcrumbs, read more link, menu, scroll to top and enhanced navigation) and when to use each.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-navigation.jpg'
categories:
- Components
- Core Components
- Navigation
tags:
- blazor
- navigation
- breadcrumbs
- links
- static-ssr
is_featured: true
published: true
slug: navigation
lang: en
redirect_from:
- /docs/components/core/navigation
custom_fields:
  group: navigation
seo_properties:
  title: 'Blazor navigation components'
  description: 'The navigation components in Osirion.Blazor (breadcrumbs, read more link, menu, scroll to top and enhanced navigation) and when to use each.'
  image: 'https://media.getosirion.com/og/components-navigation.jpg'
  canonical: 'https://getosirion.com/components/navigation'
  lang: en
  robots: index, follow
  og_title: 'Core Navigation Components - Osirion.Blazor'
  og_description: 'Breadcrumb trails and read more links for Blazor that render without script.'
  og_image_url: 'https://media.getosirion.com/og/components-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Core Navigation Components - Osirion.Blazor'
  twitter_description: 'Breadcrumb trails and read more links for Blazor that render without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-navigation.jpg'
  meta_tags: {}
---

# Navigation

Osirion.Blazor has five navigation components in two packages. Breadcrumbs and read more links are in
Osirion.Blazor.Core and render plain links. The menu, the scroll to top button and enhanced navigation scroll handling
are in Osirion.Blazor.Navigation. All of them work under static SSR; only the scroll to top button and
`EnhancedNavigation` write a small inline script.

| Component | Use it for | Package | Script |
| --- | --- | --- | --- |
| [OsirionBreadcrumbs](/components/breadcrumbs) | A trail of links built from a URL path, with the current page last. | `Osirion.Blazor.Core` | None |
| [OsirionReadMoreLink](/components/read-more-link) | A "read more" link or small call to action with an optional icon, in several variants and sizes. | `Osirion.Blazor.Core` | None |
| [Menu](/components/menu) | A site menu with submenus, mega menus and a mobile drawer, and the parts that fill it. | `Osirion.Blazor.Navigation` | None |
| [ScrollToTop](/components/scroll-to-top) | A fixed "back to top" button that appears after a scroll threshold, and its provider. | `Osirion.Blazor.Navigation` | Inline script |
| [EnhancedNavigation](/components/enhanced-navigation) | Scrolling to the top of the new page after a Blazor enhanced navigation. No visible output. | `Osirion.Blazor.Navigation` | Inline script |

The Core components are in the `Osirion.Blazor.Components` namespace and the Navigation components in
`Osirion.Blazor.Navigation.Components`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Navigation.Components
```

`OsirionBreadcrumbs` and `OsirionReadMoreLink` are available from 4.0.

The Navigation package needs registration for the scroll components; see the
[package page](/docs/packages/navigation).

## Other navigation

These components cover site navigation and links inside a page. The CMS has components that read your content:
`ContentBreadcrumbs`, `DirectoryNavigation`, `TableOfContents`, `TagCloud`, `CategoriesList`, `SearchBox` and previous
and next links. See [CMS navigation](/components/cms-navigation).

## Related

- [Navigation package](/docs/packages/navigation): setup and configuration
- [CMS navigation](/components/cms-navigation)
- [Core components](/docs/packages/core)
