---
id: 'osirion-content-not-found'
order: 4
layout: docs
title: OsirionContentNotFound
permalink: /components/content-not-found
description: OsirionContentNotFound renders a 404 or empty state with an error code, message, buttons, suggested links, a search slot and a contact address.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-content-not-found.jpg'
categories:
- Components
- Core Components
- States
- Error Handling
tags:
- blazor
- 404
- not-found
- empty-state
- static-ssr
is_featured: true
published: true
slug: content-not-found
lang: en
redirect_from:
- /docs/components/core/states/content-not-found
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'OsirionContentNotFound: 404 and empty state for Blazor'
  description: 'A 404 or empty state for Blazor with an error code, message, buttons, suggested links and a contact address, rendered without script.'
  image: 'https://media.getosirion.com/og/components-content-not-found.jpg'
  canonical: 'https://getosirion.com/components/content-not-found'
  lang: en
  robots: index, follow
  og_title: 'OsirionContentNotFound - Osirion.Blazor'
  og_description: 'A 404 or empty state with a message, buttons and suggested links, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-content-not-found.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContentNotFound - Osirion.Blazor'
  twitter_description: 'A 404 or empty state with a message, buttons and suggested links, rendered without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-content-not-found.jpg'
  meta_tags: {}
---

# OsirionContentNotFound

`OsirionContentNotFound` renders the body of a not found page: an icon, an error code, a heading, a message and
buttons back into the site, with optional suggested links, a search slot and a contact address. With the error code
removed, it also works as an empty state for a list or search. The CMS page components show it when their content
is missing.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/States/OsirionContentNotFound.razor).

```demo
content-not-found
```

The demo only renders the markup. It does not set the status code of this page, which stays 200. The component's
title is an `<h1>` by default; this page has its own, so the demo sets `HeadingLevel="2"`.

## Usage

With no parameters it renders "404", "Page Not Found", a short message and a "Go to Homepage" button:

```razor
<OsirionContentNotFound />
```

A not found page that points readers to likely destinations:

```razor
<OsirionContentNotFound Title="This page has moved"
                        Message="We reorganized the docs. One of these pages may have what you need."
                        ShowSuggestions="true"
                        Suggestions="@suggestions"
                        SecondaryButtonText="All docs"
                        SecondaryButtonUrl="/docs"
                        ShowContactInfo="true"
                        ContactEmail="hello@example.org"
                        Variant="centered" />

@code {
    private readonly ContentNotFoundSuggestion[] suggestions =
    [
        new() { Text = "Components", Url = "/docs/components", Description = "Every component, by package." },
        new() { Text = "Blog", Url = "/blog" },
    ];
}
```

An empty state: clear the error code and the default button, and make it smaller. The title is still an `<h1>`, which
suits an empty state that is the page's main content, such as a blog index with no posts yet. Inside a page that has
its own `<h1>`, add `HeadingLevel="2"`:

```razor
<OsirionContentNotFound ErrorCode="@null"
                        Title="No posts yet"
                        Message="The first article is on its way."
                        PrimaryButtonText="@null"
                        Variant="minimal"
                        Size="small" />
```

## Parameters

<!-- osirion:parameters OsirionContentNotFound -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ErrorCode` | `string?` | `"404"` | The large code above the title, such as `404`. Null or empty hides it, which suits an empty state. |
| `Title` | `string` | `"Page Not Found"` | The heading, rendered at `HeadingLevel`. |
| `HeadingLevel` | `int` | `1` | The level of the heading that holds the title, `1` to `6` (`h1` to `h6`). It defaults to `1`, the page's own heading; set `2` for an empty state inside a page that has its own `h1`, so the page keeps a single one. A value outside the range is moved to the nearest level. The title keeps its look at every level. |
| `Message` | `string` | `"The page you're looking for doesn't exist or has been moved."` | The paragraph under the title. Empty hides it. |
| `ShowIcon` | `bool` | `true` | Whether an icon is shown above the text. |
| `UseDefaultIcon` | `bool` | `true` | Whether the built-in magnifier icon is shown when neither `Icon` nor `IconUrl` is set. |
| `Icon` | `RenderFragment?` | `null` | Your own icon markup. It takes precedence over `IconUrl` and the default icon. |
| `IconUrl` | `string?` | `null` | The address of an image used as the icon, when `Icon` is not set. |
| `IconAlt` | `string` | `"Not Found"` | The alternative text of the `IconUrl` image. |
| `PrimaryButtonText` | `string?` | `"Go to Homepage"` | The text of the primary button, a link to `PrimaryButtonUrl`. Null or empty hides the button. |
| `PrimaryButtonUrl` | `string` | `"/"` | The address of the primary button. An address that is not a local path or an http, https, mailto or tel URL leaves the button out. |
| `SecondaryButtonText` | `string?` | `null` | The text of the outlined secondary button, a link to `SecondaryButtonUrl`. Null or empty, the default, hides it. |
| `SecondaryButtonUrl` | `string` | `"/"` | The address of the secondary button. An unsafe address leaves the button out. |
| `Actions` | `RenderFragment?` | `null` | More buttons or links, rendered after the two buttons. |
| `ShowSuggestions` | `bool` | `false` | Whether the `Suggestions` list is shown. |
| `SuggestionsTitle` | `string` | `"Here are some helpful links:"` | The text above the suggestions. |
| `Suggestions` | `IReadOnlyList<ContentNotFoundSuggestion>?` | `null` | The links to suggest. They are shown only when `ShowSuggestions` is true and the list is not empty. |
| `ShowSearchBox` | `bool` | `false` | Whether `SearchContent` is rendered. |
| `SearchContent` | `RenderFragment?` | `null` | A search form of your choice. It is rendered only when `ShowSearchBox` is true. |
| `ShowContactInfo` | `bool` | `false` | Whether the contact line is shown. It also needs `ContactEmail`. |
| `ContactMessage` | `string` | `"Need help? Contact us at"` | The text before the email address. |
| `ContactEmail` | `string?` | `null` | The address in the contact line, written as a `mailto:` link. |
| `Variant` | `string` | `"default"` | The look: `default` (plain), `minimal` (smaller text and icon), `centered` (a rounded panel in the secondary background) or `hero` (full window height with a gradient). Added as the class `osirion-content-not-found-{Variant}`. |
| `Size` | `string` | `"medium"` | The minimum height: `small`, `medium`, `large` or `full` (the window below the header). Added as the class `osirion-content-not-found-{Size}`. |
| `BackgroundPattern` | `BackgroundPatternType?` | `null` | A decorative pattern behind the content. Null, the default, shows none. |
| `ChildContent` | `RenderFragment?` | `null` | Other content, rendered after the search slot and before the buttons. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the root `<div>`. A `ContentNotFoundSuggestion` has `Text`, `Url`, an optional
`Description` shown under the link, and `Target` (default `_self`).

## Behavior

- The component renders plain HTML and needs no script.
- All text is HTML-encoded. A button or suggestion address must be a local path or an http, https, mailto or tel URL. Any other
  address, such as `javascript:`, leaves the button out and renders the suggestion text without a link.
- `Title` is an `<h1>`, so use the component as the main content of the page, not inside a page that already has
  one.
- The buttons take the configured CSS framework's primary and outlined secondary button classes.

### Status code

The component renders markup only; it does not set the HTTP status code. Without a 404, search engines can index the
page as real content.

On .NET 10, give the router a not found page and use the component there:

```razor
<Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

```razor
@page "/not-found"

<PageTitle>Page not found</PageTitle>

<OsirionContentNotFound SecondaryButtonText="All docs" SecondaryButtonUrl="/docs" />
```

When a page finds that its own content is missing, it calls `NavigationManager.NotFound()` on .NET 10, or sets
`Response.StatusCode` on the cascading `HttpContext` during static rendering on .NET 9. The CMS `ArticlePage`,
`DocumentPage` and `HomePage` do this themselves.

## Styling

The root has `osirion-content-not-found`, `osirion-content-not-found-{Variant}` and
`osirion-content-not-found-{Size}`, plus `osirion-content-not-found-with-code` when there is an error code and
`osirion-content-not-found-with-background` with a pattern.

| Variant | Look |
| --- | --- |
| `default` | Plain, on the page background |
| `minimal` | Smaller code, title, message and icon |
| `centered` | On a rounded panel in `--osirion-background-secondary` |
| `hero` | Full window height below the header, with a gradient background and a large gradient error code |

The sizes set a minimum height: 300px for `small`, 400px for `medium`, 500px for `large`. The `full` size fills the
window below the header, using `--osirion-header-height` (default `4rem`). Inner elements use
`osirion-content-not-found-code`, `-title`, `-message`, `-suggestions`, `-search`, `-custom`, `-actions` and
`-contact`, each prefixed with `osirion-content-not-found`. Colors and sizes come from the theme tokens
(`--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-action-primary`, `--osirion-font-size-*`).

## Related

- [Forms and feedback](/components/forms-and-feedback)
- [Page loading](/components/page-loading)
- [Background pattern](/components/background-pattern)
