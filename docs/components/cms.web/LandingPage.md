# LandingPage

Purpose
A marketing page template. It shows a hero, then the Markdown body of a content item as a `ContentSection`, optional
features and testimonials sections, an optional block of custom HTML as a highlighted section, `ChildContent` as a
section and an optional call to action as the last section. The item comes from `Content`, or
is loaded from the default content provider by `ContentPath`. Without an item, the hero text comes from `HeroTitle` and
`HeroSubtitle`. It works for pages that are not a blog, a document or a home page; `HomePage` is the one for the site
root.

An item with content blocks (`blocks:` in its front matter, see the content blocks guide at `/docs/content-blocks`)
renders its blocks with `OsirionContentBlocks` in place of the hero and the body section, followed by the body only with
`body: after`. The features, testimonials, custom content, `ChildContent` and call to action still follow.

Key parameters
Used by the page:
- ContentPath: `string?`, the path of the content item, loaded with `GetItemByPathAsync` on the default provider when
  `Content` is null
- Content: `ContentItem?`, the item to show; when set, nothing is loaded
- ShowHero: `bool`, default `true`
- HeroTitle, HeroSubtitle: `string?`, used when there is no content item (the item's title and description win)
- HeroBackgroundImage: `string?`, passed to the hero as `ImageUrl` when the item has no featured image
- HeroHeight: `string`, default `"70vh"`, passed to the hero as `MinHeight`
- HeroPrimaryButtonText, HeroPrimaryButtonUrl, HeroSecondaryButtonText, HeroSecondaryButtonUrl: `string?`
- HeroAlignment: `string`, default `"center"`: `left`, `center` or `right` (any case); anything else is `center`
- ShowHeroButtons: `bool`, default `true`; turns both hero buttons off when `false`
- ContentWidth: `string`, default `"normal"`
- ContentAlign: `string`, default `"left"`, passed as `TextAlign`
- ContentVariant: `string`, default `"default"`
- ContentPadding: `string`, default `"normal"`
- CustomContent: `string?`, HTML shown in a `ContentSection` with width `wide`, variant `highlight` and padding `large`
- ChildContent: `RenderFragment?`, shown in a `ContentSection` with width `wide` and padding `normal`
- LoadingText: `string`, default `"Loading page..."`
- NotFoundText: `string`, default `"Page content not found."`
- Class: `string?`, extra CSS classes on the root, from `OsirionComponentBase`

Sections (each renders only when it has something to show):
- Features: `ShowFeatures` (default `false`), `FeaturesSectionTitle` (default `"Features"`), `FeaturesSectionDescription`,
  `FeaturesLayout` (`"grid"` or `"list"`), `FeaturesColumns` (`int`, default `3`, 1 to 6), `FeatureItems`
  (`IReadOnlyList<FeatureItem>?`)
- Testimonials: `ShowTestimonials` (default `false`), `TestimonialsSectionTitle` (default `"What Our Customers Say"`),
  `TestimonialsLayout` (`"grid"` or `"list"`), `ShowTestimonialImages` (default `true`), `TestimonialItems`
  (`IReadOnlyList<TestimonialItem>?`; an item without a quote is skipped)
- Call to action: `ShowCallToAction` (default `true`), `CtaTitle`, `CtaDescription`, `CtaPrimaryButtonText`,
  `CtaPrimaryButtonUrl`, `CtaSecondaryButtonText`, `CtaSecondaryButtonUrl`, `CtaBackgroundColor` (a color; anything else
  is ignored), `CtaVariant` (`"primary"` or `"secondary"`)
- `Class`, `Style`, `Theme` and unmatched attributes go to the root element.

Usage
```razor
@page "/launch"
@using Osirion.Blazor.Cms.Components

<LandingPage ContentPath="pages/launch.md"
             HeroPrimaryButtonText="Get started"
             HeroPrimaryButtonUrl="/docs"
             HeroSecondaryButtonText="See the code"
             HeroSecondaryButtonUrl="https://github.com/demonjic/Osirion.Blazor"
             ContentWidth="wide"
             ContentAlign="center"
             NotFoundText="This page is not published yet.">
    <p>Questions? Write to the team.</p>
</LandingPage>
```

Notes
- Not routable: the host page declares the route. Namespace `Osirion.Blazor.Cms.Components`.
- Renders `div.osirion-landing-page` (plus `Class`), then `HeadContent` with `SeoMetadataRenderer` for the item, the
  `HeroSection` when `ShowHero` is on, `ContentSection` for the item body (the body's leading title heading is dropped
  while the hero shows the title), the features and testimonials sections, the custom content section, the
  `ChildContent` section and the call to action (`section.osirion-landing-cta`).
- Loading: the page loads only when `Content` is null and `ContentPath` is not blank. The `div.osirion-loading`
  spinner with `LoadingText` shows only while loading and the component is interactive. When `Content` is still null
  after loading and `ContentPath` is set (no default provider, nothing found at the path, or the provider threw), it
  shows `div.osirion-not-found` with `NotFoundText` instead of the sections and signals 404 (`NavigationManager.NotFound()`
  on .NET 10, the status code of the request on .NET 9). A provider exception is logged, not shown. A page without a
  `ContentPath` answers 200.
- Services the host must register: `IContentProviderManager` (`GetDefaultProvider()` is used) and the logging
  factory. The page has no render mode of its own and works in static SSR; the tests render it statically and
  interactively.
- `TestimonialItem` has `Name`, `Role`, `Company`, `Quote`, `ImageUrl` and `Rating` (default `5`); `FeatureItem` has
  `Title`, `Description`, `Icon`, `IconUrl` and `LinkUrl`. Both are defined in the same file. The page does not show
  `Rating`.
