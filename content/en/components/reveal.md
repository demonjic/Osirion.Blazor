---
id: 'osirion-reveal'
order: 9
layout: docs
title: OsirionReveal
permalink: /components/reveal
description: OsirionReveal fades or slides content into view when it first scrolls into the viewport, as a small script enhancement.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-reveal.jpg'
categories:
- Components
- Core Components
- Content
tags:
- reveal
- animation
- scroll
- osirion-blazor-core
is_featured: false
published: true
slug: reveal
lang: en
custom_fields:
  group: content
seo_properties:
  title: 'OsirionReveal: reveal on scroll for Blazor'
  description: 'OsirionReveal fades or slides content into view when it first scrolls into the viewport. Parameters, behavior and styling.'
  image: 'https://media.getosirion.com/og/components-reveal.jpg'
  canonical: 'https://getosirion.com/components/reveal'
  lang: en
  robots: index, follow
  og_title: 'OsirionReveal - Osirion.Blazor'
  og_description: 'Fade or slide content into view when it first scrolls into the viewport.'
  og_image_url: 'https://media.getosirion.com/og/components-reveal.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionReveal - Osirion.Blazor'
  twitter_description: 'Fade or slide content into view when it first scrolls into the viewport.'
  twitter_image_url: 'https://media.getosirion.com/og/components-reveal.jpg'
  meta_tags: {}
---

# OsirionReveal

`OsirionReveal` wraps content and fades or slides it into view the first time it scrolls into the viewport. Use it
for sections of a landing page, not for content a reader must see at once. The animation is an enhancement made by a
small script that runs in the browser.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Rendering/OsirionReveal.razor).

```demo
reveal
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionReveal>
    <section>
        <h2>Pricing</h2>
        <p>Content that slides up when it scrolls into view.</p>
    </section>
</OsirionReveal>

<OsirionReveal Animation="OsirionReveal.RevealAnimation.Fade">
    <p>Content that only fades in.</p>
</OsirionReveal>
```

The enum is nested in the component, so outside it you write `OsirionReveal.RevealAnimation.Fade`. The component
loads its script through BlazorJSComponents, which `AddOsirion` registers; without `AddOsirion`, call
`builder.Services.AddJSComponents();`.

## Parameters

<!-- osirion:parameters OsirionReveal -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The content to reveal. Required. |
| `Animate` | `bool` | `true` | Whether the content starts hidden and is revealed when it scrolls into view. When `false`, the script does nothing and the content is shown from the start. With reduced motion requested the script does not run either. |
| `Animation` | `RevealAnimation` | `RevealAnimation.Up` | The direction the content moves in from: `Up`, `Down`, `Left`, `Right` or `Fade` (no movement). |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the root `<div>`. `Class` and `Style` are accepted but the component does not write them,
so put your own classes on the content inside. `Theme` is written to the root element.

## Behavior

- The root is `<div class="osirion-reveal osirion-reveal-{animation}">` with `data-osirion-reveal` set to the
  `Animate` value. The component adds no ARIA roles.
- A script observes the root with an `IntersectionObserver`. When at least 12% of it is visible, the script adds the
  class `osirion-reveal-visible` and stops observing, so the reveal happens once.
- Before it starts observing, the script adds the class `osirion-reveal-armed`. Only then does the stylesheet start the
  element at `opacity: 0`, moved 1.25rem away in the direction of `Animation` (`Fade` has no movement), with a 0.55
  second transition.
- Without the script (blocked, failing to load, or before it runs) the element is server-rendered visible and in
  place: nothing hides it until the script has armed it.
- With `Animate="false"` the script does nothing, so the content is visible and in place.
- Under `prefers-reduced-motion: reduce` the script does not arm or observe, so the content is visible and in place for
  every direction.

## Styling

| Class | Purpose |
| --- | --- |
| `osirion-reveal` | The root element |
| `osirion-reveal-up`, `-down`, `-left`, `-right`, `-fade` | The direction, from `Animation` |
| `osirion-reveal-armed` | Added by the script when it takes over; the hidden starting state applies only with it |
| `osirion-reveal-visible` | Added by the script once the element has been revealed |

The component's rules are scoped to it, so to restyle the transition, target the content you place inside, or
override with a selector that is more specific than a class plus the component's scope attribute.

## Related

- [Content components](/components/content)
- [Metric card](/components/metric-card), which counts its figures up as it scrolls into view
- [Reading progress](/components/reading-progress)
