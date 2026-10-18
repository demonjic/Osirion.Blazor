---
id: 'analytics-components-group'
order: 1
layout: docs
title: Analytics
permalink: /components/analytics
description: The six analytics tracker components in Osirion.Blazor.Analytics (Clarity, GA4, Matomo, Plausible, Umami and Yandex Metrica) and what they share.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-analytics.jpg'
categories:
- Components
- Analytics Components
tags:
- blazor
- analytics
- tracking
- cookie-consent
published: true
slug: analytics
lang: en
custom_fields:
  group: analytics
seo_properties:
  title: 'Blazor analytics tracker components'
  description: 'Tracker components for Clarity, Google Analytics 4, Matomo, Plausible, Umami and Yandex Metrica in Blazor, with an optional consent gate.'
  image: 'https://media.getosirion.com/og/components-analytics.jpg'
  canonical: 'https://getosirion.com/components/analytics'
  lang: en
  robots: index, follow
  og_title: 'Analytics trackers - Osirion.Blazor'
  og_description: 'One tracker component per analytics service, configured from appsettings.json and gated on cookie consent.'
  og_image_url: 'https://media.getosirion.com/og/components-analytics.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Analytics trackers - Osirion.Blazor'
  twitter_description: 'One tracker component per analytics service, configured from appsettings.json and gated on cookie consent.'
  twitter_image_url: 'https://media.getosirion.com/og/components-analytics.jpg'
  meta_tags: {}
---

# Analytics

`Osirion.Blazor.Analytics` has one tracker component per analytics service. Each writes that service's tracking script
into the page from configuration. They render on the server, so they work under Static SSR with no interactive render
mode, and they live in the `Osirion.Blazor.Analytics.Components` namespace.

| Component | Service | Registration | Configuration section |
| --- | --- | --- | --- |
| [ClarityTracker](/components/clarity-tracker) | Microsoft Clarity | `AddClarity` | `Osirion:Analytics:Clarity` |
| [GA4Tracker](/components/ga4-tracker) | Google Analytics 4 | `AddGA4` | `Osirion:Analytics:GA4` |
| [MatomoTracker](/components/matomo-tracker) | Matomo | `AddMatomo` | `Osirion:Analytics:Matomo` |
| [PlausibleTracker](/components/plausible-tracker) | Plausible | `AddPlausible` | `Osirion:Analytics:Plausible` |
| [UmamiTracker](/components/umami-tracker) | Umami | `AddUmami` | `Osirion:Analytics:Umami` |
| [YandexMetricaTracker](/components/yandex-metrica-tracker) | Yandex Metrica | `AddYandexMetrica` | `Osirion:Analytics:YandexMetrica` |

Plausible and Umami are available from 4.1.

Trackers inject third-party scripts, so these pages have no live demos: a demo would load the provider's script on the
documentation page itself. Each page shows the markup its tracker renders instead.

## What they share

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

- **Base class.** All six derive from `AnalyticsTracker`, which adds the `SiteId` and `Enabled` parameters. `Enabled`
  set to `false` turns a tracker off for one render; `SiteId` replaces the configured site ID for the trackers that
  use one (Clarity, Matomo and Umami). A parameter you leave unset keeps the configured value.
- **Nothing renders by accident.** A tracker whose service is not registered, whose `Enabled` is `false`, or whose
  required value is missing or invalid, renders nothing.
- **Consent.** The package has one consent gate, off by default. With it on, no tracker writes its script until the
  visitor accepted the `analytics` category in [`OsirionCookieConsent`](/components/cookie-consent). Setup is on the
  [package page](/docs/packages/analytics).
- **Content Security Policy.** Clarity, GA4, Matomo, Plausible and Yandex Metrica write inline scripts and add no
  nonce; Umami writes only an external script tag. Each page lists the addresses its markup names.

## Related

- [Osirion.Blazor.Analytics](/docs/packages/analytics): install, registration, configuration and the consent gate
- [Cookie consent](/components/cookie-consent): the banner that records the visitor's choice
