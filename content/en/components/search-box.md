---
id: 'cms-web-search-box'
order: 6
title: SearchBox
permalink: /components/search-box
description: "SearchBox renders a search form that submits the query with GET to a results page, with no script, under static SSR and interactive modes."
author: Dejan Demonjić
date: "2024-12-29"
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-search-box.jpg'
categories:
- Components
- CMS Web Components
- Navigation Components
tags:
- cms
- search
- forms
- accessibility
published: true
slug: search-box
lang: en
redirect_from:
- /docs/components/cms-web/navigation/search-box
custom_fields:
  group: cms-navigation
seo_properties:
  title: 'SearchBox: a search form for Blazor'
  description: 'A search form for Blazor content sites that submits with GET to a results page built with ContentList. No script needed.'
  image: 'https://media.getosirion.com/og/components-search-box.jpg'
  canonical: 'https://getosirion.com/components/search-box'
  lang: en
  robots: index, follow
  og_title: 'SearchBox - Osirion.Blazor'
  og_description: 'A search field and button in a plain GET form that sends the query to your results page. No script needed.'
  og_image_url: 'https://media.getosirion.com/og/components-search-box.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'SearchBox - Osirion.Blazor'
  twitter_description: 'A search field and button in a plain GET form that sends the query to your results page. No script needed.'
  twitter_image_url: 'https://media.getosirion.com/og/components-search-box.jpg'
  meta_tags: {}
---

# SearchBox

`SearchBox` renders a search field and a submit button inside a plain HTML form. Submitting it sends the browser to
your results page with the query in the URL, for example `/search?q=routing`, so it works without script and in every
render mode.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/Navigation/SearchBox.razor).

```demo
search-box
```

Submitting the demo opens this site's `/search` page with the query in the address.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<SearchBox />
```

The results page reads the query and passes it to `ContentList`, which filters the content. Passing the query back
to `SearchBox` keeps it in the field:

```razor
@page "/search"
@using Osirion.Blazor.Cms.Web.Components

<SearchBox Title="Search the docs" SearchQuery="@(Query ?? string.Empty)" />

@if (!string.IsNullOrWhiteSpace(Query))
{
    <ContentList SearchQuery="@Query"
                 ShowPagination="true"
                 ItemsPerPage="12"
                 NoContentText="No results found." />
}

@code {
    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }
}
```

The example apps place a `SearchBox` in their navigation menu (`<SearchBox Class="p-3" />` in the Bootstrap app) and
have a `/search` page that passes the `q` query to `ContentList`.

## Parameters

<!-- osirion:parameters SearchBox -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ActionUrl` | `string` | `"/search"` | The page the form submits to with GET. Defaults to `/search`. |
| `QueryParameterName` | `string` | `"q"` | The name of the query string parameter that carries the search text, which is also the name of the input. Defaults to `q`. |
| `SearchQuery` | `string` | `""` | The text shown in the field, usually the query of the results page, so it stays in the field after a search. |
| `Placeholder` | `string` | `"Search content..."` | The placeholder of the field. It is a hint only: the field's accessible name is `Label`. |
| `SearchButtonText` | `string` | `"Search"` | The visible text of the submit button. The button's accessible name stays `Submit search`. |
| `Title` | `string?` | `null` | The heading above the form, rendered as an `h2`. No heading when `null`. |
| `Label` | `string` | `"Search"` | The text of the field's visually hidden label, which is its accessible name. Set it for another language. |
| `Id` | `string?` | `null` | The `id` of the input, which the label points to. Without it, the id is derived from `ActionUrl` and `QueryParameterName`, so it is the same on every render and in every process. Set it when one page holds two boxes with the same settings. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class` goes on the outer `div`. `Style` and `Theme` are accepted but not written, and other attributes on the tag
are captured but not rendered.

## Behavior

- The form uses `method="get"` and `role="search"`. Nothing is posted and no antiforgery token is needed.
- `SearchBox` does not search. `ContentList` with a `SearchQuery` splits the text on spaces and keeps the published
  items where any word appears, ignoring case, in the title, description, rendered HTML, categories or tags. It is
  a substring match over the loaded content, not a ranked full-text index.
- The field is named by a visually hidden `label` whose text is `Label`, "Search" by default, and `Placeholder` is
  only a hint. The button is announced as "Submit search"; its visible text is hidden from screen readers. That
  button name is a fixed English text, so on other languages set `Label`, `Placeholder` and `SearchButtonText` and
  expect the button name to stay "Submit search".
- The input's `id` is `Id` when you set it, otherwise `search-input-` and a number derived from `ActionUrl` and
  `QueryParameterName`. It is the same on every render and in every process, so a prerendered and an interactive box
  agree. Two boxes with the same settings on one page need different `Id` values.
- The submitted text reaches your page through the query string. Render it with Razor (`@Query`), which encodes
  it; never write it out as raw markup.

## Styling

| Class | Element |
| --- | --- |
| `osirion-search-container` | Outer `div`, plus your `Class` |
| `osirion-search-title` | The `h2` heading |
| `osirion-search-box` | The `form` |
| `osirion-search-input` | The `input type="search"` |

The button takes the primary button class of the CSS framework configured in the theming options: `btn btn-primary`
for Bootstrap, `mud-button mud-button-filled mud-button-filled-primary` for MudBlazor, `rz-button rz-button-primary`
for Radzen, and `osirion-button osirion-button-primary` for Fluent UI and for no framework. The field uses
`--osirion-background-primary`, `--osirion-border-color`, `--osirion-text-primary`, `--osirion-text-tertiary`
(placeholder) and `--osirion-focus-ring-color`.

## Related

- [Content List](/components/content-list), which runs the search on the results page
- [CMS navigation components](/components/cms-navigation)
