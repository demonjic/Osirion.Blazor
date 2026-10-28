using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Osirion.Blazor.Components;
using Osirion.Blazor.Theming.Components;
using Osirion.Blazor.Theming.Services;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Components;

public class ThemeToggleStateTests : BunitContext
{
    private readonly IThemeService _themeService = Substitute.For<IThemeService>();

    public ThemeToggleStateTests()
    {
        _themeService.CurrentMode.Returns(ThemeMode.Light);
        Services.AddSingleton(Options.Create(new ThemingOptions
        {
            DefaultMode = ThemeMode.Dark,
            Framework = CssFramework.FluentUI
        }));
        Services.AddSingleton(_themeService);
        SetRendererInfo(new RendererInfo("Static", false));
        ComponentFactories.AddStub<BlazorJSComponents.JS>();
    }

    [Fact]
    public void ThemeToggle_LightMode_OffersDarkModeWithMoonIcon()
    {
        var cut = Render<ThemeToggle>();

        var button = cut.Find("button");
        button.GetAttribute("title").ShouldBe("Switch to dark mode");
        button.GetAttribute("aria-label").ShouldBe("Switch to dark mode");
        cut.FindAll("svg.bi-moon").Count.ShouldBe(1);
        cut.FindAll("svg.bi-sun").ShouldBeEmpty();
        cut.Find(".theme-text").TextContent.ShouldBe("Switch to dark mode");
    }

    // Fails before the change: the name, the tooltip and the hidden text were English sentences built in code.
    [Theory]
    [InlineData(ThemeMode.Light, "Пређи на тамну тему")]
    [InlineData(ThemeMode.Dark, "Пређи на светлу тему")]
    public void ThemeToggle_Labels_NameTheButtonInTheLanguageOfThePage(ThemeMode mode, string expected)
    {
        _themeService.CurrentMode.Returns(mode);

        var cut = Render<ThemeToggle>(parameters => parameters
            .Add(p => p.SwitchToDarkLabel, "Пређи на тамну тему")
            .Add(p => p.SwitchToLightLabel, "Пређи на светлу тему"));

        var button = cut.Find("button");
        button.GetAttribute("aria-label").ShouldBe(expected);
        button.GetAttribute("title").ShouldBe(expected);
        cut.Find(".theme-text").TextContent.ShouldBe(expected);
    }

    // The script switches the theme in the browser and names the button anew from these, without the server.
    [Fact]
    public void ThemeToggle_Button_CarriesBothLabelsForTheScript()
    {
        var cut = Render<ThemeToggle>(parameters => parameters
            .Add(p => p.SwitchToDarkLabel, "Tamna tema")
            .Add(p => p.SwitchToLightLabel, "Svetla tema"));

        var button = cut.Find("button.theme-toggle-btn");
        button.GetAttribute("data-switch-to-dark-label").ShouldBe("Tamna tema");
        button.GetAttribute("data-switch-to-light-label").ShouldBe("Svetla tema");
    }

    [Fact]
    public void ThemeToggle_DarkMode_OffersLightModeWithSunIcon()
    {
        _themeService.CurrentMode.Returns(ThemeMode.Dark);

        var cut = Render<ThemeToggle>();

        var button = cut.Find("button");
        button.GetAttribute("title").ShouldBe("Switch to light mode");
        button.GetAttribute("aria-label").ShouldBe("Switch to light mode");
        cut.FindAll("svg.bi-sun").Count.ShouldBe(1);
        cut.Find(".theme-text").TextContent.ShouldBe("Switch to light mode");
    }

    [Fact]
    public void ThemeToggle_SystemMode_IsShownAsLight()
    {
        _themeService.CurrentMode.Returns(ThemeMode.System);

        var cut = Render<ThemeToggle>();

        cut.FindAll("svg.bi-moon").Count.ShouldBe(1);
    }

    [Fact]
    public void ThemeToggle_Button_CarriesDefaultThemeAndFramework()
    {
        var cut = Render<ThemeToggle>();

        var button = cut.Find("button.theme-toggle-btn");
        button.GetAttribute("data-default-theme").ShouldBe("dark");
        button.GetAttribute("data-framework").ShouldBe("FluentUI");
    }

    [Fact]
    public void ThemeToggle_LoadsTheToggleScript()
    {
        var cut = Render<ThemeToggle>();

        var stub = cut.FindComponent<Stub<BlazorJSComponents.JS>>();
        stub.Instance.Parameters.Get(x => x.Src).ShouldBe("./_content/Osirion.Blazor.Theming/js/theme-toggle.js");
    }

    [Fact]
    public void ThemeToggle_ThemeChanged_RendersTheNewMode()
    {
        var cut = Render<ThemeToggle>();
        _themeService.CurrentMode.Returns(ThemeMode.Dark);

        _themeService.ThemeChanged += Raise.EventWith(
            _themeService,
            new ThemeChangedEventArgs(ThemeMode.Dark, ThemeMode.Light));

        cut.WaitForAssertion(() => cut.FindAll("svg.bi-sun").Count.ShouldBe(1));
    }

    [Fact]
    public void ThemeToggle_Rendered_SubscribesToThemeChanges()
    {
        Render<ThemeToggle>();

        _themeService.Received(1).ThemeChanged += Arg.Any<EventHandler<ThemeChangedEventArgs>>();
    }

    [Fact]
    public async Task ThemeToggle_Disposed_UnsubscribesFromThemeChanges()
    {
        Render<ThemeToggle>();

        await DisposeComponentsAsync();

        _themeService.Received(1).ThemeChanged -= Arg.Any<EventHandler<ThemeChangedEventArgs>>();
    }
}
