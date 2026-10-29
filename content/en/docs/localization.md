---
id: 'guide-localization'
order: 8
layout: docs
title: "Localization"
permalink: /docs/localization
description: "Serve a site in more than one language with Osirion.Blazor: language folders, translations, locale routes, hreflang links, and what does not work yet."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/docs-localization.jpg'
categories:
- Documentation
- Guides
tags:
- localization
- hreflang
- translations
- content
- seo
published: true
slug: localization
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-localization.jpg'
  title: 'Localize an Osirion.Blazor content site'
  description: 'Language folders, translations, locale routes and hreflang links for a Markdown site built with Osirion.Blazor, and the limits that exist today.'
  image: 'https://media.getosirion.com/og/docs-localization.jpg'
  canonical: 'https://getosirion.com/docs/localization'
  robots: index, follow
---

# Localization

This guide sets up a site whose pages exist in more than one language, each translation a separate Markdown file. It
is for site authors who already have a working single-language site. The library groups translations and finds
them; the site chooses the addresses and writes the language links, and this guide shows how.

## Before you start

You need the site from [Getting started](/docs/getting-started) and the ideas in [How content is organized](/docs/content).
The examples use two languages, English (`en`, the default) and Serbian (`sr`), and a `blog` folder.

## Put each language in its own folder

```text
content/
  en/
    blog/
      hello.md
  sr/
    blog/
      hello.md
```

A translation sits at the same path in each language folder. That shared path is what pairs the files (see [Write `lang` in every translation](#write-lang-in-every-translation)).

## Turn localization on

The same keys exist for the local folder provider (`Osirion:Cms:Web:FileSystem`) and for each GitHub provider
(`Osirion:Cms:Web:GitHub:<Name>`):

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true,
          "EnableLocalization": true,
          "DefaultLocale": "en",
          "SupportedLocales": [ "en", "sr" ]
        }
      }
    }
  }
}
```

`EnableLocalization` is `false` by default. With it on, a first folder whose name is in `SupportedLocales` is taken as
the item's language and left out of its address: `sr/blog/hello.md` has the address `blog/hello`. `SupportedLocales`
defaults to `["en"]`, so list every language, with the exact folder name (`sr-Latn-RS` is fine if that is the folder).

## Write `lang` in every translation

```yaml
---
title: "Zdravo, svete"
date: 2026-10-17
slug: hello
lang: sr
---
```

A file without `lang` gets the locale of its folder, or `DefaultLocale` when its path has no locale folder. An explicit
`lang` always wins. A folder's `_index.md` follows the same rule for the locale of the folder. This arrives in the
release after 4.1.0. In 4.1.0 the `lang` key is required: a file with front matter and no `lang` gets the locale `en`,
whatever its folder or `DefaultLocale`, so a Serbian file without `lang: sr` is treated as a second English page and the
lookup of its translations fails, and a folder's `_index.md` without `lang` counts as English when navigation asks for
the folders of one language. On 4.1.0, write `lang` in every file, the default language included.

Translations are paired by an id the provider builds from the path without the language folder and the extension, so
`en/blog/hello.md` and `sr/blog/hello.md` belong together even when their slugs differ. Files at different paths, such as
`en/blog/hello.md` and `sr/blog/zdravo.md`, pair only if you give both the same `id` in front matter.

## Serve a second script without a second file

Serbian is written in Cyrillic and in Latin script. A site can write each page once, in Cyrillic under `sr/`, and let
the provider serve the same content in Latin as `sr-Latn`: a derived locale, made by transliteration when content
loads. Add `DerivedLocales` next to `SupportedLocales`, for the local folder provider or any GitHub provider
(`Osirion:Cms:Web:GitHub:<Name>`):

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "EnableLocalization": true,
          "DefaultLocale": "sr",
          "SupportedLocales": [ "sr", "en" ],
          "DerivedLocales": [
            { "Locale": "sr-Latn", "From": "sr", "Transliteration": "serbian-cyrillic-to-latin" }
          ]
        }
      }
    }
  }
}
```

`From` is a locale the content is written in, so list it in `SupportedLocales`. The derived locale is added to the
provider's locales for you; listing it there as well changes nothing. Startup fails, naming the key (for example
`DerivedLocales[0].From`), when `EnableLocalization` is off, when `Locale` or `From` is not a culture name or they are
the same, when `From` is not in `SupportedLocales` or is itself derived, when no transliterator has the name, and when a
locale is derived twice.

**What is derived.** Every `From` item without an item of the same localization id in the derived locale gets one. A
real file always wins: write `sr-latn/blog/hello.md` (the folder name in any letter case) and the provider serves that
file instead. The derived item keeps the localization id, the slug and the address (`blog/hello`), takes the derived
locale and the path a file of that locale would have (`sr-Latn/blog/hello.md`), and has `IsDerived` set, with the
source locale in `DerivedFromLocale`. It shows up in lists, tag and category pages of `sr-Latn`, in
`GetContentTranslationsAsync`, and `GetLocalizationInfoAsync` reports `sr-Latn` once it has published items.

**What is transliterated.** The title, description, categories, tags, series and the front matter's SEO and social
titles and descriptions. In the rendered HTML, the text and the `alt`, `title`, `aria-label` and `placeholder`
attributes. Left as written:

- everything inside `code`, `pre`, `kbd`, `samp`, `script`, `style` and any element with `translate="no"`, so wrap a
  brand name in `<span translate="no">` to keep it in Cyrillic;
- a link's text that is its own address (an autolink);
- every `href` and `src` that is absolute, protocol-relative, `mailto:` or `tel:`, a fragment, a relative path, or a
  file (`/sr/feed.xml`, images);
- the slug, the address, the author, the dates, the images, `canonical` and JSON-LD.

A link to one of the site's own pages that starts with the source locale moves under the derived locale:
`/sr/blog/hello` becomes `/sr-Latn/blog/hello`, written with `Locale` exactly as configured, so give `Locale` the letter
case your routes use. A link without a locale segment (`/blog/hello`) stays as it is.

**Read only.** A derived item has no file and no markdown source of its own (`OriginalMarkdown` is empty: search reads
the rendered content, and a source in the other script must never be written back under the derived locale). The
repositories refuse to save or delete it, and the admin, which edits files, never lists it. To change it, change the
Cyrillic file, or add a file for the derived locale, which then wins.

Folders are not derived: a folder's `_index.md` is, as a page, but the directory tree of `sr-Latn` is empty, so build
navigation for the derived locale from its pages or from the source locale's folders. The built-in transliterator is
`serbian-cyrillic-to-latin`, which writes Љ, Њ and Џ as `LJ`, `NJ` and `DŽ` in a word written in capitals and as `Lj`,
`Nj` and `Dž` otherwise. To add another, implement `ITextTransliterator` (a `Name` and `Transliterate(string)`) and
register it with `services.AddSingleton<ITextTransliterator, MyTransliterator>()`; one registered after the built-in
under the same name replaces it.

## Route by language

Two translations share one address (`blog/hello`), so the address alone cannot choose a language. Give the site a route
per language and look the page up with its locale. This page serves `/blog/hello` in the default language and
`/sr/blog/hello` in Serbian, and writes the `hreflang` links (see [Write the hreflang links](#write-the-hreflang-links)):

```razor
@page "/blog/{Slug}"
@page "/{Locale}/blog/{Slug}"
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Domain.Repositories
@using Osirion.Blazor.Cms.Domain.Services
@inject IContentProviderManager Content
@inject NavigationManager Navigation

@if (item is not null)
{
    <ArticlePage Item="item"
                 Locale="@locale"
                 AlternateLanguageUrls="alternates"
                 SchemaTypes="new[] { SchemaType.BlogPosting }" />
}

@code {
    private const string DefaultLocale = "en";

    private ContentItem? item;
    private string locale = DefaultLocale;
    private List<string> alternates = [];

    [Parameter] public string Slug { get; set; } = string.Empty;

    [Parameter] public string? Locale { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        locale = Locale ?? DefaultLocale;

        var found = await Content.GetContentByQueryAsync(new ContentQuery
        {
            Directory = "blog",
            Slug = Slug,
            Locale = locale,
        });
        item = found.FirstOrDefault();
        if (item is null)
        {
            Navigation.NotFound();
            return;
        }

        var translations = await Content.GetContentByQueryAsync(new ContentQuery { LocalizationId = item.ContentId });
        alternates = translations
            .Select(translation => $"{translation.Locale}|{Navigation.ToAbsoluteUri(PathOf(translation))}")
            .ToList();

        var original = translations.FirstOrDefault(translation => translation.Locale == DefaultLocale);
        if (original is not null)
        {
            alternates.Add($"x-default|{Navigation.ToAbsoluteUri(PathOf(original))}");
        }
    }

    private static string PathOf(ContentItem translation) =>
        translation.Locale == DefaultLocale ? $"/{translation.Url}" : $"/{translation.Locale}/{translation.Url}";
}
```

`Locale` is a parameter of the page components and of `ContentList`; the page components default to `en`, so pass it
on every page of a non-English site. A list of Serbian posts is `<ContentList Directory="blog" Locale="sr" />`.

To list the languages a site has, the built-in providers implement `ILocalizationInfoProvider`: cast
`Content.GetDefaultProvider()` to it and call `GetLocalizationInfoAsync()`. It returns the default locale first, then
each of `SupportedLocales` that has published content, each with its English and native name; with
`EnableLocalization` off it returns only the default locale. It reads the content the provider has already loaded.
[LocalizedNavigation](/components/localized-navigation) uses it for its language links, which put the locale in front of
the current path (`/sr/blog/hello`); pass its `LocaleUrlFormatter` to leave the default language without a prefix, as
the page above routes it.

## Write the hreflang links

`SeoMetadataRenderer` writes one `<link rel="alternate" hreflang="...">` for each entry of its `AlternateLanguageUrls`
parameter, an entry being a language code and an address joined by a vertical bar (`sr|https://example.com/sr/blog/hello`).
It does not look up translations itself. `ArticlePage`, `DocumentPage`, `HomePage`, `AboutPage` and `ContactPage` pass
the parameter on, as the page above does. An entry without both parts is skipped and a warning is logged.

Google's rules for these tags: each address must be fully qualified (`https://...`), and every language version must
list itself and all the others, or the tags are ignored. The page above builds its list from the translations, which
include the page itself, and adds an `x-default` entry for the default language.

## Put every language in the sitemap and feeds

The sitemap writes each address once, so translations that share an address appear only once. Build language-aware
addresses with `ContentUrlFormatter`, and pass the same formatter to `UseOsirionRedirects` if you use `redirect_from`:

```csharp
app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://example.com";
    feeds.FeedDirectory = "blog";
    feeds.ContentUrlFormatter = item =>
        item.Locale == "en" ? "/" + item.Url : $"/{item.Locale}/{item.Url}";
});
```

The sitemap carries no `hreflang` annotations; the links in the page head are what the library supports. The feed
paths are fixed, so there is one RSS and one Atom feed: `FeedLocale` limits them to one language, and leaving it empty
lists every language together.

## Check that it works

Run the site, open `/content-check` from the [content guide](/docs/content), and confirm that every file shows the
locale you expect (`sr` for the Serbian files). Then open `/sr/blog/hello`, choose View Source and look in the head for:

```html
<link rel="alternate" hreflang="en" href="https://localhost:5001/blog/hello" />
<link rel="alternate" hreflang="sr" href="https://localhost:5001/sr/blog/hello" />
<link rel="alternate" hreflang="x-default" href="https://localhost:5001/blog/hello" />
```

The host and port are yours. If a Serbian file shows `en` on 4.1.0, it has no `lang`. If a page has no `hreflang`
links, its translations were not found: check that the files share a path or an `id`. Two files of one id that claim the
same locale show up once in the list above, the first of them, and the provider logs a warning that names the locale. On
4.1.0 they make the lookup fail, and `LocalizedContentView` logs `Error retrieving translations dictionary`.

## What does not work yet

- **`lang` is required in 4.1.0** (see [Write `lang` in every translation](#write-lang-in-every-translation)): `DefaultLocale` does not apply to a file that has front matter. The release
  after 4.1.0 reads a missing `lang` from the folder or `DefaultLocale`.
- **Addresses carry no language.** Translations share one `Url`, so routes, sitemap and redirect addresses need the
  formatters above.
- **`LocalizedContentView` links each translation to its own `Url`**, which is the same address for translations at
  the same path, so pass `TranslationUrlFormatter` to give each language its route.
  Its category and tag links start with the current locale, and its previous and next links use the source path by
  default. `OnLocaleChanged` is never raised, because the translation links are plain links. See
  [LocalizedContentView](/components/localized-content-view).
- **Nothing is translated for you.** Menu text, footer text and the labels of components come from your configuration
  and parameters.

## Next steps

- [Locale routing](/docs/locale-routing): set the culture of each request from its URL prefix, with canonical
  redirects, localized links and a language picker
- [SEO](/docs/seo): canonical addresses, the sitemap and feeds
- [Front matter reference](/docs/front-matter): `lang` and `id`
- [SeoMetadataRenderer](/components/seo-metadata-renderer): the `AlternateLanguageUrls` parameter
- [LocalizedContentView](/components/localized-content-view) and [LocalizedNavigation](/components/localized-navigation)
- [Localization for Blazor content sites](/blog/localization-for-blazor-content-sites)
