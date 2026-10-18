---
order: 12
title: NotFoundPage
permalink: /components/not-found-page
description: "NotFoundPage renders a page not found page: a title, a way home, an optional search box and suggested pages, with noindex and status 404."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-not-found-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- not-found-page
- cms
- 404
- seo
- static-ssr
slug: not-found-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'NotFoundPage: a 404 page for Blazor'
  description: 'NotFoundPage renders a page not found page with a way home, a search box and suggested pages, asks search engines not to index it and answers 404.'
  image: 'https://media.getosirion.com/og/components-not-found-page.jpg'
  canonical: 'https://getosirion.com/components/not-found-page'
  lang: en
  robots: index, follow
  og_title: 'NotFoundPage - Osirion.Blazor'
  og_description: 'A page not found page with a way home, a search box and suggested pages, rendered without script and answering 404.'
  og_image_url: 'https://media.getosirion.com/og/components-not-found-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'NotFoundPage - Osirion.Blazor'
  twitter_description: 'A page not found page with a way home, a search box and suggested pages.'
  twitter_image_url: 'https://media.getosirion.com/og/components-not-found-page.jpg'
  meta_tags: {}
---

# NotFoundPage

`NotFoundPage` renders a whole page not found page: a title, a sentence, a link home, an optional search box and a list
of suggested pages. It asks search engines not to index the page. Use it as the router's not found page.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/NotFoundPage.razor).

There is no live demo on this page. `NotFoundPage` sets the page title and adds a `noindex` robots tag to the head, so
rendering it inside this page would ask search engines to drop the documentation page. Open any address that does not
exist on this site, such as `/no-such-page`, to see it.

## Usage

On .NET 10, give the router a not found page and use `NotFoundPage` there. The router answers with status 404. This is
the page the example site routes to, with suggestions and a search box:

```razor
@page "/not-found"
@using Osirion.Blazor.Cms.Web.Components
@using Osirion.Blazor.Components

<NotFoundPage Message="The page you are looking for does not exist or has moved."
              Suggestions="suggestions"
              ShowSearch />

@code {
    private static readonly IReadOnlyList<ContentNotFoundSuggestion> suggestions =
    [
        new() { Text = "Documentation", Url = "/docs", Description = "Every component, with examples." },
        new() { Text = "Blog", Url = "/blog", Description = "Guides, release notes and patterns." }
    ];
}
```

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

On .NET 9, put the component in the router's `NotFound` template instead; under static server-side rendering it then
sets the status to 404 itself.

`ShowSearch` adds a search box that submits to `SearchUrl` (`/search` by default), so turn it on only when the site has
a search page there. `ChildContent` adds your own markup under the box, for example a contact link.

## Parameters

<!-- osirion:parameters NotFoundPage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `"Page not found"` | The title, the page's `h1` and title tag. Defaults to `Page not found`. |
| `Message` | `string` | `"The page you are looking for does not exist or has moved."` | The sentence under the title. |
| `HomeUrl` | `string` | `"/"` | The address of the home link. Defaults to `/`. |
| `HomeText` | `string` | `"Back to the home page"` | The text of the home link. Defaults to `Back to the home page`. |
| `Suggestions` | `IReadOnlyList<ContentNotFoundSuggestion>?` | `null` | Pages to suggest, for example the documentation and the blog. None by default. |
| `SuggestionsTitle` | `string` | `"These pages might help:"` | The heading of the suggestions. Defaults to `These pages might help:`. |
| `ShowSearch` | `bool` | `false` | Whether a search box is shown. Defaults to `false`; turn it on when the site has a search page at `SearchUrl`. |
| `SearchUrl` | `string` | `"/search"` | The search page the box submits to. Defaults to `/search`. |
| `SearchPlaceholder` | `string` | `"Search the site"` | The placeholder of the search box. Defaults to `Search the site`. |
| `ChildContent` | `RenderFragment?` | `null` | Content shown below, for example a contact link. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- **Rendering.** The page renders on the server and needs no script.
- **Head.** The page sets the document title to `Title` and writes `<meta name="robots" content="noindex" />`.
- **Status.** On .NET 10 the page does not set the status; the router's `NotFoundPage` does. Setting it from the page
  there would send an empty response. On .NET 9 the page sets status 404 on the request while the response has not
  started, so use it inside the router's `NotFound` template there.
- **Content.** The page renders [OsirionContentNotFound](/components/content-not-found) with the error code 404, your
  `Title` (the page's `h1`) and `Message`, a primary button to `HomeUrl` labeled `HomeText`, the suggestions when
  there is at least one, and the search box when `ShowSearch` is on.
- **Security.** All text is encoded. The addresses in `HomeUrl`, `SearchUrl` and the suggestions are written as given, so
  build them from values you control.
- **Attributes.** `Class`, `Style` and unmatched attributes go to the root element, and so does `Theme` as
  `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

## Styling

The root element has the class `osirion-not-found-page`, which pads the page. The content is an
`OsirionContentNotFound`, which has its own classes (`osirion-content-not-found` and its parts); see its page for them.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [OsirionContentNotFound](/components/content-not-found): the component the page is built on
- [SearchBox](/components/search-box): the search box
