---
id: 'cms-web-seo-and-feeds-overview'
order: 1
layout: docs
title: SEO and feeds
permalink: /components/seo-and-feeds
description: The Osirion.Blazor.Cms.Web pieces that tell search engines and feed readers about your content, SeoMetadataRenderer and the sitemap and feed endpoints.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-seo-and-feeds.jpg'
categories:
- Components
- CMS Web Components
- SEO
tags:
- blazor
- seo
- sitemap
- rss
- atom
- json-ld
published: true
slug: seo-and-feeds
lang: en
custom_fields:
  group: seo-and-feeds
seo_properties:
  title: 'Blazor SEO metadata, sitemap and feeds'
  description: 'SeoMetadataRenderer writes head metadata and JSON-LD for Blazor content pages; MapOsirionFeeds serves the sitemap, RSS and Atom feeds.'
  image: 'https://media.getosirion.com/og/components-seo-and-feeds.jpg'
  canonical: 'https://getosirion.com/components/seo-and-feeds'
  lang: en
  robots: index, follow
  og_title: 'SEO and feeds - Osirion.Blazor'
  og_description: 'Head metadata and JSON-LD for content pages, plus an XML sitemap, an RSS feed and an Atom feed.'
  og_image_url: 'https://media.getosirion.com/og/components-seo-and-feeds.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'SEO and feeds - Osirion.Blazor'
  twitter_description: 'Head metadata and JSON-LD for content pages, plus an XML sitemap, an RSS feed and an Atom feed.'
  twitter_image_url: 'https://media.getosirion.com/og/components-seo-and-feeds.jpg'
  meta_tags: {}
---

# SEO and feeds

Two parts of the `Osirion.Blazor.Cms.Web` package describe your published content to machines. One is a component
that writes into the page head. The other is a set of endpoints that serve the sitemap and the feeds. Both read the
same content items, so a title, description or date you set in front matter reaches the page, the sitemap and the
feeds.

| Part | What it does | Kind |
| --- | --- | --- |
| [SeoMetadataRenderer](/components/seo-metadata-renderer) | Writes the title, description, canonical URL, robots, Open Graph, Twitter Card and JSON-LD tags for a content item or a list. | Component, in the page head |
| [Feeds and sitemap](/components/feeds-and-sitemap) | Serves `/sitemap.xml`, an RSS feed at `/feed.xml` and an Atom feed at `/atom.xml`. Available from 4.0. | Endpoints, mapped with `MapOsirionFeeds` |

Neither has a live demo, because neither renders visible markup: the renderer would replace the title and tags of the
page it sits on, and the feeds are XML documents.

The site address matters for both. Set `SiteUrl` in `SeoMetadataOptions` for canonical URLs and in the feed options
for the sitemap and feeds (from the release after 4.1.0 the feeds fall back to the `SeoMetadataOptions` address), so that no forged `Host` header can put another host into what a crawler or cache keeps.

## Related

- [CMS pages](/components/cms-pages): the pages that already render `SeoMetadataRenderer`
- [Content components](/components/content): `ContentView` and `ContentList`
- [Blazor SEO checklist for SSR and SSG](/blog/blazor-seo-ssr-ssg-checklist)
