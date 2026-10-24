---
order: 6
title: ArticlePage
permalink: /components/article-page
description: "ArticlePage renders a whole article page from one content item: head metadata, hero, breadcrumbs, body, series and article navigation, related articles and a sidebar."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-article-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- article-page
- cms
- article
- seo
- static-ssr
slug: article-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'ArticlePage: a whole article page for Blazor'
  description: 'ArticlePage renders an article page from one CMS content item: head metadata, hero, breadcrumbs, body, related articles and a sidebar.'
  image: 'https://media.getosirion.com/og/components-article-page.jpg'
  canonical: 'https://getosirion.com/components/article-page'
  lang: en
  robots: index, follow
  og_title: 'ArticlePage - Osirion.Blazor'
  og_description: 'An article page from one content item: head metadata, hero, breadcrumbs, body, related articles and a sidebar, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-article-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ArticlePage - Osirion.Blazor'
  twitter_description: 'An article page from one content item: head metadata, hero, breadcrumbs, body, related articles and a sidebar.'
  twitter_image_url: 'https://media.getosirion.com/og/components-article-page.jpg'
  meta_tags: {}
---

# ArticlePage

`ArticlePage` renders a complete article page from one content item: the head metadata, a hero or your own header,
breadcrumbs, the body, article and series navigation, related articles and a sidebar with a table of contents,
categories and tags. Use it for blog posts and articles when you want the whole page with one component. This site's
blog posts are rendered by it.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/ArticlePage.razor).

There is no live demo on this page. `ArticlePage` writes the page's title, canonical address and structured data
into the head, and when the item is missing it makes the whole response a 404, so rendering it inside another page
would change that page's head and status. Open any post under [/blog](/blog) to see it.

## Usage

The page the example site uses for a blog post, found by its slug in the `blog` directory:

```razor
@page "/blog/{slug}"
@using Osirion.Blazor.Cms.Web.Components

<ArticlePage Locale="en"
             DirectoryName="blog"
             ItemSlug="@Slug"
             SchemaTypes="new[] { SchemaType.BlogPosting }" />

@code {
    [Parameter] public string? Slug { get; set; }
}
```

Pass an item you loaded yourself with `Item`, or find it with `ItemPath` (the source path), `ItemUrl` (the public URL
without a locale folder) or `ItemSlug`; `Query` takes a full `ContentQuery` instead. Each of `ItemPath`, `ItemUrl`,
`ItemSlug`, `Locale` and `DirectoryName` that changes makes the page look the item up again, and an `Item` you pass is
never replaced.

Your own header and author block, in place of the hero and the default end of the article:

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Cms.Web.Components

<ArticlePage ItemSlug="@Slug" DirectoryName="blog">
    <HeaderContent>
        <h1>@context.Title</h1>
        <p>@context.Description</p>
    </HeaderContent>
    <AuthorContent>
        <OsirionAuthorCard Name="@context.Author" />
    </AuthorContent>
</ArticlePage>
```

A closing call to action after the article and its sidebar, outside the article column. `FooterContent` gets the
item as `context`, like `HeaderContent`:

```razor
@using Osirion.Blazor.Cms.Web.Components

<ArticlePage ItemSlug="@Slug" DirectoryName="blog">
    <FooterContent>
        <p>Questions about @context.Title? <a href="/contact">Get in touch</a>.</p>
    </FooterContent>
</ArticlePage>
```

## Parameters

The parameters come from three base classes besides the page itself: the lookup (`Item`, `ItemPath`, `ItemUrl`,
`ItemSlug`, `Query`, `Locale`, `DirectoryName`), the page chrome (hero, breadcrumbs, sidebar sections, head
metadata) and the neighbors and related items. A few shared parameters do nothing on this page; the table says which.

<!-- osirion:parameters ArticlePage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ShowSidebar` | `bool` | `true` | Whether to show the sidebar next to the article: the table of contents, the item's categories and tags and `SidebarContent`. It stays in view while the article scrolls. |
| `ShowTableOfContents` | `bool` | `true` | Whether the sidebar has a table of contents built from the article's headings. It needs `ShowSidebar`. |
| `TableOfContentsTitle` | `string` | `"Table of Contents"` | The heading of the table of contents. |
| `CustomFooterContent` | `string?` | `null` | HTML shown after the article and its author, for example a call to subscribe. It is sanitized like the article itself. |
| `ShowSeriesNavigation` | `bool` | `true` | Whether an article that belongs to a series (`series` front matter) shows where it stands in it, with links to the other parts, above the body. Defaults to `true`; nothing renders for an article outside a series. |
| `AuthorContent` | `RenderFragment<ContentItem>?` | `null` | What is shown after the body about the article's author, for example an `OsirionAuthorCard` built from `context.Author`. Nothing is shown when `null`. |
| `HeaderContent` | `RenderFragment<ContentItem>?` | `null` | The article's header, shown instead of the hero. It must render the page's `h1`, usually from `context.Title`; a leading `h1` in the body is still left out, as it is under the hero. When `null`, the hero is shown as configured by `ShowHero`. |
| `FooterContent` | `RenderFragment<ContentItem>?` | `null` | Content shown after the article and its sidebar, outside the article column, for example a closing call to action or related links. Unlike `CustomFooterContent`, which ends the article itself, it is your own markup, written as you give it. Nothing is rendered when `null`. |
| `SidebarContent` | `RenderFragment?` | `null` | Content shown at the end of the sidebar, in a section of its own. It needs `ShowSidebar`. |
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

- **Rendering.** The page renders on the server and needs no script; it does not use streaming rendering. While an
  interactive render waits for the item it shows a spinner with `LoadingText`.
- **Head.** The page writes a `SeoMetadataRenderer` for the item: title, description, canonical address, Open Graph,
  Twitter Card and structured data. `SchemaTypes`, `WebsiteName`, `OrganizationName`, `SiteLogoUrl`,
  `GenerateMultipleSchemas`, `CustomBreadcrumbs`, `PrevPageUrl`, `NextPageUrl` and `AlternateLanguageUrls` reach it.
  `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth` and `DefaultImageHeight` reach the renderer when set, and so do `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` when you set them: a switch you leave out keeps the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section.
- **Header.** `HeaderContent`, when set, replaces the hero. Otherwise, with `ShowHero` on, a hero shows the item's
  title and description, its featured image (or `HeroImageUrl`), and the author, publication date and reading time.
  The date follows the language of the item's locale.
  With `ShowHero` off and no `HeaderContent` the page has no heading. Whenever a hero or `HeaderContent` supplies the
  title, the body leaves out its own leading `h1`, so the page keeps a single `h1`.
- **Breadcrumbs.** `ShowBreadcrumbs` renders [Breadcrumbs](/components/breadcrumbs) from the item's `Url`, ending with
  the item's title.
- **Navigation.** `ShowSeriesNavigation` shows where an article stands in its series (`series` front matter), above
  the body, and nothing for an article outside a series. `ShowNavigation` shows the previous and next articles under
  the body through [OsirionContentNavigation](/components/osirion-content-navigation).
- **Related articles.** With `ShowRelatedContent` on (the default) and no `RelatedItems`, the page picks up to
  `RelatedItemsCount` items of the same locale and directory that share a tag or a category with the article. Nothing
  renders when none do.
- **Sidebar.** `ShowSidebar` adds a column that stays in view while the article scrolls. It holds the table of
  contents, the item's categories and tags, and `SidebarContent`. Category and tag links default to
  `/category/{slug}` and `/tag/{slug}`. Tags take one of eight badge colors, chosen from the tag's name so a tag keeps
  its color between requests. For an article without headings the table of contents section is hidden, since
  [TableOfContents](/components/table-of-contents) renders nothing. Below 640 pixels the table of contents is hidden,
  and a sidebar with no other section takes no space under the article.
- **Footer.** `FooterContent`, when set, follows the article and its sidebar in a `footer`, outside the article
  column. Nothing renders without it.
- **Not found.** Without an item the page renders the not-found state of
  [OsirionContentNotFound](/components/content-not-found) and answers with status 404: `NavigationManager.NotFound()` on .NET 10, the status code of
  the request on .NET 9. The status is only set while the response has not started, and not during interactive
  rendering. `NotFoundTitle` and `NotFoundText` are its heading and message, and a button leads to `BackLinkUrl` with the text `BackLinkText` unless `ShowBackLink` is off or the address is empty.
- **Drafts.** The lookup by `ItemPath`, `ItemUrl` or `ItemSlug` asks for published items only, so a draft
  (`published: false`) answers 404 like a missing item. A `Query` you pass decides through its
  `IncludeUnpublished` and `Status`, and an `Item` you pass is shown as given.
- **Security.** The body is sanitized by `ContentRenderer`, and `CustomFooterContent` is sanitized the same way.
  `HeaderContent`, `AuthorContent`, `SidebarContent` and `FooterContent` are your own markup, written as you give it.
- **Attributes.** `Class`, `Style` and unmatched attributes go to the root element, and so does `Theme` as
  `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

## Styling

The root element has the classes `osirion-article-page` and, with the sidebar, `osirion-article-with-sidebar`. The
page content is `osirion-article-page-content` with `osirion-article-layout` (plus `osirion-article-layout-with-sidebar`)
and `osirion-article-main`. A custom header sits in `osirion-article-header`, the author block in
`osirion-article-author`, the footer in `osirion-article-footer`, and `FooterContent` in `osirion-article-page-footer`
after the layout. The sidebar sections use `osirion-sidebar-section`,
`osirion-sidebar-title`, `osirion-toc`, `osirion-sidebar-category`, `osirion-sidebar-tag` and the badge classes
`osirion-tag-badge-0` to `osirion-tag-badge-7`. Colors come from theme tokens such as `--osirion-text-primary`,
`--osirion-text-secondary`, `--osirion-action-primary` and `--osirion-link-color`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [ContentRenderer](/components/content-renderer): the body inside the page
- [HeroSection](/components/hero-section): the hero and its parameters
- [TableOfContents](/components/table-of-contents): the sidebar's contents list
- [SeoMetadataRenderer](/components/seo-metadata-renderer): the head metadata
