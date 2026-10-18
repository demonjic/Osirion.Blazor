---
id: 'osirion-footer'
order: 3
layout: docs
title: OsirionFooter
permalink: /components/footer
description: A site footer with link sections, social links, a copyright line, legal links and free content slots, rendered on the server without script.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-footer.jpg'
categories:
- Components
- Core Components
- Layout
tags:
- blazor
- footer
- layout
- navigation
- static-ssr
- accessibility
is_featured: true
published: true
slug: footer
lang: en
redirect_from:
- /docs/components/core/layout/footer
custom_fields:
  group: layout
seo_properties:
  title: 'OsirionFooter: site footer for Blazor'
  description: 'A Blazor site footer with link sections, social links, a copyright line, legal links and content slots, rendered without script.'
  image: 'https://media.getosirion.com/og/components-footer.jpg'
  canonical: 'https://getosirion.com/components/footer'
  lang: en
  robots: index, follow
  og_title: 'OsirionFooter - Osirion.Blazor'
  og_description: 'Site footer with link sections, social links, copyright and legal links.'
  og_image_url: 'https://media.getosirion.com/og/components-footer.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionFooter - Osirion.Blazor'
  twitter_description: 'Site footer with link sections, social links, copyright and legal links.'
  twitter_image_url: 'https://media.getosirion.com/og/components-footer.jpg'
  meta_tags: {}
---

# OsirionFooter

`OsirionFooter` renders a site footer: a top row with your logo and description, columns of links and social links,
then a bottom row with the copyright line and legal links. Everything is plain markup, so it works the same under
static SSR and interactive rendering.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Layout/OsirionFooter.razor).

```demo
footer
```

The demo uses the `minimal` variant with sample text. On its own the footer renders a `footer` element, so it adds a
second contentinfo landmark to this page; in a real layout, put it in the `Footer` slot of
[OsirionPageLayout](/components/page-layout) instead.

## Usage

With only a company name, the footer shows a copyright line for the current year:

```razor
@using Osirion.Blazor.Components

<OsirionFooter CompanyName="Example Ltd" CompanyUrl="https://example.com" />
```

A fuller footer passes link data from code:

```razor
<OsirionFooter CompanyName="Example Ltd"
               CompanyUrl="/"
               Description="Components for content sites built with Blazor."
               Links="links"
               SocialLinks="socialLinks"
               BottomLinks="legalLinks">
    <Logo>
        <img src="/logo.svg" width="40" height="40" alt="Example Ltd" />
    </Logo>
</OsirionFooter>

@code {
    private readonly IReadOnlyList<FooterSection> links =
    [
        new FooterSection
        {
            Title = "Documentation",
            Links =
            [
                new FooterLink { Text = "Getting started", Href = "/docs" },
                new FooterLink { Text = "Components", Href = "/docs/components" }
            ]
        },
        new FooterSection
        {
            Title = "Project",
            Links =
            [
                new FooterLink { Text = "Source code", Href = "https://github.com/demonjic/Osirion.Blazor", OpenInNewTab = true }
            ]
        }
    ];

    private readonly IReadOnlyList<FooterSocialLink> socialLinks =
    [
        new FooterSocialLink { Text = "NuGet", Href = "https://www.nuget.org/packages/Osirion.Blazor", AriaLabel = "Osirion.Blazor on NuGet" }
    ];

    private readonly IReadOnlyList<FooterLink> legalLinks =
    [
        new FooterLink { Text = "Privacy", Href = "/privacy" },
        new FooterLink { Text = "Terms", Href = "/terms" }
    ];
}
```

`FooterSection`, `FooterLink` and `FooterSocialLink` are in the `Osirion.Blazor.Components` namespace.

## Parameters

<!-- osirion:parameters OsirionFooter -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `CompanyName` | `string` | `"Company"` | The name linked in the default copyright line (`&copy; year name. All rights reserved.`). Ignored when `Copyright` is set or `ShowCopyright` is `false`. |
| `CompanyUrl` | `string` | `"/"` | The address the company name links to in the default copyright line. An address that starts with `http` opens in a new tab with `rel="noopener noreferrer"`; any other opens in the same tab. |
| `Description` | `string?` | `null` | The text under the logo, rendered as raw HTML, so pass only text you control. It is shown only when the top row renders, which needs at least one of `Logo`, `Links`, `SocialLinks` (while `ShowSocialLinks` is on), `LeftContent` or `RightContent`. |
| `Copyright` | `string?` | `null` | Custom copyright text, rendered as raw HTML, so pass only text you control. It replaces the default line built from `CompanyName`, `CompanyUrl` and the current year. |
| `ShowCopyright` | `bool` | `true` | Whether the default copyright line is shown when `Copyright` is empty. |
| `Logo` | `RenderFragment?` | `null` | The logo, shown in the top row above `Description`. |
| `LeftContent` | `RenderFragment?` | `null` | Free content for the left column of the top row, such as contact details. It renders above the logo and description. |
| `RightContent` | `RenderFragment?` | `null` | Free content for the right column of the top row. |
| `Links` | `IReadOnlyList<FooterSection>?` | `null` | The link columns: each `FooterSection` has an optional `Title` (an `h2`) and a list of `FooterLink`. |
| `SocialLinks` | `IReadOnlyList<FooterSocialLink>?` | `null` | The social links. Each shows its `Icon`, or its `Text` when there is no icon, and always opens in a new tab; set `AriaLabel` on icon-only links. |
| `BottomLinks` | `IReadOnlyList<FooterLink>?` | `null` | The links in the bottom row, such as privacy and terms. They sit in a `nav` labelled "Footer legal navigation". |
| `ShowSocialLinks` | `bool` | `true` | Whether `SocialLinks` are shown. |
| `SocialTitle` | `string` | `"Follow Us"` | The `h2` above the social links. An empty value hides it. |
| `ShowDivider` | `bool` | `true` | Whether a line separates the top and bottom rows. It shows only when both rows render. |
| `ShowTopSection` | `bool` | `true` | Whether the top row (logo, description, links, social links, left and right content) renders. |
| `ShowBottomSection` | `bool` | `true` | Whether the bottom row (copyright line and `BottomLinks`) renders. |
| `Variant` | `string` | `"default"` | The variant, added as the class `osirion-footer-{value}`. The stylesheet styles `minimal` and `centered`; `default` and any other value add a class with no rules. |
| `GridLayout` | `string` | `"auto"` | The number of link columns, added as the class `osirion-footer-grid-{value}`. The stylesheet defines `2-column`, `3-column` and `4-column`; `auto` and any other value use the default layout. |
| `ChildContent` | `RenderFragment?` | `null` | Extra content rendered below the bottom row. |
| `Docked` | `bool` | `false` | Whether the footer is docked, adding the class `osirion-footer-docked-{DockingMode}`. |
| `DockingMode` | `string` | `"sticky"` | How a docked footer is placed: `"sticky"` adds `margin-top: auto`, which pushes the footer to the bottom of a flex column; `"fixed"` fixes it to the bottom of the viewport, where it covers the end of the page. Used only when `Docked` is `true`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`Class`, `Style` and unmatched attributes go on the outer element.

`FooterSection` has `Title` (optional) and `Links`. `FooterLink` has `Text`, `Href` (default `"#"`) and
`OpenInNewTab`, which adds `target="_blank"` and `rel="noopener noreferrer"`. `FooterSocialLink` extends `FooterLink`
with `Icon` and `AriaLabel`; social links always open in a new tab.

## Behavior

- **Landmark.** On its own the footer is a `footer` element. Inside the `Footer` slot of
  [OsirionPageLayout](/components/page-layout) it renders a `div` with the same classes, because the
  layout already provides the `footer` landmark. Available from 4.0; in 3.x the footer nested a second `footer`.
- **Headings.** Section titles and the social title are `h2` elements, so the outline does not skip a level after a
  page's `h1`.
- **Legal links** are wrapped in a `nav` labelled "Footer legal navigation".
- **Top row.** It renders when at least one of `Logo`, `Links`, `SocialLinks`, `LeftContent` or `RightContent` is set.
  `SocialLinks` count only while `ShowSocialLinks` is on, so switching them off leaves no empty row. `Description` alone
  does not make it render.
- **Copyright year.** The default line uses the server's current year each time the component renders, so a static
  export shows the year of the export.
- **Small screens.** Below 768 pixels the top row stacks into one column: the links, the left content, then the
  right content. With a `Logo`, the logo and description come first.
- **Social links without text.** When a link has only an icon, set `AriaLabel` so screen reader users hear where it
  goes.
- **Printing.** The footer is hidden when the page is printed.

> [!WARNING]
> `Description` and `Copyright` are rendered as raw HTML. Pass only text you control, never visitor or CMS input.

## Styling

The outer element has `osirion-footer`, `osirion-footer-{Variant}` and `osirion-footer-grid-{GridLayout}`, plus
`osirion-footer-docked-{DockingMode}` when docked. Inner parts include `osirion-footer-main`, `osirion-footer-brand`,
`osirion-footer-links`, `osirion-footer-section-title`, `osirion-footer-link`, `osirion-footer-social-link`,
`osirion-footer-divider`, `osirion-footer-bottom`, `osirion-footer-copyright` and `osirion-footer-bottom-link`.

Colors come from the theme tokens `--osirion-background-primary`, `--osirion-text-primary`,
`--osirion-text-secondary`, `--osirion-border-color` and `--osirion-action-primary`, so the CSS framework adapters
recolor the footer. With `Theme` set to `Light` or `Dark`, the footer overrides those tokens on itself.

A fixed footer (`Docked="true"` with `DockingMode="fixed"`) covers the end of the page. Give your main content a
bottom padding equal to the footer's height in your own stylesheet; the component's scoped stylesheet does not reach
the `body`.

## Related

- [OsirionPageLayout](/components/page-layout)
- [OsirionContactInfoSection](/components/contact-info-section)
- [OsirionSubscriptionCard](/components/subscription-card)
- [Layout overview](/components/layout)
