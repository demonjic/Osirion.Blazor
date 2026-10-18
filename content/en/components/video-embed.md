---
id: 'osirion-video-embed'
order: 6
layout: docs
title: OsirionVideoEmbed
permalink: /components/video-embed
description: "OsirionVideoEmbed shows a YouTube or Vimeo thumbnail and loads the player only when pressed, privacy-enhanced by default; without script it links to the video."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-video-embed.jpg'
categories:
- Components
- Core Components
- Content
tags:
- video
- youtube
- vimeo
- privacy
- performance
slug: video-embed
lang: en
published: true
redirect_from:
- /docs/components/core/content/video-embed
custom_fields:
  group: content
seo_properties:
  title: 'OsirionVideoEmbed: click-to-play video for Blazor'
  description: 'OsirionVideoEmbed shows a YouTube or Vimeo thumbnail and loads the player only when pressed, privacy-enhanced by default; without script it links to the video.'
  image: 'https://media.getosirion.com/og/components-video-embed.jpg'
  canonical: 'https://getosirion.com/components/video-embed'
  lang: en
  robots: index, follow
  og_title: 'OsirionVideoEmbed - Osirion.Blazor'
  og_description: 'A YouTube or Vimeo thumbnail that loads the player when pressed.'
  og_image_url: 'https://media.getosirion.com/og/components-video-embed.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionVideoEmbed - Osirion.Blazor'
  twitter_description: 'A YouTube or Vimeo thumbnail that loads the player when pressed.'
  twitter_image_url: 'https://media.getosirion.com/og/components-video-embed.jpg'
  meta_tags: {}
---

# OsirionVideoEmbed

`OsirionVideoEmbed` shows a YouTube or Vimeo video as a thumbnail with a play button and loads the player only when
the visitor presses it. Until then, nothing but the thumbnail is requested from the video host. Without script, the
thumbnail is a link that opens the video on its own site.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionVideoEmbed.razor). Available from 4.0.

```demo
video-embed
```

## Usage

The component's script loads through BlazorJSComponents, which `AddOsirion` registers. If you register
Osirion.Blazor.Core without `AddOsirion`, add the loader in `Program.cs`:

```csharp
builder.Services.AddJSComponents();
```

Then:

```razor
@using Osirion.Blazor.Components

<OsirionVideoEmbed VideoId="V0Af7y7aMBE" Title="Build better web apps with Blazor in .NET 10" />
```

A Vimeo video that starts at 30 seconds, with a thumbnail served from your own site:

```razor
<OsirionVideoEmbed Provider="VideoProvider.Vimeo"
                   VideoId="76979871"
                   Title="Product tour"
                   ThumbnailUrl="/img/tour-poster.jpg"
                   StartSeconds="30" />
```

## Parameters

<!-- osirion:parameters OsirionVideoEmbed -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Provider` | `VideoProvider` | `VideoProvider.YouTube` | The video host: `VideoProvider.YouTube` or `VideoProvider.Vimeo`. |
| `VideoId` | `string` | `""` | The video ID: the 11 characters after `v=` in a YouTube watch address (letters, digits, `-` or `_`), or the number in a Vimeo address (digits). Any other text throws `ArgumentException`. Required. |
| `Title` | `string` | `""` | The video's title. It is shown over the top of the thumbnail and names the play link and the player for screen readers. Required. |
| `ThumbnailUrl` | `string?` | `null` | The thumbnail image. Defaults to YouTube's own thumbnail (from `i.ytimg.com`); Vimeo has no public thumbnail address, so without one a Vimeo embed shows a plain black frame. Host the image yourself to send no request to the video host before the click. |
| `StartSeconds` | `int?` | `null` | Where playback starts, in seconds, in the player address and in the link that works without script. Not set by default; a negative value throws `ArgumentOutOfRangeException`. |
| `PrivacyEnhanced` | `bool` | `true` | Whether the player is asked not to track the visitor: YouTube's `youtube-nocookie.com` domain, or Vimeo's `dnt=1`. |
| `AspectRatio` | `VideoAspectRatio` | `VideoAspectRatio.Widescreen` | The frame's shape: `Widescreen` (16:9), `Standard` (4:3), `Square` (1:1) or `Vertical` (9:16, at most 22.5rem wide). |
| `PlayLabelText` | `string?` | `null` | The accessible name of the play link. Defaults to "Play video: " followed by the `Title`; set it for other languages. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes and `Class` go on the container `<div>`. `Style` is accepted but the component does not write it.

## Behavior

- Static SSR renders the thumbnail inside a link to the video's page (`youtube.com/watch` or `vimeo.com`), plus the
  start time if set. That link works with script off.
- With script, a plain click replaces the thumbnail with an `<iframe>` player that starts playing and receives focus.
  A click with Ctrl, Cmd, Shift or Alt, or with a button other than the main one, still follows the link, so "open in
  new tab" keeps working.
- The player URL autoplays after the click. With `PrivacyEnhanced` on, YouTube uses `youtube-nocookie.com` and Vimeo
  gets `dnt=1`. The iframe has `referrerpolicy="strict-origin-when-cross-origin"`.
- The thumbnail is an `<img>` with an empty `alt`, because the link already has an accessible name: the play label
  followed by the title, or `PlayLabelText`. The title shown over the thumbnail is hidden from screen readers for the
  same reason.
- The default YouTube thumbnail comes from `i.ytimg.com`, a Google host. Set `ThumbnailUrl` to an image on your own
  site when no request may reach the video host before the visitor chooses to play.
- `VideoId` is validated: YouTube IDs must be 11 letters, digits, `-` or `_`, and Vimeo IDs digits only. Anything else
  throws `ArgumentException`, so no other text reaches the player URL. A negative `StartSeconds` throws
  `ArgumentOutOfRangeException`.
- The frame shape comes from CSS classes (`aspect-ratio`), not inline styles. With a Content-Security-Policy, allow
  `frame-src https://www.youtube-nocookie.com https://player.vimeo.com` (add `https://www.youtube.com` if you turn
  `PrivacyEnhanced` off) and `img-src https://i.ytimg.com` for the default YouTube thumbnails.

## Styling

| Class | Element |
| --- | --- |
| `osirion-video-embed` | The container, black background with rounded corners |
| `osirion-video-embed-widescreen`, `-standard`, `-square`, `-vertical` | The aspect ratio, from `AspectRatio` |
| `osirion-video-embed-link` | The link covering the thumbnail |
| `osirion-video-embed-thumbnail` | The thumbnail `<img>` (lazy loaded, `object-fit: cover`) |
| `osirion-video-embed-play` | The play button, drawn in CSS |
| `osirion-video-embed-title` | The title over the top of the thumbnail |
| `osirion-video-embed-player` | The `<iframe>` the script adds |

The corner radius uses `--osirion-radius-md` and the focus ring `--osirion-focus-ring-width` and
`--osirion-focus-ring-color`, each with a fallback value. The play button's color change on hover is not animated
under `prefers-reduced-motion: reduce`.

## Related

- [Responsive image](/components/responsive-image)
- [Content components](/components/content)
