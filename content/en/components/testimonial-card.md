---
id: 'osirion-testimonial-card'
order: 2
layout: docs
title: OsirionTestimonialCard
permalink: /components/testimonial-card
description: OsirionTestimonialCard shows a quote with the person's name, photo, role, an optional star rating and a link. Parameters and behavior.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-testimonial-card.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- testimonial-card
- testimonials
- cards
- rating
- osirion-blazor-core
is_featured: true
published: true
slug: testimonial-card
lang: en
redirect_from:
- /docs/components/core/cards/testimonial-card
custom_fields:
  group: cards
seo_properties:
  title: 'OsirionTestimonialCard: testimonial cards for Blazor'
  description: 'OsirionTestimonialCard shows a quote with the person''s name, photo, role, an optional star rating and a link. Parameters and behavior.'
  image: 'https://media.getosirion.com/og/components-testimonial-card.jpg'
  canonical: 'https://getosirion.com/components/testimonial-card'
  lang: en
  robots: index, follow
  og_title: 'OsirionTestimonialCard - Osirion.Blazor'
  og_description: 'A quote card with name, photo, role, optional rating and a read more link. Renders without script.'
  og_image_url: 'https://media.getosirion.com/og/components-testimonial-card.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionTestimonialCard - Osirion.Blazor'
  twitter_description: 'A quote card with name, photo, role, optional rating and a read more link.'
  twitter_image_url: 'https://media.getosirion.com/og/components-testimonial-card.jpg'
  meta_tags: {}
---

# OsirionTestimonialCard

`OsirionTestimonialCard` shows a quote with the name, photo, position and company of the person who said it, an
optional one to five star rating and an optional link to the full text. Use it on its own or let
`OsirionTestimonialCarousel` render a row of them. It needs no script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionTestimonialCard.razor).

```demo
testimonial-card
```

## Usage

```razor
<OsirionTestimonialCard Name="Ana Petrović"
                        TestimonialText="We moved our documentation site to static SSR in a weekend." />
```

A card with a photo, rating and link to the case study:

```razor
<OsirionTestimonialCard Name="Ana Petrović"
                        Position="Engineering lead"
                        Company="Example Ltd"
                        ProfileImageUrl="/images/people/ana.jpg"
                        LinkedInUrl="https://www.linkedin.com/in/example"
                        TestimonialText="We moved our documentation site to static SSR in a weekend."
                        ShowRating="true"
                        Rating="5"
                        ReadMoreHref="/case-studies/example"
                        ReadMoreText="Read the case study"
                        IsFirstVisible="true" />
```

For formatted text, pass `TestimonialContent` instead of `TestimonialText`:

```razor
<OsirionTestimonialCard Name="Ana Petrović">
    <TestimonialContent>
        <p>Page weight went <strong>down</strong> and our search traffic went up.</p>
    </TestimonialContent>
</OsirionTestimonialCard>
```

## Parameters

<!-- osirion:parameters OsirionTestimonialCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ProfileImageUrl` | `string?` | `null` | The person's photo. Its alternative text is built from the name, position and company. |
| `Name` | `string` | `""` | The person's name, rendered in a `cite` element. Required. |
| `Company` | `string?` | `null` | The company's name. |
| `Position` | `string?` | `null` | The person's job title. With `Company` it reads "Position at Company". |
| `LinkedInUrl` | `string?` | `null` | A profile address. When set, the name is a link that opens in a new tab with `rel="noopener noreferrer"`. |
| `TestimonialText` | `string?` | `null` | The quote as plain text. |
| `TestimonialContent` | `RenderFragment?` | `null` | The quote as markup; replaces `TestimonialText`. |
| `ReadMoreHref` | `string?` | `null` | The read more link's target. The link shows only when this is set. |
| `ReadMoreText` | `string` | `"Read full testimonial"` | The read more link's text. |
| `ReadMoreVariant` | `ReadMoreVariant` | `ReadMoreVariant.Default` | The read more link's look: `Default`, `Arrow`, `External`, `Download`, `Plain` or `Button`. |
| `ReadMoreTarget` | `string?` | `null` | The read more link's `target`, for example `_blank`. |
| `ShowRating` | `bool` | `false` | Whether the star rating is shown. |
| `Rating` | `int` | `5` | The number of filled stars, clamped to 0 to 5; 0 hides the rating. |
| `Variant` | `TestimonialVariant` | `TestimonialVariant.Default` | The card's look: `Default`, `Minimal`, `Highlighted` or `Compact`. |
| `Size` | `TestimonialSize` | `TestimonialSize.Normal` | The card's size: `Small`, `Normal` or `Large`. |
| `ImageSize` | `int` | `64` | The photo's width and height in pixels. Left at 64, it becomes 48 for `Small` and 80 for `Large`. |
| `Elevated` | `bool?` | `null` | Whether the card is raised with a drop shadow; `null` means it is. |
| `Borderless` | `bool` | `false` | Whether the border and background are removed. |
| `IsHiddenFromAssistiveTech` | `bool` | `false` | Sets `tabindex="-1"` on the card's links, for copies inside an `aria-hidden` container such as a looping carousel. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content under the quote. |
| `IsFirstVisible` | `bool` | `false` | Loads the photo eagerly with `fetchpriority="high"`, for a card above the fold; otherwise it loads lazily. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- The rating is an element with `role="img"` and the label "Rating: 5 out of 5 stars"; the star icons are hidden from
  assistive technology.
- The quote sits in a `blockquote`. Plain `TestimonialText` is clamped to four lines on a normal card (two on `Small`,
  `Minimal` and `Compact`, five on `Large`), so keep quotes short or link to the full text.
- The card has a fixed height range (280px to 320px on a normal card), so cards in a row line up.
- Text is HTML-encoded. `LinkedInUrl` and `ReadMoreHref` are written as given, so pass URLs your application
  controls.
- The generated alternative text and labels ("Profile photo of", "at", "View ... LinkedIn profile") are English.

## Styling

The root element has `osirion-testimonial-card` plus `osirion-testimonial-{variant}`, `osirion-testimonial-{size}`,
`osirion-testimonial-theme-{theme}` and, when set, `osirion-testimonial-elevated` and `osirion-testimonial-borderless`.
Set these custom properties on the card or a parent to change its look:

| Custom property | Default |
| --- | --- |
| `--osirion-testimonial-card-background` | the primary background at 72% opacity |
| `--osirion-testimonial-card-border` | the primary text color at 10% opacity |
| `--osirion-testimonial-card-radius` | `--osirion-radius-xl` |
| `--osirion-testimonial-card-shadow` | a thin inset highlight |
| `--osirion-testimonial-action-color` | `--osirion-text-primary`, for the read more link |
| `--osirion-testimonial-action-hover-color` | `--osirion-text-primary` |

## Related

- [Testimonial carousel](/components/testimonial-carousel)
- [OsirionReadMoreLink](/components/read-more-link)
- [Cards overview](/components/cards)
