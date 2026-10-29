---
id: 'osirion-author-card'
order: 4
layout: docs
title: OsirionAuthorCard
permalink: /components/author-card
description: OsirionAuthorCard introduces an author with avatar or initials, name, role, bio and profile links. Renders without script.
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-author-card.jpg'
categories:
- Components
- Core Components
- Cards
tags:
- author-card
- authors
- cards
- osirion-blazor-core
is_featured: false
published: true
slug: author-card
lang: en
redirect_from:
- /docs/components/core/cards/author-card
custom_fields:
  group: cards
seo_properties:
  title: 'OsirionAuthorCard: author cards for Blazor'
  description: 'OsirionAuthorCard introduces an author with avatar or initials, name, role, bio and profile links. Renders without script.'
  image: 'https://media.getosirion.com/og/components-author-card.jpg'
  canonical: 'https://getosirion.com/components/author-card'
  lang: en
  robots: index, follow
  og_title: 'OsirionAuthorCard - Osirion.Blazor'
  og_description: 'An author card with avatar or initials, name, role, bio and rel="me" links.'
  og_image_url: 'https://media.getosirion.com/og/components-author-card.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionAuthorCard - Osirion.Blazor'
  twitter_description: 'An author card with avatar or initials, name, role, bio and profile links.'
  twitter_image_url: 'https://media.getosirion.com/og/components-author-card.jpg'
  meta_tags: {}
---

# OsirionAuthorCard

`OsirionAuthorCard` introduces a person: an avatar (or their initials), the name, a role, a short bio and links to
their profiles. Put it under an article or at the top of an author's page. It renders without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Cards/OsirionAuthorCard.razor). Available from 4.0.

```demo
author-card
```

## Usage

```razor
<OsirionAuthorCard Name="Jane Developer" />
```

With every detail, and links in an `AuthorLink` list:

```razor
@using Osirion.Blazor.Core.Models

<OsirionAuthorCard Name="Jane Developer"
                   Role="Principal engineer"
                   Bio="Writes about Blazor and the web platform."
                   AvatarUrl="/img/authors/jane.jpg"
                   ProfileUrl="/authors/jane-developer"
                   Links="links" />

@code {
    private readonly AuthorLink[] links =
    [
        new("GitHub", "https://github.com/jane"),
        new("Email", "mailto:jane@example.com")
    ];
}
```

### An author's page

With Osirion.Blazor.Cms.Web, combine the card with `ContentList`, which filters by the `author` front matter field
(the whole name, ignoring case):

```razor
@page "/authors/jane-developer"
@using Osirion.Blazor.Cms.Web.Components

<OsirionAuthorCard Name="Jane Developer" Role="Principal engineer" />
<ContentList Author="Jane Developer" Directory="blog" />
```

To link bylines on article pages to that page, set `AuthorUrlFormatter` on `ContentView`; the author's name then links
there with `rel="author"`.

## Parameters

<!-- osirion:parameters OsirionAuthorCard -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Name` | `string` | `""` | The author's name; the initials come from its first two words. Required. |
| `Role` | `string?` | `null` | The author's role or title, for example `Principal engineer`. |
| `Bio` | `string?` | `null` | A short biography, shown as plain text. |
| `AvatarUrl` | `string?` | `null` | The avatar image. Without one, the author's initials are shown. The image is decorative (the name is next to it), so it has an empty `alt`. |
| `ProfileUrl` | `string?` | `null` | The author's page, for example the listing of their articles. The name links to it with `rel="author"`. |
| `Links` | `IReadOnlyList<AuthorLink>?` | `null` | Links such as the author's site or social profiles, rendered with `rel="me"`. A link with an empty label, or with an address that is not a site path or an http, https, mailto or tel address, is left out. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`AuthorLink` is a record in `Osirion.Blazor.Core.Models`: `AuthorLink(string Label, string Url)`. Unmatched
attributes go on the card's root element.

## Behavior

- URLs are checked before they are written. `AvatarUrl`, `ProfileUrl` and each link's `Url` are rendered only when
  they are a path on the site (starting with a single `/`) or an absolute `http`, `https` or `mailto` URL. Other
  schemes such as `javascript:` and `data:`, protocol-relative URLs (`//host`) and relative paths without a leading
  `/` are left out, so author details taken from content cannot inject script.
- The avatar is decorative (the name is next to it): it has an empty `alt`, a 64 by 64 size, lazy loading and
  async decoding. The initials are hidden from assistive technology for the same reason.
- `rel="me"` lets services that verify profile links, such as Mastodon, match the author's site to the profile.
- All text is HTML-encoded.

## Styling

The card uses the classes `osirion-author-card`, `osirion-author-card-avatar`, `osirion-author-card-initials`,
`osirion-author-card-body`, `osirion-author-card-name`, `osirion-author-card-role`, `osirion-author-card-bio` and
`osirion-author-card-links`. It reads the theme tokens `--osirion-border-color`, `--osirion-background-primary`,
`--osirion-text-primary`, `--osirion-text-secondary`, and `--osirion-action-primary` with
`--osirion-action-primary-text` for the initials. On viewports 30rem wide or narrower the avatar stacks above the text.

## Related

- [Cards overview](/components/cards)
- [Article metadata](/components/article-metadata)
- [CMS web components](/docs/packages/cms-web)
