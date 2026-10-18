---
id: 'cms-web-categories-list'
order: 1
layout: "component"
title: CategoriesList
permalink: /components/categories-list
description: "CategoriesList renders a link to every content category with its item count, loaded from the default CMS content provider."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-categories-list.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- categories
- taxonomy
published: true
slug: categories-list
lang: en
redirect_from:
- /docs/components/cms-web/navigation/categories-list
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'CategoriesList: category links with counts for Blazor'
  description: 'Render links to every CMS category with item counts, sorting, a limit, an active category and your own URL scheme.'
  image: 'https://media.getosirion.com/og/components-categories-list.jpg'
  canonical: 'https://getosirion.com/components/categories-list'
  lang: en
  robots: index, follow
  og_title: 'CategoriesList - Osirion.Blazor'
  og_description: 'Links to every content category with its item count, loaded from the default content provider.'
  og_image_url: 'https://media.getosirion.com/og/components-categories-list.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'CategoriesList - Osirion.Blazor'
  twitter_description: 'Links to every content category with its item count, loaded from the default content provider.'
  twitter_image_url: 'https://media.getosirion.com/og/components-categories-list.jpg'
  meta_tags: {}
---

# CategoriesList

`CategoriesList` renders a list of links, one per content category, with the number of items in each. It loads the
categories itself from the default content provider, so use it in a sidebar, on a home page or on a category page.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/CategoriesList.razor).

```demo
categories-list
```

The demo reads the categories of this site's own content and marks `Components` as the active one.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<CategoriesList Title="Categories" />
```

A category page that lists the items of one category and marks it in the list:

```razor
@page "/category/{Category}"
@using Osirion.Blazor.Cms.Web.Components

<ContentList Category="@Category" />

<aside>
    <CategoriesList Title="Categories"
                    ActiveCategory="@Category"
                    SortByCount="false" />
</aside>

@code {
    [Parameter]
    public string Category { get; set; } = string.Empty;
}
```

The default links go to `/category/{slug}`, which matches this route. `ContentList` passes the value to the content
query, which matches a category by its name or by its slug, ignoring case, so the default links work for any category
name. With your own `CategoryUrlFormatter`, read the value your own way and pass the name or the slug.

## Parameters

<!-- osirion:parameters CategoriesList -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | `null` | The heading above the list, rendered as an `h3`. No heading when `null`. |
| `LoadingText` | `string` | `"Loading categories..."` | The text shown while the categories load. Only an interactive render mode can show it: under static SSR the response is sent with the finished list. |
| `NoContentText` | `string` | `"No categories available."` | The text shown when there are no categories to list, when no default content provider is registered or when loading failed. |
| `CategoryUrlFormatter` | `Func<ContentCategory, string>?` | `null` | A function that builds each link from its category (`Name`, `Slug` and `Count`). Without it, links go to `/category/{Slug}`. |
| `ActiveCategory` | `string?` | `null` | The slug or name of the current category, compared ignoring case. Its link gets the `osirion-active` class; `aria-current` is not written. |
| `ShowCount` | `bool` | `true` | Whether each category's item count is shown after its name. |
| `SortByCount` | `bool` | `true` | How categories are ordered: `true` by item count, highest first; `false` by name. |
| `MaxCategories` | `int?` | `null` | The number of categories to show, taken from the start after sorting. All categories when `null`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the outer `div`. `Style` and `Theme` are accepted but not written, and other attributes on the tag
are captured but not rendered, so use `Class` rather than `id` or `data-` attributes.

## Behavior

- Categories come from the `categories` front matter of every item the default provider holds, across all
  locales and including unpublished items. Names that differ only in case count as one category, shown with the
  spelling of the first item that has it; the count is the number of items that carry it.
- The list is loaded again on every parameter change. Caching belongs to the content provider, not the component.
- Under static SSR the response is sent with the finished list. `LoadingText` can show briefly in interactive
  render modes.
- With no default provider registered, or when the provider throws, `NoContentText` is shown. A provider error is
  logged through `ILogger`; its details never reach the page.
- Links carry `rel="nofollow"`. Category names are written as text, so they are HTML-encoded.
- The current category is marked with a class only, not with `aria-current`.

## Styling

| Class | Element |
| --- | --- |
| `osirion-categories-list-container` | Outer `div`, plus your `Class` |
| `osirion-categories-title` | The `h3` heading |
| `osirion-categories-list` | The `ul` |
| `osirion-category-item` | Each `li` |
| `osirion-category-link` | Each link; `osirion-active` is added to the current one |
| `osirion-category-count` | The count inside a link |
| `osirion-loading`, `osirion-no-categories` | Loading and empty states |

Colors come from `--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-background-hover`,
`--osirion-background-tertiary`, `--osirion-nav-active-background`, `--osirion-nav-active-text`,
`--osirion-link-color` and `--osirion-action-primary`.

## Related

- [TagCloud](/components/tag-cloud), the same idea for tags
- [Content List](/components/content-list), for the category page itself
- [CMS navigation components](/components/cms-navigation)
