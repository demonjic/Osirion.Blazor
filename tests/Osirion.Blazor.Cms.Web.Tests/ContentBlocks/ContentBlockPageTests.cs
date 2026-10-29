using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Web.Components;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.ContentBlocks;

/// <summary>
/// The page-level components render an item's blocks, its body only with <c>body: after</c>, and an item without
/// blocks as before; one FAQPage per page.
/// </summary>
public class ContentBlockPageTests : ContentBlockTestBase
{
    // Guard test: captured from ContentRenderer before content blocks existed (commit 502165da); an item without blocks
    // must render it unchanged.
    private const string MarkupBeforeBlocks =
        "<div class=\"osirion-content-view\"><div class=\"osirion-content-body\"><link rel=\"stylesheet\" href=\"_content/Osirion.Blazor.Core/css/osirion-html-renderer.min.css\" /><div class=\"osirion-syntax-highlighter-config\" data-enable-copy=\"false\" data-enable-line-highlighting=\"false\" data-show-line-numbers=\"false\" hidden></div><article class=\"osirion-html-renderer osirion-content\"><p>Body text.</p></article></div></div>";

    private static ContentItem Item(string yaml, string body = "<h1>Home</h1><p>Body text.</p>")
    {
        var item = ContentItem.Create("home", "Home", body, SourcePath, "test");
        item.SetBlocks(Read(yaml));
        return item;
    }

    [Fact]
    public void ContentRenderer_ItemWithoutBlocks_RendersAsBefore()
    {
        var cut = RenderStatic<ContentRenderer>(p => p
            .Add(c => c.Item, ContentItem.Create("p", "Post", "<p>Body text.</p>", "blog/post.md", "test")));

        Normalize(cut.Markup).ShouldBe(MarkupBeforeBlocks);
    }

    [Fact]
    public void ContentRenderer_ItemWithBlocks_RendersOnlyTheBlocks()
    {
        var cut = RenderStatic<ContentRenderer>(p => p.Add(c => c.Item, Item("blocks:\n  - { type: markdown, markdown: From blocks }\n")));

        cut.Find(".osirion-content-blocks").TextContent.ShouldContain("From blocks");
        cut.Markup.ShouldNotContain("Body text.");
    }

    [Fact]
    public void ContentRenderer_BodyAfter_RendersTheBodyAfterTheBlocks()
    {
        var cut = RenderStatic<ContentRenderer>(p => p
            .Add(c => c.Item, Item("body: after\nblocks:\n  - { type: markdown, markdown: From blocks }\n"))
            .Add(c => c.OmitLeadingTitle, true));

        var markup = cut.Markup;
        markup.IndexOf("From blocks", StringComparison.Ordinal).ShouldBeLessThan(markup.IndexOf("Body text.", StringComparison.Ordinal));
        markup.ShouldNotContain("<h1>Home</h1>");
    }

    [Fact]
    public void LandingPage_ItemWithBlocks_ReplacesTheHeroWithTheBlocks()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
        Services.AddSingleton<NavigationManager>(new Base.RecordingNavigationManager());

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.Content, Item("blocks:\n  - { type: hero, title: From blocks }\n")));

        cut.FindAll("h1").ShouldHaveSingleItem().TextContent.ShouldBe("From blocks");
        cut.Markup.ShouldNotContain("Body text.");
    }

    [Fact]
    public void LandingPage_BodyAfter_RendersTheBodyAfterTheBlocks()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
        Services.AddSingleton<NavigationManager>(new Base.RecordingNavigationManager());

        var cut = RenderStatic<LandingPage>(p => p.Add(c => c.Content, Item("body: after\nblocks:\n  - { type: hero, title: From blocks }\n")));

        cut.Markup.IndexOf("From blocks", StringComparison.Ordinal).ShouldBeLessThan(cut.Markup.IndexOf("Body text.", StringComparison.Ordinal));
    }

    [Fact]
    public void Faq_OnlyTheFirstFaqBlockWritesStructuredData()
    {
        var cut = RenderBlocks("blocks:\n  - { type: faq, items: [{ question: A?, answer: a }] }\n  - { type: faq, items: [{ question: B?, answer: b }] }\n");

        cut.FindAll(".osirion-faq-answer").Count.ShouldBe(2);
        var script = cut.FindAll("script[type='application/ld+json']").ShouldHaveSingleItem();
        script.TextContent.ShouldContain("A?");
    }

    [Fact]
    public void Faq_PageWithItsOwnFaqPage_WritesNone()
    {
        var cut = RenderBlocks("blocks:\n  - { type: faq, items: [{ question: A?, answer: a }] }\n", includeFaqStructuredData: false);

        cut.FindAll(".osirion-faq-answer").ShouldHaveSingleItem();
        cut.FindAll("script[type='application/ld+json']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_ItemWithAFaqBlock_LeavesOutTheGeneratedFaqPage()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("https://example.com/how-does-it-work");
        var item = ContentItem.Create("q", "How does it work", "<p>x</p>", SourcePath, "test");
        item.SetDescription("It works well.");
        item.SetBlocks(Read("blocks:\n  - { type: faq, items: [{ question: A?, answer: a }] }\n"));
        var plain = ContentItem.Create("q2", "How does it work", "<p>x</p>", "en/other.md", "test");
        plain.SetDescription("It works well.");

        var withBlock = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, item).Add(c => c.EnableGeneratedFaq, true));
        var withoutBlock = RenderStatic<SeoMetadataRenderer>(p => p.Add(c => c.Content, plain).Add(c => c.EnableGeneratedFaq, true));

        withBlock.Markup.ShouldNotContain("FAQPage");
        withoutBlock.Markup.ShouldContain("FAQPage");
    }

    // Line breaks, indentation and the CSS isolation attributes (b-xxxxxxxxxx) depend on the build, not on the markup.
    private static string Normalize(string markup) =>
        System.Text.RegularExpressions.Regex.Replace(
            string.Concat(markup.Split('\n').Select(line => line.Trim())), " b-[a-z0-9]{10}(?=[ >/])", string.Empty);
}
