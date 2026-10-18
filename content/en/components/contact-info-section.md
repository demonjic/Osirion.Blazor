---
id: 'contact-info-section'
order: 3
layout: docs
title: OsirionContactInfoSection
permalink: /components/contact-info-section
description: OsirionContactInfoSection lists an address, phone numbers, email addresses and websites as links, with labels, icons and validated URLs.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-contact-info-section.jpg'
categories:
- Components
- Core Components
- Sections
tags:
- blazor
- contact
- address
- footer
- static-ssr
is_featured: true
published: true
slug: contact-info-section
lang: en
redirect_from:
- /docs/components/core/sections/contact-info-section
custom_fields:
  group: sections
seo_properties:
  title: 'OsirionContactInfoSection: contact details for Blazor'
  description: 'List an address, phone numbers, email addresses and websites as links in Blazor, with labels, icons, encoding and validated URLs.'
  image: 'https://media.getosirion.com/og/components-contact-info-section.jpg'
  canonical: 'https://getosirion.com/components/contact-info-section'
  lang: en
  robots: index, follow
  og_title: 'OsirionContactInfoSection - Osirion.Blazor'
  og_description: 'Contact details as tel, mailto and website links, with labels and icons.'
  og_image_url: 'https://media.getosirion.com/og/components-contact-info-section.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionContactInfoSection - Osirion.Blazor'
  twitter_description: 'Contact details as tel, mailto and website links, with labels and icons.'
  twitter_image_url: 'https://media.getosirion.com/og/components-contact-info-section.jpg'
  meta_tags: {}
---

# OsirionContactInfoSection

`OsirionContactInfoSection` shows contact details: a title, a short message, an address, and phone numbers, email
addresses and websites as `tel:`, `mailto:` and web links, each with a label and an icon. Use it in a footer, on a
contact page, or next to the contact form, which renders it for its own contact details.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Sections/OsirionContactInfoSection.razor).

```demo
contact-info-section
```

## Usage

Pass a `ContactInformation` from `Osirion.Blazor.Core.Models`. Only the fields you fill are shown:

```razor
@using Osirion.Blazor.Core.Models

<OsirionContactInfoSection ContactInfo="@contact" />

@code {
    private readonly ContactInformation contact = new()
    {
        Address = "Belgrade, Serbia",
        Email = "hello@example.org"
    };
}
```

Phone, email and website take several values separated by commas. Labels can be translated:

```razor
<OsirionContactInfoSection Title="Kontakt"
                           AddressLabel="Adresa:"
                           PhoneLabel="Telefon:"
                           EmailLabel="E-pošta:"
                           WebsiteLabel="Sajt:"
                           ContactInfo="@contact" />

@code {
    private readonly ContactInformation contact = new()
    {
        Phone = "+381 11 123 4567, +381 64 123 4567",
        Email = "sales@example.org, support@example.org",
        Website = "example.org",
        Message = "We answer within one working day."
    };
}
```

## Parameters

<!-- osirion:parameters OsirionContactInfoSection -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `"Contact information"` | The heading, rendered as an `h2`; nothing is rendered when it is empty. Simple markup is allowed and passes through the registered HTML sanitizer. |
| `ContactInfo` | `ContactInformation?` | `null` | The details to show; only the fields that are filled in are rendered. Required. |
| `AddressLabel` | `string` | `"Address:"` | The label of the address. |
| `PhoneLabel` | `string` | `"Phone:"` | The label of the phone numbers. |
| `EmailLabel` | `string` | `"Email:"` | The label of the email addresses. |
| `WebsiteLabel` | `string` | `"Website:"` | The label of the websites. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes are written to the root `<div>`.

`ContactInformation` has five `string` properties, all empty by default:

| Property | Shown as |
| --- | --- |
| `Address` | Text, sanitized HTML allowed |
| `Phone` | One `tel:` link per comma-separated number |
| `Email` | One `mailto:` link per comma-separated address |
| `Website` | One link per comma-separated address, showing its host (and port, if it has one) |
| `Message` | A paragraph under the title, sanitized HTML allowed |

## Behavior

- The component renders plain HTML and needs no script.
- Phone numbers, email addresses and website text are HTML-encoded (since 4.0).
- `Title`, `Message` and `Address` may contain simple inline markup, such as a `<br>` between address lines. They go through the
  registered HTML sanitizer, so script and event handlers are removed.
- Websites are validated: `https://` and `http://` addresses are linked, an address without a scheme such as
  `example.org` is linked as `https://example.org/`, and anything else, such as a `javascript:` URL, is left out.
- A website on the current site's host is linked by its path in the same tab. Other websites open in a new tab with
  `rel="noopener noreferrer"`.
- The icons are inline SVG with `aria-hidden="true"`; the visible labels name each group.

## Styling

The root has `osirion-contact-info-section`. Inside are `osirion-contact-info-title`,
`osirion-contact-info-description`, `osirion-contact-info-list`, and per group `osirion-contact-info-item`,
`osirion-contact-info-icon`, `osirion-contact-info-label` and `osirion-contact-info-value`; the links also have
`osirion-contact-link`.

The section reads five custom properties, which you can set through `Style`:

| Custom property | Used for | Default |
| --- | --- | --- |
| `--osirion-contact-accent` | Icons and link hover color | `--osirion-action-primary` |
| `--osirion-contact-text` | Title and labels | `inherit` |
| `--osirion-contact-text-muted` | Message, values and links | `inherit` |
| `--osirion-contact-bg` | Background | not set |
| `--osirion-contact-spacing` | Padding | `0` |

```razor
<OsirionContactInfoSection ContactInfo="@contact"
                           Style="--osirion-contact-bg: var(--osirion-background-secondary); --osirion-contact-spacing: 1.5rem;" />
```

With `prefers-contrast: high`, the accent color becomes pure blue.

## Related

- [Contact form](/components/contact-form)
- [Footer](/components/footer)
- [Core section components](/components/sections)
