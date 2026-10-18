---
order: 10
title: AboutPage
permalink: /components/about-page
description: "AboutPage renders an about page from one content item, found by the slug about: a heading, the Markdown body and your own content such as author cards."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-about-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- about-page
- cms
- seo
- static-ssr
slug: about-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'AboutPage: an about page from content for Blazor'
  description: 'AboutPage renders an about page from one CMS content item: a heading, the Markdown body, AboutPage structured data and your own content.'
  image: 'https://media.getosirion.com/og/components-about-page.jpg'
  canonical: 'https://getosirion.com/components/about-page'
  lang: en
  robots: index, follow
  og_title: 'AboutPage - Osirion.Blazor'
  og_description: 'An about page from one content item: a heading, the Markdown body and your own content, rendered without script.'
  og_image_url: 'https://media.getosirion.com/og/components-about-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'AboutPage - Osirion.Blazor'
  twitter_description: 'An about page from one content item: a heading, the Markdown body and your own content.'
  twitter_image_url: 'https://media.getosirion.com/og/components-about-page.jpg'
  meta_tags: {}
---

# AboutPage

`AboutPage` renders a whole about page from one content item, which it finds by the slug `about`: the title as the
page's `h1`, the description under it, the Markdown body and whatever `ChildContent` adds below, such as author cards
for the people behind the site. It writes `AboutPage` structured data and answers 404 when the item is missing.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/AboutPage.razor).

There is no live demo on this page. `AboutPage` writes the page's title, canonical address and structured data into the
head and makes the whole response a 404 when its item is missing, so rendering it inside another page would change
that page's head and status.

## Usage

With the item in `about.md` (the slug `about` is the default), the page needs no parameters:

```razor
@page "/about"
@using Osirion.Blazor.Cms.Web.Components

<AboutPage />
```

Author cards below the body, and a hero instead of the plain heading:

```razor
@page "/about"
@using Osirion.Blazor.Cms.Web.Components
@using Osirion.Blazor.Components

<AboutPage ShowHero="true" WebsiteName="Example Ltd">
    <OsirionAuthorCard Name="Ana Petrović" Role="Founder" />
</AboutPage>
```

To use another item, set `ItemSlug`, `ItemUrl` or `ItemPath`, or pass `Item` or a full `Query`. `ItemSlug` defaults to
`about` and `ShowHero` to `false`, which the constructor sets. The item's `title` is the heading and its `description`
the sentence under it.

## Parameters

`AboutPage` takes the lookup parameters of `OsirionContentItemPageBase` (`Item`, `ItemPath`, `ItemUrl`, `ItemSlug`,
`Query`, `Locale`, `DirectoryName`) and the hero and SEO parameters of the page base. It has no sidebar, breadcrumbs
or related items, so those parameters do nothing here, as the table says.

<!-- osirion:parameters AboutPage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | Content shown after the body, for example author cards, a timeline or links to the project. |
| `Item` | `ContentItem?` | `null` | The item to show. When you set it, the page shows it and loads nothing; otherwise the page finds the first match of `OsirionContentPageBase.Query` or of the `ItemPath`, `ItemUrl`, `ItemSlug`, locale and directory parameters, and sets this property. Used by `ArticlePage`, `AboutPage` and `ContactPage`; `HomePage` and `DocumentPage` take their item as `Content`. |
| `ItemPath` | `string?` | `null` | The source path of the item to find (`ContentItem.Path`, for example `en/articles/my-article.md`). A change of it, of `ItemUrl`, `ItemSlug`, the locale or the directory makes the page look the item up again. |
| `ItemUrl` | `string?` | `null` | The public URL of the item to find (`ContentItem.Url`, for example `articles/my-article`), without a locale folder. |
| `ItemSlug` | `string?` | `"about"` | The slug of the item to find (`ContentItem.Slug`, for example `my-article`). Combine it with `DirectoryName` when slugs repeat across directories. |
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
| `ShowHero` | `bool` | `false` | Whether the page's title and description appear in a `HeroSection` styled by the other `Hero` parameters. When it is off, `ArticlePage` shows no heading unless `HeaderContent` is set, while `AboutPage` and `ContactPage` show a plain `OsirionPageHeading`. `AboutPage` and `ContactPage` start with it off. `DocumentPage` never shows a hero. |
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
- **Head.** The page writes a `SeoMetadataRenderer` for the item, with `AboutPage` structured data unless
  `SchemaTypes` says otherwise. `WebsiteName`, `OrganizationName`, `SiteLogoUrl`, `GenerateMultipleSchemas` and
  `AlternateLanguageUrls` reach it. `SiteDescription`, `TwitterSite`, `TwitterCreator`, `FacebookAppId`, `DefaultImageUrl`, `DefaultImageWidth` and `DefaultImageHeight` reach the renderer when set, and so do `AllowAiDiscovery`, `AllowAiTraining`, `AllowSearchIndexing`, `EnableGeoOptimization` and `EnableAeoOptimization` when you set them: a switch you leave out keeps the value of `AddOsirionSeoMetadata` or the `Osirion:Seo` section.
- **Heading.** With `ShowHero` off, the title and description are an
  [OsirionPageHeading](/components/page-heading); with it on, a `HeroSection` styled by the `Hero` parameters. Either
  way the title is the page's `h1`, and the body leaves out its own leading `h1`.
- **Body.** The item's body is rendered by `ContentRenderer`, so it is sanitized and code blocks are highlighted.
  `ChildContent` follows it.
- **Not found.** Without an item the page renders the not-found state of
  [OsirionContentNotFound](/components/content-not-found), shaped by the `NotFound*` and `BackLink*` parameters (`NotFoundTitle` and `NotFoundText` are its heading and message, and a button leads to `BackLinkUrl` with the text `BackLinkText` unless `ShowBackLink` is off or the address is empty.), and answers with status 404: `NavigationManager.NotFound()`
  on .NET 10, the status code of the request on .NET 9, while the response has not started.
- **Attributes.** `Class`, `Style` and unmatched attributes go to the root element, and so does `Theme` as
  `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

## Styling

The root element has the class `osirion-about-page`. The body is in `osirion-about-page-body` and the child content in
`osirion-about-page-more`; both are centered at the width `--osirion-container-lg` (960 pixels when unset), the width
of `OsirionPageHeading`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [ContactPage](/components/contact-page): the matching contact page
- [OsirionAuthorCard](/components/author-card): author cards for `ChildContent`
- [PageHeading](/components/page-heading): the plain heading
