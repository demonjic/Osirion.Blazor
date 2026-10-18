---
id: 'osirion-responsive-image'
order: 5
layout: docs
title: OsirionResponsiveImage
permalink: /components/responsive-image
description: "OsirionResponsiveImage renders an img with intrinsic size, srcset, sizes and lazy loading, so images neither shift the layout nor load too large."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-responsive-image.jpg'
categories:
- Components
- Core Components
- Content
tags:
- images
- srcset
- lazy-loading
- core-web-vitals
- static-ssr
slug: responsive-image
lang: en
published: true
redirect_from:
- /docs/components/core/content/responsive-image
custom_fields:
  group: content
seo_properties:
  title: 'OsirionResponsiveImage: responsive images for Blazor'
  description: 'OsirionResponsiveImage renders an img with intrinsic size, srcset, sizes and lazy loading, so images neither shift the layout nor load too large.'
  image: 'https://media.getosirion.com/og/components-responsive-image.jpg'
  canonical: 'https://getosirion.com/components/responsive-image'
  lang: en
  robots: index, follow
  og_title: 'OsirionResponsiveImage - Osirion.Blazor'
  og_description: 'An img with intrinsic size, srcset, sizes and lazy loading. Renders without script.'
  og_image_url: 'https://media.getosirion.com/og/components-responsive-image.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionResponsiveImage - Osirion.Blazor'
  twitter_description: 'An img with intrinsic size, srcset, sizes and lazy loading. Renders without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-responsive-image.jpg'
  meta_tags: {}
---

# OsirionResponsiveImage

`OsirionResponsiveImage` renders an `<img>` that keeps its space while it loads, offers the browser files sized for
the screen, and loads lazily unless it is the page's main image. It is plain HTML, so it needs no script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionResponsiveImage.razor). Available from 4.0.

```demo
responsive-image
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionResponsiveImage Src="/images/team.jpg" Alt="The team at the 2026 meetup" Width="1200" Height="800" />
```

This renders a lazily loaded, asynchronously decoded image with `width` and `height`, so the browser reserves its
space before the file arrives.

With an image service or CDN that resizes on request, give the widths to offer and how to build each file's URL:

```razor
<OsirionResponsiveImage Src="@PhotoAt(1600)"
                        Alt="A laptop showing charts on a desk"
                        Width="1600"
                        Height="1066"
                        Widths="imageWidths"
                        SrcFormatter="(_, width) => PhotoAt(width)"
                        Sizes="(min-width: 992px) 720px, 100vw"
                        Priority="true"
                        Caption="Monthly figures for the second quarter." />

@code {
    private readonly int[] imageWidths = [640, 960, 1280, 1600];

    private static string PhotoAt(int width) =>
        $"https://images.example.com/laptop.jpg?w={width}&h={width * 2 / 3}&fit=crop";
}
```

The `srcset` becomes four candidates from 640 to 1600 pixels wide. `Priority` marks this as the main image of the
page, so it loads at once instead of lazily. `images.example.com` stands for your own image service.

Without an image service, write the candidates yourself with `SrcSet`, one file per width:

```razor
<OsirionResponsiveImage Src="/img/a-1280.jpg"
                        Alt="A chart of monthly sign-ups"
                        Width="1280"
                        Height="720"
                        SrcSet="/img/a-640.jpg 640w, /img/a-1280.jpg 1280w"
                        Sizes="(min-width: 768px) 50vw, 100vw" />
```

## Parameters

<!-- osirion:parameters OsirionResponsiveImage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Src` | `string` | `""` | The image URL, written to `src`. Browsers that ignore `srcset` load it, and it is the first argument of `SrcFormatter`. Required. |
| `Alt` | `string` | `""` | The text alternative. Describe what the image shows; use an empty string only for an image that is purely decorative, which screen readers then skip. Required. |
| `Width` | `int?` | `null` | The image's intrinsic width in pixels, written to `width`. With `Height` it reserves the space before the file arrives, so the content below does not jump (Cumulative Layout Shift). When set it must be greater than zero, or the component throws `ArgumentOutOfRangeException`. |
| `Height` | `int?` | `null` | The image's intrinsic height in pixels, written to `height`. See `Width`; the same rule applies. |
| `SrcSet` | `string?` | `null` | The `srcset` candidates as written, for example `/img/a-640.jpg 640w, /img/a-1280.jpg 1280w`. Takes precedence over `Widths`. |
| `Widths` | `IReadOnlyList<int>?` | `null` | The widths, in pixels, to offer in `srcset`. They are sorted, and duplicates and values of zero or less are dropped. Each candidate's URL comes from `SrcFormatter`; without one this parameter is ignored. |
| `SrcFormatter` | `Func<string, int, string>?` | `null` | How the URL of the file of a given width in pixels is built from `Src`, for an image service such as `(src, width) => $"{src}?w={width}"`. Used with `Widths`. |
| `Sizes` | `string?` | `null` | The `sizes` attribute: how wide the image is shown, for example `(min-width: 768px) 50vw, 100vw`. It is written only when a `srcset` exists. When it is not set, lazily loaded images use `auto, 100vw` (the browser measures the layout where it can) and `Priority` images use `100vw`. |
| `Priority` | `bool` | `false` | Whether this is the page's main image, typically the largest one in the first view. It then loads at once (`loading="eager"`) with `fetchpriority="high"` instead of lazily, which speeds up Largest Contentful Paint. Mark only one image per page. |
| `Caption` | `string?` | `null` | A caption. When it is not empty, the image is wrapped in `figure` with the caption in a `figcaption`; the CSS classes and extra attributes stay on the `img`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes and `Class` go on the `<img>`, not on the `<figure>`. `Style` is accepted but the component does
not write it.

## Behavior

- Every image gets `decoding="async"`. Without `Priority` it also gets `loading="lazy"`; with `Priority` it gets
  `loading="eager"` and `fetchpriority="high"`. Mark only one image per page as `Priority`, the one in the first view:
  lazy loading that image would delay Largest Contentful Paint.
- `Widths` are sorted, duplicates and values of zero or less are dropped. If nothing is left, no `srcset` is written.
- Every file in the `srcset` should have the aspect ratio of `Width` and `Height`. The stylesheet sets
  `height: auto` and `max-width: 100%`, so the image scales down inside its container and keeps that ratio.
- `Width` or `Height` set to zero or a negative number throws `ArgumentOutOfRangeException`.
- Without `Width` and `Height` the attributes are left out and the browser cannot reserve the space.
- URLs are written as attributes, so Razor encodes them. The component does not check where they point; pass URLs
  you control or have validated.

## Styling

| Class | Element |
| --- | --- |
| `osirion-responsive-image` | The `<img>`: `display: block`, `max-width: 100%`, `height: auto` |
| `osirion-responsive-image-figure` | The `<figure>` when there is a caption |
| `osirion-responsive-image-caption` | The `<figcaption>` |

The figure's bottom margin uses `--osirion-spacing-4`, and the caption uses `--osirion-spacing-2`,
`--osirion-font-size-sm` and `--osirion-text-secondary`, each with a fallback value.

## Related

- [Video embed](/components/video-embed)
- [Content components](/components/content)
