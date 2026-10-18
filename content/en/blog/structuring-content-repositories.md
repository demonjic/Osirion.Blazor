---
title: "Structuring Content Repositories"
author: "Osirion Team"
date: "2025-01-25"
last_modified: 2026-10-11
description: "Lay out a Markdown content repository for Osirion.Blazor: folders, _index.md, slugs and URLs, snake_case front matter, locales and drafts."
slug: "structuring-content-repositories"
categories: [CMS]
tags: [GitHub, Markdown, Front Matter, Content Structure]
featured_image: 'https://media.getosirion.com/og/blog-structuring-content-repositories.jpg'
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-structuring-content-repositories.jpg'
  title: "Structuring a Markdown Content Repository"
  description: "Folders, _index.md, slugs, front matter keys, locales and drafts in a content repository read by Osirion.Blazor."
  type: "Article"
---

# Structuring a Markdown content repository

Osirion.Blazor's CMS reads a folder of Markdown files, from a GitHub repository or from disk, and turns each file
into a page. The folder layout decides the URLs, and the front matter decides the title, the date, the listing and
whether the page is public at all. This article shows a layout that keeps URLs stable as the site grows, and the
rules the providers apply when they read it.

## A layout that works

This site's own content is a good model:

```text
content/
  en/
    home.md
    blog/
      _index.md
      structuring-content-repositories.md
      localization-for-blazor-content-sites.md
    docs/
      _index.md
      components/
        _index.md
        core/
          _index.md
          content/
            _index.md
            callout.md
```

- `content` is the provider's `ContentPath`.
- `en` is a locale folder. Keep one even on a single-language site; adding a second language later then needs no
  moves.
- Each section (`blog`, `docs`) is a folder. Group by topic rather than by date: a post's folder is part of its URL,
  and a URL with a year in it ages badly.
- Each folder that appears in navigation has an `_index.md`.
- Images live in the web app (`wwwroot/assets/images/...`) and pages refer to them by site path, such as
  `/assets/images/blazor-cover.png`.

The provider settings for this layout:

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
            "SupportedLocales": [ "en" ],
            "SupportedExtensions": [ ".md", ".markdown" ]
          }
        }
      }
    }
  }
}
```

## How a file becomes a URL

A content item's URL is its folder path below `ContentPath`, without the locale folder, followed by its slug.
`content/en/blog/structuring-content-repositories.md` becomes `blog/structuring-content-repositories`. Your routes
then decide where that URL is served; the example app routes `/blog/{slug}` and `/docs/{*slug}`.

The slug is the `slug` front matter key or, without one, the file name in lower case with spaces turned into hyphens
and other characters removed. A slug may contain only lowercase letters, digits and hyphens. A file whose `slug`
breaks that rule is not loaded (the GitHub provider keeps the version it loaded before, if any) and the error is
logged, so check the logs after adding content.

Choose slugs once. Renaming a file without a `slug` key changes its URL and breaks every link to it. Setting `slug`
explicitly keeps the URL when you rename the file.

`permalink` sets the URL of a folder when you write it in that folder's `_index.md`. In 4.1.0 it does not change the URL
of an ordinary content file; the release after 4.1.0 serves a file with a `permalink` at that address.

## Folder metadata in _index.md

`_index.md` describes the folder it sits in. The providers read these keys from its front matter:

```yaml
---
title: "Blog"
description: "Release notes and guides for Osirion.Blazor."
order: 2
lang: en
permalink: /blog
featured_image: /assets/images/blazor-cover.png
---
```

`title` names the folder in navigation, `order` sorts folders, and `permalink` gives the folder its URL.

## Front matter keys are snake_case

The front matter is read with an underscored naming convention, and keys the reader does not know are ignored
without an error. `featured_image` works; `featuredImage` is silently ignored. The keys a content file can use:

| Key | Meaning |
| --- | --- |
| `title` | Page title and heading. |
| `description` | Summary for lists and the meta description. |
| `author` | Author name; `ContentList` can filter by it from 4.1. |
| `date` | Publication date, `yyyy-MM-dd`, read as UTC. Lists sort by it. |
| `last_modified` | Date of the last meaningful change; from 4.0 it drives `dateModified` and the sitemap's `lastmod`. |
| `slug` | The last URL segment (see above). |
| `lang` | The item's locale. Without it, an item takes the locale of its folder, or `DefaultLocale` (the release after 4.1.0; 4.1.0 reads an item with front matter as `en`). |
| `id` | Localization id that pairs translations. |
| `categories`, `tags` | Lists of names. Their pages are `/category/{name}` and `/tag/{name}` by default, with the name in lower case and spaces as hyphens. |
| `featured_image` | Image for the page header, lists and social previews. |
| `is_featured` | Marks an item for featured lists. |
| `published` | `false` hides the item. |
| `order` | Position where a list or navigation sorts by order. |
| `series`, `series_order` | From 4.0: the series name and the part number, read by `ContentSeriesNavigation`. |
| `seo_properties` | Nested SEO overrides: `title`, `description`, `canonical`, `robots`, `og_image_url`, `json_ld` and more. |

A post's front matter with the keys most sites need:

```yaml
---
title: "Structuring content repositories"
description: "Folders, slugs and front matter for a Markdown content repository."
author: "Jane Doe"
date: "2026-10-11"
last_modified: "2026-10-11"
slug: "structuring-content-repositories"
lang: en
categories: [CMS]
tags: [Markdown, Front Matter]
featured_image: /assets/images/structure.png
---
```

Pick one spelling for each category and tag and reuse it. Copy the front matter of an existing post when you start a
new one; most mistakes are a misspelled key.

## Locales

Put each language in its own folder below `ContentPath` and list it in `SupportedLocales`. Translations with the same
relative path pair up on their own; translations with different file names need the same `id`. Set `lang` in every
file. [Localization for Blazor content sites](/blog/localization-for-blazor-content-sites) covers routes, translation
links and `hreflang`.

## Drafts

Add `published: false` to keep a file off the site. Lists, feeds and the sitemap leave it out, and its page answers
404. Since 4.0 a file without a `published` key is published, as in Jekyll and Hugo.

A draft is hidden from the site, not secret. In a public repository anyone can read it on GitHub. Keep unfinished work
that must stay private in a branch the provider does not read, or in a private repository.

## Repository hygiene

- Keep the content repository free of build output and large binaries: the GitHub provider reads the whole tree.
- Use pull requests for content changes, so a second person reviews the front matter before it goes live.
- Configure a signed webhook (`WebhookSecret`) so a merge refreshes the site without a restart.

## Related

- [Localization for Blazor content sites](/blog/localization-for-blazor-content-sites)
- [Blazor SEO checklist for static SSR content sites](/blog/blazor-seo-ssr-ssg-checklist)
- [ContentList](/components/content-list)
- [Series navigation](/components/series-navigation)
