namespace Osirion.Blazor.Options;

/// <summary>
/// The texts of the shell itself, the words <see cref="Components.OsirionSiteLayout"/> writes that are not the site's
/// content: link and button names, mostly heard through a screen reader. English by default; set them in the language
/// of the site, and per language version in <see cref="OsirionSiteOptions.Locales"/>.
/// </summary>
public sealed class SiteLabels
{
    /// <summary>Gets or sets the text of the skip link to the main content. Defaults to <c>Skip to main content</c>.</summary>
    public string SkipLink { get; set; } = "Skip to main content";

    /// <summary>Gets or sets the accessible name of the main menu. Defaults to <c>Main navigation</c>.</summary>
    public string Menu { get; set; } = "Main navigation";

    /// <summary>
    /// Gets or sets the accessible name of the button that opens the collapsed menu. Defaults to
    /// <c>Toggle navigation menu</c>.
    /// </summary>
    public string MenuToggle { get; set; } = "Toggle navigation menu";

    /// <summary>
    /// Gets or sets what the theme toggle says while the light theme is on. Defaults to <c>Switch to dark mode</c>.
    /// </summary>
    public string SwitchToDarkTheme { get; set; } = "Switch to dark mode";

    /// <summary>
    /// Gets or sets what the theme toggle says while the dark theme is on. Defaults to <c>Switch to light mode</c>.
    /// </summary>
    public string SwitchToLightTheme { get; set; } = "Switch to light mode";

    /// <summary>
    /// Gets or sets the search field's label and the search button's visible text. Defaults to <c>Search</c>.
    /// </summary>
    public string Search { get; set; } = "Search";

    /// <summary>
    /// Gets or sets the accessible name of the search button; keep the words of <see cref="Search"/> in it. Defaults to
    /// <c>Submit search</c>.
    /// </summary>
    public string SubmitSearch { get; set; } = "Submit search";

    /// <summary>
    /// Gets or sets the accessible name of the footer's legal links. Defaults to <c>Footer legal navigation</c>.
    /// </summary>
    public string LegalLinks { get; set; } = "Footer legal navigation";

    /// <summary>Gets or sets the accessible name of the announcement. Defaults to <c>Announcement</c>.</summary>
    public string Announcement { get; set; } = "Announcement";

    /// <summary>
    /// Gets or sets the accessible name of the announcement's close button. Defaults to <c>Dismiss announcement</c>.
    /// </summary>
    public string DismissAnnouncement { get; set; } = "Dismiss announcement";

    /// <summary>
    /// Gets or sets the accessible name of the language picker (<see cref="OsirionSiteOptions.LanguagePicker"/>).
    /// Defaults to <c>Language</c>.
    /// </summary>
    public string Language { get; set; } = "Language";

    /// <summary>
    /// Gets or sets the name of the home page as the first entry of the breadcrumbs in structured data, for example
    /// <c>Početna</c>. When empty, <c>Osirion:Seo:BreadcrumbHomeText</c> applies, then <c>Home</c>.
    /// </summary>
    public string? Home { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of a menu entry's submenu toggle, with <c>{0}</c> where the entry's text goes.
    /// Defaults to <c>Toggle {0} submenu</c>.
    /// </summary>
    public string ToggleSubmenu { get; set; } = Navigation.Components.MenuItem.DefaultToggleSubmenuLabel;

    /// <summary>
    /// Gets or sets the cookie consent banner's texts (<see cref="OsirionSiteOptions.ShowCookieConsent"/>). Null, the
    /// default, keeps the banner's English texts.
    /// </summary>
    public SiteCookieConsentLabels? CookieConsent { get; set; }
}
