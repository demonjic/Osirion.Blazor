---
order: 8
title: HomePage
permalink: /components/home-page
description: "HomePage renders a home page from one content item: head metadata, a hero, the Markdown body, a featured content section and an optional browse section."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-home-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- home-page
- cms
- hero
- featured-content
- static-ssr
slug: home-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'HomePage: a home page from content for Blazor'
  description: 'HomePage renders a home page from one CMS content item: head metadata, a hero, the body, featured content and a browse section.'
  image: 'https://media.getosirion.com/og/components-home-page.jpg'
  canonical: 'https://getosirion.com/components/home-page'
  lang: en
  robots: index, follow
  og_title: 'HomePage - Osirion.Blazor'
  og_description: 'A home page from one content item: hero, Markdown body, featured content and a browse section, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-home-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'HomePage - Osirion.Blazor'
  twitter_description: 'A home page from one content item: hero, Markdown body, featured content and a browse section.'
  twitter_image_url: 'https://media.getosirion.com/og/components-home-page.jpg'
  meta_tags: {}
---

# HomePage

`HomePage` renders a home page from one content item: the head metadata, a hero built from the item's title,
description and featured image, the item's Markdown body, a section of content items and an optional browse section
with categories and tags. Use it when the text of the home page lives in your content repository. This site's home
page is rendered by it.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/HomePage.razor).

There is no live demo on this page. `HomePage` writes the page's title, canonical address and structured data into the
head and makes the whole response a 404 when it has nothing to show, so rendering it inside another page would change
that page's head and status. Open the [home page](/) of this site to see it.

## Usage

`HomePage` does not load its item: pass it as `Content`. This is the home page of the example site, which loads the
item with the slug `homepage` and shows a centered jumbotron hero:

```razor
@page "/"
@using Osirion.Blazor.Cms.Components
@using Osirion.Blazor.Cms.Domain.Entities
@using Osirion.Blazor.Cms.Domain.Repositories
@using Osirion.Blazor.Cms.Domain.Services
@using Osirion.Blazor.Components
@inject IContentProviderManager ContentProviders

<HomePage Content="content"
          HeroAlignment="Alignment.Center"
          HeroVariant="HeroVariant.Jumbotron"
          HeroPrimaryButtonText="Get Started"
          HeroPrimaryButtonUrl="/docs"
          ShowCategories="false"
          ShowTags="false" />

@code {
    private ContentItem? content;

    protected override async Task OnInitializedAsync()
    {
        var result = await ContentProviders.GetContentByQueryAsync(new ContentQuery { Locale = "en", Slug = "homepage" });
        content = result?.FirstOrDefault();
    }
}
```

`ChildContent` adds your own markup after the body and before the featured section. With `ChildContent` and no
`Content` the page still renders, without a title.

## Parameters

`HomePage` shares its hero, SEO and breadcrumb parameters with the other page components. A few parameters do
nothing, and the table says which: `ContentPath`. The lookup parameters of the base class (`Item`, `ItemPath`,
`ItemUrl`, `ItemSlug`, `Query`) are not used: the page runs no lookup, and loads only its featured and recent items.

<!-- osirion:parameters HomePage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ContentPath` | `string?` | `null` | Has no effect: `HomePage` does not load by path. Pass the item as `Content`. |
| `Content` | `ContentItem?` | `null` | The item that makes the page: its title and description are the hero's, its featured image the hero's image and its body follows the hero (without its own leading `h1` while the hero is shown). With neither `Content` nor `ChildContent` the page shows the not-found state and signals 404. |
| `ShowFeaturedContent` | `bool` | `true` | Whether to show the featured section, an `OsirionContentListSection` of the newest `FeaturedCount` items of the page's `Locale` that are marked `featured: true` in their front matter. Without such items the section is left out. |
| `FeaturedSectionTitle` | `string` | `"Featured Content"` | The heading of the featured section. |
| `FeaturedSectionDescription` | `string?` | `null` | The sentence under the featured section's heading. None when null. |
| `FeaturedLayout` | `string` | `"grid"` | The layout of the featured items: `grid`, `list`, `carousel` or `featured-first`. |
| `FeaturedColumns` | `int` | `3` | The number of columns of the featured items in the `grid` layout. |
| `FeaturedCount` | `int` | `3` | The number of items the featured section shows. |
| `ShowFeaturedExcerpt` | `bool` | `true` | Whether each featured item shows an excerpt of its body. |
| `ShowFeaturedAuthor` | `bool` | `true` | Whether each featured item shows its author. |
| `ShowFeaturedCategories` | `bool` | `true` | Whether each featured item shows its categories. |
| `FeaturedViewAllUrl` | `string?` | `null` | The address of the "View all posts" link under the featured items. The link needs this and `ShowFeaturedViewAll`. |
| `ShowFeaturedViewAll` | `bool` | `true` | Whether the "View all posts" link shows. It also needs `FeaturedViewAllUrl`. |
| `ShowRecentContent` | `bool` | `true` | Whether to show the recent section: the newest `RecentCount` items of the page's `Locale`, featured or not, under `RecentSectionTitle`. Without items the section is left out. |
| `RecentSectionTitle` | `string` | `"Recent Posts"` | The heading of the recent section. |
| `RecentCount` | `int` | `6` | The number of items the recent section shows. |
| `ShowCategories` | `bool` | `false` | Whether the browse section lists the categories, under the heading `CategoriesSectionTitle`. |
| `ShowTags` | `bool` | `false` | Whether the browse section shows the tag cloud, under the heading `TagsSectionTitle`. |
| `BrowseSectionTitle` | `string` | `"Browse Content"` | The heading of the browse section, which holds the category list and the tag cloud. |
| `MaxCategories` | `int` | `8` | The most categories the browse section lists. |
| `MaxTags` | `int` | `15` | The most tags the browse section shows. |
| `ShowCategoryCounts` | `bool` | `true` | Whether each listed category shows its item count. |
| `ShowTagCounts` | `bool` | `true` | Whether each tag shows its item count. |
| `CustomContent` | `string?` | `null` | HTML shown in a section after the other sections. It is written through `OsirionHtmlRenderer`, which sanitizes it with the registered HTML sanitizer. |
| `ContentUrlFormatter` | `Func<ContentItem, string>?` | `null` | Builds the link of each item in the featured and recent sections from the item. Without it a link is `/{item.Url}`. |
| `ReadMoreText` | `string` | `"Read more"` | The text of the link under each item of the recent section. No link is shown when it is empty. |
| `ChildContent` | `RenderFragment?` | `null` | Content shown after the body and before the featured section. Child content alone keeps the page from the not-found state. |
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
| `SchemaTypes` | `SchemaType[]?` | `null` | The schema.org types written as the page's structured data, passed to `SeoMetadataRenderer`. When it is null, `AboutPage` uses `AboutPage`, `ContactPage` uses `ContactPage`, `DocumentPage` uses `TechArticle` and the other pages leave the choice to the renderer; `ArticlePage` then writes `Article` when neither the configured `DefaultSchemaTypes` nor the front matter `type` names one. `DocumentPage` also takes the first value as the article's microdata type. |
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

- **Rendering.** The page renders on the server and needs no script; it does not use streaming rendering.
- **Head.** The page writes a `SeoMetadataRenderer` for the item. `SchemaTypes`, `WebsiteName`, `OrganizationName`,
  `SiteLogoUrl`, `GenerateMultipleSchemas`, `CustomBreadcrumbs`, `PrevPageUrl`, `NextPageUrl` and
  `AlternateLanguageUrls` reach it. `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth` and `DefaultImageHeight` reach the renderer when set, and so do `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` when you set them: a switch you leave out keeps the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section.
- **Order.** The page writes, in this order: the head metadata, the hero (`ShowHero`), the body, `ChildContent`, the
  featured section (`ShowFeaturedContent`), the recent section (`ShowRecentContent`), the browse section (when
  `ShowCategories` or `ShowTags` is on) and `CustomContent`.
- **Hero.** The hero takes the item's title and description, and `HeroImageUrl` or, without it, the item's featured
  image. The hero buttons show when both a text and an address are set. While the hero is shown, the body leaves out
  its own leading `h1`, so the page keeps a single `h1`.
- **Featured section.** It is an `OsirionContentListSection` of the newest `FeaturedCount` items of the page's `Locale`
  (`en` by default) whose front matter says `featured: true`. With no such item the section is left out; it used to
  list the newest items of any locale, and showed a not-found block when there were none.
- **Recent section.** The newest `RecentCount` items of the page's `Locale`, featured or not, under
  `RecentSectionTitle`, each with a `ReadMoreText` link. With no items, or with `ShowRecentContent` off, it is left
  out. A featured item can appear in both sections.
- **Links.** `ContentUrlFormatter` builds the link of each item in the featured and recent sections; without it a link
  is `/{item.Url}`.
- **Loading.** The page queries the default provider for the two sections, and again only when `Locale`,
  `ShowFeaturedContent`, `FeaturedCount`, `ShowRecentContent` or `RecentCount` change. A query that fails is logged and
  leaves its section out; the rest of the page renders.
- **Browse section.** It lists categories and tags with `CategoriesList` and `TagCloud`, up to `MaxCategories` and
  `MaxTags`, with counts unless you turn them off. The links default to `/category/{slug}` and `/tag/{slug}`.
- **Not found.** With neither `Content` nor `ChildContent`, the page renders the not-found state of
  [OsirionContentNotFound](/components/content-not-found), shaped by the `NotFound*` and `BackLink*` parameters (`NotFoundTitle` and `NotFoundText` are its heading and message, and a button leads to `BackLinkUrl` with the text `BackLinkText` unless `ShowBackLink` is off or the address is empty.), and answers with status 404: `NavigationManager.NotFound()`
  on .NET 10, the status code of the request on .NET 9, while the response has not started.
- **Security.** The body and `CustomContent` pass through `OsirionHtmlRenderer`, which sanitizes them.
- **Attributes.** `Class`, `Style` and unmatched attributes go to the root element, and so does `Theme` as
  `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

## Styling

The root element has the class `osirion-homepage`. The body, the child content and the featured and recent sections
sit in `osirion-homepage-content`. The browse section is `osirion-homepage-browse` with `osirion-browse-content` and
`osirion-browse-section`, and the custom content is `osirion-homepage-custom`. Colors come from theme tokens such as
`--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-action-primary` and `--osirion-link-color`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [HeroSection](/components/hero-section): the hero and its parameters
- [CategoriesList](/components/categories-list) and [TagCloud](/components/tag-cloud): the browse section
- [LandingPage](/components/landing-page): a marketing page from one content item
