---
title: "The Osirion.Blazor CMS Components"
author: "Dejan Demonjić"
date: "2025-05-21"
last_modified: 2026-10-11
description: "What each Osirion.Blazor CMS component renders, from content lists and article pages to tags, categories and search, for Markdown kept in GitHub."
tags: [GitHub, CMS, Components, Markdown]
categories: [Documentation]
slug: "github-cms-components"
is_featured: false
featured_image: 'https://media.getosirion.com/og/blog-github-cms-components.jpg'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-github-cms-components.jpg'
---

# The Osirion.Blazor CMS Components

The Osirion.Blazor CMS reads Markdown files with YAML front matter from a GitHub repository (or a folder on disk) and
renders them with Blazor components. The components live in `Osirion.Blazor.Cms.Web` and render on the server, so lists,
articles, tag pages and search results are complete HTML under static SSR. This article is a tour of what each one
does and how they fit together. For a step-by-step build, see
[Build a blog with GitHub as the CMS](/blog/github-cms-with-blazor).

## Where the content comes from

With the meta-package, `AddOsirion(builder.Configuration)` registers a GitHub provider for every section under
`Osirion:Cms:Web:GitHub`:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "GitHub": {
          "Blog": {
            "Owner": "your-github-user",
            "Repository": "your-content-repo",
            "Branch": "main",
            "ContentPath": "content",
            "IsDefault": true
          }
        }
      }
    }
  }
}
```

The provider reads the repository through the GitHub API, keeps the content in memory, and on a refresh downloads only
the files that changed. It needs a token that can read the repository, even a public one: set `ApiToken` from user
secrets or your host's secret store, never in `appsettings.json`. Also call `builder.Services.AddJSComponents()`, which
registers the script loader that `OsirionStyles` and the article renderer use.

To read Markdown from disk instead, configure `Osirion:Cms:Web:FileSystem` with a `BasePath`; the components work the
same with either provider.

Each Markdown file becomes a content item. Its front matter keys are snake_case:

```markdown
---
title: "Hello, Blazor"
description: "The first post."
author: "Jane Developer"
date: 2026-10-01
slug: hello-blazor
categories: [Tutorials]
tags: [blazor, ssr]
featured_image: /images/hello.png
is_featured: true
---

The post body in Markdown.
```

`published` defaults to `true`; set `published: false` to keep a file off the site. An item's URL is its folder path
below `ContentPath` followed by its slug, so the file above in `content/blog/` is served at `blog/hello-blazor`.

Add `@using Osirion.Blazor.Cms.Web.Components` to `_Imports.razor` for the components below.

## ContentList

`ContentList` queries the default provider and renders the matching items as a grid of cards with the featured image,
title, excerpt and metadata. It filters by `Directory`, `Category`, `Tag` and `OnlyFeatured`, sorts with `SortBy` and
`SortDirection` (newest first by default), and pages with `ShowPagination` and `ItemsPerPage`:

```razor
<ContentList Directory="blog" ShowPagination ItemsPerPage="12" />
```

Pagination uses plain links with a `page` query parameter and keeps the other parameters in the URL, so it works without
script and every page has its own address. The list also writes collection metadata into the page head: the page title, a
canonical URL and structured data, plus previous and next links when paginated. Turn that off with
`EnableSeoMetadata="false"` if the page writes its own.

Available from 4.0: `Author` lists the items of one author.

## ArticlePage and ContentView

`ArticlePage` is a complete article page: a `HeroSection` built from the front matter, breadcrumbs, the rendered
content, a sidebar with a table of contents, categories and tags, previous and next links, and related items. It finds
the item by `ItemSlug`, `ItemUrl` or `ItemPath`, and answers a missing item with HTTP 404:

```razor
<ArticlePage DirectoryName="blog" ItemSlug="@Slug" />
```

Most parts can be switched off, for example `ShowSidebar`, `ShowTableOfContents`, `ShowBreadcrumbs` and `ShowHero`, and
the hero takes its options through `Hero*` parameters such as `HeroVariant`.

`ContentView` is the smaller building block: it renders one item, given as `Item` or loaded by `Path`, with its header
and content, and leaves the page layout to you.

The Markdown is rendered to HTML on the server and sanitized with an allow-list before it is written to the page:
scripts, event handler attributes, `javascript:` URLs, forms, iframes and inline styles are removed.

## CategoriesList and TagCloud

Both load every category or tag from the default provider and render them as links with item counts:

```razor
<CategoriesList Title="Categories" />
<TagCloud Title="Tags" MaxTags="20" />
```

Links go to `/category/{slug}` and `/tag/{slug}` unless you pass `CategoryUrlFormatter` or `TagUrlFormatter`.
`ShowCount` and `SortByCount` are on by default; `ActiveCategory` and `ActiveTag` highlight the current one. A matching
page is one `ContentList` with the filter:

```razor
@page "/tag/{TagSlug}"

<ContentList Tag="@TagSlug" ShowPagination />

@code {
    [Parameter]
    public string? TagSlug { get; set; }
}
```

The category filter treats hyphens in the URL as spaces, so `/category/getting-started` finds "Getting Started". The tag
filter compares the value with each whole tag, ignoring case but not spaces, while `TagCloud` turns spaces into hyphens
in its links. Write multi-word tags with hyphens instead of spaces (`static-ssr`, not `static ssr`) and the links will
match.

## SearchBox

`SearchBox` is a GET form that sends the query to `/search?q=...`; `ActionUrl` and `QueryParameterName` change both
parts. It needs no script. On the results page, pass the query to `ContentList`:

```razor
@page "/search"

<SearchBox SearchQuery="@Query" />

@if (!string.IsNullOrWhiteSpace(Query))
{
    <ContentList SearchQuery="@Query" ShowPagination NoContentText="Nothing matched your search." />
}

@code {
    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }
}
```

Without a query, `ContentList` lists every item, which is why the example renders it only when there is one. The search
runs in memory over the loaded items and matches any of the words in the title, description, content, categories or
tags.

## DirectoryNavigation

`DirectoryNavigation` renders a list of folders as links, with item counts and nested subfolders. It does not load
anything itself: pass the folders through `Directories`, and use `CurrentDirectory` to highlight the active one.

## Keeping content fresh

Register the webhook services with `builder.Services.AddGitHubWebhookAndPolling()` and add `app.UseGitHubWebhook()`, then
point a GitHub webhook at `POST /api/github/webhook`. Deliveries must be signed with the provider's `WebhookSecret`
(keep it in user secrets or your host's secret store); the endpoint rejects every delivery while no secret is set, and
refreshes only the providers whose owner, repository, branch and secret match the delivery.

## Related

- [CMS web components](/docs/packages/cms-web)
- [CMS navigation components](/components/cms-navigation)
- [Build a blog with GitHub as the CMS](/blog/github-cms-with-blazor)
