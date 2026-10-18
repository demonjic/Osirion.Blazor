---
title: "New in Osirion.Blazor 4.0: Content Components"
author: "Dejan Demonjić"
date: "2026-10-11"
last_modified: 2026-10-18
description: "Osirion.Blazor 4.0 adds tabs, accordions and FAQs, callouts, responsive images, video embeds, author pages, series, feeds and more."
slug: "osirion-blazor-4-0-components"
categories: [Releases]
tags: [Release Notes, Components, Static SSR, Feeds, Analytics]
featured_image: 'https://media.getosirion.com/og/blog-osirion-blazor-4-0-components.jpg'
is_featured: false
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-osirion-blazor-4-0-components.jpg'
  title: "New in Osirion.Blazor 4.0: Content Components"
  description: "The components and features new in Osirion.Blazor 4.0, each with a short example and a link to its documentation."
  type: "Article"
---

# New in Osirion.Blazor 4.0: content components

Version 4.0 adds the building blocks content sites ask for most: tabs, accordions and FAQs, callouts, responsive
images, video embeds, author pages, series, pricing tables, steps, an announcement bar, a reading progress bar, a
sitemap with RSS and Atom feeds, and two cookie-free analytics providers. All of them render with static SSR, and most
need no script at all.

Every one of them has a page with a live demo, all listed in the component gallery:

![The component gallery on getosirion.com](https://media.getosirion.com/shots/blog-osirion-blazor-4-0-components/gallery-dark.webp){width=1600 height=1125}

> [!NOTE]
> The same release also hardens the CMS and its security; see
> [Osirion.Blazor 4.0: security hardening and Fluent UI v5](/blog/osirion-blazor-4-0-release).

The Razor examples assume `@using Osirion.Blazor.Components` and, for the item types, `@using Osirion.Blazor.Core.Models`.

## Callouts

GitHub-style alerts in Markdown are now styled, and `OsirionCallout` renders the same look from Razor. Their icons
are drawn in CSS, so the HTML sanitizer can keep removing inline SVG.

```markdown
> [!WARNING]
> Back up the database before you run the migration.
```

```razor
<OsirionCallout Type="CalloutType.Warning" Title="Back up first">
    <p>Back up the database before you run the migration.</p>
</OsirionCallout>
```

[Callout documentation](/components/callout)

## Tabs

`OsirionTabs` switch without script, through a radio group per tab set, and the arrow keys move between tabs.

```razor
<OsirionTabs AriaLabel="Install command">
    <OsirionTab Title=".NET CLI">
        <pre><code>dotnet add package Osirion.Blazor</code></pre>
    </OsirionTab>
    <OsirionTab Title="Package Manager">
        <pre><code>Install-Package Osirion.Blazor</code></pre>
    </OsirionTab>
</OsirionTabs>
```

[Tabs documentation](/components/tabs)

## Accordion and FAQ

`OsirionAccordion` is built on the native `<details>` element; set `AllowMultiple="false"` to keep one item open at a
time. `OsirionFaq` renders questions and answers and writes matching FAQPage structured data.

```razor
<OsirionAccordion AllowMultiple="false">
    <OsirionAccordionItem Title="Which .NET versions are supported?" Open="true">
        <p>.NET 9 and .NET 10.</p>
    </OsirionAccordionItem>
    <OsirionAccordionItem Title="Where does the content come from?">
        <p>Markdown files in a GitHub repository or a local folder.</p>
    </OsirionAccordionItem>
</OsirionAccordion>

<OsirionFaq Items="faq" />

@code {
    private readonly FaqItem[] faq =
    [
        new("Does the FAQ need JavaScript?", "No. It is built on the native details element and renders with static SSR."),
    ];
}
```

[Accordion and FAQ documentation](/components/accordion-and-faq)

## Responsive images

`OsirionResponsiveImage` renders an `<img>` with intrinsic `Width` and `Height`, so nothing shifts while it loads. It
builds `srcset` from `SrcSet` or from `Widths` with a `SrcFormatter`, loads lazily with async decoding, and loads at
once with `fetchpriority="high"` when `Priority` is set.

```razor
<OsirionResponsiveImage Src="/assets/images/cover-1600.jpg"
                        Alt="The dashboard with three charts"
                        Width="1600"
                        Height="1000"
                        Widths="widths"
                        SrcFormatter="@((src, width) => $"/assets/images/cover-{width}.jpg")"
                        Priority="true" />

@code {
    private readonly int[] widths = [640, 960, 1280, 1600];
}
```

[Responsive image documentation](/components/responsive-image)

## Video embeds

`OsirionVideoEmbed` shows a YouTube or Vimeo thumbnail and loads the player only when the visitor presses play. It is
privacy-enhanced by default (`youtube-nocookie.com`, Vimeo `dnt=1`), and without script the thumbnail links to the
video. It needs `builder.Services.AddJSComponents()`.

```razor
<OsirionVideoEmbed VideoId="V0Af7y7aMBE" Title="Build better web apps with Blazor in .NET 10" />
```

[Video embed documentation](/components/video-embed)

## Reading progress

`OsirionReadingProgress` is a bar at the top of the window that fills as the reader scrolls. It is a CSS
scroll-driven animation with no script, and browsers without `animation-timeline` do not show it.

```razor
<OsirionReadingProgress />
```

[Reading progress documentation](/components/reading-progress)

## Author pages

`OsirionAuthorCard` shows an avatar or initials, the name linked with `rel="author"`, a role, a bio and profile links
with `rel="me"`; links with unsafe URL schemes are not rendered. `ContentList.Author` lists one author's items, and
`ContentView.AuthorUrlFormatter` links bylines to the author's page.

```razor
<OsirionAuthorCard Name="Jane Doe"
                   Role="Maintainer"
                   Bio="Writes about static SSR and content sites."
                   ProfileUrl="/authors/jane-doe"
                   Links="links" />

<ContentList Directory="blog" Author="Jane Doe" />

@code {
    private readonly AuthorLink[] links = [new("GitHub", "https://github.com/demonjic/Osirion.Blazor")];
}
```

`ContentList` lives in `Osirion.Blazor.Cms.Web.Components`.

[Author card documentation](/components/author-card)

## Series

Two new front matter keys group posts into a series, and `ContentSeriesNavigation` shows "Part 2 of 5", lists the
parts and links the previous and next part.

```yaml
series: "Building a docs site"
series_order: 2
```

Pass it the `ContentItem` the page shows; it renders nothing for an item outside a series. Like `ContentList`, it
lives in `Osirion.Blazor.Cms.Web.Components`.

```razor
<ContentSeriesNavigation Item="@post" />
```

[Series navigation documentation](/components/series-navigation)

## Pricing tables and feature comparison

`OsirionPricingTable` shows plans side by side with a highlighted plan, and `OsirionFeatureComparison` renders an
accessible table with check marks, crosses and values.

```razor
<OsirionPricingTable Plans="plans" />

@code {
    private readonly PricingPlan[] plans =
    [
        new() { Name = "Community", Price = "Free", Features = ["All components"], CallToActionText = "Install", CallToActionUrl = "https://www.nuget.org/packages/Osirion.Blazor" },
        new() { Name = "Team", Price = "$29", Period = "per month", Features = ["Priority issues"], Highlighted = true, Badge = "Most popular" },
    ];
}
```

[Pricing table documentation](/components/pricing-table)

## Steps

`OsirionSteps` renders numbered "how it works" steps as an ordered list, side by side or stacked with a joining line.

```razor
<OsirionSteps Items="steps" Vertical="true" />

@code {
    private readonly StepItem[] steps =
    [
        new("Install the package", "dotnet add package Osirion.Blazor"),
        new("Point it at your content", "A GitHub repository or a local folder of Markdown files."),
        new("Publish"),
    ];
}
```

[Steps documentation](/components/steps)

## Announcement bar

`OsirionAnnouncementBar` is a site-wide announcement that closes without script: an antiforgery-protected form post
sets a cookie for that announcement and redirects back. Map the endpoint once; like the cookie consent endpoint, it
needs `app.UseAntiforgery()` in the pipeline before the endpoints.

```razor
<OsirionAnnouncementBar Id="release-4-0">
    Osirion.Blazor 4.0 is out. <a href="/blog/osirion-blazor-4-0-components">See what is new</a>
</OsirionAnnouncementBar>
```

```csharp
using Osirion.Blazor.Core.Extensions;

app.MapOsirionAnnouncements();
```

[Announcement bar documentation](/components/announcement-bar)

## Sitemap and feeds

`MapOsirionFeeds` in Osirion.Blazor.Cms.Web maps `/sitemap.xml`, an RSS 2.0 feed at `/feed.xml` and an Atom feed at
`/atom.xml` from the published content. Items marked `noindex` stay out of the sitemap.

```csharp
using Osirion.Blazor.Cms.Web.Feeds;

app.MapOsirionFeeds(feeds =>
{
    feeds.SiteUrl = "https://example.com";
    feeds.FeedDirectory = "blog";
});
```

[Sitemap and feeds documentation](/components/feeds-and-sitemap)

## Plausible and Umami

Osirion.Blazor.Analytics adds two cookie-free providers. Both read their settings from configuration, are validated
at startup and honor the consent gate from 4.0. Render `<PlausibleTracker />` or `<UmamiTracker />` in the page head.

```json
{
  "Osirion": {
    "Analytics": {
      "Plausible": {
        "ScriptUrl": "https://plausible.io/js/pa-XXXXX.js"
      },
      "Umami": {
        "SiteId": "your-website-id"
      }
    }
  }
}
```

[Analytics documentation](/docs/packages/analytics)

## Also in 4.0

- `OsirionPageLayout` renders a skip link to the main content as its first focusable element, shown only while it
  has keyboard focus (`ShowSkipLink`, `SkipLinkText`, `MainContentId`).
- A content item's `LastModified` comes from the `last_modified` front matter key, then the file's write time, then
  the publication date. In 4.0 it was the time the content was loaded.
- The featured image at the top of `ContentView` and `LocalizedContentView` loads eagerly with
  `fetchpriority="high"`; images in lists stay lazy.
- A tracker whose provider is not registered renders nothing instead of failing the page.
- Text on the primary color keeps its contrast in Bootstrap dark mode, `OsirionFooter` inside the page layout no
  longer adds a second footer landmark, and the cookie consent banner is a named region.

The [changelog](https://github.com/demonjic/Osirion.Blazor/blob/master/CHANGELOG.md) lists every change.

## Related

- [Osirion.Blazor 4.0: security hardening and Fluent UI v5](/blog/osirion-blazor-4-0-release)
- [Content components](/components/content)
- [Osirion.Blazor on NuGet](https://www.nuget.org/packages/Osirion.Blazor)
