---
id: 'osirion-static-contact-form'
order: 1
layout: docs
title: OsirionStaticContactForm
permalink: /components/static-contact-form
description: "A contact form for sites published as static files: it posts JSON to an endpoint you provide, such as a Cloudflare Worker, with a Turnstile check and a honeypot."
author: Dejan Demonjić
date: 2026-10-23
last_modified: 2026-10-23
featured_image: 'https://media.getosirion.com/og/components-static-contact-form.jpg'
categories:
- Components
- Core Components
- Forms
tags:
- blazor
- contact-form
- forms
- static-site
- cloudflare
- turnstile
published: true
slug: static-contact-form
lang: en
custom_fields:
  group: forms-and-feedback
seo_properties:
  title: 'OsirionStaticContactForm: contact form for static Blazor sites'
  description: 'A Blazor contact form for sites published as static files: JSON to your own endpoint, such as a Cloudflare Worker, with Turnstile and a honeypot.'
  image: 'https://media.getosirion.com/og/components-static-contact-form.jpg'
  canonical: 'https://getosirion.com/components/static-contact-form'
  lang: en
  robots: index, follow
  og_title: 'OsirionStaticContactForm - Osirion.Blazor'
  og_description: 'A contact form for static sites that posts JSON to your endpoint, protected by Turnstile and a honeypot.'
  og_image_url: 'https://media.getosirion.com/og/components-static-contact-form.jpg'
  og_type: article
  json_ld: ''
  type: Article
  twitter_card: summary_large_image
  twitter_title: 'OsirionStaticContactForm - Osirion.Blazor'
  twitter_description: 'A contact form for static sites that posts JSON to your endpoint, protected by Turnstile and a honeypot.'
  twitter_image_url: 'https://media.getosirion.com/og/components-static-contact-form.jpg'
  meta_tags: {}
---

# OsirionStaticContactForm

`OsirionStaticContactForm` is a contact form for sites published as static files, for example on Cloudflare Workers
static assets, where no ASP.NET server is there to receive a form post. Its script sends the message as JSON to an
endpoint you provide, such as a Cloudflare Worker, with a Cloudflare Turnstile token and a honeypot field, and shows
the endpoint's answer next to the button. A site that runs on a server should use
[OsirionContactForm](/components/contact-form), which posts and validates without script.

Package `Osirion.Blazor.Core`, namespace `Osirion.Blazor.Components`. [Source on GitHub](https://github.com/demonjic/Osirion.Blazor/blob/master/src/Osirion.Blazor.Core/Components/Forms/OsirionStaticContactForm.razor). Available from the release after 4.2.0; until then it is listed under Unreleased in the [release notes](/changelog).

This page has no live demo: a form here would need an endpoint behind it, and this site has none.

## Usage

The component's script loads through BlazorJSComponents, which `AddOsirion` registers. If you register
Osirion.Blazor.Core without `AddOsirion`, add the loader in `Program.cs`:

```csharp
builder.Services.AddJSComponents();
```

Then point the form at your endpoint and give it your Turnstile site key:

```razor
@using Osirion.Blazor.Components

<OsirionStaticContactForm Endpoint="/api/contact"
                          TurnstileSiteKey="0x4AAAAAAA..."
                          PrivacyNote="Your message is emailed to us and not stored on this site. Cloudflare Turnstile checks the form for spam.">
    <NoScriptContent>
        <p>The form needs JavaScript. Write to <a href="mailto:hello@example.com">hello@example.com</a> instead.</p>
    </NoScriptContent>
</OsirionStaticContactForm>
```

The site key is public: it is written into the page. The secret key belongs to the endpoint only. For local runs,
Cloudflare's test site key `1x00000000000000000000AA` always passes and yields the token `XXXX.DUMMY.TOKEN.XXXX`, which
the test secret key `1x0000000000000000000000000000000AA` accepts.

`ShowCompanyField="true"` adds an optional company field. Every label, button text and message is a parameter, so the
form can speak another language.

## Wire contract

The script sends one request per attempt:

- `POST` to `Endpoint`, with `Content-Type: application/json`.
- The body is `{"name": "...", "email": "...", "message": "...", "website": "", "token": "..."}`, plus `"company"`
  after `"email"` when `ShowCompanyField` is on. `website` is the honeypot: people never see it, so it is empty unless a
  bot filled it. `token` is the Turnstile response, or empty when no site key is set.
- The browser sends it from the page, so it carries the page's `Origin`. A same-origin endpoint, such as `/api/contact`
  on the same site, needs no CORS. An `https` endpoint on another origin must answer the CORS preflight that a JSON
  post triggers and allow your site's origin.

The endpoint answers JSON, and the status next to the button shows:

| Answer | Message |
| --- | --- |
| `{"ok": true}` | `SuccessMessage`, and the fields are cleared |
| `{"ok": false, "error": "fields"}` | `FieldsErrorMessage` |
| `{"ok": false, "error": "check"}` | `CheckErrorMessage` |
| `{"ok": false, "error": "failed"}`, any other answer, a body that is not JSON, or no answer | `ErrorMessage` |

The browser checks the fields before anything is sent: name and email are required, the email must look like one, and
the lengths match the endpoint below: name up to 100 characters, email up to 254, message from 10 to 5,000, company up
to 200. The endpoint must still check everything itself, because anyone can post to it without the page.

## The endpoint

A minimal Cloudflare Worker for the endpoint. It is an outline to adapt, not code the library ships: deliver the message
where you need it (an email binding, a queue, your CRM) and add a rate limit.

```js
// worker.js: answers POST /api/contact; every other address is a file of the static site.
const SITEVERIFY = 'https://challenges.cloudflare.com/turnstile/v0/siteverify';
const EMAIL = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
// Name and email often end up in mail headers, where a line break would start a header of the sender's own.
const CONTROL = /[\u0000-\u001f\u007f]/;

const reply = (status, body) => new Response(JSON.stringify(body), {
  status,
  headers: { 'Content-Type': 'application/json; charset=utf-8', 'Cache-Control': 'no-store' }
});
const text = (value) => (typeof value === 'string' ? value.trim() : '');

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    if (url.pathname !== '/api/contact') return env.ASSETS.fetch(request);
    if (request.method !== 'POST') return reply(405, { ok: false, error: 'failed' });
    // Only your own page posts here.
    if (request.headers.get('Origin') !== url.origin) return reply(403, { ok: false, error: 'failed' });
    if (!(request.headers.get('Content-Type') || '').startsWith('application/json')) {
      return reply(415, { ok: false, error: 'failed' });
    }

    const body = await request.text();
    if (body.length > 16384) return reply(413, { ok: false, error: 'fields' });
    let input;
    try {
      input = JSON.parse(body);
    } catch {
      return reply(400, { ok: false, error: 'fields' });
    }

    const name = text(input?.name);
    const email = text(input?.email);
    const message = text(input?.message);
    const token = text(input?.token);
    const valid = !CONTROL.test(name + email + token)
      && name.length > 0 && name.length <= 100
      && email.length <= 254 && EMAIL.test(email)
      && message.length >= 10 && message.length <= 5000
      && token.length > 0 && token.length <= 2048;
    if (!valid) return reply(400, { ok: false, error: 'fields' });

    // A filled honeypot means a bot: it is told the message went, and nothing is sent.
    if (text(input.website)) return reply(200, { ok: true });

    const verdict = await fetch(SITEVERIFY, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ secret: env.TURNSTILE_SECRET, response: token, remoteip: request.headers.get('CF-Connecting-IP') })
    }).then((response) => response.json()).catch(() => ({}));
    // The widget reports the action "contact"; a token from another site or another form fails here.
    if (verdict.success !== true || verdict.hostname !== url.hostname || verdict.action !== 'contact') {
      return reply(403, { ok: false, error: 'check' });
    }

    // Deliver the message here. Log failures by code only, never the visitor's name, address or text.
    return reply(200, { ok: true });
  }
};
```

With Workers static assets, `run_worker_first` makes the Worker answer `/api/*` while every other address is served as
a file. Keep the secret key out of the repository with `wrangler secret put TURNSTILE_SECRET`.

```json
// wrangler.jsonc
{
  "name": "my-site",
  "main": "worker.js",
  "compatibility_date": "2026-10-16",
  "assets": {
    "directory": "./dist/site",
    "binding": "ASSETS",
    "run_worker_first": ["/api/*"]
  }
}
```

## Content Security Policy

The component writes no inline script and no inline event handler, and nothing it runs needs `'unsafe-eval'`. With a
site key, a strict policy needs Turnstile's host for its script and its frame:

```text
script-src 'self' https://challenges.cloudflare.com;
frame-src https://challenges.cloudflare.com;
connect-src 'self';
```

Add the endpoint's origin to `connect-src` when it is not your own site. BlazorJSComponents, which loads the
component's script, writes `style="display: none; !important"` on its `bl-script` element. Under a `style-src` without
`'unsafe-inline'` the browser blocks that attribute and logs one report per page, with no visible effect; to allow it,
add its hash:

```text
style-src 'self' 'unsafe-hashes' 'sha256-EXrMEQM0mPkgnkB6KboseHKD5Hs/aDUfUef9LAWu4uA=';
```

The [security guide](/docs/security#write-a-content-security-policy) covers the policy for the rest of the library.

## Parameters

<!-- osirion:parameters OsirionStaticContactForm -->

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `Endpoint` | `string` | `""` | The address the message is posted to as JSON: a path on the site, such as `/api/contact`, or an `https` URL. Any other value (empty, `http:`, `javascript:`, `data:`, `mailto:` or a protocol-relative `//host`) renders nothing and, in Development, logs a warning. Required. |
| `TurnstileSiteKey` | `string?` | `null` | The site key of the Cloudflare Turnstile widget. When it is set, the script loads Turnstile from `challenges.cloudflare.com`, renders the widget in the form with the action `contact` and sends its token; without it the token is sent empty and nothing is loaded from Cloudflare. |
| `ShowCompanyField` | `bool` | `false` | Whether an optional company field is shown. Its value is sent as `company`. |
| `NameLabel` | `string` | `"Name"` | The label of the name field. |
| `EmailLabel` | `string` | `"Email"` | The label of the email field. |
| `CompanyLabel` | `string` | `"Company"` | The label of the company field, shown with `ShowCompanyField`. |
| `MessageLabel` | `string` | `"Message"` | The label of the message field. |
| `SubmitButtonText` | `string` | `"Send message"` | The text of the submit button. |
| `SubmittingText` | `string` | `"Sending..."` | The text of the submit button while the message is sent; the button is disabled meanwhile. |
| `SuccessMessage` | `string` | `"Thank you. Your message was sent."` | The message shown when the endpoint answers `{"ok": true}`. The fields are cleared. |
| `FieldsErrorMessage` | `string` | `"Check the fields: your name, a valid email address and a message of at least 10 characters."` | The message shown when the endpoint answers `{"ok": false, "error": "fields"}`: a field is missing, too long or malformed. |
| `CheckErrorMessage` | `string` | `"The spam check did not pass. Please try again."` | The message shown when the endpoint answers `{"ok": false, "error": "check"}`, or when the Turnstile widget fails to load or reports an error. |
| `CheckPendingMessage` | `string` | `"Wait for the spam check to finish, then send again."` | The message shown when the visitor sends before the Turnstile check has finished. Nothing is sent. |
| `ErrorMessage` | `string` | `"The message could not be sent. Please try again later."` | The message shown for any other answer: `{"ok": false, "error": "failed"}`, an unknown error, a response that is not JSON, or no response at all. |
| `PrivacyNote` | `string?` | `null` | A short note under the form, such as where the message goes and who checks it for spam. Plain text; it is encoded. Nothing is rendered when it is empty. |
| `NoScriptContent` | `RenderFragment?` | `null` | What visitors without script see instead of the form, rendered inside `noscript`, for example an email link. Nothing is rendered when it is not set. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes, `Class`, `Style` and the `Theme` attribute go on the outer `<div>`.

## Behavior

- **Hidden until its script runs.** The endpoint takes JSON, so without script the form could send nothing. It
  renders with the `hidden` attribute and its script reveals it; visitors without script see `NoScriptContent`, so give
  them another way to reach you, such as an email link.
- **Static SSR and static files.** The script is a BlazorJSComponents module and needs no Blazor circuit. It works on a
  page served by ASP.NET Core with static SSR, and on a copy of that page served as a file, as long as the page loads
  `blazor.web.js` and keeps its `<!--Blazor-Web-Initializers:...-->` comment. A static export must include the
  files that comment names and the component's script under `_content/Osirion.Blazor.Core/Components/Forms/`; a
  crawler that follows only attributes misses them.
- **Turnstile.** With `TurnstileSiteKey`, the script loads
  `https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit` once per page, however many forms the page
  has, and renders the widget into the form with the action `contact`. The widget follows the color mode the
  component's `Theme` or the page sets (`data-theme`, `data-bs-theme` or `data-osirion-theme`), otherwise the visitor's
  system setting. Without a site key nothing is loaded from Cloudflare and the token is sent empty.
- **Sending.** On submit the browser checks the fields first. A send before the Turnstile check has finished shows
  `CheckPendingMessage` and sends nothing. While a message is on its way the button is disabled and shows
  `SubmittingText`. After every attempt the widget is reset, because a token passes one check only, and the button is
  enabled again. A success clears the fields; a failure keeps them, so the visitor can try again.
- **Errors from Turnstile.** When the widget reports an error, or its script cannot load, the status shows
  `CheckErrorMessage`.
- **Messages are text.** The status is written with `textContent` and every label, message and `PrivacyNote` is
  encoded, so none of them can carry markup.
- **Endpoint check.** `Endpoint` must be a path on the site, such as `/api/contact`, or an `https` URL. An empty value,
  `http:`, `javascript:`, `data:`, `mailto:`, a protocol-relative `//host` or a relative path without the leading `/`
  renders nothing, and in Development the component logs a warning that names the value.

## Accessibility

- Every field has a `label` bound with `for`, and name, email and company carry `autocomplete` (`name`, `email`,
  `organization`), so browsers and password managers can fill them.
- The honeypot sits in an `aria-hidden` wrapper, out of the tab order (`tabindex="-1"`) and with `autocomplete="off"`,
  so neither people nor assistive technology reach it.
- The status is a `role="status"` live region. It is emptied before each attempt, so the same answer twice in a row is
  announced again.
- The browser's own validation messages explain a missing or malformed field before anything is sent.

## Styling

| Class | Element |
| --- | --- |
| `osirion-static-contact-form` | The outer `<div>`, which takes `Class` and `Style` |
| `osirion-static-contact-form-form` | The `<form>`, a single column grid |
| `osirion-static-contact-form-field` | A label and its input |
| `osirion-static-contact-form-trap` | The honeypot's wrapper, clipped out of sight |
| `osirion-static-contact-form-turnstile` | The widget container, 65px high before the widget loads so the button does not move |
| `osirion-static-contact-form-actions` | The button and the status, side by side |
| `osirion-static-contact-form-status` | The status, with `data-state` set to `success`, `fields`, `check`, `pending` or `failed` |
| `osirion-static-contact-form-privacy` | The privacy note |

Labels, inputs and the button use the active CSS framework's classes (`form-label`, `form-control` and `btn` under
Bootstrap). Without a framework, the inputs use the theme tokens `--osirion-input-background`,
`--osirion-input-border`, `--osirion-input-text`, `--osirion-input-radius` and `--osirion-input-focus-border`, and the
status uses `--osirion-status-success-text` and `--osirion-status-danger-text`, each with a fallback value.

## Related

- [OsirionContactForm](/components/contact-form)
- [Forms and feedback](/components/forms-and-feedback)
- [Security guide](/docs/security)
