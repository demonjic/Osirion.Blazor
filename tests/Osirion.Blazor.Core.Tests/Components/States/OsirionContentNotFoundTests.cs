using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.States;

public class OsirionContentNotFoundTests : OsirionComponentTestBase
{
    [Fact]
    public void ContentNotFound_Defaults_RendersCodeTitleMessageDefaultIconAndHomeButton()
    {
        var cut = RenderStatic<OsirionContentNotFound>(_ => { });

        cut.Find(".osirion-content-not-found-code").TextContent.ShouldBe("404");
        cut.Find("h1.osirion-content-not-found-title").TextContent.ShouldBe("Page Not Found");
        cut.Find(".osirion-content-not-found-message").TextContent
            .ShouldBe("The page you're looking for doesn't exist or has been moved.");
        cut.Find(".osirion-content-not-found-default-icon svg").ShouldNotBeNull();

        var actions = cut.FindAll(".osirion-content-not-found-actions a");
        actions.Count.ShouldBe(1);
        actions[0].TextContent.Trim().ShouldBe("Go to Homepage");
        actions[0].GetAttribute("href").ShouldBe("/");
    }

    // Fails before the fix: the title was always an h1, so an empty state inside a page added a second h1.
    [Theory]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    public void ContentNotFound_HeadingLevel_SetsTheTitleElement(int level, string tag)
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.Title, "Nothing here")
            .Add(p => p.HeadingLevel, level));

        cut.Find($"{tag}.osirion-content-not-found-title").TextContent.ShouldBe("Nothing here");
        cut.FindAll("h1").ShouldBeEmpty();
    }

    // Fails before the fix: there was no level to set; a value outside 1 to 6 now moves to the nearest level.
    [Theory]
    [InlineData(0, "h1")]
    [InlineData(-4, "h1")]
    [InlineData(7, "h6")]
    public void ContentNotFound_HeadingLevelOutsideTheRange_IsClamped(int level, string tag)
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters.Add(p => p.HeadingLevel, level));

        cut.FindAll($"{tag}.osirion-content-not-found-title").ShouldHaveSingleItem();
    }

    [Fact]
    public void ContentNotFound_Defaults_RendersDefaultClasses()
    {
        var cut = RenderStatic<OsirionContentNotFound>(_ => { });

        cut.Find("div").ClassList.ShouldBe(
        [
            "osirion-content-not-found",
            "osirion-content-not-found-default",
            "osirion-content-not-found-medium",
            "osirion-content-not-found-with-code"
        ]);
    }

    [Fact]
    public void ContentNotFound_VariantSizePatternAndClass_RendersMatchingClassesAndPattern()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ErrorCode, null)
            .Add(p => p.Variant, "hero")
            .Add(p => p.Size, "large")
            .Add(p => p.BackgroundPattern, BackgroundPatternType.Dots)
            .Add(p => p.Class, "not-found-page"));

        cut.Find("div").ClassList.ShouldBe(
        [
            "osirion-content-not-found",
            "osirion-content-not-found-hero",
            "osirion-content-not-found-large",
            "osirion-content-not-found-with-background",
            "not-found-page"
        ]);
        cut.Find(".osirion-bg-wrapper").ClassList.ShouldContain("osirion-content-not-found-background");
        cut.FindAll(".osirion-content-not-found-code").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_CustomIcon_WinsOverIconUrlAndDefaultIcon()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.Icon, Text("custom icon"))
            .Add(p => p.IconUrl, "/img/404.svg"));

        cut.Find(".osirion-content-not-found-icon").TextContent.Trim().ShouldBe("custom icon");
        cut.FindAll("img").ShouldBeEmpty();
        cut.FindAll(".osirion-content-not-found-default-icon").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_IconUrl_RendersImageWithAlt()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.IconUrl, "/img/404.svg")
            .Add(p => p.IconAlt, "Lost"));

        var image = cut.Find("img.osirion-content-not-found-image");
        image.GetAttribute("src").ShouldBe("/img/404.svg");
        image.GetAttribute("alt").ShouldBe("Lost");
        cut.FindAll(".osirion-content-not-found-default-icon").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_DefaultIconDisabled_RendersNoIcon()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.UseDefaultIcon, false));

        cut.FindAll(".osirion-content-not-found-icon").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_IconHidden_RendersNoIconEvenWhenProvided()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowIcon, false)
            .Add(p => p.Icon, Text("custom icon"))
            .Add(p => p.IconUrl, "/img/404.svg"));

        cut.FindAll(".osirion-content-not-found-icon").ShouldBeEmpty();
        cut.FindAll("img").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_EmptyMessageAndNoPrimaryButton_RendersNeither()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.Message, "")
            .Add(p => p.PrimaryButtonText, null));

        cut.FindAll(".osirion-content-not-found-message").ShouldBeEmpty();
        cut.FindAll(".osirion-content-not-found-actions a").ShouldBeEmpty();
    }

    // Fails before the fix: each button link carried style="text-decoration: none;", which a Content Security Policy
    // without 'unsafe-inline' in style-src blocks. A class in the component's stylesheet removes the underline now.
    [Fact]
    public void ContentNotFound_ButtonLinks_DropTheUnderlineWithAClassInsteadOfAStyleAttribute()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.PrimaryButtonText, "Home")
            .Add(p => p.PrimaryButtonUrl, "/")
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "/docs"));

        var buttons = cut.FindAll(".osirion-content-not-found-actions a");
        buttons.Count.ShouldBe(2);
        buttons.ShouldAllBe(button => !button.HasAttribute("style") && button.ClassList.Contains("osirion-content-not-found-button"));
    }

    // Fails before the fix: a javascript: button or suggestion URL was written to href unchecked.
    [Fact]
    public void ContentNotFound_UnsafeAddresses_RenderWithoutALink()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.PrimaryButtonText, "Home")
            .Add(p => p.PrimaryButtonUrl, "javascript:alert(1)")
            .Add(p => p.SecondaryButtonText, "Docs")
            .Add(p => p.SecondaryButtonUrl, "data:text/html,x")
            .Add(p => p.ShowSuggestions, true)
            .Add(p => p.Suggestions,
            [
                new ContentNotFoundSuggestion { Text = "Bad", Url = "javascript:alert(2)", Description = "Still shown" },
                new ContentNotFoundSuggestion { Text = "Good", Url = "/blog" }
            ]));

        cut.Markup.ShouldNotContain("javascript:");
        cut.Markup.ShouldNotContain("data:text");
        cut.FindAll(".osirion-content-not-found-actions a").ShouldBeEmpty();
        var links = cut.FindAll(".osirion-content-not-found-suggestion-link");
        links.Count.ShouldBe(1);
        links[0].GetAttribute("href").ShouldBe("/blog");
        cut.Markup.ShouldContain("Bad");
        cut.Markup.ShouldContain("Still shown");
    }

    [Fact]
    public void ContentNotFound_Suggestions_RendersEachLinkWithTargetAndDescription()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowSuggestions, true)
            .Add(p => p.SuggestionsTitle, "Try these:")
            .Add(p => p.Suggestions,
            [
                new ContentNotFoundSuggestion { Text = "Blog", Url = "/blog", Description = "Latest posts" },
                new ContentNotFoundSuggestion { Text = "GitHub", Url = "https://github.com", Target = "_blank" }
            ]));

        cut.Find(".osirion-content-not-found-suggestions-title").TextContent.ShouldBe("Try these:");
        var links = cut.FindAll(".osirion-content-not-found-suggestion-link");
        links.Select(a => (a.TextContent.Trim(), a.GetAttribute("href"), a.GetAttribute("target")))
            .ShouldBe([("Blog", "/blog", "_self"), ("GitHub", "https://github.com", "_blank")]);
        cut.FindAll(".osirion-content-not-found-suggestion-item small")[0].TextContent.Trim().ShouldBe("Latest posts");
    }

    // Fails before the fix: a suggestion without a description still rendered an empty p and small, and an empty
    // SuggestionsTitle rendered an empty p, which add stray spacing and empty elements for assistive technology.
    [Fact]
    public void ContentNotFound_EmptyTexts_RenderNoEmptyElements()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowSuggestions, true)
            .Add(p => p.SuggestionsTitle, "")
            .Add(p => p.Suggestions, [new ContentNotFoundSuggestion { Text = "Blog", Url = "/blog" }]));

        cut.FindAll(".osirion-content-not-found-suggestions-title").ShouldBeEmpty();
        cut.FindAll(".osirion-content-not-found-suggestion-item p").ShouldBeEmpty();
        cut.FindAll(".osirion-content-not-found-suggestion-item small").ShouldBeEmpty();
    }

    // Fails before the fix: Create403 pointed its button at javascript:history.back(), a script URL as an address.
    [Fact]
    public void ContentNotFound_Factories_UseOnlyLocalButtonAddresses()
    {
        OsirionContentNotFound[] configured =
        [
            OsirionContentNotFound.Create404(),
            OsirionContentNotFound.Create403(),
            OsirionContentNotFound.CreateEmptySearch("x"),
            OsirionContentNotFound.CreateEmptyContent()
        ];

        foreach (var component in configured)
        {
            component.PrimaryButtonUrl.ShouldStartWith("/");
            component.SecondaryButtonUrl.ShouldStartWith("/");
        }
    }

    [Fact]
    public void ContentNotFound_SuggestionsNotShown_RendersNoSuggestions()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.Suggestions, [new ContentNotFoundSuggestion { Text = "Blog", Url = "/blog" }]));

        cut.FindAll(".osirion-content-not-found-suggestions").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_EmptySuggestionList_RendersNoSuggestions()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowSuggestions, true)
            .Add(p => p.Suggestions, []));

        cut.FindAll(".osirion-content-not-found-suggestions").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ContentNotFound_SearchContent_RendersOnlyWhenSearchBoxIsShown(bool showSearchBox, int expected)
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowSearchBox, showSearchBox)
            .Add(p => p.SearchContent, Text("search")));

        cut.FindAll(".osirion-content-not-found-search").Count.ShouldBe(expected);
    }

    [Fact]
    public void ContentNotFound_ChildContentAndActions_RendersBoth()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .AddChildContent("<span class=\"custom\">Custom</span>")
            .Add(p => p.Actions, builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "extra-action");
                builder.AddContent(2, "Report");
                builder.CloseElement();
            }));

        cut.Find(".osirion-content-not-found-custom .custom").TextContent.ShouldBe("Custom");
        cut.Find(".osirion-content-not-found-actions button.extra-action").TextContent.ShouldBe("Report");
    }

    [Fact]
    public void ContentNotFound_SecondaryButton_RendersAfterThePrimaryButton()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.SecondaryButtonText, "Contact")
            .Add(p => p.SecondaryButtonUrl, "/contact"));

        var actions = cut.FindAll(".osirion-content-not-found-actions a");
        actions.Select(a => (a.TextContent.Trim(), a.GetAttribute("href")))
            .ShouldBe([("Go to Homepage", "/"), ("Contact", "/contact")]);
    }

    [Fact]
    public void ContentNotFound_ContactInfo_RendersMailtoLink()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowContactInfo, true)
            .Add(p => p.ContactMessage, "Write to")
            .Add(p => p.ContactEmail, "help@example.com"));

        var contact = cut.Find(".osirion-content-not-found-contact");
        contact.TextContent.ShouldContain("Write to");
        contact.QuerySelector("a")!.GetAttribute("href").ShouldBe("mailto:help@example.com");
    }

    [Fact]
    public void ContentNotFound_ContactInfoWithoutEmail_RendersNoContactBlock()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .Add(p => p.ShowContactInfo, true));

        cut.FindAll(".osirion-content-not-found-contact").ShouldBeEmpty();
    }

    [Fact]
    public void ContentNotFound_AdditionalAttributes_AreWrittenOnTheRoot()
    {
        var cut = RenderStatic<OsirionContentNotFound>(parameters => parameters
            .AddUnmatched("data-testid", "not-found"));

        cut.Find("div.osirion-content-not-found").GetAttribute("data-testid").ShouldBe("not-found");
    }

    [Fact]
    public void Create404_ReturnsNotFoundConfiguration()
    {
        var component = OsirionContentNotFound.Create404();

        component.ErrorCode.ShouldBe("404");
        component.Title.ShouldBe("Page Not Found");
        component.Message.ShouldBe("The page you're looking for doesn't exist or has been moved.");
        component.PrimaryButtonText.ShouldBe("Go to Homepage");
        component.PrimaryButtonUrl.ShouldBe("/");
    }

    [Fact]
    public void Create403_ReturnsAccessDeniedConfiguration()
    {
        var component = OsirionContentNotFound.Create403();

        component.ErrorCode.ShouldBe("403");
        component.Title.ShouldBe("Access Denied");
        component.Message.ShouldBe("You don't have permission to access this resource.");
        component.PrimaryButtonText.ShouldBe("Go to Homepage");
    }

    // Fails before the fix: the button was a javascript: URL, which a strict Content Security Policy blocks.
    [Fact]
    public void Create403_ButtonLeadsToTheHomePageInsteadOfAJavaScriptUrl()
    {
        var component = OsirionContentNotFound.Create403();

        component.PrimaryButtonUrl.ShouldBe("/");
    }

    [Fact]
    public void CreateEmptySearch_ReturnsNoResultsConfigurationQuotingTheTerm()
    {
        var component = OsirionContentNotFound.CreateEmptySearch("blazor");

        component.Title.ShouldBe("No Results Found");
        component.Message.ShouldBe(
            "We couldn't find any results for \"blazor\". Try adjusting your search or browse our categories.");
        component.ShowSearchBox.ShouldBeTrue();
        component.UseDefaultIcon.ShouldBeTrue();
    }

    [Fact]
    public void CreateEmptyContent_ReturnsNoContentConfiguration()
    {
        var component = OsirionContentNotFound.CreateEmptyContent();

        component.Title.ShouldBe("No Content Available");
        component.Message.ShouldBe("There's no content to display at the moment. Please check back later.");
        component.ShowIcon.ShouldBeTrue();
        component.UseDefaultIcon.ShouldBeTrue();
    }

    private static RenderFragment Text(string text) => builder => builder.AddContent(0, text);
}
