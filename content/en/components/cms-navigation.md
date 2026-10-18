---
id: 'cms-web-navigation-components-overview'
order: 1
layout: docs
title: CMS navigation
permalink: /components/cms-navigation
description: 'Components that link CMS content: categories, tags, breadcrumbs, directories, previous and next links, series, table of contents and search.'
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-cms-navigation.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- blazor
- cms
- navigation
- categories
- tags
- search
- table-of-contents
is_featured: true
published: true
slug: cms-navigation
lang: en
redirect_from:
- /docs/components/cms-web/navigation
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'Blazor CMS navigation components'
  description: 'Blazor components that link CMS content: categories, tags, breadcrumbs, directories, previous and next links, series, table of contents and search.'
  image: 'https://media.getosirion.com/og/components-cms-navigation.jpg'
  canonical: 'https://getosirion.com/components/cms-navigation'
  lang: en
  robots: index, follow
  og_title: 'CMS navigation - Osirion.Blazor'
  og_description: 'Blazor components that link CMS content: categories, tags, breadcrumbs, directories, series, table of contents and search.'
  og_image_url: 'https://media.getosirion.com/og/components-cms-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'CMS navigation - Osirion.Blazor'
  twitter_description: 'Blazor components that link CMS content: categories, tags, breadcrumbs, directories, series, table of contents and search.'
  twitter_image_url: 'https://media.getosirion.com/og/components-cms-navigation.jpg'
  meta_tags: {}
---

# CMS navigation

These components help readers move through content served by the Osirion CMS: category and tag links,
breadcrumbs, directory lists, previous and next links, series links, a table of contents and a search form. They
ship in the Osirion.Blazor.Cms.Web package, in the `Osirion.Blazor.Cms.Web.Components` namespace.

```razor
@using Osirion.Blazor.Cms.Web.Components
```

## Components

| Component | What it renders | Where its data comes from |
| --- | --- | --- |
| [CategoriesList](/components/categories-list) | Links to every category, with item counts | Loaded from the default content provider |
| [TagCloud](/components/tag-cloud) | Links to every tag, with item counts | Loaded from the default content provider, or the tags you pass |
| [ContentBreadcrumbs](/components/content-breadcrumbs) | Home, the item's directories and the item title | The content item or directory you pass |
| [DirectoryNavigation](/components/directory-navigation) | A list of directories with one level of subdirectories | The directories you pass |
| [LocalizedNavigation](/components/localized-navigation) | A directory tree with links to content, for one locale | Loaded from the default content provider |
| [OsirionContentNavigation](/components/osirion-content-navigation) | Previous and next links between the items of a directory | Loaded around the item you pass, or the neighbours you pass |
| [ContentSeriesNavigation](/components/series-navigation) | "Part 2 of 5", the parts of a series and previous and next links (from 4.0) | Loaded from the items with the same `series` |
| [TableOfContents](/components/table-of-contents) | Links to the headings of an HTML string | The HTML you pass |
| [SearchBox](/components/search-box) | A search form that submits with GET | None; your results page reads the query |

## Before you start

Components that load data use `IContentProviderManager`, so a content provider must be registered. The example
apps register everything, including the CMS from the `Osirion:Cms` configuration section, with one call:

```csharp
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(builder.Configuration);
```

## Rendering

Every component here renders under static SSR. Links are plain anchors and the search box is a plain HTML form,
so none of them needs script (`TableOfContents` marks the section being read with a small script, as an enhancement).
Components that load data do it in `OnParametersSetAsync`; without streaming
rendering, a statically rendered page is sent once the data is there. Click callbacks such as `DirectoryClicked`
run only in an interactive render mode.

## Styling

Each component ships a scoped stylesheet whose colors, spacing and fonts come from the Osirion theme tokens
(`--osirion-text-primary`, `--osirion-action-primary`, `--osirion-background-tertiary` and others), so setting the
tokens or using a framework adapter restyles them with the rest of the site. Scoped rules carry an extra attribute
selector, so an override in your own stylesheet needs one more class or element than the rule it replaces, for
example `.sidebar .osirion-categories-list .osirion-category-link`. The entries of `LocalizedNavigation` are built in
code, so its scoped rules do not reach them; style those from your own stylesheet (see its page).

## Related

- [CMS Web components](/docs/packages/cms-web)
- [Core CMS components](/components/cms-pages), such as `ContentList` for category, tag and search result pages
- [OsirionBreadcrumbs](/components/breadcrumbs), breadcrumbs built from a URL path instead of the content tree
