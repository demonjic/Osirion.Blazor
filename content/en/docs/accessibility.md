---
id: 'guide-accessibility'
order: 10
layout: docs
title: "Accessibility"
permalink: /docs/accessibility
description: "What Osirion.Blazor components do for keyboard and screen reader users, how to check your site, and what stays your job."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: /assets/images/blazor-cover.png
categories:
- Documentation
- Guides
tags:
- accessibility
- keyboard
- screen-reader
- contrast
- guides
published: true
slug: accessibility
lang: en
custom_fields:
  group: guides
seo_properties:
  title: 'Accessibility checks for an Osirion.Blazor site'
  description: 'What the components do for accessibility, which checks the repository runs, and how to test your own site with a keyboard, a screen reader and axe.'
  image: /assets/images/blazor-cover.png
  canonical: 'https://getosirion.com/docs/accessibility'
  robots: index, follow
---

# Accessibility

This guide says what Osirion.Blazor components do for keyboard and screen reader users, shows how to check a site that
uses them, and lists what the library cannot do for you. It is for developers who ship a site built with the
components and want to know where their own work starts.

The library does not claim conformance with WCAG or any other standard. It states what the code does and which checks
the repository runs; whether your pages conform depends on your content and your own styles.

## Before you start

- A running site that uses Osirion.Blazor 4.1.0 or later. [Getting started](/docs/getting-started) builds one.
- A desktop browser. A screen reader is useful for [the screen reader pass](#do-a-keyboard-pass-and-a-screen-reader-pass): NVDA on Windows, VoiceOver on macOS.
- Node.js, only if you want the command line checker in [Run an automated checker](#run-an-automated-checker).

## Know what the components do

This is what the code of the 4.1.0 release does. Each row names the component, so you can read its page.

| Area | What it does | Component |
| --- | --- | --- |
| Skip link | A link to the main content is the first focusable element. It is off screen until it gets focus. `ShowSkipLink` (default `true`), `SkipLinkText` (default "Skip to main content") and `MainContentId` (default `osirion-main-content`) configure it. | [`OsirionPageLayout`](/components/page-layout) |
| Landmarks | The layout always renders `main`, and renders `header` and `footer` when you fill the `Header` and `Footer` slots. The `main` element has `tabindex="-1"` so the skip link moves focus there. `OsirionFooter` inside the layout's footer slot renders a `div`, so the page keeps one footer landmark. | [`OsirionPageLayout`](/components/page-layout), [`OsirionFooter`](/components/footer) |
| Named navigation | `nav` elements carry an `aria-label`: `Menu` ("Navigation menu", set with `AriaLabel`), breadcrumbs ("breadcrumb"), the table of contents ("Table of Contents"), and the document navigation and pagination of `DocumentPage`. The search box is a form with `role="search"`. | [`Menu`](/docs/packages/navigation), [`SearchBox`](/components/search-box) |
| Focus styles | Interactive elements get an outline on keyboard focus (`:focus-visible`), drawn from `--osirion-focus-ring-color`, `--osirion-focus-ring-width` and `--osirion-focus-ring-offset`. | Theming, `Menu`, `ThemeToggle` |
| Reduced motion | Under `prefers-reduced-motion: reduce` the header does not hide on scroll, the carousels and the metric count-up do not animate, and `ThemeToggle` switches themes without its fade. | Core, Theming |
| Labelled controls | Contact form fields have a `label` bound with `for` and an `aria-describedby` that points at the validation message. The theme toggle and the menu toggle have an `aria-label`. The honeypot field of the contact form is `aria-hidden` and has `tabindex="-1"`. | [`OsirionContactForm`](/components/contact-form), [`ThemeToggle`](/components/theme-toggle) |
| Scrollable content | Code blocks and tables in rendered Markdown get `tabindex="0"`, so a keyboard user can scroll them. `FocusableScrollBlocks="false"` turns this off. | [`OsirionHtmlRenderer`](/components/html-renderer) |
| Cookie consent | The banner is a region named by its `Title`. | [`OsirionCookieConsent`](/components/cookie-consent) |

Some motion fixes arrive in the release after 4.1.0: the changelog lists `OsirionPageLoading`, `OsirionBackgroundPattern`
and `OsirionReveal` under Unreleased for their behavior under reduced motion. Read
[the changelog](https://github.com/demonjic/Osirion.Blazor/blob/master/CHANGELOG.md) for the version you run.

## Know what the repository tests

The contrast of the theme colors is held by tests in `tests/Osirion.Blazor.Theming.Tests`:

- `OsirionThemeContrastTests` reads the light and dark theme files and checks every text pair (primary, secondary
  and tertiary text on each background, links, button text, the active navigation item, status messages and form
  inputs) for a contrast ratio of at least 4.5:1, and the focus border for at least 3:1.
- `PaletteStylesheetTests` holds each variant of each [palette](/components/palettes) to the text contrast target of
  that variant, and the code colors to the code background.
- `AdapterContrastTests` holds the active navigation item to 4.5:1 under the Bootstrap, Fluent UI, MudBlazor and
  Radzen adapters, in both themes. This test arrives in the release after 4.1.0, together with the color changes it
  checks.

`tests/Osirion.Blazor.Core.Tests` checks the skip link and the landmarks of `OsirionPageLayout`. The continuous
integration workflow runs `dotnet test` on every pull request and every push to `master`, so these tests run there. The workflows do not run an automated
checker such as axe, so read these results as checks of the library's own markup and colors, not of your pages.

Run the contrast tests yourself from a clone of the repository:

```bash
dotnet test tests/Osirion.Blazor.Theming.Tests -c Release
```

## Do a keyboard pass and a screen reader pass

These two passes find what no stylesheet check can. Run them on your own pages, not on the examples.

1. Reload the page and press `Tab` once. The skip link appears at the top left. Press `Enter`: focus moves to the main
   content, and the next `Tab` goes to the first link or control inside it.
2. Keep pressing `Tab` through the page. Every control you can click must receive focus, in reading order, with a
   visible outline. Nothing may trap focus.
3. Open the menu, the theme toggle and any tabs or accordion with the keyboard alone (`Enter` or `Space`, arrow keys
   for tabs).
4. Turn on a screen reader and open its landmarks or headings list. You should find the header, the navigation, one
   main region and the footer, and a heading outline with one `h1` that does not skip levels.
5. Move to each control and listen to its name. A control announced as "button" or "link" alone has no name.

## Run an automated checker

An automated checker finds a part of the problems: missing names, duplicate ids, the contrast of text it can compute.
Use the axe browser extension, or the command line:

```bash
npx @axe-core/cli http://localhost:5000
```

Replace the address with the one your site listens on. Fix what it reports on your own pages. A clean report does not
mean the page works with a keyboard or a screen reader, so keep the [keyboard and screen reader passes](#do-a-keyboard-pass-and-a-screen-reader-pass) for what a tool
cannot judge.

## Do the part the library cannot do

The library renders what you give it. These stay your job:

- **Alt text.** `OsirionResponsiveImage` takes `Alt`, which defaults to an empty string: the image is then decorative.
  Write a description for every image that carries information.
- **Heading order.** Write one `h1` per page and do not skip levels in your Markdown. `ArticlePage` (with its hero),
  `DocumentPage`, `HomePage` (with its hero), `AboutPage` and `ContactPage` render the page title as the `h1` and leave an
  opening `# Title` out of the body, so start your sections at `##`.
- **Link text.** Write links that make sense out of context. "Read the migration guide" works, "click here" does not.
- **Your own colors.** The tests hold the theme and palette colors only. If you set custom properties or write your
  own CSS, check the contrast of what you changed, including the focus outline. The themes and palettes set the focus
  ring variables on the `html` element, so load your stylesheet after Osirion's and match their selectors:

```css
:root[data-osirion-theme],
:root[data-osirion-palette][data-osirion-theme] {
    --osirion-focus-ring-color: #1d4ed8;
    --osirion-focus-ring-width: 3px;
}
```

- **Language.** Set `lang` on the `html` element in your `App.razor`, and set `SkipLinkText` in the language of the page.
- **Content that moves or flashes.** Avoid it, and give a way to stop it.

The skip link text, the main region id and an image description are set like this:

```razor
<OsirionPageLayout SkipLinkText="Zum Inhalt springen" MainContentId="content">
    <Body>
        <OsirionResponsiveImage Src="/images/team.jpg" Alt="The team at the 2026 meetup" Width="800" Height="600" />
    </Body>
</OsirionPageLayout>
```

## Check that it works

- The skip link appears on the first `Tab` and moves focus into `main`.
- Every interactive element shows a focus outline and has a name that a screen reader announces.
- The axe run reports nothing on your pages, or only items you have reviewed.
- `dotnet test tests/Osirion.Blazor.Theming.Tests -c Release` passes in a clone of the repository.

If the skip link does not appear, your pages may not use `OsirionPageLayout` (which `OsirionSiteLayout` builds on), or
`ShowSkipLink` is `false`. If it appears but does nothing, check that `MainContentId` is unique on the page.

## Next steps

- [`OsirionPageLayout`](/components/page-layout): the skip link, the landmarks and the hide-on-scroll header.
- [Palettes](/components/palettes): the color sets the contrast tests cover, including the `Contrast` palette.
- [`ThemeToggle`](/components/theme-toggle) and [`ThemePicker`](/components/theme-picker): theme controls with names.
- [Security](/docs/security): the other half of running a site in production.
