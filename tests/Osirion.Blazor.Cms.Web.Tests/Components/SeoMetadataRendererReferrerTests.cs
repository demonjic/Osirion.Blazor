using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Components;

/// <summary>
/// Pins the referrer meta tag: none unless the site or the page names a policy, because a referrer meta tag replaces
/// the policy of the <c>Referrer-Policy</c> response header for the whole document.
/// </summary>
public class SeoMetadataRendererReferrerTests : CmsWebComponentTestBase
{
    // Fails before the fix: every page said "no-referrer-when-downgrade", which replaced a stricter header policy.
    [Fact]
    public void SeoMetadataRenderer_NoReferrerPolicy_WritesNoReferrerMetaTag()
    {
        var cut = RenderPage(_ => { });

        cut.FindAll("meta[name='referrer']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_ConfiguredReferrerPolicy_IsWritten()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { ReferrerPolicy = "strict-origin" }));

        var cut = RenderPage(_ => { });

        Meta(cut, "referrer").ShouldBe("strict-origin");
    }

    [Fact]
    public void SeoMetadataRenderer_PageReferrerPolicy_WinsOverTheConfiguredOne()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions { ReferrerPolicy = "strict-origin" }));

        var cut = RenderPage(p => p.Add(c => c.ReferrerPolicy, "no-referrer"));

        Meta(cut, "referrer").ShouldBe("no-referrer");
    }

    [Theory]
    [InlineData("Same-Origin", "same-origin")]
    [InlineData(" origin ", "origin")]
    public void SeoMetadataRenderer_ReferrerPolicyInAnotherCase_IsWrittenAsTheToken(string configured, string expected)
    {
        var cut = RenderPage(p => p.Add(c => c.ReferrerPolicy, configured));

        Meta(cut, "referrer").ShouldBe(expected);
    }

    [Theory]
    [InlineData("strict")]
    [InlineData("origin\" onload=\"x")]
    [InlineData(" ")]
    public void SeoMetadataRenderer_UnknownReferrerPolicy_WritesNoReferrerMetaTag(string configured)
    {
        var cut = RenderPage(p => p.Add(c => c.ReferrerPolicy, configured));

        cut.FindAll("meta[name='referrer']").ShouldBeEmpty();
    }

    [Fact]
    public void SeoMetadataRenderer_AdditionalMetaTagNamedReferrer_IsStillSkipped()
    {
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new SeoMetadataOptions
        {
            AdditionalMetaTags = new Dictionary<string, string> { ["referrer"] = "unsafe-url" }
        }));

        var cut = RenderPage(_ => { });

        cut.FindAll("meta[name='referrer']").ShouldBeEmpty();
    }

    private IRenderedComponent<SeoMetadataRenderer> RenderPage(
        Action<ComponentParameterCollectionBuilder<SeoMetadataRenderer>> parameters,
        Osirion.Blazor.Cms.Domain.Entities.ContentItem? item = null)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(SeoTestContent.PageUrl);
        return RenderStatic<SeoMetadataRenderer>(p =>
        {
            p.Add(c => c.Content, item ?? SeoTestContent.Article());
            parameters(p);
        });
    }

    private static string? Meta(IRenderedComponent<SeoMetadataRenderer> cut, string name) =>
        cut.FindAll($"meta[name='{name}']").ShouldHaveSingleItem().GetAttribute("content");

    private static string? Property(IRenderedComponent<SeoMetadataRenderer> cut, string property) =>
        cut.FindAll($"meta[property='{property}']").ShouldHaveSingleItem().GetAttribute("content");
}
