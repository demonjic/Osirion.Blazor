---
id: 'guide-content-blocks'
order: 7
layout: docs
title: "Content blocks"
permalink: /docs/content-blocks
description: "Write a marketing page as front matter: a list of blocks (hero, sections, steps, cards, metrics, FAQ, call to action) that renders with Osirion components, validated when the content loads."
author: Dejan Demonjić
date: 2026-10-29
last_modified: 2026-10-29
featured_image: 'https://media.getosirion.com/og/docs-content-blocks.jpg'
categories:
- Documentation
- Guides
tags:
- content
- front matter
- blocks
- landing page
published: true
slug: content-blocks
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-content-blocks.jpg'
  title: 'Content blocks: pages written as front matter'
  description: 'Compose a page from Osirion components in Markdown front matter, with validation that names the file, the block and the key.'
  image: 'https://media.getosirion.com/og/docs-content-blocks.jpg'
  canonical: 'https://getosirion.com/docs/content-blocks'
  robots: index, follow
---

# Content blocks

A content item whose front matter has `blocks:` renders as a page composed of Osirion components, in the order of the
list, instead of its Markdown body. A company site writes a landing page, a product page or a pricing page as a
Markdown file, and needs no Razor page for each one. It arrives in the release after 4.2.0.

```yaml
---
title: "Decisions your plant can explain"
description: "The decision system of record for an industrial workforce."
slug: home
blocks:
  - type: hero
    title: "Every workforce decision, made, enforced, recorded and explained"
    subtitle: "Access, attendance and presence at the site."
    primary_action: { text: "Book a diagnostic", url: "/diagnostic" }
    secondary_action: { text: "Take the product tour", url: "/demo" }
    image: { src: "/images/hero.webp", alt: "A shift board in a plant" }
  - type: steps
    title: "How a decision moves"
    items:
      - { title: "Made", description: "A rule decides." }
      - { title: "Recorded", description: "The decision is stored with its reason." }
  - type: cta
    title: "See it on your own data"
    primary_action: { text: "Book a diagnostic", url: "/diagnostic" }
---
```

## Where blocks render

- `ContentRenderer`, which `ArticlePage`, `AboutPage`, `ContactPage` and `DocumentPage` use for the body, renders the
  blocks of an item that has them.
- `LandingPage` renders the blocks in place of its own hero and body section; its features, testimonials, custom
  content and call to action still follow.
- `OsirionContentBlocks` renders a list of blocks anywhere: `<OsirionContentBlocks Blocks="@item.Blocks" SourcePath="@item.Path" />`.

Only the blocks render, unless the front matter says `body: after`: then the Markdown body follows the blocks. An item
without `blocks:` renders exactly as before. A `hero` block writes the page's `h1`, so use it on a page that does not
write its own heading (`LandingPage`, or a page of your own).

## Common keys

| Key | Type | Meaning |
| --- | --- | --- |
| `type` | text, required | The block type, lower case words joined by hyphens |
| `id` | text | The section's anchor, `[a-z0-9-]+`, unique in the file |
| `variant` | `default`, `tinted`, `dark` | The band style (rendered once the band styles are in the library) |
| `eyebrow` | text | A small label above the title (rendered once the band styles are in the library) |
| `title` | text | The block heading (`h2`, `h1` for `hero`) |
| `description` | inline Markdown | The lead paragraph under the title |

Titles, labels and button texts are plain text: inline Markdown markers in them are removed. Descriptions,
subtitles and captions are inline Markdown (emphasis, links, code) where the component accepts markup. `markdown` values
and FAQ answers are block Markdown, rendered with the extensions page bodies use. Every piece of HTML goes through the
site's `IHtmlContentSanitizer`, the sanitizer of page bodies, so a `<script>` in content never reaches the page.

An action is `{ text, url }`. A link is a site path (`/platform`, `/platform#govern`), a fragment of the page
(`#pricing`), an `https:` URL, `mailto:` or `tel:`. An image `src` is a site path or an `https:` URL. Anything else,
such as `javascript:`, `data:`, `http:` or `//host`, is an error.

When `IOsirionLocalePaths` is registered (see [Locale routing](/docs/locale-routing)), site paths in actions, card
links and Markdown links are localized for the culture of the request: a page under `/sr/` links to `/sr/platform`.
Without it, links stay as written.

## Block types

| `type` | Keys besides the common ones | Rendered by |
| --- | --- | --- |
| `hero` | `subtitle`, `highlights`, `primary_action`, `secondary_action`, `image: { src, alt }`, `aside` (an `image` or a `record` block) | `HeroSection` |
| `section` | `markdown` | `OsirionBaseSection` |
| `split` | `markdown`, `aside` (a `record`, `checklist`, `image`, `metrics` or `markdown` block), `aside_position: end` or `start` | coming with `OsirionSplitSection` |
| `steps` | `items: [{ title, description }]` | `OsirionSteps` |
| `cards` | `columns: 2, 3 or 4` (3 by default), `items: [{ title, description, icon, url, link_text, items }]` | `OsirionFeatureCard` in one grid |
| `checklist` | `items: [{ text, description }]` | coming with `OsirionChecklist` |
| `metrics` | `items: [{ value, label, description }]` | `OsirionMetricGrid` |
| `faq` | `items: [{ question, answer }]`, `answer` in block Markdown | `OsirionFaq` |
| `cta` | `primary_action`, `secondary_action` | `OsirionCallToActionSection` |
| `record` | `status`, `tone` (`neutral`, `success`, `warning`, `danger`, `info`), `rows: [{ label, value }]`, `caption` | coming with `OsirionRecordCard` |
| `image` | `src`, `alt` (required; `alt: ""` for a decorative image), `caption` | a `figure` |
| `markdown` | `markdown` | the sanitized Markdown in a plain section |
| `component` | `name`, `parameters: { key: value }` | the site's component registered under `name` |

`split`, `checklist` and `record` are read and validated today. Their components are on the way; until then a block of
one of these types renders nothing and logs a warning that names the type, unless the site registers a renderer for it.
A card's `icon` and `items`, a hero's `highlights` and `aside: { type: record }`, and the `variant` and `eyebrow` of
every block are read and validated the same way and render once their components support them.

The call to action section shows a button only for a site path, an `http:`, `https:` or `mailto:` address, so a `tel:`
or `#fragment` action of a `cta` block is not shown; use it in a `hero` instead.

## FAQ structured data

The first `faq` block of a page writes FAQPage structured data with the plain text of its answers; later `faq` blocks
do not, so a page has one FAQPage. `SeoMetadataRenderer` leaves out its generated FAQ for an item with a `faq` block.
When a page writes its own FAQPage, set `IncludeFaqStructuredData="false"` on `OsirionContentBlocks`.

## Errors

The blocks are read when the content loads. A file whose blocks break a rule is left out, as a file with broken YAML is,
and the error log names every problem with the file, the block and the key:

```text
The blocks of 'en/home.md' are not valid (2 problem(s)):
  blocks[0].primary_action.url: 'javascript:alert(1)' is not an allowed link; use a site-relative path (/page), a fragment (#section), an https: URL, mailto: or tel:
  blocks[2].columns: '5' is not one of: 2, 3, 4
```

The checks: `type` is required; the values of `variant`, `tone`, `columns`, `aside_position` and `body`; the `id`
pattern and uniqueness; the link rules; the required keys of each type (a step's `title`, an answer, an image's `src`
and `alt`, an action's `text` and `url`); and a key the type does not have, which is usually a misspelling.

A page is never served half built. What can only be known when the page renders (a type without a renderer and no
fallback, a component name the site did not register) is logged as an error and renders nothing; in Development the
block shows a notice with the file, the block and the reason instead. Nothing from the content is ever written as raw
HTML.

## Site renderers and components

```csharp
builder.Services.AddOsirionContentBlocks(blocks => blocks
    .AddRenderer<PricingTableRenderer>("pricing-table")  // a new type, or a replacement for a library type
    .AddComponent<ProductTour>("product-tour")            // what type: component, name: product-tour renders
    .UseFallbackRenderer<UnknownBlockRenderer>());        // every type that has no renderer
```

A renderer derives from `ContentBlockRenderer<TBlock>` and reads its block from `Model`; a renderer of a type the
library does not define receives a `CustomContentBlock`, whose `Values` hold the block's other keys as written. Its
`Context` renders Markdown (`InlineMarkdown`, `Markdown`, `PlainText`), localizes links (`Link`), renders a nested
block (`RenderBlock`) and reports a block it cannot render (`Problem`).

```csharp
public sealed class PricingTableRenderer : ContentBlockRenderer<CustomContentBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "id", Model.Id);
        builder.AddContent(2, Context.PlainText(Model.Title));
        builder.CloseElement();
    }
}
```

A component registered with `AddComponent` receives the block's `parameters` when it declares
`[Parameter] public IReadOnlyDictionary<string, string> Parameters { get; set; }`. Parameters given to a component
without that parameter are an error.

## Derived locales

A [derived locale](/docs/localization) transliterates the text of an item's blocks with the rest of the item: titles,
descriptions, eyebrows, subtitles, highlights, item texts, labels, values, captions, statuses, questions, answers,
button and link texts, `alt` and `markdown`. `type`, `id`, `variant`, `tone`, `columns`, `icon`, `name`, every `url` and
`src`, a component's `parameters` and the keys of a custom block stay as written. Links are localized when the page
renders, so they are not moved when the item is derived.
