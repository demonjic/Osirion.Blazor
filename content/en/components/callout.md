---
order: 2
title: OsirionCallout
permalink: /components/callout
description: "Highlight a note, tip, important point, warning or caution in content, written in Markdown with GitHub's alert syntax or with the OsirionCallout component."
author: Dejan Demonjić
date: 2026-10-11
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-callout.jpg'
categories:
- Components
- Core Components
- Content
tags:
- callout
- alert
- markdown
- accessibility
slug: callout
published: true
redirect_from:
- /docs/components/core/content/callout
custom_fields:
  group: content
seo_properties:
  title: 'OsirionCallout: notes, tips and warnings for Blazor'
  description: 'Highlight a note, tip, important point, warning or caution in content, written in Markdown with GitHub''s alert syntax or with the OsirionCallout component.'
  image: 'https://media.getosirion.com/og/components-callout.jpg'
  canonical: 'https://getosirion.com/components/callout'
  lang: en
  robots: index, follow
  og_title: 'OsirionCallout - Osirion.Blazor'
  og_description: 'Notes, tips, important points, warnings and cautions, from Razor or Markdown. Renders without script.'
  og_image_url: 'https://media.getosirion.com/og/components-callout.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionCallout - Osirion.Blazor'
  twitter_description: 'Notes, tips, important points, warnings and cautions, from Razor or Markdown. Renders without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-callout.jpg'
  meta_tags: {}
---

# OsirionCallout

A callout highlights a note, tip, important point, warning or caution inside content. Write it in Markdown with
GitHub's alert syntax, or render it from a Razor page with `OsirionCallout`. Both look the same, follow the theme and
the CSS framework adapter, and need no script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Content/OsirionCallout.razor). Available from 4.0.

```demo
callout
```

## Usage

### In Markdown

Start a blockquote with the kind in brackets:

```markdown
> [!NOTE]
> Content is read from the `content` folder in Development.
```

The five kinds render like this:

> [!NOTE]
> Useful information the reader should notice even when skimming.

> [!TIP]
> Optional advice that helps the reader do something better or more easily.

> [!IMPORTANT]
> Key information the reader needs to succeed.

> [!WARNING]
> Urgent information that needs the reader's attention to avoid problems.

> [!CAUTION]
> Advice about the risks or negative outcomes of an action.

### In Razor

```razor
@using Osirion.Blazor.Components

<OsirionCallout Type="CalloutType.Warning" Title="Back up first">
    <p>Back up the database before you run the migration.</p>
</OsirionCallout>
```

## Parameters

<!-- osirion:parameters OsirionCallout -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Type` | `CalloutType` | `CalloutType.Note` | The kind of callout: `Note`, `Tip`, `Important`, `Warning` or `Caution`. It sets the color, the icon and the default `Title`. |
| `Title` | `string?` | `null` | The heading shown above the content. When empty or white space, the name of the `Type` is shown ("Note", "Warning" and so on), as Markdown alerts show it; set it for other languages or a more specific heading. |
| `ChildContent` | `RenderFragment?` | `null` | The callout's content, rendered in the body under the title. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes go on the root `<div>`. `Style` is accepted but the component does not write it; use `Class`.

## Behavior

- The component renders `role="note"`, so assistive technology announces it as a note without interrupting the
  reader the way an alert role would.
- The title is text, not color: the color and icon only reinforce it.
- The title keeps the normal text color; the kind's color is used for the icon and border, so light colors such as
  amber never carry text.
- The icon is drawn in CSS, so the HTML sanitizer can keep removing inline SVG from content.
- The root element is `<div class="osirion-callout osirion-callout-{type}">` with the title in a
  `<p class="osirion-callout-title">` and the content in `<div class="osirion-callout-body">`.

## Styling

Both forms share the `--osirion-callout-color` and `--osirion-callout-icon` custom properties. Each kind takes its
color from the theme's status tokens (`--osirion-status-info`, `--osirion-status-success`, `--osirion-action-primary`,
`--osirion-status-warning`, `--osirion-status-danger`), so a framework adapter recolors callouts with the rest of the
site. Override a kind with its class, for example `.osirion-callout-tip, .markdown-alert-tip { --osirion-callout-color: #0e7490; }`.
The rules live in the Core stylesheet (`css/utilities/callouts.scss`), not in scoped component CSS, so that the
Markdown alerts, which the CMS renders without a component, get the same look.

## Related

- [Accordion and FAQ](/components/accordion-and-faq)
- [Tabs](/components/tabs)
- [Content components](/components/content)
