# OsirionCookieConsent

Purpose
GDPR consent banner with optional categories and a customization panel. SSR-compatible.

Key parameters
- Title, Message
- AcceptButtonText, DeclineButtonText, CustomizeButtonText, SavePreferencesButtonText
- ShowDeclineButton, ShowCustomizeButton, ShowCustomizationPanel
- CustomizePanelTitle
- PolicyLink, PolicyLinkText
- Categories: IReadOnlyList<CookieCategory> (defaults: necessary, analytics, marketing, preferences)
- Position: bottom or top
- ConsentExpiryDays (default 365, posted with the form; the endpoint accepts 1 to 3650)
- ConsentEndpoint (server endpoint to persist preferences)
- Evaluation: ConsentEvaluation.Server (default) or ConsentEvaluation.Browser; OsirionSiteLayout passes Osirion:Analytics:Consent:Evaluation

Behavior (Evaluation = Server)
- Reads consent cookie "osirion_cookie_consent"; shows banner when absent
- Supports "?customize-cookies" query to open customization directly
- Renders an antiforgery token inside the form and posts the current path and query (never the absolute URL) as returnUrl

Behavior (Evaluation = Browser, for sites exported to static files)
- Renders the same banner on every page with the hidden attribute, in a div instead of the form: no antiforgery token, no returnUrl, no expiryDays field, and every button is type="button". Nothing is read from the request
- Renders the category panel hidden; Customize (an aria-expanded disclosure button) opens it, and is offered only with ShowCustomizationPanel on
- Writes data-osirion-consent-banner and data-osirion-consent-days (ConsentExpiryDays bounded as the endpoint bounds it) on the root, and loads wwwroot/js/osirion-consent.js through BlazorJSComponents (AddOsirion registers it; otherwise AddJSComponents(), or the banner throws)
- The script shows the banner when there is no valid consent cookie; writes Accept, Decline and Save Preferences to "osirion_cookie_consent" in the endpoint's JSON and escaping, path=/, SameSite=Lax, Secure on https, not HttpOnly; hides the banner; dispatches "osirion-consent-change" on window (detail.consentType, detail.categories); runs the scripts held for consented categories (see ConsentScripts.HoldUntilConsent)
- Any element with data-osirion-consent-open reopens the banner with the saved choices and focuses it. A choice that withdraws a category whose scripts already ran reloads the page, because a running tracker cannot be stopped
- The script cannot read or replace the endpoint's HttpOnly cookie, so a visitor who answered under Server evaluation sees the banner again until that cookie expires

Rendering modes
- Server: the banner needs the HTTP request (static server rendering or prerendering). Without an HttpContext, for example under WebAssembly or in an interactive server circuit, it renders nothing and does not throw
- Browser: renders in every render mode, with or without an HttpContext

Notes
- Emits osirion-cookie-consent-{position} and -customizing classes
- Map the server endpoint with MapOsirionCookieConsent and call UseAntiforgery; the endpoint rejects posts without a valid token and only redirects to local paths. See CookieConsent.md.
