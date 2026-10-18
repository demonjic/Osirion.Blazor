---
id: 'osirion-accordion-and-faq'
order: 4
layout: docs
title: Accordion and FAQ
permalink: /components/accordion-and-faq
description: "OsirionAccordion builds collapsible sections on the native details element; OsirionFaq renders questions and answers with FAQPage structured data."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-accordion-and-faq.jpg'
categories:
- Components
- Core Components
- Content
tags:
- accordion
- faq
- structured-data
- json-ld
- static-ssr
- accessibility
slug: accordion-and-faq
lang: en
published: true
redirect_from:
- /docs/components/core/content/accordion-and-faq
custom_fields:
  group: content
seo_properties:
  title: 'OsirionAccordion and OsirionFaq: accordions for Blazor'
  description: 'OsirionAccordion builds collapsible sections on the native details element; OsirionFaq renders questions and answers with FAQPage structured data.'
  image: 'https://media.getosirion.com/og/components-accordion-and-faq.jpg'
  canonical: 'https://getosirion.com/components/accordion-and-faq'
  lang: en
  robots: index, follow
  og_title: 'Accordion and FAQ - Osirion.Blazor'
  og_description: 'Collapsible sections on the native details element, and an FAQ with structured data.'
  og_image_url: 'https://media.getosirion.com/og/components-accordion-and-faq.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Accordion and FAQ - Osirion.Blazor'
  twitter_description: 'Collapsible sections on the native details element, and an FAQ with structured data.'
  twitter_image_url: 'https://media.getosirion.com/og/components-accordion-and-faq.jpg'
  meta_tags: {}
---

# Accordion and FAQ

`OsirionAccordion` and `OsirionAccordionItem` render collapsible sections on the native `<details>` element, so they
open and close without script and work with the keyboard as they are. `OsirionFaq` uses them to render questions and
answers, and also writes FAQPage structured data so search engines can read the answers. Google stopped showing
FAQ rich results in May 2026, so the data no longer changes how the page looks in Google results.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionAccordion.razor). Available from 4.0.

```demo
accordion-and-faq
```

## Usage

### Accordion

```razor
@using Osirion.Blazor.Components

<OsirionAccordion>
    <OsirionAccordionItem Title="Requirements" Open="true">
        <p>.NET 9 or .NET 10.</p>
    </OsirionAccordionItem>
    <OsirionAccordionItem Title="Installation">
        <pre><code>dotnet add package Osirion.Blazor</code></pre>
    </OsirionAccordionItem>
</OsirionAccordion>
```

Set `AllowMultiple="false"` to keep at most one item open: opening one closes the others. An `OsirionAccordionItem`
also works on its own, outside an accordion, as a single collapsible section.

### FAQ

```razor
@using Osirion.Blazor.Components
@using Osirion.Blazor.Core.Models

<OsirionFaq Items="faq" AllowMultiple="false" />

@code {
    private readonly FaqItem[] faq =
    [
        new("Does Osirion.Blazor need JavaScript?",
            "No. Every component renders with static server-side rendering; script only adds enhancements."),
        new("Where does the content come from?",
            "Markdown files in a GitHub repository or in a local folder."),
    ];
}
```

`FaqItem` is a record in `Osirion.Blazor.Core.Models` with two values, `Question` and `Answer`.

## Parameters

### OsirionAccordion

<!-- osirion:parameters OsirionAccordion -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | `null` | The `OsirionAccordionItem` components. |
| `AllowMultiple` | `bool` | `true` | Whether several items can be open at once. When `false`, the items share a `name` and the browser closes the others when one opens; browsers without support for exclusive `details` elements let several stay open. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the container `<div>`. `Class` is added to the container. `Style` is accepted but the
component does not write it.

### OsirionAccordionItem

<!-- osirion:parameters OsirionAccordionItem -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | The title, shown in the `summary` element, which the visitor clicks or presses Enter on to open and close the section. Required. |
| `Open` | `bool` | `false` | Whether the section is open when the page renders. |
| `ChildContent` | `RenderFragment?` | `null` | The section's content, shown while the section is open. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes and `Class` go on the `<details>` element. `Style` is accepted but the component does not write
it.

### OsirionFaq

<!-- osirion:parameters OsirionFaq -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Items` | `IReadOnlyList<FaqItem>` | `[]` | The questions and answers, in the order they are shown. Answers are plain text: markup in an answer is shown as text, and line breaks are kept. Renders nothing when empty. |
| `IncludeStructuredData` | `bool` | `true` | Whether FAQPage structured data (JSON-LD) is written after the questions. Turn it off when the page already describes these questions in its own structured data, and use one FAQPage per page. |
| `AllowMultiple` | `bool` | `true` | Whether several answers can be open at once. When `false`, opening one answer closes the others, in browsers that support exclusive `details` elements. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`OsirionFaq` passes `Class` (next to `osirion-faq`) and unmatched attributes to its accordion. `Style` is accepted but
the component does not write it.

## Behavior

- Everything is native HTML: `<details>` and `<summary>` open and close in the browser with no script, in static SSR
  and in interactive render modes.
- One-at-a-time accordions rely on the `name` attribute of `<details>`. Browsers that do not support it let several
  items stay open; nothing else breaks.
- Answers in `OsirionFaq` are plain text, encoded like any Razor text, so HTML in an answer shows as text. Line breaks
  you write in an answer (`\n`) are kept on screen.
- The structured data is a `<script type="application/ld+json">` element with an FAQPage whose `mainEntity` lists each
  question with its accepted answer. It is built with System.Text.Json, whose default encoder escapes `<`, `>` and
  `&`, so a question or answer cannot close the script element. Text that cannot be written as JSON (half of a
  surrogate pair) leaves the structured data out, and the questions still render.
- Turn `IncludeStructuredData` off when the page already describes the same questions in its own structured data,
  and use one FAQPage per page.

## Styling

| Class | Element |
| --- | --- |
| `osirion-accordion` | The container |
| `osirion-accordion-item` | Each `<details>` |
| `osirion-accordion-summary` | The `<summary>`, with a chevron on the right in place of the native marker |
| `osirion-accordion-content` | The content below the summary |
| `osirion-faq` | Added to the container by `OsirionFaq` |
| `osirion-faq-answer` | The paragraph holding an answer |

The styles use the theme tokens `--osirion-border-color` (the lines between items), `--osirion-text-primary`,
`--osirion-text-secondary` (answers), `--osirion-action-primary` (summary hover color) and `--osirion-focus-ring-width`
with `--osirion-focus-ring-color` for the keyboard focus ring. Each has a fallback value, and a CSS framework adapter
that sets them restyles the accordion with the site. The chevron's rotation is not animated under
`prefers-reduced-motion: reduce`.

## Related

- [Tabs](/components/tabs)
- [Callouts](/components/callout)
- [Content components](/components/content)
