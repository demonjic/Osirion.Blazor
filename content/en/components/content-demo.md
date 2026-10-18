---
id: 'osirion-content-demo'
order: 10
layout: docs
title: OsirionContentDemo
permalink: /components/content-demo
description: OsirionContentDemo renders a registered Razor component as a live example, with Preview and Code tabs, in Markdown content or on a page.
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-content-demo.jpg'
categories:
- Components
- Core Components
- Content
tags:
- demo
- live-example
- markdown
- osirion-blazor-core
- static-ssr
is_featured: false
published: true
slug: content-demo
lang: en
custom_fields:
  group: content
seo_properties:
  title: 'OsirionContentDemo: live demos in Markdown for Blazor'
  description: 'OsirionContentDemo renders a registered Razor component as a live example with Preview and Code tabs, from a fenced demo block in Markdown or from a page.'
  image: 'https://media.getosirion.com/og/components-content-demo.jpg'
  canonical: 'https://getosirion.com/components/content-demo'
  lang: en
  robots: index, follow
  og_title: 'OsirionContentDemo - Osirion.Blazor'
  og_description: 'Live examples in Markdown content, with the source one tab away.'
  og_image_url: 'https://media.getosirion.com/og/components-content-demo.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContentDemo - Osirion.Blazor'
  twitter_description: 'Live examples in Markdown content, with the source one tab away.'
  twitter_image_url: 'https://media.getosirion.com/og/components-content-demo.jpg'
  meta_tags: {}
---

# OsirionContentDemo

`OsirionContentDemo` renders a live example: a Razor component you registered, and, when the demo has source code, a
Code tab next to the preview. [OsirionHtmlRenderer](/components/html-renderer) uses it for every fenced `demo` block in
Markdown content whose text is a registered key, which is how the examples on this site are embedded. A page can also
use it directly.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Rendering/OsirionContentDemo.razor).

```demo
content-demo
```

The example above is itself a demo, so it shows the Preview and Code tabs of the demo registered as `install-tabs`.

## Usage

Register the demos once. The Code tab shows the source you pass, usually the demo's own `.razor` file embedded in the
assembly:

```csharp
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Services;

builder.Services.AddOsirionContentDemos(demos => demos
    .Add<InstallTabsDemo>("install-tabs",
        title: "Install command tabs",
        source: ContentDemoRegistry.ReadEmbeddedSource(
            typeof(InstallTabsDemo).Assembly,
            "MySite.Components.Demos.InstallTabsDemo.razor")));
```

```xml
<ItemGroup>
  <EmbeddedResource Include="Components\Demos\*.razor" />
</ItemGroup>
```

In Markdown, write a fenced block with the language `demo` and the key as its text:

````markdown
```demo
install-tabs
```
````

On a Razor page, name the key:

```razor
@using Osirion.Blazor.Components

<OsirionContentDemo Key="install-tabs" />
```

## Parameters

<!-- osirion:parameters OsirionContentDemo -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Demo` | `ContentDemo?` | `null` | The demo to render, with its component, parameters, title and source. Takes precedence over `Key`. |
| `Key` | `string?` | `null` | The key of a demo registered with `AddOsirionContentDemos`, used when `Demo` is not set. The component renders nothing when neither resolves to a demo. |
| `EnableCopyButton` | `bool` | `true` | Whether the source code on the Code tab has a language header and a Copy button. The component loads the copy script itself, once per page, so the button works with or without `OsirionHtmlRenderer`. |
| `PreviewText` | `string` | `"Preview"` | The label of the preview tab. Shown only when the demo has source code. |
| `CodeText` | `string` | `"Code"` | The label of the Code tab. Shown only when the demo has source code. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`ContentDemo` has `Key`, `ComponentType`, and the optional `Parameters` (passed to the component; entries whose value
is `null` are left out), `Title` and `Source`. Unmatched attributes and `Class` go on the root `<div>`; `Style` is
accepted but the component does not write it.

## Behavior

- Keys are 1 to 60 lowercase letters, digits or hyphens, and registering a key twice throws `ArgumentException`.
- Only blocks at the top level of the content become demos. A `demo` block inside a list or quote, an unknown key, or
  `OsirionHtmlRenderer` with `EnableDemos="false"` leaves it a code block, so a typo is visible instead of silently
  empty. On GitHub, the same Markdown shows the block as a code block with the key, which keeps files readable there.
- A demo with no `Source` renders only its preview, without tabs. The component renders nothing when neither `Demo`
  nor a registered `Key` resolves.
- The source is HTML-encoded before it is shown; it is never run. Demos are components you register, so content can
  only choose among them. The marker that holds a demo's place in content is added after sanitizing and carries a
  random value per render, so content cannot forge one.
- The tabs are an [OsirionTabs](/components/tabs) with a group name of their own for each instance, so several demos
  on a page do not share radio buttons. The tab group is named by the demo's `Title` followed by "example".
- Demos render under static SSR like any component. A demo that needs interactivity sets its own render mode.
- The Copy button on the source has the same markup as the HTML renderer's code blocks. `OsirionContentDemo` loads
  the copy script (`_content/Osirion.Blazor.Core/js/osirion-copy-button.es5.min.js`) itself when `EnableCopyButton` is
  on and the demo has source, and it runs once per page load, so the button works used directly on a page and inside
  `OsirionHtmlRenderer`. The script is loaded through BlazorJSComponents, which `AddOsirion` registers; without
  `AddOsirion`, call `builder.Services.AddJSComponents()` yourself.

## Styling

| Class | Element |
| --- | --- |
| `osirion-content-demo` | The root element |
| `osirion-content-demo-preview` | The box around the live component |

The preview box uses `--osirion-spacing-6`, `--osirion-border-color`, `--osirion-radius-md` and
`--osirion-background-primary`, each with a fallback value. The tabs and the source use the styles of
[OsirionTabs](/components/tabs) and [OsirionHtmlRenderer](/components/html-renderer).

## Related

- [OsirionHtmlRenderer](/components/html-renderer)
- [Tabs](/components/tabs)
- [Content components](/components/content)
