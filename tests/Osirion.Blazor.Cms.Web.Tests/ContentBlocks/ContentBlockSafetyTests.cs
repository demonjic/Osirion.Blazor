using Bunit;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.ContentBlocks;

/// <summary>Markup and links from content never reach the page unsanitized, and links are localized.</summary>
public class ContentBlockSafetyTests : ContentBlockTestBase
{
    [Theory]
    [InlineData("type: markdown\n    markdown: \"Hi <script>alert(1)</script> <img src=x onerror=alert(2)>\"")]
    [InlineData("type: section\n    title: T\n    markdown: \"<script>alert(1)</script>Hi\"")]
    [InlineData("type: faq\n    items: [{ question: Q, answer: \"<script>alert(1)</script>Hi\" }]")]
    [InlineData("type: section\n    title: \"<script>alert(1)</script>T\"\n    description: \"<script>alert(1)</script>Hi\"")]
    [InlineData("type: hero\n    title: T\n    subtitle: \"<script>alert(1)</script>Hi\"")]
    [InlineData("type: image\n    src: /a.png\n    alt: A\n    caption: \"<script>alert(1)</script>Hi\"")]
    public void ScriptInContent_NeverReachesTheOutput(string block)
    {
        var cut = RenderBlocks($"blocks:\n  - {block}\n");

        cut.Markup.ShouldNotContain("<script>alert");
        cut.Markup.ShouldNotContain("onerror");
        cut.FindAll("script").Where(script => script.GetAttribute("type") != "application/ld+json").ShouldBeEmpty();
    }

    [Fact]
    public void ScriptLinkInInlineMarkdown_IsRemoved()
    {
        var cut = RenderBlocks("blocks:\n  - { type: section, title: T, description: \"[click](javascript:alert(1))\" }\n");

        cut.Markup.ShouldNotContain("javascript:");
    }

    [Fact]
    public void ScriptInAFaqAnswer_StaysOutOfTheStructuredData()
    {
        var cut = RenderBlocks("blocks:\n  - { type: faq, items: [{ question: Q, answer: \"</script><script>alert(1)</script>\" }] }\n");

        cut.Find("script[type='application/ld+json']").TextContent.ShouldNotContain("<script>");
    }

    [Fact]
    public void SitePaths_AreLocalizedWhenLocalePathsAreRegistered()
    {
        UseSerbianLocalePaths();

        var cut = RenderBlocks("""
            blocks:
              - type: hero
                title: T
                primary_action: { text: Go, url: /contact }
                secondary_action: { text: Out, url: "https://example.com/x" }
              - type: hero
                title: U
                primary_action: { text: Down, url: "#pricing" }
              - type: section
                title: S
                description: "See [the platform](/platform) or [mail](mailto:a@example.com)."
                markdown: "Read [the docs](/docs#start)."
              - type: cards
                items: [{ title: C, url: /platform#govern }]
            """);

        var heroButtons = cut.FindAll("a.osirion-hero-button-primary");
        heroButtons[0].GetAttribute("href").ShouldBe("/sr/contact");
        heroButtons[1].GetAttribute("href").ShouldBe("#pricing");
        cut.Find("a.osirion-hero-button-secondary").GetAttribute("href").ShouldBe("https://example.com/x");
        var descriptionLinks = cut.FindAll("p.osirion-section-description a");
        descriptionLinks[0].GetAttribute("href").ShouldBe("/sr/platform");
        descriptionLinks[1].GetAttribute("href").ShouldBe("mailto:a@example.com");
        cut.Find(".osirion-section-content a").GetAttribute("href").ShouldBe("/sr/docs#start");
        cut.Find(".osirion-feature-card-content a").GetAttribute("href").ShouldBe("/sr/platform#govern");
    }

    [Fact]
    public void SitePaths_StayAsWrittenWithoutLocalePaths()
    {
        var cut = RenderBlocks("blocks:\n  - { type: hero, title: T, primary_action: { text: Go, url: /contact } }\n  - { type: section, title: S, description: \"[p](/platform)\" }\n");

        cut.Find("a.osirion-hero-button-primary").GetAttribute("href").ShouldBe("/contact");
        cut.Find("p.osirion-section-description a").GetAttribute("href").ShouldBe("/platform");
    }
}
