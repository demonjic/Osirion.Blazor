---
id: 'osirion-breadcrumbs'
order: 1
layout: docs
title: OsirionBreadcrumbs
permalink: /components/breadcrumbs
description: OsirionBreadcrumbs turns a URL path into an accessible breadcrumb trail, with a home link, custom segment text and the page title as the last crumb.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-breadcrumbs.jpg'
categories:
- Components
- Core Components
- Navigation
tags:
- blazor
- breadcrumbs
- navigation
- accessibility
- static-ssr
is_featured: true
published: true
slug: breadcrumbs
lang: en
redirect_from:
- /docs/components/core/navigation/breadcrumbs
custom_fields:
  group: navigation
seo_properties:
  title: 'OsirionBreadcrumbs: breadcrumb trails for Blazor'
  description: 'Build an accessible breadcrumb trail from a URL path in Blazor, with a home link, custom segment text and the page title as the last crumb.'
  image: 'https://media.getosirion.com/og/components-breadcrumbs.jpg'
  canonical: 'https://getosirion.com/components/breadcrumbs'
  lang: en
  robots: index, follow
  og_title: 'OsirionBreadcrumbs - Osirion.Blazor'
  og_description: 'An accessible breadcrumb trail built from a URL path, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-breadcrumbs.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionBreadcrumbs - Osirion.Blazor'
  twitter_description: 'An accessible breadcrumb trail built from a URL path, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-breadcrumbs.jpg'
  meta_tags: {}
---

# OsirionBreadcrumbs

`OsirionBreadcrumbs` renders a breadcrumb trail from a URL path: one link per segment, an optional home link first,
and the current page last. Use it at the top of articles and docs pages so readers can move up the hierarchy.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Navigation/OsirionBreadcrumbs.razor).

```demo
breadcrumbs
```

## Usage

Pass the path. Each segment becomes a crumb whose text is the slug turned into words:

```razor
<OsirionBreadcrumbs Path="/docs/components/navigation" />
```

This renders **Home / Docs / Components / Navigation**, with links to `/`, `/docs` and `/docs/components`, and the
last crumb as text.

Slugs lose punctuation and casing, so pass the page title for the last crumb with `CurrentPageText`:

```razor
<OsirionBreadcrumbs Path="/blog/pre-mvp-make-it-right"
                    CurrentPageText="Pre-MVP: make it right" />
```

The trail reads **Home / Blog / Pre-MVP: make it right** instead of ending in "Pre Mvp Make It Right". The CMS
`ArticlePage` does this for you: it passes the item's URL and title.

To build the path from the current address, take the path part of the URI so a query string does not end up in the
last segment:

```razor
@inject NavigationManager Navigation

<OsirionBreadcrumbs Path="@(new Uri(Navigation.Uri).AbsolutePath)" />
```

To change how every segment is written, pass a `SegmentFormatter`. It receives the raw segment and returns the text:

```razor
<OsirionBreadcrumbs Path="/docs/api/cms-web"
                    SegmentFormatter="@(segment => segment == "api" ? "API" : segment.Replace('-', ' '))" />
```

## Parameters

<!-- osirion:parameters OsirionBreadcrumbs -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Path` | `string` | `""` | The path to split into crumbs, for example `/docs/components/navigation`. Leading and trailing slashes are ignored, and each segment becomes one crumb. Pass the path only: a query string would end up in the last segment. |
| `ShowHome` | `bool` | `true` | Whether a home link is rendered before the path segments. `HomeText` and `HomeUrl` set its text and address. |
| `HomeText` | `string` | `"Home"` | The text of the home link. |
| `HomeUrl` | `string` | `"/"` | The address of the home link, written as given. |
| `LinkLastItem` | `bool` | `false` | Whether the last crumb, the current page, is a link instead of text. It carries `aria-current="page"` either way. |
| `UrlPrefix` | `string` | `"/"` | The text put before each segment's cumulative path to form its link, for example `/en/` for a localized site. With `Path="/a/b/c"` and the default prefix the crumbs link to `/a` and `/a/b`, and to `/a/b/c` when `LinkLastItem` is set. The home link does not use it. |
| `SegmentFormatter` | `Func<string, string>?` | `null` | A function that returns the text of a segment from its raw slug, exactly as it sits in `Path`, so still percent-encoded. Without it, a percent-encoded segment is decoded, hyphens become spaces and each word starts with a capital letter. It does not apply to the last crumb while `CurrentPageText` is set. |
| `CurrentPageText` | `string?` | `null` | The text of the last crumb, the current page. Pass the page title: a slug turned into words loses its punctuation and casing ("Pre Mvp" for "Pre-MVP"). When empty, the last segment is formatted like the others. Available since 4.0. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Each link is `UrlPrefix` followed by the path up to that segment: with `Path="/a/b/c"` and the default prefix, the
crumbs link to `/a` and `/a/b`, and to `/a/b/c` as well when `LinkLastItem` is set. The home link uses `HomeUrl`, not
the prefix. The markup carries only what these parameters set, and `Style`, which goes on the `nav`: additional
attributes and `Theme` are accepted and not written.

## Behavior

- The component renders plain HTML and needs no script, so it works under static SSR, Interactive Server and
  WebAssembly.
- The trail is a `<nav aria-label="breadcrumb">` containing an `<ol>`: a navigation landmark named "breadcrumb"
  holding an ordered list.
- The last crumb carries `aria-current="page"`, whether it is text or a link (`LinkLastItem`).
- Segment text and `CurrentPageText` are written as text and HTML-encoded.
- A percent-encoded segment is decoded for the crumb text, so `/docs/caf%C3%A9` shows "Café", and keeps its encoding in
  the link. An invalid escape, such as the `%` in `100%`, is shown as written. `SegmentFormatter` receives the segment
  as it sits in the path, still encoded.
- The component writes no structured data. On CMS pages, `SeoMetadataRenderer` adds the `BreadcrumbList` JSON-LD
  and names its last item with the content title.

## Styling

| Class | Element |
| --- | --- |
| `osirion-breadcrumbs` | The `<nav>` |
| `osirion-breadcrumbs-list` | The `<ol>` |
| `osirion-breadcrumbs-item` | Each `<li>`; every item but the last is followed by a `/` separator drawn in CSS |
| `osirion-breadcrumbs-current` | The last `<li>` |
| `osirion-breadcrumbs-link` | Each link |
| `osirion-breadcrumbs-text` | The last crumb when it is not a link |

The colors come from the theme tokens `--osirion-text-primary` (links), `--osirion-link-color` (link hover, falling
back to `--osirion-action-primary`), `--osirion-text-secondary` (current page) and `--osirion-text-tertiary`
(separator), so a framework adapter recolors the trail with the rest of the site. The `<nav>` has a bottom margin of
`--osirion-spacing-6`, and the separator margin uses `--osirion-spacing-2`. At 768px wide and narrower, the text drops
to `--osirion-font-size-sm` and the separator margin narrows.

## Related

- [Read more link](/components/read-more-link)
- [Navigation components](/components/navigation)
- [CMS Web navigation](/components/cms-navigation)
