using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class OsirionCallToActionSectionTests : OsirionComponentTestBase
{
    public static TheoryData<string?, string?> IncompleteOrUnsafeButtons => new()
    {
        { null, "/start" },
        { "  ", "/start" },
        { "Start", null },
        { "Start", "  " },
        { "Start", "javascript:alert(1)" },
        { "Start", "//evil.example" },
    };

    [Fact]
    public void OsirionCallToActionSection_AllParameters_RenderALabelledSectionWithHeadingTextContentAndButtons()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.Description, "Create a site in five minutes.")
            .Add(c => c.PrimaryButtonText, "Get started")
            .Add(c => c.PrimaryButtonUrl, "/docs/getting-started")
            .Add(c => c.SecondaryButtonText, "Write to us")
            .Add(c => c.SecondaryButtonUrl, "mailto:hello@example.com")
            .AddChildContent("<p class=\"note\">No account needed.</p>"));

        var section = cut.Find("section.osirion-call-to-action-section");
        var heading = section.QuerySelector("h2.osirion-call-to-action-title")!;
        heading.TextContent.ShouldBe("Ready to start?");
        heading.Id.ShouldNotBeNullOrWhiteSpace();
        section.GetAttribute("aria-labelledby").ShouldBe(heading.Id);
        section.QuerySelector("p.osirion-call-to-action-description")!.TextContent.ShouldBe("Create a site in five minutes.");
        section.QuerySelector(".osirion-call-to-action-content > p.note")!.TextContent.ShouldBe("No account needed.");
        section.Children.Select(child => child.ClassName).ShouldBe(
        [
            "osirion-call-to-action-title",
            "osirion-call-to-action-description",
            "osirion-call-to-action-content",
            "osirion-call-to-action-buttons",
        ]);

        var buttons = section.QuerySelectorAll(".osirion-call-to-action-buttons > a");
        buttons.Length.ShouldBe(2);
        buttons[0].TextContent.ShouldBe("Get started");
        buttons[0].GetAttribute("href").ShouldBe("/docs/getting-started");
        buttons[0].ClassList.ShouldBe(
            ["osirion-call-to-action-button", "osirion-call-to-action-button-primary", "osirion-button", "osirion-button-primary"],
            ignoreOrder: true);
        buttons[1].TextContent.ShouldBe("Write to us");
        buttons[1].GetAttribute("href").ShouldBe("mailto:hello@example.com");
        buttons[1].ClassList.ShouldBe(
            ["osirion-call-to-action-button", "osirion-call-to-action-button-secondary", "osirion-button", "osirion-button-secondary"],
            ignoreOrder: true);
    }

    [Fact]
    public void OsirionCallToActionSection_Bootstrap_ButtonsTakeTheFrameworksClasses()
    {
        Services.AddSingleton(Options.Create(new ThemingOptions { Framework = CssFramework.Bootstrap }));

        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.PrimaryButtonText, "Get started")
            .Add(c => c.PrimaryButtonUrl, "/start")
            .Add(c => c.SecondaryButtonText, "Pricing")
            .Add(c => c.SecondaryButtonUrl, "https://example.com/pricing"));

        var buttons = cut.FindAll("a.osirion-call-to-action-button");
        buttons[0].ClassList.ShouldContain("btn");
        buttons[0].ClassList.ShouldContain("btn-primary");
        buttons[1].ClassList.ShouldContain("btn");
        buttons[1].ClassList.ShouldContain("btn-secondary");
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    public void OsirionCallToActionSection_HeadingLevel_SetsTheHeadingElement(int level, string tag)
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.HeadingLevel, level));

        var heading = cut.Find($"{tag}.osirion-call-to-action-title");
        heading.TextContent.ShouldBe("Ready to start?");
        cut.Find("section").GetAttribute("aria-labelledby").ShouldBe(heading.Id);
        cut.FindAll("h1, h2, h3, h4, h5, h6").ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(0, "h1")]
    [InlineData(-3, "h1")]
    [InlineData(7, "h6")]
    [InlineData(int.MaxValue, "h6")]
    public void OsirionCallToActionSection_HeadingLevelOutsideTheRange_MovesToTheNearestLevel(int level, string tag)
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.HeadingLevel, level));

        cut.FindAll($"{tag}.osirion-call-to-action-title").ShouldHaveSingleItem();
    }

    [Fact]
    public void OsirionCallToActionSection_TwoSections_GetDifferentHeadingIds()
    {
        var first = RenderStatic<OsirionCallToActionSection>(p => p.Add(c => c.Title, "One")).Find("h2").Id;
        var second = RenderStatic<OsirionCallToActionSection>(p => p.Add(c => c.Title, "Two")).Find("h2").Id;

        first.ShouldNotBeNullOrWhiteSpace();
        second.ShouldNotBe(first);
    }

    [Theory]
    [MemberData(nameof(IncompleteOrUnsafeButtons))]
    public void OsirionCallToActionSection_IncompleteOrUnsafePrimaryButton_IsLeftOutAndTheSecondaryKept(string? text, string? url)
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.PrimaryButtonText, text)
            .Add(c => c.PrimaryButtonUrl, url)
            .Add(c => c.SecondaryButtonText, "Pricing")
            .Add(c => c.SecondaryButtonUrl, "/pricing"));

        var button = cut.FindAll("a").ShouldHaveSingleItem();
        button.ClassList.ShouldContain("osirion-call-to-action-button-secondary");
        button.GetAttribute("href").ShouldBe("/pricing");
    }

    [Theory]
    [MemberData(nameof(IncompleteOrUnsafeButtons))]
    public void OsirionCallToActionSection_IncompleteOrUnsafeSecondaryButton_IsLeftOutAndThePrimaryKept(string? text, string? url)
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.PrimaryButtonText, "Get started")
            .Add(c => c.PrimaryButtonUrl, "/start")
            .Add(c => c.SecondaryButtonText, text)
            .Add(c => c.SecondaryButtonUrl, url));

        var button = cut.FindAll("a").ShouldHaveSingleItem();
        button.ClassList.ShouldContain("osirion-call-to-action-button-primary");
        button.GetAttribute("href").ShouldBe("/start");
    }

    [Fact]
    public void OsirionCallToActionSection_NoButtons_RendersNoButtonRow()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.Description, "Create a site in five minutes."));

        cut.FindAll(".osirion-call-to-action-buttons").ShouldBeEmpty();
        cut.FindAll(".osirion-call-to-action-content").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionCallToActionSection_WithoutTitle_RendersNoHeadingAndNoLabelReference()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "  ")
            .Add(c => c.Description, "Create a site in five minutes."));

        cut.FindAll("h1, h2, h3, h4, h5, h6").ShouldBeEmpty();
        cut.Find("section").HasAttribute("aria-labelledby").ShouldBeFalse();
        cut.Find("p.osirion-call-to-action-description").TextContent.ShouldBe("Create a site in five minutes.");
    }

    [Fact]
    public void OsirionCallToActionSection_ChildContentOnly_RendersTheSectionWithTheContent()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p.AddChildContent("<span class=\"badge\">Free</span>"));

        cut.Find("section > .osirion-call-to-action-content > .badge").TextContent.ShouldBe("Free");
    }

    [Fact]
    public void OsirionCallToActionSection_TitleAndDescription_AreHtmlEncoded()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "<script>alert(1)</script> & more")
            .Add(c => c.Description, "<b>bold</b> <img src=x onerror=alert(1)>"));

        cut.FindAll("script, b, img").ShouldBeEmpty();
        cut.Find("h2").TextContent.ShouldBe("<script>alert(1)</script> & more");
        cut.Find("p").TextContent.ShouldBe("<b>bold</b> <img src=x onerror=alert(1)>");
        cut.Markup.ShouldContain("&lt;script&gt;");
    }

    [Theory]
    [InlineData(null, null, null, null)]
    [InlineData("", " ", "", "")]
    [InlineData(null, null, "Start", "javascript:alert(1)")]
    [InlineData(null, null, "Start", null)]
    [InlineData(null, null, null, "/start")]
    public void OsirionCallToActionSection_NothingToShow_RendersNothing(string? title, string? description, string? buttonText, string? buttonUrl)
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, title)
            .Add(c => c.Description, description)
            .Add(c => c.PrimaryButtonText, buttonText)
            .Add(c => c.PrimaryButtonUrl, buttonUrl)
            .Add(c => c.SecondaryButtonText, buttonText)
            .Add(c => c.SecondaryButtonUrl, buttonUrl));

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void OsirionCallToActionSection_ClassStyleAndAttributes_GoOnTheSection()
    {
        var cut = RenderStatic<OsirionCallToActionSection>(p => p
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.Class, "my-cta")
            .Add(c => c.Style, "margin-top: 4rem")
            .AddUnmatched("id", "get-started")
            .AddUnmatched("data-track", "closing-cta"));

        var section = cut.Find("section");
        section.ClassList.ShouldBe(["osirion-call-to-action-section", "my-cta"], ignoreOrder: true);
        section.GetAttribute("style").ShouldBe("margin-top: 4rem");
        section.Id.ShouldBe("get-started");
        section.GetAttribute("data-track").ShouldBe("closing-cta");
    }
}
