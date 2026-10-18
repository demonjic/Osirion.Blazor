---
id: 'cms-web-osirion-content-navigation'
order: 5
title: OsirionContentNavigation
permalink: /components/osirion-content-navigation
description: "OsirionContentNavigation renders previous and next links between CMS items of the same directory, found by front matter order."
author: Dejan Demonjić
date: "2024-12-29"
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-osirion-content-navigation.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- previous-next
- pagination
published: true
slug: osirion-content-navigation
lang: en
redirect_from:
- /docs/components/cms-web/navigation/osirion-content-navigation
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'OsirionContentNavigation: previous and next for Blazor'
  description: 'Render previous and next links between CMS items of a directory, with descriptions, icons, three layouts and your own URLs.'
  image: 'https://media.getosirion.com/og/components-osirion-content-navigation.jpg'
  canonical: 'https://getosirion.com/components/osirion-content-navigation'
  lang: en
  robots: index, follow
  og_title: 'OsirionContentNavigation - Osirion.Blazor'
  og_description: 'Previous and next links between CMS items of the same directory, found by front matter order.'
  og_image_url: 'https://media.getosirion.com/og/components-osirion-content-navigation.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContentNavigation - Osirion.Blazor'
  twitter_description: 'Previous and next links between CMS items of the same directory, found by front matter order.'
  twitter_image_url: 'https://media.getosirion.com/og/components-osirion-content-navigation.jpg'
  meta_tags: {}
---

# OsirionContentNavigation

`OsirionContentNavigation` renders a pair of links to the previous and next item, for example at the end of an
article or a documentation page. Give it the current item and it finds the neighbours in the same directory, or
pass the neighbours yourself.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/OsirionContentNavigation.razor).

```demo
osirion-content-navigation
```

The demo takes the second blog post of this site's own content, so it has a neighbour on each side, and shows the card
layout with descriptions.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<OsirionContentNavigation Item="@Item" />
```

The built-in `ArticlePage` uses `<OsirionContentNavigation Item="Item" ShowDescription ShowPlaceholder />`. A
documentation page with card links, root-relative URLs and its own labels:

```razor
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Web.Components

<OsirionContentNavigation Item="@Item"
                          Variant="ContentNavigationVariant.Card"
                          ShowDescription="true"
                          PreviousLabel="Previous page"
                          NextLabel="Next page"
                          AriaLabel="Documentation pages"
                          UrlGenerator="@(item => $"/{item.Url.TrimStart('/')}")" />

@code {
    [Parameter]
    public ContentItem? Item { get; set; }
}
```

## Parameters

<!-- osirion:parameters OsirionContentNavigation -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Item` | `ContentItem?` | `null` | The current item. Required: nothing renders without it, even when both neighbours are passed. A missing neighbour is looked up from it. |
| `PreviousItem` | `ContentItem?` | `null` | The previous item. When `null`, it is looked up from `Item`. |
| `NextItem` | `ContentItem?` | `null` | The next item. When `null`, it is looked up from `Item`. |
| `PreviousLabel` | `string` | `"Previous"` | The label above the previous item's title. |
| `NextLabel` | `string` | `"Next"` | The label above the next item's title. |
| `AriaLabel` | `string` | `"Content navigation"` | The accessible name of the `nav` element. |
| `ShowIcons` | `bool` | `true` | Whether the arrow icons are shown. |
| `PreviousIcon` | `RenderFragment?` | `null` | The icon of the previous link. A left chevron when not set and `ShowIcons` is `true`. |
| `NextIcon` | `RenderFragment?` | `null` | The icon of the next link. A right chevron when not set and `ShowIcons` is `true`. |
| `ShowDescription` | `bool` | `false` | Whether each neighbour's description is shown under its title. |
| `ShowPlaceholder` | `bool` | `false` | Whether an empty placeholder is rendered where a neighbour is missing, so the other link keeps its side. |
| `ShowNavigation` | `bool` | `true` | Whether the navigation renders. When `false`, nothing renders and the neighbours are not looked up. |
| `PreventDefault` | `bool` | `false` | Whether the links are stopped from navigating in an interactive render mode. Leave it `false`: the click callbacks are not raised, so a prevented link does nothing. |
| `Variant` | `ContentNavigationVariant` | `ContentNavigationVariant.Default` | The layout: `Default`, `Compact` or `Card`. It is written as a class on the `nav`. |
| `UrlGenerator` | `Func<ContentItem, string>?` | `null` | A function that builds each link. Without it, the link is the item's `Url`, a path without a leading slash that resolves against the page's address. |
| `TitleExtractor` | `Func<ContentItem, string>?` | `null` | A function that returns the text shown as each title. Without it, the item's `Title`, or `Untitled`. |
| `DescriptionExtractor` | `Func<ContentItem, string>?` | `null` | A function that returns the text shown as each description. Without it, the item's `Description`. |
| `OnPreviousClick` | `EventCallback<ContentItem>` | - | A callback for a click on the previous link. Declared but never raised in 4.0. |
| `OnNextClick` | `EventCallback<ContentItem>` | - | A callback for a click on the next link. Declared but never raised in 4.0. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`ContentNavigationVariant` has `Default`, `Compact` and `Card`. `Class` goes on the `nav`, and other attributes on the
tag are added to it, `Theme` included. `Style` is accepted but not written.

## Behavior

- When a neighbour is missing, the component queries the published items with the same locale whose directory has
  the same name as `Item.Directory`, sorted by the `order` front matter ascending. The items right before and after
  `Item` (matched by path) become the neighbours. An item without a directory is compared with all items of its
  locale. Give the items of a directory distinct `order` values so the sequence is predictable.
- Looked-up neighbours are kept inside the component, not in `PreviousItem` and `NextItem`. When `Item` changes the
  lookup runs again, and a neighbour you stop passing falls back to the lookup.
- The `nav` renders only when `Item` is set and at least one neighbour exists. With one neighbour it gets the
  `osirion-content-navigation-single` class.
- If the lookup fails, the error is logged through `ILogger` and only the neighbours you passed are shown.
- An item's `Url` is a path without a leading slash, so the default link is relative to the page's base address.
  Set `UrlGenerator`, as the example does, to make it root-relative.
- Titles and descriptions are written as text, so they are HTML-encoded. The links are plain anchors and need no
  script.

## Styling

| Class | Element |
| --- | --- |
| `osirion-content-navigation` | The `nav`, plus your `Class` |
| `osirion-content-navigation-default`, `-compact`, `-card` | The variant, on the `nav` |
| `osirion-content-navigation-single` | Added when only one neighbour is shown |
| `osirion-content-previous`, `osirion-content-next` | The two links |
| `osirion-content-navigation-label` | The label and icon |
| `osirion-content-navigation-icon` | The icon wrapper |
| `osirion-content-navigation-title` | The neighbour's title |
| `osirion-content-navigation-description` | The neighbour's description |
| `osirion-content-navigation-placeholder` | The empty placeholder |

The links use `--osirion-border-color`, `--osirion-background-primary`, `--osirion-text-primary` and
`--osirion-action-primary`, with `--osirion-focus-ring-color` for keyboard focus. The card variant uses
`--osirion-card-background`, `--osirion-card-border-color` and `--osirion-card-shadow`.

## Related

- [ContentSeriesNavigation](/components/series-navigation), previous and next links inside a series
- [ContentBreadcrumbs](/components/content-breadcrumbs)
- [CMS navigation components](/components/cms-navigation)
