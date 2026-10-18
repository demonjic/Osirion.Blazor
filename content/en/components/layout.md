---
id: 'core-layout-components-overview'
order: 1
layout: docs
title: Layout
permalink: /components/layout
description: 'Layout components in Osirion.Blazor.Core: page shell, footer, sticky sidebar, loading spinner and background patterns, all working under static SSR.'
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-layout.jpg'
categories:
- Components
- Core Components
- Layout
tags:
- blazor
- layout
- page-layout
- footer
- sidebar
- static-ssr
is_featured: true
published: true
slug: layout
lang: en
redirect_from:
- /docs/components/core/layout
custom_fields:
  group: layout
seo_properties:
  title: 'Blazor layout components'
  description: 'Page shell, footer, sticky sidebar, loading spinner and background patterns for Blazor sites, all working under static SSR.'
  image: 'https://media.getosirion.com/og/components-layout.jpg'
  canonical: 'https://getosirion.com/components/layout'
  lang: en
  robots: index, follow
  og_title: 'Layout components - Osirion.Blazor'
  og_description: 'Page shell, footer, sticky sidebar, loading spinner and background patterns for Blazor sites.'
  og_image_url: 'https://media.getosirion.com/og/components-layout.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Layout components - Osirion.Blazor'
  twitter_description: 'Page shell, footer, sticky sidebar, loading spinner and background patterns.'
  twitter_image_url: 'https://media.getosirion.com/og/components-layout.jpg'
  meta_tags: {}
---

# Layout

These components build the frame of a page. All of them render their markup on the server and work under static
SSR; only `OsirionPageLayout` loads a small script, for the header that hides on scroll and for skip-link focus.

| Component | What it renders |
| --- | --- |
| [OsirionPageLayout](/components/page-layout) | `header`, `main` and `footer` with a sticky footer, a header that hides while scrolling down, and (from 4.0) a skip link. |
| [OsirionFooter](/components/footer) | Site footer with link columns, social links, copyright and legal links. |
| [OsirionStickySidebar](/components/sticky-sidebar) | An `aside` that stays in view and scrolls on its own, for a table of contents or docs navigation. |
| [OsirionPageLoading](/components/page-loading) | A centered CSS spinner with optional text. |
| [OsirionBackgroundPattern](/components/background-pattern) | A decorative CSS pattern layer behind a section. |

## A typical main layout

```razor
@inherits LayoutComponentBase
@using Osirion.Blazor.Components

<OsirionPageLayout>
    <Header>
        <NavMenu />
    </Header>
    <Body>
        @Body
    </Body>
    <Footer>
        <OsirionFooter CompanyName="Example Ltd" />
    </Footer>
</OsirionPageLayout>
```

`OsirionPageLayout` loads its script through BlazorJSComponents, which `AddOsirion` registers (without `AddOsirion`,
call `builder.Services.AddJSComponents()`). From 4.0, an
`OsirionFooter` inside the `Footer` slot renders a `div`, so the page has one footer landmark.

## Related

- [Popups](/components/forms-and-feedback): cookie consent and the announcement bar, which usually sit in the layout too.
- [Sections](/components/sections): hero and content sections that take a background pattern.
