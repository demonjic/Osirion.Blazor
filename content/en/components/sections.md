---
id: 'core-sections-components-overview'
order: 1
layout: docs
title: Sections
permalink: /components/sections
description: Page sections from Osirion.Blazor Core, from heroes and content sections to pricing tables and numbered steps, rendering under static SSR.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-sections.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- sections
- hero
- pricing
- static-ssr
is_featured: true
published: true
slug: sections
lang: en
redirect_from:
- /docs/components/core/carousels
- /docs/components/core/sections
custom_fields:
  group: sections
seo_properties:
  title: 'Blazor section components'
  description: 'Hero, content section, contact details, pricing table, feature comparison and steps components for Blazor pages, rendering under static SSR.'
  image: 'https://media.getosirion.com/og/components-sections.jpg'
  canonical: 'https://getosirion.com/components/sections'
  lang: en
  robots: index, follow
  og_title: 'Core Section Components - Osirion.Blazor'
  og_description: 'Full-width page sections for Blazor landing pages and articles, rendering under static SSR.'
  og_image_url: 'https://media.getosirion.com/og/components-sections.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Core Section Components - Osirion.Blazor'
  twitter_description: 'Full-width page sections for Blazor landing pages and articles, rendering under static SSR.'
  twitter_image_url: 'https://media.getosirion.com/og/components-sections.jpg'
  meta_tags: {}
---

# Sections

Section components build the blocks of a landing page or the top of an article: a hero, a titled content band,
contact details, pricing, the steps of a process, metrics, detail cards and a closing call to action. Each renders complete HTML under static SSR and needs no script,
except the responsive showcase, which is a demo tool for interactive pages, and the count-up of a metric card with `Animate`.

Most of them live in the `Osirion.Blazor.Components` namespace of the Osirion.Blazor.Core package, and their data
types in `Osirion.Blazor.Core.Models`:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models
```

## Components

| Component | What it renders | Script | Available |
| --- | --- | --- | --- |
| [Hero section](/components/hero-section) | A page header with title, subtitle, buttons and a side or background image | None | 4.0 |
| [Base section](/components/base-section) | A `<section>` with heading, description, container and background options | None | 4.0 |
| [Contact info section](/components/contact-info-section) | Address, phone numbers, email addresses and websites as links | None | 4.0 |
| [Pricing table and feature comparison](/components/pricing-table) | Plans side by side, and a table comparing their features | None | 4.0 |
| [Steps](/components/steps) | Numbered "how it works" steps as an ordered list | None | 4.0 |
| [Call to action section](/components/call-to-action-section) | The closing heading, short text and up to two buttons of a page | None | 4.3 |
| [Responsive showcase](/components/responsive-showcase-section) | A preview frame that switches between desktop, tablet and mobile widths | Interactive render mode | 4.0 |
| [Metric grid](/components/metric-grid) | Metric cards in a responsive list | Only for animated cards | 3.0 |
| [Context card grid](/components/context-card-grid) | A heading and labeled detail cards, each with an accent color | None | 3.0 |
| [Page heading](/components/page-heading) | A page title as the `h1` with an optional sentence, for pages without a hero | None | 4.0 |

## Carousels

Two carousels fill a band of a marketing page: a strip of client or partner logos, and a rail of testimonial cards.
Both render every item as HTML under static server-side rendering and move with a CSS animation. A small script adds
dragging, horizontal wheel scrolling and a pause button.

| Component | What it shows | Moves by default |
| --- | --- | --- |
| [InfiniteLogoCarousel](/components/infinite-logo-carousel) | Logos, optionally linked, with light and dark variants | No, set `EnableAutoScroll` |
| [OsirionTestimonialCarousel](/components/testimonial-carousel) | Testimonial cards with name, role, quote and rating | Yes |

### What they share

- **Setup.** The enhancement script loads through BlazorJSComponents, which `AddOsirion` registers. Without
  `AddOsirion`, add `builder.Services.AddJSComponents();` in `Program.cs`.
- **Your own items.** Pass your logos or testimonials. Without them, both components show built-in sample content
  meant for demos, with images that load from external addresses.
- **Motion.** The animation pauses while the pointer is over the carousel (unless `PauseOnHover` is off) and while
  keyboard focus is inside it. While the carousel animates, the script adds a pause button, and it runs its
  per-frame work only while the carousel is on screen and moving. Vertical wheel scrolling always reaches the page.
- **Reduced motion.** Under `prefers-reduced-motion: reduce` the animation stops; the testimonial carousel then shows
  its cards as a static wrapped grid.
- **Looping copies.** The items repeated to close the loop are `aria-hidden` and `inert`, so screen readers
  read each item once and their links are not in the tab order.
- **Labels.** Each carousel is a `<section>` named by `SectionTitle`. The pause button's labels default to "Pause
  animation" and "Play animation"; set `data-pause-label` and `data-play-label` attributes on the component to
  translate them.

## Related

- [Background pattern](/components/background-pattern)
- [Cards](/components/cards), including the testimonial card the carousel repeats
- [Core components](/docs/packages/core)
