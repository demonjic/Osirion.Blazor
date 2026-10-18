---
id: 'cms-web-content-breadcrumbs'
order: 2
layout: "component"
title: ContentBreadcrumbs
permalink: /components/content-breadcrumbs
description: "ContentBreadcrumbs renders a breadcrumb trail from a CMS item's directory chain: home, each parent directory and the item title."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-content-breadcrumbs.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- breadcrumbs
- directories
- accessibility
published: true
slug: content-breadcrumbs
lang: en
redirect_from:
- /docs/components/cms-web/navigation/content-breadcrumbs
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'ContentBreadcrumbs: content tree breadcrumbs for Blazor'
  description: 'Render a breadcrumb trail from a CMS item''s directories, with a home link, a current item and your own directory URLs.'
  image: 'https://media.getosirion.com/og/components-content-breadcrumbs.jpg'
  canonical: 'https://getosirion.com/components/content-breadcrumbs'
  lang: en
  robots: index, follow
  og_title: 'ContentBreadcrumbs - Osirion.Blazor'
  og_description: 'A breadcrumb trail from a CMS item''s directories: home, each parent directory and the item title.'
  og_image_url: 'https://media.getosirion.com/og/components-content-breadcrumbs.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ContentBreadcrumbs - Osirion.Blazor'
  twitter_description: 'A breadcrumb trail from a CMS item''s directories: home, each parent directory and the item title.'
  twitter_image_url: 'https://media.getosirion.com/og/components-content-breadcrumbs.jpg'
  meta_tags: {}
---

# ContentBreadcrumbs

`ContentBreadcrumbs` renders a breadcrumb trail for a CMS item: a home link, a link for each directory from the
root down to the item's directory, and the item's title. Use it on article and documentation pages, where the
trail should follow the content tree rather than the URL.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/ContentBreadcrumbs.razor).

```demo
content-breadcrumbs
```

The demo takes a blog post from this site's own content and shows its trail: Home, Blog and the post's title.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<ContentBreadcrumbs Content="@Item" />
```

The component walks up from `Item.Directory` through each directory's `Parent`. A documentation page that sets
its own home link and directory URLs:

```razor
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Web.Components

<ContentBreadcrumbs Content="@Item"
                    Directory="@Item?.Directory"
                    HomeText="Docs"
                    HomeUrl="/docs"
                    DirectoryUrlFormatter="@(directory => $"/{directory.Url.TrimStart('/')}")" />

@code {
    [Parameter]
    public ContentItem? Item { get; set; }
}
```

A directory's `Url` is the `permalink` from its `_index.md`, and is empty when there is none. Without a formatter,
a link goes to that permalink, or, when the folder declares none, to its folder path without the locale folder
(`blog` for `en/blog`). The locale folder itself (`en`) has no page and is left out of the trail.

## Parameters

<!-- osirion:parameters ContentBreadcrumbs -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Content` | `ContentItem?` | `null` | The item being shown. Its title is the last crumb, and its `Directory` is the deepest directory of the trail when `Directory` is not set (read once, when the component is first initialized). |
| `Directory` | `DirectoryItem?` | `null` | The deepest directory of the trail. The crumbs are its parents up to the root, without a locale folder such as `en`. |
| `ShowHome` | `bool` | `true` | Whether the home link is rendered first. |
| `HomeText` | `string` | `"Home"` | The text of the home link. |
| `HomeUrl` | `string` | `"/"` | The target of the home link. Defaults to `/`. |
| `HideCurrentItem` | `bool` | `false` | Whether the item's title, the last crumb, is left out, for example when the page heading already shows it. |
| `DirectoryUrlFormatter` | `Func<DirectoryItem, string>?` | `null` | A function that builds each directory link. Without it, a link goes to the address the directory's `_index.md` declares in `permalink`, otherwise to the directory's path without the locale folder. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the `nav`. `Style` and `Theme` are accepted but not written, and other attributes on the tag are
captured but not rendered.

## Behavior

- The component takes `Content.Directory` only once, when it is first initialized. If the item can change while
  the component stays on the page (an interactive page that loads another item), pass `Directory` as well, as the
  built-in `DocumentPage` does.
- A directory's name is its folder name, or the `title` from its `_index.md`.
- It renders `<nav aria-label="breadcrumb">` around an ordered list. The current item is plain text in a `span`,
  not a link. It has no `aria-current` attribute.
- The `/` separators are drawn with CSS (`::after`), not written into the markup. The directory links are written in
  the component's markup, so its scoped stylesheet reaches them like the home and current crumbs.
- Titles and directory names are written as text, so they are HTML-encoded. No script is needed.

## Styling

| Class | Element |
| --- | --- |
| `osirion-breadcrumbs` | The `nav`, plus your `Class` |
| `osirion-breadcrumbs-list` | The `ol` |
| `osirion-breadcrumbs-item` | Each `li`; the current item's `li` also has `osirion-breadcrumbs-current` |
| `osirion-breadcrumbs-link` | The home and directory links |
| `osirion-breadcrumbs-text` | The current item's title |

Links use `--osirion-text-secondary` and turn `--osirion-action-primary` on hover; separators use
`--osirion-text-tertiary`.

## Related

- [OsirionBreadcrumbs](/components/breadcrumbs), breadcrumbs built from a URL path
- [DirectoryNavigation](/components/directory-navigation)
- [CMS navigation components](/components/cms-navigation)
