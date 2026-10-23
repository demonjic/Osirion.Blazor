---
id: 'analytics-yandex-metrica-tracker'
order: 6
layout: docs
title: YandexMetricaTracker
permalink: /components/yandex-metrica-tracker
description: YandexMetricaTracker writes the Yandex Metrica counter script into the page head from configuration and reports page views after enhanced navigation.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-yandex-metrica-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- yandex-metrica
- tracking
- enhanced-navigation
published: true
slug: yandex-metrica-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'YandexMetricaTracker: Yandex Metrica counter for Blazor'
  description: 'YandexMetricaTracker writes the Yandex Metrica counter script into the page head from configuration and reports page views after enhanced navigation.'
  image: 'https://media.getosirion.com/og/components-yandex-metrica-tracker.jpg'
  canonical: 'https://getosirion.com/components/yandex-metrica-tracker'
  lang: en
  robots: index, follow
  og_title: 'YandexMetricaTracker - Osirion.Blazor'
  og_description: 'Writes the Yandex Metrica counter into the page head and reports page views after enhanced navigation.'
  og_image_url: 'https://media.getosirion.com/og/components-yandex-metrica-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'YandexMetricaTracker - Osirion.Blazor'
  twitter_description: 'Writes the Yandex Metrica counter into the page head and reports page views after enhanced navigation.'
  twitter_image_url: 'https://media.getosirion.com/og/components-yandex-metrica-tracker.jpg'
  meta_tags: {}
---

# YandexMetricaTracker

`YandexMetricaTracker` adds the Yandex Metrica counter to a page. Use it when you measure a Blazor site with Yandex Metrica. It renders a script, not visible markup, and a live demo would load Yandex's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/YandexMetricaTracker.razor).

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddYandexMetrica(options => options.CounterId = "12345678"));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <YandexMetricaTracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:YandexMetrica` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "YandexMetrica": { "CounterId": "12345678" }
    }
  }
}
```

The key `YandexMetrica` at the top level of the configuration, used before the `Osirion:Analytics` prefix existed, is still read when the new section is absent.

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters YandexMetricaTracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `CounterId` | `string?` | `null` | The counter ID for this render, replacing the configured `CounterId`. It must be a number of up to 18 digits; anything else renders nothing. |
| `WebVisor` | `bool?` | `null` | Whether Webvisor session replay is on for this render, replacing the configured `WebVisor`, which is `false` unless configured. |
| `ClickMap` | `bool?` | `null` | Whether the click map is on for this render, replacing the configured `ClickMap`, which is `true` unless configured. |
| `TrackLinks` | `bool?` | `null` | Whether clicks on outbound links are tracked for this render, replacing the configured `TrackLinks`, which is `true` unless configured. |
| `AccurateTrackBounce` | `bool?` | `null` | Whether accurate bounce rate tracking is on for this render, replacing the configured `AccurateTrackBounce`, which is `true` unless configured. |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`CounterId`, `WebVisor`, `ClickMap`, `TrackLinks` and `AccurateTrackBounce` replace the configured values for this render. `Enabled` set to `false` turns the tracker off for this render. `SiteId` is inherited but ignored: this tracker takes `CounterId` instead.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:

`12345678` is a placeholder counter ID.

```html
<script type="text/javascript">
    (function(m,e,t,r,i,k,a){m[i]=m[i]||function(){(m[i].a=m[i].a||[]).push(arguments)};
    m[i].l=1*new Date();
    for (var j = 0; j < document.scripts.length; j++) {if (document.scripts[j].src === r) { return; }}
    k=e.createElement(t),a=e.getElementsByTagName(t)[0],k.async=1,k.src=r,a.parentNode.insertBefore(k,a)})
    (window, document, "script", "https://mc.yandex.ru/metrika/tag.js", "ym");

    ym(12345678, "init", {"clickmap":true,"trackLinks":true,"accurateTrackBounce":true});
</script>
<script src="/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js" defer data-osirion-analytics="yandex" data-counter="12345678"></script>
```

The init options list only what is on: the defaults give `clickmap`, `trackLinks` and `accurateTrackBounce`. The counter ID is written as a bare number, which is why it must be digits only. The last script is the page view module (see Behavior); its address is relative, so the document needs a `<base href="/" />`.


## Configuration

The keys live under `Osirion:Analytics:YandexMetrica`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `CounterId` | empty | The counter ID, digits only (up to 18). Anything else renders nothing. |
| `TrackLinks` | `true` | Writes `trackLinks`. |
| `AccurateTrackBounce` | `true` | Writes `accurateTrackBounce`. |
| `ClickMap` | `true` | Writes `clickmap`. |
| `WebVisor` | `false` | Writes `webvisor`, session replay. |
| `TrackHash` | none | Written as `trackHash` when set. |
| `DeferLoad` | `false` | `true` writes `defer: true`. |
| `AlternativeCdn` | none | The script address instead of `https://mc.yandex.ru/metrika/tag.js`. It must be an absolute http(s) address; otherwise the default is used. |
| `Params`, `UserParams` | none | Dictionaries written as `params` and `userParams` when not empty. |
| `EcommerceEnabled`, `EcommerceContainerName` | `false`, `dataLayer` | `EcommerceEnabled` writes `ecommerce` with the container name. |
| `AutoTrackPageViews` | `true` | Adds the page view module script; `false` leaves it out. |
| `HashTracking` | `false` | The older spelling of `TrackHash`: `true` writes `trackHash: true` while `TrackHash` is not set. |

`CounterId` and `AlternativeCdn` are checked at startup when the service is enabled: a non-blank `CounterId` that is not numeric, or an `AlternativeCdn` that is not an absolute http(s) address, stops the app with a message that names the key.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

On a site exported to static files, set `Evaluation` to `Browser` (`Osirion:Analytics:Consent:Evaluation`).
The tracker then renders for every visitor the script it would write after consent, held as `type="text/plain"`,
and `osirion-consent.js` runs it once the visitor accepted the category; until then no request reaches Yandex.
See [Sites exported to static files](/components/cookie-consent#sites-exported-to-static-files).

Yandex Metrica has no consent API in this tracker: the gate only decides whether the script is written. The `noscript` pixel from the Yandex snippet is not written: it is valid only in the body, and the tracker belongs in the head, so visitors without JavaScript are not counted.

## Content Security Policy

The tracker writes an inline script and adds no nonce, so `script-src` must allow inline scripts, or allow this script by its hash. Allow `https://mc.yandex.ru` for `script-src` (or the host of `AlternativeCdn`); `img-src` is not needed for a `noscript` pixel, because the tracker writes none. The counter then sends to Yandex addresses on its own; this code does not show them, so take them from Yandex's documentation. With consent evaluated in the browser, the held script runs with its text unchanged, so the same hash allows it; a static site has no per-response nonce, so use the hash there.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, `CounterId` is numeric and, with the consent gate on, the visitor consented.
- **Page views after enhanced navigation.** Blazor enhanced navigation patches the page without a full load, so the counter would not see the new page. The page view module (`/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js`) listens for Blazor's `enhancedload` event and calls `ym(counter, 'hit', url, ...)` with the new URL, title and referrer. Navigation inside a router that already runs interactively does not raise `enhancedload` and is not reported.
- **Events.** The tracker offers no custom event API; call `ym` from your own script.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
