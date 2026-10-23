using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Sections;

public class OsirionPricingTests : OsirionComponentTestBase
{
    private static readonly PricingPlan[] Plans =
    [
        new() { Name = "Starter", Price = "Free", Features = ["1 site"], CallToActionText = "Get started", CallToActionUrl = "/start" },
        new()
        {
            Name = "Team", Price = "$29", Period = "per month", Description = "For small teams", Features = ["5 sites", "Analytics"],
            CallToActionText = "Start trial", CallToActionUrl = "https://example.com/trial", Highlighted = true, Badge = "Most popular",
        },
    ];

    public OsirionPricingTests() => SetRendererInfo(new RendererInfo("Static", false));

    [Fact]
    public void OsirionPricingTable_Plans_RenderAsAListWithPricesFeaturesAndActions()
    {
        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, Plans));

        var plans = cut.FindAll("li.osirion-pricing-plan");
        plans.Count.ShouldBe(2);
        plans[0].QuerySelector(".osirion-pricing-name")!.TextContent.ShouldBe("Starter");
        plans[0].QuerySelector(".osirion-pricing-amount")!.TextContent.ShouldBe("Free");
        plans[0].QuerySelector(".osirion-pricing-period").ShouldBeNull();
        plans[1].QuerySelector(".osirion-pricing-period")!.TextContent.ShouldBe("per month");
        plans[1].QuerySelectorAll(".osirion-pricing-features li").Select(li => li.TextContent).ShouldBe(["5 sites", "Analytics"]);
        plans[1].QuerySelector(".osirion-pricing-badge")!.TextContent.ShouldBe("Most popular");
        plans[1].ClassList.ShouldContain("osirion-pricing-plan-highlighted");
        var action = plans[1].QuerySelector("a.osirion-pricing-action")!;
        action.GetAttribute("href").ShouldBe("https://example.com/trial");
        action.GetAttribute("aria-label").ShouldBe("Start trial: Team");
    }

    [Fact]
    public void OsirionPricingTable_HighlightedPlan_GetsThePrimaryButtonAndOthersTheSecondary()
    {
        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, Plans));

        var actions = cut.FindAll("a.osirion-pricing-action");
        actions[0].ClassList.ShouldContain("osirion-button-secondary");
        actions[1].ClassList.ShouldContain("osirion-button-primary");
    }

    [Fact]
    public void OsirionPricingTable_UnsafeActionUrl_LeavesTheButtonOut()
    {
        PricingPlan[] plans = [new() { Name = "X", Price = "$1", CallToActionText = "Buy", CallToActionUrl = "javascript:alert(1)" }];

        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, plans));

        cut.FindAll("a").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionPricingTable_NoPlans_RendersNothing()
    {
        Render<OsirionPricingTable>(p => p.Add(t => t.Plans, [])).Markup.Trim().ShouldBeEmpty();
    }

    // Guard test: plans with a price and no PlanHeadingLevel keep the markup they had before either option existed.
    [Fact]
    public void OsirionPricingTable_PricesWithoutPlanHeadingLevel_KeepTheirMarkup()
    {
        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, Plans));

        cut.MarkupMatches(
            """
            <ul class="osirion-pricing-table" role="list">
              <li class="osirion-pricing-plan">
                <p class="osirion-pricing-name">Starter</p>
                <p class="osirion-pricing-price"><span class="osirion-pricing-amount">Free</span></p>
                <ul class="osirion-pricing-features"><li>1 site</li></ul>
                <a class="osirion-pricing-action osirion-button osirion-button-secondary" href="/start" aria-label="Get started: Starter">Get started</a>
              </li>
              <li class="osirion-pricing-plan osirion-pricing-plan-highlighted">
                <p class="osirion-pricing-badge">Most popular</p>
                <p class="osirion-pricing-name">Team</p>
                <p class="osirion-pricing-price">
                  <span class="osirion-pricing-amount">$29</span>
                  <span class="osirion-pricing-period">per month</span>
                </p>
                <p class="osirion-pricing-description">For small teams</p>
                <ul class="osirion-pricing-features"><li>5 sites</li><li>Analytics</li></ul>
                <a class="osirion-pricing-action osirion-button osirion-button-primary" href="https://example.com/trial" aria-label="Start trial: Team">Start trial</a>
              </li>
            </ul>
            """);
    }

    [Fact]
    public void OsirionPricingTable_PlanWithoutPrice_LeavesTheAmountAndPeriodOut()
    {
        var plan = new PricingPlan
        {
            Name = "Enterprise",
            Period = "per year",
            Description = "Quoted on request",
            Features = ["Unlimited sites"],
            CallToActionText = "Contact sales",
            CallToActionUrl = "/contact",
        };

        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, [plan]));

        plan.Price.ShouldBeEmpty();
        var card = cut.Find("li.osirion-pricing-plan");
        card.QuerySelector(".osirion-pricing-price").ShouldBeNull();
        card.QuerySelector(".osirion-pricing-amount").ShouldBeNull();
        card.QuerySelector(".osirion-pricing-period").ShouldBeNull();
        card.QuerySelector(".osirion-pricing-name")!.TextContent.ShouldBe("Enterprise");
        card.QuerySelector(".osirion-pricing-description")!.TextContent.ShouldBe("Quoted on request");
        card.QuerySelector("a.osirion-pricing-action")!.GetAttribute("href").ShouldBe("/contact");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void OsirionPricingTable_BlankPrice_LeavesTheAmountAndPeriodOut(string price)
    {
        PricingPlan[] plans = [new() { Name = "Enterprise", Price = price, Period = "per year" }];

        var cut = Render<OsirionPricingTable>(p => p.Add(t => t.Plans, plans));

        cut.FindAll(".osirion-pricing-price").ShouldBeEmpty();
        cut.FindAll(".osirion-pricing-period").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(2, "h2")]
    [InlineData(3, "h3")]
    [InlineData(4, "h4")]
    [InlineData(5, "h5")]
    [InlineData(6, "h6")]
    public void OsirionPricingTable_PlanHeadingLevel_RendersEachNameAsThatHeading(int level, string tag)
    {
        var cut = Render<OsirionPricingTable>(p => p
            .Add(t => t.Plans, Plans)
            .Add(t => t.PlanHeadingLevel, level));

        cut.FindAll($"li.osirion-pricing-plan > {tag}.osirion-pricing-name").Select(h => h.TextContent).ShouldBe(["Starter", "Team"]);
        cut.FindAll("h1, h2, h3, h4, h5, h6").Count.ShouldBe(2);
        cut.FindAll("p.osirion-pricing-name").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0, "h1")]
    [InlineData(-2, "h1")]
    [InlineData(7, "h6")]
    public void OsirionPricingTable_PlanHeadingLevelOutsideTheRange_IsMovedToTheNearestLevel(int level, string tag)
    {
        var cut = Render<OsirionPricingTable>(p => p
            .Add(t => t.Plans, Plans)
            .Add(t => t.PlanHeadingLevel, level));

        cut.FindAll($"{tag}.osirion-pricing-name").Count.ShouldBe(2);
    }

    [Fact]
    public void OsirionFeatureComparison_Rows_RenderMarksTextAndHeaders()
    {
        FeatureComparisonRow[] rows =
        [
            new("Static SSR", [true, true]),
            new("Single sign-on", [false, "Add-on"]),
            new("Short row", [true]),
        ];

        var cut = Render<OsirionFeatureComparison>(p => p
            .Add(t => t.PlanNames, ["Starter", "Team"])
            .Add(t => t.Rows, rows));

        cut.FindAll("thead th[scope='col']").Select(th => th.TextContent).ShouldBe(["Feature", "Starter", "Team"]);
        cut.FindAll("tbody th[scope='row']").Select(th => th.TextContent).ShouldBe(["Static SSR", "Single sign-on", "Short row"]);
        var body = cut.FindAll("tbody tr");
        body[0].QuerySelectorAll(".osirion-feature-comparison-yes").Length.ShouldBe(2);
        body[0].QuerySelectorAll("td .osirion-visually-hidden")[0].TextContent.ShouldBe("Included");
        body[1].QuerySelector("td .osirion-visually-hidden")!.TextContent.ShouldBe("Not included");
        body[1].QuerySelectorAll("td")[1].TextContent.Trim().ShouldBe("Add-on");
        body[2].QuerySelectorAll("td").Length.ShouldBe(2);
        body[2].QuerySelectorAll("td")[1].TextContent.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void OsirionFeatureComparison_ScrollRegion_IsFocusableAndNamedByTheCaption()
    {
        var cut = Render<OsirionFeatureComparison>(p => p
            .Add(t => t.PlanNames, ["A"])
            .Add(t => t.Rows, [new FeatureComparisonRow("F", [true])])
            .Add(t => t.Caption, "Plan features")
            .Add(t => t.ShowCaption, true));

        var region = cut.Find("div.osirion-feature-comparison");
        region.GetAttribute("role").ShouldBe("region");
        region.GetAttribute("tabindex").ShouldBe("0");
        region.GetAttribute("aria-label").ShouldBe("Plan features");
        cut.Find("caption").ClassList.ShouldContain("osirion-feature-comparison-caption");
    }
}
