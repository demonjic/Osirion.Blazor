using Osirion.Blazor.Components;

namespace Osirion.Blazor.Analytics.Options;

/// <summary>
/// Controls whether tracker scripts wait for the visitor's analytics consent, and where that consent is evaluated.
/// </summary>
/// <remarks>
/// With <see cref="Evaluation"/> at <see cref="ConsentEvaluation.Server"/> (the default), consent is read on the server
/// from the cookie written by <c>OsirionCookieConsent</c>, so the gate works under Static SSR and in the prerender pass
/// of interactive modes. When the gate is on and no <c>HttpContext</c> is available (for example inside an already
/// interactive component), the tracker renders nothing. With <see cref="ConsentEvaluation.Browser"/> the trackers render
/// their scripts held for every visitor and the consent script runs them in the browser, which is what a site exported
/// to static files needs. Turning the gate on is recommended for sites with visitors in the EU.
/// </remarks>
public class AnalyticsConsentOptions
{
    /// <summary>Configuration section name.</summary>
    public const string Section = "Osirion:Analytics:Consent";

    /// <summary>
    /// Gets or sets whether trackers render only after the visitor consented to analytics.
    /// The default is <see langword="false"/> so existing hosts keep tracking as before.
    /// </summary>
    public bool RequireConsent { get; set; }

    /// <summary>
    /// Gets or sets the consent category that must be accepted. Defaults to <c>analytics</c>, the
    /// category written by <c>OsirionCookieConsent</c>.
    /// </summary>
    public string Category { get; set; } = "analytics";

    /// <summary>
    /// Gets or sets where consent is evaluated. <c>Server</c>, the default, reads the consent cookie for each request and
    /// renders a tracker only after consent. <c>Browser</c> renders every tracker for every visitor with its scripts held
    /// (<c>type="text/plain"</c>), and the consent script runs them once the visitor's consent includes
    /// <see cref="Category"/>: use it for a site exported to static files. It takes effect only with
    /// <see cref="RequireConsent"/> on. <c>OsirionSiteLayout</c> renders its cookie banner in the same mode, so this one
    /// setting (<c>Osirion:Analytics:Consent:Evaluation</c>) moves the banner and the trackers together.
    /// </summary>
    public ConsentEvaluation Evaluation { get; set; } = ConsentEvaluation.Server;
}
