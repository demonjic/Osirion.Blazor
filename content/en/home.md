---
id: 'homepage'
order: 1
layout: landing
title: 'Blazor sites from Markdown, rendered on the server'
permalink: /
description: 'Osirion.Blazor is an open-source (MIT) Blazor component library. It turns Markdown from GitHub or a local folder into blogs, docs and product sites, with static SSR.'
author: 'Dejan Demonjić'
date: '2024-12-29'
last_modified: 2026-10-17
featured_image: '/images/osirion-hero.png'
published: true
slug: 'homepage'
lang: 'en'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/home.jpg'
  title: 'Osirion.Blazor: Blazor components for content sites'
  description: 'Open-source Blazor components that render Markdown from GitHub or a local folder as blogs, docs and product sites with static server-side rendering.'
  canonical: 'https://getosirion.com'
  robots: 'index, follow'
---

## The quickest path

Osirion.Blazor runs on .NET 9 and .NET 10. In a Blazor Web App you already have (or a new one from `dotnet new blazor`),
add the package:

```bash
dotnet add package Osirion.Blazor
```

Register it in `Program.cs`:

```csharp
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(builder.Configuration);
```

Point it at a folder of Markdown files in `appsettings.json`:

```json
{
  "Osirion": {
    "Cms": {
      "Web": {
        "FileSystem": {
          "BasePath": "content",
          "IsDefault": true
        }
      }
    }
  }
}
```

Then put `<OsirionStyles />` in your layout and list the posts in `content/blog` on a page:

```razor
@page "/blog"
@using Osirion.Blazor.Cms.Web.Components

<ContentList Directory="blog" ShowPagination />
```

[Getting started](/docs/getting-started) walks through every step, including the article page and a GitHub repository
as the content source. It also describes the `osirion-blog`, `osirion-docs` and `osirion-site` templates, which create
a running site in three commands. The template package, `Osirion.Blazor.Templates`, is not on nuget.org yet, so use the
steps above until it is. The [templates page](/templates) lists what each template creates.

## Themes and frameworks

Set `Osirion:Theming:Framework` to `Bootstrap`, `FluentUI`, `MudBlazor` or `Radzen` and `OsirionStyles` loads an
adapter that maps the framework's colors onto Osirion's, so the components match the rest of the site in light and dark
mode. Your app still loads the framework itself. On top of that, named [palettes](/components/palettes) (Osirion,
Commit, Editor, Studio, Studio Blue and Contrast) change the colors of the whole page, and the choice is kept in a
cookie so the server renders it without a flash. The picker below is the component itself: choose a palette and this
page changes. The [themes page](/themes) shows every palette and the tokens behind them.
