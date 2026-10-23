---
id: 'components-overview'
order: 1
layout: docs
title: Packages
permalink: /docs/packages
description: Every Osirion.Blazor component by module and package (Core, CMS web, CMS admin, Navigation, Analytics, Theming) and what they share.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/docs-packages.jpg'
categories:
- Components
- Documentation
- Blazor
tags:
- blazor
- components
- core
- cms
- navigation
- analytics
- theming
is_featured: true
published: true
slug: packages
lang: en
custom_fields:
  group: packages
seo_properties:
  title: 'Osirion.Blazor packages: modules and their components'
  description: 'The NuGet packages Osirion.Blazor is split into, the components each one holds, and the parameters and behavior they share.'
  image: 'https://media.getosirion.com/og/docs-packages.jpg'
  canonical: 'https://getosirion.com/docs/packages'
  lang: en
  robots: index, follow
  og_title: 'Osirion.Blazor packages: modules and their components'
  og_description: 'The NuGet packages Osirion.Blazor is split into and the components each one holds.'
  og_image_url: 'https://media.getosirion.com/og/docs-packages.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Osirion.Blazor packages: modules and their components'
  twitter_description: 'The NuGet packages Osirion.Blazor is split into and the components each one holds.'
  twitter_image_url: 'https://media.getosirion.com/og/docs-packages.jpg'
  meta_tags: {}
---

# Packages

Osirion.Blazor is split into modules, and each module is its own NuGet package. The `Osirion.Blazor` package
references all of them and adds the `AddOsirion` registration; see [Getting started](/docs/getting-started) for the
setup. This page lists the components by module and links to their reference pages.

## What every component shares

- **Server-rendered markup.** Components render their HTML on the server, so pages work under static SSR. A few
  (for example the carousels, `OsirionImageGallery` and `OsirionPageLayout`) load a small script of their own for
  behavior that needs the browser. `AddOsirion` registers the script loader they use; with single modules, see the
  [installation guide](/docs/installation).
- **Common parameters.** Components inherit `OsirionComponentBase`, which adds `Class` and `Style` for the root
  element, `Attributes` (any unmatched attribute is passed through) and `Theme` (a `ThemeMode`: `Light`, `Dark`,
  `System` or `Inverted`, default `System`). The old `SetInteractive` parameter is obsolete and setting it is a
  compile error; interactivity is detected automatically. A misspelled parameter is passed through as an attribute
  too, so in Development a warning names the parameter an unknown attribute resembles.
- **Sanitized content.** Markdown is rendered to HTML and cleaned with an allow-list before it is shown, so script,
  event handler attributes and `javascript:` URLs in content are removed.

Components new in 4.0 are marked below.

## Core

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. Building blocks for any page.

- [Layout](/components/layout): `OsirionPageLayout`, `OsirionFooter`, `OsirionStickySidebar`,
  `OsirionBackgroundPattern`.
- [Sections](/components/sections): `HeroSection`, `OsirionBaseSection`, `OsirionContactInfoSection`,
  `OsirionResponsiveShowcaseSection`, `OsirionContextCardGrid`, `OsirionMetricGrid`. From 4.0:
  `OsirionPricingTable`, `OsirionFeatureComparison`, `OsirionSteps`. From 4.3: `OsirionCallToActionSection`.
- [Cards](/components/cards): `OsirionFeatureCard`, `OsirionTestimonialCard`, `OsirionSubscriptionCard`,
  `OsirionMetricCard`. From 4.0: `OsirionAuthorCard`.
- [Carousels](/components/sections): `InfiniteLogoCarousel`, `OsirionTestimonialCarousel`.
- [Content](/components/content): `OsirionArticleMetdata`. From 4.0: `OsirionCallout`, `OsirionTabs`,
  `OsirionAccordion`, `OsirionFaq`, `OsirionResponsiveImage`, `OsirionVideoEmbed`, `OsirionReadingProgress`.
- [Forms](/components/forms-and-feedback): `OsirionContactForm`.
- [Navigation](/components/navigation): `OsirionBreadcrumbs`, `OsirionReadMoreLink`.
- [Popups](/components/forms-and-feedback): `OsirionCookieConsent`. From 4.0: `OsirionAnnouncementBar`.
- [Rendering](/components/content): `OsirionHtmlRenderer`, `OsirionReveal`.
- [States](/components/forms-and-feedback): `OsirionPageLoading`, `OsirionContentNotFound`.
- `OsirionImageGallery`, a gallery with a lightbox.

## CMS web

Package `Osirion.Blazor.Cms.Web`. Components that read content from the configured provider (a local folder or a
GitHub repository).

- [Content](/components/cms-pages): `ContentList`, `ContentView`, `ContentPage`, `ContentRenderer`,
  `LocalizedContentView`, `SeoMetadataRenderer`.
- [Navigation](/components/cms-navigation): `ContentBreadcrumbs`, `CategoriesList`, `TagCloud`,
  `SearchBox`, `TableOfContents`, `DirectoryNavigation`, `LocalizedNavigation`, `OsirionContentNavigation`. From
  4.0: `ContentSeriesNavigation`.
- Page-level components that combine the above: `ArticlePage`, `DocumentPage`, `LandingPage` and `HomePage`.

## CMS admin

Package `Osirion.Blazor.Cms.Admin`. Admin pages under `/osirion` for browsing and editing content in a GitHub
repository, with a Markdown editor and preview. Every admin page requires the `Osirion.Cms.Admin` authorization
policy, so the host must add its own sign-in first. The `Osirion.Blazor.Cms` package combines CMS web and CMS admin.

## Navigation

Package `Osirion.Blazor.Navigation`, namespace `Osirion.Blazor.Navigation.Components`.
[Navigation components](/docs/packages/navigation): `Menu` with `MenuItem`, `MenuGroup`, `MenuDivider`,
`MenuCallToAction`, `MenuPanel`, `MenuPanelLink` and `MenuImageCard`; `EnhancedNavigation`, which handles scroll
position after Blazor's enhanced navigation; `ScrollToTop` and `ScrollToTopProvider`.

## Analytics

Package `Osirion.Blazor.Analytics`, namespace `Osirion.Blazor.Analytics.Components`.
[Analytics components](/docs/packages/analytics): `ClarityTracker`, `MatomoTracker`, `GA4Tracker` and
`YandexMetricaTracker`, configured under `Osirion:Analytics`. From 4.0: `PlausibleTracker` and `UmamiTracker`.

## Theming

Package `Osirion.Blazor.Theming`, namespace `Osirion.Blazor.Theming.Components`.
[Theming components](/docs/packages/theming): [OsirionStyles](/components/osirion-styles) loads the
stylesheets and the adapter for Bootstrap, Fluent UI, MudBlazor or Radzen;
[ThemeToggle](/components/theme-toggle) is a button that switches between the light and dark theme.
