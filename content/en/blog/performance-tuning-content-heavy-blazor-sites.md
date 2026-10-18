---
title: "Performance Tuning for Content-Heavy Blazor Sites"
author: "Osirion Team"
date: "2025-01-25"
last_modified: 2026-10-11
description: "Make a Blazor content site fast with Osirion.Blazor: static SSR, in-memory content, webhook refresh, image loading, little script and compression."
slug: "performance-tuning-content-heavy-blazor-sites"
categories: [Performance]
tags: [Performance, Static SSR, Images, Caching, Core Web Vitals]
featured_image: 'https://media.getosirion.com/og/blog-performance-tuning-content-heavy-blazor-sites.jpg'
published: true
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/blog-performance-tuning-content-heavy-blazor-sites.jpg'
  title: "Performance Tuning for Content-Heavy Blazor Sites"
  description: "Static SSR, in-memory content, webhook refresh, image loading and minimal script for a fast Osirion.Blazor content site."
  type: "Article"
---

# Performance tuning for content-heavy Blazor sites

A content site is fast when the server answers quickly, the main image arrives early, nothing shifts while the page
loads, and the browser has little script to run. This article goes through each of these on a site built with
Osirion.Blazor and points out the settings that matter.

## Render on the server, ship little script

Static server-side rendering (static SSR) sends finished HTML and keeps no connection open per reader. It is the
default in a Blazor Web App, and Osirion's content pages are built for it: `ArticlePage`, `DocumentPage` and
`HomePage` load their item on the server and, since 4.0, render without streaming, so the first response holds the
whole article. Give an interactive render mode only to the components that need one, not to the router.

Most content components need no script at all. Available from 4.0:

- `OsirionTabs` switch with a radio group per tab set.
- `OsirionAccordion` and `OsirionFaq` use the native `<details>` element.
- `OsirionReadingProgress` is a CSS scroll-driven animation.
- `OsirionAnnouncementBar` closes through a form post that sets a cookie.

`OsirionVideoEmbed` (also from 4.0) does use a small script, but it shows only a thumbnail and loads the YouTube or
Vimeo player when the reader presses play; without script, the thumbnail links to the video.

Scripts that do load come in through the loader that `builder.Services.AddJSComponents()` registers, only on pages
that render the component. Syntax highlighting is one of them: when the HTML contains code blocks,
`OsirionHtmlRenderer` loads Prism 1.29.0 from cdnjs with Subresource Integrity. Set `PrismBaseUrl` to serve it from
your own site, or `EnableSyntaxHighlighting="false"` where plain code blocks are enough.

## Keep the content in memory

Each content provider loads all of its items once and answers every query from memory, so rendering a page does not
wait for GitHub or the disk. Readers who arrive while the first load is running wait for that one load instead of
starting their own, and a failed refresh keeps the content that was already loaded.

The GitHub provider reads the repository with one Git Trees request and downloads only the files whose SHA changed.
For a repository of 50 files in 10 folders, that is 3 to 13 requests per refresh, where earlier versions made about
80. If GitHub answers with a rate limit, the refresh stops and the current content stays.

Refresh when you push instead of on a timer. Register the webhook, map it, and set a secret:

```csharp
using Osirion.Blazor.Cms.Infrastructure.Extensions;

builder.Services.AddGitHubWebhookAndPolling();

var app = builder.Build();

app.UseGitHubWebhook();
```

```bash
dotnet user-secrets set "Osirion:Cms:Web:GitHub:<Name>:WebhookSecret" "<long random value>"
```

`<Name>` is the provider's name under `Osirion:Cms:Web:GitHub`. In the repository settings on GitHub, add a webhook for `https://<your host>/api/github/webhook` with content type
`application/json` and the same secret. Deliveries without a valid `X-Hub-Signature-256` signature are rejected, and
every delivery is rejected while no provider has a secret.

## Load the main image early and the rest late

Largest Contentful Paint is usually the featured image at the top of an article. `ArticlePage` shows it in its hero
with `fetchpriority="high"`. From 4.0, `ContentView` and `LocalizedContentView` load their featured image eagerly
with `fetchpriority="high"` too; in 4.0 it was lazy. Images in content lists stay lazy, which is what you want below
the fold.

For images you place in Razor, use `OsirionResponsiveImage` (available from 4.0). With `Width` and `Height` the
browser reserves the space before the file arrives, so the text below does not jump (Cumulative Layout Shift).
`Widths` and `SrcFormatter` build `srcset`, so a phone downloads a smaller file:

```razor
<OsirionResponsiveImage Src="/assets/images/dashboard-1600.jpg"
                        Alt="The dashboard with three charts"
                        Width="1600"
                        Height="1000"
                        Widths="widths"
                        SrcFormatter="@((src, width) => $"/assets/images/dashboard-{width}.jpg")"
                        Priority="true" />

@code {
    private readonly int[] widths = [640, 960, 1280, 1600];
}
```

`Priority` marks the page's main image: it loads at once with high fetch priority. Leave it off for every other image,
which then loads lazily with asynchronous decoding.

Images inside Markdown render as plain `<img>` elements, and the content sanitizer removes `srcset`, `sizes` and
`loading` from content HTML. Export in-content images at the width your article column shows them, and keep large or
above-the-fold images in Razor, where `OsirionResponsiveImage` can handle them.

## Compress and cache static files

The example app maps static files with `app.MapStaticAssets()`, which serves files compressed at build time and
lets fingerprinted URLs be cached for a long time. Reference stylesheets through `@Assets["..."]` in `App.razor` to
get the fingerprinted URL. For the HTML itself, the example adds response compression:

```csharp
using Microsoft.AspNetCore.ResponseCompression;

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

app.UseResponseCompression();
```

Compressing responses over HTTPS can expose secrets in pages that mix them with attacker-controlled input (BREACH);
public content pages are the usual safe case, so leave compression off for pages that show tokens or personal data.

The sitemap and feed endpoints from `MapOsirionFeeds` (available from 4.0) send `Cache-Control` with a maximum age
of 10 minutes by default; change it with `CacheDuration`.

## Be deliberate about analytics

Every tracker adds a third-party script. Render only the trackers you use, and consider the consent gate: with
`Osirion:Analytics:Consent:RequireConsent` set to `true`, no tracker script renders until the visitor accepts the
analytics category in `OsirionCookieConsent`. Available from 4.0: Plausible and Umami, both cookie-free. Umami
renders a single script tag; Plausible renders its script and a short inline initialization snippet.

## Measure before and after

Change one thing at a time and measure it. Lighthouse in Chrome DevTools shows Largest Contentful Paint and Cumulative
Layout Shift for one load; field data in PageSpeed Insights shows what real visitors see, including Interaction to
Next Paint. Measure a long article and a list page, on a throttled mobile profile, before and after each change.

## Related

- [Responsive image](/components/responsive-image)
- [Video embed](/components/video-embed)
- [HTML renderer](/components/html-renderer)
- [Blazor SEO checklist for static SSR content sites](/blog/blazor-seo-ssr-ssg-checklist)
- [Structuring content repositories](/blog/structuring-content-repositories)
