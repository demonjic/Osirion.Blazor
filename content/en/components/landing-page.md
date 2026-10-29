---
order: 9
title: LandingPage
permalink: /components/landing-page
description: "LandingPage renders a marketing page from one content item or from parameters: a hero, a Markdown body in a configurable section, custom HTML and your own content."
author: "Osirion Team"
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-landing-page.jpg'
categories:
- Components
- CMS Web Components
- Page Components
tags:
- landing-page
- cms
- hero
- marketing
- static-ssr
slug: landing-page
lang: en
layout: "component"
published: true
custom_fields:
  group: cms-pages
seo_properties:
  title: 'LandingPage: a marketing page for Blazor'
  description: 'LandingPage renders a marketing page from one CMS content item or from parameters: a hero, a body section and your own content.'
  image: 'https://media.getosirion.com/og/components-landing-page.jpg'
  canonical: 'https://getosirion.com/components/landing-page'
  lang: en
  robots: index, follow
  og_title: 'LandingPage - Osirion.Blazor'
  og_description: 'A marketing page from one content item or from parameters: a hero, a body section and your own content.'
  og_image_url: 'https://media.getosirion.com/og/components-landing-page.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'LandingPage - Osirion.Blazor'
  twitter_description: 'A marketing page from one content item or from parameters: a hero, a body section and your own content.'
  twitter_image_url: 'https://media.getosirion.com/og/components-landing-page.jpg'
  meta_tags: {}
---

# LandingPage

`LandingPage` renders a marketing page: a tall hero, the Markdown body of a content item in a section you can size,
align and style, and optional HTML or markup of your own after it. Use it for a product or campaign page whose text
lives in your content repository, or whose hero you set from parameters.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/PageLevel/LandingPage.razor).

There is no live demo on this page. `LandingPage` writes the page's title and structured data into the head, and its
hero is 70 percent of the window tall by default, so it does not belong inside another page.

## Usage

Load the page from a content item by its source path, with a hero that links to the documentation and to the source:

```razor
@page "/launch"
@using Osirion.Blazor.Cms.Components

<LandingPage ContentPath="en/launch.md"
             HeroHeight="50vh"
             HeroPrimaryButtonText="Get started"
             HeroPrimaryButtonUrl="/docs"
             HeroSecondaryButtonText="Source on GitHub"
             HeroSecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor"
             ContentWidth="narrow" />
```

Without an item, the hero comes from `HeroTitle`, `HeroSubtitle` and `HeroBackgroundImage`, and `ChildContent` holds
the rest of the page:

```razor
<LandingPage HeroTitle="Launch week" HeroSubtitle="Everything that ships on Monday.">
    <p>Details follow.</p>
</LandingPage>
```

When an item is set, its title and description win over `HeroTitle` and `HeroSubtitle`, which only fill in for a
missing item or description. Pass an item you loaded yourself with `Content` instead of `ContentPath` when you also
need it elsewhere.

## Parameters

The page renders three sections beyond the hero and the body: features (`ShowFeatures`, `FeatureItems`), testimonials
(`ShowTestimonials`, `TestimonialItems`) and a call to action (`ShowCallToAction` and the `Cta` parameters). A section
renders only when it has something to show: at least one feature, at least one testimonial with a quote, a call to
action with a title, a description or a button. The `FeatureItem` and `TestimonialItem` types are in the namespace
`Osirion.Blazor.Cms.Components`. The string options (`"center"`, `"grid"`, `"primary"`) stay strings in 4.x; they
become enums in 5.0.

<!-- osirion:parameters LandingPage -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ContentPath` | `string?` | `null` | The source path of the item to load from the default provider while `Content` is not set. When nothing is found, the page shows `NotFoundText` and signals 404. |
| `ShowUnpublished` | `bool` | `false` | Whether to show an item that is not published (`published: false` in its front matter) when the component loads it by `ContentPath`. Off by default: a draft then renders the not-found state and signals 404, as a missing item does. Turn it on only for previews you protect yourself; lists, the sitemap and feeds leave drafts out either way. An item you pass in is shown whatever its status. |
| `Content` | `ContentItem?` | `null` | The item that makes the page: its title and description are the hero's, its featured image the hero's image and its body follows the hero. Without an item the page shows what the other parameters give it. An item with content blocks (`blocks:`) renders its blocks in place of the hero, followed by its body only with `body: after`; the features, testimonials, custom content and call to action still follow. |
| `ShowHero` | `bool` | `true` | Whether to show a hero with the title, the description, the image and up to two buttons. |
| `HeroTitle` | `string?` | `null` | The hero's title when there is no `Content`. The item's title wins when both are set. |
| `HeroSubtitle` | `string?` | `null` | The hero's subtitle when there is no `Content`, or when the item has no description. |
| `HeroBackgroundImage` | `string?` | `null` | The hero's image address when the item has no featured image. |
| `HeroAlignment` | `string` | `"center"` | The alignment of the hero's text: `left`, `center` or `right`, in any case. Any other value is read as `center`. The string is kept in 4.x; an enum is planned for 5.0. |
| `HeroHeight` | `string` | `"70vh"` | The minimum height of the hero, as a CSS length such as `70vh` or `30rem`. |
| `ShowHeroButtons` | `bool` | `true` | Whether the hero shows its buttons. A button also needs its text and its address. |
| `HeroPrimaryButtonText` | `string?` | `null` | The text of the hero's primary button. The button needs `HeroPrimaryButtonUrl` too. |
| `HeroPrimaryButtonUrl` | `string?` | `null` | The address of the hero's primary button. |
| `HeroSecondaryButtonText` | `string?` | `null` | The text of the hero's secondary button. The button needs `HeroSecondaryButtonUrl` too. |
| `HeroSecondaryButtonUrl` | `string?` | `null` | The address of the hero's secondary button. |
| `ContentWidth` | `string` | `"normal"` | The width of the section that holds the item's body: `narrow`, `normal`, `wide` or `full`. |
| `ContentAlign` | `string` | `"left"` | The text alignment of the body: `left`, `center` or `right`. |
| `ContentVariant` | `string` | `"default"` | The look of the section that holds the body: `default`, `highlight`, `bordered` or `background`. |
| `ContentPadding` | `string` | `"normal"` | The padding of the section that holds the body: `none`, `small`, `normal` or `large`. |
| `ShowTestimonials` | `bool` | `false` | Whether to show the testimonials section, after the body and the features. It renders only when `TestimonialItems` has a testimonial with a quote. |
| `TestimonialsSectionTitle` | `string` | `"What Our Customers Say"` | The heading of the testimonials section. |
| `TestimonialsLayout` | `string` | `"grid"` | The layout of the testimonials: `grid`, cards in columns, or `list`, one under the other. Any other value is read as `grid`. |
| `ShowTestimonialImages` | `bool` | `true` | Whether each testimonial shows its `TestimonialItem.ImageUrl`, as a small round picture that carries no alternative text because the author's name stands beside it. |
| `TestimonialItems` | `IReadOnlyList<TestimonialItem>?` | `null` | The testimonials. One without a quote is skipped. |
| `ShowFeatures` | `bool` | `false` | Whether to show the features section, after the body. It renders only when `FeatureItems` has an item. |
| `FeaturesSectionTitle` | `string` | `"Features"` | The heading of the features section. |
| `FeaturesSectionDescription` | `string?` | `null` | The sentence under the features heading. None when null. |
| `FeaturesLayout` | `string` | `"grid"` | The layout of the features: `grid`, cards in `FeaturesColumns` columns, or `list`, one under the other. Any other value is read as `grid`. |
| `FeaturesColumns` | `int` | `3` | The number of columns of the `grid` layout, 1 to 6; a value outside the range is moved to the nearest one. Narrow screens show fewer columns. |
| `FeatureItems` | `IReadOnlyList<FeatureItem>?` | `null` | The features, each with an icon, a title that can link, and a description. |
| `ShowCallToAction` | `bool` | `true` | Whether to show the call to action, the last section of the page. It renders only when a title, a description or a button (text and address) is set. |
| `CtaTitle` | `string?` | `null` | The heading of the call to action. |
| `CtaDescription` | `string?` | `null` | The sentence under the call to action's heading. |
| `CtaPrimaryButtonText` | `string?` | `null` | The text of the call to action's primary button. The button needs `CtaPrimaryButtonUrl` too. |
| `CtaPrimaryButtonUrl` | `string?` | `null` | The address of the call to action's primary button. It is written as given, so build it from values you control. |
| `CtaSecondaryButtonText` | `string?` | `null` | The text of the call to action's secondary button. The button needs `CtaSecondaryButtonUrl` too. |
| `CtaSecondaryButtonUrl` | `string?` | `null` | The address of the call to action's secondary button. It is written as given, so build it from values you control. |
| `CtaBackgroundColor` | `string?` | `null` | A CSS color for the call to action's background: a hex value, `rgb()`, `hsl()`, a color name or a `var()` reference. Anything else is ignored, so the value cannot add CSS declarations. The text color stays that of `CtaVariant`, so choose a color that keeps the text readable. |
| `CtaVariant` | `string` | `"primary"` | The look of the call to action: `primary`, the theme's action color behind light text, or `secondary`, a quiet panel. Any other value is read as `primary`. |
| `CustomContent` | `string?` | `null` | HTML shown in a wide, highlighted section after the body. It is written through `OsirionHtmlRenderer`, which sanitizes it with the registered HTML sanitizer. |
| `LoadingText` | `string` | `"Loading page..."` | The text shown while the item loads, in interactive render modes only. A statically rendered response is sent once the item has loaded. |
| `NotFoundText` | `string` | `"Page content not found."` | The message shown when `ContentPath` finds no item. The page also signals 404. |
| `ChildContent` | `RenderFragment?` | `null` | Content shown in a wide section after the body and the custom content. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The `Theme` and `Style` rows come from the base class. `Class`, `Style`, `Theme` and unmatched attributes go to the root
element; `Theme` is written as `data-theme` (`data-bs-theme` under Bootstrap) when it is `Light` or `Dark`.

`TestimonialItem` has `Name`, `Role`, `Company`, `Quote`, `ImageUrl` and `Rating`, a number from 1 to 5 that the page
does not show. `FeatureItem` has `Title`, `Description`, `Icon` (text, such as a symbol, written in a decorative
element), `IconUrl` (a picture, drawn without alternative text) and `LinkUrl` (links the title). The addresses are
written as given, so build them from values you control.

## Behavior

- **Rendering.** The page renders on the server and needs no script. The loading text appears only in interactive
  render modes.
- **Order.** The page writes, in this order: the head metadata, the hero (`ShowHero`), the body section, the features,
  the testimonials, `CustomContent` in a wide highlighted section, `ChildContent` in a wide section and the call to
  action.
- **Head.** The page writes a `SeoMetadataRenderer` for the item and passes nothing else to it, so all of the
  renderer's other settings take their defaults.
- **Hero.** The hero shows the item's title and description (or `HeroTitle` and `HeroSubtitle`), the item's featured
  image (or `HeroBackgroundImage`) and up to two buttons, each shown when its text and its address are both set.
  `HeroHeight` sets its minimum height, `HeroAlignment` (`left`, `center`, `right`; anything else is `center`) aligns its
  text, and `ShowHeroButtons` turns the buttons off. The hero is centered by default, as the
  default of `HeroAlignment` says; it used to be aligned left, because the parameter never reached it. While the hero is shown, the body leaves out its own leading `h1`, so the page
  keeps a single `h1`.
- **Body section.** The body is written by `ContentSection` with `ContentWidth` (`narrow`, `normal`, `wide`, `full`),
  `ContentAlign` (`left`, `center`, `right`), `ContentVariant` (`default`, `highlight`, `bordered`, `background`) and
  `ContentPadding` (`none`, `small`, `normal`, `large`).
- **Features.** Each is a card with its icon, a title and a description, in `FeaturesColumns` columns (1 to 6) of the
  `grid` layout or stacked in the `list` layout. Narrow screens show fewer columns.
- **Testimonials.** Each is a quote with the author's name, role and company, and a small picture when
  `ShowTestimonialImages` is on, in the `grid` or `list` layout. A testimonial without a quote is skipped.
- **Call to action.** The last section: `CtaTitle`, `CtaDescription` and up to two buttons, each shown when its text and
  address are both set. `CtaVariant` is `primary` (the theme's action color behind its text color) or `secondary` (a
  quiet panel); any other value is `primary`. `CtaBackgroundColor` takes a hex value, `rgb()`, `hsl()`, a color name or
  a `var()` reference and ignores anything else; the text color stays that of the variant, so pick a color that keeps
  it readable.
- **Not found.** With a `ContentPath` that finds nothing, the page shows `NotFoundText` in a plain block and signals
  404, as `ArticlePage` does: `NavigationManager.NotFound()` on .NET 10, the status code of the request on .NET 9,
  while the response has not started. A page built from parameters alone, without a `ContentPath`, is not a missing
  page and answers 200.
- **Drafts.** An item loaded by `ContentPath` that is not published (`published: false`) is not served: the page
  renders the not-found state and signals 404, as for a missing item. `ShowUnpublished` turns this off for
  previews, and a `Content` you pass is shown as given.
- **Security.** The body and `CustomContent` pass through `OsirionHtmlRenderer`, which sanitizes them.

## Styling

The root element has the class `osirion-landing-page`. The hero is a `HeroSection` and the body, features,
testimonials, custom content and child content are `ContentSection` elements (`osirion-content-section`), which the
page's stylesheet spaces and sizes. The features are `osirion-landing-features` with `osirion-landing-feature-list`
and `osirion-landing-feature`, the testimonials `osirion-landing-testimonials` with `osirion-landing-testimonial-list`
and `osirion-landing-testimonial`, and the call to action `osirion-landing-cta` with `osirion-landing-cta-primary` or
`osirion-landing-cta-secondary`.
Colors come from theme tokens such as `--osirion-text-primary`, `--osirion-text-secondary`, `--osirion-action-primary`
and `--osirion-background-primary`.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [HeroSection](/components/hero-section): the hero and its parameters
- [HomePage](/components/home-page): a home page from one content item
- [Sections](/components/sections): pricing, steps, metrics and testimonials to use in `ChildContent`
