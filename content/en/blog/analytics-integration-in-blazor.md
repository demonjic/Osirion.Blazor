---
id: ""
title: Analytics in Blazor with Osirion.Blazor
description: Add Clarity, Matomo, GA4 or Yandex Metrica to a Blazor app with tracker components, configuration checked at startup and an optional consent gate.
author: Dejan Demonjić
date: 2025-04-12
last_modified: 2026-10-11
featured_image: 'https://media.getosirion.com/og/blog-analytics-integration-in-blazor.jpg'
categories: 
- Analytics
- Integration
tags: 
- Analytics
- Clarity
- Matomo
- GA4
- Privacy
- Blazor
is_featured: true
published: true
layout: null
slug: analytics-integration-in-blazor
custom_fields: {}
seo: 
  meta_title: ""
  meta_description: ""
  canonical_url: ""
  robots: "index, follow"
  og_title: ""
  og_description: ""
  og_image_url: ""
  og_type: article
  json_ld: ""
  schema_type: Article
  twitter_card: summary_large_image
  twitter_title: ""
  twitter_description: ""
  twitter_image_url: ""

seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-analytics-integration-in-blazor.jpg'
---

# Analytics in Blazor with Osirion.Blazor

An analytics product gives you a script snippet and leaves the rest to you: where to put it, how to keep the IDs out of
your markup, what happens when Blazor's enhanced navigation swaps the page, and how to wait for consent. The
`Osirion.Blazor.Analytics` package handles those parts. You register a provider, put its tracker component in the page
head, and the component writes the provider's script during server rendering.

Osirion.Blazor 4.0 ships trackers for Microsoft Clarity, Matomo, Google Analytics 4 and Yandex Metrica. Available from
4.0: Plausible (`AddPlausible`, `PlausibleTracker`) and Umami (`AddUmami`, `UmamiTracker`), both cookie-free.

## Register the providers

The package comes with the `Osirion.Blazor` meta-package, or you can install it on its own:

```bash
dotnet add package Osirion.Blazor.Analytics
```

Register the providers you use in `Program.cs`:

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics =>
{
    analytics
        .AddClarity(options => options.SiteId = "your-clarity-project-id")
        .AddMatomo(options =>
        {
            options.TrackerUrl = "https://analytics.example.com/";
            options.SiteId = "1";
        })
        .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX");
});
```

If you already call `AddOsirion(builder.Configuration)`, you can leave the code out and configure the same providers in
`appsettings.json`. A provider is registered when its section exists:

```json
{
  "Osirion": {
    "Analytics": {
      "Clarity": { "SiteId": "your-clarity-project-id" },
      "Matomo": { "TrackerUrl": "https://analytics.example.com/", "SiteId": "1" },
      "GA4": { "MeasurementId": "G-XXXXXXXXXX" }
    }
  }
}
```

The Matomo tracker appends `matomo.php` and `matomo.js` to `TrackerUrl`, so end it with a slash.

## Render the trackers

Put the tracker components in the `<head>` of `App.razor`, after `<HeadOutlet />`:

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    ...
    <HeadOutlet />

    <ClarityTracker />
    <MatomoTracker />
    <GA4Tracker />
</head>
```

A tracker renders its script on the server, so it works under static SSR and in the prerender pass of the interactive
render modes. It renders nothing when its provider is disabled or a required value is missing.

In 4.0, keep a tracker in the layout only when its provider is registered: a `<ClarityTracker />` without a Clarity
configuration fails the page. From 4.0, such a tracker renders nothing.

Every tracker takes an `Enabled` parameter that overrides the configured value for that render. `ClarityTracker` and
`MatomoTracker` also take `SiteId`; `GA4Tracker` takes `MeasurementId` and `DebugMode`; `YandexMetricaTracker` takes
`CounterId`, `WebVisor`, `ClickMap`, `TrackLinks` and `AccurateTrackBounce`.

## Turn tracking off outside production

Every provider has an `Enabled` option, `true` by default. Set it to `false` in `appsettings.Development.json` and the
trackers render nothing while you work locally:

```json
{
  "Osirion": {
    "Analytics": {
      "Clarity": { "Enabled": false },
      "Matomo": { "Enabled": false },
      "GA4": { "Enabled": false }
    }
  }
}
```

## Bad values fail at startup

The options are validated when the application starts. A GA4 measurement ID that does not look like `G-XXXXXXXXXX`, a
Yandex Metrica counter that is not numeric, or a tracker URL that is not an http(s) URL stops the app with a message
that names the configuration key, so a typo never ships as a broken snippet. A provider whose `Enabled` is `false` is
not validated.

Every configured value written into a script or an attribute is encoded, so a value from configuration cannot break out
of the snippet.

## Page views after enhanced navigation

With enhanced navigation, Blazor replaces the page content without a full page load, so a script that counts a page
view on load sees only the first page. Each provider deals with it in its own way:

- **Matomo and Yandex Metrica**: while `AutoTrackPageViews` is on (the default), the tracker adds a static script,
  `/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js`, that listens for Blazor's `enhancedload` event and
  reports the new URL. It is an external file, not an inline script.
- **GA4**: turn on "Page changes based on browser history events" in the Enhanced Measurement settings of your GA4 web
  data stream.
- **Clarity**: the tracker adds nothing; Clarity's own script follows history changes.

Navigation inside a router that is already interactive does not raise `enhancedload`, so the static script does not
report it.

## Consent

By default, a configured tracker renders on every page. If your site needs consent before analytics runs, turn on the
consent gate:

```csharp
builder.Services.AddOsirionAnalytics(analytics =>
{
    analytics
        .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX")
        .RequireConsent();
});
```

or in configuration:

```json
{ "Osirion": { "Analytics": { "Consent": { "RequireConsent": true } } } }
```

With the gate on, a tracker renders only when the request carries the cookie written by the `OsirionCookieConsent`
banner and the visitor accepted the `analytics` category. Without that, no tracker script reaches the page. The
[cookie consent article](/blog/gdpr-cookie-consent-blazor) covers the banner, the cookie, and how GA4 Consent Mode and
Matomo's own consent setting fit in.

## Related

- [Analytics components](/docs/packages/analytics)
- [Cookie consent in Blazor with Osirion.Blazor](/blog/gdpr-cookie-consent-blazor)
- [Osirion.Blazor.Analytics on NuGet](https://www.nuget.org/packages/Osirion.Blazor.Analytics)
