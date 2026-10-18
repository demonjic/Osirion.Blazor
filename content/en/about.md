---
id: 'about'
order: 2
layout: page
title: 'About Osirion.Blazor'
permalink: /about
description: 'Osirion.Blazor is an open-source Blazor library for content sites, made and maintained by Dejan Demonjić. Its principles, licence and how to contribute.'
author: 'Dejan Demonjić'
date: '2026-10-17'
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/about.jpg'
published: true
slug: 'about'
lang: 'en'
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/about.jpg'
  title: 'About Osirion.Blazor and its maintainer'
  description: 'Osirion.Blazor is an open-source (MIT) Blazor library for content sites, maintained by Dejan Demonjić. Principles, licence and how to contribute.'
  canonical: 'https://getosirion.com/about'
  robots: 'index, follow'
---

## The project

Osirion.Blazor is a set of Blazor components for sites built from content: a developer blog, a documentation site or
a product site. It reads Markdown files with front matter from a GitHub repository or a local folder and renders them
as lists, articles and whole pages, with the layout, navigation, theming, SEO metadata and feeds around them. It
targets .NET 9 and .NET 10. This site is built with it, and so are the three sites on the [showcase](/showcase).

If you are new here, [getting started](/docs/getting-started) is the shortest way in, and the
[component gallery](/components) shows what is available.

## Principles

These are the rules the code and the documentation follow. Each links to the page that lets you check it.

- **Static server-side rendering first.** Components render complete HTML on the server and work without an
  interactive render mode. A component that needs the browser loads a small script only on a page that uses it, and
  its documentation page says so. See [what is in each package](/docs/packages).
- **Accessibility.** The library does not claim conformance with a standard; it states what the code does. The repository tests the contrast of every theme color and
  palette, and the [accessibility guide](/docs/accessibility) says what the components do, what is tested and what
  stays your job as the site author.
- **Security.** Rendered Markdown is cleaned with an allow-list, URLs are checked, and trackers wait for consent. The
  [security guide](/docs/security) lists the cookies the library sets, with their flags, and what a Content Security
  Policy has to allow.
- **Honest documentation.** Every component has its own page with a live demo, its parameters and its limits. If a
  page is wrong, that is a bug: [open an issue](https://github.com/demonjic/Osirion.Blazor/issues).

## The maintainer

Osirion.Blazor is made and maintained by Dejan Demonjić, a principal software engineer in Belgrade. He is a co-founder
of [Tridesetri](https://tridesetri.com), a software company, and of [Hexavera](https://hexavera.com), a workforce
intelligence platform. His personal blog is [demonjic.com](https://demonjic.com). All three sites run on
Osirion.Blazor; the [showcase](/showcase) describes each.

## Licence

Osirion.Blazor is released under the [MIT licence](https://github.com/demonjic/Osirion.Blazor/blob/master/LICENSE.txt).
The copyright is held by Dejan Demonjić (2025). You may use, copy, modify and distribute it, including in commercial
work, as long as the licence text stays with the copies.

## How to contribute

Contributions are welcome, from a corrected sentence in the documentation to a new component.

1. Read the [contributing guide](https://github.com/demonjic/Osirion.Blazor/blob/master/CONTRIBUTING.md). It covers
   setting up the repository, the coding guidelines, the component structure, running the tests and the pull request
   process.
2. Look through the [open issues](https://github.com/demonjic/Osirion.Blazor/issues), or open one for a bug, a gap in
   the documentation or a feature you need. The repository has issue forms for each.
3. For a larger change, open an issue first, so the approach can be agreed before you write the code.

The project is hosted at [github.com/demonjic/Osirion.Blazor](https://github.com/demonjic/Osirion.Blazor). Other ways
to reach the maintainer are on the [contact page](/contact).
