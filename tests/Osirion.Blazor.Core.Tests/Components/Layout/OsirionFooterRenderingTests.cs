using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Layout;

public class OsirionFooterRenderingTests : OsirionComponentTestBase
{
    [Fact]
    public void Footer_Defaults_RendersDefaultCopyrightWithCompanyLink()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.CompanyName, "Osirion")
            .Add(p => p.CompanyUrl, "/about"));

        var link = cut.Find(".osirion-footer-copyright a");
        link.TextContent.ShouldBe("Osirion");
        link.GetAttribute("href").ShouldBe("/about");
        link.GetAttribute("target").ShouldBe("_self");
        cut.FindAll(".osirion-footer-main").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_ExternalCompanyUrl_OpensInANewTab()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.CompanyUrl, "https://example.com"));

        cut.Find(".osirion-footer-copyright a").GetAttribute("target").ShouldBe("_blank");
    }

    // Fails before the fix: an external company link opened a new tab without rel, so the opened page got window.opener.
    [Fact]
    public void Footer_ExternalCompanyUrl_CannotReachTheOpener()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.CompanyUrl, "https://example.com"));

        cut.Find(".osirion-footer-copyright a").GetAttribute("rel").ShouldBe("noopener noreferrer");
    }

    [Fact]
    public void Footer_SameSiteCompanyUrl_HasNoRel()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.CompanyUrl, "/about"));

        cut.Find(".osirion-footer-copyright a").HasAttribute("rel").ShouldBeFalse();
    }

    [Fact]
    public void Footer_CustomCopyright_ReplacesTheDefaultCopyright()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Copyright, "<strong>Custom</strong> notice"));

        cut.Find(".osirion-footer-copyright strong").TextContent.ShouldBe("Custom");
        cut.FindAll(".osirion-footer-copyright a").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_CopyrightHidden_RendersNoCopyright()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.ShowCopyright, false));

        cut.FindAll(".osirion-footer-copyright").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_LogoAndDescription_RendersBrandBlock()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Logo, Text("Logo"))
            .Add(p => p.Description, "Built <em>fast</em>"));

        cut.Find(".osirion-footer-main-left-content").ClassList.ShouldContain("osirion-footer-main-left-content-has-child");
        cut.Find(".osirion-footer-logo").TextContent.Trim().ShouldBe("Logo");
        cut.Find(".osirion-footer-description em").TextContent.ShouldBe("fast");
    }

    [Fact]
    public void Footer_LeftAndRightContent_RendersBothColumns()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.LeftContent, Text("Left"))
            .Add(p => p.RightContent, Text("Right")));

        cut.Find(".osirion-footer-main-left-content").TextContent.Trim().ShouldBe("Left");
        cut.Find(".osirion-footer-main-left-content").ClassList.ShouldNotContain("osirion-footer-main-left-content-has-child");
        cut.Find(".osirion-footer-main-right-content").TextContent.Trim().ShouldBe("Right");
        cut.FindAll(".osirion-footer-brand").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_LinkThatOpensInNewTab_GetsTargetAndRel()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Links,
            [
                new FooterSection
                {
                    Links =
                    [
                        new FooterLink { Text = "Docs", Href = "https://docs.example.com", OpenInNewTab = true },
                        new FooterLink { Text = "Home", Href = "/" }
                    ]
                }
            ]));

        var links = cut.FindAll(".osirion-footer-link");
        links[0].GetAttribute("target").ShouldBe("_blank");
        links[0].GetAttribute("rel").ShouldBe("noopener noreferrer");
        links[1].HasAttribute("target").ShouldBeFalse();
        links[1].HasAttribute("rel").ShouldBeFalse();
        cut.FindAll(".osirion-footer-section-title").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_SocialLinkWithIcon_RendersIconInsteadOfText()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.SocialLinks,
            [
                new FooterSocialLink { Text = "GitHub", Href = "https://github.com", AriaLabel = "GitHub", Icon = Text("icon") }
            ])
            .Add(p => p.SocialTitle, ""));

        var link = cut.Find(".osirion-footer-social-link");
        link.TextContent.Trim().ShouldBe("icon");
        link.QuerySelector("span").ShouldBeNull();
        cut.FindAll(".osirion-footer-section-title").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_SocialLinksHidden_RendersNoSocialBlock()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.SocialLinks, [new FooterSocialLink { Text = "GitHub", Href = "https://github.com" }])
            .Add(p => p.ShowSocialLinks, false));

        cut.FindAll(".osirion-footer-social").ShouldBeEmpty();
    }

    // Fails before the fix: social links that were switched off still made the top row render, as an empty row.
    [Fact]
    public void Footer_OnlyHiddenSocialLinks_RendersNoTopRow()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.SocialLinks, [new FooterSocialLink { Text = "GitHub", Href = "https://github.com" }])
            .Add(p => p.ShowSocialLinks, false));

        cut.FindAll(".osirion-footer-main").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_HiddenSocialLinksWithALogo_StillRendersTheTopRow()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.SocialLinks, [new FooterSocialLink { Text = "GitHub", Href = "https://github.com" }])
            .Add(p => p.ShowSocialLinks, false)
            .Add(p => p.Logo, (RenderFragment)(builder => builder.AddContent(0, "Logo"))));

        cut.FindAll(".osirion-footer-main").Count.ShouldBe(1);
        cut.FindAll(".osirion-footer-social").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_TopSectionHidden_RendersNeitherMainNorDivider()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Logo, Text("Logo"))
            .Add(p => p.ShowTopSection, false));

        cut.FindAll(".osirion-footer-main").ShouldBeEmpty();
        cut.FindAll(".osirion-footer-divider").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_BottomSectionHidden_RendersNoBottomOrDivider()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.ShowBottomSection, false));

        cut.FindAll(".osirion-footer-bottom").ShouldBeEmpty();
        cut.FindAll(".osirion-footer-divider").ShouldBeEmpty();
    }

    [Fact]
    public void Footer_BottomLinks_RendersLegalNavigation()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.BottomLinks,
            [
                new FooterLink { Text = "Privacy", Href = "/privacy" },
                new FooterLink { Text = "Terms", Href = "/terms" }
            ]));

        var nav = cut.Find("nav.osirion-footer-bottom-links");
        nav.GetAttribute("aria-label").ShouldBe("Footer legal navigation");
        nav.QuerySelectorAll("a").Select(a => a.GetAttribute("href")).ShouldBe(["/privacy", "/terms"]);
    }

    // Fails before the change: the legal links were always named in English.
    [Fact]
    public void Footer_BottomLinksLabel_NamesTheLegalNavigation()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.BottomLinks, [new FooterLink { Text = "Приватност", Href = "/privatnost" }])
            .Add(p => p.BottomLinksLabel, "Правне напомене"));

        cut.Find("nav.osirion-footer-bottom-links").GetAttribute("aria-label").ShouldBe("Правне напомене");
    }

    [Fact]
    public void Footer_ChildContent_RendersExtraBlock()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .AddChildContent("<span class=\"extra\">Extra</span>"));

        cut.Find(".osirion-footer-extra .extra").TextContent.ShouldBe("Extra");
    }

    [Theory]
    [InlineData(ThemeMode.Light, "light")]
    [InlineData(ThemeMode.Dark, "dark")]
    [InlineData(ThemeMode.System, null)]
    [InlineData((ThemeMode)99, null)]
    public void Footer_Theme_SetsComponentThemeAttribute(ThemeMode theme, string? expected)
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Theme, theme));

        cut.Find("footer").GetAttribute("data-osirion-component-theme").ShouldBe(expected);
    }

    [Fact]
    public void Footer_DockedWithVariantGridAndClass_RendersMatchingClasses()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Variant, "compact")
            .Add(p => p.GridLayout, "three")
            .Add(p => p.Docked, true)
            .Add(p => p.DockingMode, "fixed")
            .Add(p => p.Class, "site-footer"));

        cut.Find("footer").ClassList.ShouldBe(
            ["osirion-footer", "osirion-footer-compact", "osirion-footer-grid-three", "osirion-footer-docked-fixed", "site-footer"]);
    }

    [Fact]
    public void Footer_Defaults_RendersDefaultVariantAndAutoGridWithoutDocking()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.DockingMode, "fixed"));

        cut.Find("footer").ClassList.ShouldBe(["osirion-footer", "osirion-footer-default", "osirion-footer-grid-auto"]);
    }

    private static RenderFragment Text(string text) => builder => builder.AddContent(0, text);
}
