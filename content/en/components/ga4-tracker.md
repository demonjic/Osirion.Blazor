---
id: 'analytics-ga4-tracker'
order: 2
layout: docs
title: GA4Tracker
permalink: /components/ga4-tracker
description: GA4Tracker writes the Google Analytics 4 gtag.js snippet into the page head from configuration, with Consent Mode when the consent gate is on.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-ga4-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- ga4
- google-analytics
- gtag
- consent-mode
published: true
slug: ga4-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'GA4Tracker: Google Analytics 4 tag for Blazor'
  description: 'GA4Tracker writes the Google Analytics 4 gtag.js snippet into the page head from configuration, with Consent Mode when the consent gate is on.'
  image: 'https://media.getosirion.com/og/components-ga4-tracker.jpg'
  canonical: 'https://getosirion.com/components/ga4-tracker'
  lang: en
  robots: index, follow
  og_title: 'GA4Tracker - Osirion.Blazor'
  og_description: 'Writes the Google Analytics 4 gtag.js snippet into the page head, with Consent Mode when the consent gate is on.'
  og_image_url: 'https://media.getosirion.com/og/components-ga4-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'GA4Tracker - Osirion.Blazor'
  twitter_description: 'Writes the Google Analytics 4 gtag.js snippet into the page head, with Consent Mode when the consent gate is on.'
  twitter_image_url: 'https://media.getosirion.com/og/components-ga4-tracker.jpg'
  meta_tags: {}
---

# GA4Tracker

`GA4Tracker` adds the Google Analytics 4 tag (`gtag.js`) to a page. Use it when you measure a Blazor site with Google Analytics. It renders a script, not visible markup, and a live demo would load Google's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/GA4Tracker.razor).

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX"));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <GA4Tracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:GA4` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "GA4": { "MeasurementId": "G-XXXXXXXXXX" }
    }
  }
}
```

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters GA4Tracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `MeasurementId` | `string?` | `null` | The measurement ID for this render, replacing the configured `MeasurementId`. It must look like `G-XXXXXXXXXX` (or `GT-` followed by letters and digits); anything else renders nothing. |
| `DebugMode` | `bool?` | `null` | Whether GA4 debug mode is written for this render, replacing the configured `DebugMode`, which is `false` unless configured. |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`MeasurementId` and `DebugMode` replace the configured values for this render. `Enabled` set to `false` turns the tracker off for this render. `SiteId` is inherited but ignored: this tracker takes `MeasurementId` instead.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:

`G-XXXXXXXXXX` is a placeholder measurement ID.

```html
<script async src="https://www.googletagmanager.com/gtag/js?id=G-XXXXXXXXXX"></script>
<script>
    window.dataLayer = window.dataLayer || [];
    function gtag(){dataLayer.push(arguments);}
    gtag('js', new Date());
    gtag('config', "G-XXXXXXXXXX", {"anonymize_ip":true,"send_page_view":true,"transport_type":"beacon"});
    document.addEventListener('click', function (e) {
        if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) { return; }
        var el = e.target && e.target.closest ? e.target.closest('a[href]') : null;
        if (!el || el.hasAttribute('download')) { return; }
        var target = el.getAttribute('target');
        if (target && target !== '_self') { return; }
        if (el.protocol !== 'http:' && el.protocol !== 'https:') { return; }
        if (el.hostname === window.location.hostname) { return; }
        gtag('event', 'click', {
            'event_category': 'outbound',
            'event_label': el.href,
            'transport_type': 'beacon'
        });
    });
</script>
```

The click handler only reports: the browser follows the link itself, so a blocked or failing `gtag` cannot break navigation. Modified clicks, links with a `target` other than `_self`, downloads, non-http(s) links (`mailto:`, `tel:`) and links to the same host are skipped. The measurement ID is JSON-encoded in the script and attribute-encoded in the address.


## Configuration

The keys live under `Osirion:Analytics:GA4`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `MeasurementId` | empty | The Google tag ID. It must look like `G-XXXXXXXXXX` (`G-` or `GT-` and letters or digits); anything else renders nothing. |
| `AnonymizeIp` | `true` | Writes `anonymize_ip: true` into the config call. |
| `SendPageView` | `true` | Writes `send_page_view`; `false` leaves the first page view to your own code. |
| `TransportType` | `beacon` | Writes `transport_type`. |
| `TrackOutboundLinks` | `true` | Adds the outbound link click handler. |
| `LinkAttribution` | `true` | Obsolete and without effect. It wrote `gtag('require', 'linkid')`, a Universal Analytics command that is not part of gtag.js. |
| `DebugMode` | `false` | Writes `debug_mode: true` in the `config` call, the form the [DebugView page](https://support.google.com/analytics/answer/7201382) documents. `false` writes nothing, because Google turns debug mode off by leaving the parameter out. |
| `CookieDomain`, `CookieFlags`, `CookieExpires` | none | Written into the config call when set (`CookieExpires` is an integer). |
| `RestrictDataProcessing` | `false` | Writes `restricted_data_processing: true`. |
| `ConfigParameters` | none | A dictionary of extra parameters merged into the config call; a key of the same name replaces the one above. |

`MeasurementId` is checked at startup when the service is enabled: a non-blank value that does not look like a Google tag ID stops the app with a message that names the key. `AutoTrackPageViews` has no effect on GA4: use `SendPageView`.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

With the gate on, the script first sets Google Consent Mode defaults to denied for `ad_storage`, `ad_user_data`, `ad_personalization` and `analytics_storage`, then grants `analytics_storage` only, before `gtag('js')` and the config call. Advertising signals stay denied, because this gate says nothing about marketing consent. With the gate off, no Consent Mode call is written.

## Content Security Policy

The tracker writes an inline script and adds no nonce, so `script-src` must allow inline scripts, or allow this script by its hash. Allow `https://www.googletagmanager.com` for the script, which is the only address this code writes. The tag then loads and sends to further Google addresses on its own; this code does not show them, so take them from Google's documentation for the tag.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, the measurement ID is valid and, with the consent gate on, the visitor consented.
- **Page views.** The first page view comes from the config call. After a Blazor enhanced navigation GA4 relies on its Enhanced Measurement setting "Page changes based on browser history events"; the tracker emits no page view module.
- **Events.** The tracker offers no custom event API; call `gtag` from your own script.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
