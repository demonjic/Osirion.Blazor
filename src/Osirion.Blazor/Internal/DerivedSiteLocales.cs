using System.Globalization;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Transliteration;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Internal;

/// <summary>
/// Fills each <see cref="SiteLocaleOptions"/> entry that names <see cref="SiteLocaleOptions.DerivedFrom"/> with its
/// source culture's version, transliterated, before the options are validated and used, so
/// <see cref="OsirionSiteOptions.ForCulture(System.Globalization.CultureInfo)"/> serves a derived version like any other. A value the entry sets itself
/// wins. An entry that breaks a rule is left as it is, for the validators to report.
/// </summary>
internal sealed class DerivedSiteLocales : IPostConfigureOptions<OsirionSiteOptions>
{
    private readonly TextTransliterators _transliterators;
    private readonly IOsirionLocalePaths? _localePaths;

    public DerivedSiteLocales(TextTransliterators transliterators)
    {
        _transliterators = transliterators;
    }

    public DerivedSiteLocales(TextTransliterators transliterators, IOsirionLocalePaths localePaths)
        : this(transliterators)
    {
        _localePaths = localePaths;
    }

    public void PostConfigure(string? name, OsirionSiteOptions options)
    {
        // Every source is read before any entry changes, so the result does not depend on the entries' order.
        var derived = new List<(string Culture, SiteLocaleOptions Entry)>();
        foreach (var (culture, entry) in options.Locales)
        {
            if (TryDerive(options, culture, entry, out var version))
            {
                derived.Add((culture, version));
            }
        }

        foreach (var (culture, entry) in derived)
        {
            options.Locales[culture] = entry;
        }
    }

    private bool TryDerive(OsirionSiteOptions options, string culture, SiteLocaleOptions? entry, out SiteLocaleOptions version)
    {
        version = null!;
        if (entry?.DerivedFrom is not { } from
            || !_transliterators.TryGet(entry.Transliteration, out var transliterator)
            || !TryGetCulture(from, out var source)
            || IsDerived(options, from))
        {
            return false;
        }

        var texts = new DerivedTexts(transliterator, from, culture, _localePaths);
        // The source's addresses are first put under its own prefix, so moving them to the derived prefix finds them.
        version = WithOwnValues(entry, texts.Version(options.ForCulture(source, _localePaths)));
        return true;
    }

    private static bool IsDerived(OsirionSiteOptions options, string culture) =>
        options.Locales.Any(locale => string.Equals(locale.Key, culture, StringComparison.OrdinalIgnoreCase) && locale.Value?.DerivedFrom is not null);

    private static bool TryGetCulture(string name, out CultureInfo culture)
    {
        try
        {
            culture = CultureInfo.GetCultureInfo(name);
            return culture.Name.Length > 0;
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.InvariantCulture;
            return false;
        }
    }

    private static SiteLocaleOptions WithOwnValues(SiteLocaleOptions own, SiteLocaleOptions derived) => new()
    {
        Name = own.Name ?? derived.Name,
        LogoUrl = own.LogoUrl ?? derived.LogoUrl,
        HomeUrl = own.HomeUrl ?? derived.HomeUrl,
        Menu = own.Menu ?? derived.Menu,
        SearchUrl = own.SearchUrl ?? derived.SearchUrl,
        SearchPlaceholder = own.SearchPlaceholder ?? derived.SearchPlaceholder,
        CallToAction = own.CallToAction ?? derived.CallToAction,
        Announcement = own.Announcement ?? derived.Announcement,
        FooterDescription = own.FooterDescription ?? derived.FooterDescription,
        FooterSections = own.FooterSections ?? derived.FooterSections,
        SocialLinks = own.SocialLinks ?? derived.SocialLinks,
        LegalLinks = own.LegalLinks ?? derived.LegalLinks,
        Owner = own.Owner ?? derived.Owner,
        OwnerUrl = own.OwnerUrl ?? derived.OwnerUrl,
        Copyright = own.Copyright ?? derived.Copyright,
        SocialTitle = own.SocialTitle ?? derived.SocialTitle,
        Labels = own.Labels ?? derived.Labels,
        DerivedFrom = own.DerivedFrom,
        Transliteration = own.Transliteration,
    };

    /// <summary>Writes one source version in the derived culture: texts transliterated, page links moved.</summary>
    private sealed class DerivedTexts(ITextTransliterator transliterator, string from, string to, IOsirionLocalePaths? localePaths)
    {
        public SiteLocaleOptions Version(OsirionSiteOptions source) => new()
        {
            Name = Text(source.Name),
            LogoUrl = source.LogoUrl,
            HomeUrl = Link(source.HomeUrl),
            Menu = source.Menu.Select(MenuItem).ToList(),
            SearchUrl = Link(source.SearchUrl),
            SearchPlaceholder = Text(source.SearchPlaceholder),
            CallToAction = source.CallToAction is { } callToAction ? CallToAction(callToAction) : null,
            Announcement = source.Announcement is { } announcement ? Announcement(announcement) : null,
            FooterDescription = OptionalText(source.FooterDescription),
            FooterSections = source.FooterSections.Select(FooterSection).ToList(),
            SocialLinks = source.SocialLinks.Select(SocialLink).ToList(),
            LegalLinks = source.LegalLinks.Select(FooterLink).ToList(),
            Owner = OptionalText(source.Owner),
            OwnerUrl = source.OwnerUrl is null ? null : Link(source.OwnerUrl),
            Copyright = source.Copyright is null ? null : HtmlTransliteration.Transform(source.Copyright, transliterator, from, to, localePaths),
            SocialTitle = Text(source.SocialTitle),
            Labels = Labels(source.Labels),
        };

        private string Text(string text) => transliterator.Transliterate(text);

        private string? OptionalText(string? text) => text is null ? null : Text(text);

        private string Link(string address) => DerivedLocaleLinks.Move(address, from, to, localePaths);

        private string? OptionalLink(string? address) => address is null ? null : Link(address);

        private SiteMenuItem MenuItem(SiteMenuItem item) => new()
        {
            Text = Text(item.Text),
            Href = OptionalLink(item.Href),
            Description = OptionalText(item.Description),
            MatchPrefix = item.MatchPrefix,
            Items = item.Items.Select(MenuItem).ToList(),
        };

        // The footer section is named by its title, which is transliterated, so the name must be too.
        private SiteCallToAction CallToAction(SiteCallToAction callToAction) => new()
        {
            Text = Text(callToAction.Text),
            Href = Link(callToAction.Href),
            FooterSection = OptionalText(callToAction.FooterSection),
        };

        private SiteAnnouncement Announcement(SiteAnnouncement announcement) => new()
        {
            Id = announcement.Id,
            Text = Text(announcement.Text),
            LinkText = OptionalText(announcement.LinkText),
            LinkUrl = OptionalLink(announcement.LinkUrl),
        };

        private FooterSection FooterSection(FooterSection section) => new()
        {
            Title = OptionalText(section.Title),
            Links = section.Links.Select(FooterLink).ToList(),
        };

        private FooterLink FooterLink(FooterLink link) => new()
        {
            Text = Text(link.Text),
            Href = Link(link.Href),
            OpenInNewTab = link.OpenInNewTab,
        };

        private SiteSocialLink SocialLink(SiteSocialLink link) => new()
        {
            Network = link.Network,
            Href = link.Href,
            Label = OptionalText(link.Label),
        };

        private SiteLabels Labels(SiteLabels labels) => new()
        {
            SkipLink = Text(labels.SkipLink),
            Menu = Text(labels.Menu),
            MenuToggle = Text(labels.MenuToggle),
            SwitchToDarkTheme = Text(labels.SwitchToDarkTheme),
            SwitchToLightTheme = Text(labels.SwitchToLightTheme),
            Search = Text(labels.Search),
            SubmitSearch = Text(labels.SubmitSearch),
            LegalLinks = Text(labels.LegalLinks),
            Announcement = Text(labels.Announcement),
            DismissAnnouncement = Text(labels.DismissAnnouncement),
            Language = Text(labels.Language),
            Home = OptionalText(labels.Home),
        };
    }
}
