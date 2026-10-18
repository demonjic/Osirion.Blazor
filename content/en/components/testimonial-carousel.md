---
id: 'osirion-testimonial-carousel'
order: 8
layout: docs
title: OsirionTestimonialCarousel
permalink: /components/testimonial-carousel
description: OsirionTestimonialCarousel shows testimonial cards in a continuously looping rail, driven by CSS, with a pause button and a static grid under reduced motion.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-testimonial-carousel.jpg'
categories:
- Components
- Core Components
- Carousels
tags:
- blazor
- carousel
- testimonials
- reviews
- accessibility
- static-ssr
is_featured: true
published: true
slug: testimonial-carousel
lang: en
redirect_from:
- /docs/components/core/carousels/testimonial-carousel
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionTestimonialCarousel: testimonial rail for Blazor'
  description: 'Show testimonial cards in a looping rail with ratings, LinkedIn links and read more links; it pauses on focus and respects reduced motion.'
  image: 'https://media.getosirion.com/og/components-testimonial-carousel.jpg'
  canonical: 'https://getosirion.com/components/testimonial-carousel'
  lang: en
  robots: index, follow
  og_title: 'OsirionTestimonialCarousel Component - Osirion.Blazor'
  og_description: 'Testimonial cards in a looping rail, driven by CSS, with a pause button and reduced motion support.'
  og_image_url: 'https://media.getosirion.com/og/components-testimonial-carousel.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionTestimonialCarousel Component - Osirion.Blazor'
  twitter_description: 'Testimonial cards in a looping rail, driven by CSS.'
  twitter_image_url: 'https://media.getosirion.com/og/components-testimonial-carousel.jpg'
  meta_tags: {}
---

# OsirionTestimonialCarousel

`OsirionTestimonialCarousel` shows testimonials as a rail of cards that loops continuously. Each card is an
[OsirionTestimonialCard](/components/testimonial-card) with the person's name, role, company, quote
and optionally a photo, star rating and links. The loop is a CSS animation, so it runs under static server-side
rendering without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Carousels/OsirionTestimonialCarousel.razor).

```demo
testimonial-carousel
```

## Usage

The enhancement script (dragging, wheel scrolling, pause button) loads through BlazorJSComponents, which `AddOsirion` registers. If you register
Osirion.Blazor.Core without `AddOsirion`, add the loader in `Program.cs`:

```csharp
builder.Services.AddJSComponents();
```

Always pass your own testimonials. Without `CustomTestimonials`, the component shows eight built-in sample testimonials from invented people, with photos that load from `images.unsplash.com`.

```razor
@using Osirion.Blazor.Components

<OsirionTestimonialCarousel Title="What our customers say" CustomTestimonials="testimonials" />

@code {
    private readonly List<TestimonialItem> testimonials =
    [
        new("Jane Doe", "CTO", "Northwind Traders", "We moved our docs site to static SSR in a week."),
        new("Sam Lee", "Lead Developer", "Fabrikam", "The components render the same with and without script."),
        new("Ana Ruiz", "Product Manager", "Contoso", "Our editors publish from GitHub without touching code."),
    ];
}
```

With photos, ratings and links to case studies:

```razor
<OsirionTestimonialCarousel Title="Customer stories"
                            SectionTitle="Customer testimonials"
                            CustomTestimonials="testimonials"
                            ShowRating="true"
                            CardVariant="TestimonialVariant.Highlighted"
                            CardHeight="340"
                            Speed="CarouselSpeed.Slow" />

@code {
    private readonly List<TestimonialItem> testimonials =
    [
        new("Jane Doe", "CTO", "Northwind Traders", "We moved our docs site to static SSR in a week.",
            ProfileImageUrl: "/images/people/jane.jpg",
            ShowRating: true, Rating: 5,
            ReadMoreHref: "/customers/northwind", ReadMoreText: "Read the case study"),
        new("Sam Lee", "Lead Developer", "Fabrikam", "The components render the same with and without script.",
            ProfileImageUrl: "/images/people/sam.jpg",
            LinkedInUrl: "https://www.linkedin.com/in/your-profile",
            ShowRating: true, Rating: 4),
        new("Ana Ruiz", "Product Manager", "Contoso", "Our editors publish from GitHub without touching code.",
            ShowRating: true, Rating: 5),
    ];
}
```

A star rating shows only when both the carousel's `ShowRating` and the item's `ShowRating` are `true`.

## Parameters

<!-- osirion:parameters OsirionTestimonialCarousel -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading above the rail, rendered as an `h2`; nothing is rendered when it is empty. |
| `SectionTitle` | `string` | `"Customer Testimonials"` | The accessible name (`aria-label`) of the `section`. |
| `AnimationDuration` | `int` | `60` | The seconds one full loop takes at `Normal` speed. Lower is faster. |
| `CustomTestimonials` | `List<TestimonialItem>?` | `null` | The testimonials. Without it the carousel shows built-in sample testimonials, so always pass your own. Fewer than three are repeated on the rail to close the loop; your list is not changed. |
| `PauseOnHover` | `bool` | `true` | Whether the loop pauses while the pointer is over the carousel. Keyboard focus inside it always pauses the loop. |
| `Direction` | `AnimationDirection` | `AnimationDirection.Right` | `Right` moves the cards from right to left, `Left` from left to right. |
| `CardWidth` | `int` | `400` | The width of each card in pixels. |
| `CardHeight` | `int` | `300` | The fixed height of each card in pixels; size it for your longest quote. |
| `CardGap` | `int` | `24` | The gap between cards in pixels. |
| `MaxVisibleTestimonials` | `int?` | `null` | Shows only the first N testimonials. A value of zero or less, or none, shows all of them. |
| `CardVariant` | `TestimonialVariant` | `TestimonialVariant.Default` | The cards' look: `Default`, `Minimal`, `Highlighted` or `Compact`. |
| `CardSize` | `TestimonialSize` | `TestimonialSize.Normal` | The cards' size: `Small`, `Normal` or `Large`. |
| `CardElevated` | `bool?` | `null` | Whether the cards are elevated, for every testimonial that does not set its own `Elevated`. |
| `CardBorderless` | `bool` | `false` | Whether the cards are drawn without a border. |
| `Speed` | `CarouselSpeed` | `CarouselSpeed.Normal` | Changes the loop time: `Slow` adds 30 seconds to `AnimationDuration`, `Fast` takes 30 off, to a minimum of 10. |
| `ShowRating` | `bool` | `false` | Whether star ratings are allowed; a card shows its rating only when its testimonial's `ShowRating` is also on. |
| `InheritTheme` | `bool` | `false` | Whether the cards take the carousel's `OsirionComponentBase.Theme` instead of following the system. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the `<section>`.

### TestimonialItem

`TestimonialItem` is a record: `new TestimonialItem(Name, Position, Company, TestimonialText, ...)` with the other
values named.

| Value | Type | Default | Description |
| --- | --- | --- | --- |
| `Name` | `string` | required | The person's name, rendered in a `<cite>`. |
| `Position` | `string?` | `null` | Job title. With `Company`, shown as "Position at Company". |
| `Company` | `string?` | `null` | Company name. |
| `TestimonialText` | `string?` | `null` | The quote, as plain text. |
| `ProfileImageUrl` | `string?` | `null` | Photo; its alt text is built from the name, position and company. |
| `LinkedInUrl` | `string?` | `null` | Turns the name into a link that opens in a new tab. |
| `ShowRating` | `bool` | `false` | Shows this item's rating (when the carousel's `ShowRating` is on too). |
| `Rating` | `int` | `5` | Stars out of five. |
| `ReadMoreHref` | `string?` | `null` | Adds a read more link at the bottom of the card. |
| `ReadMoreText` | `string?` | `"Read more"` | Text of that link. |
| `ReadMoreVariant` | `ReadMoreVariant` | `Default` | Style of that link. |
| `ReadMoreTarget` | `string?` | `null` | `target` of that link. |
| `AdditionalCssClass` | `string?` | `null` | Extra classes on this card. |
| `Elevated` | `bool?` | `null` | Elevation for this card; overrides `CardElevated`. |

## Behavior

- **Without script** the rail loops with a CSS animation. The cards are rendered twice so the loop has no seam; the
  second set is `aria-hidden` and `inert`, so screen readers read each testimonial once and its links are not in
  the tab order.
- **With script** the visitor can drag the rail or move it with a horizontal wheel or trackpad gesture, a pause button
  appears in the top right corner, and the photos, stars and LinkedIn icons are shown in color only on the cards
  nearest the center (and on the card under the pointer).
- **Pausing.** Hover pauses the loop when `PauseOnHover` is on; keyboard focus inside the carousel always pauses it.
  Translate the pause button with `data-pause-label` and `data-play-label` attributes on the component.
- **Reduced motion.** Under `prefers-reduced-motion: reduce` the animation stops and every testimonial is shown once,
  in a wrapped grid of two columns (one on narrow screens).
- **Fewer than three items.** The rail repeats items until it holds three, so the loop has no gap. The repeats are real cards, not hidden copies, and the list you passed is not changed. Pass at least three testimonials to avoid them. An empty list renders an empty rail.
- **MaxVisibleTestimonials.** When it is a positive number, only that many testimonials are shown, and a value below three is padded by repeating them as above. Zero or less shows all of them.
- **Images.** The photos of the first two cards load eagerly with high fetch priority; the rest load lazily.
- **Text and URLs.** Names, roles and quotes are rendered as text, so Razor encodes them. `LinkedInUrl`,
  `ReadMoreHref` and `ProfileImageUrl` are written as given, so pass only URLs you trust.

## Styling

| Class or property | Purpose |
| --- | --- |
| `osirion-testimonial-carousel` | The `<section>` |
| `osirion-testimonial-carousel-title` | The heading |
| `osirion-testimonial-carousel-track` | The moving rail |
| `osirion-testimonial-carousel-slide` | The slot around each card |
| `osirion-carousel-pause` | The pause button the script adds |
| `--osirion-testimonial-carousel-duration`, `--osirion-testimonial-card-width`, `--osirion-testimonial-card-height`, `--osirion-testimonial-card-gap` | Set on the rail from the parameters |

Spacing, font sizes and colors come from the theme tokens (`--osirion-spacing-*`, `--osirion-font-size-*`,
`--osirion-text-primary`), so a CSS framework adapter restyles the carousel with the rest of the site. The cards have
their own classes; see the [testimonial card](/components/testimonial-card) page.

## Related

- [Testimonial card](/components/testimonial-card)
- [InfiniteLogoCarousel](/components/infinite-logo-carousel)
- [Carousels](/components/sections)
