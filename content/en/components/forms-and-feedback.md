---
id: 'core-forms-components-overview'
order: 1
layout: docs
title: Forms and feedback
permalink: /components/forms-and-feedback
description: The contact form, the cookie and announcement banners, and the not-found state from Osirion.Blazor.Core. All of them post or render on the server under static SSR.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-forms-and-feedback.jpg'
categories:
- Components
- Core Components
- Forms
tags:
- blazor
- forms
- contact-form
- static-ssr
- validation
is_featured: true
published: true
slug: forms-and-feedback
lang: en
redirect_from:
- /docs/components/core/forms
- /docs/components/core/popups
- /docs/components/core/states
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'Blazor forms and feedback components'
  description: 'The contact form, cookie consent banner, announcement bar and not found state in Osirion.Blazor.Core, and when to use each.'
  image: 'https://media.getosirion.com/og/components-forms-and-feedback.jpg'
  canonical: 'https://getosirion.com/components/forms-and-feedback'
  lang: en
  robots: index, follow
  og_title: 'Forms - Osirion.Blazor'
  og_description: 'A contact form for static SSR with server validation, a honeypot and email delivery.'
  og_image_url: 'https://media.getosirion.com/og/components-forms-and-feedback.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'Forms - Osirion.Blazor'
  twitter_description: 'A contact form for static SSR with server validation and email delivery.'
  twitter_image_url: 'https://media.getosirion.com/og/components-forms-and-feedback.jpg'
  meta_tags: {}
---

# Forms and feedback

These components take input from a visitor or tell them something about the page: the contact form, the cookie and
announcement banners, and the state shown when there is nothing to show. All of them live in the
`Osirion.Blazor.Components` namespace of the Osirion.Blazor.Core package and work under static SSR without script.

## Contact form

`OsirionContactForm` is built on Blazor's `EditForm`, so it posts as a plain HTML form under static SSR, validates
with data annotations on the server, and needs no script.

| Component | What it does |
| --- | --- |
| [OsirionContactForm](/components/contact-form) | Name, email, subject and message fields with a privacy checkbox, a honeypot spam check and delivery through SMTP, SendGrid or Infobip, or through your own `OnSubmit` callback. |

### Quick start

```razor
@using Osirion.Blazor.Components

<OsirionContactForm Id="contact" UseBuiltInEmailService="true" />
```

`UseBuiltInEmailService` needs the email service registered, either from an `Osirion:Email` configuration section
read by `AddOsirion(builder.Configuration)` or in code with `UseEmailServices`. Without it, handle the message
yourself through `OnSubmit`. With neither, the form has nothing to deliver the message with and shows its error
message instead of claiming success. The [contact form page](/components/contact-form) covers both.

The newsletter sign-up form lives with the cards: see
[OsirionSubscriptionCard](/components/subscription-card).

## Banners

Both banners work the same way: their buttons post a small form to an endpoint you map, the endpoint sets a cookie
and redirects back, and the next server render reads that cookie. No script is involved, so they work under static
SSR, and every post is protected by an antiforgery token.

| Component | What it does |
| --- | --- |
| [OsirionCookieConsent](/components/cookie-consent) | Cookie banner with accept, decline and per-category choices; analytics trackers can wait for its consent. |
| [OsirionAnnouncementBar](/components/announcement-bar) | Site-wide announcement that visitors can close; remembered per announcement ID. Available from 4.0. |

### Setup for both

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionCookieConsent();

var app = builder.Build();

app.UseAntiforgery();
app.MapOsirionCookieConsent();  // POST /api/cookie-consent
app.MapOsirionAnnouncements();  // POST /api/announcement/dismiss (4.1)
```

```razor
@using Osirion.Blazor.Components

<OsirionAnnouncementBar Id="release-4-0">
    Osirion.Blazor 4.0 is out. <a href="/blog">Read what's new</a>
</OsirionAnnouncementBar>

@* page content *@

<OsirionCookieConsent PolicyLink="/privacy" />
```

Both components read cookies from the current HTTP request, so put them in a statically rendered layout. In an
interactive render without a request they render nothing.

## Empty and missing content

State components fill a page, or part of one, when there is no content to show: the address points nowhere, a list
is empty, or the content is still on its way.

| Component | What it renders | Script | Available |
| --- | --- | --- | --- |
| [OsirionContentNotFound](/components/content-not-found) | A 404 or empty state with a message, buttons, suggested links and a contact address | None | 4.0 |
| [OsirionPageLoading](/components/page-loading) | A loading indicator; documented with the layout components | See its page | 4.0 |

### Status codes

These components only render markup; they do not set the HTTP status code. A not found page should answer with
404 so search engines do not index it as a real page. The CMS page components (`ArticlePage`, `DocumentPage`,
`HomePage`) do this for you; on your own pages, see
[content not found](/components/content-not-found#status-code).

## Related

- [Layout](/components/layout)
- [Analytics](/docs/packages/analytics)
- [Core components](/docs/packages/core)
