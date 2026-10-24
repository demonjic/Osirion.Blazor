using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

public class OsirionTestimonialCardTests : OsirionComponentTestBase
{
    [Fact]
    public void TestimonialCard_NameOnly_RendersCiteAndDefaultClassesWithoutOptionalParts()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters.Add(p => p.Name, "Ann"));

        cut.Find("cite.osirion-testimonial-name").TextContent.ShouldBe("Ann");
        var classes = cut.Find("div.osirion-testimonial-card").ClassList;
        classes.ShouldContain("osirion-testimonial-default");
        classes.ShouldContain("osirion-testimonial-normal");
        classes.ShouldContain("osirion-testimonial-theme-system");
        classes.ShouldContain("osirion-testimonial-elevated");
        classes.ShouldNotContain("osirion-testimonial-borderless");
        cut.FindAll(".osirion-testimonial-rating").ShouldBeEmpty();
        cut.FindAll("img").ShouldBeEmpty();
        cut.FindAll("a").ShouldBeEmpty();
        cut.FindAll(".osirion-testimonial-details").ShouldBeEmpty();
        cut.FindAll(".osirion-testimonial-additional").ShouldBeEmpty();
        cut.FindAll("footer").ShouldBeEmpty();
        cut.Find("blockquote.osirion-testimonial-quote").ChildElementCount.ShouldBe(0);
    }

    [Fact]
    public void TestimonialCard_VariantSizeThemeAndModifiers_AreReflectedInClasses()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Variant, TestimonialVariant.Highlighted)
            .Add(p => p.Size, TestimonialSize.Large)
            .Add(p => p.Theme, ThemeMode.Dark)
            .Add(p => p.Elevated, false)
            .Add(p => p.Borderless, true));

        var classes = cut.Find("div.osirion-testimonial-card").ClassList;
        classes.ShouldContain("osirion-testimonial-highlighted");
        classes.ShouldContain("osirion-testimonial-large");
        classes.ShouldContain("osirion-testimonial-theme-dark");
        classes.ShouldContain("osirion-testimonial-borderless");
        classes.ShouldNotContain("osirion-testimonial-elevated");
    }

    [Theory]
    [InlineData(3, 3, 2)]
    [InlineData(5, 5, 0)]
    [InlineData(9, 5, 0)]
    public void TestimonialCard_ShowRating_RendersFilledAndEmptyStarsForTheClampedRating(int rating, int filled, int empty)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.ShowRating, true)
            .Add(p => p.Rating, rating));

        var stars = cut.Find(".osirion-testimonial-rating");
        stars.GetAttribute("role").ShouldBe("img");
        stars.GetAttribute("aria-label").ShouldBe($"Rating: {filled} out of 5 stars");
        cut.FindAll(".osirion-testimonial-star-filled svg").Count.ShouldBe(filled);
        cut.FindAll(".osirion-testimonial-star-empty svg").Count.ShouldBe(empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-4)]
    public void TestimonialCard_ShowRatingWithZeroOrNegativeRating_RendersNoStars(int rating)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.ShowRating, true)
            .Add(p => p.Rating, rating));

        cut.FindAll(".osirion-testimonial-rating").ShouldBeEmpty();
    }

    [Fact]
    public void TestimonialCard_RatingWithoutShowRating_RendersNoStars()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Rating, 4));

        cut.FindAll(".osirion-testimonial-rating").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Dev", "Acme", "Profile photo of Ann, Dev at Acme")]
    [InlineData("Dev", null, "Profile photo of Ann, Dev")]
    [InlineData(null, "Acme", "Profile photo of Ann from Acme")]
    [InlineData(" ", " ", "Profile photo of Ann")]
    public void TestimonialCard_ProfileImage_AltTextDescribesPositionAndCompany(string? position, string? company, string expectedAlt)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Position, position)
            .Add(p => p.Company, company)
            .Add(p => p.ProfileImageUrl, "/ann.jpg"));

        var image = cut.Find("img.osirion-testimonial-image");
        image.GetAttribute("src").ShouldBe("/ann.jpg");
        image.GetAttribute("alt").ShouldBe(expectedAlt);
    }

    [Fact]
    public void TestimonialCard_PositionAndCompany_RendersBothWithSeparator()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Position, "Dev")
            .Add(p => p.Company, "Acme"));

        cut.Find(".osirion-testimonial-position").TextContent.ShouldBe("Dev");
        cut.Find(".osirion-testimonial-separator").TextContent.ShouldBe("at");
        cut.Find(".osirion-testimonial-company").TextContent.ShouldBe("Acme");
    }

    [Fact]
    public void TestimonialCard_PositionOnly_RendersPositionWithoutSeparator()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Position, "Dev"));

        cut.Find(".osirion-testimonial-position").TextContent.ShouldBe("Dev");
        cut.FindAll(".osirion-testimonial-separator").ShouldBeEmpty();
        cut.FindAll(".osirion-testimonial-company").ShouldBeEmpty();
    }

    [Fact]
    public void TestimonialCard_CompanyOnly_RendersCompanyWithoutSeparator()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Company, "Acme"));

        cut.Find(".osirion-testimonial-company").TextContent.ShouldBe("Acme");
        cut.FindAll(".osirion-testimonial-separator").ShouldBeEmpty();
        cut.FindAll(".osirion-testimonial-position").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(TestimonialSize.Small, 48)]
    [InlineData(TestimonialSize.Normal, 64)]
    [InlineData(TestimonialSize.Large, 80)]
    public void TestimonialCard_DefaultImageSize_FollowsCardSize(TestimonialSize size, int expected)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Size, size)
            .Add(p => p.ProfileImageUrl, "/ann.jpg"));

        var image = cut.Find("img");
        image.GetAttribute("width").ShouldBe(expected.ToString(System.Globalization.CultureInfo.InvariantCulture));
        image.GetAttribute("height").ShouldBe(expected.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void TestimonialCard_ExplicitImageSize_IsKeptRegardlessOfCardSize()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.Size, TestimonialSize.Small)
            .Add(p => p.ImageSize, 100)
            .Add(p => p.ProfileImageUrl, "/ann.jpg"));

        cut.Find("img").GetAttribute("width").ShouldBe("100");
    }

    [Theory]
    [InlineData(true, "eager", "high")]
    [InlineData(false, "lazy", "auto")]
    public void TestimonialCard_FirstVisible_ControlsImageLoadingPriority(bool firstVisible, string loading, string priority)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.IsFirstVisible, firstVisible)
            .Add(p => p.ProfileImageUrl, "/ann.jpg"));

        var image = cut.Find("img");
        image.GetAttribute("loading").ShouldBe(loading);
        image.GetAttribute("fetchpriority").ShouldBe(priority);
    }

    [Theory]
    [InlineData(false, "0")]
    [InlineData(true, "-1")]
    public void TestimonialCard_LinkedInUrl_RendersNameLinkFocusableUnlessHidden(bool hidden, string tabIndex)
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.LinkedInUrl, "https://linkedin.com/in/ann")
            .Add(p => p.IsHiddenFromAssistiveTech, hidden));

        var link = cut.Find("a.osirion-testimonial-name-link");
        link.GetAttribute("href").ShouldBe("https://linkedin.com/in/ann");
        link.GetAttribute("rel").ShouldBe("noopener noreferrer");
        link.GetAttribute("aria-label").ShouldBe("View Ann's LinkedIn profile");
        link.GetAttribute("tabindex").ShouldBe(tabIndex);
        link.QuerySelector("cite")!.TextContent.ShouldBe("Ann");
        link.QuerySelector("svg.bi-linkedin").ShouldNotBeNull();
    }

    [Fact]
    public void TestimonialCard_TestimonialText_RendersParagraphInQuote()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.TestimonialText, "Great product"));

        cut.Find("blockquote p.osirion-testimonial-text").TextContent.ShouldBe("Great product");
    }

    [Fact]
    public void TestimonialCard_TestimonialContent_OverridesTestimonialText()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.TestimonialText, "Plain text")
            .Add(p => p.TestimonialContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "strong");
                builder.AddContent(1, "Rich text");
                builder.CloseElement();
            })));

        cut.Find("blockquote strong").TextContent.ShouldBe("Rich text");
        cut.FindAll("blockquote p.osirion-testimonial-text").ShouldBeEmpty();
    }

    [Fact]
    public void TestimonialCard_ChildContent_RendersAdditionalSection()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "Verified buyer"))));

        cut.Find(".osirion-testimonial-additional").TextContent.Trim().ShouldBe("Verified buyer");
    }

    [Fact]
    public void TestimonialCard_ReadMoreHref_RendersFooterLink()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.ReadMoreHref, "/stories/ann")
            .Add(p => p.ReadMoreText, "Full story")
            .Add(p => p.ReadMoreTarget, "_blank"));

        var link = cut.Find("footer.osirion-testimonial-actions a");
        link.GetAttribute("href").ShouldBe("/stories/ann");
        link.GetAttribute("target").ShouldBe("_blank");
        link.TextContent.ShouldContain("Full story");
    }

    // Fails before the fix: the link carried a fixed style attribute with the action colors, which a Content Security
    // Policy without 'unsafe-inline' in style-src blocks. A class in the card's stylesheet sets them now.
    [Fact]
    public void TestimonialCard_ReadMoreLink_TakesItsColorsFromAClassInsteadOfAStyleAttribute()
    {
        var cut = RenderStatic<OsirionTestimonialCard>(parameters => parameters
            .Add(p => p.Name, "Ann")
            .Add(p => p.ReadMoreHref, "/stories/ann"));

        var link = cut.Find("footer.osirion-testimonial-actions a");
        link.HasAttribute("style").ShouldBeFalse();
        link.ClassList.ShouldContain("osirion-testimonial-read-more");
    }
}
