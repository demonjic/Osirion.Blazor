---
id: 'guide-security'
order: 11
layout: docs
title: "Security"
permalink: /docs/security
description: "How Osirion.Blazor sanitizes content and URLs, which components take raw HTML, what a Content Security Policy must allow, the cookies it sets and how to keep secrets out of source."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-security.jpg'
categories:
- Documentation
- Guides
tags:
- security
- sanitizer
- csp
- cookies
- secrets
- antiforgery
- guides
published: true
slug: security
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-security.jpg'
  title: 'Secure an Osirion.Blazor site'
  description: 'Content and URL sanitizing, raw HTML parameters, Content Security Policy, cookie flags, secrets, the contact form honeypot, rate limiting and antiforgery.'
  image: 'https://media.getosirion.com/og/docs-security.jpg'
  canonical: 'https://getosirion.com/docs/security'
  robots: index, follow
---

# Security

This guide walks through what Osirion.Blazor does to keep rendered content safe, what you must do yourself, and how to
configure the pieces a production site needs: a Content Security Policy, secrets, rate limiting and antiforgery. It is
for developers who are about to put a site on the internet.

It states what the code does. It does not promise that a site is secure: your own components, your hosting and your
configuration decide that.

## Before you start

- A running site on Osirion.Blazor 4.1.0 (see [Getting started](/docs/getting-started)).
- Content from a source you do not fully control, such as pull requests to a content repository, is the case this
  guide protects against. Content that only you write is lower risk, but the same steps apply.

## Keep sanitizing on

Markdown is rendered to HTML, and that HTML is sanitized with an allow-list (HtmlSanitizer on the AngleSharp HTML5
parser) in two places: when content is ingested from a provider, and again where it is written into the page as raw
markup (`OsirionHtmlRenderer`, `ContentView`, `LocalizedContentView`, `DocumentPage`). `AddOsirion` registers the
sanitizer with its default allow-list.

By default the sanitizer keeps common content markup, `class`, `id`, `role`, every `aria-*` attribute, the
`text-align` of table cells, and the URL schemes `http`, `https`, `mailto` and `tel` plus relative URLs. It removes
forms and form controls, iframes, `style` elements and inline styles (except `text-align` in table cells), the
`name` attribute, event handler attributes and the `javascript:`, `vbscript:` and `data:` schemes. The elements
`script`, `svg`, `math`, `template`, `object`, `embed`, `applet`, `meta`, `base`, `link`, `frame`, `frameset` and
`noscript` can never be allowed, whatever the options say.

If your content needs more, extend the allow-list instead of turning sanitizing off:

```csharp
using Osirion.Blazor.Core.Extensions;

builder.Services.AddOsirionHtmlSanitizer(options =>
    options.AllowElement("chart", "type", "title", "caption", "colors"));
```

`AllowElement` keeps the attributes you list on that element only, in addition to the attributes every element may carry. An element you allow becomes your
responsibility: if your own script writes one of its attribute values into `innerHTML`, the browser can turn escaped
text back into markup. Read such values with `textContent` or `setAttribute`.

`OsirionHtmlRenderer.SanitizeHtml` defaults to `true`. Set it to `false` only for markup your application built
itself, never for content, user input or anything read from a repository:

```razor
<OsirionHtmlRenderer HtmlContent="@fromYourCode" SanitizeHtml="false" />
```

Front matter `json_ld` (under `seo_properties`) is parsed and written again with `<`, `>` and `&` escaped before it goes into a script element;
invalid JSON-LD is dropped and a warning is logged.

## Know the URL rules

Links and image addresses can come from content or configuration, so components check them before they write an
`href` or `src`:

| Where | Allowed |
| --- | --- |
| Sanitized HTML (rendered Markdown) | Relative URLs, `http`, `https`, `mailto`, `tel` |
| `Menu`, `MenuItem`, `MenuGroup` footer, `MenuPanelLink`, `MenuImageCard`, `MenuCallToAction` and the brand link | Relative URLs, `http`, `https`, `mailto`, `tel`; any other scheme renders no `href` |
| `OsirionAuthorCard` (avatar, profile and link addresses) and `OsirionPricingTable` (button address) | A path on the site, or an absolute `http`, `https` or `mailto` address |

A path on the site starts with one `/` and is not protocol-relative (`//host`). The internal check behind the last row
is called `SafeLink`. `InfiniteLogoCarousel` applies the same check in the release after 4.1.0; in 4.1.0 it writes any
`Url` into `href`, so pass it only addresses you control.

`HeroSection` and `OsirionBaseSection` escape the image address before they write it into a CSS `url()`, so a front
matter `featured_image` cannot add style rules.

## Pass only trusted text to raw HTML parameters

A few parameters are written into the page as raw HTML, without sanitizing, so you can use inline markup in them.
Pass only text you wrote:

| Component | Raw HTML parameters |
| --- | --- |
| [`OsirionFooter`](/components/footer) | `Description`, `Copyright` |
| [`OsirionBaseSection`](/components/base-section) | `Title`, `Description` |
| [`OsirionContactForm`](/components/contact-form) | `PrivacyPolicyText` |
| [`OsirionSubscriptionCard`](/components/subscription-card) | `Title`, `Description`, `PrivacyPolicyText` |
| [`ContentPage`](/components/content-page) | `FooterContent`, `ShareLink.Icon` |
| [`OsirionStyles`](/components/osirion-styles) | `CustomVariables`, written as raw CSS |

Never bind these to visitor input or to text from a content repository. For a title that must contain content you do
not control, use the component's template parameter (`TitleContent` on `OsirionBaseSection`) instead.

## Write a Content Security Policy

`OsirionStyles` takes a `Nonce` parameter and writes it on its inline script and inline `style` elements, so a site that
uses only `OsirionStyles` and `ThemeToggle` or `ThemePicker` can run a policy without `'unsafe-inline'` for them (see
the nonce example below). The other components in the table still write inline script or style and add no nonce, so a
site that uses them still needs `'unsafe-inline'` or must leave them out. The table shows what each component needs:

| Component | Needs |
| --- | --- |
| `OsirionStyles` | An inline script that sets the theme attributes before first paint, and inline `style` elements when `CustomVariables` or generated theme variables are written. All of them carry the `Nonce` parameter as a `nonce` attribute, so `'nonce-...'` allows them. `osirion-styles.js` is a file from your own origin |
| `ThemeToggle` | Nothing inline: a click listener in `theme-toggle.js` does the work, and the page needs `OsirionStyles` as well |
| `ThemePicker` | Nothing inline: no inline script and no event handler attribute |
| `ScrollToTop`, `EnhancedNavigation` | One inline script each |
| `OsirionHtmlRenderer` | An inline `style` element for the accessible code theme (`UseAccessibleTheme`) and an inline `onclick` on the Copy button; Prism from `cdnjs.cloudflare.com`, or from `PrismBaseUrl` when you serve it yourself |
| `OsirionFooter` | An inline `style` element |
| Components with a `Style` parameter | `style-src` that allows inline style attributes |
| Analytics: GA4, Clarity, Matomo, Plausible, Yandex Metrica | An inline script, and the provider's own script host |
| Analytics: Umami | An external script tag only |
| Matomo and Yandex Metrica with `AutoTrackPageViews` | An external script from your own site, `/_content/Osirion.Blazor.Analytics/js/osirion-analytics.js` |

The practical policy for 4.1.0 allows inline script and style from your own origin, and adds the hosts of the services
you use:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
    await next();
});
```

Be clear about what this gives you: with `'unsafe-inline'` in `script-src`, the policy no longer stops an injected inline
script. The sanitizer ([Keep sanitizing on](#keep-sanitizing-on)) is then the defense that matters, so do not weaken it. The script of `OsirionStyles`
contains the current theme, so its text changes between requests and a fixed hash would not cover it; use a nonce.

A strict policy for `OsirionStyles` needs a new random nonce for every response, the same value in the header and in the
`Nonce` parameter. Generate it in middleware and hand it to the layout:

```csharp
app.Use(async (context, next) =>
{
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    context.Items["csp-nonce"] = nonce;
    context.Response.Headers.ContentSecurityPolicy =
        $"default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self' 'nonce-{nonce}'";
    await next();
});
```

```razor
@* App.razor, a static server rendered component *@
<OsirionStyles Nonce="@(HttpContext?.Items["csp-nonce"] as string)" />

@code {
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
}
```

The nonce does not cover inline `style` attributes: a component with a `Style` parameter, or one that writes a `style`
attribute, still needs `style-src-attr 'unsafe-inline'` or `style-src 'unsafe-inline'` without a nonce.

## Know which cookies the library sets

| Cookie | Set by | Flags |
| --- | --- | --- |
| `osirion-preferred-theme` | `ThemeService` on the server; `ThemeToggle` in the browser | Path `/`, one year, `SameSite=Lax`, not `HttpOnly` (the page script reads and replaces it). `Secure` from the server; the toggle's script adds `Secure` on HTTPS pages |
| `osirion-preferred-palette` | `ThemeService` on the server; `ThemePicker` in the browser | Path `/`, one year, `SameSite=Lax`, not `HttpOnly`. `Secure` from the server; the picker's script adds `Secure` on HTTPS pages |
| `osirion_cookie_consent` | The cookie consent endpoint (`MapOsirionCookieConsent`) | Path `/`, `HttpOnly`, `Secure`, `SameSite=Lax`. Lifetime comes from `ConsentExpiryDays` (default 365) |
| `osirion_announcement_<id>` | The announcement endpoint (`MapOsirionAnnouncements`) | Path `/`, `HttpOnly`, `Secure`, `SameSite=Lax`, essential. 30 days unless the form posts 1 to 365 |

The theme and palette cookies hold a short name such as `dark`, no personal data. Every cookie the server sets is
`Secure`, so a browser drops it on plain HTTP other than `localhost`, which browsers treat as secure: serve the site
over HTTPS, or the banners can return on every page.

The consent and announcement endpoints require an antiforgery token, which their forms carry. See
[Protect the contact form](#protect-the-contact-form).

## Keep secrets out of source

Tokens and keys do not belong in `appsettings.json`. These settings are secrets:

| Setting | Key |
| --- | --- |
| GitHub token for a content provider | `Osirion:Cms:Web:GitHub:<Name>:ApiToken` |
| GitHub webhook secret | `Osirion:Cms:Web:GitHub:<Name>:WebhookSecret` |
| SendGrid key | `Osirion:Email:SendGrid:ApiKey` |
| SMTP password | `Osirion:Email:Smtp:Password` |
| Infobip key | `Osirion:Email:Infobip:ApiKey` |

`<Name>` is the name of the provider in your configuration, for example `Site`. In development, store them with user
secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Site:ApiToken" "<your token>"
dotnet user-secrets set "Osirion:Email:SendGrid:ApiKey" "<your key>"
```

In production, use environment variables, where `:` becomes `__` (`Osirion__Cms__Web__GitHub__Site__ApiToken`), or your
secret store. For a site that only reads content, give the GitHub token access to read that repository's contents and
nothing more.

## Protect the contact form

The `OsirionContactForm` has three layers, and only one of them is automatic:

1. **Antiforgery.** The form is an `EditForm`, which adds an antiforgery token for you. The token is checked only when
   your pipeline calls `app.UseAntiforgery()`. The cookie consent and announcement endpoints also need it: without the
   middleware, ASP.NET Core throws for every post to them.
2. **A honeypot.** The form has a hidden `Website` field that people do not see and that is off screen, `aria-hidden` and
   out of the tab order. A submission with that field filled is treated as spam and answered with the spam message.
   This stops simple bots that fill every field. It does not stop a person or a bot that leaves the field empty.
3. **A rate limit, which you add.** The library has none. Without one, anyone can post the form as often as they
   like, and each post sends an email through your provider. Limit posts per address with the ASP.NET Core rate limiter:

```csharp
using System.Threading.RateLimiting;

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        HttpMethods.IsPost(context.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                })
            : RateLimitPartition.GetNoLimiter("not-a-post"));
});

var app = builder.Build();

app.UseRateLimiter();
app.UseAntiforgery();
```

The numbers are an example; choose your own. The limiter counts every post from an address, including the cookie
consent and announcement forms, and it sees the contact form only when the page handles it as a post, which is what
happens without an interactive render mode. Under Interactive Server the submit travels over the circuit, so limit it in
your `OnSubmit` handler instead. Behind a reverse proxy, `RemoteIpAddress` is the proxy's address unless you
configure forwarded headers, and then every visitor shares one limit.

## Check that it works

- A post that contains `<script>alert(1)</script>` in its Markdown shows the text or nothing, and no script runs.
- `curl -s -D - -o /dev/null https://your-site/` shows your `Content-Security-Policy` header. A `HEAD` request (`curl -I`) answers 405 on a page, so read the headers of a `GET`.
- In the browser's cookie view, `osirion_cookie_consent` shows `HttpOnly`, `Secure` and `Lax` after you accept the banner.
- `git grep -n -e ghp_ -e github_pat_` finds no GitHub token in your repository.
- The sixth rapid post of the contact form from one address gets status 429.

## Next steps

- [`OsirionHtmlRenderer`](/components/html-renderer): the sanitizing parameters and the Prism hosts.
- [`OsirionContactForm`](/components/contact-form) and [`OsirionCookieConsent`](/components/cookie-consent).
- [Analytics package](/docs/packages/analytics): the consent gate and what each tracker writes.
- [Troubleshooting](/docs/troubleshooting): symptoms and fixes, including a theme toggle that does nothing.
