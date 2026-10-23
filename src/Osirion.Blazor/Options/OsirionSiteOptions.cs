using Osirion.Blazor.Components;
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
    /// <c>Book a call</c> linking to a scheduling page; <see langword="null"/>, the default, shows none.
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
    /// <c>app.MapOsirionCookieConsent()</c>.
    /// </summary>
    public bool ShowCookieConsent { get; set; }

    /// <summary>Gets or sets whether the scroll-to-top button is shown. Defaults to <see langword="true"/>.</summary>
    public bool ShowScrollToTop { get; set; } = true;

    /// <summary>Gets the problems that keep the options from describing a site; empty when they are valid.</summary>
    public IReadOnlyList<string> GetErrors()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add($"{Section}:Name is required.");
        }

        AddMenuErrors(Menu, "Menu", errors);
        AddCallToActionErrors(CallToAction, errors);
        if (Announcement is { } announcement && (string.IsNullOrWhiteSpace(announcement.Id) || string.IsNullOrWhiteSpace(announcement.Text)))
        {
            errors.Add($"{Section}:Announcement needs an Id and a Text.");
        }

        for (var index = 0; index < SocialLinks.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(SocialLinks[index].Href))
            {
                errors.Add($"{Section}:SocialLinks:{index} needs an Href.");
            }
        }

        return errors;
    }

    private static void AddCallToActionErrors(SiteCallToAction? callToAction, List<string> errors)
    {
        if (callToAction is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(callToAction.Text))
        {
            errors.Add($"{Section}:CallToAction needs a Text.");
        }

        if (string.IsNullOrWhiteSpace(callToAction.Href))
        {
            errors.Add($"{Section}:CallToAction needs an Href.");
        }
        else if (!SafeLink.IsSafe(callToAction.Href))
        {
            // The layout writes the address into the button's href as it is, so a javascript: or data: URL would run.
            errors.Add($"{Section}:CallToAction:Href must be a path on the site (starting with /) or an http, https or mailto address.");
        }
    }

    private static void AddMenuErrors(List<SiteMenuItem> items, string path, List<string> errors)
    {
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (string.IsNullOrWhiteSpace(item.Text))
            {
                errors.Add($"{Section}:{path}:{index} needs a Text.");
            }

            if (item.Items.Count == 0 && string.IsNullOrWhiteSpace(item.Href))
            {
                errors.Add($"{Section}:{path}:{index} needs an Href or Items.");
            }

            if (item.Items.Any(child => child.Items.Count > 0))
            {
                errors.Add($"{Section}:{path}:{index} nests deeper than one level; pass your own menu to the layout's Header for that.");
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
    /// Gets or sets the network, which picks the icon: <c>github</c>, <c>x</c>, <c>linkedin</c>, <c>nuget</c> or
    /// <c>rss</c>. Other values show <see cref="Label"/> as text.
    /// </summary>
    public string Network { get; set; } = string.Empty;

    /// <summary>Gets or sets the address of the profile.</summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the accessible name, for example <c>Osirion.Blazor on GitHub</c>. Defaults to the network's name.
    /// </summary>
    public string? Label { get; set; }
}

/// <summary>A link at the end of the menu, styled as the primary button of the site's CSS framework.</summary>
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
