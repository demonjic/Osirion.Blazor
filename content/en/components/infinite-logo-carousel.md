---
id: 'infinite-logo-carousel'
order: 7
layout: docs
title: InfiniteLogoCarousel
permalink: /components/infinite-logo-carousel
description: InfiniteLogoCarousel shows client or partner logos in a row that can loop continuously, with light and dark logo variants and optional links.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-infinite-logo-carousel.jpg'
categories:
- Components
- Core Components
- Carousels
tags:
- blazor
- carousel
- logos
- clients
- dark-mode
- accessibility
is_featured: true
published: true
slug: infinite-logo-carousel
lang: en
redirect_from:
- /docs/components/core/carousels/infinite-logo-carousel
custom_fields:
  group: sections
seo_properties:
  title: 'InfiniteLogoCarousel: logo carousel for Blazor'
  description: 'Show client or partner logos in a looping row with light and dark variants, links, grayscale and a pause button, rendered under static SSR.'
  image: 'https://media.getosirion.com/og/components-infinite-logo-carousel.jpg'
  canonical: 'https://getosirion.com/components/infinite-logo-carousel'
  lang: en
  robots: index, follow
  og_title: 'InfiniteLogoCarousel Component - Osirion.Blazor'
  og_description: 'Client or partner logos in a looping row with light and dark variants and optional links.'
  og_image_url: 'https://media.getosirion.com/og/components-infinite-logo-carousel.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'InfiniteLogoCarousel Component - Osirion.Blazor'
  twitter_description: 'Client or partner logos in a looping row with light and dark variants.'
  twitter_image_url: 'https://media.getosirion.com/og/components-infinite-logo-carousel.jpg'
  meta_tags: {}
---

# InfiniteLogoCarousel

`InfiniteLogoCarousel` shows a row of client, partner or technology logos. Turn on `EnableAutoScroll` and the row
loops continuously; leave it off for a static row. Logos can link somewhere and can have separate light and dark
images.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Carousels/InfiniteLogoCarousel.razor).

```demo
infinite-logo-carousel
```

## Usage

The enhancement script loads through BlazorJSComponents, which `AddOsirion` registers. If you register
Osirion.Blazor.Core without `AddOsirion`, add the loader in `Program.cs`:

```csharp
builder.Services.AddJSComponents();
```

Always pass your own logos. Without `CustomLogos`, the component shows built-in sample logos that load from external addresses (`cdn.tridesetri.com` and `placehold.co`) and link to other sites.

```razor
@using Osirion.Blazor.Components

<InfiniteLogoCarousel CustomLogos="logos" SectionTitle="Our clients" />

@code {
    private readonly LogoItem[] logos =
    [
        new("/images/clients/northwind.svg", "Northwind Traders"),
        new("/images/clients/fabrikam.svg", "Fabrikam"),
        new("/images/clients/adventure-works.svg", "Adventure Works"),
    ];
}
```

A looping strip with a heading, links and a logo that has light and dark versions:

```razor
<InfiniteLogoCarousel Title="Trusted by teams like these"
                      SectionTitle="Client logos"
                      CustomLogos="logos"
                      EnableAutoScroll="true"
                      AnimationDuration="40"
                      LogoWidth="160"
                      LogoHeight="64" />

@code {
    private readonly LogoItem[] logos =
    [
        new("/images/clients/northwind.svg", "Northwind Traders", Url: "/customers/northwind", Target: "_self"),
        new("/images/clients/contoso-light.svg", "Contoso",
            LightImageUrl: "/images/clients/contoso-light.svg",
            DarkImageUrl: "/images/clients/contoso-dark.svg"),
        new("/images/clients/fabrikam.svg", "Fabrikam", EnableGrayscale: false),
    ];
}
```

## Parameters

<!-- osirion:parameters InfiniteLogoCarousel -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading above the logos, rendered as an `h2`; nothing is rendered when it is empty. |
| `SectionTitle` | `string` | `"Our Clients"` | The accessible name (`aria-label`) of the `section`; set it to something that describes the logos. |
| `AnimationDuration` | `int` | `60` | The seconds one full loop takes when `EnableAutoScroll` is on. Lower is faster. |
| `CustomLogos` | `IReadOnlyList<LogoItem>?` | `null` | The logos. Without it the carousel shows built-in sample logos that load from external addresses, so always pass your own. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content rendered as one more item after the logos. |
| `EnableAutoScroll` | `bool` | `false` | Whether the row loops continuously with a CSS animation. With it off the row is static. |
| `PauseOnHover` | `bool` | `true` | Whether the loop pauses while the pointer is over the carousel. Keyboard focus inside it always pauses the loop. |
| `Direction` | `AnimationDirection` | `AnimationDirection.Right` | `Right` moves the logos from right to left, `Left` from left to right. |
| `EnableGrayscale` | `bool` | `true` | Whether logos are shown in grayscale until hovered; with script, the logos nearest the center are in color too. A logo's own `EnableGrayscale` overrides it. |
| `LogoWidth` | `int` | `200` | The width of each logo slot in pixels, also written as the images' `width`; slots are 160px wide below 768px. |
| `LogoHeight` | `int` | `80` | The height of each logo slot in pixels, also written as the images' `height`. |
| `LogoGap` | `int` | `24` | The gap between logos in pixels. |
| `MaxVisibleLogos` | `int?` | `null` | Shows only the first N logos. A value of zero or less, or none, shows all of them. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the `<section>`.

### LogoItem

`LogoItem` is a record: `new LogoItem(ImageUrl, AltText, ...)` with the optional values named.

| Value | Type | Default | Description |
| --- | --- | --- | --- |
| `ImageUrl` | `string` | required | The logo image. Not used when both light and dark images are set. |
| `AltText` | `string` | required | The image's alt text, and the link's `title` when the logo links. |
| `LightImageUrl` | `string?` | `null` | Image for the light theme. |
| `DarkImageUrl` | `string?` | `null` | Image for the dark theme. Both must be set for the light and dark switch to apply. |
| `Url` | `string?` | `null` | Makes the logo a link. |
| `Target` | `string?` | `"_blank"` when linked | The link's `target`. |
| `NoFollow` | `bool?` | `true` when linked | Adds `nofollow` to `rel`. |
| `NoOpener` | `bool?` | `true` when linked | Adds `noopener` to `rel`. |
| `EnableGrayscale` | `bool?` | `null` | Overrides the carousel's `EnableGrayscale` for this logo. |

## Behavior

- **Without script** the logos render as a list. Logos that do not fit the width are cut off at the faded edges, so
  either keep a static row short or turn on `EnableAutoScroll`. With auto-scroll, the CSS animation runs without
  script.
- **With script**, the logo nearest the center and its two neighbors are shown in color when `EnableGrayscale` is on.
  With auto-scroll on, the visitor can also drag the row or scroll it with a horizontal wheel or trackpad gesture,
  and a pause button appears in the top right corner.
- **Pausing.** Hover pauses the loop when `PauseOnHover` is on; keyboard focus inside the carousel always pauses it.
  Under `prefers-reduced-motion: reduce` the loop does not run.
- **Accessibility.** The `<section>` is named by `SectionTitle`, so set it to something that describes the logos. The
  logos are a list. With auto-scroll on, a second copy of the logos closes the loop; it is `aria-hidden` and `inert`, so
  screen readers read each logo once and the copy's links are not in the tab order. Translate the pause button with
  `data-pause-label` and `data-play-label` attributes, which go on the `<section>`.
- **Dark mode.** For a logo with both light and dark images, CSS shows the dark image when an ancestor has
  `data-bs-theme="dark"`, `data-theme="dark"` or one of the classes `dark-theme`, `theme-dark` or
  `osirion-theme-dark`, and follows `prefers-color-scheme` when the `<html>` element has no theme attribute.
- **Links.** Logo links open in a new tab with `rel="nofollow noopener"` unless the logo says otherwise. A `Url` that is not a path on the site or an
  absolute http, https or mailto address, such as a `javascript:` or `data:` URL, is not written: the logo shows
  without a link.
- Images load lazily and cannot be dragged as files, so dragging moves the row.

## Styling

| Class or property | Purpose |
| --- | --- |
| `osirion-infinite-carousel` | The `<section>` |
| `osirion-carousel-title` | The heading |
| `osirion-carousel-list` | A list of logos |
| `osirion-logo-item` | A logo slot |
| `osirion-logo-grayscale`, `osirion-logo-dual` | Added to slots with grayscale or light and dark images |
| `osirion-carousel-content-item` | The slot holding `ChildContent` |
| `osirion-carousel-pause` | The pause button the script adds |
| `--osirion-carousel-duration`, `--osirion-logo-width`, `--osirion-logo-height`, `--osirion-logo-gap` | Set from the parameters of the same names |

The heading and the pause button use the theme's `--osirion-text-primary`, `--osirion-background` and font size
tokens, and the focus ring uses `--osirion-focus-ring-width` and `--osirion-focus-ring-color`. Below 768px, logo
slots shrink to 160px wide.

## Related

- [Testimonial carousel](/components/testimonial-carousel)
- [Carousels](/components/sections)
