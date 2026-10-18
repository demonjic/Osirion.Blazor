using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Models;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// The locale selector of <see cref="LocalizedNavigation"/>, fed by a provider that reports its locales through
/// <see cref="ILocalizationInfoProvider"/>.
/// </summary>
public class LocalizedNavigationLocaleSelectorTests : CmsWebComponentTestBase
{
    private readonly IContentProvider _provider = Substitute.For<IContentProvider, ILocalizationInfoProvider>();
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();

    public LocalizedNavigationLocaleSelectorTests()
    {
        Services.AddSingleton(_manager);
        _manager.GetDefaultProvider().Returns(_provider);
        var docs = DirectoryItem.Create("id-docs", "en/docs", "Docs", "test");
        _provider.GetDirectoriesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<DirectoryItem>>([docs]));
        ReturnLocales(Locale("en", "English", isDefault: true), Locale("sr", "srpski"));
    }

    // Fails before the change: the selector never rendered, because the component did not load the locales.
    [Fact]
    public void LocalizedNavigation_ProviderWithSeveralLocales_LinksTheCurrentPageInEachLocale()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/en/docs/intro");

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        var links = cut.FindAll(".osirion-locale-selector a.osirion-locale-button");
        links.Select(a => a.TextContent.Trim()).ShouldBe(["English", "srpski"]);
        links.Select(a => a.GetAttribute("href")).ShouldBe(["/en/docs/intro", "/sr/docs/intro"]);
        links.Select(a => a.GetAttribute("hreflang")).ShouldBe(["en", "sr"]);
    }

    [Fact]
    public void LocalizedNavigation_CurrentLocale_IsMarkedWithAriaCurrent()
    {
        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "SR"));

        var links = cut.FindAll("a.osirion-locale-button");
        links.Select(a => a.GetAttribute("aria-current")).ShouldBe([null, "true"]);
        links[1].ClassList.ShouldContain("osirion-active");
        links[0].ClassList.ShouldNotContain("osirion-active");
    }

    [Fact]
    public void LocalizedNavigation_LocaleNameFormatter_NamesEachLocale()
    {
        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.CurrentLocale, "en")
            .Add(c => c.LocaleNameFormatter, (string code) => $"Lang {code.ToUpperInvariant()}"));

        cut.FindAll("a.osirion-locale-button").Select(a => a.TextContent.Trim()).ShouldBe(["Lang EN", "Lang SR"]);
    }

    [Fact]
    public void LocalizedNavigation_LocaleWithoutNativeName_ShowsItsCode()
    {
        ReturnLocales(Locale("en", "English", isDefault: true), Locale("x-test", string.Empty));

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        cut.FindAll("a.osirion-locale-button").Select(a => a.TextContent.Trim()).ShouldBe(["English", "x-test"]);
    }

    [Fact]
    public void LocalizedNavigation_PageWithoutLocaleSegment_PutsTheLocaleInFront()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/docs/intro?tab=2#top");

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        cut.FindAll("a.osirion-locale-button").Select(a => a.GetAttribute("href")).ShouldBe(["/en/docs/intro", "/sr/docs/intro"]);
    }

    [Fact]
    public void LocalizedNavigation_LocaleUrlFormatter_BuildsEachLink()
    {
        var cut = RenderStatic<LocalizedNavigation>(p => p
            .Add(c => c.CurrentLocale, "en")
            .Add(c => c.LocaleUrlFormatter, (string code) => $"/home/{code}"));

        cut.FindAll("a.osirion-locale-button").Select(a => a.GetAttribute("href")).ShouldBe(["/home/en", "/home/sr"]);
    }

    [Fact]
    public void LocalizedNavigation_SingleLocale_RendersNoSelector()
    {
        ReturnLocales(Locale("en", "English", isDefault: true));

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        cut.FindAll(".osirion-locale-selector").ShouldBeEmpty();
    }

    [Fact]
    public void LocalizedNavigation_LocalizationDisabled_DoesNotLoadTheLocales()
    {
        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.EnableLocalization, false));

        cut.FindAll(".osirion-locale-selector").ShouldBeEmpty();
        ((ILocalizationInfoProvider)_provider).DidNotReceive().GetLocalizationInfoAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void LocalizedNavigation_LocalesFailToLoad_StillRendersTheTree()
    {
        ((ILocalizationInfoProvider)_provider).GetLocalizationInfoAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        cut.FindAll(".osirion-locale-selector").ShouldBeEmpty();
        cut.FindAll("a.osirion-nav-link").ShouldHaveSingleItem();
    }

    [Fact]
    public void LocalizedNavigation_NoLocaleCallback_WritesNoHandlerOnTheLocaleLinks()
    {
        var cut = RenderStatic<LocalizedNavigation>(p => p.Add(c => c.CurrentLocale, "en"));

        cut.Find(".osirion-locale-selector").InnerHtml.ShouldNotContain("onclick");
    }

    [Fact]
    public void LocalizedNavigation_LocaleCallbackInInteractiveMode_RunsWithTheChosenLocale()
    {
        SetRendererInfo(new RendererInfo("Server", true));
        string? chosen = null;
        var cut = Render<LocalizedNavigation>(p => p
            .Add(c => c.CurrentLocale, "en")
            .Add(c => c.OnLocaleChanged, (string code) => chosen = code));

        cut.Find("a.osirion-locale-button[hreflang=sr]").Click();

        chosen.ShouldBe("sr");
        cut.Find("a.osirion-locale-button[hreflang=sr]").GetAttribute("aria-current").ShouldBe("true");
    }

    private void ReturnLocales(params LocaleInfo[] locales)
    {
        var info = new LocalizationInfo
        {
            DefaultLocale = locales.First(locale => locale.IsDefault).Code,
            AvailableLocales = [.. locales],
        };
        ((ILocalizationInfoProvider)_provider).GetLocalizationInfoAsync(Arg.Any<CancellationToken>()).Returns(info);
    }

    private static LocaleInfo Locale(string code, string nativeName, bool isDefault = false) => new()
    {
        Code = code,
        Name = nativeName,
        NativeName = nativeName,
        IsDefault = isDefault,
    };
}
