---
id: 'templates'
order: 3
layout: docs
title: Templates
permalink: /templates
description: "Three dotnet new templates start a Markdown blog, a documentation site or a product site on Osirion.Blazor, server-rendered and ready to run."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-18
featured_image: 'https://media.getosirion.com/og/templates.jpg'
categories:
- Documentation
- Getting Started
tags:
- templates
- dotnet-new
- getting-started
published: true
slug: templates
lang: en
custom_fields:
  group: getting-started
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/templates.jpg'
  title: 'Blazor templates for blogs, docs and product sites'
  description: 'dotnet new templates for a Markdown blog, a documentation site and a product site on Osirion.Blazor, with static server-side rendering.'
---

# Templates

The `Osirion.Blazor.Templates` package adds three project templates to `dotnet new`. Each creates a .NET 10 site
with static server-side rendering, content in Markdown files and the menu and footer in `appsettings.json`, and
references the Osirion.Blazor version the package was released with. The package is published with the first release
after 4.1.0.

```bash
dotnet new install Osirion.Blazor.Templates
dotnet new osirion-blog -n MyBlog
cd MyBlog
dotnet run
```

## The templates

| Short name | What you get |
| --- | --- |
| `osirion-blog` | A blog: the newest posts on the home page, a blog list, post pages, an About page, a 404 page, RSS, Atom and a sitemap |
| `osirion-docs` | A documentation site: a home page, pages in `content/docs` with side navigation in front matter `order`, a table of contents, previous and next links, a 404 page and a sitemap; `/docs` leads to the first page |
| `osirion-site` | A product or company site: a home page with a hero, three steps and the latest news, news in `content/news` with RSS, About and Contact pages, and a sitemap |

What each template creates, run straight after `dotnet new`:

![A blog created with dotnet new osirion-blog: the newest posts on the home page](https://media.getosirion.com/shots/templates/osirion-blog-dark.webp){width=1600 height=1000}

![A documentation site created with dotnet new osirion-docs: side navigation, a page and its table of contents](https://media.getosirion.com/shots/templates/osirion-docs-dark.webp){width=1600 height=1000}

![A product site created with dotnet new osirion-site: a hero, three steps and the latest news](https://media.getosirion.com/shots/templates/osirion-site-dark.webp){width=1600 height=1000}

## Options

| Option | Default | Description |
| --- | --- | --- |
| `--SiteName` | `My Blog`, `My Docs` or `My Product` | The name in the menu, the footer, the feeds and the page titles |
| `--SiteUrl` | `https://example.com` | The public address, without a trailing slash, that the feeds, the sitemap and `robots.txt` link to |
| `--skipRestore` | `false` | Skips the restore after the project is created |

```bash
dotnet new osirion-docs -n Handbook --SiteName "Team handbook" --SiteUrl https://handbook.example.com
```

## Next steps

[Getting started](/docs/getting-started) walks through the blog the template creates and how to make it yours, then
adds Osirion.Blazor to an existing Blazor Web App. The [components](/components) are the pages' building blocks.
