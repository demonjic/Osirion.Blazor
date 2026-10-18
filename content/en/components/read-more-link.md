---
id: 'osirion-read-more-link'
order: 2
layout: docs
title: OsirionReadMoreLink
permalink: /components/read-more-link
description: OsirionReadMoreLink renders a read more or call-to-action link with an optional icon, six variants, three sizes and a stretched mode for cards.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-read-more-link.jpg'
categories:
- Components
- Core Components
- Navigation
tags:
- blazor
- links
- call-to-action
- cards
- accessibility
is_featured: true
published: true
slug: read-more-link
lang: en
redirect_from:
- /docs/components/core/navigation/read-more-link
custom_fields:
  group: navigation
seo_properties:
  title: 'OsirionReadMoreLink: read more links for Blazor'
  description: 'A read more or call-to-action link for Blazor with an optional icon, six variants, three sizes and a stretched mode for cards.'
  image: 'https://media.getosirion.com/og/components-read-more-link.jpg'
  canonical: 'https://getosirion.com/components/read-more-link'
  lang: en
  robots: index, follow
  og_title: 'OsirionReadMoreLink - Osirion.Blazor'
  og_description: 'A read more or call-to-action link with an optional icon, variants, sizes and a stretched mode for cards.'
  og_image_url: 'https://media.getosirion.com/og/components-read-more-link.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionReadMoreLink - Osirion.Blazor'
  twitter_description: 'A read more or call-to-action link with an optional icon, variants, sizes and a stretched mode for cards.'
  twitter_image_url: 'https://media.getosirion.com/og/components-read-more-link.jpg'
  meta_tags: {}
---

# OsirionReadMoreLink

`OsirionReadMoreLink` renders an `<a>` with text and an optional icon, for "read more" links in cards and lists and
for small calls to action. The feature and testimonial cards use it for their own links.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Navigation/OsirionReadMoreLink.razor).

```demo
read-more-link
```

## Usage

The smallest form needs only `Href`. The text defaults to "Read more" with a chevron on the right:

```razor
<OsirionReadMoreLink Href="/blog/static-ssr" />
```

In a list of posts every link would read "Read more", so give each one an accessible name that starts with the
visible text. With `Stretched`, the whole card becomes clickable; the card needs `position: relative` because the
link covers its nearest positioned ancestor:

```razor
<article class="post-card" style="position: relative;">
    <h3>Static SSR with Blazor</h3>
    <p>What renders on the server, and what needs interactivity.</p>
    <OsirionReadMoreLink Href="/blog/static-ssr"
                         AriaLabel="Read more: Static SSR with Blazor"
                         Stretched="true" />
</article>
```

Other variants change the icon or the look:

```razor
<OsirionReadMoreLink Href="https://github.com/demonjic/Osirion.Blazor"
                     Text="Source on GitHub"
                     Variant="ReadMoreVariant.External"
                     Target="_blank"
                     rel="noopener" />

<OsirionReadMoreLink Href="/docs"
                     Text="Read the docs"
                     Variant="ReadMoreVariant.Button"
                     Size="LinkSize.Large" />
```

## Parameters

<!-- osirion:parameters OsirionReadMoreLink -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Href` | `string` | `"#"` | The link address. It is written to `href` as given, so pass only addresses you trust. Required. |
| `Text` | `string` | `"Read more"` | The visible text. |
| `AriaLabel` | `string?` | `null` | The accessible name, written as `aria-label`. When empty, `Text` is used. Keep the visible text at its start, as in `Read more: Static SSR with Blazor`, so speech input users can say what they see. |
| `ShowText` | `bool` | `true` | Whether `Text` is rendered. Turn it off for an icon-only link and set `AriaLabel`. |
| `ShowIcon` | `bool` | `true` | Whether the icon is rendered, the built-in one or `IconContent`. |
| `IconPosition` | `IconPosition` | `IconPosition.Right` | Which side of the text the icon sits on: `Left` or `Right`. |
| `IconContent` | `RenderFragment?` | `null` | Your own icon, rendered in place of the built-in one inside an `aria-hidden` span. |
| `Variant` | `ReadMoreVariant` | `ReadMoreVariant.Default` | The look and the built-in icon: `Default` (chevron), `Arrow`, `External`, `Download`, `Plain` (body text color, chevron) or `Button` (filled, chevron). |
| `Size` | `LinkSize` | `LinkSize.Normal` | The size: `Small`, `Normal` or `Large`. |
| `Stretched` | `bool` | `false` | Whether the link covers its nearest positioned ancestor, such as a card, so the whole card is clickable. The ancestor needs `position: relative`. |
| `Block` | `bool` | `false` | Whether the link is displayed as a full-width block. |
| `Target` | `string?` | `null` | The `target` attribute, for example `_blank`. No `rel` is added; pass it as an attribute when you need one. |
| `Animated` | `bool` | `true` | Whether the icon moves slightly on hover. Reduced motion turns it off. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content rendered inside the link, after the text and icon. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes, such as `rel` or `data-*`, are written to the `<a>` element. `Theme` is accepted by every Osirion
component; the link does not use it.

### Variants

| Variant | Icon | Look |
| --- | --- | --- |
| `Default` | Chevron | Link color |
| `Arrow` | Arrow | Link color, semibold |
| `External` | Box with an outgoing arrow | Link color |
| `Download` | Tray with a downward arrow | Link color |
| `Plain` | Chevron | Body text color, normal weight; combine with `ShowIcon="false"` for text only |
| `Button` | Chevron | Filled with the primary color, padded like a button |

## Behavior

- The component renders a plain link and needs no script; it works the same under static SSR, Interactive Server
  and WebAssembly.
- The built-in icons are inline SVG inside a `<span aria-hidden="true">`, so screen readers read only the accessible
  name.
- `aria-label` replaces the link's content as its accessible name. Keep the visible text at the start of it ("Read
  more: Static SSR with Blazor") so speech input users can say what they see.
- When `AriaLabel` is empty, the accessible name is the current `Text`, read on every render, so a `Text` that
  changes later changes the name too. `AriaLabel` is never overwritten.
- `Target` is written as given and no `rel` is added; pass `rel` as an attribute when you need one.
- `Href` is written as given. Pass only addresses you trust, such as a path in your own site or a URL from your own
  content.
- With `prefers-reduced-motion: reduce`, the hover animation and transitions are off. In print, the icon is hidden.

## Styling

The link always has `osirion-read-more` plus a class for its variant (`osirion-read-more-default`,
`osirion-read-more-arrow` and so on) and its size (`osirion-read-more-small`, `-normal`, `-large`). Depending on the
parameters it also gets `osirion-read-more-stretched`, `osirion-read-more-block`, `osirion-read-more-animated` and
`osirion-read-more-icon-left` or `osirion-read-more-icon-right`. Inside, the text is `osirion-read-more-text` and the
icon `osirion-read-more-icon`.

The color is `--osirion-link-color`, which falls back to `--osirion-action-primary`, and the hover color is
`--osirion-read-more-hover-color`, which falls back to `--osirion-color-primary-700`. When the browser prefers a dark
color scheme, the hover color is `--osirion-link-hover-color` instead, which falls back to
`--osirion-action-primary-hover`. The `Button` variant takes its fill from `--osirion-action-primary` and its text
color from `--osirion-action-primary-text`. Set the variables on the link (through `Style`) or a container to recolor
it:

```razor
<OsirionReadMoreLink Href="/pricing"
                     Text="See pricing"
                     Style="--osirion-link-color: #7c3aed; --osirion-read-more-hover-color: #5b21b6;" />
```

The link also takes the configured CSS framework's link class:

| Framework | Link class | Stretched class |
| --- | --- | --- |
| Bootstrap | `icon-link icon-link-hover` (not when stretched) | `stretched-link` |
| Fluent UI | `osirion-fluent-link` | `osirion-fluent-stretched` |
| MudBlazor | `mud-link` | `mud-stretched` |
| Radzen | `rz-link` | `rz-stretched` |
| None | `osirion-link` | `osirion-stretched` |

## Related

- [Breadcrumbs](/components/breadcrumbs)
- [Feature card](/components/feature-card)
- [Testimonial card](/components/testimonial-card)
- [Navigation components](/components/navigation)
