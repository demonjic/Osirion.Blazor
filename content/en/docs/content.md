---
id: 'guide-content'
order: 6
layout: docs
title: "How content is organized"
permalink: /docs/content
description: "How folders, _index.md and slug decide a page's address and place in lists, and how order, drafts and redirect_from work in Osirion.Blazor."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-content.jpg'
categories:
- Documentation
- Guides
tags:
- content
- markdown
- slug
- redirects
- drafts
published: true
slug: content
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-content.jpg'
  title: 'How Osirion.Blazor content is organized'
  description: 'Folders, _index.md, slug, order, drafts and redirect_from: how a Markdown file becomes a page address and an entry in lists and the sitemap.'
  image: 'https://media.getosirion.com/og/docs-content.jpg'
  canonical: 'https://getosirion.com/docs/content'
  robots: index, follow
---

# How content is organized

This guide explains how a Markdown file in your content folder becomes a page: where its address comes from, where it
appears in lists, how to hide a draft and how to move a page without breaking old links. It is for anyone adding
pages to a site built with Osirion.Blazor.

## Before you start

You need a site that reads a local content folder, as set up in [Getting started](/docs/getting-started). The examples
use `BasePath` set to `content`. Every address below is the one the library gives the item (`ContentItem.Url`); your
own routes decide which of them the site serves.

## Put files in folders

The provider reads every `.md` and `.markdown` file under `BasePath`, including subfolders:

```text
content/
  en/
    about.md
    blog/
      _index.md
      hello-world.md
    docs/
      _index.md
      install.md
```

Each folder becomes a directory named after the folder. A file called `_index.md` describes its folder and takes
these keys from its front matter: `title` (the directory's display name), `description`, `order`, `lang`,
`featured_image` and `permalink` (the address folder links use; the `_index.md` page itself stays at the folder's
path). Without a `title` the folder keeps its folder name and the other keys still apply; in 4.1.0 an `_index.md` with
front matter and no `title` applies none of them. Its `lang` is the locale of its folder unless you write it (in 4.1.0 it
is `en` unless you write it). The `_index.md` is also a page. Without a `title` its page takes the folder's name; in 4.1.0
a page with front matter and no `title` does not load, so write one. A folder without an `_index.md` keeps its folder name.

An `_index.md` is also a page of its own, at the folder's address. A list of that folder includes it, except on the
page it is shown on, so put real text in it or leave it out of lists you build yourself.

## Give each page a slug

A page's address is its folder path below `BasePath` plus its `slug`:

| File | Front matter | Address |
| --- | --- | --- |
| `blog/hello-world.md` | `slug: hello-world` | `blog/hello-world` |
| `docs/install.md` | no `slug` | `docs/install` |
| `blog/_index.md` | `slug: blog` | `blog` |

Without a `slug`, the file name decides: it is lowercased, spaces become hyphens, and anything but letters, digits and
hyphens is dropped, so `My Post.md` becomes `my-post` and `my_post.md` becomes `mypost`. Accents are removed and
Serbian letters are transliterated (`đ` becomes `dj`). The file name decides even when the page has a `title`. A
`slug` you write may contain only lowercase letters, digits and hyphens. Anything else (`Bad_Slug`) stops the file
from loading, and the error goes to the log. An `_index.md` uses its folder's path as its address, whatever its slug.
The exception is an `_index.md` with no folder above it, in the content root or, with localization on, directly in a
language folder: it has no path to use, so its address is its slug, which is `index` unless you set one.

A page with a `permalink` is served at that address. A leading or trailing slash is optional (`/custom/place` and
`custom/place` are the same address), the language folder is not added, and the value is used as written: placeholders
such as `:year` are not expanded. The address of the item, the sitemap, the feeds and the redirects all come from this
one value. An `_index.md` keeps its folder's address, and uses its `permalink` as the address folder links use. This
arrives in the release after 4.1.0; in 4.1.0 the `permalink` of a page is read but ignored, and a page keeps the folder
path and slug. To serve a page at another address in 4.1.0, route to it yourself, or pass a `ContentUrlFormatter` to the
components, the feeds and the redirects.

## Keep the language folder out of the address

If your files sit in language folders such as `content/en`, turn localization on and the first folder is dropped from
the address when it is one of `SupportedLocales`:

| File | `EnableLocalization` | Address |
| --- | --- | --- |
| `en/blog/hello-world.md` | `true` | `blog/hello-world` |
| `en/blog/hello-world.md` | `false` | `en/blog/hello-world` |

With localization off, the folder stays in the address, so a single-language site keeps its files in the content
root. Two translations of one page share one address, which the [localization guide](/docs/localization) deals with.

## Order pages

Lists show the newest page first, by `date`. To order by hand, give each page an integer `order` and sort by it:

```razor
@using Osirion.Blazor.Cms.Domain.Enums

<ContentList Directory="docs"
             SortBy="SortField.Order"
             SortDirection="SortDirection.Ascending" />
```

`Directory` takes a folder's name (`docs`) or its path below `BasePath` (`en/docs`), and lists the pages directly in
that folder, not those of its subfolders. In 4.1.0 it matches only the folder's display name, which is the `title` of
its `_index.md` when it has one; the release after 4.1.0 also accepts the folder name and path.

A page without `order` has 0 and comes first in an ascending list. The documentation components order by `order`
ascending without being asked, including `DocumentPage` and the previous and next links of
[`OsirionContentNavigation`](/components/osirion-content-navigation). A directory takes its `order` from its
`_index.md`.

## Hide a draft

Add `published: false` to the front matter. Lists, the sitemap and feeds, the redirect map and the page components
leave the page out, so its address answers like an unknown one. A file without a `published` key is published.

## Move a page without breaking its old address

List the earlier addresses under `redirect_from` in the page that moved:

```yaml
redirect_from:
- /docs/old-name
- /blog/2025/old-name
```

Then add the redirect middleware in `Program.cs`, before the endpoints that render pages:

```csharp
using Osirion.Blazor.Cms.Web.Redirects;

app.UseOsirionRedirects();
```

A GET or HEAD request for an old address gets a 301 to the page's current address (`/` plus `Url`). Things to know:

- The map is built from published content and kept for five minutes (`CacheDuration`), so a new `redirect_from` can
  take that long to start working.
- A page that exists at the old address wins over a redirect away from it. The redirect is ignored and a warning is
  logged.
- If two pages list the same old address, the first one found wins and a warning is logged.
- For pages that are not content items, add fixed redirects, and pass `ContentUrlFormatter` when your site routes
  content somewhere other than `/{Url}`:

```csharp
app.UseOsirionRedirects(redirects =>
{
    redirects.Redirects["/docs/components/core"] = "/components";
    redirects.Permanent = true;
});
```

Set `Permanent` to `false` for a 302 while you are unsure about the move.

## Show live demos in a page

A fenced code block whose language is `demo` renders a registered Razor component in its place. It needs a
registration in `Program.cs`, so it is covered on the [OsirionContentDemo](/components/content-demo) page.

## Check that it works

Add a throwaway page that lists what the provider loaded, and open `/content-check`:

```razor
@page "/content-check"
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Domain.Repositories
@using Osirion.Blazor.Cms.Domain.Services
@inject IContentProviderManager Content

<ul>
    @foreach (var item in items)
    {
        <li>@item.Path to /@item.Url (@item.Status, @item.Locale)</li>
    }
</ul>

@code {
    private IReadOnlyList<ContentItem> items = [];

    protected override async Task OnInitializedAsync() =>
        items = await Content.GetContentByQueryAsync(new ContentQuery { IncludeUnpublished = true });
}
```

Each line shows a file, its address and whether it is published. Delete the page afterwards: it lists drafts.

If a file is missing from the list, the front matter could not be read. The provider logs
`Error processing markdown file` with the reason and carries on. The usual causes are a `slug` with a character that is
not allowed, in 4.1.0 a missing `title` when the file has front matter, a list key such as `tags` written as one value
instead of a list, and a value of the wrong type such as `order: abc` or `published: maybe`. A `date` that cannot be
read is not an error: the page gets the creation time of its file, which a fresh checkout changes (see the
[front matter reference](/docs/front-matter)). A file that is empty or only white space is skipped without a message.

## Next steps

- [Front matter reference](/docs/front-matter): every key a page can carry
- [Localization](/docs/localization): language folders and translations
- [SEO](/docs/seo): canonical addresses, sitemap and feeds
- [ContentList](/components/content-list): sort, filter and page a list
- [CMS web package](/docs/packages/cms-web): the content components
