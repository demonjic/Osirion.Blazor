---
id: 'analytics-plausible-tracker'
order: 4
layout: docs
title: PlausibleTracker
permalink: /components/plausible-tracker
description: PlausibleTracker writes the Plausible Analytics script and its init call into the page head from configuration.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-plausible-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- plausible
- tracking
- privacy
published: true
slug: plausible-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'PlausibleTracker: Plausible Analytics script for Blazor'
  description: 'PlausibleTracker writes the Plausible Analytics script and its init call into the page head from configuration.'
  image: 'https://media.getosirion.com/og/components-plausible-tracker.jpg'
  canonical: 'https://getosirion.com/components/plausible-tracker'
  lang: en
  robots: index, follow
  og_title: 'PlausibleTracker - Osirion.Blazor'
  og_description: 'Writes the Plausible Analytics script and init call into the page head from configuration.'
  og_image_url: 'https://media.getosirion.com/og/components-plausible-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'PlausibleTracker - Osirion.Blazor'
  twitter_description: 'Writes the Plausible Analytics script and init call into the page head from configuration.'
  twitter_image_url: 'https://media.getosirion.com/og/components-plausible-tracker.jpg'
  meta_tags: {}
---

# PlausibleTracker

`PlausibleTracker` adds the Plausible Analytics script to a page. Use it when you measure a Blazor site with Plausible, cloud or self-hosted. It renders a script, not visible markup, and a live demo would load Plausible's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/PlausibleTracker.razor). Available from 4.0.

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddPlausible(options => options.ScriptUrl = "https://analytics.example.com/js/your-script.js"));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <PlausibleTracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:Plausible` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "Plausible": { "ScriptUrl": "https://analytics.example.com/js/your-script.js" }
    }
  }
}
```

`ScriptUrl` is the script address from your site's settings in Plausible; Plausible identifies the site by that script, so there is no site ID.

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters PlausibleTracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Only `Enabled` is used by this tracker. `SiteId` is inherited but ignored: Plausible identifies the site by its script.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:

The script address is a placeholder; copy the real one from your Plausible site settings.

```html
<script async src="https://analytics.example.com/js/your-script.js"></script>
<script>
    window.plausible=window.plausible||function(){(plausible.q=plausible.q||[]).push(arguments)},plausible.init=plausible.init||function(i){plausible.o=i||{}};
    plausible.init({});
</script>
```

The second script is Plausible's own queue stub followed by `plausible.init`, whose argument holds only the options you set (empty here), so the settings of the site in Plausible apply for the rest. With `Endpoint`, `HashBasedRouting` and the switches below set, they appear as `endpoint`, `hashBasedRouting` and so on; `AutoTrackPageViews` set to `false` writes `autoCapturePageviews: false`.


## Configuration

The keys live under `Osirion:Analytics:Plausible`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `ScriptUrl` | empty | The script address from the Plausible site settings. Required: without it the tracker renders nothing. It must be an http(s) address or a site path such as `/js/script.js`. |
| `Endpoint` | none | A custom event endpoint, for a proxy; written as `endpoint`. It must be an http(s) address or a site path. |
| `OutboundLinks`, `FileDownloads`, `FormSubmissions`, `HashBasedRouting`, `CaptureOnLocalhost` | none | Nullable switches, written only when set, so the dashboard settings apply otherwise. |
| `AutoTrackPageViews` | `true` | `false` writes `autoCapturePageviews: false`. |

`ScriptUrl` and `Endpoint` are checked at startup when the service is enabled: a non-blank value that is not an http(s) address or a site path stops the app with a message that names the key.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

On a site exported to static files, set `Evaluation` to `Browser` (`Osirion:Analytics:Consent:Evaluation`).
The tracker then renders for every visitor the script it would write after consent, held as `type="text/plain"`,
and `osirion-consent.js` runs it once the visitor accepted the category; until then no request reaches Plausible.
See [Sites exported to static files](/components/cookie-consent#sites-exported-to-static-files).

Plausible sets no cookies. Whether you still ask for consent is your decision; the gate covers this tracker like the others and, when on, holds the script back until the visitor consented.

## Content Security Policy

The tracker writes an inline script and adds no nonce, so `script-src` must allow inline scripts, or allow this script by its hash. Allow the host of `ScriptUrl` for `script-src`, and, if you set `Endpoint`, its host as well. The code only passes `Endpoint` to the script, and the script decides where it sends events, so take the addresses it contacts from Plausible's documentation. With consent evaluated in the browser, the held script runs with its text unchanged, so the same hash allows it; a static site has no per-response nonce, so use the hash there.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, `ScriptUrl` and a set `Endpoint` are valid and, with the consent gate on, the visitor consented.
- **Page views.** The script reports page views itself, including enhanced navigations, through the History API. The tracker emits no page view module.
- **Events.** Send custom events from the browser with `plausible('Event name')`; the queue stub lets you call it before the script has loaded.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
