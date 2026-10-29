using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Analytics.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Options;
using Osirion.Blazor.Tests.Fakes;
using Shouldly;

namespace Osirion.Blazor.Tests.Components;

public class OsirionSiteLayoutTests : BunitContext
{
    private const string UnsafeCallToActionHref =
        "Osirion:Site:CallToAction:Href must be a path on the site (starting with /), a fragment (#section) or an http, https, mailto or tel address.";

    private static readonly Dictionary<string, string?> Site = new()
    {
        ["Osirion:Site:Name"] = "Osirion.Blazor",
        ["Osirion:Site:LogoUrl"] = "/favicon.png",
        ["Osirion:Site:Menu:0:Text"] = "Docs",
        ["Osirion:Site:Menu:0:Href"] = "/docs",
        ["Osirion:Site:Menu:1:Text"] = "Guides",
        ["Osirion:Site:Menu:1:Items:0:Text"] = "Getting started",
        ["Osirion:Site:Menu:1:Items:0:Href"] = "/guides/start",
        ["Osirion:Site:Menu:2:Text"] = "Home",
        ["Osirion:Site:Menu:2:Href"] = "/",
        ["Osirion:Site:FooterSections:0:Title"] = "Project",
        ["Osirion:Site:FooterSections:0:Links:0:Text"] = "Contact",
        ["Osirion:Site:FooterSections:0:Links:0:Href"] = "/contact",
        ["Osirion:Site:SocialLinks:0:Network"] = "github",
        ["Osirion:Site:SocialLinks:0:Href"] = "https://github.com/obrana-boranija/Osirion.Blazor",
        ["Osirion:Site:SocialLinks:0:Label"] = "Osirion.Blazor on GitHub",
        ["Osirion:Site:SocialLinks:1:Network"] = "mastodon",
        ["Osirion:Site:SocialLinks:1:Href"] = "https://example.social/@osirion",
        ["Osirion:Site:LegalLinks:0:Text"] = "Privacy",
        ["Osirion:Site:LegalLinks:0:Href"] = "/privacy-policy",
    };

    [Fact]
    public void AddOsirion_SiteSection_BindsMenuFooterAndSocialLinks()
    {
        var site = BuildOptions(Site);

        site.Name.ShouldBe("Osirion.Blazor");
        site.Menu.Select(item => item.Text).ShouldBe(["Docs", "Guides", "Home"]);
        site.Menu[1].Items.ShouldHaveSingleItem().Href.ShouldBe("/guides/start");
        site.FooterSections.ShouldHaveSingleItem().Links.ShouldHaveSingleItem().Text.ShouldBe("Contact");
        site.SocialLinks.Select(link => link.Network).ShouldBe(["github", "mastodon"]);
        site.LegalLinks.ShouldHaveSingleItem().Href.ShouldBe("/privacy-policy");
    }

    [Fact]
    public void AddOsirion_SiteWithoutName_FailsNamingTheKey()
    {
        var exception = Should.Throw<OptionsValidationException>(() => BuildOptions(new() { ["Osirion:Site:ShowSearch"] = "true" }));

        exception.Failures.ShouldContain("Osirion:Site:Name is required.");
    }

    [Theory]
    [InlineData("Menu:0:Text", "Docs", "Osirion:Site:Menu:0 needs an Href or Items.")]
    [InlineData("Menu:0:Href", "/docs", "Osirion:Site:Menu:0 needs a Text.")]
    public void AddOsirion_IncompleteMenuItem_FailsNamingTheItem(string key, string value, string expected)
    {
        var exception = Should.Throw<OptionsValidationException>(() => BuildOptions(new()
        {
            ["Osirion:Site:Name"] = "Site",
            [$"Osirion:Site:{key}"] = value,
        }));

        exception.Failures.ShouldContain(expected);
    }

    [Fact]
    public void GetErrors_MenuNestedTwoLevels_IsRejected()
    {
        var site = new OsirionSiteOptions { Name = "Site" };
        site.Menu.Add(new SiteMenuItem
        {
            Text = "Guides",
            Items = [new SiteMenuItem { Text = "Content", Items = [new SiteMenuItem { Text = "SEO", Href = "/seo" }] }],
        });

        site.GetErrors().ShouldContain(error => error.StartsWith("Osirion:Site:Menu:0 nests deeper than one level", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("", "/contact", "Osirion:Site:CallToAction needs a Text.")]
    [InlineData(" ", "/contact", "Osirion:Site:CallToAction needs a Text.")]
    [InlineData("Book a call", "", "Osirion:Site:CallToAction needs an Href.")]
    [InlineData("Book a call", "javascript:alert(1)", UnsafeCallToActionHref)]
    [InlineData("Book a call", "data:text/html,hello", UnsafeCallToActionHref)]
    [InlineData("Book a call", "//cal.example/book", UnsafeCallToActionHref)]
    [InlineData("Book a call", "contact", UnsafeCallToActionHref)]
    public void GetErrors_IncompleteOrUnsafeCallToAction_IsRejectedNamingTheKey(string text, string href, string expected)
    {
        var site = new OsirionSiteOptions { Name = "Site", CallToAction = new SiteCallToAction { Text = text, Href = href } };

        site.GetErrors().ShouldBe([expected]);
    }

    // Guards the accepted addresses: a site path, https and mailto pass the check.
    [Theory]
    [InlineData("/contact")]
    [InlineData("https://cal.example/book")]
    [InlineData("mailto:hello@example.com")]
    public void GetErrors_CompleteCallToAction_IsAccepted(string href)
    {
        var site = new OsirionSiteOptions { Name = "Site", CallToAction = new SiteCallToAction { Text = "Book a call", Href = href } };

        site.GetErrors().ShouldBeEmpty();
    }

    [Fact]
    public void AddOsirion_CallToActionWithoutHref_FailsNamingTheKey()
    {
        var exception = Should.Throw<OptionsValidationException>(() => BuildOptions(new()
        {
            ["Osirion:Site:Name"] = "Site",
            ["Osirion:Site:CallToAction:Text"] = "Book a call",
        }));

        exception.Failures.ShouldContain("Osirion:Site:CallToAction needs an Href.");
    }

    [Fact]
    public void UseSite_Code_ConfiguresTheSite()
    {
        var services = HostServices.Create();
        services.AddOsirion(osirion => osirion.UseSite(site => site.Name = "In code"));

        services.BuildServiceProvider().GetRequiredService<IOptions<OsirionSiteOptions>>().Value.Name.ShouldBe("In code");
    }

    // Fails before the fix: OsirionPageLayout needs the JS component services, which only AddJSComponents registered.
    [Fact]
    public void AddOsirion_RegistersTheJsComponentServicesOnce()
    {
        var services = HostServices.Create();
        services.AddJSComponents();

        services.AddOsirion();

        services.Count(descriptor => descriptor.ServiceType == typeof(BlazorJSComponents.JSElementReferenceScope)).ShouldBe(1);
        HostServices.Create().AddOsirion().BuildServiceProvider().GetService<BlazorJSComponents.JSElementReferenceScope>().ShouldNotBeNull();
    }

    [Fact]
    public void OsirionSiteLayout_Options_RenderBrandMenuFooterAndBody()
    {
        var cut = RenderLayout(Site);

        cut.Find(".osirion-page-main").TextContent.ShouldContain("Page body");
        var menu = cut.Find("nav[aria-label='Main navigation']");
        menu.TextContent.ShouldContain("Osirion.Blazor");
        menu.QuerySelector("a[href='/docs']").ShouldNotBeNull();
        menu.QuerySelector("a[href='/guides/start']").ShouldNotBeNull();
        cut.Find("footer").TextContent.ShouldContain("Contact");
        cut.Find("footer a[href='/privacy-policy']").TextContent.Trim().ShouldBe("Privacy");
    }

    [Fact]
    public void OsirionSiteLayout_SocialLinks_KnownNetworkGetsAnIconOthersTheirLabel()
    {
        var cut = RenderLayout(Site);

        var github = cut.Find("a.osirion-footer-social-link[href='https://github.com/obrana-boranija/Osirion.Blazor']");
        github.GetAttribute("aria-label").ShouldBe("Osirion.Blazor on GitHub");
        github.QuerySelector("svg.osirion-site-social-icon")!.GetAttribute("aria-hidden").ShouldBe("true");
        cut.Find("a.osirion-footer-social-link[href='https://example.social/@osirion']").TextContent.Trim().ShouldBe("mastodon");
    }

    [Theory]
    [InlineData("facebook", "Facebook")]
    [InlineData("FaceBook", "Facebook")]
    [InlineData("instagram", "Instagram")]
    [InlineData("INSTAGRAM", "Instagram")]
    public void OsirionSiteLayout_FacebookOrInstagram_GetsAnIconNamedAfterTheNetwork(string network, string name)
    {
        var cut = RenderLayout(new Dictionary<string, string?>
        {
            ["Osirion:Site:Name"] = "Site",
            ["Osirion:Site:SocialLinks:0:Network"] = network,
            ["Osirion:Site:SocialLinks:0:Href"] = "https://example.com/profile",
        });

        var link = cut.Find("a.osirion-footer-social-link[href='https://example.com/profile']");
        link.GetAttribute("aria-label").ShouldBe(name);
        link.TextContent.Trim().ShouldBeEmpty();
        var icon = link.QuerySelector("svg.osirion-site-social-icon").ShouldNotBeNull();
        icon.GetAttribute("aria-hidden").ShouldBe("true");
        icon.QuerySelector("path")!.GetAttribute("d").ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void OsirionSiteLayout_Defaults_HaveNoSearchCookieBannerOrAnnouncement()
    {
        var cut = RenderLayout(Site);

        cut.FindAll("form[action='/search']").ShouldBeEmpty();
        cut.FindAll("[aria-label='Cookie Consent']").ShouldBeEmpty();
        cut.FindAll("[aria-label='Announcement']").ShouldBeEmpty();
    }

    // Guard test: passes before the browser mode exists; the configured banner stays the server form by default.
    [Fact]
    public void OsirionSiteLayout_ShowCookieConsent_RendersTheServerFormByDefault()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(Site) { ["Osirion:Site:ShowCookieConsent"] = "true" });

        cut.Find("form[action='/api/cookie-consent'] [aria-label='Cookie Consent']").ShouldNotBeNull();
        cut.FindAll("[data-osirion-consent-banner]").ShouldBeEmpty();
    }

    // One setting moves the banner and the trackers, so a static site cannot end up with only one of them in the browser.
    [Fact]
    public void OsirionSiteLayout_ConsentEvaluatedInTheBrowser_RendersTheBannerAndHoldsTheTrackers()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:ShowCookieConsent"] = "true",
            ["Osirion:Analytics:Consent:RequireConsent"] = "true",
            ["Osirion:Analytics:Consent:Evaluation"] = "Browser",
            ["Osirion:Analytics:GA4:MeasurementId"] = "G-TEST1234",
        });

        var banner = cut.Find("[data-osirion-consent-banner]");
        banner.HasAttribute("hidden").ShouldBeTrue();
        banner.QuerySelector("[aria-label='Cookie Consent']").ShouldNotBeNull();
        cut.FindAll("form[action='/api/cookie-consent']").ShouldBeEmpty();
        Render<GA4Tracker>().Find("script[src*='googletagmanager']").GetAttribute("type").ShouldBe("text/plain");
    }

    [Fact]
    public void OsirionSiteLayout_SearchAndAnnouncement_AreShownWhenConfigured()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:ShowSearch"] = "true",
            ["Osirion:Site:Announcement:Id"] = "launch",
            ["Osirion:Site:Announcement:Text"] = "Version 4.0 is out.",
            ["Osirion:Site:Announcement:LinkText"] = "Read the notes",
            ["Osirion:Site:Announcement:LinkUrl"] = "/blog/4-1",
        });

        cut.Find("form[action='/search']").ShouldNotBeNull();
        var announcement = cut.Find("[aria-label='Announcement']");
        announcement.TextContent.ShouldContain("Version 4.0 is out.");
        announcement.QuerySelector("a[href='/blog/4-1']")!.TextContent.ShouldBe("Read the notes");
    }

    // Fails before the fix: the layout passed neither value to the footer, so a site could not change them.
    [Fact]
    public void OsirionSiteLayout_CopyrightAndSocialTitle_ReachTheFooter()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Copyright"] = "&copy; Osirion contributors",
            ["Osirion:Site:SocialTitle"] = "Elsewhere",
        });

        cut.Find(".osirion-footer-copyright").TextContent.Trim().ShouldBe("© Osirion contributors");
        cut.FindAll(".osirion-footer-social .osirion-footer-section-title").ShouldHaveSingleItem().TextContent.ShouldBe("Elsewhere");
    }

    // Fails before the fix: the default line separated the owner and "All rights reserved" with an em dash.
    [Fact]
    public void OsirionSiteLayout_DefaultCopyright_NamesTheOwnerWithoutADash()
    {
        var cut = RenderLayout(Site);

        var copyright = cut.Find(".osirion-footer-copyright").TextContent;
        copyright.ShouldContain("Osirion.Blazor. All rights reserved.");
        copyright.ShouldNotContain("—");
        cut.Find(".osirion-footer-social .osirion-footer-section-title").TextContent.ShouldBe("Follow Us");
    }

    [Fact]
    public void OsirionSiteLayout_HeaderAndFooterSlots_ReplaceTheGeneratedOnes()
    {
        var cut = RenderLayout(Site, p => p
            .Add(c => c.Header, "<div class=\"my-header\">Mine</div>")
            .Add(c => c.Footer, "<div class=\"my-footer\">Mine</div>"));

        cut.Find(".my-header").ShouldNotBeNull();
        cut.Find(".my-footer").ShouldNotBeNull();
        cut.FindAll("nav[aria-label='Main navigation']").ShouldBeEmpty();
        cut.FindAll(".osirion-footer-social-link").ShouldBeEmpty();
    }

    // Guards the default: without CallToAction the menu actions keep their markup and the bar holds nothing more.
    [Fact]
    public void OsirionSiteLayout_WithoutCallToAction_RendersNoLinkAmongTheMenuActions()
    {
        var cut = RenderLayout(Site);

        cut.Find(".osirion-menu-actions").QuerySelectorAll("a").ShouldBeEmpty();
        cut.FindAll(".osirion-menu-pinned-actions").ShouldBeEmpty();
    }

    // Fails before the fix: the button sat in .osirion-menu, the menu's no-script drawer below its 1200px breakpoint, so
    // on a phone the site's main action was hidden behind the toggle. It is now in the menu's pinned actions, which stay
    // in the bar at every width, and it is rendered once.
    [Fact]
    public void OsirionSiteLayout_CallToAction_RendersOnePrimaryButtonLinkInTheBarOutsideTheDrawer()
    {
        var cut = RenderLayout(WithCallToAction("/contact"));

        var button = cut.Find("nav[aria-label='Main navigation'] > .osirion-menu-pinned-actions > a.osirion-site-call-to-action");
        button.TextContent.ShouldBe("Book a call");
        button.GetAttribute("href").ShouldBe("/contact");
        button.ClassName.ShouldBe("osirion-button osirion-button-primary osirion-site-call-to-action");
        cut.FindAll("nav a.osirion-site-call-to-action").ShouldHaveSingleItem();
        cut.Find(".osirion-menu").QuerySelectorAll("a.osirion-site-call-to-action").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionSiteLayout_CallToActionUnderBootstrap_GetsTheBootstrapPrimaryButtonClasses()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(WithCallToAction("/contact"))
        {
            ["Osirion:Theming:Framework"] = "Bootstrap",
        });

        cut.Find("a.osirion-site-call-to-action").ClassName.ShouldBe("btn btn-primary osirion-site-call-to-action");
    }

    [Theory]
    [InlineData("https://cal.example/book", "noopener")]
    [InlineData("http://cal.example/book", "noopener")]
    [InlineData("/contact", null)]
    [InlineData("mailto:hello@example.com", null)]
    public void OsirionSiteLayout_CallToAction_GetsNoopenerOnlyForAnHttpAddressAndNeverOpensANewTab(string href, string? rel)
    {
        var cut = RenderLayout(WithCallToAction(href));

        var button = cut.Find("a.osirion-site-call-to-action");
        button.GetAttribute("href").ShouldBe(href);
        button.GetAttribute("rel").ShouldBe(rel);
        button.HasAttribute("target").ShouldBeFalse();
    }

    // Fails before the fix: the layout wrote the options as they were, so a call to action set without the startup check
    // (options configured in code without UseSite) reached the page as a script URL or as a link without text.
    [Theory]
    [InlineData("Book a call", "javascript:alert(1)")]
    [InlineData("Book a call", "data:text/html,hello")]
    [InlineData(" ", "/contact")]
    public void OsirionSiteLayout_UncheckedCallToActionWithUnsafeHrefOrBlankText_RendersNoButton(string text, string href)
    {
        Services.Configure<OsirionSiteOptions>(site =>
        {
            site.Name = "Site";
            site.CallToAction = new SiteCallToAction { Text = text, Href = href };
        });

        var cut = RenderLayout([]);

        cut.FindAll("a.osirion-site-call-to-action").ShouldBeEmpty();
        cut.FindAll(".osirion-menu-pinned-actions").ShouldBeEmpty();
    }

    // The search box, theme toggle and MenuActions stay in the drawer; the call to action ends the bar after them, in
    // source and keyboard order as on screen.
    [Fact]
    public void OsirionSiteLayout_CallToAction_FollowsSearchThemeToggleAndMenuActions()
    {
        var cut = RenderLayout(
            new Dictionary<string, string?>(WithCallToAction("/contact")) { ["Osirion:Site:ShowSearch"] = "true" },
            p => p.Add(c => c.MenuActions, "<a class=\"my-action\" href=\"/subscribe\">Subscribe</a>"));

        var order = cut.Find("nav[aria-label='Main navigation']")
            .QuerySelectorAll("form[action='/search'], .theme-switcher, a.osirion-site-call-to-action, a.my-action")
            .Select(element => element.Matches("form") ? "search"
                : element.ClassList.Contains("theme-switcher") ? "theme toggle"
                : element.ClassList.Contains("my-action") ? "menu actions"
                : "call to action")
            .ToList();
        order.ShouldBe(["search", "theme toggle", "menu actions", "call to action"]);
        cut.Find(".osirion-menu-actions a.my-action").ShouldNotBeNull();
    }

    // Fails before the fix: a footer column could not list the call to action, so a site copied its address into
    // FooterSections and had to keep the two in step.
    [Fact]
    public void OsirionSiteLayout_CallToActionWithFooterSection_IsTheLastLinkOfThatSection()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(WithCallToAction("https://cal.example/book"))
        {
            ["Osirion:Site:CallToAction:FooterSection"] = "Project",
        });

        var links = cut.FindAll("footer .osirion-footer-section").ShouldHaveSingleItem().QuerySelectorAll("a.osirion-footer-link");
        links.Select(link => link.TextContent.Trim()).ShouldBe(["Contact", "Book a call"]);
        links[^1].GetAttribute("href").ShouldBe("https://cal.example/book");
    }

    // The options are a singleton: rendering must not add the link to them, or every render would add it once more.
    [Fact]
    public void OsirionSiteLayout_CallToActionWithFooterSection_LeavesTheOptionsUnchanged()
    {
        var values = new Dictionary<string, string?>(WithCallToAction("/book")) { ["Osirion:Site:CallToAction:FooterSection"] = "Project" };

        var cut = RenderLayout(values);
        cut.Render();

        Services.GetRequiredService<IOptions<OsirionSiteOptions>>().Value.FooterSections[0].Links.ShouldHaveSingleItem();
        cut.FindAll("footer a[href='/book']").ShouldHaveSingleItem();
    }

    // Guards the default: without FooterSection the footer lists only the configured links.
    [Fact]
    public void OsirionSiteLayout_CallToActionWithoutFooterSection_AddsNoFooterLink()
    {
        var cut = RenderLayout(WithCallToAction("/book"));

        cut.FindAll("footer a[href='/book']").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Projects")]
    [InlineData("project")]
    public void GetErrors_CallToActionFooterSectionNamingNoSection_IsRejectedNamingTheKey(string footerSection)
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            CallToAction = new SiteCallToAction { Text = "Book a call", Href = "/book", FooterSection = footerSection },
            FooterSections = [new FooterSection { Title = "Project", Links = [] }],
        };

        site.GetErrors().ShouldBe([$"Osirion:Site:CallToAction:FooterSection is \"{footerSection}\", which is not the Title of any of Osirion:Site:FooterSections."]);
    }

    [Fact]
    public void GetErrors_CallToActionFooterSectionNamingASection_IsAccepted()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            CallToAction = new SiteCallToAction { Text = "Book a call", Href = "/book", FooterSection = "Project" },
            FooterSections = [new FooterSection { Title = "Project", Links = [] }],
        };

        site.GetErrors().ShouldBeEmpty();
    }

    // Fails before the change: the shell's texts were English, and only the menu's name could be set.
    [Fact]
    public void OsirionSiteLayout_Labels_NameEveryPartOfTheShell()
    {
        var cut = RenderLayout(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:ShowSearch"] = "true",
            ["Osirion:Site:Announcement:Id"] = "launch",
            ["Osirion:Site:Announcement:Text"] = "Почињемо.",
            ["Osirion:Site:Labels:SkipLink"] = "Пређи на садржај",
            ["Osirion:Site:Labels:Menu"] = "Главни мени",
            ["Osirion:Site:Labels:MenuToggle"] = "Отвори мени",
            ["Osirion:Site:Labels:SwitchToDarkTheme"] = "Тамна тема",
            ["Osirion:Site:Labels:SwitchToLightTheme"] = "Светла тема",
            ["Osirion:Site:Labels:Search"] = "Претрага",
            ["Osirion:Site:Labels:SubmitSearch"] = "Покрени претрагу",
            ["Osirion:Site:Labels:LegalLinks"] = "Правне напомене",
            ["Osirion:Site:Labels:Announcement"] = "Обавештење",
            ["Osirion:Site:Labels:DismissAnnouncement"] = "Затвори обавештење",
        });

        cut.Find("a.osirion-skip-link").TextContent.ShouldBe("Пређи на садржај");
        cut.FindAll("nav[aria-label='Главни мени']").ShouldHaveSingleItem();
        cut.Find("input.osirion-navbar-toggler").GetAttribute("aria-label").ShouldBe("Отвори мени");
        var themeToggle = cut.Find("button.theme-toggle-btn");
        themeToggle.GetAttribute("data-switch-to-dark-label").ShouldBe("Тамна тема");
        themeToggle.GetAttribute("data-switch-to-light-label").ShouldBe("Светла тема");
        cut.Find("form[role='search'] label").TextContent.ShouldBe("Претрага");
        var submit = cut.Find("form[role='search'] button[type='submit']");
        submit.GetAttribute("aria-label").ShouldBe("Покрени претрагу");
        submit.TextContent.Trim().ShouldBe("Претрага");
        cut.Find("nav.osirion-footer-bottom-links").GetAttribute("aria-label").ShouldBe("Правне напомене");
        cut.Find("[role='region']").GetAttribute("aria-label").ShouldBe("Обавештење");
        cut.Find("button.osirion-announcement-dismiss").GetAttribute("aria-label").ShouldBe("Затвори обавештење");
    }

    [Fact]
    public void OsirionSiteLayout_MenuLabelParameter_WinsOverTheLabel()
    {
        var cut = RenderLayout(
            new Dictionary<string, string?>(Site) { ["Osirion:Site:Labels:Menu"] = "Главни мени" },
            parameters => parameters.Add(p => p.MenuLabel, "Site menu"));

        cut.FindAll("nav[aria-label='Site menu']").ShouldHaveSingleItem();
    }

    // The defaults of the labels are the components' own English texts, so a site that sets no label renders as before.
    [Fact]
    public void SiteLabels_Defaults_AreTheComponentsOwnDefaults()
    {
        var labels = new SiteLabels();

        labels.SkipLink.ShouldBe(new OsirionPageLayout().SkipLinkText);
        labels.MenuToggle.ShouldBe(new Navigation.Components.Menu().ToggleAriaLabel);
        labels.SwitchToDarkTheme.ShouldBe(new Theming.Components.ThemeToggle().SwitchToDarkLabel);
        labels.SwitchToLightTheme.ShouldBe(new Theming.Components.ThemeToggle().SwitchToLightLabel);
        labels.Search.ShouldBe(new Cms.Web.Components.SearchBox().Label);
        labels.Search.ShouldBe(new Cms.Web.Components.SearchBox().SearchButtonText);
        labels.SubmitSearch.ShouldBe(new Cms.Web.Components.SearchBox().SubmitLabel);
        labels.LegalLinks.ShouldBe(new OsirionFooter().BottomLinksLabel);
        labels.Announcement.ShouldBe(new OsirionAnnouncementBar().RegionLabel);
        labels.DismissAnnouncement.ShouldBe(new OsirionAnnouncementBar().DismissText);
        labels.Menu.ShouldBe("Main navigation");
    }

    [Fact]
    public void OsirionSiteLayout_UiCultureWithALocale_RendersThatVersion()
    {
        var values = new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Locales:sr-Latn-RS:Name"] = "Osirion latinicom",
            ["Osirion:Site:Locales:sr-Latn-RS:Menu:0:Text"] = "Dokumentacija",
            ["Osirion:Site:Locales:sr-Latn-RS:Menu:0:Href"] = "/sr-latn-rs/docs",
            ["Osirion:Site:Locales:sr-Latn-RS:Labels:SkipLink"] = "Pređi na sadržaj",
        };

        var cut = WithUiCulture("sr-Latn-RS", () => RenderLayout(values));

        cut.Find("a.osirion-skip-link").TextContent.ShouldBe("Pređi na sadržaj");
        var menu = cut.Find("nav[aria-label='Main navigation']");
        menu.TextContent.ShouldContain("Osirion latinicom");
        menu.QuerySelector("a[href='/sr-latn-rs/docs']").ShouldNotBeNull();
        menu.QuerySelector("a[href='/docs']").ShouldBeNull();
        // What the version leaves out is the site's.
        cut.Find("footer a[href='/privacy-policy']").TextContent.Trim().ShouldBe("Privacy");
    }

    [Fact]
    public void OsirionSiteLayout_UiCultureWithoutALocale_RendersTheSite()
    {
        var values = new Dictionary<string, string?>(Site) { ["Osirion:Site:Locales:sr-Latn-RS:Name"] = "Osirion latinicom" };

        var cut = WithUiCulture("de-DE", () => RenderLayout(values));

        cut.Find("nav[aria-label='Main navigation']").TextContent.ShouldContain("Osirion.Blazor");
        cut.Find("nav[aria-label='Main navigation'] a[href='/docs']").ShouldNotBeNull();
    }

    [Fact]
    public void ForCulture_CultureWithoutAnEntryForItOrItsParents_ReturnsTheSameOptions()
    {
        var site = new OsirionSiteOptions { Name = "Site", Locales = { ["sr-Latn-RS"] = new SiteLocaleOptions { Name = "Sajt" } } };

        site.ForCulture(CultureInfo.GetCultureInfo("de-DE")).ShouldBeSameAs(site);
        site.ForCulture(CultureInfo.InvariantCulture).ShouldBeSameAs(site);
    }

    [Fact]
    public void ForCulture_ParentCultureEntry_ServesTheChildCulture()
    {
        var site = new OsirionSiteOptions { Name = "Site", Locales = { ["sr"] = new SiteLocaleOptions { Name = "Сајт" } } };

        site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn-RS")).Name.ShouldBe("Сајт");
    }

    [Fact]
    public void ForCulture_MostSpecificEntry_Wins()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            Locales =
            {
                ["sr"] = new SiteLocaleOptions { Name = "Сајт" },
                ["sr-Latn-RS"] = new SiteLocaleOptions { Name = "Sajt" },
            },
        };

        site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn-RS")).Name.ShouldBe("Sajt");
    }

    // Options set in code may bring a dictionary that compares keys by letter case.
    [Fact]
    public void ForCulture_KeyInAnotherLetterCase_StillMatches()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            Locales = new Dictionary<string, SiteLocaleOptions> { ["sr-latn-rs"] = new() { Name = "Sajt" } },
        };

        site.ForCulture(CultureInfo.GetCultureInfo("sr-Latn-RS")).Name.ShouldBe("Sajt");
    }

    [Fact]
    public void ForCulture_TakesEveryValueTheEntrySetsAndLeavesTheOptionsUnchanged()
    {
        var entry = new SiteLocaleOptions();
        foreach (var property in typeof(SiteLocaleOptions).GetProperties())
        {
            property.SetValue(entry, property.PropertyType == typeof(string) ? $"locale {property.Name}" : Activator.CreateInstance(property.PropertyType));
        }

        var site = new OsirionSiteOptions { Name = "Site", Locales = { ["en"] = entry } };
        var english = site.ForCulture(CultureInfo.GetCultureInfo("en-GB"));

        foreach (var property in typeof(SiteLocaleOptions).GetProperties())
        {
            typeof(OsirionSiteOptions).GetProperty(property.Name)!.GetValue(english).ShouldBe(property.GetValue(entry), property.Name);
        }

        site.Name.ShouldBe("Site");
        english.Locales.ShouldBeEmpty();
    }

    // A new site option needs a decision: it differs per language version (and is in SiteLocaleOptions) or it does not.
    [Fact]
    public void SiteLocaleOptions_HoldEveryValueThatIsNotTheSameForEveryVersion()
    {
        string[] sameForEveryVersion = ["ShowSearch", "ShowThemeToggle", "ShowCookieConsent", "ShowScrollToTop", "Locales"];

        typeof(OsirionSiteOptions).GetProperties().Select(property => property.Name).Except(sameForEveryVersion)
            .ShouldBe(typeof(SiteLocaleOptions).GetProperties().Select(property => property.Name), ignoreOrder: true);
    }

    [Fact]
    public void AddOsirion_LocalesSection_BindsEachVersionInAnyLetterCase()
    {
        var site = BuildOptions(new Dictionary<string, string?>(Site)
        {
            ["Osirion:Site:Locales:en:Name"] = "Site in English",
            ["Osirion:Site:Locales:en:Labels:SkipLink"] = "Skip to the text",
        });

        site.Locales["EN"].Name.ShouldBe("Site in English");
        site.ForCulture(CultureInfo.GetCultureInfo("en-US")).Labels.SkipLink.ShouldBe("Skip to the text");
    }

    [Fact]
    public void GetErrors_LocaleKeyThatIsNotACultureName_IsRejectedNamingTheKey()
    {
        var site = new OsirionSiteOptions { Name = "Site", Locales = { ["not a culture!"] = new SiteLocaleOptions() } };

        site.GetErrors().ShouldBe(["Osirion:Site:Locales:not a culture! is not a culture name, such as sr-Latn-RS or en."]);
    }

    [Fact]
    public void GetErrors_ValuesALocaleSets_AreCheckedUnderTheLocaleKey()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            FooterSections = [new FooterSection { Title = "Contact", Links = [] }],
            Locales =
            {
                ["sr"] = new SiteLocaleOptions
                {
                    Name = " ",
                    Menu = [new SiteMenuItem { Text = "Документи" }],
                    CallToAction = new SiteCallToAction { Text = "Пишите нам", Href = "/kontakt", FooterSection = "Контакт" },
                },
            },
        };

        site.GetErrors().ShouldBe(
        [
            "Osirion:Site:Locales:sr:Name cannot be blank.",
            "Osirion:Site:Locales:sr:Menu:0 needs an Href or Items.",
            "Osirion:Site:Locales:sr:CallToAction:FooterSection is \"Контакт\", which is not the Title of any of Osirion:Site:Locales:sr:FooterSections.",
        ]);
    }

    [Fact]
    public void GetErrors_IncompleteAnnouncementOrSocialLink_IsRejectedUnderTheKeyThatSetsIt()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            Announcement = new SiteAnnouncement { Id = "launch" },
            SocialLinks = [new SiteSocialLink { Network = "github" }],
            Locales =
            {
                ["en"] = new SiteLocaleOptions
                {
                    Announcement = new SiteAnnouncement { Text = "We are live." },
                    SocialLinks = [new SiteSocialLink { Network = "x", Href = "https://x.com/site" }, new SiteSocialLink { Network = "rss" }],
                },
            },
        };

        site.GetErrors().ShouldBe(
        [
            "Osirion:Site:Announcement needs an Id and a Text.",
            "Osirion:Site:SocialLinks:0 needs an Href.",
            "Osirion:Site:Locales:en:Announcement needs an Id and a Text.",
            "Osirion:Site:Locales:en:SocialLinks:1 needs an Href.",
        ]);
    }

    [Fact]
    public void GetErrors_BlankLocaleKey_IsRejected()
    {
        var site = new OsirionSiteOptions { Name = "Site", Locales = { [" "] = new SiteLocaleOptions() } };

        site.GetErrors().ShouldBe(["Osirion:Site:Locales:  is not a culture name, such as sr-Latn-RS or en."]);
    }

    // Options set in code can hold an entry without values; it changes nothing and is no error.
    [Fact]
    public void LocaleWithoutValues_IsNoErrorAndChangesNothing()
    {
        var site = new OsirionSiteOptions { Name = "Site", Locales = { ["en"] = null! } };

        site.GetErrors().ShouldBeEmpty();
        site.ForCulture(CultureInfo.GetCultureInfo("en")).ShouldBeSameAs(site);
    }

    // A problem in a value the version inherits is the site's, reported once under the site's key.
    [Fact]
    public void GetErrors_ValueALocaleInherits_IsReportedOnceUnderTheSiteKey()
    {
        var site = new OsirionSiteOptions
        {
            Name = "Site",
            Menu = [new SiteMenuItem { Text = "Docs" }],
            Locales = { ["sr"] = new SiteLocaleOptions { Name = "Сајт" } },
        };

        site.GetErrors().ShouldBe(["Osirion:Site:Menu:0 needs an Href or Items."]);
    }

    // CultureInfo.CurrentUICulture is what UseRequestLocalization sets for a request; the render runs under it.
    private static T WithUiCulture<T>(string culture, Func<T> render)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            return render();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static Dictionary<string, string?> WithCallToAction(string href) => new(Site)
    {
        ["Osirion:Site:CallToAction:Text"] = "Book a call",
        ["Osirion:Site:CallToAction:Href"] = href,
    };

    private static OsirionSiteOptions BuildOptions(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = HostServices.Create(configuration);
        services.AddOsirion(configuration);
        return services.BuildServiceProvider().GetRequiredService<IOptions<OsirionSiteOptions>>().Value;
    }

    private IRenderedComponent<OsirionSiteLayout> RenderLayout(
        Dictionary<string, string?> values,
        Action<ComponentParameterCollectionBuilder<OsirionSiteLayout>>? configure = null)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        Services.AddSingleton<IConfiguration>(configuration);
        Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        Services.AddOsirion(configuration);
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Static", false));

        return Render<OsirionSiteLayout>(p =>
        {
            p.Add(c => c.Body, (RenderFragment)(builder => builder.AddContent(0, "Page body")));
            configure?.Invoke(p);
        });
    }
}
