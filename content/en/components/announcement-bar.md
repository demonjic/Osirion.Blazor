---
id: 'osirion-announcement-bar'
order: 3
layout: docs
title: OsirionAnnouncementBar
permalink: /components/announcement-bar
description: A site-wide announcement above the page that visitors can close without script; the dismissal is remembered in a cookie per announcement.
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-announcement-bar.jpg'
categories:
- Components
- Core Components
- Popups
tags:
- blazor
- announcement
- banner
- static-ssr
- antiforgery
- accessibility
is_featured: true
published: true
slug: announcement-bar
lang: en
redirect_from:
- /docs/components/core/popups/announcement-bar
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'OsirionAnnouncementBar: site announcement for Blazor'
  description: 'A site-wide Blazor announcement bar that visitors can close without script, remembered per announcement in a cookie.'
  image: 'https://media.getosirion.com/og/components-announcement-bar.jpg'
  canonical: 'https://getosirion.com/components/announcement-bar'
  lang: en
  robots: index, follow
  og_title: 'OsirionAnnouncementBar - Osirion.Blazor'
  og_description: 'A dismissible site-wide announcement that closes without script.'
  og_image_url: 'https://media.getosirion.com/og/components-announcement-bar.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionAnnouncementBar - Osirion.Blazor'
  twitter_description: 'A dismissible site-wide announcement that closes without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-announcement-bar.jpg'
  meta_tags: {}
---

# OsirionAnnouncementBar

`OsirionAnnouncementBar` shows a one-line announcement across the top of every page, such as a release or an event,
with a close button. Closing posts a small form; an endpoint in your app remembers the choice in a cookie and
redirects back, so it works under static SSR without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Popups/OsirionAnnouncementBar.razor). Available from 4.0.

## Working example

There is no live demo on this page, because closing the bar posts to an endpoint of the site that serves it and sets
a real cookie there. This site shows the component itself: the announcement bar above the menu is an
`OsirionAnnouncementBar` in the header of the page layout. Close it and it stays closed for 30 days, the default
`RememberDays`, until the announcement gets a new `Id`.

## Setup

Map the dismissal endpoint in `Program.cs`:

```csharp
using Osirion.Blazor.Core.Extensions;

var app = builder.Build();

app.UseAntiforgery();
app.MapOsirionAnnouncements(); // POST /api/announcement/dismiss
```

The endpoint requires an antiforgery token, so `app.UseAntiforgery()` must run before the endpoints, as in the
Blazor Web App template.

## Usage

Put the bar at the top of a statically rendered layout:

```razor
@using Osirion.Blazor.Components

<OsirionAnnouncementBar Id="release-4-0">
    Osirion.Blazor 4.0 is out. <a href="/blog">Read what's new</a>
</OsirionAnnouncementBar>
```

With [OsirionPageLayout](/components/page-layout), place it in the `Header` slot above your menu,
so it scrolls away with the header:

```razor
<OsirionPageLayout>
    <Header>
        <OsirionAnnouncementBar Id="docs-survey" RememberDays="7" RegionLabel="Site notice">
            Tell us what is missing from the docs. <a href="/survey">Take the two-minute survey</a>
        </OsirionAnnouncementBar>
        <NavMenu />
    </Header>
    <Body>
        @Body
    </Body>
</OsirionPageLayout>
```

To announce something new, change `Id`. Visitors who closed the previous announcement then see the new one.

## Parameters

<!-- osirion:parameters OsirionAnnouncementBar -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Id` | `string` | `""` | The announcement's ID: 1 to 40 lowercase letters, digits or hyphens, such as `release-4-1`. A dismissal is remembered per ID, so a new announcement needs a new one. |
| `ChildContent` | `RenderFragment?` | `null` | The announcement, a normal Razor fragment that may contain links. It is encoded like any Razor output. |
| `Dismissible` | `bool` | `true` | Whether the bar has a close button. |
| `RememberDays` | `int` | `30` | How many days a dismissal lasts, from 1 to 365. Any other value throws an `ArgumentOutOfRangeException`. |
| `DismissText` | `string` | `"Dismiss announcement"` | The accessible name of the close button. |
| `RegionLabel` | `string` | `"Announcement"` | The accessible name of the bar, which is a region landmark. |
| `DismissEndpoint` | `string` | `"/api/announcement/dismiss"` | Where the close form posts. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` and unmatched attributes go on the bar element.

An invalid `Id` throws `ArgumentException`, and a `RememberDays` outside 1 to 365 throws
`ArgumentOutOfRangeException`, so a typo fails during development instead of writing a bad cookie name.

## Behavior

- **Closing without script.** The close button submits a form with the ID, the lifetime and the current path. The
  endpoint sets the cookie `osirion_announcement_{Id}` and redirects back; the next render leaves the bar out.
- **The cookie.** `HttpOnly`, `Secure`, `SameSite=Lax`, path `/`. Browsers treat `localhost` as secure, so local
  development keeps the dismissal; another plain HTTP host drops it.
- **Security.** The form carries an antiforgery token. A post without a valid token or with an invalid ID gets 400,
  the lifetime is bounded to 365 days, and the endpoint redirects only to local paths.
- **Rendering.** The bar reads the cookie from the request, so it renders under static SSR and while prerendering.
  Without an HTTP request (an interactive render) it renders nothing rather than show a dismissed announcement again,
  so keep it in a statically rendered layout.
- **Accessibility.** The bar is a region named by `RegionLabel`. The close button has an accessible name from
  `DismissText` and a visible focus outline; its "x" glyph is hidden from assistive technology.
- **Content.** `ChildContent` is a normal Razor fragment and is encoded like any other Razor output.

## Styling

| Class | Element |
| --- | --- |
| `osirion-announcement` | The bar: a centered flex row. |
| `osirion-announcement-content` | The announcement text; links in it inherit the bar's color and are underlined. |
| `osirion-announcement-form` | The close form. |
| `osirion-announcement-dismiss` | The close button. |

The background is `--osirion-action-primary` and the text `--osirion-action-primary-text`, so the bar follows the
theme and the CSS framework adapter. Override them on the bar to use other colors:

```css
.osirion-announcement {
    --osirion-action-primary: #14532d;
    --osirion-action-primary-text: #ffffff;
}
```

## Related

- [OsirionCookieConsent](/components/cookie-consent)
- [OsirionPageLayout](/components/page-layout)
- [Forms and feedback](/components/forms-and-feedback)
