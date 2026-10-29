---
id: 'core-components-overview'
order: 1
layout: docs
title: Osirion.Blazor.Core
permalink: /docs/packages/core
description: Osirion.Blazor.Core holds the layout, content, card, section, navigation and form components. Setup steps and every category.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/docs-packages-core.jpg'
categories:
- Components
- Core Components
- Documentation
tags:
- core-components
- osirion-blazor-core
- static-ssr
- blazor-components
is_featured: true
published: true
slug: core
lang: en
redirect_from:
- /docs/components/core
- /docs/components/core/core
custom_fields:
  group: packages
seo_properties:
  title: 'Core components - Osirion.Blazor'
  description: 'Osirion.Blazor.Core holds the layout, content, card, section, navigation and form components. Setup steps and every category.'
  image: 'https://media.getosirion.com/og/docs-packages-core.jpg'
  canonical: 'https://getosirion.com/docs/packages/core'
  lang: en
  robots: index, follow
  og_title: 'Core components - Osirion.Blazor'
  og_description: 'Layout, content, card, section, navigation and form components that render under static SSR.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages-core.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Core components - Osirion.Blazor'
  twitter_description: 'Layout, content, card, section, navigation and form components that render under static SSR.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages-core.jpg'
  meta_tags: {}
---

# Osirion.Blazor.Core
Osirion.Blazor.Core is the package with the page building blocks: layout, content, cards, sections, navigation, forms
and states. Every component renders complete HTML under static SSR; where a component uses script, the script adds
to markup that already works (the exceptions are `OsirionReveal` and `OsirionStaticContactForm`, see Rendering and
Forms below). Version 4.0.0 targets .NET 9 and .NET 10.

## Setup

Install the meta package, which includes Core, the theming package that loads its styles, and the `AddOsirion`
registration used below:

```bash
dotnet add package Osirion.Blazor
```

Register the services in `Program.cs`:

```csharp
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(builder.Configuration);
```

`AddOsirion` also registers the BlazorJSComponents loader through which the components that enhance themselves with a
script (page layout, carousels, metric cards, image gallery, reveal, video embeds, the HTML renderer) load it.

Add the namespaces to `_Imports.razor`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Theming.Components
```

Render `<OsirionStyles />` once in your layout. It links the Core stylesheet and the components' scoped CSS, and the
adapter for your CSS framework; see [OsirionStyles](/components/osirion-styles).

## Categories

| Category | Components |
| --- | --- |
| [Layout](/components/layout) | [`OsirionPageLayout`](/components/page-layout) (header, body and footer slots, sticky footer, and from 4.0 a skip link to the main content), `OsirionFooter`, `OsirionStickySidebar`, `OsirionBackgroundPattern`, `OsirionPageLoading`. |
| [Content](/components/content) | [`OsirionArticleMetdata`](/components/article-metadata) (author, date, read time). From 4.0: [callouts](/components/callout), [tabs](/components/tabs), [accordion and FAQ](/components/accordion-and-faq), [responsive image](/components/responsive-image), [video embed](/components/video-embed), [reading progress](/components/reading-progress). From 4.3: [checklist](/components/checklist). |
| [Cards](/components/cards) | Feature, testimonial, subscription and metric cards, and from 4.0 [`OsirionAuthorCard`](/components/author-card). From 4.3: [`OsirionCardGrid`](/components/card-grid). |
| [Sections](/components/sections) | `HeroSection`, `OsirionBaseSection`, `OsirionContactInfoSection`, `OsirionResponsiveShowcaseSection`, `OsirionMetricGrid`, `OsirionContextCardGrid`. From 4.0: `OsirionPricingTable`, `OsirionFeatureComparison`, `OsirionSteps`. From 4.3: `OsirionCallToActionSection`. |
| [Navigation](/components/navigation) | `OsirionBreadcrumbs` and `OsirionReadMoreLink`. Menus and scroll helpers are in the [Navigation package](/docs/packages/navigation). |
| [Carousels](/components/sections) | `InfiniteLogoCarousel` and `OsirionTestimonialCarousel`. |
| [Forms](/components/forms-and-feedback) | `OsirionContactForm`, which posts under static SSR, and [`OsirionStaticContactForm`](/components/static-contact-form) for sites published as static files, which sends JSON to your endpoint with a script: it renders hidden and only its script shows it. The newsletter form is a card: `OsirionSubscriptionCard`. |
| [Popups](/components/forms-and-feedback) | [`OsirionCookieConsent`](/components/cookie-consent), and from 4.0 [`OsirionAnnouncementBar`](/components/announcement-bar), a site-wide notice that closes without script. |
| [Rendering](/components/content) | `OsirionHtmlRenderer`, which sanitizes HTML and highlights code, and `OsirionReveal`, which fades content in as it scrolls into view. With `Animate` on (the default) the content starts hidden and only its script shows it, so do not wrap content that must be visible without script. |
| [States](/components/forms-and-feedback) | `OsirionContentNotFound` for missing content. |

`OsirionImageGallery`, a responsive image grid with an optional lightbox, is also part of Core and has no page yet.

## Shared parameters

Core components derive from `OsirionComponentBase` (all except `OsirionResponsiveShowcaseSection`), which gives them
these parameters:

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Class` | `string?` | `null` | Extra CSS classes. Most components add them to their root element. |
| `Style` | `string?` | `null` | Inline style, on the components that render it. |
| `Theme` | `ThemeMode` | `System` | `Light`, `Dark` or `Inverted` writes that value in `data-theme` (`data-bs-theme` with Bootstrap) on the component; `System` writes nothing. |
| `Attributes` | `IDictionary<string, object>` | empty | Captures unmatched attributes such as `id` or `aria-label`. |

`SetInteractive` is obsolete and fails the build; components read the render mode from `RendererInfo` themselves.

## Related

- [Theming components](/docs/packages/theming)
- [CMS web components](/docs/packages/cms-web)
- [All components](/components)
