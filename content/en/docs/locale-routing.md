---
id: 'guide-locale-routing'
order: 8
layout: docs
title: "Locale routing"
permalink: /docs/locale-routing
description: "Take the language of each request from the first segment of its address, redirect to canonical addresses, link pages across languages and add a language picker."
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/docs-locale-routing.jpg'
categories:
- Documentation
- Guides
tags:
- localization
- routing
- culture
- hreflang
published: true
slug: locale-routing
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-locale-routing.jpg'
  title: 'Locale routing for Osirion.Blazor sites'
  description: 'One address per page and language: the culture from the URL prefix, canonical redirects, localized links and an SSR language picker.'
  image: 'https://media.getosirion.com/og/docs-locale-routing.jpg'
  canonical: 'https://getosirion.com/docs/locale-routing'
  robots: index, follow
---

# Locale routing

This guide is for a site with language versions: `/contact` in the default language and `/sr/contact` in Serbian.
It sets the culture of each request from its address, redirects the addresses that should not exist, and writes links
that stay in the visitor's language. It arrives in the release after 4.2.0.

Two settings work together, and each does one job:

- `Osirion:Localization`, set up here, decides which culture a request has, from its URL.
- `Osirion:Site:Locales` decides what the site shell (`OsirionSiteLayout`: menu, footer, labels) says in that culture.
  The [OsirionSiteLayout reference](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/components/OsirionSiteLayout.md)
  lists its keys.

Content in several languages (language folders, translations, `hreflang` links in the head) is covered by
[Localization](/docs/localization).

## Before you start

You need a working site from [Getting started](/docs/getting-started). The examples use English (`en`, the default) and
Serbian in Latin script (`sr-Latn`, at `/sr`).

## How a request gets its culture

The first segment of the path alone decides. `/sr/contact` is Serbian; `/contact`, and any address whose first segment
is not a configured prefix, is the default culture. No cookie and no `Accept-Language` header is read, so one address
always answers with the same page, for caches and crawlers alike.

The path is not rewritten: a page declares the route of each language itself, and `NavigationManager.Uri` keeps the
address the visitor asked for.

Components with an interactive render mode run in a circuit that the browser opens at `/_blazor`, an address without a
prefix, so inside the circuit the culture is the default one. Keep text that must follow the page's language in static
rendering, or pass the culture to the interactive component as a parameter.

## Configure the cultures

```json
{
  "Osirion": {
    "Localization": {
      "DefaultCulture": "en",
      "Cultures": [
        { "Name": "en", "DisplayName": "English" },
        { "Name": "sr-Latn", "Prefix": "sr" }
      ]
    }
  }
}
```

- `DefaultCulture` is required and is served at the root, without a prefix.
- Each entry of `Cultures` has a `Name` (a culture name the platform knows). `Prefix` is the first path segment;
  it defaults to the name in lower case, so `sr-Latn` gives `sr-latn`. `DisplayName` is what a language picker shows;
  it defaults to the culture's own name for itself, starting with a capital letter.
- List the default culture to give it a display name. When it is not listed it is added first, with the defaults.
  Either way its prefix (`/en/...`) redirects to the address without one.

`AddOsirion(builder.Configuration)` registers localization when the section exists. Without `AddOsirion`, call
`builder.Services.AddOsirionLocalization(builder.Configuration)`, or set the options in code with
`AddOsirionLocalization(options => ...)`.

The options are checked when the application starts. A missing `DefaultCulture`, a name that is not a culture, the
same culture twice, two cultures with one prefix, or a prefix that is not lower-case letters and digits in words joined
by hyphens stops startup with a message naming the key, for example
`Osirion:Localization:Cultures:1:Prefix 'sr' is the prefix of another culture.`

## Add the middleware in the right place

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddOsirion(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler("/error", createScopeForErrors: true);
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseOsirionLocalization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();
app.Run();
```

`UseOsirionLocalization` goes after `UseExceptionHandler` and `UseStatusCodePagesWithReExecute`, so the requests they
re-execute pass through it: a missing page under `/sr/` is answered by a Serbian not-found page, because the culture is
read from the address the visitor asked for. It goes before `MapRazorComponents` and anything else that reads the
culture.

It does two things:

1. It redirects (301) GET and HEAD requests whose prefix is not the canonical one: `/SR/contact` to `/sr/contact`, and
   the default culture's `/en/contact` to `/contact` (and `/en` to `/`). The path base and the query string are kept.
   Other methods are passed on, because a 301 turns a POST into a GET without its body.
2. It runs ASP.NET Core's request localization with the path as its only source. `CultureInfo.CurrentCulture` and
   `CultureInfo.CurrentUICulture` are the request's culture, and the response has a `Content-Language` header.

## Declare the localized routes

Give each page one route per language:

```razor
@page "/contact"
@page "/sr/contact"
@using System.Globalization

<h1>@(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "sr" ? "Kontakt" : "Contact")</h1>
```

A page that takes a parameter does the same: `@page "/blog/{Slug}"` and `@page "/sr/blog/{Slug}"`. Use resource files
or your own lookup for real translations; the culture is already set when the page renders.

## Write links in the visitor's language

Inject `IOsirionLocalePaths` and let it add the prefix:

```razor
@using Osirion.Blazor.Core.Localization
@inject IOsirionLocalePaths LocalePaths

<a href="@LocalePaths.LocalizeForCurrentCulture("/pricing")">Pricing</a>
```

- `LocalizeForCurrentCulture("/pricing")` gives `/sr/pricing` during a Serbian request and `/pricing` during an English
  one. It uses `CultureInfo.CurrentUICulture`, falling back through parent cultures (`sr-Latn-RS`, then `sr-Latn`, then
  `sr`) to a configured one, and to the default culture after that.
- `Localize("/pricing", culture)` gives the address in a given culture. A path that already has a prefix gets the new
  one instead; `/` gives `/sr`; the query string and fragment are kept.
- `WithoutLocale("/sr/pricing")` gives `/pricing`.
- `TryGetCulture(path, out var culture, out var remainder)` reads the prefix of a path.
- `Cultures` and `DefaultCulture` list the cultures with their prefix and display name.

Anything that is not a path on the site (an absolute URL, `//host`, `mailto:`, `tel:`, a relative path) is returned
unchanged, so you can pass every link through it.

## Add a language picker

`OsirionLanguagePicker` (Osirion.Blazor.Navigation) links the current page in each language:

```razor
@using Osirion.Blazor.Navigation.Components

<OsirionLanguagePicker AriaLabel="Language" />
```

It renders a `nav` with a list of plain links, so it works under static rendering without a script. Each link has
`hreflang` and `lang` set to the culture name, the current language has `aria-current="true"`, and the query string of
the current address is kept. With fewer than two cultures it renders nothing.

<!-- osirion:parameters OsirionLanguagePicker -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `AriaLabel` | `string` | `"Language"` | The accessible name of the navigation landmark. Defaults to `Language`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Language versions for search engines

With locale routing registered, `SeoMetadataRenderer` writes the `hreflang` links of each page (every culture plus
`x-default`; a content item links only its translations) and `/sitemap.xml` lists every language version with
`xhtml:link` alternates. A page written in Razor passes `Title`, `Description` and `CanonicalPath` to the renderer to
get the same head as a content page. See [SEO](/docs/seo), the
[SeoMetadataRenderer reference](/components/seo-metadata-renderer) and [Feeds and sitemap](/components/feeds-and-sitemap).

## Sites in a non-Latin script

Blazor encodes text with the default `HtmlEncoder`, which writes letters outside Basic Latin as character references
(`&#x41A;` for `К`). The page still shows the right letters, but the HTML is larger and harder to read. Allow every
script once in `Program.cs`:

```csharp
using System.Text.Encodings.Web;
using System.Text.Unicode;

builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
```

A site written in Serbian Cyrillic can also serve a Latin version without writing it twice: list `sr-Latn` here (its
prefix is `sr-latn`) and derive its content, shell and resource strings from `sr`, as
[Localization](/docs/localization#serve-a-second-script-without-a-second-file) shows.

## Check that it works

Run the site and request a few addresses, for example with `curl -I`:

- `/sr/contact` answers 200 with `Content-Language: sr-Latn`.
- `/contact` answers 200 with `Content-Language: en`.
- `/SR/contact` answers 301 with `Location: /sr/contact`, and `/en/contact` answers 301 with `Location: /contact`.
- `/sr/no-such-page` answers 404, and your not-found page renders with the Serbian culture.

## Next steps

- [Localization](/docs/localization): content in several languages and `hreflang` links
- [SEO](/docs/seo): `hreflang` links in the head and the sitemap, and the head of a page that is not content
- [Configure Osirion.Blazor](/docs/configuration): `Osirion:Site:Locales` and `Osirion:Site:Labels` for the site shell
- [Navigation components](/components/navigation)
