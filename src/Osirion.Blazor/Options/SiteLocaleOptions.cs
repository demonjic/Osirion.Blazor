using Osirion.Blazor.Components;

namespace Osirion.Blazor.Options;

/// <summary>
/// What one language version of the site says differently: an entry of <see cref="OsirionSiteOptions.Locales"/>. Each
/// value it sets replaces the site's value as a whole (a <see cref="Menu"/> is the whole menu, <see cref="Labels"/> all
/// the labels, with English for any it leaves out); each value it leaves out is the site's. Whether search, the theme
/// toggle, the cookie banner and the scroll-to-top button are shown is the same for every version, so it is not here.
/// </summary>
public sealed class SiteLocaleOptions
{
    /// <summary>Gets or sets the site's name in this version.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the logo, for a version whose logo carries text.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets the address of this version's home page, for example <c>/en</c>.</summary>
    public string? HomeUrl { get; set; }

    /// <summary>Gets or sets this version's main menu.</summary>
    public List<SiteMenuItem>? Menu { get; set; }

    /// <summary>Gets or sets this version's search page.</summary>
    public string? SearchUrl { get; set; }

    /// <summary>Gets or sets the placeholder of the search box.</summary>
    public string? SearchPlaceholder { get; set; }

    /// <summary>Gets or sets this version's call to action.</summary>
    public SiteCallToAction? CallToAction { get; set; }

    /// <summary>Gets or sets this version's announcement.</summary>
    public SiteAnnouncement? Announcement { get; set; }

    /// <summary>Gets or sets the sentence about the site in the footer.</summary>
    public string? FooterDescription { get; set; }

    /// <summary>Gets or sets the footer's link columns.</summary>
    public List<FooterSection>? FooterSections { get; set; }

    /// <summary>Gets or sets the profiles linked in the footer.</summary>
    public List<SiteSocialLink>? SocialLinks { get; set; }

    /// <summary>Gets or sets the links next to the copyright.</summary>
    public List<FooterLink>? LegalLinks { get; set; }

    /// <summary>Gets or sets the owner named in the copyright line.</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the address the owner's name links to.</summary>
    public string? OwnerUrl { get; set; }

    /// <summary>Gets or sets the footer's copyright line, written as markup.</summary>
    public string? Copyright { get; set; }

    /// <summary>Gets or sets the heading above the footer's social links.</summary>
    public string? SocialTitle { get; set; }

    /// <summary>
    /// Gets or sets the shell's texts in this version's language, English for any it leaves out. In configuration give
    /// it at least one key: an empty section does not exist there, so it would leave the site's labels in place.
    /// </summary>
    public SiteLabels? Labels { get; set; }
}
