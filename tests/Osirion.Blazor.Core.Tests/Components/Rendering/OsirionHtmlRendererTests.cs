using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Services;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Rendering;

public class OsirionHtmlRendererTests : OsirionComponentTestBase
{
    private const string HostileContent = "<p>Hello</p><img src=\"/a.png\" alt=\"a\" onerror=\"alert(1)\"><script>alert(2)</script>";

    /// <summary>A highlighted code block whose title attribute holds markup; a browser keeps it as attribute text.</summary>
    private const string CodeBlockWithMarkupInAttribute =
        "<pre><code class=\"language-js\" title=\"x><img src=x onerror=alert(1)>\">y</code></pre>";

    // Fails before the fix: SanitizeHtml defaulted to false and the content was rendered as given.
    [Fact]
    public void OsirionHtmlRenderer_DefaultParameters_RendersContentWithoutScriptOrHandlers()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, HostileContent));

        cut.Find("article").InnerHtml.MarkupMatches("<p>Hello</p><img src=\"/a.png\" alt=\"a\">");
    }

    [Fact]
    public void OsirionHtmlRenderer_SanitizeHtmlFalse_RendersMarkupAsGiven()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<p onclick=\"track()\">Trusted</p>")
            .Add(p => p.SanitizeHtml, false));

        cut.Find("article p").GetAttribute("onclick").ShouldBe("track()");
    }

    [Fact]
    public void OsirionHtmlRenderer_CustomSanitizerFunc_RendersItsOutput()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, HostileContent)
            .Add(p => p.HtmlSanitizer, _ => "<p>custom</p>"));

        cut.Find("article").InnerHtml.MarkupMatches("<p>custom</p>");
    }

    [Fact]
    public void OsirionHtmlRenderer_RegisteredSanitizer_RendersItsOutput()
    {
        var sanitizer = Substitute.For<IHtmlContentSanitizer>();
        sanitizer.Sanitize(HostileContent).Returns("<p>registered</p>");
        Services.AddSingleton(sanitizer);

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, HostileContent));

        cut.Find("article").InnerHtml.MarkupMatches("<p>registered</p>");
    }

    [Fact]
    public void OsirionHtmlRenderer_HostAllowList_KeepsCustomElement()
    {
        Services.AddOsirionHtmlSanitizer(options => options.AllowElement("chart", "type", "data"));

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<chart type=\"line\" data=\"[[1, 2]]\" onclick=\"alert(1)\"></chart>"));

        cut.Find("article").InnerHtml.MarkupMatches("<chart type=\"line\" data=\"[[1, 2]]\"></chart>");
    }

    // The copy button handler is added by the component after sanitization; content handlers are not.
    [Fact]
    public void OsirionHtmlRenderer_CopyButtonEnabled_KeepsLibraryHandlerAndRemovesContentHandler()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-csharp\" onclick=\"alert(1)\">var x = 1;</code></pre>")
            .Add(p => p.EnableCopyButton, true));

        cut.Find("button.osirion-copy-button").GetAttribute("onclick").ShouldBe("osirionCopyCode(this)");
        cut.Find("article code").HasAttribute("onclick").ShouldBeFalse();
    }

    [Fact]
    public void OsirionHtmlRenderer_CodeBlockWithMarkupInAttribute_RendersNoElementFromTheAttribute()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlockWithMarkupInAttribute));

        cut.FindAll("article img").ShouldBeEmpty();
    }

    // Fails before the fix: after sanitization the code tag was rewritten with a pattern that stopped at the
    // first '>', even inside a quoted attribute value, so a sanitizer that leaves '>' unescaped in attribute
    // values (valid HTML) saw the markup in the title turn into a live element.
    [Fact]
    public void OsirionHtmlRenderer_CodeBlockWithMarkupInAttributeAndCustomSanitizer_RendersNoElementFromTheAttribute()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlockWithMarkupInAttribute)
            .Add(p => p.HtmlSanitizer, html => html));

        cut.FindAll("article img").ShouldBeEmpty();
        cut.Find("article code").GetAttribute("title").ShouldBe("x><img src=x onerror=alert(1)>");
    }

    // Fails before the fix: Replace(match.Value) rewrote every identical block with the first wrapper, and the
    // next iteration then found the original text again inside that wrapper and wrapped it a second time.
    [Fact]
    public void OsirionHtmlRenderer_IdenticalCodeBlocksWithCopyButton_WrapsEachBlockOnce()
    {
        Services.AddJSComponents();
        const string block = "<pre><code class=\"language-csharp\">var x = 1;</code></pre>";

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, block + "<p>between</p>" + block)
            .Add(p => p.EnableCopyButton, true));

        cut.FindAll("article .osirion-code-wrapper").Count.ShouldBe(2);
        cut.FindAll("article .osirion-code-wrapper .osirion-code-wrapper").ShouldBeEmpty();
    }

    // Fails before the fix: the class group never matched, so a pre that had a class got a second class attribute.
    [Theory]
    [InlineData("<pre class=\"language-js\"><code>x</code></pre>", "language-js line-numbers")]
    [InlineData("<pre><code class=\"language-js\">x</code></pre>", "line-numbers")]
    [InlineData("<pre class='language-js line-numbers'><code>x</code></pre>", "language-js line-numbers")]
    public void OsirionHtmlRenderer_ShowLineNumbers_AddsOneLineNumbersClassAndKeepsExistingClasses(string html, string expectedClass)
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, html)
            .Add(p => p.ShowLineNumbers, true));

        var pre = cut.Find("article pre");
        pre.GetAttribute("class").ShouldBe(expectedClass);
        pre.Attributes.Count(attribute => attribute.Name == "class").ShouldBe(1);
    }

    // Fails before the fix: the Prism stylesheet was requested from cdnjs without integrity or crossorigin.
    [Fact]
    public void OsirionHtmlRenderer_PinnedPrismTheme_CarriesIntegrityAndCrossOrigin()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-js\">x</code></pre>")
            .Add(p => p.UseAccessibleTheme, false));

        var theme = cut.Find("link[href*='prism-okaidia']");
        theme.GetAttribute("href").ShouldBe("https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism-okaidia.min.css");
        theme.GetAttribute("integrity").ShouldStartWith("sha512-");
        theme.GetAttribute("crossorigin").ShouldBe("anonymous");
    }

    // Fails before the fix: the accessible code theme was an inline <style> element, which a Content Security Policy
    // without 'unsafe-inline' in style-src blocks, leaving code blocks unstyled. It is now in the renderer's stylesheet.
    [Fact]
    public void OsirionHtmlRenderer_AccessibleTheme_WritesNoInlineStyleAndNeedsNoOtherStylesheet()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-js\">x</code></pre>"));

        // Only BlazorJSComponents' own bl-script elements carry a style attribute (see the security guide)
        cut.FindAll("style").ShouldBeEmpty();
        cut.FindAll("[style]").ShouldAllBe(element => element.LocalName == "bl-script");
        cut.Find("article").ClassList.ShouldContain("osirion-wcag-theme");
        cut.FindAll("link[rel=stylesheet]").Select(link => link.GetAttribute("href"))
            .ShouldBe(["_content/Osirion.Blazor.Core/css/osirion-html-renderer.min.css"]);
    }

    [Fact]
    public void OsirionHtmlRenderer_CustomPrismBaseUrl_LoadsFromTheHostWithoutIntegrity()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-js\">x</code></pre>")
            .Add(p => p.UseAccessibleTheme, false)
            .Add(p => p.PrismBaseUrl, "/lib/prism/"));

        var theme = cut.Find("link[href*='prism-okaidia']");
        theme.GetAttribute("href").ShouldBe("/lib/prism/themes/prism-okaidia.min.css");
        theme.HasAttribute("integrity").ShouldBeFalse();
    }

    // Fails before the fix: only the PrismBaseUrl parameter chose the source, so a renderer inside ContentView or
    // ArticlePage, which do not expose it, always loaded Prism from cdnjs.
    [Fact]
    public void OsirionHtmlRenderer_ConfiguredPrismBaseUrl_LoadsEveryPrismFileFromTheSite()
    {
        Services.AddJSComponents();
        Services.AddOsirionPrism(options => options.BaseUrl = "/lib/prism/1.29.0/");

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-python\">x</code></pre>")
            .Add(p => p.UseAccessibleTheme, false)
            .Add(p => p.ShowLineNumbers, true));

        cut.Find("link[href*='prism-okaidia']").GetAttribute("href").ShouldBe("/lib/prism/1.29.0/themes/prism-okaidia.min.css");
        cut.Find("link[href*='prism-line-numbers']").GetAttribute("href").ShouldBe("/lib/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.css");
        cut.Markup.ShouldContain("/lib/prism/1.29.0/prism.min.js");
        cut.Markup.ShouldContain("/lib/prism/1.29.0/components/prism-python.min.js");
        cut.Markup.ShouldContain("/lib/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.js");
        cut.Markup.ShouldNotContain("cdnjs.cloudflare.com");
        cut.Markup.ShouldNotContain("sha512-");
    }

    [Fact]
    public void OsirionHtmlRenderer_PrismBaseUrlParameter_WinsOverTheConfiguredOne()
    {
        Services.AddJSComponents();
        Services.AddOsirionPrism(options => options.BaseUrl = "/lib/prism/1.29.0/");

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-js\">x</code></pre>")
            .Add(p => p.UseAccessibleTheme, false)
            .Add(p => p.PrismBaseUrl, "/vendor/prism/"));

        cut.Find("link[href*='prism-okaidia']").GetAttribute("href").ShouldBe("/vendor/prism/themes/prism-okaidia.min.css");
        cut.Markup.ShouldNotContain("/lib/prism/1.29.0/");
    }

    [Fact]
    public void OsirionHtmlRenderer_PrismOptionsWithoutBaseUrl_LoadThePinnedCdnReleaseWithIntegrity()
    {
        Services.AddJSComponents();
        Services.AddOsirionPrism(_ => { });

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-js\">x</code></pre>")
            .Add(p => p.UseAccessibleTheme, false));

        var theme = cut.Find("link[href*='prism-okaidia']");
        theme.GetAttribute("href").ShouldBe("https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism-okaidia.min.css");
        theme.GetAttribute("integrity").ShouldStartWith("sha512-");
    }

    [Fact]
    public void OsirionHtmlRenderer_PrismScripts_AreNotLoadedFromCdnWithoutIntegrity()
    {
        Services.AddJSComponents();

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre><code class=\"language-python\">x</code></pre>"));

        // The generic loader cannot enforce integrity, so no direct cdnjs script address may appear in the markup
        // outside the integrity-aware loader arguments, which carry the hash with every address.
        cut.FindAll("script[src*='cdnjs.cloudflare.com']").ShouldBeEmpty();
        cut.Markup.ShouldContain("osirion-script-loader.js");
        cut.Markup.ShouldContain("prism-python.min.js");
        cut.Markup.ShouldContain("sha512-");
    }

    // Fails before the fix: the configuration element had a fixed id, so two renderers on a page repeated it.
    [Fact]
    public void OsirionHtmlRenderer_TwoRenderers_ShareNoElementId()
    {
        Services.AddJSComponents();

        var first = RenderStatic<OsirionHtmlRenderer>(parameters => parameters.Add(p => p.HtmlContent, "<p>a</p>"));

        first.FindAll("[id]").ShouldBeEmpty();
    }
}
