using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class SearchBoxAccessibilityTests : CmsWebComponentTestBase
{
    // Fails before the fix: aria-label was set to the placeholder, which overrides the label as the field's accessible name.
    [Fact]
    public void SearchBox_Field_IsNamedByItsLabelNotByTheTextOfThePlaceholder()
    {
        var cut = RenderStatic<SearchBox>(p => p.Add(c => c.Placeholder, "Type here"));

        var input = cut.Find("input");
        input.HasAttribute("aria-label").ShouldBeFalse();
        input.GetAttribute("placeholder").ShouldBe("Type here");
        cut.Find("label").TextContent.ShouldBe("Search");
        cut.Find("label").GetAttribute("for").ShouldBe(input.GetAttribute("id"));
    }

    [Fact]
    public void SearchBox_Label_CanBeSetForAnotherLanguage()
    {
        var cut = RenderStatic<SearchBox>(p => p.Add(c => c.Label, "Suchen"));

        cut.Find("label").TextContent.ShouldBe("Suchen");
    }

    // Fails before the change: the button's name was always the English "Submit search".
    [Fact]
    public void SearchBox_SubmitButton_IsNamedBySubmitLabelAndShowsItsText()
    {
        var cut = RenderStatic<SearchBox>(p => p
            .Add(c => c.SubmitLabel, "Suche starten")
            .Add(c => c.SearchButtonText, "Suche"));

        var button = cut.Find("button[type='submit']");
        button.GetAttribute("aria-label").ShouldBe("Suche starten");
        button.TextContent.Trim().ShouldBe("Suche");
    }

    [Fact]
    public void SearchBox_SubmitButton_IsNamedSubmitSearchByDefault()
    {
        var cut = RenderStatic<SearchBox>(_ => { });

        cut.Find("button[type='submit']").GetAttribute("aria-label").ShouldBe("Submit search");
    }

    // Fails before the fix: the id came from GetHashCode(), which differs between instances and processes, so a prerendered and an interactive box disagreed.
    [Fact]
    public void SearchBox_DefaultId_IsTheSameForEveryInstanceWithTheSameSettings()
    {
        var first = RenderStatic<SearchBox>(_ => { }).Find("input").GetAttribute("id");
        var second = RenderStatic<SearchBox>(_ => { }).Find("input").GetAttribute("id");

        first.ShouldNotBeNullOrWhiteSpace();
        second.ShouldBe(first);
        first.ShouldStartWith("search-input-");
    }

    [Fact]
    public void SearchBox_Id_NamesTheInputAndTheLabelTarget()
    {
        var cut = RenderStatic<SearchBox>(p => p.Add(c => c.Id, "site-search"));

        cut.Find("input").GetAttribute("id").ShouldBe("site-search");
        cut.Find("label").GetAttribute("for").ShouldBe("site-search");
    }

    [Fact]
    public void SearchBox_DefaultId_DiffersWhenTheFormSubmitsElsewhere()
    {
        var site = RenderStatic<SearchBox>(_ => { }).Find("input").GetAttribute("id");
        var other = RenderStatic<SearchBox>(p => p.Add(c => c.ActionUrl, "/find")).Find("input").GetAttribute("id");

        other.ShouldNotBe(site);
    }
}
