---
title: "Cookie Consent in Blazor: Banner, Consent Cookie and Analytics Gate"
author: "Dejan Demonjić"
date: "2025-01-23"
last_modified: 2026-10-11
description: "Ask for cookie consent in a Blazor app with OsirionCookieConsent, keep analytics off until visitors agree, and let them change their choice."
tags: [Privacy, Cookie Consent, GDPR, Analytics, Blazor]
categories: [Components, Privacy]
slug: "gdpr-cookie-consent-blazor"
is_featured: true
featured_image: 'https://media.getosirion.com/og/blog-gdpr-cookie-consent-blazor.jpg'
seo_properties:
  title: "Cookie Consent in Blazor with Osirion.Blazor"
  description: "A consent banner that works without JavaScript, a server-side consent cookie, and analytics that waits for the visitor's choice."
  image: 'https://media.getosirion.com/og/blog-gdpr-cookie-consent-blazor.jpg'
  og_image_url: 'https://media.getosirion.com/og/blog-gdpr-cookie-consent-blazor.jpg'
  type: "Article"
---

# Cookie Consent in Blazor: Banner, Consent Cookie and Analytics Gate

If your site has visitors in the EU and uses analytics or marketing cookies, you will probably need to ask before
setting them. Osirion.Blazor gives you three pieces for that: the `OsirionCookieConsent` banner, a consent cookie the
server can read, and a consent gate that keeps analytics trackers off the page until the visitor agrees.

This article describes what those pieces do. It is not legal advice: whether you need consent, for which cookies, and
what your banner has to say depends on the laws that apply to your site and its visitors, so check with whoever advises
you on privacy.

## Set up the banner

Register the services and map the endpoint the banner posts to:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionCookieConsent();

var app = builder.Build();

app.MapOsirionCookieConsent();
app.UseAntiforgery();
```

`AddOsirionCookieConsent` registers `IHttpContextAccessor` and the antiforgery services. `MapOsirionCookieConsent` maps a
POST endpoint at `/api/cookie-consent`; pass another pattern if you need one, and set the banner's `ConsentEndpoint` to
match.

Then render the banner in your layout:

```razor
@using Osirion.Blazor.Components

<OsirionCookieConsent PolicyLink="/privacy"
                      Message="We would like to use analytics cookies to see which pages are read. Nothing is set until you choose." />
```

Write the message yourself, so it names what your site asks for. Releases up to 4.2.0 used a default text that said
continuing to browse means agreeing, which is not consent; the newer default does not, but it cannot know your purposes.

## What happens when the visitor chooses

The banner is a plain HTML form, so it works under static SSR with no JavaScript. It has three buttons: Decline,
Customize and Accept All (`ShowDeclineButton` and `ShowCustomizeButton` hide the first two). Each one posts the
visitor's choice together with an antiforgery token and the current path.

- **Accept All** records every default category as accepted.
- **Decline** records `necessary` as accepted and the other default categories as declined.
- **Customize** sends the visitor back to the same page with `?customize-cookies=true`, where the banner shows one
  checkbox per category and a Save Preferences button. Required categories are checked and disabled.

The endpoint rejects a request without a valid antiforgery token, writes the choice to a cookie named
`osirion_cookie_consent`, and redirects back. It redirects only to local paths, so the form cannot be used to send
visitors to another site. The cookie is `HttpOnly`, `Secure` and `SameSite=Lax`, and expires after
`ConsentExpiryDays` (365 by default). Its value is JSON:

```json
{
  "Version": "1.0",
  "ConsentDate": "2026-10-11T09:30:00Z",
  "ConsentType": "save-preferences",
  "Categories": { "necessary": true, "analytics": true }
}
```

Once the cookie exists, the banner no longer renders.

The default categories are `necessary` (required), `analytics`, `marketing` and `preferences`. You can pass your own
list through the `Categories` parameter as `CookieCategory` objects (`Id`, `Name`, `Description`, `IsRequired`,
`IsEnabled`). Keep in mind that Accept All and Decline always record the four default IDs: a category with another ID is
recorded only when the visitor saves custom preferences.

## Keep analytics off until the visitor agrees

Trackers from `Osirion.Blazor.Analytics` render on every page by default. Turn on the consent gate and they render only
when the consent cookie says the visitor accepted `analytics`:

```csharp
using Osirion.Blazor.Analytics.Extensions;

builder.Services.AddOsirionAnalytics(analytics =>
{
    analytics
        .AddGA4(options => options.MeasurementId = "G-XXXXXXXXXX")
        .RequireConsent();
});
```

The same switch in configuration is `Osirion:Analytics:Consent:RequireConsent`. The gate reads the cookie on the
server, so without consent no tracker script is sent to the browser at all. It fails closed: when there is no
`HttpContext`, for example inside a component that is already interactive, the tracker renders nothing. After the
visitor accepts, the endpoint's redirect is a full page load, and the trackers render on that page.

Two providers have their own consent settings, and the gate works with both:

- **GA4 Consent Mode.** With the gate on, the GA4 script first sets the Consent Mode defaults to denied for
  `ad_storage`, `ad_user_data`, `ad_personalization` and `analytics_storage`, then grants `analytics_storage`. The
  advertising signals stay denied, because the gate only knows about analytics consent.
- **Matomo `RequireConsent`.** Without the gate, setting `RequireConsent` on the Matomo options makes the script push
  `requireConsent`, so Matomo tracks nothing until your own page pushes `setConsentGiven` or `rememberConsentGiven` onto
  `_paq`. With the gate on, the option is not needed and is left out of the script.

Available from 4.0: Plausible and Umami trackers, which set no cookies. Whether you still ask for consent for them is
your decision; the gate covers them like the other trackers.

## Check consent in your own components

Read a category with `CookieConsentHandler`, for example before rendering an embed that sets marketing cookies:

```razor
@using Microsoft.AspNetCore.Http
@using Osirion.Blazor.Core.Handlers

@if (_marketingAllowed)
{
    <MarketingWidget />
}

@code {
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private bool _marketingAllowed;

    protected override void OnInitialized()
    {
        _marketingAllowed = HttpContext is not null
            && CookieConsentHandler.IsCategoryConsented(HttpContext, "marketing");
    }
}
```

`MarketingWidget` stands for your own component. `IsCategoryConsented` returns `false` when the cookie is missing or
cannot be read. `CookieConsentHandler.GetConsentData` returns the whole record if you need the date or the other
categories.

## Let visitors change their mind

The banner appears only while there is no consent cookie, so give visitors a way to remove it, for example on your
privacy page. `CookieConsentHandler.ClearConsent` deletes the cookie; map a small endpoint that calls it:

```csharp
using Microsoft.AspNetCore.Antiforgery;
using Osirion.Blazor.Core.Handlers;

app.MapPost("/privacy/reset-consent", async (HttpContext context, IAntiforgery antiforgery) =>
{
    if (!await antiforgery.IsRequestValidAsync(context))
    {
        return Results.BadRequest();
    }

    CookieConsentHandler.ClearConsent(context);
    return Results.LocalRedirect("/privacy");
});
```

```razor
<form method="post" action="/privacy/reset-consent">
    <AntiforgeryToken />
    <button type="submit">Change your cookie choices</button>
</form>
```

After the redirect, the banner shows again and the gated trackers stop rendering. Clearing the consent cookie does not
remove cookies that a third-party script already set in the browser.

## Styling

The banner uses your CSS framework's button classes through the Osirion theme adapter. `Position` places it at the
`"bottom"` (default) or `"top"` of the page, and `Class` adds your own class to the banner. The main hooks are
`.osirion-cookie-consent`, `.osirion-cookie-consent-form` and `.osirion-cookie-consent-actions`. `Icon` takes a render
fragment shown next to the text.

## Related

- [Cookie consent component reference](/components/cookie-consent)
- [Analytics in Blazor with Osirion.Blazor](/blog/analytics-integration-in-blazor)
- [Analytics components](/docs/packages/analytics)
