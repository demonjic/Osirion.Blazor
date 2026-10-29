# OsirionCookieConsent

GDPR-ready cookie consent banner with categories, form-based submission, and SSR support.

Basic usage

```razor
@using Osirion.Blazor.Components

<OsirionCookieConsent />
```

Customized

```razor
<OsirionCookieConsent 
    Title="Privacy Preferences"
    Message="We use cookies to improve your experience."
    PolicyLink="/privacy"
    PolicyLinkText="Privacy Policy"
    ShowCustomizeButton="true"
    Categories="@Categories" />

@code {
    private static readonly List<CookieCategory> Categories = new()
    {
        new() { Id = "necessary", Name = "Necessary", Description = "Required", IsRequired = true, IsEnabled = true },
        new() { Id = "analytics", Name = "Analytics", Description = "Usage metrics" }
    };
}
```

Server endpoint

Register the services and map the endpoint. The endpoint requires a valid antiforgery token, so the app must also call `UseAntiforgery()` after `UseRouting()` (when the app calls it) and before the endpoints, as the Blazor Web App template does. Registering the antiforgery services does not add the middleware. Without it, every post to the endpoint fails with `InvalidOperationException` ("contains anti-forgery metadata, but a middleware was not found that supports anti-forgery") and a 500 response.

```csharp
builder.Services.AddRazorComponents();
builder.Services.AddOsirionCookieConsent(); // also registers antiforgery services

var app = builder.Build();

app.UseAntiforgery();
app.MapOsirionCookieConsent(); // POST /api/cookie-consent
```

The banner posts to ConsentEndpoint (default /api/cookie-consent) with an antiforgery token and the current page as a local path and query. The endpoint:

- returns 400 without a body, and records nothing, when the antiforgery token is missing or invalid or the body is not a form post;
- writes the consent cookie and redirects back to the return URL only when it is a local path such as `/blog/post?page=2`; absolute, protocol-relative (`//host`) and other non-local values redirect to `/`;
- for Customize, redirects back with a single `customize-cookies=true` query flag, and removes a leftover flag after consent is saved.

If you post to the endpoint from your own markup, include `<AntiforgeryToken />` inside the form and post a local path as `returnUrl`.

Static hosting (consent evaluated in the browser)

A site exported to static files serves one file to every visitor, so there is no request to read the cookie from and no endpoint to post to. Set one key and the banner and the trackers move to the browser together:

```json
{
  "Osirion": {
    "Site": { "ShowCookieConsent": true },
    "Analytics": { "Consent": { "RequireConsent": true, "Evaluation": "Browser" } }
  }
}
```

`OsirionSiteLayout` passes `Osirion:Analytics:Consent:Evaluation` to its banner; a page that renders the banner itself writes `<OsirionCookieConsent Evaluation="ConsentEvaluation.Browser" />`. The banner is rendered hidden in every page, without a form, and `osirion-consent.js` shows it when the visitor has no valid consent cookie, writes the cookie in the endpoint's format (not `HttpOnly`, since script writes it), and runs the tracker scripts that were rendered held (`type="text/plain" data-osirion-consent="analytics"`) once the consent includes their category. A link to any page with `?customize-cookies=true` (for example an `Osirion:Site:LegalLinks` entry) or a link or button with `data-osirion-consent-open` reopens the banner; withdrawing a category whose trackers already ran reloads the page. No endpoint or antiforgery setup is needed. The script is a file from your own origin with no inline code; the inline tracker snippets need their `'sha256-...'` hashes in `script-src`. Full details: content/en/components/cookie-consent.md, section "Sites exported to static files".
