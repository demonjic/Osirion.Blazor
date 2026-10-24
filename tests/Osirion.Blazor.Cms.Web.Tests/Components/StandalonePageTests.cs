using System.Text.Json.Nodes;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

public class StandalonePageTests : CmsWebComponentTestBase
{
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private readonly RecordingNavigationManager _navigation = new();

    public StandalonePageTests()
    {
        Services.AddSingleton(_manager);
        Services.AddSingleton<NavigationManager>(_navigation);
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
        ReturnFromQuery();
    }

    [Fact]
    public void AboutPage_Defaults_LoadsTheAboutItemWithItsTitleAsTheOnlyH1()
    {
        ReturnFromQuery(Page("About", "<h1>About</h1><p>We build components.</p>"));
        var head = Render<HeadOutlet>();

        var cut = RenderStatic<AboutPage>(_ => { });

        _manager.Received().GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Slug == "about"), Arg.Any<CancellationToken>());
        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["About"]);
        cut.Find(".osirion-about-page-body").TextContent.ShouldContain("We build components.");
        SchemaTypesIn(head).ShouldContain("AboutPage");
    }

    [Fact]
    public void AboutPage_ChildContent_FollowsTheBody()
    {
        ReturnFromQuery(Page("About", "<p>Body</p>"));

        var cut = RenderStatic<AboutPage>(p => p.AddChildContent("<p class=\"team\">Team</p>"));

        cut.Find(".osirion-about-page-more .team").TextContent.ShouldBe("Team");
    }

    [Fact]
    public void AboutPage_Defaults_PutTheTitleInAPageHeading()
    {
        ReturnFromQuery(Page("About", "<p>Body</p>", "Who we are."));

        var cut = RenderStatic<AboutPage>(_ => { });

        cut.Find("header.osirion-page-heading h1").TextContent.ShouldBe("About");
        cut.Find(".osirion-page-heading-description").TextContent.ShouldBe("Who we are.");
        cut.FindAll(".osirion-hero-title").ShouldBeEmpty();
    }

    [Fact]
    public void AboutPage_ShowHero_PutsTheTitleInTheHero()
    {
        ReturnFromQuery(Page("About", "<p>Body</p>"));

        var cut = RenderStatic<AboutPage>(p => p.Add(c => c.ShowHero, true));

        cut.FindAll("h1").ShouldHaveSingleItem().ClassList.ShouldContain("osirion-hero-title");
        cut.FindAll(".osirion-page-heading").ShouldBeEmpty();
    }

    [Fact]
    public void AboutPage_HeaderContent_ReplacesTheHeadingAboveTheBodyAndKeepsOneH1()
    {
        ReturnFromQuery(Page("About", "<h1>About</h1><p>Body</p>", "Who we are."));

        var cut = RenderStatic<AboutPage>(p => p
            .Add(c => c.HeaderContent, item => $"<h1>{item.Title}</h1><a class=\"book\" href=\"/book\">Book a call</a>"));

        var header = cut.Find("header.osirion-about-page-header");
        header.QuerySelector("a.book")!.TextContent.ShouldBe("Book a call");
        header.NextElementSibling!.ClassList.ShouldContain("osirion-about-page-body");
        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["About"]);
        cut.FindAll(".osirion-page-heading").ShouldBeEmpty();
    }

    [Fact]
    public void AboutPage_HeaderContentWithShowHero_ShowsTheHeaderInsteadOfTheHero()
    {
        ReturnFromQuery(Page("About", "<p>Body</p>"));

        var cut = RenderStatic<AboutPage>(p => p
            .Add(c => c.ShowHero, true)
            .Add(c => c.HeaderContent, item => $"<h1>{item.Title}</h1>"));

        cut.FindAll(".osirion-hero-title").ShouldBeEmpty();
        cut.FindAll("h1").ShouldHaveSingleItem().TextContent.ShouldBe("About");
    }

    [Fact]
    public void AboutPage_ContentMissing_SignalsNotFound()
    {
        var cut = RenderStatic<AboutPage>(_ => { });

        cut.FindAll(".osirion-content-not-found").ShouldHaveSingleItem();
        _navigation.NotFoundCalls.ShouldBe(1);
    }

    [Fact]
    public void ContactPage_NoContent_RendersTheFormUnderTheTitle()
    {
        var head = Render<HeadOutlet>();

        var cut = RenderStatic<ContactPage>(p => p.Add(c => c.Description, "Questions and bug reports."));

        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Contact"]);
        cut.Find(".osirion-contact-page-form form").ShouldNotBeNull();
        cut.FindAll(".osirion-contact-page-body").ShouldBeEmpty();
        head.Find("meta[name='description']").GetAttribute("content").ShouldBe("Questions and bug reports.");
        SchemaTypesIn(head).ShouldBeEmpty();
        _navigation.NotFoundCalls.ShouldBe(0);
    }

    [Fact]
    public void ContactPage_WithContent_ShowsTheIntroductionAndWritesContactPageData()
    {
        ReturnFromQuery(Page("Get in touch", "<h1>Get in touch</h1><p>We answer within a day.</p>"));
        var head = Render<HeadOutlet>();

        var cut = RenderStatic<ContactPage>(_ => { });

        cut.FindAll("h1").Select(h => h.TextContent.Trim()).ShouldBe(["Get in touch"]);
        cut.Find(".osirion-contact-page-body").TextContent.ShouldContain("We answer within a day.");
        SchemaTypesIn(head).ShouldContain("ContactPage");
    }

    [Fact]
    public void ContactPage_ContactInfo_IsShownBesideTheFormOnlyWhenSet()
    {
        var without = RenderStatic<ContactPage>(_ => { });
        var with = RenderStatic<ContactPage>(p => p.Add(c => c.ContactInfo, new ContactInformation { Address = "Belgrade" }));

        without.Markup.ShouldNotContain("Belgrade");
        with.Markup.ShouldContain("Belgrade");
    }

    [Fact]
    public void ContactPage_FormContent_ReplacesTheBuiltInForm()
    {
        var cut = RenderStatic<ContactPage>(p => p.Add(c => c.FormContent, "<p class=\"custom\">Email us</p>"));

        cut.FindAll(".osirion-contact-page-form form").ShouldBeEmpty();
        cut.Find(".osirion-contact-page-form .custom").TextContent.ShouldBe("Email us");
    }

    [Fact]
    public void NotFoundPage_Defaults_AskSearchEnginesNotToIndexIt()
    {
        var head = Render<HeadOutlet>();

        var cut = RenderStatic<NotFoundPage>(_ => { });

        cut.Find("h1").TextContent.ShouldBe("Page not found");
        head.Find("meta[name='robots']").GetAttribute("content").ShouldBe("noindex");
    }

    [Fact]
    public void NotFoundPage_SuggestionsAndSearch_AppearOnlyWhenAskedFor()
    {
        var plain = RenderStatic<NotFoundPage>(_ => { });
        var helpful = RenderStatic<NotFoundPage>(p => p
            .Add(c => c.ShowSearch, true)
            .Add(c => c.Suggestions, [new ContentNotFoundSuggestion { Text = "Blog", Url = "/blog" }]));

        plain.FindAll("form").ShouldBeEmpty();
        plain.Markup.ShouldNotContain("These pages might help:");
        helpful.Find("form").GetAttribute("action").ShouldBe("/search");
        helpful.Markup.ShouldContain("These pages might help:");
    }

    private void ReturnFromQuery(params ContentItem[] items) =>
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>(items));

    private static ContentItem Page(string title, string content, string description = "Summary")
    {
        var item = ContentItem.Create($"id-{title}", title, content, $"{title}.md", "test");
        item.SetUrl($"/{title.ToLowerInvariant()}");
        item.SetDescription(description);
        return item;
    }

    private static List<string?> SchemaTypesIn(IRenderedComponent<HeadOutlet> head) =>
        head.FindAll("script[type='application/ld+json']")
            .Select(script => JsonNode.Parse(script.TextContent)?["@type"]?.GetValue<string>())
            .ToList();
}
