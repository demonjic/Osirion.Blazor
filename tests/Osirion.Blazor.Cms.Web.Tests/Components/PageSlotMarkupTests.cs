using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Guard tests: the page components render the markup they rendered before their optional slots were added, as long
/// as those slots are left out. The snapshots in <c>Snapshots/</c> were captured from the components before the
/// change, so a slot that adds a wrapper, a class or whitespace while unused fails here.
/// </summary>
public partial class PageSlotMarkupTests : CmsWebComponentTestBase
{
    private static readonly DateTime Published = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private readonly IContentProvider _provider = Substitute.For<IContentProvider>();

    public PageSlotMarkupTests()
    {
        var manager = Substitute.For<IContentProviderManager>();
        manager.GetDefaultProvider().Returns(_provider);
        Services.AddSingleton(manager);
        Services.AddSingleton<NavigationManager>(new RecordingNavigationManager());
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new Osirion.Blazor.Cms.Web.Options.SeoMetadataOptions()));
    }

    [Fact]
    public void AboutPage_WithoutHeaderContent_RendersTheMarkupItRenderedBefore()
    {
        var cut = RenderStatic<AboutPage>(p => p.Add(c => c.Item, Item("About", "<p>Body</p>", "about.md")));

        Normalize(cut.Markup).MarkupMatches(Snapshot("AboutPage.html"));
    }

    [Fact]
    public void ArticlePage_WithoutFooterContent_RendersTheMarkupItRenderedBefore()
    {
        using var culture = new CultureScope("en-US");
        var article = Item("Titled", "<h2 id=\"one\">One</h2><p>Body</p>", "blog/titled.md");
        article.SetUrl("/blog/titled");
        article.AddTag("blazor");

        var cut = RenderStatic<ArticlePage>(p => p
            .Add(c => c.Item, article)
            .Add(c => c.ShowRelatedContent, false));

        Normalize(cut.Markup).MarkupMatches(Snapshot("ArticlePage.html"));
    }

    [Fact]
    public void ContentListPage_WithoutHeaderContent_RendersTheMarkupItRenderedBefore()
    {
        using var culture = new CultureScope("en-US");
        var post = Item("First post", "<p>Body</p>", "blog/first-post.md");
        post.SetUrl("blog/first-post");
        _provider.GetItemsByQueryAsync(Arg.Any<ContentQuery>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>?>([post]));

        var cut = RenderStatic<ContentListPage>(p => p.Add(c => c.Directory, "blog"));

        Normalize(cut.Markup).MarkupMatches(Snapshot("ContentListPage.html"));
    }

    private static ContentItem Item(string title, string content, string path)
    {
        var item = ContentItem.Create($"id-{title}", title, content, path, "test");
        item.SetCreatedDate(Published);
        return item;
    }

    // The script references of TableOfContents carry an identifier that is new on every render.
    private static string Normalize(string markup) => ScriptScopeId().Replace(markup, "scope");

    private static string Snapshot(string file)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return File.ReadAllText(Path.Combine(
                    directory.FullName, "tests", "Osirion.Blazor.Cms.Web.Tests", "Components", "Snapshots", file));
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }

    [GeneratedRegex("[0-9a-f]{32}")]
    private static partial Regex ScriptScopeId();
}
