---
order: 3
title: LocalizedContentView
permalink: /components/localized-content-view
description: "Render one content item with links to its translations, locale-aware category and tag links, a sanitized body and optional previous and next links."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-localized-content-view.jpg'
categories:
- Components
- CMS Web Components
- Core Components
tags:
- localized-content-view
- localization
- translations
- cms
- static-ssr
slug: localized-content-view
lang: en
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/localized-content-view
custom_fields:
  group: cms-pages
seo_properties:
  title: 'LocalizedContentView: translated articles for Blazor'
  description: 'LocalizedContentView renders one CMS content item with links to its translations and locale-aware category and tag links.'
  image: 'https://media.getosirion.com/og/components-localized-content-view.jpg'
  canonical: 'https://getosirion.com/components/localized-content-view'
  lang: en
  robots: index, follow
  og_title: 'LocalizedContentView - Osirion.Blazor'
  og_description: 'One content item with links to its translations, locale-aware category and tag links and a sanitized body.'
  og_image_url: 'https://media.getosirion.com/og/components-localized-content-view.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'LocalizedContentView - Osirion.Blazor'
  twitter_description: 'One content item with links to its translations, locale-aware category and tag links and a sanitized body.'
  twitter_image_url: 'https://media.getosirion.com/og/components-localized-content-view.jpg'
  meta_tags: {}
---

# LocalizedContentView

`LocalizedContentView` renders one content item, like [ContentView](/components/content-view),
and adds links to the same item in other languages. Use it on multilingual sites where each translation is a
separate Markdown file.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/LocalizedContentView.razor).

```demo
localized-content-view
```

The demo loads this site's own page for the 1.5 release notes and shows it in a scrolling frame. This site's content is
written in English only, so the item has no translations and the language links do not appear; they show above the
article once a second language exists. `LocalizedContentView` writes the title as an `h1` unless you set
`HeadingLevel`; the demo sets it to `2`, because this documentation page has its own `h1`.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<LocalizedContentView Item="@item" CurrentLocale="@Locale" />
```

A page routed as `/{locale}/blog/{slug}`, with readable language names and translation links that keep the locale
in the address:

```razor
<LocalizedContentView Item="@item"
                      CurrentLocale="@Locale"
                      LocaleNameFormatter="@(code => code switch { "en" => "English", "sr" => "Srpski", _ => code })"
                      TranslationUrlFormatter="@((id, locale) => $"/{locale}/{item!.Url}")" />
```

This formatter assumes each translation has the same slug; build the URL another way if yours differ.

## How translations are found

Translations are the items that share the current item's `ContentId`. Give each translation the same `id` in its
front matter, or let the provider pair them: with `EnableLocalization` on, an item without an `id` gets a stable id
built from its path without the locale folder, so `en/blog/post.md` and `sr/blog/post.md` belong together. The links
appear only when localization is enabled on the component and more than one translation exists; the current locale
is left out.

## Parameters

<!-- osirion:parameters LocalizedContentView -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `LocalizationId` | `string?` | `null` | The id an item shares with its translations. The component sets it from the item's `ContentId`; leave it unset. |
| `Path` | `string?` | `null` | The source path of the item to load from the default provider while `Item` is not set. A later change of the path is ignored once an item is shown, so pass `Item` on a page that changes address while it is interactive. |
| `CurrentLocale` | `string` | `"en"` | The locale of the page. It is left out of the translation links, it is the locale of the default category and tag links (`/{locale}/category/{slug}`) and of the neighbor query. |
| `OnLocaleChanged` | `EventCallback<string>` | - | Has no effect: translation links are plain links and nothing in the markup raises it. |
| `LoadingText` | `string` | `"Loading content..."` | The text shown while the item loads, in interactive render modes only. A statically rendered response is sent once the item has loaded. |
| `NotFoundText` | `string` | `"Content not found."` | The message shown when no item is found, when loading fails (the error is logged) and when neither `Item` nor `Path` is set. |
| `LocaleNameFormatter` | `Func<string, string>?` | `null` | Builds the link text of a translation from its locale code. Without it the text is the code in upper case, such as `SR`. |
| `TranslationUrlFormatter` | `Func<string, string, string>?` | `null` | Builds a translation link from the localization id and the target locale. Without it the link is the translation's `Url` under a leading slash. The built-in providers store that `Url` without the locale folder, so when it is the same address as the shown item's, the link puts the translation's locale in front (`/sr/blog/post`); a translation whose `Url` differs, for example from a `permalink`, keeps its own. Set this when your routes are built another way. |
| `CategoryUrlFormatter` | `Func<string, string>?` | `null` | Builds a category link from the category name. Without it the link is `/{CurrentLocale}/category/{slug}`. |
| `TagUrlFormatter` | `Func<string, string>?` | `null` | Builds a tag link from the tag name. Without it the link is `/{CurrentLocale}/tag/{slug}`. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the links of the previous and next items from the item. Without it a link is `/{item.Path}`, the source path, which is usually not the public address. |
| `Item` | `ContentItem?` | `null` | The item to render. When it is set, the component loads its translations (and the neighbors when `ShowNavigationLinks` is on) and never loads by `Path`. |
| `PreviousItem` | `ContentItem?` | `null` | The item the previous link points to. Loading with `ShowNavigationLinks` on looks the neighbors up only when you pass neither this nor `NextItem`; values you pass are kept, as `ContentPage` relies on for the neighbors it computed. |
| `NextItem` | `ContentItem?` | `null` | The item the next link points to. See `PreviousItem`. |
| `ShowNavigationLinks` | `bool` | `false` | Whether to show previous and next links to items in the same directory (matched by its path) and locale, newest first. The component looks the neighbors up whenever it loads, also for an item you pass, unless you pass `PreviousItem` or `NextItem`. |
| `ShowJumbotron` | `bool` | `true` | Whether to render the header: the title as the page's `h1`, the author ("By ..."), the date, the reading time and the featured image. When it is on, the body leaves out its own leading `h1` so the title appears once; when it is off, the body is written as it is. |
| `EnableLocalization` | `bool` | `true` | Whether to look up the item's translations and list them. The links appear only when the item has more than one translation, and the current locale is left out. |
| `HeadingLevel` | `int` | `1` | The level of the heading that holds the title, `1` to `6` (`h1` to `h6`). It defaults to `1`, the page's own heading; set `2` for a view embedded in a page that has its own `h1`, so the page keeps a single one. A value outside the range is moved to the nearest level. The title keeps its look at every level. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The `Theme` and `Style` rows come from the base class. `LocalizedContentView` does not write them, or unmatched attributes, so only `Class` reaches the root element.

## Behavior

- **Rendering.** Static SSR renders the item, its translation links and its footer on the server; there is no
  script. Translation, category, tag and navigation links are ordinary links.
- **Loading.** The component loads by `Path` only while `Item` is null, so in an interactive page that changes
  address, pass `Item` rather than a new `Path`.
- **Header.** With `ShowJumbotron` on, the header shows the title as an `h1` (set `HeadingLevel`, 1 to 6, for another
  level, `2` when the page has its own `h1`), and the body leaves out its own leading `h1` so the title appears once.
  With it off, the body is written as it is, leading `h1` included.
- **Default URLs.** The built-in providers drop the locale folder from an item's `Url`, so every translation stores the
  same address. When a translation's `Url` is the same address as the shown item's, its link puts the translation's
  locale in front (`/sr/blog/post`), so it does not point at the page the visitor is on. A translation whose `Url`
  differs, for example from a `permalink`, keeps its own. Set `TranslationUrlFormatter` when your routes are built
  another way, such as a default language without a prefix. The default previous and next link is the item's source
  path; set `ContentUrlFormatter` when you show navigation links.
- **Neighbors.** With `ShowNavigationLinks` on, the component looks up the previous and next items of the item's
  directory, matched by its path, and locale, unless you pass `PreviousItem` or `NextItem`; what you pass is kept and
  nothing is looked up. Inside [ContentPage](/components/content-page) the neighbors the page computed are the ones
  shown.
- **Dates.** The date in the header follows the language of the item's locale, or the current culture when the locale
  is missing or not a culture name, and is written inside a `<time>` element.
- **Locale changes.** `OnLocaleChanged` is never raised. The translation links are ordinary links that load another
  page, so the component never changes locale itself; read the locale from the new page's route instead.
- **Not found.** Without an item the component renders `NotFoundText` in a plain block and signals 404, as
  `ArticlePage` does: `NavigationManager.NotFound()` on .NET 10, the status code of the request on .NET 9, while the
  response has not started. Render it only when you have an item or a path to load.
- **Security.** The body passes through the registered `IHtmlContentSanitizer` before it is written as HTML, the
  same allow-list `ContentView` uses. Titles, names and locale labels are encoded as text.
- **Images.** The featured image takes its `alt` text from the `featured_image_alt` entry of the front matter's `custom_fields`, for example `custom_fields:` with `featured_image_alt: A bridge at dusk` below it. The front matter model has no property of its own for it, and a `featured_image_alt` key outside `custom_fields` is ignored. Without the entry the `alt` is empty, which marks the image as decorative, instead of repeating the title that the heading beside it already shows. From 4.0 the image loads eagerly with
  `fetchpriority="high"`.
- **SEO.** Nothing is written to the head. For `hreflang` links, pass the translations to
  [SeoMetadataRenderer](/components/seo-metadata-renderer) as `AlternateLanguageUrls`.

## Styling

The root element has the class `osirion-localized-content-view`. Inside are `osirion-translations` (the language
links), `osirion-content-article`, `osirion-content-header` with `osirion-content-meta`, `osirion-content-body`,
`osirion-content-footer` with `osirion-content-categories` and `osirion-content-tags`, and
`osirion-content-navigation`. The not-found and loading states use `osirion-not-found` and `osirion-loading`.

The body is markup the component writes itself, outside `OsirionHtmlRenderer`, so the library gives its headings,
paragraphs, lists and tables no typography: they take the style of your CSS framework or your own CSS (the rules
`osirion-content-body h2`, `p` and the like that older builds carried never matched and are gone). Two rules do reach
it, and keep a wide image or code block inside the column: `img` is at most as wide as the body, and `pre` scrolls
sideways. For a body with the library's typography, render it with [ContentRenderer](/components/content-renderer).
Colors and spacing come from theme tokens such as `--osirion-text-primary`, `--osirion-text-secondary` and
`--osirion-action-primary`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [ContentView](/components/content-view): the single-language version
- [ContentPage](/components/content-page): renders this component inside a page layout
- [LocalizedNavigation](/components/localized-navigation): language-aware navigation
- [Localization for Blazor content sites](/blog/localization-for-blazor-content-sites)
