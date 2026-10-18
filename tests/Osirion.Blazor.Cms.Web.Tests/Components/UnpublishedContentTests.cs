using Bunit;
using NSubstitute;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// A page or view that looks an item up by path or address shows a draft (<c>published: false</c>) like a missing
/// item: the providers return drafts from those lookups because the admin edits them, so the components check the
/// status themselves. <c>ShowUnpublished</c> lets a host render drafts for previews.
/// </summary>
public class UnpublishedContentTests : CmsPageComponentTestBase
{
    private static ContentItem Draft(string name)
    {
        var item = Doc(name, "<p>Draft body</p>");
        item.Status = ContentStatus.Draft;
        return item;
    }

    // Fails before the fix: the draft rendered with status 200.
    [Fact]
    public void DocumentPage_DraftLoadedByPath_ShowsNotFoundAndSignals404()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/draft.md"));

        cut.FindAll("article").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("Draft body");
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void DocumentPage_PublishedItemLoadedByPath_Renders()
    {
        Provider.GetItemByPathAsync("docs/live.md").Returns(Doc("live"));

        var cut = RenderStatic<DocumentPage>(p => p.Add(c => c.ContentPath, "docs/live.md"));

        cut.FindAll("article").ShouldNotBeEmpty();
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the parameter did not exist.
    [Fact]
    public void DocumentPage_ShowUnpublished_RendersTheDraft()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<DocumentPage>(p => p
            .Add(c => c.ContentPath, "docs/draft.md")
            .Add(c => c.ShowUnpublished, true));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the draft rendered with status 200.
    [Fact]
    public void ContentView_DraftLoadedByPath_ShowsNotFoundAndSignals404()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Path, "docs/draft.md"));

        cut.Markup.ShouldNotContain("Draft body");
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void ContentView_PublishedItemLoadedByPath_Renders()
    {
        Provider.GetItemByPathAsync("docs/live.md").Returns(Doc("live", "<p>Live body</p>"));

        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Path, "docs/live.md"));

        cut.Markup.ShouldContain("Live body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the parameter did not exist.
    [Fact]
    public void ContentView_ShowUnpublished_RendersTheDraft()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<ContentView>(p => p
            .Add(c => c.Path, "docs/draft.md")
            .Add(c => c.ShowUnpublished, true));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // An item the parent passes is the host's choice, so the view shows it whatever its status.
    [Fact]
    public void ContentView_DraftSuppliedByTheParent_Renders()
    {
        var cut = RenderStatic<ContentView>(p => p.Add(c => c.Item, Draft("draft")));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the draft rendered with status 200.
    [Fact]
    public void LocalizedContentView_DraftLoadedByPath_ShowsNotFoundAndSignals404()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Path, "docs/draft.md"));

        cut.Markup.ShouldNotContain("Draft body");
        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void LocalizedContentView_PublishedItemLoadedByPath_Renders()
    {
        Provider.GetItemByPathAsync("docs/live.md").Returns(Doc("live", "<p>Live body</p>"));

        var cut = RenderStatic<LocalizedContentView>(p => p.Add(c => c.Path, "docs/live.md"));

        cut.Markup.ShouldContain("Live body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the parameter did not exist.
    [Fact]
    public void LocalizedContentView_ShowUnpublished_RendersTheDraft()
    {
        Provider.GetItemByPathAsync("docs/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<LocalizedContentView>(p => p
            .Add(c => c.Path, "docs/draft.md")
            .Add(c => c.ShowUnpublished, true));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the draft rendered with status 200.
    [Fact]
    public void LandingPage_DraftLoadedByPath_ShowsNotFoundAndSignals404()
    {
        Provider.GetItemByPathAsync("pages/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.ContentPath, "pages/draft.md"));

        cut.Markup.ShouldNotContain("Draft body");
        cut.FindAll(".osirion-not-found").ShouldHaveSingleItem();
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void LandingPage_PublishedItemLoadedByPath_Renders()
    {
        Provider.GetItemByPathAsync("pages/live.md").Returns(Doc("live", "<p>Live body</p>"));

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.ContentPath, "pages/live.md"));

        cut.Markup.ShouldContain("Live body");
        cut.FindAll(".osirion-not-found").ShouldBeEmpty();
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the parameter did not exist.
    [Fact]
    public void LandingPage_ShowUnpublished_RendersTheDraft()
    {
        Provider.GetItemByPathAsync("pages/draft.md").Returns(Draft("draft"));

        var cut = RenderStatic<LandingPage>(p => p
            .Add(c => c.ContentPath, "pages/draft.md")
            .Add(c => c.ShowUnpublished, true));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the draft rendered with status 200.
    [Fact]
    public void ContentPage_DraftLoadedByUrl_ShowsNotFoundAndSignals404()
    {
        Provider.GetItemByUrlAsync("docs/draft").Returns(Draft("draft"));

        var cut = RenderStatic<ContentPage>(p => p.Add(c => c.Path, "docs/draft"));

        cut.Markup.ShouldNotContain("Draft body");
        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        Navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void ContentPage_PublishedItemLoadedByUrl_Renders()
    {
        Provider.GetItemByUrlAsync("docs/live").Returns(Doc("live", "<p>Live body</p>"));

        var cut = RenderStatic<ContentPage>(p => p.Add(c => c.Path, "docs/live"));

        cut.Markup.ShouldContain("Live body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // Fails before the fix: the parameter did not exist.
    [Fact]
    public void ContentPage_ShowUnpublished_RendersTheDraft()
    {
        Provider.GetItemByUrlAsync("docs/draft").Returns(Draft("draft"));

        var cut = RenderStatic<ContentPage>(p => p
            .Add(c => c.Path, "docs/draft")
            .Add(c => c.ShowUnpublished, true));

        cut.Markup.ShouldContain("Draft body");
        Navigation.NotFoundCalls.ShouldBe(0);
    }

    // ArticlePage finds its item with a content query, which leaves drafts out unless the host's own Query asks
    // for them; this guards that the query the page builds keeps them out.
    [Fact]
    public void ArticlePage_LookupBySlug_AsksForPublishedItemsOnly()
    {
        ContentQuery? sent = null;
        Manager.GetContentByQueryAsync(Arg.Do<ContentQuery>(q => sent = q), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([]));

        RenderStatic<ArticlePage>(p => p.Add(c => c.ItemSlug, "draft"));

        sent.ShouldNotBeNull();
        sent.IncludeUnpublished.ShouldBeFalse();
        sent.Status.ShouldBeNull();
    }
}
