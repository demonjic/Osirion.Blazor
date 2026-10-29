---
order: 2
title: Feeds and Sitemap
description: "Serve /sitemap.xml, an RSS feed at /feed.xml and an Atom feed at /atom.xml from your published content with MapOsirionFeeds and OsirionFeedOptions."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/components-feeds-and-sitemap.jpg'
categories:
- Components
- CMS Web Components
- Core Components
tags:
- sitemap
- rss
- atom
- feeds
- seo
permalink: /components/feeds-and-sitemap
slug: feeds-and-sitemap
published: true
redirect_from:
- /docs/components/cms-web/core/feeds-and-sitemap
custom_fields:
  group: seo-and-feeds
seo_properties:
  title: 'Feeds and sitemap: sitemap.xml, RSS and Atom for Blazor'
  description: 'Serve /sitemap.xml, an RSS feed at /feed.xml and an Atom feed at /atom.xml from your published Blazor content with MapOsirionFeeds.'
  image: 'https://media.getosirion.com/og/components-feeds-and-sitemap.jpg'
  canonical: 'https://getosirion.com/components/feeds-and-sitemap'
  lang: en
  robots: index, follow
  og_title: 'Feeds and sitemap - Osirion.Blazor'
  og_description: 'MapOsirionFeeds serves an XML sitemap, an RSS feed and an Atom feed built from your published content.'
  og_image_url: 'https://media.getosirion.com/og/components-feeds-and-sitemap.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Feeds and sitemap - Osirion.Blazor'
  twitter_description: 'MapOsirionFeeds serves an XML sitemap, an RSS feed and an Atom feed built from your published content.'
  twitter_image_url: 'https://media.getosirion.com/og/components-feeds-and-sitemap.jpg'
  meta_tags: {}
---

# Feeds and sitemap

`MapOsirionFeeds` maps three endpoints built from the published content of your content providers: an XML sitemap
at `/sitemap.xml`, an RSS 2.0 feed at `/feed.xml` and an Atom 1.0 feed at `/atom.xml`. Search engines read the
sitemap; feed readers subscribe to either feed. These are endpoints, not components, so this page has no demo and no
parameter table.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Feeds`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Feeds/ContentFeedEndpoints.cs). Available from 4.0.

## Usage

Call it in `Program.cs` after the content services are registered:

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://example.com";
    feeds.FeedDirectory = "blog";
    feeds.AdditionalSitemapUrls = ["/", "/blog"];
});
```

With no arguments, `app.MapOsirionFeeds()` maps all three endpoints over all content and builds URLs from the
request.

| Path | Format | Content |
| --- | --- | --- |
| `/sitemap.xml` | sitemaps.org XML sitemap | `AdditionalSitemapUrls`, then every published item (the first 50,000 items a provider returns) |
| `/feed.xml` | RSS 2.0 | The newest `FeedItemCount` items by creation date, within `FeedDirectory` and `FeedLocale` |
| `/atom.xml` | Atom 1.0 | The same items as the RSS feed |

## Configuration

`OsirionFeedOptions` has these properties:

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `SiteUrl` | `string?` | `null` | Public address used for every absolute URL, for example `https://example.com`. When empty, `Osirion:Seo:SiteUrl` is used (from the release after 4.1.0), and when that is empty too, the request's scheme, host and path base. |
| `Title` | `string?` | `null` | Feed title; otherwise the SEO `SiteName`, then the host name. |
| `Description` | `string?` | `null` | Feed description; otherwise the SEO `SiteDescription`, then the title. |
| `FeedDirectory` | `string?` | `null` | Directory the feeds cover, for example `blog`; all content when empty. |
| `FeedLocale` | `string?` | `null` | Locale the feeds cover, for example `en`; every locale when empty. Also written as the RSS `language`. |
| `FeedItemCount` | `int` | `20` | How many of the newest items the feeds list. |
| `AdditionalSitemapUrls` | `IList<string>` | empty | Paths that are not content items but belong in the sitemap, such as `/` or `/blog`. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds an item's path or absolute URL; default `/{item.Url}`. |
| `CacheDuration` | `TimeSpan` | 10 minutes | `Cache-Control: public, max-age` of every response. |
| `EnableSitemap` | `bool` | `true` | Maps `/sitemap.xml`. |
| `EnableRss` | `bool` | `true` | Maps `/feed.xml`. |
| `EnableAtom` | `bool` | `true` | Maps `/atom.xml`. |

The options are checked when `MapOsirionFeeds` runs: a `SiteUrl` that is not an absolute `http` or `https` URL
throws `ArgumentException`, and a `FeedItemCount` below 1 throws `ArgumentOutOfRangeException`, so a bad value stops
the application at startup instead of producing broken feeds.

## What goes into each document

- **Sitemap.** Each URL has a `lastmod` from the item's `last_modified` front matter, then, with the file system
  provider and `FileSystemOptions.UseFileWriteTimeAsLastModified` turned on (it is off by default), the file's write
  time, then the item's creation date: its `date` front matter, or for a file the file's creation time. Set
  `last_modified` when dates must survive a fresh checkout or a new deployment. Items whose `seo_properties.robots` contains `noindex` are left out, and duplicate URLs are written
  once. The sitemap covers all content; `FeedDirectory` and `FeedLocale` do not apply to it.
- **Language versions.** On a site with [locale routing](/docs/locale-routing) (`IOsirionLocalePaths` registered),
  the sitemap follows Google's format for localized versions: `urlset` declares
  `xmlns:xhtml="http://www.w3.org/1999/xhtml"`, and a `url` with other language versions lists every version,
  itself included, as `<xhtml:link rel="alternate" hreflang="sr-Latn" href="..."/>` (the culture name), then
  `x-default` with the default culture's address. Each `AdditionalSitemapUrls` path is listed once per configured
  culture, at the address `IOsirionLocalePaths.Localize` gives it (`/contact` and `/sr/contact`); an absolute URL
  there is listed once, without alternates. Each content item is listed at its address in its own culture (the `sr`
  item at `/sr/blog/post`), and the items with the same `ContentId` (the localization id) in configured cultures are
  each other's alternates; a culture without a translation, and a translation whose `robots` holds `noindex`, is not
  linked, and a single version gets no alternates. An item whose locale is not a configured culture keeps its address
  and gets no alternates. Without locale routing the sitemap is unchanged.
- **RSS.** Each item has its title, link, a permalink `guid`, `pubDate` from the creation date (`date`) and the
  `description` front matter. The channel carries an `atom:link rel="self"` pointing to `/feed.xml`.
- **Atom.** Each entry has an id (its URL), title, link, `published` (the creation date), `updated` (the last-modified
  date, or the creation date), the author when set, and the description as its summary. The feed's own `updated` is
  the newest entry's, or the current time for an empty feed. The feed-level author is the feed title, which satisfies
  Atom's rule that every entry has an author.
- Feeds carry descriptions, not full article bodies. Unpublished items (`published: false`) never appear, and a
  feed lists items by creation date even when a provider ignores the sort order.
- Characters that XML cannot represent are dropped from titles and descriptions rather than failing the request.

## Security and caching

Set `SiteUrl`, or `Osirion:Seo:SiteUrl` from the release after 4.1.0, in production. Without either, URLs follow whatever `Host` header a request sends, so a forged header could
put another host into a response that a cache or crawler stores. Responses are marked public, so browsers and
shared caches may keep serving a document for up to `CacheDuration` after the content changes.

## Pointing crawlers and readers at them

Add the sitemap to your `robots.txt`:

```text
Sitemap: https://example.com/sitemap.xml
```

Let browsers and feed readers discover the feeds from the page head, for example in `App.razor`:

```html
<link rel="alternate" type="application/rss+xml" title="Example blog" href="/feed.xml" />
<link rel="alternate" type="application/atom+xml" title="Example blog" href="/atom.xml" />
```

## Related

- [SeoMetadataRenderer](/components/seo-metadata-renderer): head metadata; its `SiteName` and
  `SiteDescription` options also title the feeds
- [ContentList](/components/content-list): the list pages whose links the feeds share
- [CMS Web core components](/components/cms-pages)
