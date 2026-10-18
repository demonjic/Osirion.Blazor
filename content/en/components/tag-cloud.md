---
id: 'cms-web-tag-cloud'
order: 9
title: TagCloud
permalink: /components/tag-cloud
description: "TagCloud renders a link to every content tag with its item count, loaded from the default CMS provider or from a list you pass."
author: Dejan Demonjić
date: "2024-12-29"
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-tag-cloud.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- navigation
- tags
- taxonomy
published: true
slug: tag-cloud
lang: en
redirect_from:
- /docs/components/cms-web/navigation/tag-cloud
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'TagCloud: tag links with counts for Blazor'
  description: 'Render links to every CMS tag with item counts, sorting, a limit, an active tag and your own URL scheme.'
  image: 'https://media.getosirion.com/og/components-tag-cloud.jpg'
  canonical: 'https://getosirion.com/components/tag-cloud'
  lang: en
  robots: index, follow
  og_title: 'TagCloud - Osirion.Blazor'
  og_description: 'A wrapping row of tag links with item counts, loaded from the default content provider or passed in.'
  og_image_url: 'https://media.getosirion.com/og/components-tag-cloud.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'TagCloud - Osirion.Blazor'
  twitter_description: 'A wrapping row of tag links with item counts, loaded from the default content provider or passed in.'
  twitter_image_url: 'https://media.getosirion.com/og/components-tag-cloud.jpg'
  meta_tags: {}
---

# TagCloud

`TagCloud` renders a wrapping row of tag links, each with the number of items that carry the tag. It loads the
tags from the default content provider, or shows the tags you pass. Every tag is drawn at the same size; the
component does not scale tags by count.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/TagCloud.razor).

```demo
tag-cloud
```

The demo shows the twelve most used tags of this site's own content.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<TagCloud Title="Tags" MaxTags="20" />
```

A tag page that keeps the tag name in the query string:

```razor
@page "/tags"
@using Osirion.Blazor.Cms.Web.Components

<TagCloud Title="Tags"
          ActiveTag="@Tag"
          TagUrlFormatter="@(tag => $"/tags?tag={Uri.EscapeDataString(tag.Name)}")" />

@if (!string.IsNullOrWhiteSpace(Tag))
{
    <ContentList Tag="@Tag" />
}

@code {
    [SupplyParameterFromQuery(Name = "tag")]
    public string? Tag { get; set; }
}
```

`ContentList` passes `Tag` to the content query, which matches a tag by its name or by its slug, ignoring case. The
default `/tag/{slug}` links therefore work with a route that takes the slug, and a formatter like the one above works
with the name.

## Parameters

<!-- osirion:parameters TagCloud -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `MaxTags` | `int?` | `null` | The number of loaded tags to show, taken from the start after sorting. All tags when `null`. Tags passed in `Tags` are shown as given. |
| `Title` | `string?` | `null` | The heading above the tags, rendered as an `h3`. No heading when `null`. |
| `LoadingText` | `string` | `"Loading tags..."` | The text shown while the tags load. Only an interactive render mode can show it: under static SSR the response is sent with the finished list. |
| `NoContentText` | `string` | `"No tags available."` | The text shown when there are no tags to show, when no default content provider is registered or when loading failed. |
| `ActiveTag` | `string?` | `null` | The slug or name of the current tag, compared ignoring case. Its link gets the `osirion-tag-active` class; `aria-current` is not written. |
| `TagUrlFormatter` | `Func<ContentTag, string>?` | `null` | A function that builds each link from its tag (`Name`, `Slug` and `Count`). Without it, links go to `/tag/{Slug}`. |
| `ShowCount` | `bool` | `true` | Whether each tag's item count is shown after its name. |
| `SortByCount` | `bool` | `true` | How loaded tags are ordered: `true` by item count, highest first; `false` by name. Tags passed in `Tags` keep their order. |
| `Tags` | `IReadOnlyList<ContentTag>?` | `null` | The tags to show. When `null` or empty, the component loads the tags of every item from the default content provider. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the outer `div`. `Style` and `Theme` are accepted but not written, and other attributes on the tag
are captured but not rendered.

## Behavior

- Loaded tags come from the `tags` front matter of every item the default provider holds, across all locales and
  including unpublished items. Names that differ only in case count as one tag.
- `MaxTags` and `SortByCount` apply only to tags the component loads; tags you pass in `Tags` are shown as given.
- The loaded tags are kept inside the component, not in `Tags`, so changing `MaxTags` or `SortByCount` later shows the
  new limit or order, and tags you pass in `Tags` win over loaded ones for as long as the list is not empty.
- Under static SSR the response is sent with the finished list, and the loading text is never rendered.
- With no default provider registered, or when the provider throws, `NoContentText` is shown. A provider error is
  logged through `ILogger`; its details never reach the page.
- Links carry `rel="nofollow"`. Tag names are written as text, so they are HTML-encoded.
- The current tag is marked with a class only, not with `aria-current`.

## Styling

| Class | Element |
| --- | --- |
| `osirion-tag-cloud` | Outer `div` (it also carries `osirion-tags-container`), plus your `Class` |
| `osirion-tag-cloud-title` | The `h3` heading |
| `osirion-tags-container` | The `div` around the links |
| `osirion-tag-link` | Each link; `osirion-tag-active` is added to the current one |
| `osirion-tag-count` | The count inside a link |
| `osirion-loading`, `osirion-no-tags` | Loading and empty states |

Tag colors use `--osirion-tag-background-color`, `--osirion-tag-text-color` and
`--osirion-tag-hover-background-color`, which fall back to the theme's background and text tokens. The active tag
uses `--osirion-action-primary` and `--osirion-action-primary-text`.

## Related

- [CategoriesList](/components/categories-list), the same idea for categories
- [Content List](/components/content-list), for the tag page itself
- [CMS navigation components](/components/cms-navigation)
