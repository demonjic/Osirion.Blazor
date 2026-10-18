---
title: "Hero Sections in Blazor with HeroSection"
author: "Dejan Demonjić"
date: "2025-01-22"
last_modified: 2026-10-18
description: "Build landing page and article headers with the HeroSection component: three variants, side or background images, patterns, buttons and metadata."
tags: [Components, Hero Section, Landing Pages, SSR, Blazor]
categories: [Components, Design]
slug: "modern-hero-sections-blazor"
is_featured: true
featured_image: 'https://media.getosirion.com/og/blog-modern-hero-sections-blazor.jpg'
seo_properties:
  title: "Hero Sections in Blazor with HeroSection"
  description: "Use HeroSection for landing pages and article headers: variants, side and background images, patterns, buttons and article metadata."
  image: 'https://media.getosirion.com/og/blog-modern-hero-sections-blazor.jpg'
  og_image_url: 'https://media.getosirion.com/og/blog-modern-hero-sections-blazor.jpg'
  type: "Article"
---

# Hero Sections in Blazor with HeroSection

The hero is the first block on a landing page or an article: a heading, a line or two of text, one or two buttons and
often an image. `HeroSection` from `Osirion.Blazor.Core` renders that block as a `<section>` with no script, so it is
complete in the first HTML response under static SSR. This article goes through the choices it gives you and when to
use each.

## The smallest hero

```razor
@using Osirion.Blazor.Components

<HeroSection Title="Build content sites with Blazor"
             Subtitle="SSR-first components for blogs, docs and landing pages."
             PrimaryButtonText="Get started"
             PrimaryButtonUrl="/docs" />
```

`Title` is rendered as the page's `<h1>`, so do not add another `<h1>` to the same page. `Subtitle` and `Summary`
render as paragraphs below it. A button appears only when it has both its text and its URL; `ShowPrimaryButton` and
`ShowSecondaryButton` hide one without clearing its values. The buttons are links styled with your CSS framework's
button classes through the Osirion theme adapter: the primary one filled, the secondary one outlined.

## Pick a variant

`Variant` sets the scale of the block:

- `HeroVariant.Hero` (default): the general-purpose size.
- `HeroVariant.Jumbotron`: more padding and a larger title, for the top of a home or product page.
- `HeroVariant.Minimal`: less padding and a smaller title, for section pages and article headers.

The variant also sets the `width` and `height` attributes of a side image (600 by 315, 600 by 600 and 315 by 315), so
the browser reserves space for it before it loads. `MinHeight` (default `60vh`) sets the minimum height of the section
for every variant.

## Side image or background image

With `ImageUrl`, the image is shown next to the text by default:

```razor
<HeroSection Variant="HeroVariant.Jumbotron"
             Title="Osirion.Blazor"
             Summary="Components for content sites that render on the server first."
             ImageUrl="/assets/images/blazor-cover.png"
             ImageAlt="Blazor logo on a dark background"
             PrimaryButtonText="Read the docs"
             PrimaryButtonUrl="/docs"
             SecondaryButtonText="View on GitHub"
             SecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor" />
```

![HeroSection with a side image, from its live demo on the component page](https://media.getosirion.com/shots/blog-modern-hero-sections-blazor/hero-variants-dark.webp){width=1600 height=900}

The side image is an `<img>` with `fetchpriority="high"`, because the hero image is usually the largest thing on screen
when the page loads. Its alt text falls back to the title when `ImageAlt` is not set; set it whenever the image carries
meaning of its own.

Set `UseBackgroundImage` to place the image behind the text instead. Text over a photo is hard to read, so add
`HasOverlay`, which darkens the image and switches the text to white:

```razor
<HeroSection Title="Release notes"
             Subtitle="What changed in each version."
             ImageUrl="/assets/images/blazor-cover.png"
             UseBackgroundImage
             HasOverlay
             Alignment="Alignment.Center" />
```

A background image is decorative to assistive technology, so do not put information in it that the text does not
repeat. The URL is CSS-escaped before it is written into the `style` attribute, so a value from front matter cannot add
style declarations of its own.

## Patterns and colors

Without a background image, `BackgroundPattern` draws a decorative pattern behind the content: `Dots`, `Grid`,
`Diagonal`, `Honeycomb`, `GradientMesh`, `AnimatedGrid`, `TechWave`, `Circuit` or `DotsFade` from the
`BackgroundPatternType` enum. `BackgroundColor` and `TextColor` take any CSS color and are written as inline styles; by
default the section uses the theme's background and text colors, so it follows dark mode and your framework adapter.

`Alignment` aligns the text left (default), center or right. With a side image, centered text stays left-aligned so it
lines up with the image.

`HasDivider` (on by default) draws a thin shadow line in the primary color at the top edge; set it to `false` for a
flat edge.

## An article header

For a blog post, turn on the metadata line and use the minimal variant:

```razor
<HeroSection Variant="HeroVariant.Minimal"
             Title="@post.Title"
             Summary="@post.Description"
             ShowMetadata
             Author="@post.Author"
             PublishDate="@post.PublishDate"
             ReadTime="6 min read" />
```

`post` stands for your own model; `PublishDate` is a `DateTime?`. If your posts come from the Osirion CMS, you do not
need to write this: `ArticlePage`, `HomePage` and `LandingPage` render a `HeroSection` from the item's front matter and
expose its options as `Hero*` parameters, such as `HeroVariant`, `HeroAlignment` and `HeroBackgroundPattern`.

## Your own markup

When a plain string is not enough, `TitleContent`, `SubtitleContent` and `SummaryContent` replace the matching element
with your own markup. If you use `TitleContent`, render the `<h1>` yourself. `ChildContent` goes below the buttons, for
example a badge row or a short install command:

```razor
<HeroSection Title="Install in one line">
    <pre><code>dotnet add package Osirion.Blazor</code></pre>
</HeroSection>
```

## Styling

The section carries `osirion-hero-section` plus one class per choice: `osirion-hero-variant-{hero|jumbotron|minimal}`,
`osirion-hero-align-{left|center|right}`, and `osirion-hero-with-side-image` or `osirion-hero-with-background`. Title
sizes scale with the viewport through custom properties set on the section: `--hero-title-min`, `--hero-title-pref` and
`--hero-title-max`, with the same trio for `--hero-subtitle-*` and `--hero-summary-*`. The component's stylesheet sets
them per variant, so an override needs a selector at least as specific as those rules.

## Related

- [HeroSection reference](/components/hero-section)
- [Section components](/components/sections)
- [Background pattern](/components/background-pattern)
