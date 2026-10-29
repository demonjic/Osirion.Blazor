---
id: 'core-cards-components-overview'
order: 1
layout: docs
title: Cards
permalink: /components/cards
description: The card components in Osirion.Blazor.Core (feature, testimonial, subscription, metric and author cards) and when to use each.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-cards.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- cards
- feature-card
- testimonial-card
- subscription-card
- author-card
is_featured: true
published: true
slug: cards
lang: en
redirect_from:
- /docs/components/core/cards
custom_fields:
  group: cards
seo_properties:
  title: 'Blazor card components'
  description: 'The card components in Osirion.Blazor.Core (feature, testimonial, subscription, metric and author cards) and when to use each.'
  image: 'https://media.getosirion.com/og/components-cards.jpg'
  canonical: 'https://getosirion.com/components/cards'
  lang: en
  robots: index, follow
  og_title: 'Cards - Osirion.Blazor'
  og_description: 'Feature, testimonial, subscription, metric and author cards for Blazor content sites.'
  og_image_url: 'https://media.getosirion.com/og/components-cards.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Cards - Osirion.Blazor'
  twitter_description: 'Feature, testimonial, subscription, metric and author cards for Blazor content sites.'
  twitter_image_url: 'https://media.getosirion.com/og/components-cards.jpg'
  meta_tags: {}
---

# Cards

Osirion.Blazor.Core has five card components and a grid to lay them out. All of them render complete HTML under static SSR and live in the
`Osirion.Blazor.Components` namespace.

| Component | Use it for |
| --- | --- |
| [OsirionFeatureCard](/components/feature-card) | A feature, service or article teaser: image, title, description, a read more link and category links. |
| [OsirionTestimonialCard](/components/testimonial-card) | A quote with the person's name, photo, role and an optional star rating. |
| [OsirionSubscriptionCard](/components/subscription-card) | A newsletter signup form that posts without script, with validation and topic choices. |
| [OsirionAuthorCard](/components/author-card) | An author's avatar or initials, name, role, bio and profile links. Available from 4.0. |
| [OsirionMetricCard](/components/metric-card) | One figure with a label and description, optionally counting up when it scrolls into view. |
| [OsirionCardGrid](/components/card-grid) | Cards in up to four equal columns that collapse on narrow screens. Available from 4.3. |

```razor
@using Osirion.Blazor.Components

<OsirionFeatureCard Title="Static SSR first"
                    Description="Every component renders meaningful HTML before any script runs." />

<OsirionTestimonialCard Name="Ana Petrović"
                        TestimonialText="We moved our documentation site to static SSR in a weekend." />
```

To show several testimonials in a scrolling row, use the
[testimonial carousel](/components/testimonial-carousel), which renders `OsirionTestimonialCard`
for each item.

Each card passes unmatched attributes to its root element and takes extra CSS classes in `Class` (`OsirionMetricCard`
also takes `CssClass`, its older name). The cards take their colors, borders and spacing from the Osirion theme tokens, so a CSS
framework adapter restyles them along with the rest of the site.

## Related

- [Core components overview](/docs/packages/core)
- [Sections](/components/sections), for pricing tables, steps and metric grids
