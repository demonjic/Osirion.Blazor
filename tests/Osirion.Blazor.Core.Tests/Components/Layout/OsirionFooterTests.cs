using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Layout;

public class OsirionFooterTests : OsirionComponentTestBase
{
    // Fails before the fix: section titles were h3, so on a page whose last heading is the h1 the footer skipped a
    // level (axe heading-order). An h2 never skips one, whatever the page above it holds.
    [Fact]
    public void Footer_SectionAndSocialTitles_AreLevelTwoHeadings()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Links, [new FooterSection { Title = "Site", Links = [new FooterLink { Text = "About", Href = "/about" }] }])
            .Add(p => p.SocialLinks, [new FooterSocialLink { Text = "GitHub", Href = "https://github.com", AriaLabel = "GitHub" }])
            .Add(p => p.SocialTitle, "Elsewhere"));

        cut.FindAll(".osirion-footer-section-title").Select(h => (h.TagName, h.TextContent.Trim()))
            .ShouldBe([("H2", "Site"), ("H2", "Elsewhere")]);
    }

    // Fails before the fix: the footer wrote a <style> element on every page and style="bottom: 0;" on its brand, which
    // a Content Security Policy without 'unsafe-inline' in style-src blocks. Both now come from its stylesheet.
    [Fact]
    public void Footer_WithBrandAndBottomLinks_RendersNoInlineStyle()
    {
        var cut = RenderStatic<OsirionFooter>(parameters => parameters
            .Add(p => p.Description, "A site")
            .Add(p => p.Links, [new FooterSection { Title = "Site", Links = [new FooterLink { Text = "About", Href = "/about" }] }])
            .Add(p => p.BottomLinks, [new FooterLink { Text = "Privacy", Href = "/privacy" }]));

        cut.FindAll(".osirion-footer-brand").Count.ShouldBe(1);
        cut.FindAll(".osirion-footer-bottom-links").Count.ShouldBe(1);
        cut.FindAll("style").ShouldBeEmpty();
        cut.FindAll("[style]").ShouldBeEmpty();
    }
}
