---
id: 'analytics-matomo-tracker'
order: 3
layout: docs
title: MatomoTracker
permalink: /components/matomo-tracker
description: MatomoTracker writes the Matomo tracking script into the page head from configuration and reports page views after enhanced navigation.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-matomo-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- matomo
- tracking
- enhanced-navigation
published: true
slug: matomo-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'MatomoTracker: Matomo tracking script for Blazor'
  description: 'MatomoTracker writes the Matomo tracking script into the page head from configuration and reports page views after enhanced navigation.'
  image: 'https://media.getosirion.com/og/components-matomo-tracker.jpg'
  canonical: 'https://getosirion.com/components/matomo-tracker'
  lang: en
  robots: index, follow
  og_title: 'MatomoTracker - Osirion.Blazor'
  og_description: 'Writes the Matomo tracking script into the page head and reports page views after enhanced navigation.'
  og_image_url: 'https://media.getosirion.com/og/components-matomo-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'MatomoTracker - Osirion.Blazor'
  twitter_description: 'Writes the Matomo tracking script into the page head and reports page views after enhanced navigation.'
  twitter_image_url: 'https://media.getosirion.com/og/components-matomo-tracker.jpg'
  meta_tags: {}
---

# MatomoTracker

`MatomoTracker` adds the Matomo tracking script to a page. Use it when you run Matomo, self-hosted or in the cloud, and want its page views from a Blazor site. It renders a script, not visible markup, and a live demo would load your Matomo server's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/MatomoTracker.razor).

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddMatomo(options =>
    {
        options.SiteId = "1";
        options.TrackerUrl = "https://analytics.example.com/";
    }));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <MatomoTracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:Matomo` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "Matomo": { "SiteId": "1", "TrackerUrl": "https://analytics.example.com/" }
    }
  }
}
```

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters MatomoTracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`SiteId` replaces the configured site ID for this render. `Enabled` set to `false` turns the tracker off for this render.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:

`analytics.example.com` is a placeholder for your Matomo host and `1` for your site ID.

```html
<script>
    var _paq = window._paq = window._paq || [];
    _paq.push(['trackPageView']);
    _paq.push(['enableLinkTracking']);
    (function () {
        var u = "https://analytics.example.com/";
        _paq.push(['setTrackerUrl', u + 'matomo.php']);
        _paq.push(['setSiteId', "1"]);
        var d = document, g = d.createElement('script'), s = d.getElementsByTagName('script')[0];
        g.async = true; g.src = u + 'matomo.js'; s.parentNode.insertBefore(g, s);
    })();
</script>
<script src="/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js" defer data-osirion-analytics="matomo"></script>
```

The second script is the page view module (see Behavior). Its address is relative, so the document needs a `<base href="/" />`, which the Blazor templates have.


## Configuration

The keys live under `Osirion:Analytics:Matomo`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `SiteId` | none | The Matomo site ID. Without it the tracker renders nothing. |
| `TrackerUrl` | empty | The address of your Matomo installation, ending in a slash; `matomo.php` and `matomo.js` are appended. Required: without it the tracker renders nothing. It must be an absolute http(s) address or a protocol-relative one such as `//analytics.example.com/`. |
| `TrackLinks` | `true` | Adds `enableLinkTracking`. |
| `RequireConsent` | `false` | Matomo's own consent mode: adds `requireConsent`, so Matomo stays silent until your page pushes `setConsentGiven` or `rememberConsentGiven` onto `_paq`. |
| `AutoTrackPageViews` | `true` | Adds the page view module script; `false` leaves it out. The first page view is always sent. |
| `TrackDownloads` | `true` | Obsolete and without effect: Matomo reports downloads together with outlinks when `TrackLinks` is on, and has no documented switch for downloads alone. |

`TrackerUrl` is checked at startup when the service is enabled and not blank: a value that is neither an absolute http(s) address nor protocol-relative stops the app with a message that names the key.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

On a site exported to static files, set `Evaluation` to `Browser` (`Osirion:Analytics:Consent:Evaluation`).
The tracker then renders for every visitor the script it would write after consent, held as `type="text/plain"`,
and `osirion-consent.js` runs it once the visitor accepted the category; until then no request reaches Matomo.
See [Sites exported to static files](/components/cookie-consent#sites-exported-to-static-files).

With the osirion consent gate on, the script only exists after the visitor consented, so Matomo's own `requireConsent` is not written even when `RequireConsent` is `true`. With the gate off, `RequireConsent` keeps Matomo silent until your page grants consent in `_paq`.

## Content Security Policy

The tracker writes an inline script and adds no nonce, so `script-src` must allow inline scripts, or allow this script by its hash. Allow the host of `TrackerUrl` for `script-src` (`matomo.js`) and for the requests to `matomo.php`. The page view module is an external script from your own site, so it needs no inline allowance. With consent evaluated in the browser, the held script runs with its text unchanged, so the same hash allows it; a static site has no per-response nonce, so use the hash there.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, `SiteId` is not blank, `TrackerUrl` is valid and, with the consent gate on, the visitor consented.
- **Page views after enhanced navigation.** Blazor enhanced navigation patches the page without a full load, so Matomo would not see the new page. The page view module (`/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js`) listens for Blazor's `enhancedload` event and pushes the new referrer, URL, title and a page view onto `_paq`. Navigation inside a router that already runs interactively does not raise `enhancedload` and is not reported.
- **Events.** The tracker offers no custom event API; push to `_paq` from your own script.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
