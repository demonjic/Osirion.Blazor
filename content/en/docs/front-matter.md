---
id: 'guide-front-matter'
order: 7
layout: docs
title: "Front matter reference"
permalink: /docs/front-matter
description: "Every front matter key Osirion.Blazor reads, with its type, default and effect, how dates and custom_fields work, and a complete annotated example."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-24
featured_image: 'https://media.getosirion.com/og/docs-front-matter.jpg'
categories:
- Documentation
- Guides
tags:
- front-matter
- yaml
- markdown
- seo
- content
published: true
slug: front-matter
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-front-matter.jpg'
  title: 'Osirion.Blazor front matter reference'
  description: 'The front matter keys a Markdown page can carry, their types, defaults and effects, plus seo_properties, dates, custom_fields and a full example.'
  image: 'https://media.getosirion.com/og/docs-front-matter.jpg'
  canonical: 'https://getosirion.com/docs/front-matter'
  robots: index, follow
---

# Front matter reference

Front matter is the block of YAML at the top of a Markdown file, between two lines that hold only `---`. This guide
lists every key Osirion.Blazor reads, what each does and what happens when it is missing. It is for anyone writing
pages for a site built with the library.

## Before you start

Read [How content is organized](/docs/content) first: it covers `slug`, `order`, `published` and `redirect_from` in
context. The keys below are the ones the YAML parser of Osirion.Blazor 4.1.0 maps onto its front matter model.

## Write the block

```markdown
---
title: "Hello, world"
date: 2026-10-17
---

The page text, in Markdown.
```

Keys are snake_case (`last_modified`, not `lastModified`). Unknown keys are ignored: the parser maps the YAML onto the
front matter model and skips what it does not know, without a warning. A typo in a key name therefore disappears
silently. To keep values of your own, put them under `custom_fields` (see [Keep your own values in custom_fields](#keep-your-own-values-in-custom_fields)).

A file with no front matter block still loads: its title is its file name, its language comes from its folder, its date
is the creation time of the file and every other value takes its default.

## Use the keys

| Key | Type | Default | Effect |
| --- | --- | --- | --- |
| `title` | string | none | The page's heading, its link text in lists and the base of the `<title>`. Without one the page takes its file name, and a folder's `_index.md` takes the folder's name. In 4.1.0 a file with a front matter block and no title does not load, so write one there. |
| `description` | string | empty | The meta description (cut to 160 characters), the summary in lists and the feed description. |
| `slug` | string | from the file name | The last part of the address. Lowercase letters, digits and hyphens only; anything else stops the file from loading. |
| `date` | date text | the file's creation time (the date of its last commit with a GitHub provider) | The publication date: list order, the feed `pubDate`, `datePublished`. In 4.1.0 a missing `date` is the day the content was loaded. See [Write dates the parser can read](#write-dates-the-parser-can-read). |
| `last_modified` | date text | `date` | When the page last changed: `dateModified`, `article:modified_time`, sitemap `lastmod`. See [Write dates the parser can read](#write-dates-the-parser-can-read). |
| `author` | string | empty | The byline, `article:author` and the author in structured data. |
| `categories` | list of strings | empty | Category pages and lists. The first one is `article:section`. |
| `tags` | list of strings | empty | Tag pages, the tag cloud, the meta keywords and `article:tag`. |
| `featured_image` | string | none | The card and hero image, and the Open Graph image when `og_image_url` is not set. |
| `is_featured` | boolean | `false` | Marks the page for the featured lists (`OnlyFeatured`, `FeaturedCount`). |
| `published` | boolean | `true` | `false` keeps the page out of lists, the sitemap, feeds, redirects and the page components. |
| `order` | integer | `0` | Position when a list sorts by order, and in documentation navigation. For an `_index.md`, the folder's position. |
| `id` | string | none | Pairs translations: items with the same `id` are translations of each other. With localization on, a missing `id` is built from the path without the language folder. |
| `lang` | string | the folder's locale, or `DefaultLocale` | The page's language: `og:locale`, `inLanguage`, the `Locale` filter of queries. A missing `lang` takes the locale of the page's language folder, or `DefaultLocale`; this arrives in the release after 4.1.0, and 4.1.0 reads a missing `lang` as `en`, even in a folder for another language. See the [localization guide](/docs/localization). |
| `redirect_from` | list of strings | empty | Earlier addresses that redirect to this page, with `UseOsirionRedirects`. |
| `series` | string | none | Pages with the same series name are listed together by `ContentSeriesNavigation`. |
| `series_order` | integer | none | The page's position in its series, starting at 1. |
| `permalink` | string | none | The address the page is served at, such as `/docs/deploy`; slashes at the ends are optional, no language folder is added, and placeholders such as `:year` are not expanded. An `_index.md` uses it as its folder's address. Arrives in the release after 4.1.0; 4.1.0 reads it but does not change the page's address. |
| `layout` | string | none | Stored on the item as `Metadata.Layout`. No library component picks a template from it; your own code can. |
| `custom_fields` | map | empty | Your own values. See [Keep your own values in custom_fields](#keep-your-own-values-in-custom_fields). |
| `seo_properties` | map | see [seo_properties](#control-search-and-social-tags-with-seo_properties) | Search and social overrides. |

Write lists as YAML lists, or inline as `[a, b]`. A single value where a list belongs (`tags: foo`), and a value of the
wrong type (`order: abc`, `published: maybe`), stop the file from loading. The provider logs the reason and goes on
with the other files.

A value that contains a colon, or starts with a character YAML reads as syntax, needs quotes:
`title: "Setup: step one"`.

## Write dates the parser can read

`date` and `last_modified` are read as text and parsed with the invariant culture. Use ISO form, with or without a
time:

```yaml
date: 2026-10-17
last_modified: 2026-10-17T09:30:00Z
```

A date without a time zone is taken as UTC. A page without a `date` key is not an error either: it gets the creation
time of its file (the date of the file's last commit with a GitHub provider, which costs one more request to GitHub for
each undated file the first time it is read), the same as a `date` that cannot be parsed and a file with no front matter
block. This arrives in the release after 4.1.0; in 4.1.0 a page with front matter and no `date` key gets the day the
content was loaded, so it changes on every restart, and the GitHub provider still does. Even with the fix, a file's
creation time changes with a fresh checkout or deployment, and the date of a last commit moves when the file is edited,
so an undated page can still jump to the top of newest-first lists and move its sitemap `lastmod` and feed date. Always
write `date`.

`last_modified` is what search engines and feed readers see as the change date. When it is missing, the page was
last modified on its `date`. The file's write time is not used, because a fresh checkout gives every file the time of
the checkout. If you edit files in place and want write times, set `UseFileWriteTimeAsLastModified` on the
FileSystem provider (it is off by default and applies only to that provider). Bump `last_modified` when you make a
change worth telling a crawler about.

## Keep your own values in custom_fields

```yaml
custom_fields:
  group: guides
  badge: new
  related:
  - /docs/content
  - /docs/seo
```

Read them from the item in code:

```razor
@using Osirion.Blazor.Cms.Domain.Entities

@if (Item.Metadata?.CustomFields.TryGetValue("group", out var group) == true)
{
    <span>@group</span>
}

@code {
    [Parameter, EditorRequired]
    public ContentItem Item { get; set; } = default!;
}
```

Values arrive as the YAML parser's plain objects: a scalar is always a `string`, even `5` or `true` (parse it
yourself), a list is a `List<object>` and a map is a `Dictionary<object, object>`. The library reads none of them.
The documentation site of Osirion.Blazor groups its sidebar by `custom_fields.group`.

## Control search and social tags with seo_properties

`seo_properties` overrides what [`SeoMetadataRenderer`](/components/seo-metadata-renderer) writes for the page. Every
key is optional.

| Key | Default | Effect |
| --- | --- | --- |
| `title` | the page `title` | The `<title>`, followed by a vertical bar and the site name. Never cut by the library. |
| `description` | the page `description` | The meta description, cut to 160 characters. |
| `canonical` | the page's own address | The `rel="canonical"` link, `og:url` and the address in structured data. Use it only when another address is the preferred one. |
| `robots` | `index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1` | The robots meta tag. A value with `noindex` also keeps the page out of the sitemap. |
| `og_title` | the page `title` | `og:title`, cut to 60 characters. |
| `og_description` | the meta description | `og:description`, cut to 200 characters. |
| `og_image_url` | `image`, then `featured_image`, then the site's `DefaultImageUrl` | `og:image`. A relative address becomes absolute. |
| `og_type` | from the schema type | `og:type`. Without it the first schema type decides: `article` for Article, BlogPosting and TechArticle, `profile`, `product`, `video.other`, otherwise `website`. |
| `twitter_card` | `summary_large_image` | `twitter:card`. |
| `twitter_title` | `og_title` | `twitter:title`, cut to 70 characters. |
| `twitter_description` | the meta description | `twitter:description`, cut to 200 characters. |
| `twitter_image_url` | the Open Graph image | `twitter:image`. |
| `type` | none (the page decides) | The schema.org type, used when the component is not given `SchemaTypes` and no `DefaultSchemaTypes` are configured. Write a `SchemaType` name, such as `TechArticle`; case does not matter, but another spelling counts as no `type`, without a message. |
| `json_ld` | none | Your own JSON-LD, as a JSON string. It replaces the generated data; text that is not a JSON object or array is ignored and a warning is logged. |
| `image` | none | The default social image: `og:image` and `twitter:image` use it when `og_image_url` is not set, before `featured_image`. |
| `lang` | the page locale | `og:locale`, for example `de-DE` is written as `de_DE`. The `lang` meta tag and the structured data language keep the page locale. |
| `meta_tags` | none | Extra meta elements, a map of name to content. A name starts with a letter and holds only letters, digits and `- _ . :` (at most 64 characters); other names are skipped with a logged warning, and so are the names the renderer writes itself (`description`, `keywords`, `author`, `generator`, `language`, `robots`, `googlebot`, `bingbot`, `referrer`). Values are HTML encoded. |

These defaults apply to every page. Without `type`, `SchemaTypes` or `DefaultSchemaTypes`, a page is a `WebPage` with
`og:type` `website`; `ArticlePage` writes `Article` instead. In 4.2.0 and earlier, a page with a front matter block got
`type: Article` unless it set another, so it said `og:type` `article`; set `type` on the pages that are articles when they
are not shown by `ArticlePage`.

The page components pick their own schema types (`DocumentPage` uses `TechArticle`, `ArticlePage` takes the types you
pass), so `type` matters mainly when you call the renderer yourself. See the [SEO guide](/docs/seo) for what the
renderer writes as a whole.

## Put it together

A complete page, with the effect of each key in a comment:

```yaml
---
id: 'guide-deploy'                  # pairs translations of this page
order: 4                            # position in an order-sorted list
layout: docs                        # stored on the item; your code may read it
title: "Deploy the site"            # heading, link text, base of the <title>
permalink: /docs/deploy             # the page's address (release after 4.1.0)
description: "Publish the site to a web host."   # meta description, summaries
author: Your Name                   # byline and structured data
date: 2026-10-17                    # publication date
last_modified: 2026-10-20           # change date for crawlers and feeds
featured_image: /images/deploy.png  # card, hero and Open Graph image
categories:
- Guides                            # category pages, article:section
tags:
- deployment                        # tag pages, keywords
is_featured: false                  # featured lists
published: true                     # false hides the page everywhere
slug: deploy                        # address: docs/deploy
series: Build a site                # series navigation
series_order: 3                     # position in the series
redirect_from:
- /docs/publish                     # redirects here with UseOsirionRedirects
lang: en                            # language of this page
custom_fields:
  group: guides                     # your own values
seo_properties:
  title: 'Deploy an Osirion.Blazor site'   # <title> before the site name
  description: 'Publish a site built with Osirion.Blazor to a web host.'
  canonical: 'https://example.com/docs/deploy'
  robots: index, follow
  og_title: 'Deploy the site'
  og_description: 'Publish the site to a web host.'
  og_image_url: /images/deploy-social.png
  og_type: article
  twitter_card: summary_large_image
  type: TechArticle
---
```

## Check that it works

Copy the block into a file in your content folder and open the throwaway `/content-check` page from the
[content guide](/docs/content). The page should be listed with the address you expect and the status `Published`. If
it is missing, read the application log for `Error processing markdown file`: the message follows with the line and column of the value
that could not be read, or the field that was refused (`Slug`, `Title`), and why. To see the tags a page writes, open it in the browser, choose View Source and look for
`<title>`, `rel="canonical"`, `og:` and `application/ld+json` in the head.

## Next steps

- [How content is organized](/docs/content): addresses, order, drafts and redirects
- [Localization](/docs/localization): `lang`, `id` and language folders
- [SEO](/docs/seo): what the library writes for search engines and what stays your job
- [SeoMetadataRenderer](/components/seo-metadata-renderer): the parameters behind `seo_properties`
- [CMS web package](/docs/packages/cms-web): the content components
