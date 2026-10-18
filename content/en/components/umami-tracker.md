---
id: 'analytics-umami-tracker'
order: 5
layout: docs
title: UmamiTracker
permalink: /components/umami-tracker
description: UmamiTracker writes the Umami script tag, configured through data attributes, into the page head from configuration.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-umami-tracker.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- umami
- tracking
- privacy
published: true
slug: umami-tracker
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'UmamiTracker: Umami tracking script for Blazor'
  description: 'UmamiTracker writes the Umami script tag, configured through data attributes, into the page head from configuration.'
  image: 'https://media.getosirion.com/og/components-umami-tracker.jpg'
  canonical: 'https://getosirion.com/components/umami-tracker'
  lang: en
  robots: index, follow
  og_title: 'UmamiTracker - Osirion.Blazor'
  og_description: 'Writes the Umami script tag, configured through data attributes, into the page head.'
  og_image_url: 'https://media.getosirion.com/og/components-umami-tracker.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'UmamiTracker - Osirion.Blazor'
  twitter_description: 'Writes the Umami script tag, configured through data attributes, into the page head.'
  twitter_image_url: 'https://media.getosirion.com/og/components-umami-tracker.jpg'
  meta_tags: {}
---

# UmamiTracker

`UmamiTracker` adds the Umami tracking script to a page. Use it when you measure a Blazor site with Umami Cloud or a self-hosted Umami. It renders a script, not visible markup, and a live demo would load Umami's script on this page, so this
page has no demo and shows the markup it renders instead.

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Analytics/Components/UmamiTracker.razor). Available from 4.0.

## Usage

Register the service once, then place the tracker in the `<head>` of `App.razor`, after `<HeadOutlet />`. It renders
on the server, so it works under Static SSR with no interactive render mode.

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddUmami(options => options.SiteId = "00000000-0000-0000-0000-000000000000"));
```

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <UmamiTracker />
</head>
```

The same options can come from configuration. `AddOsirionAnalytics(builder.Configuration)` registers the service when
the `Osirion:Analytics:Umami` section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "Umami": { "SiteId": "00000000-0000-0000-0000-000000000000" }
    }
  }
}
```

For a self-hosted Umami, also set `ScriptUrl` to your instance's script (the default is the Umami Cloud script).

A tracker whose service is not registered, or whose required value is missing or invalid, renders nothing.

## Parameters

A parameter you leave unset keeps the configured value.

<!-- osirion:parameters UmamiTracker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteId` | `string?` | `null` | The site ID for this render, replacing the configured `SiteId`. `ClarityTracker` (the project ID), `MatomoTracker` (the site ID) and `UmamiTracker` (the website ID, a GUID) use it. `GA4Tracker`, `YandexMetricaTracker` and `PlausibleTracker` ignore it: they take their own ID parameter or none. |
| `Enabled` | `bool?` | `null` | Whether the tracker renders, replacing the configured `Enabled` for this render. `false` turns the tracker off. `true` does not skip the other requirements: a valid ID, a registered service and, with the consent gate on, the visitor consent. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`SiteId` replaces the configured website ID for this render and must be a GUID. `Enabled` set to `false` turns the tracker off for this render.

The last three rows, `Theme`, `Class` and `Style`, come from the component base class and have no effect: the
tracker renders no wrapper element.

## Rendered markup

With the defaults and these placeholder values (not real ones), the tracker writes:

The website ID is a placeholder (all zeros); copy the real one from Umami.

```html
<script defer src="https://cloud.umami.is/script.js" data-website-id="00000000-0000-0000-0000-000000000000"></script>
```

This is the whole output: Umami is configured through attributes only, so the tracker writes no inline script. An attribute appears only when its option is set.


## Configuration

The keys live under `Osirion:Analytics:Umami`. Every service also has `Enabled` (default `true`) and `AutoTrackPageViews`
(default `true`).

| Key | Default | Effect |
| --- | --- | --- |
| `SiteId` | none | The Umami website ID, a GUID. Without a valid GUID the tracker renders nothing. |
| `ScriptUrl` | `https://cloud.umami.is/script.js` | The script address, an http(s) address or a site path such as `/script.js`. Set it for a self-hosted instance. |
| `HostUrl` | none | Written as `data-host-url`. It must be an absolute http(s) address or a protocol-relative one. |
| `Domains` | none | Written as `data-domains`. |
| `Tag` | none | Written as `data-tag`. |
| `ExcludeSearch`, `ExcludeHash` | `false` | `true` writes `data-exclude-search` or `data-exclude-hash`. |
| `DoNotTrack` | `false` | `true` writes `data-do-not-track`. |
| `AutoTrackPageViews` | `true` | `false` writes `data-auto-track="false"`. |

At startup, when the service is enabled: a non-blank `SiteId` that is not a GUID, a `ScriptUrl` that is neither an http(s) address nor a site path, and a `HostUrl` that is not an absolute http(s) or protocol-relative address stop the app with a message that names the key.

## Consent

The consent gate of the package is off by default. Turn it on with `RequireConsent()` (or
`Osirion:Analytics:Consent:RequireConsent`) and this tracker renders nothing until the visitor accepted the
`analytics` category in [`OsirionCookieConsent`](/components/cookie-consent); see the
[package page](/docs/packages/analytics) for the setup. The check reads the consent cookie on the server and fails
closed: with the gate on and no `HttpContext`, for example inside a component that already runs interactively, the
tracker renders nothing. That is why trackers belong in `App.razor`.

Umami sets no cookies. Whether you still ask for consent is your decision; the gate covers this tracker like the others and, when on, holds the script back until the visitor consented.

## Content Security Policy

This tracker writes only an external script tag with no inline script, so it needs no inline allowance. Allow the host of `ScriptUrl` (`https://cloud.umami.is` by default) for `script-src`, and, if you set `HostUrl`, its host as well. The code only writes `HostUrl` as an attribute, and the script decides where it sends data, so take the addresses it contacts from Umami's documentation.

## Behavior

- **Rendering.** The markup is written only when `Enabled` is on, the website ID is a GUID, the addresses are valid and, with the consent gate on, the visitor consented.
- **Page views.** The script follows history changes itself; the tracker emits no page view module.
- **Events.** Send custom events from the browser with `umami.track()` or `data-umami-event` attributes.

## Related

- [Analytics components](/components/analytics): the six trackers and how they share setup
- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
