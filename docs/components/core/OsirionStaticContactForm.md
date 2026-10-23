# OsirionStaticContactForm

Purpose
A contact form for sites published as static files (for example on Cloudflare Workers static assets), where no ASP.NET
server receives a form post. Its script posts the message as JSON to an endpoint the site provides, such as a Cloudflare
Worker, with a Cloudflare Turnstile token and a honeypot field, and shows the endpoint's answer in a status region.
For a site with a server, use `OsirionContactForm`, which posts and validates without script.

Key parameters
- `Endpoint` (required): a path on the site (`/api/contact`) or an `https` URL; anything else renders nothing and, in
  Development, logs a warning
- `TurnstileSiteKey`: renders the Turnstile widget (action `contact`) and sends its token; without it nothing loads from Cloudflare
- `ShowCompanyField`: adds an optional `company` field (default false)
- `NameLabel`, `EmailLabel`, `CompanyLabel`, `MessageLabel`, `SubmitButtonText`, `SubmittingText`
- `SuccessMessage`, `FieldsErrorMessage`, `CheckErrorMessage`, `CheckPendingMessage`, `ErrorMessage`
- `PrivacyNote`: plain text under the form, encoded
- `NoScriptContent`: what visitors without script see, rendered inside `<noscript>`

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
  `{"name", "email", "company" (only with ShowCompanyField), "message", "website", "token"}`. `website` is the
  honeypot (empty for people), `token` the Turnstile response (empty without a site key).
- Answer: `{"ok": true}`, or `{"ok": false, "error": "fields" | "check" | "failed"}`. `fields` shows
  `FieldsErrorMessage`, `check` shows `CheckErrorMessage`; any other answer, a body that is not JSON or no answer
  shows `ErrorMessage`.
- The browser sends the request from the page's origin with an `Origin` header; the endpoint should accept only its own
  origin. A cross-origin endpoint has to answer the CORS preflight a JSON post needs.
- Browser limits mirror the reference endpoint: name 100, email 254, message 10 to 5000 characters, company 200.

Notes
- The form renders with `hidden` and its script reveals it, so it never shows a form that cannot send.
- Loads its script through BlazorJSComponents, which `AddOsirion` registers; without `AddOsirion`, call
  `builder.Services.AddJSComponents()`. The page must load `blazor.web.js` and carry its
  `<!--Blazor-Web-Initializers:...-->` comment, and a static export must include the files that comment names.
- Content-Security-Policy: add `https://challenges.cloudflare.com` to `script-src` and `frame-src`, and the endpoint's
  origin to `connect-src` when it is not `'self'`. No inline script, no inline event handler, no `unsafe-eval`.
  BlazorJSComponents writes `style="display: none; !important"` on its `bl-script` element; under a `style-src` without
  `'unsafe-inline'` the browser blocks it and logs one report, with no visible effect. Add
  `'unsafe-hashes' 'sha256-EXrMEQM0mPkgnkB6KboseHKD5Hs/aDUfUef9LAWu4uA='` to `style-src` to allow it.
- Every attempt resets the Turnstile widget, because a token passes one check only.
- The getosirion.com page (`content/en/components/static-contact-form.md`) has the Cloudflare Worker outline.
