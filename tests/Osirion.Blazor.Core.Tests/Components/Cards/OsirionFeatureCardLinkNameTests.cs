using AngleSharp.Dom;
using Bunit;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Cards;

/// <summary>
/// Every link a feature card renders has an accessible name (axe <c>link-name</c>, WCAG 2.4.4 and 4.1.2), and a card
/// links to a place once.
/// </summary>
public class OsirionFeatureCardLinkNameTests : OsirionComponentTestBase
{
    public static TheoryData<string, Action<ComponentParameterCollectionBuilder<OsirionFeatureCard>>> Cards => new()
    {
        { "text only with a url", p => p.Add(c => c.Title, "Fast").Add(c => c.Url, "/fast") },
        { "text only with read more", p => p.Add(c => c.Title, "Fast").Add(c => c.Url, "/fast").Add(c => c.ShowReadMoreLink, true) },
        { "no title with read more", p => p.Add(c => c.Url, "/fast").Add(c => c.ShowReadMoreLink, true).Add(c => c.ReadMoreText, " ") },
        { "image with a url", p => p.Add(c => c.Title, "Fast").Add(c => c.Url, "/fast").Add(c => c.ImageUrl, "/a.png") },
        { "image with an empty alt and a url", p => p.Add(c => c.Url, "/fast").Add(c => c.ImageUrl, "/a.png").Add(c => c.ImageAlt, "") },
        { "image with read more", p => p.Add(c => c.Title, "Fast").Add(c => c.Url, "/fast").Add(c => c.ImageUrl, "/a.png").Add(c => c.ShowReadMoreLink, true) },
        { "icon with a url", p => p.Add(c => c.Title, "Fast").Add(c => c.Url, "/fast").Add(c => c.Icon, b => b.AddMarkupContent(0, "<svg></svg>")) },
        {
            "pillow buttons",
            p => p.Add(c => c.Title, "Fast").Add(c => c.ShowPillowButtons, true).Add(c => c.PillowButtons,
            [
                new PillowButton("Docs", "/docs"),
                new PillowButton(string.Empty, "/empty"),
                new PillowButton { Href = "/labelled", AriaLabel = "Labelled" },
            ])
        },
    };

    // The check axe's link-name rule makes: every link with an href has text or an aria-label.
    [Theory]
    [MemberData(nameof(Cards))]
    public void FeatureCard_EveryLinkHasAnAccessibleName(string scenario, Action<ComponentParameterCollectionBuilder<OsirionFeatureCard>> parameters)
    {
        var cut = RenderStatic(parameters);

        foreach (var link in cut.FindAll("a[href]"))
        {
            HasAccessibleName(link).ShouldBeTrue($"{scenario}: {link.OuterHtml}");
        }
    }

    [Fact]
    public void FeatureCard_ImageAndReadMoreToTheSamePlace_LinksOnce()
    {
        var cut = RenderStatic(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Url, "/fast")
            .Add(c => c.ImageUrl, "/a.png")
            .Add(c => c.ShowReadMoreLink, true));

        cut.FindAll("a[href='/fast']").Count.ShouldBe(1);
        cut.Find("img").GetAttribute("src").ShouldBe("/a.png");
    }

    [Fact]
    public void FeatureCard_ImageAndReadMoreToDifferentPlaces_KeepsBothLinks()
    {
        var cut = RenderStatic(p => p
            .Add(c => c.Title, "Fast")
            .Add(c => c.Url, "/fast")
            .Add(c => c.ReadMoreHref, "/fast/details")
            .Add(c => c.ImageUrl, "/a.png")
            .Add(c => c.ShowReadMoreLink, true));

        cut.FindAll("a.osirion-feature-card-image-link[href='/fast']").Count.ShouldBe(1);
        cut.FindAll("a[href='/fast/details']").Count.ShouldBe(1);
    }

    [Fact]
    public void FeatureCard_DecorativeImageWithAUrl_IsNotALink()
    {
        var cut = RenderStatic(p => p.Add(c => c.Url, "/fast").Add(c => c.ImageUrl, "/a.png").Add(c => c.ImageAlt, ""));

        cut.FindAll("a").ShouldBeEmpty();
        cut.Find("img").GetAttribute("alt").ShouldBe(string.Empty);
    }

    [Fact]
    public void FeatureCard_PillowButtonWithoutTextOrLabel_IsLeftOut()
    {
        var cut = RenderStatic(p => p.Add(c => c.ShowPillowButtons, true).Add(c => c.PillowButtons,
            [new PillowButton("Docs", "/docs"), new PillowButton(" ", "/empty")]));

        cut.FindAll(".osirion-feature-card-pillow").Select(link => link.GetAttribute("href")).ShouldBe(["/docs"]);
    }

    // Guard tests: a card with an image keeps the markup it rendered before 4.4.1 (captured from 4.4.0).
    [Fact]
    public void FeatureCard_ImageWithoutUrl_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic(p => p.Add(c => c.Title, "Fast").Add(c => c.Description, "D").Add(c => c.ImageUrl, "/a.png"));

        StableMarkup.Of(cut.Markup).ShouldBe(ImageWithoutUrlMarkup);
    }

    [Fact]
    public void FeatureCard_ImageWithUrl_RendersTheMarkupItAlwaysDid()
    {
        var cut = RenderStatic(p => p.Add(c => c.Title, "Fast").Add(c => c.Description, "D").Add(c => c.ImageUrl, "/a.png").Add(c => c.Url, "/fast"));

        StableMarkup.Of(cut.Markup).ShouldBe(ImageWithUrlMarkup);
    }

    private IRenderedComponent<OsirionFeatureCard> RenderStatic(Action<ComponentParameterCollectionBuilder<OsirionFeatureCard>> parameters) =>
        RenderStatic<OsirionFeatureCard>(parameters);

    private static bool HasAccessibleName(IElement link) =>
        !string.IsNullOrWhiteSpace(link.TextContent)
        || !string.IsNullOrWhiteSpace(link.GetAttribute("aria-label"))
        || link.QuerySelectorAll("img[alt]").Any(image => !string.IsNullOrWhiteSpace(image.GetAttribute("alt")));

    private const string ImageWithoutUrlMarkup =
        """<div class="osirion-feature-card osirion-feature-card-image-left osirion-feature-card-md osirion-feature-card-content-center-left osirion-feature-card-font-md osirion-feature-card-border-always osirion-feature-card-shadow-hover osirion-feature-card-transform-hover"><div class="osirion-feature-card-image-container"><img src="/a.png" alt="Fast" class="osirion-feature-card-image" loading="lazy" onerror="this.onerror=null;this.src='https://placehold.co/450x450/1a1a1a/ffffff?text=No+Image';" /></div><div class="osirion-feature-card-content"><p class="osirion-feature-card-title">Fast</p><p class="osirion-feature-card-description">D</p></div></div>""";

    private const string ImageWithUrlMarkup =
        """<div class="osirion-feature-card osirion-feature-card-image-left osirion-feature-card-md osirion-feature-card-content-center-left osirion-feature-card-font-md osirion-feature-card-border-always osirion-feature-card-shadow-hover osirion-feature-card-transform-hover"><a href="/fast" class="osirion-feature-card-image-link"><div class="osirion-feature-card-image-container"><img src="/a.png" alt="Fast" class="osirion-feature-card-image" loading="lazy" onerror="this.onerror=null;this.src='https://placehold.co/450x450/1a1a1a/ffffff?text=No+Image';" /></div></a><div class="osirion-feature-card-content"><p class="osirion-feature-card-title">Fast</p><p class="osirion-feature-card-description">D</p></div></div>""";
}
