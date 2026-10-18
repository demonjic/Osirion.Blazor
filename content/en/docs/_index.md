---
id: 'docs-overview'
order: 1
layout: docs
title: "Documentation"
permalink: /docs
description: "Documentation for Osirion.Blazor 4.0: a setup guide and the component reference for the Core, CMS, Navigation, Analytics and Theming modules."
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-11
featured_image: 'https://media.getosirion.com/og/docs.jpg'
categories:
- Documentation
- Blazor
- Components
tags:
- blazor
- documentation
- components
- getting-started
is_featured: true
published: true
slug: docs
lang: en
custom_fields:
  group: getting-started
seo_properties:
  title: 'Osirion.Blazor documentation'
  description: 'Set up Osirion.Blazor and look up its components: content, layout, navigation, analytics and theming for SSR-first Blazor sites.'
  image: 'https://media.getosirion.com/og/docs.jpg'
  canonical: 'https://getosirion.com/docs'
  lang: en
  robots: index, follow
  og_title: 'Osirion.Blazor documentation'
  og_description: 'Setup guide and component reference for Osirion.Blazor.'
  og_image_url: 'https://media.getosirion.com/og/docs.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Osirion.Blazor documentation'
  twitter_description: 'Setup guide and component reference for Osirion.Blazor.'
  twitter_image_url: 'https://media.getosirion.com/og/docs.jpg'
  meta_tags: {}
---

# Documentation

Osirion.Blazor is a set of Blazor components for content sites, for .NET 9 and .NET 10. These pages describe
version 4.1: how to set it up and what each component renders. Features marked "Available from 4.0" are new in 4.0
and not in 3.x.

## Start here

- [Getting started](/docs/getting-started): install the package, register it in `Program.cs`, and render Markdown
  from a local folder or a GitHub repository as a blog.
- [Components overview](/components): the modules, their packages and their components on one page.

## Component reference

- [Core components](/docs/packages/core): page layout, hero and other sections, cards, carousels, the contact
  form, cookie consent, HTML rendering, and loading and not-found states.
- [CMS web components](/docs/packages/cms-web): `ContentList`, `ContentView`, `ContentPage` and
  `ContentRenderer`, plus breadcrumbs, a table of contents, search, tags, categories and SEO metadata.
- [Navigation components](/docs/packages/navigation): menus, enhanced navigation and the scroll to top button.
- [Analytics components](/docs/packages/analytics): trackers for Microsoft Clarity, Matomo, Google Analytics 4 and
  Yandex Metrica, with an optional consent gate.
- [Theming components](/docs/packages/theming): `OsirionStyles`, the CSS framework adapters and `ThemeToggle`.

## Elsewhere

- The [blog](/blog) has longer articles about building content sites with the library.
- The source, issues and example sites are on [GitHub](https://github.com/demonjic/Osirion.Blazor).
