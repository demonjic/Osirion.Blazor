---
title: "Osirion.Blazor v1.5: Release Notes"
author: "Osirion Team"
date: "2025-01-25"
last_modified: 2026-10-11
description: "Version 1.5.0 (April 2025) added the AddOsirionBlazor fluent registration and CSS framework integration for Bootstrap, Fluent UI, MudBlazor and Radzen."
slug: "osirion-blazor-v1-5-release-notes"
categories: [Releases]
tags: [Release Notes, Theming, Service Registration]
featured_image: 'https://media.getosirion.com/og/blog-osirion-blazor-v1-5-release-notes.jpg'
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-osirion-blazor-v1-5-release-notes.jpg'
  title: "Osirion.Blazor v1.5: Release Notes"
  description: "What version 1.5.0 added and changed: fluent service registration, CSS framework integration and a renamed stylesheet."
  type: "Article"
---

# Osirion.Blazor v1.5: release notes

Version 1.5.0 was released on 21 April 2025. This post is kept for history; the current release is 4.0.0, described
in [Osirion.Blazor 4.0: security hardening and Fluent UI v5](/blog/osirion-blazor-4-0-release).

## Added

- A fluent API for service registration through the `AddOsirionBlazor` method, with the `IOsirionBlazorBuilder`
  interface.
- CSS framework integration for Bootstrap, Fluent UI, MudBlazor and Radzen: the `CssFramework` enum, classes that map
  Osirion's CSS variables to each framework's variables, and a `FrameworkIntegration` parameter on `OsirionStyles`.
- `OsirionStyleOptions`, a class of its own for styling options.

## Changed

- `osirion-cms.css` was renamed to `osirion.css`, because it styles every component, not only the CMS ones.
- Styling options moved from `GitHubCmsOptions` to `OsirionStyleOptions`.
- The service collection extensions were reorganized. The earlier methods kept working.

## Upgrading to 1.5

Update the stylesheet link to `osirion.css` and move styling settings from `GitHubCmsOptions` to
`OsirionStyleOptions`. The [1.5 section of the migration guide](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/MIGRATION.md#upgrading-to-v150)
has the details.

These names belong to 1.5. Later versions changed the registration API (`AddOsirion`) and the theming options, so
follow the current documentation for new projects.
