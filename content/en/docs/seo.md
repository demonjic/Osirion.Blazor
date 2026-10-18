---
id: 'guide-seo'
order: 9
layout: docs
title: "SEO"
permalink: /docs/seo
description: "What Osirion.Blazor writes for search engines (metadata, canonical URLs, structured data, sitemap, feeds, redirects) and what your site must still do."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-seo.jpg'
categories:
- Documentation
- Guides
tags:
- seo
- sitemap
- rss
- canonical
- structured-data
- robots
published: true
slug: seo
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-seo.jpg'
  title: 'SEO for Osirion.Blazor sites'
  description: 'The tags, sitemap, feeds and redirects Osirion.Blazor writes, what stays your job, and a checklist for a new site. No ranking promises.'
  image: 'https://media.getosirion.com/og/docs-seo.jpg'
  canonical: 'https://getosirion.com/docs/seo'
  robots: index, follow
---

# SEO

This guide lists what Osirion.Blazor writes so search engines can read your pages, and what the site itself has to do.
It is for anyone launching a content site. Nothing here promises a ranking: the library makes pages readable and
consistent, and search engines decide the rest.

## Before you start

You need a site with content pages, as built in [Getting started](/docs/getting-started), and a public address for it
(`https://example.com` below). The pages should render with static server-side rendering, so crawlers get the finished
HTML.

## Know what the library writes

| Output | Written by | Notes |
| --- | --- | --- |
| `<title>`, meta description, robots, canonical, Open Graph, Twitter Card, JSON-LD | [`SeoMetadataRenderer`](/components/seo-metadata-renderer) | `ArticlePage`, `DocumentPage`, `ContentList` and the other page components already render it |
| `hreflang` links | `SeoMetadataRenderer` | Only from the `AlternateLanguageUrls` you pass; see [Localization](/docs/localization) |
| `/sitemap.xml`, `/feed.xml` (RSS 2.0), `/atom.xml` | `MapOsirionFeeds` | From published content; available since 4.0 |
| 301 redirects from old addresses | `UseOsirionRedirects` | From `redirect_from` front matter; available since 4.0 |
| `/robots.txt` | `UseOsirionRobotsGenerator`, optional | See [Serve a sitemap, feeds and robots.txt](#serve-a-sitemap-feeds-and-robots.txt); a static file works in every release |

What the library does not do: write your titles and descriptions, choose your addresses, set your public host, submit the
sitemap to a search engine, or measure anything. That is the site's job.

## Set the site's identity

In `appsettings.json`, under `Osirion:Seo`, which `AddOsirion(builder.Configuration)` reads:

```json
{
  "Osirion": {
    "Seo": {
      "SiteName": "My Blog",
      "SiteDescription": "Notes, guides and ideas, written in Markdown.",
      "OrganizationName": "My Company",
      "SiteLogoUrl": "/logo.png",
      "DefaultImageUrl": "/assets/images/cover.png"
    }
  }
}
```

`SiteName` follows the page title (`Page title | My Blog`) and is the name of the publisher in structured data;
`OrganizationName` takes its place only when `SiteName` is missing, and with neither the name is taken from the host.
`SiteLogoUrl` is the publisher logo (`/logo.png` on your site when unset, whether or not that file exists), and
`DefaultImageUrl` is the share image for pages that have no `og_image_url`, `image` or `featured_image`. The other options and the parameters that
override them are on the [SeoMetadataRenderer](/components/seo-metadata-renderer) page.

## Write a title and description for every page

The renderer takes them from front matter. Write both, and keep the description to a sentence or two (it is cut to 160
characters):

```yaml
title: "Deploy the site"
description: "Publish a site built with Osirion.Blazor to a web host."
seo_properties:
  title: 'Deploy an Osirion.Blazor site'
```

`seo_properties.title` replaces the title in the `<title>` only. The [front matter reference](/docs/front-matter)
lists every override. The library never cuts the `<title>`; search engines shorten a title link themselves, so keep it
short enough to read.

## Declare the public address

The canonical URL is the address the page was requested at, without query string and fragment, so tracking parameters such
as `utm_source` never make a second canonical. In 4.1.0 that address uses the scheme and host your app sees. Behind a
proxy or on a preview host, make the app see the public scheme and host (ASP.NET Core forwarded headers), or set
`seo_properties.canonical` on the page.

The release after 4.1.0 adds `Osirion:Seo:SiteUrl`: when it is an absolute `http` or `https` address, the canonical URL,
`og:url` and the addresses in structured data use it, whatever host served the request.

Set `canonical` in front matter only when another address is the preferred one. A wrong canonical tells search engines to
ignore the page.

## Serve a sitemap, feeds and robots.txt

Map the endpoints in `Program.cs`, after the content services are registered:

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds => app.Configuration.GetSection("Feeds").Bind(feeds));
```

and describe them in `appsettings.json`:

```json
{
  "Feeds": {
    "SiteUrl": "https://example.com",
    "Title": "My Blog",
    "Description": "Notes, guides and ideas, written in Markdown.",
    "FeedDirectory": "blog",
    "AdditionalSitemapUrls": [ "/", "/blog" ]
  }
}
```

The `Feeds` section name belongs to the templates; any name works, or set the properties in code. The options of
`OsirionFeedOptions`:

| Option | Default | Effect |
| --- | --- | --- |
| `SiteUrl` | `Osirion:Seo:SiteUrl`, then the request's address | Public address in every URL. Set one of them in production, so a forged `Host` header cannot change the URLs a cache stores. The `Osirion:Seo:SiteUrl` fallback arrives in the release after 4.1.0; in 4.1.0 set this option. |
| `Title`, `Description` | `Osirion:Seo` `SiteName` and `SiteDescription` | Feed title and description. |
| `FeedDirectory` | all content | The folder the feeds list, for example `blog`: the pages directly in it, not those of its subfolders. In 4.1.0 it matches the folder's display name (the `title` of its `_index.md` when it has one); the release after 4.1.0 also accepts the folder name. |
| `FeedLocale` | every language | The language the feeds list. |
| `FeedItemCount` | `20` | How many of the newest items the feeds list. |
| `AdditionalSitemapUrls` | none | Addresses that are not content items, such as `/` and `/blog`. |
| `ContentUrlFormatter` | `/{item.Url}` | The address of an item, when your routes differ. |
| `CacheDuration` | 10 minutes | `Cache-Control: public, max-age` of each response. |
| `EnableSitemap`, `EnableRss`, `EnableAtom` | `true` | Turn one endpoint off. |

What the sitemap holds: `AdditionalSitemapUrls`, then every published content item (at most 50,000, with no sitemap
index), each with `lastmod` from `last_modified`, or from `date` when that is missing. An item whose
`seo_properties.robots` contains `noindex` is left out, and so are drafts. It lists every item the provider loaded,
including each folder's `_index.md`, whether or not your site routes that address, so set `ContentUrlFormatter` when a
page lives elsewhere, as a home page at `/` does. The feeds carry descriptions, not article bodies. See
[Feeds and sitemap](/components/feeds-and-sitemap) for the document formats.

For `robots.txt`, put a plain file at `wwwroot/robots.txt` and point it at the sitemap:

```text
User-agent: *
Allow: /

Sitemap: https://example.com/sitemap.xml
```

The library can also answer `/robots.txt` (`app.UseOsirionRobotsGenerator()`) from a content item at the address
`robots`, or with a fixed `User-agent: *` and `Disallow: /search/` when there is none. From the release after 4.1.0 it
answers with the item's source text, so a `robots.md` that holds plain robots rules is served as those rules. In 4.1.0 it
answers with the item's rendered HTML (`<p>User-agent: *</p>`) under a `text/plain` type, which crawlers cannot use, so on
4.1.0 use the static file. `robots.txt` also does not keep a page out of search results; use `robots: noindex` in
`seo_properties` for that.

Let browsers and feed readers find the feeds from your layout, in `App.razor`:

```razor
<head>
    <link rel="alternate" type="application/rss+xml" title="Example blog" href="/feed.xml" />
    <link rel="alternate" type="application/atom+xml" title="Example blog" href="/atom.xml" />
    <HeadOutlet />
</head>
```

## Redirect moved pages

List old addresses in `redirect_from` and register `app.UseOsirionRedirects()` before the endpoints that render pages,
as described in [How content is organized](/docs/content). A 301 tells search engines the page moved for good.

## Know the structured data

JSON-LD is chosen by the `SchemaTypes` parameter, else `seo_properties.type`, else `WebPage`. A page with a front matter
block has `type: Article` unless it sets another, so it gets `Article`; a file with no front matter gets `WebPage`.
The name is read ignoring case (`techarticle` is `TechArticle`); an unrecognized name gives `WebPage`. The generated types are
`Article`, `BlogPosting`, `TechArticle`, `WebPage`, `AboutPage`, `ContactPage`, `ProfilePage`, `SoftwareApplication`,
`Product`, `Organization`, `Person`, `LocalBusiness`, `Event`, `Recipe`, `Course`, `Review`, `VideoObject`, `WebSite`,
`BreadcrumbList` and `CollectionPage`, plus `HowTo`. With one type and `GenerateMultipleSchemas` on (the default), a
`BreadcrumbList` is added. To replace the data with your own, put a JSON string in `seo_properties.json_ld`.

Google no longer shows FAQ or HowTo rich results, so treat neither as an SEO feature. `HowTo` is only schema.org
markup. `FAQ` as a schema type writes nothing, and `EnableGeneratedFaq`, which is off, builds FAQ data from your title
and description rather than from real questions; leave it off. Use FAQ markup, if at all, as content structure for readers.

## Check that it works

Run the site and request the endpoints:

```bash
curl -s https://localhost:5001/sitemap.xml
curl -s https://localhost:5001/feed.xml
curl -sI https://localhost:5001/old-address
```

Use your own port. The sitemap is a `urlset` with one `url` per page, every `loc` starting with your `SiteUrl`. The feed
is an RSS `channel` with your newest items. The last request should answer `301` with a `Location` header when
`old-address` is listed in `redirect_from`. Open a page, choose View Source and find, in the head, one `<title>`, one
`rel="canonical"`, the robots meta tag and a `script` of type `application/ld+json`. If every `loc` shows `localhost`,
`SiteUrl` is not set. If a page is missing from the sitemap, it is a draft or its `robots` holds `noindex`.

## Checklist for a new site

- [ ] `Osirion:Seo` has `SiteName`, `SiteDescription`, `SiteLogoUrl` and `DefaultImageUrl`.
- [ ] Every page has a `title`, a one or two sentence `description` and a `date`; changed pages carry `last_modified`.
- [ ] The app sees the public scheme and host, so canonical URLs are right; or `canonical` is set where it matters.
- [ ] `MapOsirionFeeds` is mapped with `SiteUrl`, and `/sitemap.xml` lists the pages you expect and none you do not.
- [ ] `wwwroot/robots.txt` exists and names the sitemap.
- [ ] Pages you moved have `redirect_from`, and `UseOsirionRedirects` runs before the page endpoints.
- [ ] Draft pages have `published: false`; pages that must stay out of search have `robots: noindex`.
- [ ] A multilingual site passes `AlternateLanguageUrls` and uses language-aware addresses.
- [ ] The sitemap address is submitted in your search engine's webmaster tools: the library does not do it.
- [ ] Each page renders its content in the server's HTML (static server-side rendering), not after a script runs.

## Next steps

- [SeoMetadataRenderer](/components/seo-metadata-renderer): every parameter and what is read from the page
- [Feeds and sitemap](/components/feeds-and-sitemap): the options and documents in detail
- [Front matter reference](/docs/front-matter): `seo_properties` and dates
- [Localization](/docs/localization): `hreflang` links
- [Blazor SEO checklist for SSR and SSG](/blog/blazor-seo-ssr-ssg-checklist)
