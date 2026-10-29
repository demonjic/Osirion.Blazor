using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Navigation.Components;
using Osirion.Blazor.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Navigation.Tests.Components;

public class OsirionLanguagePickerTests : OsirionComponentTestBase
{
    [Fact]
    public void Picker_LinksTheCurrentPageInEachCulture()
    {
        AddCultures();
        Navigate("/sr/contact");

        var cut = Render<OsirionLanguagePicker>();

        var nav = cut.Find("nav.osirion-language-picker");
        nav.GetAttribute("aria-label").ShouldBe("Language");
        var links = cut.FindAll("ul.osirion-language-picker-list > li.osirion-language-picker-item > a.osirion-language-picker-link");
        links.Select(link => (link.GetAttribute("href"), link.GetAttribute("hreflang"), link.GetAttribute("lang"), link.TextContent)).ShouldBe(
        [
            ("/contact", "en", "en", "English"),
            ("/sr/contact", "sr-Latn", "sr-Latn", "Srpski"),
            ("/de/contact", "de", "de", "Deutsch"),
        ]);
    }

    [Fact]
    public void Picker_MarksOnlyTheCultureOfTheAddressAsCurrent()
    {
        AddCultures();
        Navigate("/sr/contact");

        var cut = Render<OsirionLanguagePicker>();

        cut.FindAll("a[aria-current]").ShouldHaveSingleItem().GetAttribute("aria-current").ShouldBe("true");
        cut.Find("a[aria-current]").GetAttribute("hreflang").ShouldBe("sr-Latn");
    }

    [Fact]
    public void Picker_AtAnAddressWithoutPrefix_MarksTheDefaultCultureAndLinksItWithoutPrefix()
    {
        AddCultures();
        Navigate("/");

        var cut = Render<OsirionLanguagePicker>();

        cut.Find("a[aria-current]").GetAttribute("href").ShouldBe("/");
        cut.FindAll("a").Select(link => link.GetAttribute("href")).ShouldBe(["/", "/sr", "/de"]);
    }

    [Fact]
    public void Picker_KeepsTheQueryAndDropsTheFragment()
    {
        AddCultures();
        Navigate("/de/blog?page=2#comments");

        var cut = Render<OsirionLanguagePicker>();

        cut.FindAll("a").Select(link => link.GetAttribute("href")).ShouldBe(["/blog?page=2", "/sr/blog?page=2", "/de/blog?page=2"]);
    }

    [Fact]
    public void Picker_AddressThatNamesAnotherHost_StillLinksTheSite()
    {
        AddCultures();
        Navigate("//example.com/contact");

        var cut = Render<OsirionLanguagePicker>();

        cut.FindAll("a").Select(link => link.GetAttribute("href")).ShouldAllBe(href => href!.StartsWith('/') && !href.StartsWith("//", StringComparison.Ordinal));
    }

    [Fact]
    public void Picker_OneCulture_RendersNothing()
    {
        Services.AddOsirionLocalization(options => options.DefaultCulture = "en");
        Navigate("/contact");

        var cut = Render<OsirionLanguagePicker>();

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void Picker_AriaLabelAndClass_AreApplied()
    {
        AddCultures();
        Navigate("/contact");

        var cut = Render<OsirionLanguagePicker>(parameters => parameters
            .Add(p => p.AriaLabel, "Jezik")
            .Add(p => p.Class, "site-languages"));

        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").ShouldBe("Jezik");
        nav.ClassList.ShouldBe(["osirion-language-picker", "site-languages"]);
    }

    private void AddCultures() => Services.AddOsirionLocalization(options =>
    {
        options.DefaultCulture = "en";
        options.Cultures.Add(new LocalizationCulture { Name = "en", DisplayName = "English" });
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr", DisplayName = "Srpski" });
        options.Cultures.Add(new LocalizationCulture { Name = "de" });
    });

    // Called after the services are added: the first service resolved closes the collection.
    private void Navigate(string location)
    {
        SetRendererInfo(new RendererInfo("Static", false));
        Services.GetRequiredService<NavigationManager>().NavigateTo(location);
    }
}
