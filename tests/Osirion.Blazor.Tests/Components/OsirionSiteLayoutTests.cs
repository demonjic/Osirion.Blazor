using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Extensions;
using Osirion.Blazor.Options;
using Osirion.Blazor.Tests.Fakes;
using Shouldly;

namespace Osirion.Blazor.Tests.Components;

public class OsirionSiteLayoutTests : BunitContext
{
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
