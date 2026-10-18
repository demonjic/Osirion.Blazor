---
id: 'osirion-page-layout'
order: 1
layout: docs
title: OsirionPageLayout
permalink: /components/page-layout
description: A header, main and footer page shell with a sticky footer, a header that hides on scroll and a skip link to the main content.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-page-layout.jpg'
categories:
- Components
- Core Components
- Layout
tags:
- blazor
- page-layout
- sticky-footer
- skip-link
- accessibility
- static-ssr
is_featured: true
published: true
slug: page-layout
lang: en
redirect_from:
- /docs/components/core/layout/page-layout
custom_fields:
  group: layout
seo_properties:
  title: 'OsirionPageLayout: page shell for Blazor'
  description: 'A header, main and footer shell for Blazor layouts with a sticky footer, a header that hides on scroll and a skip link.'
  image: 'https://media.getosirion.com/og/components-page-layout.jpg'
  canonical: 'https://getosirion.com/components/page-layout'
  lang: en
  robots: index, follow
  og_title: 'OsirionPageLayout - Osirion.Blazor'
  og_description: 'Header, main and footer shell with a sticky footer, a hide-on-scroll header and a skip link.'
  og_image_url: 'https://media.getosirion.com/og/components-page-layout.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionPageLayout - Osirion.Blazor'
  twitter_description: 'Header, main and footer shell for Blazor layouts.'
  twitter_image_url: 'https://media.getosirion.com/og/components-page-layout.jpg'
  meta_tags: {}
---

# OsirionPageLayout

`OsirionPageLayout` renders the shell of a page: a `header`, a `main` and a `footer`, stacked so the footer sits at
the bottom of the viewport on short pages. Use it in your `MainLayout.razor`.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Layout/OsirionPageLayout.razor).

```demo
page-layout
```

The demo shows the three slots inside a 16 rem frame, with the skip link and the scroll behavior off because this
page has its own. On a real page the layout fills at least the height of the viewport (`MinHeightStrategy`
`"viewport"`, the default), and the header is sticky.

## Usage

```razor
@inherits LayoutComponentBase
@using Osirion.Blazor.Components

<OsirionPageLayout>
    <Header>
        <NavMenu />
    </Header>
    <Body>
        @Body
    </Body>
    <Footer>
        <OsirionFooter CompanyName="Example Ltd" />
    </Footer>
</OsirionPageLayout>
```

The layout's small collocated script loads through BlazorJSComponents, which `AddOsirion` registers. If you register
Osirion.Blazor.Core without `AddOsirion`, add the loader in `Program.cs`:

```csharp
builder.Services.AddJSComponents();
```

The Bootstrap example site in the repository uses the layout this way, with an
[announcement bar](/components/announcement-bar) above the menu in the `Header` slot:

```razor
<OsirionPageLayout StickyFooter EnableHeaderScrollBehavior>
    <Header>
        <OsirionAnnouncementBar Id="release-4-0">
            New in 4.0: tabs, FAQ and feeds. <a href="/blog">Read more</a>
        </OsirionAnnouncementBar>
        <NavMenu />
    </Header>
    <Body>
        @Body
    </Body>
    <Footer>
        <OsirionFooter CompanyName="Example Ltd" />
    </Footer>
</OsirionPageLayout>
```

## Parameters

<!-- osirion:parameters OsirionPageLayout -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Header` | `RenderFragment?` | `null` | The content of the `header` element. Without it no `header` is rendered. |
| `Body` | `RenderFragment?` | `null` | The content of the `main` element, which carries the id `MainContentId` and `tabindex="-1"` so the skip link can move focus to it. |
| `Footer` | `RenderFragment?` | `null` | The content of the `footer` element. Without it no `footer` is rendered. An `OsirionFooter` placed here renders a `div`, so the page keeps one footer landmark. |
| `StickyFooter` | `bool` | `true` | Whether the layout adds the class `osirion-sticky-footer-layout`, which pushes the footer to the bottom of the viewport when the content is short. The base layout is already a flex column whose `main` grows, so the footer also sits at the bottom with this set to `false`. |
| `EnableHeaderScrollBehavior` | `bool` | `true` | Whether the header is sticky (class `osirion-page-header-scroll`, `z-index: 1000`) and slides out of view while the reader scrolls down, returning on scrolling up. The sliding is done by the layout script, which works in every render mode, including static SSR. It does nothing when the visitor prefers reduced motion, and the header stays while it is hovered or holds focus. |
| `MinHeightStrategy` | `string` | `"viewport"` | The minimum height of the layout: `"viewport"` sets `min-height: 100vh` and `"content"` sets `min-height: 100%`. The value becomes the class `osirion-min-height-{value}`, so any other value adds a class the stylesheet does not define. |
| `ShowSkipLink` | `bool` | `true` | Whether a skip link is rendered as the first focusable element, so keyboard and screen reader users can jump past the header and menu to the main content. The link is off screen until it receives focus. |
| `SkipLinkText` | `string` | `"Skip to main content"` | The text of the skip link. Set it for other languages. |
| `MainContentId` | `string` | `"osirion-main-content"` | The id of the `main` element, which the skip link targets. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` and unmatched attributes go on the outer `div`, and so does the `data-theme` (`data-bs-theme` under
Bootstrap) attribute that `Theme` writes. `Style` is not applied.

## Behavior

- **Rendering.** The header, main and footer render on the server. The sticky footer is pure CSS: the layout is a
  flex column and `main` grows to fill it, so the footer sits at the bottom on short pages. That is also why
  `StickyFooter="false"` does not unstick the footer under the default `MinHeightStrategy`.
- **Header on scroll.** With `EnableHeaderScrollBehavior` the header is `position: sticky`; the script hides it
  while you scroll down past the first 100 pixels and shows it again when you scroll up or return to the top. The
  header stays in place while the pointer is over it or focus is inside it, so an open menu is not carried away.
  When the visitor prefers reduced motion the script leaves the header in place. Without script the header simply
  stays sticky. The script works under static SSR; it does not need an interactive render mode.
- **Skip link (from 4.0).** The link stays off screen until it receives keyboard focus, then appears at the top
  left. Its address is the current page plus `#` and `MainContentId`, because a bare `#id` would resolve against
  `<base href="/">` and lead to the home page. The `main` element has `tabindex="-1"`, and the script moves focus to
  it on click, which Blazor's enhanced navigation would otherwise skip.
- **One footer landmark (from 4.0).** Inside the `Footer` slot, [OsirionFooter](/components/footer) renders a `div`
  instead of its own `footer`, so the page keeps a single contentinfo landmark. In 3.x it rendered a nested
  `footer`.
- **Printing.** The layout drops its minimum height and its flex layout, so the page prints as plain blocks.

## Styling

| Class | Element |
| --- | --- |
| `osirion-page-layout` | Outer `div`, plus `osirion-sticky-footer-layout` and `osirion-min-height-viewport` or `osirion-min-height-content`. |
| `osirion-page-header` | The `header`; `osirion-page-header-scroll` when the scroll behavior is on. |
| `osirion-page-main` | The `main` element. |
| `osirion-page-footer` | The `footer`. |
| `osirion-skip-link` | The skip link. |

The scrolling header has `z-index: 1000`. The skip link uses the page's own text and background colors, inverted,
and the theme's focus ring (`--osirion-focus-ring-color`, `--osirion-focus-ring-width`, `--osirion-focus-ring-offset`).

## Related

- [OsirionFooter](/components/footer)
- [OsirionAnnouncementBar](/components/announcement-bar)
- [OsirionStickySidebar](/components/sticky-sidebar)
- [Layout overview](/components/layout)
