---
title: "Localization for Blazor Content Sites"
author: "Osirion Team"
date: "2025-01-25"
last_modified: 2026-10-11
description: "Serve Markdown content in several languages with Osirion.Blazor: locale folders, provider settings, routes, translation links and hreflang."
slug: "localization-for-blazor-content-sites"
categories: [Localization]
tags: [Localization, Translations, hreflang, CMS]
featured_image: 'https://media.getosirion.com/og/blog-localization-for-blazor-content-sites.jpg'
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-localization-for-blazor-content-sites.jpg'
  title: "Localization for Blazor Content Sites"
  description: "Locale folders, translation pairing, locale routes, translation links and hreflang for a multilingual Osirion.Blazor site."
  type: "Article"
---

# Localization for Blazor content sites

Osirion.Blazor's CMS serves one Markdown file per language and pairs the translations of the same page. This article
walks through a multilingual setup from the content folder to the page head: provider settings, folder layout, how
translations are paired, routes, translation links, `hreflang` and feeds. It ends with what the library leaves to
you.

## Turn localization on

Localization is a setting of the content provider and is off by default. For a GitHub provider configured in
`appsettings.json`:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Site": {
            "Owner": "your-account",
            "Repository": "your-content",
            "Branch": "main",
            "ContentPath": "content",
            "EnableLocalization": true,
            "DefaultLocale": "en",
            "SupportedLocales": [ "en", "sr" ]
          }
        }
      }
    }
  }
}
```

The FileSystem provider (`Osirion:Cms:Web:FileSystem`) takes the same three keys.

## One folder per locale

Put each language in its own folder directly below `ContentPath`, with the same structure inside:

```text
content/
  en/
    blog/
      _index.md
      release-notes.md
  sr/
    blog/
      _index.md
      release-notes.md
```

A file's locale is the first folder below `ContentPath` when that folder is listed in `SupportedLocales` (compared
without regard to case); otherwise it is `DefaultLocale`. With an empty `SupportedLocales` list, any two-letter code
or a code such as `sr-RS` counts as a locale folder.

Then set `lang` in the front matter of every file. The `lang` key wins over the folder. In 4.1.0 a file whose front matter
has no `lang` is read as `en`, whatever folder it is in; the release after 4.1.0 reads the locale from the folder, or from
`DefaultLocale`, when `lang` is missing:

```yaml
---
title: "Napomene o izdanju"
lang: sr
---
```

The locale folder is not part of the item's URL: `en/blog/release-notes.md` and `sr/blog/release-notes.md` both get
the URL `blog/release-notes`. Your routes add the locale back, as shown below.

## How translations are paired

Translations share a localization id. When the front matter has an `id`, that is the id. Without one, and with
localization on, the provider computes a stable id from the path without the locale folder and the extension, so
files with the same relative path pair up on their own.

If you translate file names, the paths differ and the computed ids differ too. Either keep the file names the same
and translate the `slug` key, or give every translation the same `id`:

```yaml
---
id: release-notes
title: "Napomene o izdanju"
slug: napomene-o-izdanju
lang: sr
---
```

A slug may contain only lowercase letters, digits and hyphens. A file whose slug breaks that rule is not loaded, and
the error is logged.

## Routes with a locale segment

Give each content page a route with and without the locale, and pass the locale to the component that queries the
content. This is the blog page of the example app:

```razor
@page "/blog"
@page "/{locale}/blog"
@page "/blog/{slug}"
@page "/{locale}/blog/{slug}"
@using Osirion.Blazor.Cms.Web.Components

@if (string.IsNullOrWhiteSpace(Slug))
{
    <ContentList Locale="@Locale" Directory="blog" ShowPagination ItemsPerPage="12" />
}
else
{
    <ArticlePage Locale="@Locale" DirectoryName="blog" ItemSlug="@Slug" />
}

@code {
    [Parameter]
    public string? Locale { get; set; } = "en";

    [Parameter]
    public string? Slug { get; set; }
}
```

`/blog/release-notes` shows the English item and `/sr/blog/release-notes` the Serbian one. Blazor route templates
cannot limit `{locale}` to your list; an unknown value finds no content, so `ArticlePage` answers 404 and
`ContentList` shows its empty state.

## Links between translations

`ArticlePage` does not render translation links. `LocalizedContentView` does: it renders the item and links to the
other translations it finds. The provider leaves the locale out of item URLs, so add it back with
`TranslationUrlFormatter`:

```razor
<LocalizedContentView Item="@item"
                      CurrentLocale="@Locale"
                      LocaleNameFormatter="@(code => code == "sr" ? "Srpski" : "English")"
                      TranslationUrlFormatter="@((id, locale) => $"/{locale}/{item!.Url}")" />
```

The formatter receives the localization id and the target locale. Building the link from the current item's URL, as
here, works when the translations share a slug; if they do not, look the translation up yourself. The links are plain
`<a>` elements, so they work with static SSR.

## hreflang in the page head

Search engines learn about translations from `<link rel="alternate" hreflang="...">` tags. `ArticlePage` passes
`AlternateLanguageUrls` to `SeoMetadataRenderer`, one `language|url` entry per translation:

```razor
<ArticlePage Locale="@Locale" DirectoryName="blog" ItemSlug="@Slug" AlternateLanguageUrls="alternates" />

@code {
    [Parameter]
    public string? Locale { get; set; } = "en";

    [Parameter]
    public string? Slug { get; set; }

    private List<string> alternates => [$"en|https://example.com/blog/{Slug}", $"sr|https://example.com/sr/blog/{Slug}"];
}
```

Malformed entries are skipped with a logged warning.

## Navigation per locale

`LocalizedNavigation` renders the folder tree of one locale. Its default links use the source path, so set the URL
formatters:

```razor
<LocalizedNavigation CurrentLocale="@Locale"
                     ExpandAllDirectories="true"
                     DirectoryUrlFormatter="@(directory => $"/{Locale}/{directory.Url.TrimStart('/')}")"
                     ContentUrlFormatter="@(item => $"/{Locale}/{item.Url}")" />
```

A folder's URL is the `permalink` of its `_index.md`. With static SSR the links navigate as usual. In an interactive
render mode, a click raises `OnDirectorySelected` or `OnContentSelected` instead of navigating, so handle those
callbacks there. The component has a locale selector in its markup, but in 4.0 it does not load the list of
available locales, so the selector does not appear; render your own language links in the layout.

## Feeds and the sitemap

Available from 4.1. `MapOsirionFeeds` links each item as `/{item.Url}`, which has no locale, so translations of the
same page would share one sitemap entry. Put the locale back with `ContentUrlFormatter`, and use `FeedLocale` for a
feed in one language:

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://example.com";
    feeds.FeedDirectory = "blog";
    feeds.FeedLocale = "en";
    feeds.ContentUrlFormatter = item => item.Locale == "en" ? $"/{item.Url}" : $"/{item.Locale}/{item.Url}";
});
```

## What the library leaves to you

- **Interface text.** Components take their labels as parameters (`ContentPage.CategoriesTitle`,
  `ContentSeriesNavigation.PositionFormat` and others). Pass translated strings from your pages, for example from an
  `IStringLocalizer`. Some labels in `LocalizedContentView` ("By", "Categories:", "Tags:", "Previous") are fixed
  English text.
- **Choosing the language.** Nothing reads `Accept-Language` or redirects a visitor to their language. Decide that in
  your own middleware or layout.
- **Dates and numbers.** Dates are formatted with the current culture. Set it with ASP.NET Core's request
  localization if you want month names in the page's language.
- **Missing translations.** A page that exists only in English is not served under `/sr/`; link to the English page
  instead.

## Related

- [LocalizedContentView](/components/localized-content-view)
- [LocalizedNavigation](/components/localized-navigation)
- [SeoMetadataRenderer](/components/seo-metadata-renderer)
- [Structuring content repositories](/blog/structuring-content-repositories)
- [Blazor SEO checklist for static SSR content sites](/blog/blazor-seo-ssr-ssg-checklist)
