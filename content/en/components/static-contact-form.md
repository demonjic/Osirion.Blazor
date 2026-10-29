---
id: 'osirion-static-contact-form'
order: 1
layout: docs
title: OsirionStaticContactForm
permalink: /components/static-contact-form
description: "A contact form for sites published as static files: it posts JSON to an endpoint you provide, such as a Cloudflare Worker, with a Turnstile check and a honeypot."
author: Dejan Demonjić
date: 2026-10-23
last_modified: 2026-10-29
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

The site key is public: it is written into the page. The secret key belongs to the endpoint only. Leave
`TurnstileSiteKey` out only when your endpoint does not check Turnstile: one that does, like the outline below, answers
`fields` to the empty token, and the visitor would be told to fix fields that are fine.

For local runs, Cloudflare's test site key `1x00000000000000000000AA` always passes and yields the token
`XXXX.DUMMY.TOKEN.XXXX`, which the test secret key `1x0000000000000000000000000000000AA` accepts. Siteverify then
answers with the hostname `example.com` and no action, so an endpoint that checks both answers `check` on every send.
The outline below has a switch for local runs only; see [Local runs with the test keys](#local-runs-with-the-test-keys).

`ShowCompanyField="true"` adds an optional company field, and `ShowSubjectField="true"` an optional subject field of up
to 200 characters. Every label, button text and message is a parameter, so the form can speak another language.
`NamePlaceholder`, `EmailPlaceholder`, `SubjectPlaceholder` and `MessagePlaceholder` write placeholders; without them
the fields have none.

## Contact information beside the form

`FormTitle` and `FormDescription` put a heading and a sentence above the form. `ContactInfo` adds an
[OsirionContactInfoSection](/components/contact-info-section) beside the form, in a shaded panel: two columns on a wide
screen, and the panel below the form under 768px. `SideBySideLayout="false"` keeps it below the form at every width, and
`ShowContactInformation="false"` hides it. Without `ContactInfo` the component renders as it always did.

```razor
<OsirionStaticContactForm Endpoint="/api/contact"
                          TurnstileSiteKey="0x4AAAAAAA..."
                          FormTitle="Get in touch"
                          FormDescription="Tell me what you need and I will reply within one business day."
                          ShowSubjectField="true"
                          ContactInfo="@contactInfo" />

@code {
    private readonly ContactInformation contactInfo = new()
    {
        Address = "Belgrade, Serbia",
        AdditionalItems =
        [
            new ContactInfoItem("Book a call:", "15 minutes", "https://example.com/book", ContactInfoIcon.Calendar),
            new ContactInfoItem("Working hours:", "9 a.m. to noon, Eastern overlap", Icon: ContactInfoIcon.Clock)
        ]
    };
}
```

## Wire contract

The script sends one request per attempt:

- `POST` to `Endpoint`, with `Content-Type: application/json`.
- The body is `{"name": "...", "email": "...", "message": "...", "website": "", "token": "..."}`, plus `"company"`
  after `"email"` when `ShowCompanyField` is on, `"subject"` after that when `ShowSubjectField` is on, and `"fields"`
  after `"message"` when the form has [additional fields](#additional-fields). `website` is the honeypot: people never see it, so it is empty unless a
  bot filled it. `token` is the Turnstile response, or empty when no site key is set, which an endpoint that checks
  Turnstile refuses as `fields`.
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
to 200, subject up to 200. The endpoint must still check everything itself, because anyone can post to it without the page.

## Additional fields

`AdditionalFields` adds questions of your own after the message, for a qualification or quote form: up to 20
`ContactFormField` definitions, rendered in list order. Each has a `Name` (a letter, then up to 49 letters, digits,
underscores or hyphens, unique in the form), a `Label`, a `Type` (`Text`, `MultilineText`, `Number`, `Email`, `Phone`,
`Select`, `Radio` or `Checkboxes`), `Required`, `MaxLength`, `Min` and `Max` for numbers, `Options` for the three choice
types (a `ContactFormFieldOption` with a `Value` and an optional `Label`), and an optional `Placeholder` and `HelpText`.
The form throws an `ArgumentException` naming the field for a definition it cannot render safely, such as a name with a
space, a name used twice or a choice field without options.

```razor
<OsirionStaticContactForm Endpoint="/api/contact"
                          TurnstileSiteKey="0x4AAAAAAA..."
                          ShowCompanyField="true"
                          AdditionalFields="@questions" />

@code {
    private static readonly ContactFormField[] questions =
    [
        new() { Name = "team_size", Label = "Team size", Type = ContactFormFieldType.Select, Required = true,
                Placeholder = "Choose one", Options = [new("1-10", "1 to 10"), new("11-50", "11 to 50"), new("51+")] },
        new() { Name = "services", Label = "What do you need?", Type = ContactFormFieldType.Checkboxes,
                Options = [new("design", "Design"), new("build", "Build"), new("support", "Support")] },
        new() { Name = "budget", Label = "Budget in euros", Type = ContactFormFieldType.Number, Min = 1000,
                HelpText = "A rough figure is enough." },
        new() { Name = "start", Label = "When should it start?", MaxLength = 100 }
    ];
}
```

The script adds the answers to the JSON it posts, under `fields`, keyed by name, after `message`:

```json
{
  "name": "Ann", "email": "ann@example.com", "company": "Example Ltd", "message": "...",
  "fields": { "team_size": "11-50", "services": ["design", "support"], "budget": "25000", "start": "" },
  "website": "", "token": "..."
}
```

Every defined field is present: a string for each type, empty when unanswered, and for `Checkboxes` an array of the
checked option values, empty when none is checked. Numbers are sent as typed. Without `AdditionalFields` the body has
no `fields` key and is exactly what it was before.

The browser checks the answers with native constraint validation only: `required` (on inputs, selects and radio
buttons), `maxlength` (200 for text, 2,000 for multi-line text, 254 for email and 40 for phone unless `MaxLength` says
otherwise), `min` and `max`, and the email type. A required group of checkboxes is not checked in the browser, because
HTML has no constraint for "at least one of these". So the endpoint must check every value again against the same
definitions: a required field is answered, a value is within its length and on one line (except `MultilineText`), a
number parses and lies in range, an email address looks like one, a phone number holds only digits, spaces and
`+ ( ) - . /`, a choice is one of the option values, and all answers together hold at most 10,000 characters. Ignore
names you did not define, and answer `fields` to anything else. `FieldsErrorMessage` names only the standard fields by
default, so reword it when the additional fields can fail too.

A check for the definitions above, to add to the endpoint below:

```js
const QUESTIONS = [
  { name: 'team_size', required: true, options: ['1-10', '11-50', '51+'] },
  { name: 'services', multiple: true, options: ['design', 'build', 'support'] },
  { name: 'budget', number: true, min: 1000, maxLength: 40 },
  { name: 'start', maxLength: 100 }
];

// The answers keyed by name, or null when one is invalid. Names that are not defined are ignored.
function answersOf(fields) {
  const answers = {};
  let total = 0;
  for (const question of QUESTIONS) {
    const raw = fields?.[question.name];
    if (question.multiple) {
      const chosen = Array.isArray(raw) ? raw.map(text) : [];
      if (chosen.some((value) => !question.options.includes(value))) return null;
      if (question.required && chosen.length === 0) return null;
      answers[question.name] = chosen;
      total += chosen.join('').length;
      continue;
    }

    const value = text(raw);
    if (!value) {
      if (question.required) return null;
      answers[question.name] = '';
      continue;
    }

    if (CONTROL.test(value) || value.length > (question.maxLength ?? 200)) return null;
    if (question.options && !question.options.includes(value)) return null;
    if (question.number) {
      const number = Number(value);
      if (!/^-?\d+(\.\d+)?$/.test(value)
        || number < (question.min ?? -Infinity) || number > (question.max ?? Infinity)) return null;
    }

    answers[question.name] = value;
    total += value.length;
  }

  return total <= 10000 ? answers : null;
}
```

In the handler, after the standard fields are checked:

```js
    const answers = answersOf(input.fields);
    if (!answers) return reply(400, { ok: false, error: 'fields' });
```

`CONTROL` refuses line breaks, which suits single-line answers; check a `MultilineText` answer with a pattern that
allows `\n`. Raise the outline's 16,384-byte body limit when your questions allow long answers.

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

    // A declared size over the limit is refused before the body is read into memory.
    if (Number(request.headers.get('Content-Length')) > 16384) return reply(413, { ok: false, error: 'fields' });
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
    const subject = text(input?.subject); // Optional: only sent with ShowSubjectField.
    const message = text(input?.message);
    const token = text(input?.token);
    const valid = !CONTROL.test(name + email + subject + token)
      && subject.length <= 200
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
    // The widget reports the action "contact"; a token from another site or another form fails here. The test secret
    // answers another hostname and no action, so a local run skips those two checks for a test answer only.
    const localTest = env.TURNSTILE_TEST === '1' && verdict.metadata?.result_with_testing_key === true;
    if (verdict.success !== true
      || (!localTest && (verdict.hostname !== url.hostname || verdict.action !== 'contact'))) {
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

### Local runs with the test keys

Siteverify answers the test secret with the hostname `example.com`, no action and
`"metadata": {"result_with_testing_key": true}`, so the outline's hostname and action checks fail for it. Those two
checks are what tie a token to your site and to this form, so do not remove them. For a local run, set the test secret
and the switch in `.dev.vars`, the file Wrangler reads secrets from in local development, and keep that file out of
git:

```text
TURNSTILE_SECRET="1x0000000000000000000000000000000AA"
TURNSTILE_TEST="1"
```

Never set `TURNSTILE_TEST` on the deployed Worker. Even if it were set there, a real secret key never yields a test
answer, so the two checks would still apply.

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
| `TurnstileSiteKey` | `string?` | `null` | The site key of the Cloudflare Turnstile widget. When it is set, the script loads Turnstile from `challenges.cloudflare.com`, renders the widget in the form with the action `contact` and sends its token. Without it nothing is loaded from Cloudflare and the token is sent empty, so leave it out only when the endpoint does not check Turnstile: one that does, such as the outline on the component's documentation page, answers `fields` to an empty token. |
| `ShowCompanyField` | `bool` | `false` | Whether an optional company field is shown. Its value is sent as `company`. |
| `ShowSubjectField` | `bool` | `false` | Whether an optional subject field is shown, between the email (or company) and the message. Its value is sent as `subject`, at most 200 characters; the field is not sent when it is hidden, so an endpoint that does not read a subject gets the same message as before. |
| `FormTitle` | `string` | `""` | The heading (`h2`) above the form. Empty by default, which renders nothing. |
| `FormDescription` | `string` | `""` | The text under the heading. Plain text; it is encoded. Empty by default, which renders nothing. |
| `NamePlaceholder` | `string?` | `null` | The placeholder of the name field. Null by default, which writes no placeholder. |
| `EmailPlaceholder` | `string?` | `null` | The placeholder of the email field. Null by default, which writes no placeholder. |
| `SubjectPlaceholder` | `string?` | `null` | The placeholder of the subject field. Null by default, which writes no placeholder. |
| `MessagePlaceholder` | `string?` | `null` | The placeholder of the message field. Null by default, which writes no placeholder. |
| `SideBySideLayout` | `bool` | `true` | Whether the contact information sits beside the form (`true`, the default) or below it. It only matters when the contact information is shown. Below 768px it is below the form either way. |
| `ShowContactInformation` | `bool` | `true` | Whether the contact information section is shown when `ContactInfo` is set. |
| `ContactInfoTitle` | `string` | `"Contact information"` | The title of the contact information section, as on `OsirionContactForm`. |
| `ContactInfoAddressLabel` | `string` | `"Address:"` | The label of the address in the contact information section. |
| `ContactInfoPhoneLabel` | `string` | `"Phone:"` | The label of the phone number in the contact information section. |
| `ContactInfoEmailLabel` | `string` | `"Email:"` | The label of the email address in the contact information section. |
| `ContactInfoWebsiteLabel` | `string` | `"Website:"` | The label of the website in the contact information section. |
| `ContactInfo` | `ContactInformation?` | `null` | The details shown in an `OsirionContactInfoSection` beside or below the form: address, phone, email, website, a message and extra rows (`ContactInformation.AdditionalItems`). When it is `null`, which is the default, no section is rendered. |
| `ShowRequiredIndicators` | `bool` | `true` | Whether a `*` follows the label of each required field: name, email and message. The marker is for the eye only; assistive technology announces the fields' `required` attribute instead. |
| `NameLabel` | `string` | `"Name"` | The label of the name field. |
| `EmailLabel` | `string` | `"Email"` | The label of the email field. |
| `CompanyLabel` | `string` | `"Company"` | The label of the company field, shown with `ShowCompanyField`. |
| `SubjectLabel` | `string` | `"Subject"` | The label of the subject field, shown with `ShowSubjectField`. |
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
| `AdditionalFields` | `IReadOnlyList<ContactFormField>?` | `null` | Fields the site adds after the message, in list order, such as a company size or a budget on a quote form: at most `ContactFormField.MaxFields`, each with a unique, safe `ContactFormField.Name`. The script sends their values as `fields`, an object keyed by name: a string for each field, an empty one when unanswered, and an array of the chosen option values for `ContactFormFieldType.Checkboxes`. The browser checks them only with native constraint validation (`required`, `maxlength`, `min`, `max`, the email type), so the endpoint must check every value against the same definitions. Null or empty, the default, renders the form as before and sends no `fields`. |
| `Theme` | `ThemeMode` | `ThemeMode.System` | The component's color mode. `Light` or `Dark` writes it to `data-theme` (`data-bs-theme` under Bootstrap) on the root element; `System` writes nothing. |
| `Class` | `string?` | `null` | Extra CSS classes for the component's root element. |
| `Style` | `string?` | `null` | An inline style for the component's root element. |

<!-- /osirion:parameters -->

Unmatched attributes, `Class`, `Style` and the `Theme` attribute go on the outer `<div>`.

## Behavior

- **Another language.** Every text the form shows is a parameter with its English text as the default: the labels
  (`NameLabel` and its siblings), placeholders, the button, the status messages and, from the release after 4.3.0,
  the contact information labels (`ContactInfoAddressLabel`, `ContactInfoPhoneLabel`, `ContactInfoEmailLabel`,
  `ContactInfoWebsiteLabel`). Fields are checked by the browser's own validation, which speaks the visitor's
  language, and the endpoint's answers show `FieldsErrorMessage`, `CheckErrorMessage` or `ErrorMessage`, so the form
  has no validation texts of its own to translate.
- **Hidden until its script runs.** The endpoint takes JSON, so without script the form could send nothing. It
  renders with the `hidden` attribute and its script reveals it; visitors without script see `NoScriptContent`, so give
  them another way to reach you, such as an email link. Where scripts run, the hidden form keeps its place, invisible,
  so revealing it does not move the content below it. The `<noscript>` is written without attributes: no scoped
  rule styles it, so it does not carry the component's CSS isolation attribute (`b-...`) into exported pages.
- **Static SSR and static files.** The script is a BlazorJSComponents module and needs no Blazor circuit. It works on a
  page served by ASP.NET Core with static SSR, and on a copy of that page served as a file, as long as the page loads
  `blazor.web.js` and keeps its `<!--Blazor-Web-Initializers:...-->` comment. A static export must include the
  files that comment names, the modules they import (such as `_content/BlazorJSComponents/component.mjs`) and the
  component's script under `_content/Osirion.Blazor.Core/Components/Forms/`; a crawler that follows only attributes
  misses them.
- **Turnstile.** With `TurnstileSiteKey`, the script loads
  `https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit` once per page, however many forms the page
  has, and renders the widget into the form with the action `contact`. The widget follows the color mode the
  component's `Theme` or the page sets (`data-theme`, `data-bs-theme` or `data-osirion-theme`), otherwise the visitor's
  system setting. Without a site key nothing is loaded from Cloudflare and the token is sent empty. The script tells
  Turnstile's API by its `render` method, so an element whose id is `turnstile`, such as the generated id of a
  "Turnstile" heading, does not stand in for it.
- **Sending.** On submit the browser checks the fields first. A send before the Turnstile check has finished shows
  `CheckPendingMessage` and sends nothing. While a message is on its way the button is disabled and shows
  `SubmittingText`. After every attempt the widget is reset, because a token passes one check only, and the button is
  enabled again. Focus that was on the button returns to it, unless the visitor moved it meanwhile. A success clears
  the fields; a failure keeps them, so the visitor can try again.
- **Errors from Turnstile.** When the widget reports an error, the status shows `CheckErrorMessage`. When its script
  cannot load or the widget cannot render, no token can arrive: the status shows `CheckErrorMessage`, and so does
  every send, which posts nothing.
- **Messages are text.** The status is written with `textContent` and every label, message and `PrivacyNote` is
  encoded, so none of them can carry markup.
- **Endpoint check.** `Endpoint` must be a path on the site, such as `/api/contact`, or an `https` URL. An empty value,
  `http:`, `javascript:`, `data:`, `mailto:`, a protocol-relative `//host` or a relative path without the leading `/`
  renders nothing, and in Development the component logs a warning that names the value.

## Accessibility

- Every field has a `label` bound with `for`, and name, email and company carry `autocomplete` (`name`, `email`,
  `organization`), so browsers and password managers can fill them.
- Name, email and message carry `required`, and their labels end in a `*` while `ShowRequiredIndicators` is on (the
  default). The `*` is `aria-hidden`, because assistive technology already announces `required`; the optional
  company field has none.
- The honeypot sits in an `aria-hidden` wrapper, out of the tab order (`tabindex="-1"`) and with `autocomplete="off"`,
  so neither people nor assistive technology reach it.
- The status is a `role="status"` live region. It is emptied before each attempt, so the same answer twice in a row is
  announced again.
- The browser's own validation messages explain a missing or malformed field before anything is sent.
- An additional field has a `label`, or a `fieldset` named by its `legend` for radio buttons and checkboxes, and its
  `HelpText` is linked with `aria-describedby`. Its `*` follows `Required` and `ShowRequiredIndicators`, and is
  `aria-hidden` like the others.

## Styling

| Class | Element |
| --- | --- |
| `osirion-static-contact-form` | The outer `<div>`, which takes `Class` and `Style` |
| `osirion-static-contact-form-form` | The `<form>`, a single column grid |
| `osirion-static-contact-form-field` | A label and its input |
| `osirion-static-contact-form-group` | The `<fieldset>` of an additional field with radio buttons or checkboxes |
| `osirion-static-contact-form-option` | One radio button or checkbox with its text |
| `osirion-static-contact-form-help` | The `HelpText` under an additional field, in `--osirion-text-secondary` |
| `osirion-static-contact-form-required` | The `*` after the label of a required field, in `--osirion-status-danger-text` |
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
