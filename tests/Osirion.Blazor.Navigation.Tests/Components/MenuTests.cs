using Bunit;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Navigation.Components;
using Shouldly;

namespace Osirion.Blazor.Navigation.Tests.Components;

public class MenuTests : BunitContext
{
    public MenuTests()
    {
        SetRendererInfo(new RendererInfo("Server", false));

        // Set default JSInterop mode
        JSInterop.Mode = JSRuntimeMode.Loose;
    }


    [Fact]
    public void Menu_ShouldRenderVerticalMenu_WhenSpecified()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
                    .Add(p => p.Orientation, MenuOrientation.Vertical)
                    .AddChildContent("Menu Content"));

        // Assert
        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("osirion-menu-vertical");
        container.ClassList.ShouldContain("osirion-menu-auto-expand");
        container.ClassList.ShouldNotContain("osirion-menu-horizontal");
        container.ClassList.ShouldNotContain("osirion-menu-sticky");
        container.HasAttribute("style").ShouldBeFalse();
        container.Id.ShouldStartWith("osirion-menu-vertical-");
        cut.Find(".osirion-menu > .osirion-menu-inner").TextContent.Trim().ShouldBe("Menu Content");
    }

    [Fact]
    public void Menu_ShouldRenderHorizontalMenu_WhenSpecified()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
                    .Add(p => p.Orientation, MenuOrientation.Horizontal)
                    .AddChildContent("Menu Content"));

        // Assert
        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("osirion-menu-horizontal");
        container.ClassList.ShouldContain("osirion-menu-align-center");
        container.ClassList.ShouldContain("osirion-menu-collapsible");
        container.ClassList.ShouldNotContain("osirion-menu-vertical");
        container.Id.ShouldStartWith("osirion-menu-horizontal-");
        var toggler = cut.Find("input.osirion-navbar-toggler");
        cut.Find("label.osirion-navbar-toggle").GetAttribute("for").ShouldBe(toggler.Id);
        cut.Find(".osirion-menu > .osirion-menu-inner").TextContent.Trim().ShouldBe("Menu Content");
    }

    [Fact]
    public void Menu_ShouldRenderStickyMenu_WhenIsSticky()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Sticky, true)
            .Add(p => p.StickyZIndex, 999)
            .AddChildContent("Menu Content"));

        // Assert
        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("osirion-menu-sticky");
        container.GetAttribute("style").ShouldBe("z-index: 999;");
    }

    [Fact]
    public void Menu_ShouldRenderWithAriaLabel_WhenProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.AriaLabel, "Main Menu")
            .AddChildContent("Menu Content"));

        // Assert
        cut.Find("nav.osirion-menu-nav").GetAttribute("aria-label").ShouldBe("Main Menu");
    }

    [Fact]
    public void Menu_ShouldRenderWithoutCollapseClass_WhenCollapseOnMobileFalse()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.CollapseOnMobile, false)
            .AddChildContent("Menu Content"));

        // Assert
        cut.Find(".osirion-menu-container").ClassList.ShouldNotContain("osirion-menu-collapsible");
        cut.FindAll(".osirion-navbar-toggler").ShouldBeEmpty();
        cut.FindAll(".osirion-navbar-toggle").ShouldBeEmpty();
    }

    [Fact]
    public void Menu_ShouldRenderWithCustomClass_WhenProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Class, "custom-menu")
            .AddChildContent("Menu Content"));

        // Assert
        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("custom-menu");
        container.ClassList.ShouldContain("osirion-menu-horizontal");
    }

    [Fact]
    public void Menu_ShouldRenderWithAdditionalAttributes_WhenProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object>
            {
                { "data-testid", "main-menu" },
                { "aria-expanded", "true" }
            })
            .AddChildContent("Menu Content"));

        // Assert
        var container = cut.Find(".osirion-menu-container");
        container.GetAttribute("data-testid").ShouldBe("main-menu");
        container.GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Menu_ShouldGenerateUniqueIds_WhenTwoMenusShareAnOrientation()
    {
        // Act
        var first = Render<Menu>(parameters => parameters.AddChildContent("A"));
        var second = Render<Menu>(parameters => parameters.AddChildContent("B"));

        // Assert
        var firstContainer = first.Find(".osirion-menu-container");
        var secondContainer = second.Find(".osirion-menu-container");
        firstContainer.Id.ShouldNotBe(secondContainer.Id);

        var firstToggler = first.Find("input.osirion-navbar-toggler");
        var secondToggler = second.Find("input.osirion-navbar-toggler");
        firstToggler.Id.ShouldNotBe(secondToggler.Id);
        first.Find("label.osirion-navbar-toggle").GetAttribute("for").ShouldBe(firstToggler.Id);
        second.Find("label.osirion-navbar-toggle").GetAttribute("for").ShouldBe(secondToggler.Id);
    }

    [Fact]
    public void Menu_ShouldKeepTheSameIds_AcrossRenders()
    {
        var cut = Render<Menu>(parameters => parameters.AddChildContent("A"));
        var id = cut.Find(".osirion-menu-container").Id;
        var togglerId = cut.Find("input.osirion-navbar-toggler").Id;

        cut.Render(parameters => parameters.Add(p => p.AriaLabel, "Other label"));

        cut.Find(".osirion-menu-container").Id.ShouldBe(id);
        cut.Find("input.osirion-navbar-toggler").Id.ShouldBe(togglerId);
    }

    [Fact]
    public void Menu_ShouldHonourAnExplicitId()
    {
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Id, "main-menu")
            .AddChildContent("A"));

        cut.Find(".osirion-menu-container").Id.ShouldBe("main-menu");
        cut.Find("input.osirion-navbar-toggler").Id.ShouldBe("main-menu-toggle");
    }

    [Fact]
    public void Menu_ShouldRenderAKeyboardOperableToggle()
    {
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.ToggleAriaLabel, "Open the menu")
            .AddChildContent("A"));

        var toggler = cut.Find("input.osirion-navbar-toggler");
        toggler.GetAttribute("type").ShouldBe("checkbox");
        toggler.HasAttribute("aria-hidden").ShouldBeFalse();
        toggler.HasAttribute("tabindex").ShouldBeFalse();
        toggler.GetAttribute("aria-label").ShouldBe("Open the menu");
        toggler.GetAttribute("aria-controls").ShouldBe(cut.Find(".osirion-menu").Id);
    }

    // Fails before the fix: the checkbox came first in the container, so at phone width the keyboard reached the
    // toggle before the brand. The drawer follows the toggle, so the next Tab after opening it lands on the items.
    [Fact]
    public void Menu_Collapsible_PutsTheBrandBeforeTheToggleAndTheDrawerRightAfterIt()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.BrandText, "Site")
            .AddChildContent("<a href=\"/docs\">Docs</a>"));

        // Assert
        var order = cut.Find(".osirion-menu-container")
            .QuerySelectorAll("a.osirion-brand-link, input.osirion-navbar-toggler, .osirion-menu a")
            .Select(element => element.Matches("input") ? "toggle" : element.Matches(".osirion-brand-link") ? "brand" : "item")
            .ToList();
        order.ShouldBe(["brand", "toggle", "item"]);
        cut.Find("input.osirion-navbar-toggler").NextElementSibling!.Matches("label.osirion-navbar-toggle").ShouldBeTrue();
    }

    [Fact]
    public void Menu_ShouldKeepClassesAndStyle_WhenTheParentRendersAgain()
    {
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Class, "custom-menu")
            .Add(p => p.StickyZIndex, 999)
            .AddChildContent("A"));

        cut.Render(parameters => parameters
            .Add(p => p.Class, "custom-menu")
            .Add(p => p.StickyZIndex, 999)
            .Add(p => p.AriaLabel, "Changed"));

        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("osirion-menu-horizontal");
        container.ClassList.ShouldContain("osirion-menu-sticky");
        container.ClassList.ShouldContain("custom-menu");
        container.ClassList.Count(c => c == "osirion-menu-horizontal").ShouldBe(1);
        container.GetAttribute("style").ShouldBe("z-index: 999;");
        container.Id.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Menu_ShouldReflectParameterChanges_InClassesAndStyle()
    {
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Orientation, MenuOrientation.Horizontal)
            .AddChildContent("A"));
        cut.Find(".osirion-menu-container").ClassList.ShouldContain("osirion-menu-sticky");

        cut.Render(parameters => parameters
            .Add(p => p.Orientation, MenuOrientation.Vertical)
            .Add(p => p.Sticky, false));

        var container = cut.Find(".osirion-menu-container");
        container.ClassList.ShouldContain("osirion-menu-vertical");
        container.ClassList.ShouldNotContain("osirion-menu-horizontal");
        container.ClassList.ShouldNotContain("osirion-menu-sticky");
        container.HasAttribute("style").ShouldBeFalse();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,x")]
    public void Menu_ShouldNotRenderBrandHref_WhenSchemeIsUnsafe(string href)
    {
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.Href, href)
            .Add(p => p.BrandText, "Brand")
            .AddChildContent("A"));

        cut.Find("a.osirion-brand-link").HasAttribute("href").ShouldBeFalse();
    }

    [Theory]
    [InlineData(MenuCollapseBreakpoint.Mobile, "osirion-menu-collapse-mobile")]
    [InlineData(MenuCollapseBreakpoint.Large, "osirion-menu-collapse-large")]
    [InlineData(MenuCollapseBreakpoint.ExtraLarge, "osirion-menu-collapse-extra-large")]
    public void Menu_ShouldMarkItsCollapseBreakpoint_WhenItCollapses(MenuCollapseBreakpoint breakpoint, string expectedClass)
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.CollapseBreakpoint, breakpoint)
            .AddChildContent("Menu Content"));

        // Assert
        var classes = cut.Find(".osirion-menu-container").ClassList;
        classes.ShouldContain("osirion-menu-collapsible");
        classes.ShouldContain(expectedClass);
        classes.Count(c => c.StartsWith("osirion-menu-collapse-", StringComparison.Ordinal)).ShouldBe(1);
    }

    [Fact]
    public void Menu_ShouldNotMarkACollapseBreakpoint_WhenCollapseOnMobileFalse()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .Add(p => p.CollapseOnMobile, false)
            .Add(p => p.CollapseBreakpoint, MenuCollapseBreakpoint.Large)
            .AddChildContent("Menu Content"));

        // Assert
        cut.Find(".osirion-menu-container").ClassList
            .ShouldNotContain(c => c.StartsWith("osirion-menu-collapse-", StringComparison.Ordinal));
        cut.FindAll(".osirion-menu-backdrop").ShouldBeEmpty();
    }

    [Fact]
    public void Menu_ShouldRenderABackdropThatClosesTheDrawer_WhenCollapsible()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters.AddChildContent("Menu Content"));

        // Assert
        var toggler = cut.Find("input.osirion-navbar-toggler");
        var backdrop = cut.Find("label.osirion-menu-backdrop");
        backdrop.GetAttribute("for").ShouldBe(toggler.Id);
        backdrop.GetAttribute("aria-hidden").ShouldBe("true");
    }

    [Fact]
    public void Menu_ShouldRenderActionsAfterTheItems_WhenProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .AddChildContent("Menu Content")
            .Add(p => p.Actions, "<button type=\"button\">Search</button>"));

        // Assert
        var menu = cut.Find(".osirion-menu");
        menu.Children.Length.ShouldBe(2);
        menu.Children[0].ClassList.ShouldContain("osirion-menu-inner");
        menu.Children[1].ClassList.ShouldContain("osirion-menu-actions");
        menu.Children[1].QuerySelector("button")!.TextContent.ShouldBe("Search");
    }

    [Fact]
    public void Menu_ShouldRenderNoActionsContainer_WhenActionsAreNotProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters.AddChildContent("Menu Content"));

        // Assert
        cut.FindAll(".osirion-menu-actions").ShouldBeEmpty();
    }

    // Fails before the fix: the menu had no place for content outside its drawer, so below the collapse breakpoint a
    // site's main action was hidden behind the toggle.
    [Fact]
    public void Menu_PinnedActions_RenderOnceInTheBarAfterTheDrawer()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .AddChildContent("Menu Content")
            .Add(p => p.Actions, "<button type=\"button\">Search</button>")
            .Add(p => p.PinnedActions, "<a class=\"book\" href=\"/contact\">Book a call</a>"));

        // Assert
        var pinned = cut.Find("nav.osirion-menu-nav > .osirion-menu-pinned-actions");
        pinned.PreviousElementSibling!.ClassList.ShouldContain("osirion-menu");
        pinned.QuerySelector("a.book")!.TextContent.ShouldBe("Book a call");
        cut.FindAll("a.book").Count.ShouldBe(1);
        cut.Find(".osirion-menu").QuerySelectorAll("a.book").ShouldBeEmpty();
    }

    // Fails before the fix: nothing told the stylesheet that pinned actions follow the toggle, so the collapsed bar
    // drew the toggle at its far edge, after them, while the keyboard reached it first.
    [Fact]
    public void Menu_PinnedActions_MarkTheContainerSoTheCollapsedBarDrawsTheToggleFirst()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .AddChildContent("Menu Content")
            .Add(p => p.PinnedActions, "<a class=\"book\" href=\"/contact\">Book a call</a>"));

        // Assert
        cut.Find(".osirion-menu-container").ClassList.ShouldContain("osirion-menu-has-pinned-actions");
    }

    [Fact]
    public void Menu_ShouldRenderNoPinnedActionsContainer_WhenPinnedActionsAreNotProvided()
    {
        // Act
        var cut = Render<Menu>(parameters => parameters
            .AddChildContent("Menu Content")
            .Add(p => p.Actions, "<button type=\"button\">Search</button>"));

        // Assert
        cut.FindAll(".osirion-menu-pinned-actions").ShouldBeEmpty();
        cut.Find(".osirion-menu-container").ClassList.ShouldNotContain("osirion-menu-has-pinned-actions");
    }
}
