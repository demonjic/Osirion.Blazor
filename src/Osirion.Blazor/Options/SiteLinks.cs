using Osirion.Blazor.Components;

namespace Osirion.Blazor.Options;

/// <summary>
/// Merges the links of a language version with the site's. A link the version writes with an address keeps it as
/// written; a link it writes without one takes the address of the site's link at the same position (the same menu
/// entry, footer column and link); every address taken from the site goes through <c>localize</c>, which puts it under
/// the version's culture prefix when <c>Osirion:Localization</c> is registered. With no <c>localize</c> and nothing to
/// take, the version's own objects are returned, so options without these features render as they did.
/// </summary>
internal static class SiteLinks
{
    // A footer link written without Href has FooterLink's default, "#", which is never a page of the site.
    private const string UnsetFooterHref = "#";

    public static string Address(string address, Func<string, string>? localize) =>
        localize is null ? address : localize(address);

    public static List<SiteMenuItem> Menu(List<SiteMenuItem>? own, List<SiteMenuItem> site, Func<string, string>? localize)
    {
        if (own is null)
        {
            return localize is null ? site : site.Select(item => Localized(item, localize)).ToList();
        }

        var merged = own.Select((item, index) => Merge(item, index < site.Count ? site[index] : null, localize)).ToList();
        return merged.SequenceEqual(own) ? own : merged;
    }

    public static SiteCallToAction? CallToAction(SiteCallToAction? own, SiteCallToAction? site, Func<string, string>? localize)
    {
        if (own is null)
        {
            return site is null || localize is null
                ? site
                : new SiteCallToAction { Text = site.Text, Href = localize(site.Href), FooterSection = site.FooterSection };
        }

        return string.IsNullOrWhiteSpace(own.Href) && site is not null
            ? new SiteCallToAction { Text = own.Text, Href = Address(site.Href, localize), FooterSection = own.FooterSection }
            : own;
    }

    public static SiteAnnouncement? Announcement(SiteAnnouncement? own, SiteAnnouncement? site, Func<string, string>? localize)
    {
        var siteLink = site?.LinkUrl;
        if (own is null)
        {
            return site is null || siteLink is null || localize is null ? site : Copy(site, site.LinkText, localize(siteLink));
        }

        // A version that names the link's text but not its address links where the site's announcement does.
        return own.LinkUrl is null && own.LinkText is not null && siteLink is not null
            ? Copy(own, own.LinkText, Address(siteLink, localize))
            : own;
    }

    public static List<FooterSection> FooterSections(List<FooterSection>? own, List<FooterSection> site, Func<string, string>? localize)
    {
        if (own is null)
        {
            return localize is null
                ? site
                : site.Select(section => new FooterSection { Title = section.Title, Links = Links(null, section.Links, localize) }).ToList();
        }

        var merged = own.Select((section, index) =>
        {
            var links = Links(section.Links, index < site.Count ? site[index].Links : [], localize);
            return ReferenceEquals(links, section.Links) ? section : new FooterSection { Title = section.Title, Links = links };
        }).ToList();
        return merged.SequenceEqual(own) ? own : merged;
    }

    public static List<FooterLink> LegalLinks(List<FooterLink>? own, List<FooterLink> site, Func<string, string>? localize)
    {
        var links = Links(own, site, localize);
        return links as List<FooterLink> ?? links.ToList();
    }

    private static IReadOnlyList<FooterLink> Links(IReadOnlyList<FooterLink>? own, IReadOnlyList<FooterLink> site, Func<string, string>? localize)
    {
        if (own is null)
        {
            return localize is null ? site : site.Select(link => Copy(link, localize(link.Href))).ToList();
        }

        var merged = own.Select((link, index) =>
            IsUnset(link.Href) && index < site.Count ? Copy(link, Address(site[index].Href, localize)) : link).ToList();
        return merged.SequenceEqual(own) ? own : merged;
    }

    private static SiteMenuItem Merge(SiteMenuItem own, SiteMenuItem? site, Func<string, string>? localize)
    {
        var items = own.Items.Count > 0
            ? Menu(own.Items, site?.Items ?? [], localize)
            : own.Href is null && site is not null ? Menu(null, site.Items, localize) : own.Items;
        var href = own.Href ?? (site?.Href is { } siteHref ? Address(siteHref, localize) : null);

        return ReferenceEquals(items, own.Items) && href == own.Href
            ? own
            : new SiteMenuItem { Text = own.Text, Description = own.Description, MatchPrefix = own.MatchPrefix, Href = href, Items = items };
    }

    private static SiteMenuItem Localized(SiteMenuItem item, Func<string, string> localize) => new()
    {
        Text = item.Text,
        Description = item.Description,
        MatchPrefix = item.MatchPrefix,
        Href = item.Href is null ? null : localize(item.Href),
        Items = item.Items.Select(child => Localized(child, localize)).ToList(),
    };

    private static bool IsUnset(string? href) => string.IsNullOrWhiteSpace(href) || href == UnsetFooterHref;

    private static FooterLink Copy(FooterLink link, string href) =>
        new() { Text = link.Text, Href = href, OpenInNewTab = link.OpenInNewTab };

    private static SiteAnnouncement Copy(SiteAnnouncement announcement, string? linkText, string linkUrl) =>
        new() { Id = announcement.Id, Text = announcement.Text, LinkText = linkText, LinkUrl = linkUrl };
}
