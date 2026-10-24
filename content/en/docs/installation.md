---
id: 'installation'
order: 2
layout: docs
title: "Install Osirion.Blazor"
permalink: /docs/installation
description: "Add the Osirion.Blazor packages to a Blazor Web App, register AddOsirion, link the stylesheets and choose a CSS framework."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-installation.jpg'
categories:
- Documentation
- Guides
tags:
- installation
- nuget
- packages
- addosirion
- css-framework
published: true
slug: installation
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-installation.jpg'
  title: 'Install Osirion.Blazor in a Blazor Web App'
  description: 'Add the Osirion.Blazor packages, call AddOsirion, link the stylesheets and pick a CSS framework in an existing Blazor Web App on .NET 9 or 10.'
  image: 'https://media.getosirion.com/og/docs-installation.jpg'
  canonical: 'https://getosirion.com/docs/installation'
  robots: index, follow
---

# Install Osirion.Blazor

This guide adds Osirion.Blazor to a Blazor Web App you already have: the packages, the service registration, the
stylesheet links and the choice of CSS framework. It is for developers who want to control each step. To start a new
site from a template instead, use [Getting started](/docs/getting-started).

## Before you start

- The .NET 9 or .NET 10 SDK. Every library package targets `net9.0` and `net10.0`.
- A Blazor Web App. `dotnet new blazor -n MySite` creates one. Osirion's content and layout components render with
  static server-side rendering, so no interactive render mode is needed.
- This guide is written for Osirion.Blazor 4.1.0.

## Choose the packages

Install `Osirion.Blazor` unless you know you need less. It references the Core, Cms, Analytics, Navigation and Theming
packages and adds the `AddOsirion` registration method and `OsirionSiteLayout`.

| Package | What it holds | Osirion packages it brings in |
| --- | --- | --- |
| `Osirion.Blazor` | `AddOsirion`, `OsirionSiteLayout` | Core, Cms, Analytics, Navigation, Theming |
| `Osirion.Blazor.Core` | Layout, sections, cards, Markdown rendering, the HTML sanitizer, email services | none |
| `Osirion.Blazor.Theming` | `OsirionStyles`, `ThemeToggle`, `ThemePicker` | Core |
| `Osirion.Blazor.Navigation` | `Menu`, `EnhancedNavigation`, `ScrollToTop` | Core |
| `Osirion.Blazor.Analytics` | Tracker components for Clarity, Matomo, GA4, Yandex Metrica, Plausible and Umami | Core |
| `Osirion.Blazor.Cms.Web` | Content components, feeds and the sitemap | Cms.Core, and through it Core and the internal Cms packages |
| `Osirion.Blazor.Cms.Admin` | Admin pages under `/osirion` | Cms.Core |
| `Osirion.Blazor.Cms` | Cms.Web and Cms.Admin together | Cms.Web, Cms.Admin |

`Osirion.Blazor.Cms.Core`, `.Application`, `.Domain` and `.Infrastructure` are internal layers of the content modules.
They are restored for you; you do not reference them. With single modules instead of the meta-package you register each
module with its own method (`AddOsirionTheming`, `AddOsirionContent`, `AddOsirionNavigation`, `AddOsirionAnalytics`),
because `AddOsirion` ships only in `Osirion.Blazor`. `AddOsirionContent` registers the script loader that some
components use, but `AddOsirionTheming` does not, so a site that references `Osirion.Blazor.Theming` alone also calls
`AddJSComponents()`, as the [Theming page](/docs/packages/theming) shows. See the [package pages](/docs/packages) for
each module.

## Add the package

From the project folder:

```bash
dotnet add package Osirion.Blazor
```

To take a single module, name it instead, for example `dotnet add package Osirion.Blazor.Theming`.

## Register the services

In `Program.cs`, add the `using` and one line after `AddRazorComponents()`:

```csharp
using Osirion.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddOsirion(builder.Configuration);
```

`AddOsirion(IConfiguration)` always registers the same defaults: enhanced navigation and scroll to top, theming with no
CSS framework, the HTML sanitizer that cleans rendered Markdown, and the script loader (`AddJSComponents`) that some
components use for their own small scripts. You do not call `AddJSComponents` yourself.

It then reads the `Osirion` section of your configuration and configures a module only when its section exists:

| Section | What it configures |
| --- | --- |
| `Osirion:Cms` | Content providers, from `Osirion:Cms:Web` |
| `Osirion:Cms:Admin` | The admin module |
| `Osirion:Theming` | Theming |
| `Osirion:Navigation` | Navigation |
| `Osirion:Analytics` | Analytics services |
| `Osirion:Email` | Email services |
| `Osirion:Seo` | Head metadata |
| `Osirion:Site` | The site `OsirionSiteLayout` renders |

The [configuration guide](/docs/configuration) covers each key. To register in code instead of configuration, use the
overload that takes a builder:

```csharp
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(osirion => osirion
    .UseTheming(theming => theming.UseFramework(CssFramework.Bootstrap))
    .UseSite(site => site.Name = "My site"));
```

This overload does not read the `Osirion` section; you set each value in the delegate. Use one of the two overloads,
not both.

## Link the stylesheets and scripts

A site references three things. Everything else Osirion needs, it loads itself.

1. The app's scoped style bundle, `{AssemblyName}.styles.css`. It imports the scoped styles of every referenced
   Osirion package. In `Components/App.razor`, with `MySite` as the assembly name:

   ```razor
   <!DOCTYPE html>
   <html lang="en">
   <head>
       <meta charset="utf-8" />
       <meta name="viewport" content="width=device-width, initial-scale=1.0" />
       <base href="/" />
       <link rel="stylesheet" href="@Assets["MySite.styles.css"]" />
       <HeadOutlet />
   </head>
   <body>
       <Routes />
       <script src="@Assets["_framework/blazor.web.js"]"></script>
   </body>
   </html>
   ```

2. `OsirionStyles`, rendered once in your main layout. It links Osirion's stylesheets and, for the configured CSS
   framework, the adapter that maps the framework's colors onto Osirion's. Add it to `Components/Layout/MainLayout.razor`:

   ```razor
   @inherits LayoutComponentBase
   @using Osirion.Blazor.Theming.Components

   <main>
       @Body
   </main>

   <OsirionStyles />
   ```

   If you use `OsirionSiteLayout` as the default layout, as the templates do, it already renders `OsirionStyles`.

3. The Blazor script, `_framework/blazor.web.js`, which the Blazor Web App template already links.

`OsirionStyles` loads its own script (`osirion-styles.js`) through the script loader, so you reference no Osirion
JavaScript by hand. Its parameters are on the [OsirionStyles page](/components/osirion-styles).

Keep `blazor.web.js` on static pages too. The script loader that every Osirion script goes through (BlazorJSComponents)
starts from it, and so do enhanced navigation and enhanced form posts. Without it the pages still render and read, but
the theme toggle, code highlighting, copy buttons and the other scripted features never run.

### Leave the admin styles out of a public site

`Osirion.Blazor` brings in `Osirion.Blazor.Cms.Admin`, so `{AssemblyName}.styles.css` also imports the admin's scoped
styles, about 72 KB (10 KB compressed), as a render-blocking stylesheet on every page. In the release after 4.2.0, a
site that renders no admin page leaves them out with one property in its project file:

```xml
<PropertyGroup>
  <OsirionCmsAdminStyles>false</OsirionCmsAdminStyles>
</PropertyGroup>
```

The admin pages take their styles only from that import, so keep the default on a site that serves them under
`/osirion`. The property comes with the NuGet packages; a project that references the Osirion projects from source
imports `src/Osirion.Blazor.Cms.Admin/buildTransitive/Osirion.Blazor.Cms.Admin.targets` itself.

## Choose a CSS framework

Osirion has its own design system and works without a CSS framework. If your app already uses one, tell Osirion so its
colors and dark mode follow the framework:

| `Framework` value | Use it when |
| --- | --- |
| `None` (the default) | The app has no CSS framework. Osirion's own colors and font apply. |
| `Bootstrap` | The app uses Bootstrap, as the Blazor Web App template does. |
| `FluentUI` | The app uses Fluent UI Blazor. |
| `MudBlazor` | The app uses MudBlazor. |
| `Radzen` | The app uses Radzen Blazor. |

Set it in `appsettings.json`:

```json
{
  "Osirion": {
    "Theming": {
      "Framework": "Bootstrap"
    }
  }
}
```

Osirion does not load the framework itself. Reference Bootstrap, Fluent UI, MudBlazor or Radzen the way that framework
documents. The [Theming page](/docs/packages/theming) lists the adapters and the other theming options.

## Check that it works

Build and run:

```bash
dotnet build
dotnet run
```

Open any page and view its source. You should see a `<link>` to your style bundle, which `@Assets` writes with a
fingerprint, for example `MySite.0uxv3q46bu.styles.css`, and links to the Osirion stylesheets, which come with a
fingerprint the same way under `MapStaticAssets`: `_content/Osirion.Blazor.Core/css/index.min.or0ndbmg4q.css` and
`_content/Osirion.Blazor.Theming/css/index.min.zwhu21npjb.css` (the codes differ per version). In the browser's
network tab each loads with status 200.

If the page has no Osirion styling, check the network tab:

- The style bundle answers 404, or the page has no link to it: the name in `App.razor` does not match your project's
  assembly name.
- No `index.min.css` request: `<OsirionStyles />` is not in a layout that the page uses.
- A framework's colors are missing: the framework itself is not referenced, or `Osirion:Theming:Framework` names a
  different one.

## Next steps

- [Configuration](/docs/configuration): the `Osirion` section and everything under it.
- [Content providers](/docs/providers): read Markdown from a folder or a GitHub repository.
- [Getting started](/docs/getting-started): list and article pages for a blog.
- [OsirionStyles](/components/osirion-styles) and [Theming](/docs/packages/theming): stylesheets, palettes and dark mode.
