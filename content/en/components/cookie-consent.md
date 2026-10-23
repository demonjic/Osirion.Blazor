---
id: 'cookie-consent'
order: 2
layout: docs
title: OsirionCookieConsent
permalink: /components/cookie-consent
description: A cookie consent banner that works without script, records the visitor's choice per category in a cookie and can gate analytics trackers.
author: Dejan Demonjić
date: 2025-09-03
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/components-cookie-consent.jpg'
categories:
- Components
- Core Components
- Popups
- Privacy
- GDPR
tags:
- blazor
- cookie-consent
- privacy
- gdpr
- static-ssr
- antiforgery
is_featured: true
published: true
slug: cookie-consent
lang: en
redirect_from:
- /docs/components/core/popups/cookie-consent
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'OsirionCookieConsent: cookie banner for Blazor'
  description: 'A Blazor cookie consent banner that works without script, stores consent per category and can gate analytics trackers.'
  image: 'https://media.getosirion.com/og/components-cookie-consent.jpg'
  canonical: 'https://getosirion.com/components/cookie-consent'
  lang: en
  robots: index, follow
  og_title: 'OsirionCookieConsent - Osirion.Blazor'
  og_description: 'Cookie consent banner without script, with categories and an analytics consent gate.'
  og_image_url: 'https://media.getosirion.com/og/components-cookie-consent.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionCookieConsent - Osirion.Blazor'
  twitter_description: 'Cookie consent banner for Blazor that works without script.'
  twitter_image_url: 'https://media.getosirion.com/og/components-cookie-consent.jpg'
  meta_tags: {}
---

# OsirionCookieConsent

`OsirionCookieConsent` shows a fixed cookie banner until the visitor accepts, declines or saves their own choice. The
buttons post a form to an endpoint in your app, which stores the decision in a cookie and redirects back, so the
banner works under static SSR without script. Analytics trackers can then wait for that consent.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Popups/OsirionCookieConsent.razor).

## Working example

There is no live demo on this page, because the banner posts to an endpoint of the site that serves it and sets a
real cookie there, and a demo would change the consent state of every page. This site runs the component itself: the
banner at the bottom of the window on a first visit is `<OsirionCookieConsent />` with its defaults. Choose Customize
there to see the category panel.

## Setup

Register the services and map the endpoint in `Program.cs`:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionCookieConsent();

var app = builder.Build();

app.UseAntiforgery();
app.MapOsirionCookieConsent(); // POST /api/cookie-consent
```

`AddOsirionCookieConsent` registers `IHttpContextAccessor` and the antiforgery services. The endpoint requires an
antiforgery token, so `app.UseAntiforgery()` must run before the endpoints, as in the Blazor Web App template.

Then place the banner in a statically rendered layout:

```razor
@using Osirion.Blazor.Components

<OsirionCookieConsent PolicyLink="/privacy" />
```

## Usage

A banner with your own text and categories:

```razor
<OsirionCookieConsent Title="Cookies on this site"
                      Message="We use a cookie to remember your choice and, if you agree, privacy-friendly statistics."
                      PolicyLink="/privacy"
                      PolicyLinkText="Privacy policy"
                      ShowCustomizeButton="true"
                      Categories="categories"
                      ConsentExpiryDays="180" />

@code {
    private readonly IReadOnlyList<CookieCategory> categories =
    [
        new CookieCategory { Id = "necessary", Name = "Necessary", Description = "Keeps the site working.", IsRequired = true, IsEnabled = true },
        new CookieCategory { Id = "analytics", Name = "Statistics", Description = "Counts visits so we can improve the docs." }
    ];
}
```

## Parameters

<!-- osirion:parameters OsirionCookieConsent -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Title` | `string` | `"Cookie Consent"` | The banner heading (`h3`), which is also the accessible name of the banner region. |
| `Message` | `string` | `"This site would like to use optional cookies in addition to the ones it needs to work. Nothing optional is set until you choose."` | The banner text, rendered as encoded text. |
| `AcceptButtonText` | `string` | `"Accept All"` | The text of the button that accepts every category. |
| `DeclineButtonText` | `string` | `"Decline"` | The text of the button that declines every optional category. |
| `CustomizeButtonText` | `string` | `"Customize"` | The text of the button that opens the category choices. |
| `SavePreferencesButtonText` | `string` | `"Save Preferences"` | The text of the button that saves the ticked categories. |
| `ShowDeclineButton` | `bool` | `true` | Whether the decline button is shown. |
| `ShowCustomizeButton` | `bool` | `true` | Whether the customize button is shown. |
| `ShowCustomizationPanel` | `bool` | `true` | Whether the category checkboxes are shown once the visitor chose to customize. When it is `false`, the customize button reloads the page without any choices. |
| `CustomizePanelTitle` | `string` | `"Cookie Preferences"` | The heading (`h4`) of the category panel. |
| `PolicyLink` | `string?` | `null` | The address of your privacy or cookie policy, shown as a link after the message. Null or empty, the default, shows no link. |
| `PolicyLinkText` | `string` | `"Learn more"` | The text of the policy link. |
| `Icon` | `RenderFragment?` | `null` | The markup shown beside the text. Null, the default, shows no icon. |
| `Categories` | `IReadOnlyList<CookieCategory>` | - | The categories shown in the panel. Defaults to `necessary` (required), `analytics`, `marketing` and `preferences`. Accept All records only those four IDs, so a custom ID is recorded only when the visitor ticks it in the panel. |
| `ConsentEndpoint` | `string` | `"/api/cookie-consent"` | Where the form posts. Defaults to `/api/cookie-consent`, the pattern `MapOsirionCookieConsent` maps; change it only together with that call. |
| `Position` | `string` | `"bottom"` | The banner position, added as the class `osirion-cookie-consent-{Position}`. The stylesheet defines `bottom` (the default) and `top`. |
| `ConsentExpiryDays` | `int` | `365` | The consent expiry in days. The value is posted with the form and applied to the consent cookie by the consent endpoint, which accepts 1 to 3650 days and uses 365 for anything else. With `ConsentEvaluation.Browser` the consent script applies the same rule. |
| `Evaluation` | `ConsentEvaluation` | `ConsentEvaluation.Server` | Where the visitor's choice is read and recorded. `Server`, the default, shows the banner when the request has no consent cookie and posts the choice to `ConsentEndpoint`. `Browser` renders the banner hidden on every page, without a form, and a script shows it, writes the same cookie and runs the tracker scripts held for the categories the visitor accepted: use it for a site exported to static files. In `OsirionSiteLayout` it follows `Osirion:Analytics:Consent:Evaluation`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

`CookieCategory` has `Id`, `Name`, `Description`, `IsRequired` (always on, checkbox disabled) and `IsEnabled`
(checked by default). `Class` goes on the banner element; unmatched attributes go on the `form`.

## Behavior

- **Flow without script.** Accept and Decline post the form, the endpoint writes the cookie and redirects to the
  same page, and the banner is no longer rendered. Customize redirects to the page with `?customize-cookies=true`,
  where the banner shows the category checkboxes; Save Preferences records the checked ones.
- **Changing the answer.** A link to any page with `?customize-cookies=true`, such as a "Cookie settings" link in the
  footer, reopens the panel after the visitor answered, with their saved choices checked; Save Preferences replaces
  the recorded consent, so withdrawing it takes the same few clicks as giving it. Arrives in the release after 4.1.0.
- **What is recorded.** Accept sets `necessary`, `analytics`, `marketing` and `preferences` to true; Decline sets
  `necessary` to true and the other three to false. Save Preferences records `necessary` plus each category the
  visitor checked; unchecked ones are left out, which counts as no consent. Custom category IDs are therefore
  recorded only through the panel.
- **The cookie.** `osirion_cookie_consent` holds JSON with `Version`, `ConsentDate`, `ConsentType` and
  `Categories`. It is `HttpOnly`, `Secure` and `SameSite=Lax`, so serve the site over HTTPS. Script cannot read it;
  decisions are made on the server.
- **Security.** The form carries an antiforgery token. A post without a valid token gets 400 and records nothing,
  and the endpoint redirects only to local paths.
- **Rendering.** The component reads the cookie from the current request. Without an HTTP request (an interactive
  render with no `HttpContext`), it renders nothing. The banner has no interactive mode.
- **Accessibility.** The banner is a region named by its title, so its text is inside a landmark. The buttons are
  real `button` elements in a form, usable with the keyboard.

## Reading consent on the server

`CookieConsentHandler` in `Osirion.Blazor.Core.Handlers` reads the cookie:

```csharp
using Osirion.Blazor.Core.Handlers;

bool statistics = CookieConsentHandler.IsCategoryConsented(httpContext, "analytics");
CookieConsentData? consent = CookieConsentHandler.GetConsentData(httpContext);
```

`ClearConsent(httpContext)` deletes the cookie, for example on a "change cookie settings" endpoint.
`app.UseOsirionCookieConsent()` adds middleware that puts the parsed data in `HttpContext.Items["CookieConsent"]`.

## Gating analytics

The analytics module can render trackers only after the visitor accepted the `analytics` category. Turn the gate on
in code:

```csharp
using Osirion.Blazor.Analytics.Extensions;
using Osirion.Blazor.Extensions;

builder.Services.AddOsirion(osirion => osirion
    .UseAnalytics(analytics => analytics
        .AddClarity(clarity => clarity.SiteId = "your-clarity-id")
        .RequireConsent()));
```

or in configuration:

```json
{
  "Osirion": {
    "Analytics": {
      "Consent": { "RequireConsent": true }
    }
  }
}
```

Without consent no tracker script is rendered at all; with GA4, Google Consent Mode starts with its defaults set to
denied. `RequireConsent(options => options.Category = "statistics")` uses another category ID. See
[Analytics](/docs/packages/analytics).

## Styling

The form has `osirion-cookie-consent-form`; the banner has `osirion-cookie-consent` and
`osirion-cookie-consent-bottom` or `osirion-cookie-consent-top`, plus `osirion-cookie-consent-customizing` while the
panel is open. The banner is `position: fixed` across the full width at the chosen edge. Inner parts include
`osirion-cookie-consent-title`, `osirion-cookie-consent-description`, `osirion-cookie-consent-link`,
`osirion-cookie-consent-actions`, `osirion-cookie-consent-customize` and `osirion-cookie-consent-category`. Colors
come from the theme tokens (`--osirion-text-primary`, `--osirion-border-color`, `--osirion-action-primary` and
others), and the buttons use the active CSS framework's classes.

## Related

- [OsirionAnnouncementBar](/components/announcement-bar)
- [Forms and feedback](/components/forms-and-feedback)
- [Analytics](/docs/packages/analytics)
