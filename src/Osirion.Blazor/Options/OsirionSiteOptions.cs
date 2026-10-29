using System.Globalization;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Options;

/// <summary>
/// The site <see cref="OsirionSiteLayout"/> renders: its name and logo, the main menu, the footer and which
/// site-wide parts are shown. Bound from the <c>Osirion:Site</c> configuration section by
/// <c>AddOsirion(configuration)</c>, or set with <c>UseSite</c>.
/// </summary>
public sealed class OsirionSiteOptions
{
    /// <summary>The configuration section the options are read from.</summary>
    public const string Section = "Osirion:Site";

    /// <summary>Gets or sets the site's name, shown beside the logo and in the footer. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the logo shown before the name, for example <c>/favicon.png</c>.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets the address of the home page, which the brand links to. Defaults to <c>/</c>.</summary>
    public string HomeUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets the main menu: links, and links with one level of sub-items shown as a dropdown. For deeper or
    /// richer menus, pass your own menu to the layout's <c>Header</c>.
    /// </summary>
    public List<SiteMenuItem> Menu { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the menu has a search box. Defaults to <see langword="false"/>; turn it on when the site
    /// has a search page at <see cref="SearchUrl"/>.
    /// </summary>
    public bool ShowSearch { get; set; }

    /// <summary>Gets or sets the search page the box submits to. Defaults to <c>/search</c>.</summary>
    public string SearchUrl { get; set; } = "/search";

    /// <summary>Gets or sets the placeholder of the search box. Defaults to <c>Search</c>.</summary>
    public string SearchPlaceholder { get; set; } = "Search";

    /// <summary>Gets or sets whether the menu has the light and dark theme toggle. Defaults to <see langword="true"/>.</summary>
    public bool ShowThemeToggle { get; set; } = true;

    /// <summary>
    /// Gets or sets a button at the end of the menu, after the search box and theme toggle, for example
    /// <c>Book a call</c> linking to a scheduling page; <see langword="null"/>, the default, shows none. It stays in the
    /// bar when the menu collapses, and <see cref="SiteCallToAction.FooterSection"/> lists it in a footer column too.
    /// </summary>
    public SiteCallToAction? CallToAction { get; set; }

    /// <summary>Gets or sets a dismissible announcement above the menu, or <see langword="null"/> for none.</summary>
    public SiteAnnouncement? Announcement { get; set; }

    /// <summary>Gets or sets a sentence about the site in the footer.</summary>
    public string? FooterDescription { get; set; }

    /// <summary>Gets or sets the footer's link columns.</summary>
    public List<FooterSection> FooterSections { get; set; } = [];

    /// <summary>Gets or sets the profiles linked in the footer, drawn with the network's icon.</summary>
    public List<SiteSocialLink> SocialLinks { get; set; } = [];

    /// <summary>Gets or sets the links next to the copyright, such as the privacy policy.</summary>
    public List<FooterLink> LegalLinks { get; set; } = [];

    /// <summary>Gets or sets the owner named in the copyright line. Defaults to <see cref="Name"/>.</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the address the owner's name links to. Defaults to <see cref="HomeUrl"/>.</summary>
    public string? OwnerUrl { get; set; }

    /// <summary>
    /// Gets or sets the footer's copyright line, written as markup (it is configuration, not content). When empty the
    /// footer writes the year and <see cref="Owner"/>.
    /// </summary>
    public string? Copyright { get; set; }

    /// <summary>Gets or sets the heading above the footer's social links. Defaults to <c>Follow Us</c>.</summary>
    public string SocialTitle { get; set; } = "Follow Us";

    /// <summary>
    /// Gets or sets whether the cookie consent banner is shown. Defaults to <see langword="false"/>; the banner needs
    /// <c>app.MapOsirionCookieConsent()</c>, unless <c>Osirion:Analytics:Consent:Evaluation</c> is <c>Browser</c>, which
    /// records the choice in the browser for a site exported to static files.
    /// </summary>
    public bool ShowCookieConsent { get; set; }

    /// <summary>Gets or sets whether the scroll-to-top button is shown. Defaults to <see langword="true"/>.</summary>
    public bool ShowScrollToTop { get; set; } = true;

    /// <summary>
    /// Gets or sets where the shell shows <c>OsirionLanguagePicker</c>, which links the current page in each culture of
    /// <c>Osirion:Localization</c>: <see cref="SiteLanguagePicker.Header"/> (a compact disclosure among the menu's
    /// actions, so a row of language names does not wrap the menu), <see cref="SiteLanguagePicker.Footer"/> (a row of
    /// links below the footer's bottom row) or <see cref="SiteLanguagePicker.Both"/>. Defaults to
    /// <see cref="SiteLanguagePicker.None"/>. Needs <c>Osirion:Localization</c>; its name is
    /// <see cref="SiteLabels.Language"/>.
    /// </summary>
    public SiteLanguagePicker LanguagePicker { get; set; }

    /// <summary>
    /// Gets or sets the shell's own texts: the skip link, the names of the menu and its toggle, the theme toggle, the
    /// search box, and the names of the footer's legal links and of the announcement. English by default.
    /// </summary>
    public SiteLabels Labels { get; set; } = new();

    /// <summary>
    /// Gets or sets what each language version of the site says differently, keyed by culture name in any letter case,
    /// for example <c>sr-Latn-RS</c> or <c>en</c>. The layout uses the entry of the request's UI culture
    /// (<see cref="CultureInfo.CurrentUICulture"/>, which <c>UseRequestLocalization</c> sets), or of its nearest parent
    /// culture with an entry (<c>sr-Latn</c>, then <c>sr</c>, for <c>sr-Latn-RS</c>); a culture with neither gets these
    /// options as they are. Empty by default.
    /// </summary>
    public Dictionary<string, SiteLocaleOptions> Locales { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the site as one language version renders it: these options with the values of the <see cref="Locales"/>
    /// entry for <paramref name="culture"/> or its nearest parent culture, or these options when no entry matches. The
    /// options themselves are not changed.
    /// </summary>
    /// <param name="culture">The culture of the request, usually <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <returns>The options of that version, which have no locales of their own.</returns>
    public OsirionSiteOptions ForCulture(CultureInfo culture) => ForCulture(culture, localePaths: null);

    /// <summary>
    /// Gets the site as one language version renders it, as <see cref="ForCulture(CultureInfo)"/> does, with every
    /// address on the site that the version takes from these options (the menu, the home and search pages, the call to
    /// action, the announcement's link, the footer columns, the legal links and the owner's link) put under the
    /// culture's prefix by <paramref name="localePaths"/>: <c>/contact</c> becomes <c>/sr/contact</c>. So a
    /// <see cref="Locales"/> entry needs only its texts. An address the entry writes itself is kept as written, and
    /// absolute, <c>mailto:</c>, <c>tel:</c> and fragment links are never changed.
    /// </summary>
    /// <param name="culture">The culture of the request, usually <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <param name="localePaths">The site's culture prefixes, when <c>Osirion:Localization</c> is registered.</param>
    /// <returns>The options of that version, which have no locales of their own.</returns>
    public OsirionSiteOptions ForCulture(CultureInfo culture, IOsirionLocalePaths? localePaths)
    {
        ArgumentNullException.ThrowIfNull(culture);

        var entry = FindEntry(culture);
        if (localePaths is null)
        {
            return entry is null ? this : With(entry, localize: null);
        }

        return With(entry ?? new SiteLocaleOptions(), address => localePaths.Localize(address, culture));
    }

    private SiteLocaleOptions? FindEntry(CultureInfo culture)
    {
        for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
        {
            // Compared here, not by the dictionary, because options set in code may bring a dictionary of their own.
            var entry = Locales.FirstOrDefault(locale => string.Equals(locale.Key, current.Name, StringComparison.OrdinalIgnoreCase));
            if (entry.Value is not null)
            {
                return entry.Value;
            }
        }

        return null;
    }

    /// <summary>Gets the problems that keep the options from describing a site; empty when they are valid.</summary>
    public IReadOnlyList<string> GetErrors()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add($"{Section}:Name is required.");
        }

        AddMenuErrors(Menu, $"{Section}:Menu", errors);
        AddCallToActionErrors(CallToAction, Section, errors);
        AddCallToActionFooterSectionErrors(Section, errors);
        AddAnnouncementErrors(Announcement, Section, errors);
        AddSocialLinkErrors(SocialLinks, Section, errors);

        foreach (var (culture, entry) in Locales)
        {
            AddLocaleErrors(culture, entry, errors);
        }

        return errors;
    }

    // Links are merged with the site's by SiteLinks: an entry's link without an address takes the site's at the same
    // position, and every address taken from the site goes through localize.
    private OsirionSiteOptions With(SiteLocaleOptions entry, Func<string, string>? localize) => new()
    {
        Name = entry.Name ?? Name,
        LogoUrl = entry.LogoUrl ?? LogoUrl,
        HomeUrl = entry.HomeUrl ?? SiteLinks.Address(HomeUrl, localize),
        Menu = SiteLinks.Menu(entry.Menu, Menu, localize),
        ShowSearch = ShowSearch,
        SearchUrl = entry.SearchUrl ?? SiteLinks.Address(SearchUrl, localize),
        SearchPlaceholder = entry.SearchPlaceholder ?? SearchPlaceholder,
        ShowThemeToggle = ShowThemeToggle,
        CallToAction = SiteLinks.CallToAction(entry.CallToAction, CallToAction, localize),
        Announcement = SiteLinks.Announcement(entry.Announcement, Announcement, localize),
        FooterDescription = entry.FooterDescription ?? FooterDescription,
        FooterSections = SiteLinks.FooterSections(entry.FooterSections, FooterSections, localize),
        SocialLinks = entry.SocialLinks ?? SocialLinks,
        LegalLinks = SiteLinks.LegalLinks(entry.LegalLinks, LegalLinks, localize),
        Owner = entry.Owner ?? Owner,
        OwnerUrl = entry.OwnerUrl ?? (OwnerUrl is null ? null : SiteLinks.Address(OwnerUrl, localize)),
        Copyright = entry.Copyright ?? Copyright,
        SocialTitle = entry.SocialTitle ?? SocialTitle,
        ShowCookieConsent = ShowCookieConsent,
        ShowScrollToTop = ShowScrollToTop,
        LanguagePicker = LanguagePicker,
        Labels = entry.Labels ?? Labels,
    };

    // A version is checked for the values it sets, so a problem in a value it inherits is reported once, under the
    // site's own key. The footer section of a call to action is checked against the sections the version shows.
    private void AddLocaleErrors(string culture, SiteLocaleOptions? entry, List<string> errors)
    {
        var key = $"{Section}:Locales:{culture}";
        if (!IsCultureName(culture))
        {
            errors.Add($"{key} is not a culture name, such as sr-Latn-RS or en.");
            return;
        }

        if (entry is null)
        {
            return;
        }

        if (entry.Name is not null && string.IsNullOrWhiteSpace(entry.Name))
        {
            errors.Add($"{key}:Name cannot be blank.");
        }

        // An entry's link without an address takes the site's at the same position, so the merged links are checked.
        if (entry.Menu is not null)
        {
            AddMenuErrors(SiteLinks.Menu(entry.Menu, Menu, localize: null), $"{key}:Menu", errors);
        }

        if (entry.CallToAction is not null)
        {
            AddCallToActionErrors(SiteLinks.CallToAction(entry.CallToAction, CallToAction, localize: null), key, errors);
        }

        if (entry.CallToAction is not null || entry.FooterSections is not null)
        {
            With(entry, localize: null).AddCallToActionFooterSectionErrors(key, errors);
        }

        AddAnnouncementErrors(entry.Announcement, key, errors);
        AddSocialLinkErrors(entry.SocialLinks ?? [], key, errors);
        AddDerivedFromErrors(culture, entry, key, errors);
    }

    // The checks that need no service; the transliterator and the source culture are checked where they are registered.
    private void AddDerivedFromErrors(string culture, SiteLocaleOptions entry, string key, List<string> errors)
    {
        if (entry.DerivedFrom is null && entry.Transliteration is null)
        {
            return;
        }

        if (!IsPredefinedCultureName(entry.DerivedFrom))
        {
            errors.Add($"{key}:DerivedFrom '{entry.DerivedFrom}' is not a culture name, such as sr.");
            return;
        }

        if (string.IsNullOrWhiteSpace(entry.Transliteration))
        {
            errors.Add($"{key}:Transliteration is required with DerivedFrom.");
        }

        if (CultureInfo.GetCultureInfo(entry.DerivedFrom!).Name.Equals(CultureInfo.GetCultureInfo(culture).Name, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"{key}:DerivedFrom cannot be the entry's own culture.");
            return;
        }

        // A chain would need an order to derive in, and a cycle has none; one level is all a second script needs.
        var source = Locales.FirstOrDefault(locale => string.Equals(locale.Key, entry.DerivedFrom, StringComparison.OrdinalIgnoreCase)).Value;
        if (source?.DerivedFrom is not null)
        {
            errors.Add($"{key}:DerivedFrom '{entry.DerivedFrom}' is itself derived; derive from a version written by hand.");
        }
    }

    // A source must be a culture the platform knows: a misspelt name would otherwise become a custom culture.
    private static bool IsPredefinedCultureName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        try
        {
            return CultureInfo.GetCultureInfo(name, predefinedOnly: true).Name.Length > 0;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static bool IsCultureName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        try
        {
            return CultureInfo.GetCultureInfo(name).Name.Length > 0;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static void AddCallToActionErrors(SiteCallToAction? callToAction, string key, List<string> errors)
    {
        if (callToAction is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(callToAction.Text))
        {
            errors.Add($"{key}:CallToAction needs a Text.");
        }

        if (string.IsNullOrWhiteSpace(callToAction.Href))
        {
            errors.Add($"{key}:CallToAction needs an Href.");
        }
        else if (!SafeLink.IsSafe(callToAction.Href))
        {
            // The address goes into the button's href, where a javascript: or data: URL would run. The layout skips such a
            // button too, but failing here tells the site why it is missing.
            errors.Add($"{key}:CallToAction:Href must be a path on the site (starting with /) or an http, https or mailto address.");
        }
    }

    private void AddCallToActionFooterSectionErrors(string key, List<string> errors)
    {
        var title = CallToAction?.FooterSection;
        if (string.IsNullOrWhiteSpace(title) || FooterSections.Exists(section => string.Equals(section.Title, title, StringComparison.Ordinal)))
        {
            return;
        }

        // The layout would find no column to add the link to; failing here tells the site why the link is missing.
        errors.Add($"{key}:CallToAction:FooterSection is \"{title}\", which is not the Title of any of {key}:FooterSections.");
    }

    private static void AddAnnouncementErrors(SiteAnnouncement? announcement, string key, List<string> errors)
    {
        if (announcement is not null && (string.IsNullOrWhiteSpace(announcement.Id) || string.IsNullOrWhiteSpace(announcement.Text)))
        {
            errors.Add($"{key}:Announcement needs an Id and a Text.");
        }
    }

    private static void AddSocialLinkErrors(List<SiteSocialLink> socialLinks, string key, List<string> errors)
    {
        for (var index = 0; index < socialLinks.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(socialLinks[index].Href))
            {
                errors.Add($"{key}:SocialLinks:{index} needs an Href.");
            }
        }
    }

    private static void AddMenuErrors(List<SiteMenuItem> items, string path, List<string> errors)
    {
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (string.IsNullOrWhiteSpace(item.Text))
            {
                errors.Add($"{path}:{index} needs a Text.");
            }

            if (item.Items.Count == 0 && string.IsNullOrWhiteSpace(item.Href))
            {
                errors.Add($"{path}:{index} needs an Href or Items.");
            }

            if (item.Items.Any(child => child.Items.Count > 0))
            {
                errors.Add($"{path}:{index} nests deeper than one level; pass your own menu to the layout's Header for that.");
            }

            AddMenuErrors(item.Items, $"{path}:{index}:Items", errors);
        }
    }
}

/// <summary>An entry of the site's main menu.</summary>
public sealed class SiteMenuItem
{
    /// <summary>Gets or sets the text of the entry.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the address the entry links to; an entry with <see cref="Items"/> needs none.</summary>
    public string? Href { get; set; }

    /// <summary>Gets or sets a short description, shown under the text in a dropdown.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the entry is marked as current on every page below its address, for example a
    /// <c>/blog</c> entry on <c>/blog/a-post</c>. Defaults to <see langword="true"/>, except for the home page.
    /// </summary>
    public bool MatchPrefix { get; set; } = true;

    /// <summary>Gets or sets the sub-items, shown as a dropdown.</summary>
    public List<SiteMenuItem> Items { get; set; } = [];
}

/// <summary>A profile on another site, linked from the footer.</summary>
public sealed class SiteSocialLink
{
    /// <summary>
    /// Gets or sets the network, which picks the icon: <c>github</c>, <c>x</c>, <c>linkedin</c>, <c>nuget</c>,
    /// <c>rss</c>, <c>facebook</c> or <c>instagram</c>, in any letter case. Other values show <see cref="Label"/> as
    /// text.
    /// </summary>
    public string Network { get; set; } = string.Empty;

    /// <summary>Gets or sets the address of the profile.</summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accessible name, for example <c>Osirion.Blazor on GitHub</c>. Defaults to the network's name.
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>
/// A link at the end of the menu, styled as the primary button of the site's CSS framework, and optionally the last
/// link of a footer column.
/// </summary>
public sealed class SiteCallToAction
{
    /// <summary>Gets or sets the text of the button, for example <c>Book a call</c>. Required.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address the button links to: a path on the site (starting with <c>/</c>) or an <c>http</c>,
    /// <c>https</c> or <c>mailto</c> address. Required. An <c>http</c> or <c>https</c> address gets
    /// <c>rel="noopener"</c> and opens in the same tab.
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <c>Title</c> of the footer section (an entry of <see cref="OsirionSiteOptions.FooterSections"/>)
    /// whose links end with the call to action, so a footer column offers it without repeating its address, for example
    /// <c>Contact</c>. The title must match exactly, letter case included, or the options fail validation;
    /// <see langword="null"/> or blank, the default, keeps the call to action out of the footer.
    /// </summary>
    public string? FooterSection { get; set; }
}

/// <summary>A dismissible announcement above the menu.</summary>
public sealed class SiteAnnouncement
{
    /// <summary>
    /// Gets or sets the announcement's id. A reader who dismisses it no longer sees it; change the id to show a new
    /// announcement to everyone.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the text of an optional link after the text.</summary>
    public string? LinkText { get; set; }

    /// <summary>Gets or sets the address of the link.</summary>
    public string? LinkUrl { get; set; }
}
