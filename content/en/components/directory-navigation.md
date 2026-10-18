---
id: 'cms-web-directory-navigation'
order: 3
layout: "component"
title: DirectoryNavigation
permalink: /components/directory-navigation
description: "DirectoryNavigation renders a list of CMS directories you pass, with one level of subdirectories, an active directory and click callbacks."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-directory-navigation.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- directories
- sidebar
published: true
slug: directory-navigation
lang: en
redirect_from:
- /docs/components/cms-web/navigation/directory-navigation
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'DirectoryNavigation: a directory list for Blazor'
  description: 'Render a sidebar list of CMS directories with one level of subdirectories, an active directory and your own directory URLs.'
  image: 'https://media.getosirion.com/og/components-directory-navigation.jpg'
  canonical: 'https://getosirion.com/components/directory-navigation'
  lang: en
  robots: index, follow
  og_title: 'DirectoryNavigation - Osirion.Blazor'
  og_description: 'A sidebar list of CMS directories with one level of subdirectories, an active directory and click callbacks.'
  og_image_url: 'https://media.getosirion.com/og/components-directory-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'DirectoryNavigation - Osirion.Blazor'
  twitter_description: 'A sidebar list of CMS directories with one level of subdirectories, an active directory and click callbacks.'
  twitter_image_url: 'https://media.getosirion.com/og/components-directory-navigation.jpg'
  meta_tags: {}
---

# DirectoryNavigation

`DirectoryNavigation` renders a list of links to the CMS directories you give it, and can show one level of
subdirectories under each. It does not load anything itself: you pass the directories, which keeps it usable for
any part of the content tree.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/DirectoryNavigation.razor).

```demo
directory-navigation
```

The demo lists the sections of this site's own content: the folders inside the English locale folder, and the folder
inside Documentation.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<DirectoryNavigation Directories="@directories" />
```

A sidebar that loads the top-level directories of one locale, marks the current one and expands it:

```razor
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Domain.Services
@using Osirion.Blazor.Cms.Web.Components
@inject IContentProviderManager ContentProviderManager

<DirectoryNavigation Title="Sections"
                     Directories="@directories"
                     IsLoading="@(directories is null)"
                     CurrentDirectory="@CurrentPath"
                     ExpandedDirectory="@CurrentPath"
                     ShowItemCount="false" />

@code {
    [Parameter]
    public string? CurrentPath { get; set; }

    private IReadOnlyList<DirectoryItem>? directories;

    protected override async Task OnInitializedAsync()
    {
        // With a locale, the list holds every directory below the locale folder, at any depth: keep the top level
        var all = await ContentProviderManager.GetDirectoryTreeFromDefaultAsync("en");
        directories = all.Where(directory => directory.Parent?.Parent is null).ToList();
    }
}
```

With a locale, `GetDirectoryTreeFromDefaultAsync` returns every directory of that locale below the locale folder, in
one flat list and at every depth; without a locale it returns the root directories, which are the locale folders when
your content sits in folders such as `en/`. Each directory's subdirectories are in `Children`, so to list the sections,
keep the directories whose parent is the locale folder, as above. `CurrentDirectory` and `ExpandedDirectory` are
compared with `DirectoryItem.Path`, the folder path in the content source.

## Parameters

<!-- osirion:parameters DirectoryNavigation -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Directories` | `IReadOnlyList<DirectoryItem>?` | `null` | The directories to list. Nothing is loaded: when `null` or empty, `NoContentText` is shown. |
| `CurrentDirectory` | `string?` | `null` | The path of the current directory, compared with the directory's `Path`. Its link gets the `osirion-active` class. |
| `ExpandedDirectory` | `string?` | `null` | The path of the top-level directory whose subdirectories are shown, compared with the directory's `Path`. |
| `ExpandAllSubdirectories` | `bool` | `false` | Whether the subdirectories of every top-level directory are shown. |
| `IsLoading` | `bool` | `false` | Whether `LoadingText` is shown instead of the list, for a page that loads the directories itself. |
| `ShowItemCount` | `bool` | `true` | Whether a count is shown after each name. The count is the number of subdirectories, not of content items. |
| `ShowSubdirectories` | `bool` | `true` | Whether subdirectories can be shown at all. One level is rendered, and only for an expanded directory. |
| `LoadingText` | `string` | `"Loading navigation..."` | The text shown while `IsLoading` is `true`. |
| `NoContentText` | `string` | `"No directories available."` | The text shown when `Directories` is `null` or empty. |
| `DirectoryUrlFormatter` | `Func<DirectoryItem, string>?` | `null` | A function that builds each link. Without it, a link goes to the address the directory's `_index.md` declares in `permalink`, otherwise to the directory's path without the locale folder. |
| `Title` | `string?` | `null` | The heading above the list, rendered as an `h3`. No heading when `null`. |
| `DirectoryClicked` | `EventCallback<DirectoryItem>` | - | The callback run with the directory when its link is clicked in an interactive render mode. When it is set, the click no longer navigates. Under static SSR it is not called. |
| `AriaLabel` | `string` | `"Directory navigation"` | The accessible name of the `nav` element. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the `nav`. This component does not apply `Style` or `Theme`, and other attributes on the tag are
captured but not rendered. Wrap it in your own element to style it.

## Behavior

- Only one level of subdirectories is rendered, even when they have children of their own, and only under a top-level
  directory that is expanded (`ExpandedDirectory`, or `ExpandAllSubdirectories`).
- A link goes to the `permalink` from the directory's `_index.md` when it has one. Otherwise it goes to the folder
  path without the locale folder, so `en/blog` links to `/blog`. A directory's name is its folder name, or the
  `title` from its `_index.md`.
- Under static SSR the links are plain links and `DirectoryClicked` is not called. In an interactive render mode
  with `DirectoryClicked` set, the click runs your callback instead of navigating, so you decide what happens.
- The list is wrapped in a `nav` named by `AriaLabel`, `"Directory navigation"` by default. Directory names are
  written as text, so they are HTML-encoded.

## Styling

| Class | Element |
| --- | --- |
| `osirion-directory-navigation` | The `nav`, plus your `Class` |
| `osirion-directory-title` | The `h3` heading |
| `osirion-directory-list` | The top-level `ul` |
| `osirion-subdirectory-list` | The `ul` of subdirectories |
| `osirion-directory-item` | Each `li` |
| `osirion-directory-link` | Each link; `osirion-active` is added to the current one |
| `osirion-directory-count` | The count inside a link |
| `osirion-loading`, `osirion-no-directories` | Loading and empty states |

Colors come from `--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-link-color`,
`--osirion-action-primary`, `--osirion-background-primary` and `--osirion-background-secondary`.

## Related

- [LocalizedNavigation](/components/localized-navigation), which loads the tree itself and lists content items
- [ContentBreadcrumbs](/components/content-breadcrumbs)
- [CMS navigation components](/components/cms-navigation)
