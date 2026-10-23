---
id: 'analytics-clarity-tracker'
order: 1
layout: docs
title: ClarityTracker
permalink: /components/clarity-tracker
description: ClarityTracker writes the Microsoft Clarity tracking script into the page head from configuration, with an optional consent gate.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-clarity-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- clarity
- microsoft-clarity
- tracking
published: true
slug: clarity-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'ClarityTracker: Microsoft Clarity script for Blazor'
  description: 'ClarityTracker writes the Microsoft Clarity tracking script into the page head from configuration, with an optional consent gate.'
  image: 'https://media.getosirion.com/og/components-clarity-tracker.jpg'
  canonical: 'https://getosirion.com/components/clarity-tracker'
  lang: en
  robots: index, follow
  og_title: 'ClarityTracker - Osirion.Blazor'
  og_description: 'Writes the Microsoft Clarity tracking script into the page head from configuration.'
  og_image_url: 'https://media.getosirion.com/og/components-clarity-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ClarityTracker - Osirion.Blazor'
  twitter_description: 'Writes the Microsoft Clarity tracking script into the page head from configuration.'
  twitter_image_url: 'https://media.getosirion.com/og/components-clarity-tracker.jpg'
  meta_tags: {}
---

# ClarityTracker

`ClarityTracker` adds the Microsoft Clarity tracking script to a page. Use it when you want Clarity session recordings and heatmaps on a Blazor site. It renders a script, not visible markup, and a live demo would load Microsoft Clarity's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/ClarityTracker.razor).

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddClarity(options => options.SiteId = "YOUR_CLARITY_PROJECT_ID"));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <ClarityTracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:Clarity` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "Clarity": { "SiteId": "YOUR_CLARITY_PROJECT_ID" }
    }
  }
}
```

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters ClarityTracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`SiteId` replaces the configured Clarity project ID for this render. `Enabled` set to `false` turns the tracker off for this render.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:


```html
<script type="text/javascript">
    (function (c, l, a, r, i, t, y) {
        c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
        t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
        y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
    })(window, document, "clarity", "script", "YOUR_CLARITY_PROJECT_ID");
</script>
```

The script address is `TrackerUrl` followed by the project ID. Both values are JSON-encoded before they are written, so neither can close the script element.


## Configuration

The keys live under `Osirion:Analytics:Clarity`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `SiteId` | none | The Clarity project ID. Without it the tracker renders nothing. |
| `TrackerUrl` | `https://www.clarity.ms/tag/` | The address the script is loaded from, followed by the project ID. It must be an absolute http(s) address or a protocol-relative one. |
| `TrackUserAttributes` | `true` | Obsolete and without effect: Clarity has no install-time setting for it. Call `clarity("identify", ...)` or `clarity("set", key, value)` from your own code. |

`TrackerUrl` is checked at startup when the service is enabled: a value that is not an absolute http(s) address, or protocol-relative, stops the app with a message that names the key. `AutoTrackPageViews` has no effect on Clarity, which follows history changes itself.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

On a site exported to static files, set `Evaluation` to `Browser` (`Osirion:Analytics:Consent:Evaluation`).
The tracker then renders for every visitor the script it would write after consent, held as `type="text/plain"`,
and `osirion-consent.js` runs it once the visitor accepted the category; until then no request reaches Microsoft Clarity.
See [Sites exported to static files](/components/cookie-consent#sites-exported-to-static-files).

Clarity has no consent API in this tracker: the gate only decides whether the script is written.

## Content Security Policy

The tracker writes an inline script and adds no nonce, so `script-src` must allow inline scripts, or allow this script by its hash. Allow the host of `TrackerUrl` as well: `https://www.clarity.ms` by default. The script then makes its own requests to hosts it chooses; this code does not show them, so take them from Microsoft's Clarity documentation. With consent evaluated in the browser, the held script runs with its text unchanged, so the same hash allows it; a static site has no per-response nonce, so use the hash there.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, `SiteId` is not blank and `TrackerUrl` is valid, and, with the consent gate on, the visitor consented.
- **Page views.** Clarity tracks page views and follows history changes itself, so enhanced navigation needs nothing from Osirion. The tracker emits no page view module.
- **Events.** The tracker offers no custom event API.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
