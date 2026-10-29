---
order: 5
title: ContentRenderer
permalink: /components/content-renderer
description: "Render only the sanitized HTML body of a content item, with code highlighting from OsirionHtmlRenderer, and build the rest of the page yourself."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-content-renderer.jpg'
categories:
- Components
- CMS Web Components
- Core Components
tags:
- content-renderer
- html
- sanitization
- syntax-highlighting
- static-ssr
slug: content-renderer
lang: en
layout: "component"
published: true
redirect_from:
- /docs/components/cms-web/core/content-renderer
custom_fields:
  group: cms-pages
seo_properties:
  title: 'ContentRenderer: a sanitized content body for Blazor'
  description: 'ContentRenderer renders only the sanitized HTML body of a CMS content item, with code highlighting, inside a layout you build.'
  image: 'https://media.getosirion.com/og/components-content-renderer.jpg'
  canonical: 'https://getosirion.com/components/content-renderer'
  lang: en
  robots: index, follow
  og_title: 'ContentRenderer - Osirion.Blazor'
  og_description: 'The body of a content item and nothing else: sanitized HTML with code highlighting, for your own article layout.'
  og_image_url: 'https://media.getosirion.com/og/components-content-renderer.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'ContentRenderer - Osirion.Blazor'
  twitter_description: 'The body of a content item and nothing else: sanitized HTML with code highlighting.'
  twitter_image_url: 'https://media.getosirion.com/og/components-content-renderer.jpg'
  meta_tags: {}
---

# ContentRenderer

`ContentRenderer` renders the body of a content item and nothing else: no title, metadata or links. Use it when you
lay out the header, sidebar and footer of an article yourself and only need the Markdown output in the middle.

Package `Osirion.Blazor.Cms.Web`, namespace `Osirion.Blazor.Cms.Web.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Cms.Web/Components/ContentRenderer.razor).

```demo
content-renderer
```

The demo loads this site's own page for the 1.5 release notes and renders its body in a scrolling frame, with
`OmitLeadingTitle` so the body's own title line is left out.

## Usage

```razor
@using Osirion.Blazor.Cms.Web.Components

<ContentRenderer Item="@item" />
```

A custom article layout around it. The page shows the title itself, so `OmitLeadingTitle` leaves out the heading line
the Markdown body starts with, and the page keeps a single `h1`:

```razor
@if (item is not null)
{
    <article class="post">
        <h1>@item.Title</h1>
        <p class="post-meta">@item.Author, @item.DateCreated.ToString("MMMM d, yyyy")</p>
        <ContentRenderer Item="@item" Class="post-body" OmitLeadingTitle />
    </article>
}
```

`ContentRenderer` does not load anything; get the item from `IContentProviderManager` or a page component first.
`ContentView` leaves out the leading title itself, and so does `ArticlePage` while its hero or header shows the title.

## Parameters

<!-- osirion:parameters ContentRenderer -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Item` | `ContentItem?` | `null` | The item to render. Its `Content` (HTML) is rendered sanitized, with code highlighting. An item with content blocks renders them with `OsirionContentBlocks` instead, followed by its `Content` only when its front matter has `body: after`. Only the empty wrapper renders when the item or its content is null. The component loads nothing, so get the item from the content provider first. |
| `OmitLeadingTitle` | `bool` | `false` | Whether to leave out the content's opening top-level heading (a Markdown `# Title` line). Set it when the page already shows the item's title as its heading, so the title appears once and the page has a single `h1`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

The `Theme` and `Style` rows come from the base class. `ContentRenderer` does not write them, or unmatched attributes, so only `Class` reaches the root element.

## Behavior

- **Rendering.** The HTML is passed to [OsirionHtmlRenderer](/components/html-renderer) with its
  default settings and rendered on the server under static SSR.
- **Security.** `OsirionHtmlRenderer` sanitizes the HTML with the registered `IHtmlContentSanitizer` before writing
  it, so script, event handler attributes, `javascript:` and `data:` URLs, forms, iframes and inline SVG never reach
  the page, whatever provider the item came from.
- **Code blocks.** When the HTML contains code blocks, the renderer adds Prism 1.29.0 from cdnjs (verified with
  Subresource Integrity) to highlight them. The code is readable without script; highlighting is an enhancement.
  Line numbers and the copy button are off. To change these settings, or to self-host Prism with `PrismBaseUrl`, use
  `OsirionHtmlRenderer` directly with `HtmlContent="@item.Content"`.
- **Live demos.** The renderer's `EnableDemos` default is on, so a fenced `demo` block in the body whose text is a
  registered demo key renders the live demo, as on this site. See [OsirionContentDemo](/components/content-demo).
- **SEO.** Nothing is written to the head.

## Styling

The root element is a `div` with the class `osirion-content-view` (the same class as `ContentView`), containing
`osirion-content-body` and the renderer's `article`. The body takes its line height and text color from the theme
tokens `--osirion-line-height-relaxed` and `--osirion-text-primary`. The renderer links its own stylesheet,
`_content/Osirion.Blazor.Core/css/osirion-html-renderer.min.css`, which is not scoped to a component and styles the
body's headings, paragraphs, lists, blockquotes, tables, code and images; that is where the body's typography comes
from. This component's own stylesheet sets only the body's line height, color and bottom margin: rules for the body's
elements that it used to carry never matched, because the body's elements do not have the component's scope
attribute.

## Related

- [CMS pages](/components/cms-pages): the group overview
- [ContentView](/components/content-view): the body plus header, metadata and footer
- [HTML renderer](/components/html-renderer): highlighting options and Prism settings
