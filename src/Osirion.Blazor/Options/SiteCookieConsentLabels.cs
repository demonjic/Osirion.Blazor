namespace Osirion.Blazor.Options;

/// <summary>
/// The cookie consent banner's texts in one language version (<see cref="SiteLabels.CookieConsent"/>). A text left
/// out keeps the banner's English default, so a version sets only what it translates.
/// </summary>
public sealed class SiteCookieConsentLabels
{
    /// <summary>Gets or sets the banner's heading, also the name of its region. Default: <c>Cookie Consent</c>.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the banner's text.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets the text of the button that accepts every category. Default: <c>Accept All</c>.</summary>
    public string? AcceptButton { get; set; }

    /// <summary>Gets or sets the text of the button that declines the optional categories. Default: <c>Decline</c>.</summary>
    public string? DeclineButton { get; set; }

    /// <summary>Gets or sets the text of the button that opens the categories. Default: <c>Customize</c>.</summary>
    public string? CustomizeButton { get; set; }

    /// <summary>Gets or sets the text of the button that saves the ticked categories. Default: <c>Save Preferences</c>.</summary>
    public string? SavePreferencesButton { get; set; }

    /// <summary>Gets or sets the heading of the category panel. Default: <c>Cookie Preferences</c>.</summary>
    public string? CustomizePanelTitle { get; set; }

    /// <summary>Gets or sets the text of the policy link. Default: <c>Learn more</c>.</summary>
    public string? PolicyLinkText { get; set; }

    /// <summary>Gets or sets the mark after a required category's name. Default: <c>(Required)</c>.</summary>
    public string? Required { get; set; }

    /// <summary>
    /// Gets or sets the names and descriptions of the categories, keyed by their ID: <c>necessary</c>,
    /// <c>analytics</c>, <c>marketing</c> and <c>preferences</c>. A category or text left out keeps its English default;
    /// the IDs the consent records do not change.
    /// </summary>
    public Dictionary<string, SiteCookieCategoryLabels> Categories { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>The name and description of one cookie category in one language version.</summary>
public sealed class SiteCookieCategoryLabels
{
    /// <summary>Gets or sets the category's name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the category's description.</summary>
    public string? Description { get; set; }
}
