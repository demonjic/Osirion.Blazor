---
id: 'guide-troubleshooting'
order: 13
layout: docs
title: "Troubleshooting"
permalink: /docs/troubleshooting
description: "Common Osirion.Blazor problems as symptom, cause and fix: no content, a 404, missing styles, a theme toggle that does nothing, a contact form that will not submit."
author: Dejan Demonjić
date: 2026-10-17
last_modified: 2026-10-17
featured_image: 'https://media.getosirion.com/og/docs-troubleshooting.jpg'
categories:
- Documentation
- Guides
tags:
- troubleshooting
- errors
- logging
- github
- csp
- guides
published: true
slug: troubleshooting
lang: en
custom_fields:
  group: guides
seo_properties:
  og_image_url: 'https://media.getosirion.com/og/docs-troubleshooting.jpg'
  title: 'Troubleshoot an Osirion.Blazor site'
  description: 'Symptoms, causes and fixes taken from the log messages and exceptions of Osirion.Blazor: empty content, 404 pages, missing styles, CSP and form problems.'
  image: 'https://media.getosirion.com/og/docs-troubleshooting.jpg'
  canonical: 'https://getosirion.com/docs/troubleshooting'
  robots: index, follow
---

# Troubleshooting

This guide lists problems a developer is likely to meet with an Osirion.Blazor site, each as a symptom, a cause and a
fix. The log messages are quoted from the code, so you can search your logs for them. It is for anyone whose site runs
but does not show what they expect.

## Before you start

- Look at the application log first. The CMS components write failures through `ILogger`, so a site without a logging
  provider hides them. The default ASP.NET Core host writes to the console.
- Know your version. Where a problem is specific to one, the item says so. The released version is 4.1.0.

## No content is shown, and the log says "Access token is required"

**Symptom.** Lists are empty or pages answer 404, and the log has the error `Access token is required for GitHub API
requests`.

**Cause.** The GitHub provider needs an access token, even for a public repository. Without one it logs that error and
loads nothing.

**Fix.** Set `ApiToken` on the provider, in a secret and not in `appsettings.json`. `Site` is the name you gave the
provider:

```bash
dotnet user-secrets init
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Site:ApiToken" "<your token>"
```

In production use the environment variable `Osirion__Cms__Web__GitHub__Site__ApiToken` or your secret store. If the log
instead says `Repository owner and name must be set before making requests`, `Owner` or `Repository` is empty in the
provider's section.

## The GitHub content stops updating, and the log mentions a rate limit

**Symptom.** The warning `GitHub rate limit reached (status 403); requests are allowed again at ...` appears (the
status can be 429), and the site keeps showing older content.

**Cause.** GitHub refused the requests. The provider stops the refresh and keeps the content it has.

**Fix.** Wait for the time in the message, and check that the token is set (see
[No content is shown](#no-content-is-shown-and-the-log-says-access-token-is-required)). Reduce how often content is
refreshed: with a webhook, refreshes happen on push, and polling (`EnablePolling`) is off by default.

If the token is set and the limit is still reached, look for the warning `GitHub redirected requests for the
configured repository ...` just before it. The repository in `Owner` and `Repository` was renamed or transferred, and
GitHub answers each request with a redirect to its new location. Releases up to 4.2.0 followed that redirect without
the token, so every request counted against the server's shared limit of 60 an hour. The provider now follows a
redirect with the token while it stays on the API host, never sends the token to another host, and logs the warning
once with the old name and the new location. Set `Owner` and `Repository` to the new name, so the requests stop
taking the detour.

## A page returns 404 on the server but works on your machine

**Symptom.** A page of the local folder provider works in development and answers 404 after a deployment, or a page
you committed does not appear with the GitHub provider.

**Cause.** The file never reached the repository, because a `.gitignore` rule matches a folder in its path. The Visual
Studio `.gitignore` has `**/[Pp]ackages/*` and `[Bb]in/`, so a content folder named `packages` or `bin` is ignored
and its files are never committed. The provider cannot serve what is not in the checkout or on the branch.

**Fix.** Ask git why the file is ignored:

```bash
git check-ignore -v content/docs/packages/core.md
```

If it prints a rule, change the rule or the folder name. Osirion's own repository keeps its `packages` content folder
with an exception under the rule:

```text
**/[Pp]ackages/*
!content/**/[Pp]ackages/*
```

## Components have no styles

**Symptom.** The page renders, but components look unstyled: no spacing, no colors, plain lists.

**Cause.** One of two stylesheets is not referenced. The scoped styles of the components come from your app's
`{App}.styles.css`, and the theme comes from `OsirionStyles`. Since 4.1.0, `OsirionStyles` no longer links the bundle
of scoped styles, because the app stylesheet already imports it.

**Fix.** Link the app stylesheet in `App.razor`, using your project name, and render `OsirionStyles` once in your
layout:

```razor
<head>
    <link rel="stylesheet" href="@Assets["MySite.styles.css"]" />
    <HeadOutlet />
</head>
```

```razor
<OsirionStyles />
```

If the theme is wrong but components have their layout, check `Osirion:Theming:Framework` and that the framework's own
stylesheet is loaded: `OsirionStyles` links the adapter for the framework, not the framework itself.

## The theme toggle does nothing under a Content Security Policy

**Symptom.** With a `Content-Security-Policy` header in place, clicking `ThemeToggle` changes nothing, and the browser
console reports a blocked inline script or event handler.

**Cause.** In 4.1.0, `ThemeToggle` calls its script through an inline `onclick` attribute and writes an inline script
without a nonce. `OsirionStyles` also writes an inline script, which is what applies the theme attributes before the
first paint. A policy without `'unsafe-inline'` in `script-src` blocks them.

**Fix.** Allow inline script from your own origin, as the [Security guide](/docs/security) shows:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.ContentSecurityPolicy =
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
    await next();
});
```

`ThemePicker` needs no inline script, but it chooses a palette, not light or dark, so it does not replace the toggle.

## The contact form never submits and shows no error

**Symptom.** You press Send with every field filled and nothing happens: no success message, no validation message.

**Cause.** `PrivacyPolicyText` is empty. The form hides the privacy checkbox when the text is null or empty, but the
model always requires `AgreeToPrivacyPolicy` to be `true`, and no message is shown for it. The form can never be valid.

**Fix.** Keep the default text or set your own:

```razor
<OsirionContactForm Id="contact"
                    PrivacyPolicyText="I agree to the <a href='/privacy'>privacy policy</a>"
                    UseBuiltInEmailService="true" />
```

`PrivacyPolicyText` is written as raw HTML, so pass only text you wrote.

## The contact form reports an error and the message is lost

**Symptom.** After a valid submission the form shows the error message. The log has one of these:

- `Contact form submitted without OnSubmit or UseBuiltInEmailService; the message was not delivered` (this warning and
  its error message arrive in the release after 4.1.0; 4.1.0 tells the visitor the message was sent and loses it without a
  log line)
- `Email service factory not registered but UseBuiltInEmailService is true`
- `Email sending failed: ...`

**Cause.** In order: nothing is set up to deliver the message (after 4.1.0); `UseBuiltInEmailService` is on but no email service is
registered; or the provider refused the message.

**Fix.** Handle `OnSubmit`, or register the email service and configure a provider. `AddOsirion` registers the email
service when an `Osirion:Email` section exists:

```json
{
  "Osirion": {
    "Email": {
      "Provider": "SendGrid",
      "ToEmail": "hello@example.com",
      "SendGrid": {
        "FromEmail": "no-reply@example.com"
      }
    }
  }
}
```

Set the key as a secret: `dotnet user-secrets set "Osirion:Email:SendGrid:ApiKey" "<your key>"`. For `Email sending
failed`, the provider's own error is logged next to it (`SendGrid API error`, `Infobip API error` or `Failed to send
email via SMTP`).

## The cookie banner answers 400 or the app throws on a post

**Symptom.** Accepting the cookie banner returns status 400 and records no choice, or the request throws an
`InvalidOperationException`.

**Cause.** The consent endpoint requires an antiforgery token. A missing or invalid token gets 400. Without the
antiforgery middleware in the pipeline, ASP.NET Core throws for every post to the endpoint.

**Fix.** Call `UseAntiforgery` after `UseAuthentication` and `UseAuthorization`, before the endpoints, and keep the
data protection keys the same across restarts and instances:

```csharp
app.UseAntiforgery();
app.MapOsirionCookieConsent();
```

If the banner returns on every page, check that the site is served over HTTPS: the consent cookie is always `Secure`.

## A GitHub webhook delivery is rejected

**Symptom.** GitHub shows a failed delivery, and the log has a line that starts with `Rejected GitHub webhook`.

**Cause.** The text after the colon says which check failed:

| Message | Cause |
| --- | --- |
| `no GitHub provider has a WebhookSecret configured` | No provider has `WebhookSecret`. Every delivery is rejected until one does |
| `the X-Hub-Signature-256 header is missing or malformed` | The webhook was not created with a secret, or something removed the header |
| `the signature does not match any configured webhook secret` | The secret on GitHub differs from the configured one |
| `the body exceeds ... bytes` | The delivery is larger than 5 MiB |

**Fix.** Set the same secret in both places, with content type `application/json`, and restart the app, because options
are read at startup:

```bash
dotnet user-secrets set "Osirion:Cms:Web:GitHub:Site:WebhookSecret" "<long random value>"
```

## The app stops at startup with an analytics message

**Symptom.** The app fails to start with an `OptionsValidationException` such as
`Osirion:Analytics:Umami:SiteId must be the website ID, a GUID.`

**Cause.** The trackers validate their settings when the app starts. The message names the key and the rule.

**Fix.** Correct the value in `Osirion:Analytics`. A tracker that renders nothing at all has a different cause: it is
disabled, its provider is not configured, or the consent gate is on (`RequireConsent`) and the visitor has not accepted
the analytics category.

## A tab component throws

**Symptom.** A page fails to render with `OsirionTab must be placed inside OsirionTabs.`

**Cause.** An `OsirionTab` sits outside its parent.

**Fix.** Put every tab in an `OsirionTabs` element.

## A parameter has no effect

**Symptom.** A component renders as if a parameter were not set, for example `<OsirionHtmlRenderer Html="..." />`
shows nothing, and the element carries an `html` attribute.

**Cause.** Components pass any attribute they do not know through to their root element, so a misspelled parameter
compiles and becomes an HTML attribute.

**Fix.** Use the parameter's exact name (`HtmlContent` here). In Development the log names it:
`OsirionHtmlRenderer has no parameter 'Html'; did you mean 'HtmlContent'? The value was passed through as an HTML
attribute.` This warning arrives in the release after 4.2.0.

## Custom JSON-LD or a subscription is ignored

Two warnings tell you that the library did not use what you gave it:

- `Ignoring custom JSON-LD for content ... because it is not a valid JSON object or array; generated structured data is
  used instead`. Fix the `json_ld` in the page's front matter.
- `Subscription submitted without an OnSubscribe handler; the address was not stored`. `OsirionSubscriptionCard` stores
  nothing itself; handle `OnSubscribe`. This warning arrives in the release after 4.1.0; 4.1.0 thanks the visitor without
  storing anything.

## Check that it works

- The log of a fresh start has no `Access token`, `Rejected GitHub webhook` or validation errors.
- A page you expect answers 200, and `git check-ignore -v` prints nothing for its path.
- Components have their styles, and the theme toggle changes the theme under your policy.
- The contact form shows a success message, or an error that matches a log line from this guide.

## Next steps

- [Getting started](/docs/getting-started): the registration and configuration these fixes refer to.
- [Security](/docs/security): the Content Security Policy, the cookies and the secrets.
- [Migration](/docs/migration): problems that start after an upgrade.
- [`OsirionContactForm`](/components/contact-form) and [`OsirionCookieConsent`](/components/cookie-consent).
