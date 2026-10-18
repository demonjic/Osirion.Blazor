---
id: 'osirion-html-renderer'
order: 8
layout: "component"
title: OsirionHtmlRenderer
permalink: /components/html-renderer
description: "OsirionHtmlRenderer sanitizes and renders HTML in Blazor, with Prism syntax highlighting loaded with SRI, line numbers, copy buttons and live demos."
author: "Osirion Team"
date: 2025-01-26
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-html-renderer.jpg'
categories:
- Components
- Core Components
- Rendering
tags:
- html
- sanitization
- syntax-highlighting
- prism
- security
published: true
slug: html-renderer
lang: en
redirect_from:
- /docs/components/core/rendering/html-renderer
custom_fields:
  group: content
seo_properties:
  title: 'OsirionHtmlRenderer: sanitized HTML for Blazor'
  description: 'OsirionHtmlRenderer sanitizes and renders HTML in Blazor, with Prism syntax highlighting loaded with SRI, line numbers, copy buttons and live demos.'
  image: 'https://media.getosirion.com/og/components-html-renderer.jpg'
  canonical: 'https://getosirion.com/components/html-renderer'
  lang: en
  robots: index, follow
  og_title: 'OsirionHtmlRenderer - Osirion.Blazor'
  og_description: 'Sanitized HTML rendering for Blazor with optional syntax highlighting for code blocks.'
  og_image_url: 'https://media.getosirion.com/og/components-html-renderer.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionHtmlRenderer - Osirion.Blazor'
  twitter_description: 'Sanitized HTML rendering for Blazor with optional syntax highlighting for code blocks.'
  twitter_image_url: 'https://media.getosirion.com/og/components-html-renderer.jpg'
  meta_tags: {}
---

# OsirionHtmlRenderer

`OsirionHtmlRenderer` sanitizes an HTML string and writes it into the page. When the HTML contains code blocks, it
also loads Prism to highlight them, with optional line numbers and copy buttons. Use it for HTML you hold outside
the CMS, such as Markdown you converted yourself or a field from an API.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Rendering/OsirionHtmlRenderer.razor).

```demo
html-renderer
```

## Usage

```razor
@using Osirion.Blazor.Components

<OsirionHtmlRenderer HtmlContent="@html" />

@code {
    private string html = "<h2>Release notes</h2><p>Version 4.0 sanitizes content by default.</p>";
}
```

Code blocks are `<pre><code class="language-...">` elements, the form Markdown processors produce. With line numbers
and copy buttons:

```razor
<OsirionHtmlRenderer HtmlContent="@articleHtml"
                     ShowLineNumbers="true"
                     EnableCopyButton="true" />

@code {
    private string articleHtml =
        """
        <p>Register the services:</p>
        <pre><code class="language-csharp">builder.Services.AddOsirion(builder.Configuration);</code></pre>
        """;
}
```

## Parameters

<!-- osirion:parameters OsirionHtmlRenderer -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `HtmlContent` | `string?` | `null` | The HTML to render. Renders nothing when empty or white space. |
| `EnableSyntaxHighlighting` | `bool` | `true` | Whether Prism highlights the code blocks. Prism loads only when the HTML has a code block (a `pre` element whose first child is a `code` element). When `false`, line numbers and copy buttons are not added either. |
| `ShowLineNumbers` | `bool` | `false` | Whether code blocks show line numbers (Prism's line-numbers plugin). Needs `EnableSyntaxHighlighting`. Each number is a button that highlights its line. |
| `EnableCopyButton` | `bool` | `false` | Whether each code block is wrapped with a header that shows its language and a Copy button. Needs `EnableSyntaxHighlighting`. |
| `EnableLineHighlighting` | `bool` | `false` | Whether line highlighting is enabled. The value is written to the page as `data-enable-line-highlighting`, but the bundled line highlighting script does not read it: with `ShowLineNumbers` on, line numbers can be selected whatever this says. |
| `UseAccessibleTheme` | `bool` | `true` | Whether the built-in high-contrast dark theme styles the code. When `false`, Prism's Okaidia theme is loaded instead. |
| `LanguageClasses` | `Dictionary<string, string>?` | `null` | CSS classes for specific languages. Obsolete and ignored: the component never read it. Style code blocks with CSS selectors on the `language-*` classes instead. |
| `SanitizeHtml` | `bool` | `true` | Whether the HTML is sanitized before it is rendered. |
| `HtmlSanitizer` | `Func<string, string>?` | `null` | A sanitizer to use instead of the registered `IHtmlContentSanitizer`. Used only while `SanitizeHtml` is `true`. |
| `PrismBaseUrl` | `string?` | `null` | The base address Prism is loaded from, for hosts that serve it themselves (for example `/lib/prism/1.29.0/`). When empty, the pinned cdnjs release is used and every file is verified with its Subresource Integrity hash. Files from a custom address load without an integrity check. |
| `EnableDemos` | `bool` | `true` | Whether fenced `demo` blocks whose text is a registered demo key (see `AddOsirionContentDemos`) render the live demo instead of the code block. Only blocks at the top level of the content are replaced; a block inside a list or quote stays a code block. |
| `FocusableScrollBlocks` | `bool` | `true` | Whether code blocks and tables take a place in the tab order (`tabindex="0"`), so a keyboard user can scroll them sideways (WCAG 2.1.1). An element that already has a `tabindex` keeps it. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`LanguageClasses` is obsolete: the component never read it. Unmatched attributes and `Class` go on the `<article>`.
`Style` is accepted but the component does not write it.

## Behavior

### Sanitizing

With the default `SanitizeHtml="true"`, the HTML goes through the registered `IHtmlContentSanitizer`, or the
built-in allow-list when none is registered. Script, event handler attributes, `javascript:`, `vbscript:` and
`data:` URLs, forms, iframes, inline styles, `<style>` and SVG are removed; `class`, `id`, `role` and `aria-*` are
kept, so `language-*` classes survive. `AddOsirion` registers the sanitizer; to change the allow-list, call
`AddOsirionHtmlSanitizer` with your changes:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options =>
    options.AllowElement("iframe", "src", "title", "allow", "allowfullscreen"));
```

Sanitizing runs before the copy button wrapper is added, so the button's own handler is not stripped. Set
`SanitizeHtml` to `false` only for markup your application produced itself, never for content, user input or
anything read from a repository.

### Syntax highlighting

Highlighting loads only when the HTML contains a code block. The component then requests Prism 1.29.0 from cdnjs,
plus one language file for each language it finds, and every file carries its Subresource Integrity hash. A language
that is not in that release is skipped. Short names are mapped to Prism's: `cs` to `csharp`, `js` to `javascript`,
`ts`, `py`, `rb`, `yml`, `sh`, `ps1`, and `razor` or `blazor` to `cshtml`; `html` and `xml` use Prism's built-in
`markup`.

To serve Prism yourself, set `PrismBaseUrl`, for example `/lib/prism/1.29.0/`. Keep the release layout
(`prism.min.js`, `components/prism-{language}.min.js`, `themes/prism-okaidia.min.css`, `plugins/line-numbers/...`).
Files from your own address load without an integrity check.

The scripts are rendered as part of the page rather than called through JavaScript interop, so highlighting also
works under static SSR, and it runs again after enhanced navigation. Without JavaScript, code blocks show without
highlighting, as preformatted text.

### Copy button and line numbers

- The Copy button copies the code's text with the Clipboard API, or an older fallback outside secure contexts, and
  briefly reads "Copied!". Its accessible name includes the language, for example "Copy C# code to clipboard".
- With line numbers on, each number becomes a toggle button (Enter or Space with the keyboard). Clicking one
  highlights that line; Ctrl, Cmd or Shift adds lines to the selection, and a polite live region announces the change.
- `EnableLineHighlighting` is written to the page as `data-enable-line-highlighting`, but the bundled script does
  not read it: lines can be highlighted whenever `ShowLineNumbers` is on.

### Scrolling code and tables

The styles let wide code, and tables on small screens, scroll sideways inside the text. With
`FocusableScrollBlocks` on (the default), every `<pre>` and `<table>` without a `tabindex` gets `tabindex="0"`, so a
keyboard user can reach and scroll it. An element with its own `tabindex` keeps it.

### Live demos in content

A fenced `demo` block at the top level of the content, whose text is a key registered with
`AddOsirionContentDemos`, renders the registered component in place of the code block, through
[OsirionContentDemo](/components/content-demo). Set `EnableDemos="false"` to leave such blocks as code blocks. The
marker that holds a demo's place is added after sanitizing, so content cannot forge one.

### Content Security Policy

The accessible theme is an inline `<style>` element and the Copy button uses an inline `onclick` attribute. Under a
policy without `'unsafe-inline'`, set `UseAccessibleTheme="false"` (and provide your theme as a file) and leave
`EnableCopyButton` off. The component also links Prism's scripts and the Okaidia theme from cdnjs unless you set
`PrismBaseUrl`, so the policy must allow that host for `script-src` and `style-src` (the Okaidia theme and the
line numbers style), or you serve Prism yourself.

## Styling

The output is wrapped in `<article class="osirion-html-renderer osirion-content">`, with `osirion-wcag-theme` added
when the accessible theme is in use. The component also links
`_content/Osirion.Blazor.Core/css/osirion-html-renderer.min.css`, which styles headings, lists, tables, links and code
inside the article from the theme tokens (`--osirion-text-primary`, `--osirion-font-family-base`,
`--osirion-font-family-mono` and others). That stylesheet is the only source of the content typography: to change it,
override its rules or set the tokens in your app's stylesheet. The component's scoped styles only set the article's base
font, size and color, because the rendered HTML does not carry their scope attribute.

Two custom properties are yours to set:

- `--osirion-syntax-background-color`: background of the copy button wrapper, its header and the code block inside
  it.
- `--osirion-code-background`: background of inline `<code>`.

The copy button wrapper uses `osirion-code-wrapper`, `osirion-code-header`, `osirion-language-label` and
`osirion-copy-button` (with `copied` after a copy); the button also takes the CSS framework's button class. A
highlighted line has `osirion-line-highlighted`.

## Related

- [Content components](/components/content)
- [OsirionContentDemo](/components/content-demo)
- [Callouts](/components/callout)
- [CMS Web components](/docs/packages/cms-web)
