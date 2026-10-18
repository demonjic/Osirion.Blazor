---
id: 'analytics-components-overview'
order: 4
layout: docs
title: Osirion.Blazor.Analytics
permalink: /docs/packages/analytics
description: Add Clarity, GA4, Matomo, Plausible, Umami or Yandex Metrica to a Blazor site from configuration, with an optional cookie consent gate.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-packages-analytics.jpg'
categories:
- Components
- Analytics Components
- Performance Tracking
tags:
- blazor
- analytics
- google-analytics
- microsoft-clarity
- matomo
- yandex-metrica
- plausible
- umami
- cookie-consent
is_featured: true
published: true
slug: analytics
lang: en
redirect_from:
- /docs/components/analytics
custom_fields:
  group: packages
seo_properties:
  title: 'Analytics Trackers for Blazor - Osirion.Blazor'
  description: 'Render Clarity, Matomo, GA4, Plausible, Umami and Yandex Metrica tracking scripts from configuration in Blazor, with an optional consent gate.'
  image: 'https://media.getosirion.com/og/docs-packages-analytics.jpg'
  canonical: 'https://getosirion.com/docs/packages/analytics'
  lang: en
  robots: index, follow
  og_title: 'Analytics Trackers for Blazor - Osirion.Blazor'
  og_description: 'Tracker components for Clarity, Matomo, GA4, Plausible, Umami and Yandex Metrica, configured from appsettings.json and gated on consent.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages-analytics.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Analytics Trackers for Blazor - Osirion.Blazor'
  twitter_description: 'Tracker components for Clarity, Matomo, GA4, Plausible, Umami and Yandex Metrica, configured from appsettings.json.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages-analytics.jpg'
  meta_tags: {}
---

# Osirion.Blazor.Analytics

`Osirion.Blazor.Analytics` writes the tracking script of an analytics service into your pages. You configure the
service once, then place its tracker component in the page head. Trackers render on the server, so they work under
Static SSR with no interactive render mode. Each tracker has its own page with its parameters, configuration keys,
consent behavior and Content Security Policy sources.

| Service | Tracker component | Registration | Configuration section |
| --- | --- | --- | --- |
| Microsoft Clarity | [`ClarityTracker`](/components/clarity-tracker) | `AddClarity` | `Osirion:Analytics:Clarity` |
| Google Analytics 4 | [`GA4Tracker`](/components/ga4-tracker) | `AddGA4` | `Osirion:Analytics:GA4` |
| Matomo | [`MatomoTracker`](/components/matomo-tracker) | `AddMatomo` | `Osirion:Analytics:Matomo` |
| Plausible | [`PlausibleTracker`](/components/plausible-tracker) | `AddPlausible` | `Osirion:Analytics:Plausible` |
| Umami | [`UmamiTracker`](/components/umami-tracker) | `AddUmami` | `Osirion:Analytics:Umami` |
| Yandex Metrica | [`YandexMetricaTracker`](/components/yandex-metrica-tracker) | `AddYandexMetrica` | `Osirion:Analytics:YandexMetrica` |

Plausible and Umami are available from 4.1. The [Analytics group page](/components/analytics) compares them.

## Install

Install the package, or the `Osirion.Blazor` meta-package, which includes it:

```bash
dotnet add package Osirion.Blazor.Analytics
```

## Setup

Register the services you use in `Program.cs`:

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX")
    .AddClarity(options => options.SiteId = "your-clarity-project-id"));
```

Then render the trackers in the `<head>` of `App.razor`:

```razor
@using Osirion.Blazor.Analytics.Components

<head>
    <meta charset="utf-8" />
    <base href="/" />
    <HeadOutlet />
    <GA4Tracker />
    <ClarityTracker />
</head>
```

> [!WARNING]
> In 4.0.0, a tracker whose service is not registered fails the page. Render only the trackers you configured.
> From 4.0 such a tracker renders nothing.

## Configuration

`AddOsirionAnalytics(builder.Configuration)` registers every service whose section exists under
`Osirion:Analytics`. `AddOsirion(builder.Configuration)` from the meta-package does the same.

```json
{
  "Osirion": {
    "Analytics": {
      "GA4": { "MeasurementId": "G-XXXXXXXXXX" },
      "Matomo": { "SiteId": "1", "TrackerUrl": "https://analytics.example.com/" }
    }
  }
}
```

```csharp
builder.Services.AddOsirionAnalytics(builder.Configuration);
```

Every service has `Enabled` (default `true`) and `AutoTrackPageViews` (default `true`); what the second one does
differs per tracker, so see the tracker's page for its keys and defaults. To keep tracking off while you develop, set
`"Enabled": false` for the service in `appsettings.Development.json`.

Tracker parameters override the configured options for one render. A parameter you leave unset keeps the configured
value.

### Consent gate

The gate is off by default. Turn it on and no tracker script is rendered until the visitor accepted the `analytics`
category in [`OsirionCookieConsent`](/components/cookie-consent):

```csharp
builder.Services.AddOsirionAnalytics(analytics => analytics
    .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX")
    .RequireConsent());
```

or in configuration:

```json
{ "Osirion": { "Analytics": { "Consent": { "RequireConsent": true, "Category": "analytics" } } } }
```

Pass a configure action, such as `RequireConsent(options => options.Category = "...")`, to check a different
category. Consent is read from the consent cookie on the server. The check fails closed: with the gate on and no
`HttpContext`, for example inside a component that already runs interactively, the tracker renders nothing. That is
why trackers belong in `App.razor`.

The gate covers all six trackers. Providers with their own consent mode react to it: the
[GA4 tracker](/components/ga4-tracker) sets Google Consent Mode, and the [Matomo tracker](/components/matomo-tracker)
leaves out its own `requireConsent`.

### Security

Configured values are JSON or attribute encoded before they are written into scripts and attributes. Formats are
checked at startup for every enabled service, and a bad value stops the app with a message that names the
configuration key; the tracker pages list what each one checks. Most trackers write inline scripts and do not add a
nonce, so a Content Security Policy must allow them: see each tracker's page for the addresses it names.

## Related

- [Analytics components](/components/analytics): the six trackers side by side
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice.
- [Navigation](/docs/packages/navigation): enhanced navigation and scroll behavior.
- [Osirion.Blazor.Analytics on NuGet](https://www.nuget.org/packages/Osirion.Blazor.Analytics)
