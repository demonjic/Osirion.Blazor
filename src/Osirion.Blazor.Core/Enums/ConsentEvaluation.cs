namespace Osirion.Blazor.Components;

/// <summary>
/// Where the visitor's cookie consent is read and recorded.
/// </summary>
public enum ConsentEvaluation
{
    /// <summary>
    /// On the server, for each request: the cookie banner posts to the consent endpoint, which writes the consent
    /// cookie, and components read that cookie from the request. Needs the running app. The default.
    /// </summary>
    Server,

    /// <summary>
    /// In the visitor's browser: the page is the same for every visitor, the banner is rendered hidden and tracker
    /// scripts are rendered held, and the consent script shows the banner, writes the consent cookie and runs the held
    /// scripts the visitor consented to. For sites exported to static files.
    /// </summary>
    Browser,
}
