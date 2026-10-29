# OsirionStaticContactForm

Purpose
A contact form for sites published as static files (for example on Cloudflare Workers static assets), where no ASP.NET
server receives a form post. Its script posts the message as JSON to an endpoint the site provides, such as a Cloudflare
Worker, with a Cloudflare Turnstile token and a honeypot field, and shows the endpoint's answer in a status region.
For a site with a server, use `OsirionContactForm`, which posts and validates without script.

Key parameters
- `Endpoint` (required): a path on the site (`/api/contact`) or an `https` URL; anything else renders nothing and, in
  Development, logs a warning
- `TurnstileSiteKey`: renders the Turnstile widget (action `contact`) and sends its token; without it nothing loads
  from Cloudflare and the token is empty, which an endpoint that checks Turnstile (such as the docs' Worker outline)
  refuses as `fields`
- `ShowCompanyField`: adds an optional `company` field (default false)
- `ShowSubjectField`: adds an optional `subject` field (default false), up to 200 characters, after the company field;
  `SubjectLabel` renames it
- `FormTitle`, `FormDescription`: a heading (`h2`) and a plain-text paragraph above the form; both empty by default
- `NamePlaceholder`, `EmailPlaceholder`, `SubjectPlaceholder`, `MessagePlaceholder`: placeholders; none by default
- `ContactInfo` (a `ContactInformation`), `ShowContactInformation` (default true), `ContactInfoTitle` (default
  "Contact information"): an `OsirionContactInfoSection` beside the form (below 768px, below it); `ContactInfo` is
  null by default and then no section is rendered. `ContactInformation.AdditionalItems` adds rows of your own
  (label, text, an optional safe link, an optional icon), such as a booking link or working hours
- `SideBySideLayout` (default true): the section sits beside the form; false puts it below
- `ShowRequiredIndicators`: a `*` after the labels of name, email and message (default true); it is `aria-hidden`,
  since the inputs carry `required`
- `NameLabel`, `EmailLabel`, `CompanyLabel`, `MessageLabel`, `SubmitButtonText`, `SubmittingText`
- `SuccessMessage`, `FieldsErrorMessage`, `CheckErrorMessage`, `CheckPendingMessage`, `ErrorMessage`
- `PrivacyNote`: plain text under the form, encoded
- `AdditionalFields`: up to 20 `ContactFormField` definitions rendered after the message, the same definitions as
  `OsirionContactForm` takes; their inputs are named `field.{Name}` and sent under `fields` (see the wire contract).
  An invalid definition throws an `ArgumentException`
- `NoScriptContent`: what visitors without script see, rendered inside a `<noscript>` that carries no CSS isolation
  attribute

Usage
```razor
<OsirionStaticContactForm Endpoint="/api/contact"
                          TurnstileSiteKey="@turnstileSiteKey"
                          PrivacyNote="Your message is emailed to us and not stored on this site.">
    <NoScriptContent>
        <p>The form needs JavaScript. Write to <a href="mailto:hello@example.com">hello@example.com</a> instead.</p>
    </NoScriptContent>
</OsirionStaticContactForm>
```

Wire contract
- Request: `POST` to `Endpoint`, `Content-Type: application/json`, body
  `{"name", "email", "company" (only with ShowCompanyField), "subject" (only with ShowSubjectField), "message",
  "fields" (only with AdditionalFields), "website", "token"}`. `website` is the
  honeypot (empty for people), `token` the Turnstile response (empty without a site key).
- `fields` is an object keyed by each `ContactFormField.Name`: a string per field (empty when unanswered), an array of
  the checked option values for `Checkboxes`. Example: `{"team_size": "11-50", "services": ["design"], "budget": ""}`.
  The browser applies native constraint validation only (`required`, `maxlength`, `min`, `max`, the email type; a
  required checkbox group is not checked), so the endpoint must validate every value again against the same
  definitions, cap all answers at 10,000 characters and ignore names it did not define. The getosirion.com page has a
  checking function for the Worker outline.
- Answer: `{"ok": true}`, or `{"ok": false, "error": "fields" | "check" | "failed"}`. `fields` shows
  `FieldsErrorMessage`, `check` shows `CheckErrorMessage`; any other answer, a body that is not JSON or no answer
  shows `ErrorMessage`.
- The browser sends the request from the page's origin with an `Origin` header; the endpoint should accept only its own
  origin. A cross-origin endpoint has to answer the CORS preflight a JSON post needs.
- Browser limits mirror the reference endpoint: name 100, email 254, message 10 to 5000 characters, company 200, subject 200.

Notes
- The form renders with `hidden` and its script reveals it, so it never shows a form that cannot send. Where scripts
  run, the hidden form keeps its place, invisible, so revealing it causes no layout shift.
- Loads its script through BlazorJSComponents, which `AddOsirion` registers; without `AddOsirion`, call
  `builder.Services.AddJSComponents()`. The page must load `blazor.web.js` and carry its
  `<!--Blazor-Web-Initializers:...-->` comment, and a static export must include the files that comment names and
  the modules they import, such as `_content/BlazorJSComponents/component.mjs`.
- Content-Security-Policy: add `https://challenges.cloudflare.com` to `script-src` and `frame-src`, and the endpoint's
  origin to `connect-src` when it is not `'self'`. No inline script, no inline event handler, no `unsafe-eval`.
  BlazorJSComponents writes `style="display: none; !important"` on its `bl-script` element; under a `style-src` without
  `'unsafe-inline'` the browser blocks it and logs one report, with no visible effect. Add
  `'unsafe-hashes' 'sha256-EXrMEQM0mPkgnkB6KboseHKD5Hs/aDUfUef9LAWu4uA='` to `style-src` to allow it.
- Every attempt resets the Turnstile widget, because a token passes one check only. Focus on the button returns to it
  after the send, unless the visitor moved it.
- When Turnstile's script cannot load or its widget cannot render, every send shows `CheckErrorMessage` and posts
  nothing. The API is recognized by its `render` method, so an element with the id `turnstile` does not hide it.
- Cloudflare's test secret answers siteverify with the hostname `example.com` and no action, so an endpoint that checks
  both refuses the test keys. The getosirion.com page (`content/en/components/static-contact-form.md`) has the
  Cloudflare Worker outline, with a switch for local runs that keeps those checks for real keys.
