---
order: 7
title: DocumentPage
permalink: /components/document-page
description: "DocumentPage renders a documentation page from one content item: side navigation, breadcrumbs, table of contents, body, edit link and previous and next links."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-document-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- document-page
- cms
- documentation
- navigation
- static-ssr
slug: document-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'DocumentPage: a documentation page for Blazor'
  description: 'DocumentPage renders a documentation page from one CMS content item with side navigation, a table of contents, an edit link and a pager.'
  image: 'https://media.getosirion.com/og/components-document-page.jpg'
  canonical: 'https://getosirion.com/components/document-page'
  lang: en
  robots: index, follow
  og_title: 'DocumentPage - Osirion.Blazor'
  og_description: 'A documentation page from one content item: side navigation, breadcrumbs, table of contents, edit link and previous and next links.'
  og_image_url: 'https://media.getosirion.com/og/components-document-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'DocumentPage - Osirion.Blazor'
  twitter_description: 'A documentation page from one content item: side navigation, table of contents, edit link and pager.'
  twitter_image_url: 'https://media.getosirion.com/og/components-document-page.jpg'
  meta_tags: {}
---

# DocumentPage

`DocumentPage` renders a documentation page from one content item: a side navigation, breadcrumbs, the title with the
author and last update, a table of contents, the body, an edit link and previous and next links. Use it for
documentation, guides and changelogs. The documentation pages of this site, including the one you are reading, are
rendered by it.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/DocumentPage.razor).

There is no live demo on this page, because this page is the demo. `DocumentPage` writes the page's title, canonical
address and structured data into the head and makes the whole response a 404 when a document is missing, so it
cannot sit inside another page.

## Usage

The smallest page loads a document by its source path (`ContentItem.Path`):

```razor
@page "/docs/getting-started"
@using Osirion.Blazor.Cms.Components

<DocumentPage ContentPath="en/docs/getting-started.md" />
```

When you have loaded the item yourself, as this site does, pass it as `Content`, with the side navigation entries and
an edit link:

```razor
@using Osirion.Blazor.Cms.Components
@using Osirion.Blazor.Cms.Domain.Entities

@if (page is not null)
{
    <DocumentPage Content="page"
                  NavigationItems="navigation"
                  EditUrlFormatter="@(item => "https://github.com/owner/repo/edit/main/content/" + item.Path)" />
}

@code {
    private ContentItem? page;

    private readonly List<DocumentNavigationItem> navigation =
    [
        new() { Title = "Getting started", Url = "/docs/getting-started", Path = "en/docs/getting-started.md" },
        new()
        {
            Title = "Packages",
            Url = "/docs/packages",
            Path = "en/docs/packages/_index.md",
            Children = [new() { Title = "Core", Url = "/docs/packages/core", Path = "en/docs/packages/core.md" }]
        }
    ];

    // Load page from the content provider in OnInitializedAsync.
}
```

`DocumentNavigationItem` (namespace `Osirion.Blazor.Cms.Components`) has `Title`, `Url`, `Path`, `Order`,
`IsExpanded` and `Children`. An entry's children are listed under it when `IsExpanded` is set or when one of them is
the current page; children are one level deep.

Without `NavigationItems` the page lists the documents of the shown document's folder in their `order`, and with
`ShowNavigationLinks` it looks up the previous and next documents there too. This site passes both from its own
outline instead.

## Parameters

`DocumentPage` loads its document itself and never uses the lookup parameters of its base class. These inherited
parameters do nothing on it: `Item`, `ItemPath`, `ItemUrl`, `ItemSlug`, `ItemUrlFormatter`, `Query`, `Locale`,
`DirectoryName`, `ShowNavigation`, `RelatedItems`, `ShowRelatedContent`, `RelatedContentTitle`, `RelatedItemsCount`,
the hero parameters, `CustomBreadcrumbs` and the sidebar section parameters. Use `Content` and `ContentPath` instead
of the lookup parameters.

<!-- osirion:parameters DocumentPage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ContentPath` | `string?` | `null` | The source path of the document to load from the default provider while `Content` is not set. When nothing is found, the page shows the not-found state and signals 404. A change of the path loads the new document, also on a page that is interactive; a `Content` you pass is never replaced by loading. |
| `Content` | `ContentItem?` | `null` | The document to show. Pass it when you have loaded it, as this documentation site does; the page then loads only what is missing: the side navigation entries and the previous and next documents. Without `Content` and `ContentPath` the page renders its frame without a document. |
| `ShowSideNavigation` | `bool` | `true` | Whether to show the side navigation, a list of pages beside the document. On narrow screens it folds behind a button. |
| `NavigationTitle` | `string?` | `null` | The heading of the side navigation. No heading is shown when it is empty. |
| `SideNavigationToggleText` | `string` | `"Documentation menu"` | The text of the button that shows the side navigation on narrow screens, where it is folded away so the page starts with its content. Defaults to `Documentation menu`. |
| `NavigationItems` | `IReadOnlyList<DocumentNavigationItem>?` | `null` | The entries of the side navigation, with optional children. When it is null, the page lists the documents of the shown document's folder, in their `order`, linked with `ContentUrlFormatter`. |
| `CurrentPath` | `string?` | `null` | The path or address that marks an entry of the side navigation as the current page (`aria-current="page"`), compared without regard to case with each entry's `Path` or `Url`. It defaults to the shown document's path. |
| `ShowMetadata` | `bool` | `true` | Whether to show the author and the "Last updated" date under the title. The date is the item's last modification, or its publication date when there is none, written in the language of the document's locale. |
| `LastUpdatedText` | `string` | `"Last updated:"` | The label in front of the date under the title, for sites written in another language than English. Defaults to `Last updated:`. |
| `ShowTableOfContents` | `bool` | `true` | Whether to show a table of contents of the document's headings, between `TocMinLevel` and `TocMaxLevel`, above the body. |
| `TableOfContentsTitle` | `string` | `"On this page"` | The table of contents title |
| `TocMinLevel` | `int` | `2` | The highest heading level in the table of contents: `2` starts at `h2`. |
| `TocMaxLevel` | `int` | `4` | The lowest heading level in the table of contents: `4` goes down to `h4`. |
| `TocShowNumbers` | `bool` | `true` | Whether the table of contents numbers its entries, passed on to its `ShowNumbers`. |
| `ShowNavigationLinks` | `bool` | `true` | Whether to show previous and next links under the document. When `OsirionContentDetailPageBase.PreviousItem` or `OsirionContentDetailPageBase.NextItem` is null, the page looks the missing one up in the document's folder, in `order`. A value you pass is kept. |
| `CustomFooterContent` | `string?` | `null` | HTML shown under the document in a highlighted section. It is written through `OsirionHtmlRenderer`, which sanitizes it with the registered HTML sanitizer. |
| `EditUrlFormatter` | `Func<ContentItem, string>?` | `null` | How the address of a page's source for editing is built, for example `item => $"https://github.com/owner/repo/edit/main/content/{item.Path}"`. When it returns an address, the page links to it after the body; no link is shown when it is `null` or returns nothing. |
| `EditLinkText` | `string` | `"Edit this page"` | The text of the edit link. Defaults to `Edit this page`. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the links of the previous and next documents and of the folder entries the page lists itself from the item. Without it a link is `/{item.Url}`. |
| `NavigationUrlFormatter` | `Func<DocumentNavigationItem, string>?` | `null` | Builds the link of a side navigation entry. Without it the link is the entry's `Url`. |
| `ShowNavigation` | `bool` | `true` | Whether `ArticlePage` shows the previous and next articles under the body. The other pages do not read it. |
| `PreviousItem` | `ContentItem?` | `null` | The item of the previous link. Read by `DocumentPage`, which looks it up when it is null and keeps a value you pass; `ArticlePage` finds its own neighbors and ignores it. |
| `NextItem` | `ContentItem?` | `null` | The item of the next link. Read by `DocumentPage`, which looks it up when it is null and keeps a value you pass; `ArticlePage` finds its own neighbors and ignores it. |
| `RelatedItems` | `IReadOnlyList<ContentItem>?` | `null` | The items of the related section of `ArticlePage`. When it is null and `ShowRelatedContent` is on, the page picks items of the same locale and directory that share a tag or a category with the article, most shared first, up to `RelatedItemsCount`. An empty list shows no section. |
| `ShowRelatedContent` | `bool` | `true` | Whether `ArticlePage` shows a related section. The other pages do not read it, although the page base still loads related items for them. |
| `RelatedContentTitle` | `string` | `"Related Articles"` | The heading of the related section of `ArticlePage`. |
| `RelatedItemsCount` | `int` | `3` | The most related items the page picks when it finds them itself. |
| `Item` | `ContentItem?` | `null` | The item to show. When you set it, the page shows it and loads nothing; otherwise the page finds the first match of `OsirionContentPageBase.Query` or of the `ItemPath`, `ItemUrl`, `ItemSlug`, locale and directory parameters, and sets this property. Used by `ArticlePage`, `AboutPage` and `ContactPage`; `HomePage` and `DocumentPage` take their item as `Content`. |
| `ItemPath` | `string?` | `null` | The source path of the item to find (`ContentItem.Path`, for example `en/articles/my-article.md`). A change of it, of `ItemUrl`, `ItemSlug`, the locale or the directory makes the page look the item up again. |
| `ItemUrl` | `string?` | `null` | The public URL of the item to find (`ContentItem.Url`, for example `articles/my-article`), without a locale folder. |
| `ItemSlug` | `string?` | `null` | The slug of the item to find (`ContentItem.Slug`, for example `my-article`). Combine it with `DirectoryName` when slugs repeat across directories. |
| `ItemUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the links of the other parts of a series from the item. Passed on by `ArticlePage` to its series navigation; no other page reads it. |
| `Locale` | `string?` | `"en"` | The locale of the content the page looks up (`en` by default): the item found with `ItemPath`, `ItemUrl` or `ItemSlug`, and the related items, must have it. Used by `ArticlePage`, `AboutPage`, `ContactPage` and `ContentListPage`. `HomePage` and `DocumentPage` show the item you pass as `Content`; `HomePage` lists the featured and recent items of this locale. |
| `Query` | `ContentQuery?` | `null` | A complete content query that finds the item in place of `ItemPath`, `ItemUrl`, `ItemSlug`, `Locale` and `DirectoryName`; the first match is shown. Used by `ArticlePage`, `AboutPage` and `ContactPage`. Changing it alone does not make a displayed page load again. |
| `SchemaTypes` | `SchemaType[]?` | `null` | The schema.org types written as the page's structured data, passed to `SeoMetadataRenderer`. When it is null, `AboutPage` uses `AboutPage`, `ContactPage` uses `ContactPage`, `DocumentPage` uses `TechArticle` and the other pages leave the choice to the renderer. `DocumentPage` also takes the first value as the article's microdata type. |
| `GenerateMultipleSchemas` | `bool` | `true` | Whether to write more than one structured data type when it applies, for example the main type and a breadcrumb list. Used by `ArticlePage`, `HomePage`, `AboutPage`, `ContactPage` and `DocumentPage`. |
| `WebsiteName` | `string?` | `null` | The site name that structured data and titles use, passed to `SeoMetadataRenderer` as `SiteNameOverride`. Used by every page component except `LandingPage`; `ContentListPage` passes it to its list. |
| `SiteDescription` | `string?` | `null` | The site description, passed to `SeoMetadataRenderer`, which uses it as the feed description when the feed options set none. Without it the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies. |
| `SiteLogoUrl` | `string?` | `null` | The address of the site logo for the organization's structured data (square, at least 112 by 112 pixels is recommended). Used by `ArticlePage`, `HomePage`, `AboutPage`, `ContactPage` and `DocumentPage`. |
| `TwitterSite` | `string?` | `null` | The handle of the site for `twitter:site`, for example `@contoso`, passed to `SeoMetadataRenderer`. Without it the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies. |
| `TwitterCreator` | `string?` | `null` | The handle of the author for `twitter:creator`, for example `@author`, passed to `SeoMetadataRenderer`. Without it the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies. |
| `FacebookAppId` | `string?` | `null` | The Facebook app ID written as `fb:app_id`, passed to `SeoMetadataRenderer`. Without it the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies. |
| `AllowAiDiscovery` | `bool` | `true` | Whether the crawler opt-in meta tags are written (`true`) or the opt-out tags (`false`), passed to `SeoMetadataRenderer` only when you set it. Without it the configured value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies, which is `true` unless changed. |
| `AllowAiTraining` | `bool` | `true` | Whether the training tags opt in (`true`) or out (`false`) of AI training, passed to `SeoMetadataRenderer` only when you set it. Without it the configured value applies. It has no effect when `AllowAiDiscovery` is `false`. |
| `AllowSearchIndexing` | `bool` | `true` | Whether search engines may index the page, passed to `SeoMetadataRenderer` only when you set it. `false` writes `noindex, nofollow` as the robots tags. Without it the configured value applies. |
| `EnableGeoOptimization` | `bool` | `true` | Whether the summary meta tags (`entity-summary`, `content-type` and the like) are written, when the item has a description, passed to `SeoMetadataRenderer` only when you set it. Without it the configured value applies. |
| `EnableAeoOptimization` | `bool` | `true` | Whether the generated answer tags are allowed, passed to `SeoMetadataRenderer` only when you set it. Without it the configured value applies. |
| `DefaultImageUrl` | `string?` | `null` | The image of the social cards for a page whose item has no featured image, passed to `SeoMetadataRenderer`. Without it the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section applies. |
| `DefaultImageWidth` | `int` | `1200` | The width in pixels of `DefaultImageUrl`, passed to `SeoMetadataRenderer`. The default `1200` yields to the configured width. |
| `DefaultImageHeight` | `int` | `630` | The height in pixels of `DefaultImageUrl`, passed to `SeoMetadataRenderer`. The default `630` yields to the configured height. |
| `OrganizationName` | `string?` | `null` | The organization name for the structured data. Used by `ArticlePage`, `HomePage`, `AboutPage`, `ContactPage` and `DocumentPage`. |
| `CustomBreadcrumbs` | `List<ValueTuple<string, string>>?` | `null` | Name and address pairs that replace the breadcrumb list in the page's structured data. They do not change the visible breadcrumbs. Used by `ArticlePage` and `HomePage`. |
| `PrevPageUrl` | `string?` | `null` | The address of the previous page in a series, written as a `rel="prev"` link in the head. Used by `ArticlePage`, `HomePage` and `DocumentPage`. |
| `NextPageUrl` | `string?` | `null` | The address of the next page in a series, written as a `rel="next"` link in the head. Used by `ArticlePage`, `HomePage` and `DocumentPage`. |
| `AlternateLanguageUrls` | `List<string>?` | `null` | The addresses of the page's other language versions, written as `hreflang` links in the head. Used by every page component that writes head metadata except `LandingPage`. |
| `LoadingText` | `string` | `"Loading article..."` | The text beside the spinner while an interactive render waits for the item (for `HomePage`, for its featured and recent items). A statically rendered response is sent once they have loaded, so it never contains it. Used by `ArticlePage`, `HomePage`, `DocumentPage`, `AboutPage` and `ContactPage`; `LandingPage` has its own. |
| `ShowLoadingText` | `bool` | `true` | Whether the loading spinner shows `LoadingText`. |
| `NotFoundTitle` | `string` | `"Page Not Found"` | The heading of the not-found state, shown when the page finds no item. Used by `ArticlePage`, `HomePage`, `DocumentPage` and `AboutPage`. |
| `NotFoundText` | `string` | `"The page you're looking for doesn't exist or has been moved."` | The message of the not-found state. `LandingPage` has a parameter of its own. |
| `ShowBackLink` | `bool` | `true` | Whether the not-found state has a button back, to `BackLinkUrl` with the text `BackLinkText`. It is not shown when the address is empty. |
| `BackLinkUrl` | `string` | `"/"` | The address of the back link in the not-found state. It is written as given, so build it from values you control. |
| `BackLinkText` | `string` | `"Go to Homepage"` | The text of the back link in the not-found state. |
| `ShowHero` | `bool` | `true` | Whether the page's title and description appear in a `HeroSection` styled by the other `Hero` parameters. When it is off, `ArticlePage` shows no heading unless `HeaderContent` is set, while `AboutPage` and `ContactPage` show a plain `OsirionPageHeading`. `AboutPage` and `ContactPage` start with it off. `DocumentPage` never shows a hero. |
| `UseHeroBackgroundImage` | `bool` | `false` | Whether the hero image is the hero's background instead of a picture beside the text. Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage` while `ShowHero` is on. |
| `HasOverlay` | `bool` | `false` | Whether an overlay darkens the hero's background image so the text stays readable. It applies only with `UseHeroBackgroundImage`. |
| `HeroImageUrl` | `string?` | `null` | The hero's image. `ArticlePage` falls back to the item's featured image when it is not set; `AboutPage` and `ContactPage` show no image without it; `HomePage` falls back to the item's featured image, as `ArticlePage` does. |
| `HeroTheme` | `ThemeMode` | `ThemeMode.System` | The color mode of the hero: `Light`, `Dark` or `System`. Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage`. |
| `HeroAlignment` | `Alignment` | `Alignment.Left` | How the hero's text is aligned, an `Alignment` value such as `Left` or `Center`. Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage`. |
| `HeroImagePosition` | `Alignment` | `Alignment.Right` | Which side of the hero text the image sits on when it is not a background, an `Alignment` value (`Left` or `Right`). Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage`. |
| `HeroVariant` | `HeroVariant` | `HeroVariant.Hero` | The hero's layout: `Hero`, `Jumbotron` or `Minimal`. Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage`. |
| `ShowHeroButtons` | `bool` | `false` | Whether the hero of `AboutPage` and `ContactPage` shows its buttons. `ArticlePage` and `HomePage` ignore it and show a button whenever its text and address are both set. |
| `HeroPrimaryButtonText` | `string?` | `null` | The text of the hero's primary button. The button needs `HeroPrimaryButtonUrl` too. |
| `HeroPrimaryButtonUrl` | `string?` | `null` | The address of the hero's primary button. The button needs `HeroPrimaryButtonText` too. |
| `HeroSecondaryButtonText` | `string?` | `null` | The text of the hero's secondary button. The button needs `HeroSecondaryButtonUrl` too. |
| `HeroSecondaryButtonUrl` | `string?` | `null` | The address of the hero's secondary button. The button needs `HeroSecondaryButtonText` too. |
| `HeroBackgroundPattern` | `BackgroundPatternType?` | `BackgroundPatternType.Grid` | The decorative pattern behind the hero text, or `null` for none. Used by `ArticlePage`, `HomePage`, `AboutPage` and `ContactPage`. |
| `ShowBreadcrumbs` | `bool` | `true` | Whether to show the visible breadcrumbs: `OsirionBreadcrumbs` in `ArticlePage`, `ContentBreadcrumbs` in `DocumentPage`. The other pages show none. |
| `ShowBreadcrumbHome` | `bool` | `true` | Whether the breadcrumbs start with a home link. Used by `ArticlePage` and `DocumentPage`. |
| `BreadcrumbHomeText` | `string` | `"Home"` | The text of the breadcrumbs' home link. Used by `ArticlePage` and `DocumentPage`. |
| `BreadcrumbHomeUrl` | `string` | `"/"` | The address of the breadcrumbs' home link. Used by `ArticlePage` and `DocumentPage`. |
| `DirectoryName` | `string?` | `null` | The name of the directory the item must be in, as a filter of the query that finds it. Used by `ArticlePage`, `AboutPage` and `ContactPage`. |
| `DirectoryUrlFormatter` | `Func<DirectoryItem, string>?` | `null` | Builds the address of a breadcrumb that stands for a directory. Used by `DocumentPage`; the other pages do not read it. |
| `ShowCategoriesSidebar` | `bool` | `true` | Whether the sidebar of `ArticlePage` lists the item's categories. The section appears only when the item has categories. |
| `CategoriesSectionTitle` | `string` | `"Categories"` | The heading of the category list: in the sidebar of `ArticlePage`, and in the browse section of `HomePage`. |
| `CategoryUrlFormatter` | `Func<string, string>?` | `null` | Builds a category link from the category name. Used by `ArticlePage` (sidebar) and `HomePage` (browse section); without it the link is `/category/{slug}`. |
| `ShowTagsSidebar` | `bool` | `true` | Whether the sidebar of `ArticlePage` lists the item's tags. The section appears only when the item has tags. |
| `TagsSectionTitle` | `string` | `"Tags"` | The heading of the tag list: in the sidebar of `ArticlePage`, and in the browse section of `HomePage`. |
| `TagUrlFormatter` | `Func<string, string>?` | `null` | Builds a tag link from the tag name. Used by `ArticlePage` (sidebar) and `HomePage` (browse section); without it the link is `/tag/{slug}`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

## Behavior

- **Rendering.** The page renders on the server and needs no script; it does not use streaming rendering. The side
  navigation folds behind a button on narrow screens with a hidden checkbox, not script.
- **Loading.** The page loads by `ContentPath` while `Content` is not set, and loads again when the path changes, so a
  page that stays interactive while its address changes shows the new document. A `Content` you pass is never replaced
  by loading. The page then loads only what is missing: the folder's entries for the side navigation, and the previous
  and next documents, each one only when you did not pass it. They are looked up again when the document or the
  parameters that decide what is missing change, not on every render.
- **Head.** When a document is shown, the page writes a `SeoMetadataRenderer` for it, with `TechArticle` structured
  data unless `SchemaTypes` says otherwise. The first of `SchemaTypes` is also the article's microdata type; `FAQ`
  becomes `FAQPage`. `WebsiteName`, `OrganizationName`, `SiteLogoUrl`, `GenerateMultipleSchemas`, `PrevPageUrl`,
  `NextPageUrl` and `AlternateLanguageUrls` reach the renderer. `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth` and `DefaultImageHeight` reach the renderer when set, and so do `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` when you set them: a switch you leave out keeps the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section.
- **Title.** The title is the page's `h1`; the body leaves out its own leading `h1`. `ShowMetadata` adds the author and
  a "Last updated" date, the last modification date or, without one, the publication date. The date is written in
  the language of the document's locale, or in the current culture when the locale is missing or not a culture name,
  inside a `<time>` element; `LastUpdatedText` sets the label in front of it.
- **Table of contents.** `ShowTableOfContents` builds it from the headings between `TocMinLevel` and `TocMaxLevel`
  and places it above the body. `TocShowNumbers="false"` drops the `1.` and `1.2` numbers in front of its entries.
- **Edit link.** The link to the source of the page appears only when `EditUrlFormatter` returns an address. It opens
  in a new tab with `rel="noopener"`.
- **Pager.** The previous and next links appear with `ShowNavigationLinks` when at least one neighbor exists.
- **Not found.** With a `ContentPath` that finds nothing, the page renders the not-found state of
  [OsirionContentNotFound](/components/content-not-found), shaped by the `NotFound*` and `BackLink*` parameters (`NotFoundTitle` and `NotFoundText` are its heading and message, and a button leads to `BackLinkUrl` with the text `BackLinkText` unless `ShowBackLink` is off or the address is empty.), and answers with status 404: `NavigationManager.NotFound()`
  on .NET 10, the status code of the request on .NET 9, while the response has not started. Without `Content` and
  without `ContentPath` the page renders its frame with no document and does not signal 404.
- **Security.** The body is sanitized by `ContentRenderer`, and `CustomFooterContent` passes through
  `OsirionHtmlRenderer`, which sanitizes it. The addresses `EditUrlFormatter`, `ContentUrlFormatter` and
  `NavigationUrlFormatter` return are written as given, so build them from values you control.
- **Attributes.** `Class`, `Style` and unmatched attributes go to the root element, and so does `Theme` as
  `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

## Styling

The root element has the classes `osirion-document-page` and, with the side navigation, `osirion-document-with-sidenav`.
Inside, `osirion-document-layout` holds the side navigation (`osirion-document-sidenav`, with `osirion-sidenav-toggle`,
`osirion-sidenav-panel`, `osirion-sidenav-title`, `osirion-sidenav-list`, `osirion-sidenav-link` and, for the current
page, `osirion-sidenav-active`) and `osirion-document-main`. The article is `osirion-document-content` with
`osirion-document-header`, `osirion-document-title`, `osirion-document-meta`, `osirion-document-toc`,
`osirion-document-body`, `osirion-document-edit` and `osirion-document-pagination`. Colors come from theme tokens such
as `--osirion-text-primary`, `--osirion-border-color`, `--osirion-action-primary` and `--osirion-background-secondary`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [ContentRenderer](/components/content-renderer): the body inside the page
- [TableOfContents](/components/table-of-contents): the contents list
- [ContentBreadcrumbs](/components/content-breadcrumbs): the breadcrumb trail
- [SeoMetadataRenderer](/components/seo-metadata-renderer): the head metadata
