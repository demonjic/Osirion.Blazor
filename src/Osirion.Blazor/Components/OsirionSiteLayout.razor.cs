using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Analytics.Options;
using Osirion.Blazor.Core.Utils;
using Osirion.Blazor.Internal;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Components;

/// <summary>
/// The whole site shell in one line: a skip link, an optional announcement, the main menu with the brand, search,
/// theme toggle and an optional call to action button, the page, and the footer with link columns, social profiles and
/// the copyright, plus the theme styles, the scroll-to-top button and, when turned on, the cookie consent banner.
/// Everything comes from <see cref="OsirionSiteOptions"/> (the <c>Osirion:Site</c> configuration section). Use it as
/// the router's default layout (<c>DefaultLayout="typeof(OsirionSiteLayout)"</c>), or wrap it in your own layout to
/// replace the header or footer. It renders without script; the menu and theme toggle add behavior where script runs.
/// </summary>
public partial class OsirionSiteLayout
{
    [Inject]
    private IOptions<OsirionSiteOptions> SiteOptions { get; set; } = default!;

    [Inject]
    private IOptions<ThemingOptions> ThemingOptions { get; set; } = default!;

    [Inject]
    private IOptions<AnalyticsConsentOptions> AnalyticsConsent { get; set; } = default!;

    /// <summary>
    /// Gets or sets the page when the layout wraps it in your own layout (<c>&lt;OsirionSiteLayout&gt;@Body&lt;/OsirionSiteLayout&gt;</c>).
    /// As the router's layout, the page comes from <see cref="LayoutComponentBase.Body"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Gets or sets a header that replaces the menu built from the options; the announcement stays.</summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>Gets or sets a footer that replaces the one built from the options.</summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Gets or sets more actions at the end of the menu, after the search box, theme toggle and call to action button.
    /// </summary>
    [Parameter]
    public RenderFragment? MenuActions { get; set; }

    /// <summary>Gets or sets the accessible name of the main menu. Defaults to <c>Main navigation</c>.</summary>
    [Parameter]
    public string MenuLabel { get; set; } = "Main navigation";

    private OsirionSiteOptions Site => SiteOptions.Value;

    // UseSite and the Osirion:Site section reject a blank Text or an unsafe Href at startup, but options configured
    // another way skip that check, so the layout applies the same rule before it writes the address, as the menu does
    // for its links.
    private SiteCallToAction? CallToAction =>
        Site.CallToAction is { } callToAction && !string.IsNullOrWhiteSpace(callToAction.Text) && SafeLink.IsSafe(callToAction.Href)
            ? callToAction
            : null;

    private string CallToActionClass => $"{ButtonClasses.Primary(ThemingOptions.Value.Framework)} osirion-site-call-to-action";

    // The banner records consent where the trackers look for it, so Osirion:Analytics:Consent:Evaluation moves both.
    private ConsentEvaluation CookieConsentEvaluation => AnalyticsConsent.Value.Evaluation;

    private IReadOnlyList<FooterSection>? FooterSections => NullIfEmpty(Site.FooterSections);

    private IReadOnlyList<FooterLink>? LegalLinks => NullIfEmpty(Site.LegalLinks);

    private IReadOnlyList<FooterSocialLink>? SocialLinks => NullIfEmpty(Site.SocialLinks.Select(link =>
    {
        var label = string.IsNullOrWhiteSpace(link.Label) ? SocialIcons.NameOf(link.Network) : link.Label;
        return new FooterSocialLink
        {
            Href = link.Href,
            Text = label,
            AriaLabel = label,
            Icon = SocialIcons.For(link.Network),
        };
    }).ToList());

    // An absolute http or https address leads to another site as a rule, which then cannot reach back into this page
    // through window.opener. The button still opens in the same tab, as the menu's other links do.
    private static string? GetCallToActionRel(string href) =>
        Uri.TryCreate(href, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            ? "noopener"
            : null;

    private static NavLinkMatch GetMatch(SiteMenuItem item) =>
        item.MatchPrefix && item.Href is { Length: > 1 } ? NavLinkMatch.Prefix : NavLinkMatch.All;

    private static IReadOnlyList<T>? NullIfEmpty<T>(IReadOnlyList<T> items) => items.Count > 0 ? items : null;
}
