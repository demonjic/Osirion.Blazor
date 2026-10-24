using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Tests.Base;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Components.Rendering;

public class OsirionHtmlRendererOptionsTests : OsirionComponentTestBase
{
    private const string CodeBlock = "<pre><code class=\"language-sql\">select 1</code></pre>";

    public OsirionHtmlRendererOptionsTests()
    {
        Services.AddJSComponents();
    }

    private const string Table = "<table><tr><td>1</td></tr></table>";

    // Wide code, and tables on small screens, scroll sideways; a keyboard can scroll only what it can focus.
    [Fact]
    public void OsirionHtmlRenderer_CodeBlocksAndTables_AreInTheTabOrder()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock + Table + "<ul><li>" + Table + "</li></ul>"));

        cut.Find("pre").GetAttribute("tabindex").ShouldBe("0");
        cut.FindAll("table").Select(table => table.GetAttribute("tabindex")).ShouldBe(["0", "0"]);
    }

    [Fact]
    public void OsirionHtmlRenderer_CodeBlockInTheCopyButtonWrapper_StaysInTheTabOrder()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock)
            .Add(p => p.EnableCopyButton, true));

        cut.Find(".osirion-code-wrapper pre").GetAttribute("tabindex").ShouldBe("0");
    }

    [Fact]
    public void OsirionHtmlRenderer_FocusableScrollBlocksOff_LeavesThemOutOfTheTabOrder()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock + Table)
            .Add(p => p.FocusableScrollBlocks, false));

        cut.Find("pre").HasAttribute("tabindex").ShouldBeFalse();
        cut.Find("table").HasAttribute("tabindex").ShouldBeFalse();
    }

    [Fact]
    public void OsirionHtmlRenderer_ElementWithItsOwnTabIndex_KeepsIt()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre tabindex=\"-1\"><code>x</code></pre>")
            .Add(p => p.SanitizeHtml, false));

        cut.Find("pre").GetAttribute("tabindex").ShouldBe("-1");
    }

    // The elements are found in the parsed content, so text that only looks like a tag is never rewritten.
    [Fact]
    public void OsirionHtmlRenderer_TagLikeTextInAnAttribute_IsLeftAsWritten()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<p aria-label=\"a <table> and a <pre>\">x</p>" + CodeBlock));

        cut.Find("p").GetAttribute("aria-label").ShouldBe("a <table> and a <pre>");
        cut.FindAll("table").ShouldBeEmpty();
        cut.Find("pre").GetAttribute("tabindex").ShouldBe("0");
    }

    [Fact]
    public void OsirionHtmlRenderer_CodeBlocksWithAccessibleTheme_AddsTheThemeClassAndStyles()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock)
            .Add(p => p.Class, "post-body"));

        cut.Find("article").ClassName.ShouldBe("osirion-html-renderer osirion-content osirion-wcag-theme post-body");
        cut.FindAll("link[href$='css/osirion-prism-accessible.min.css']").Count.ShouldBe(1);
        cut.FindAll("link[href*='prism-okaidia']").ShouldBeEmpty();
    }

    [Fact]
    public void OsirionHtmlRenderer_NoCodeBlocks_LoadsNoHighlighting()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<p>Plain</p>"));

        cut.Find("article").ClassName.ShouldBe("osirion-html-renderer osirion-content");
        cut.FindAll("style").ShouldBeEmpty();
        cut.Markup.ShouldNotContain("osirion-script-loader.js");
    }

    [Fact]
    public void OsirionHtmlRenderer_ShowLineNumbers_LoadsTheLineNumbersPlugin()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock)
            .Add(p => p.ShowLineNumbers, true));

        cut.Find("link[href*='prism-line-numbers.min.css']").GetAttribute("crossorigin").ShouldBe("anonymous");
        cut.Markup.ShouldContain("prism-line-numbers.min.js");
    }

    [Fact]
    public void OsirionHtmlRenderer_FeatureFlags_AreWrittenToTheConfigurationElement()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock)
            .Add(p => p.EnableCopyButton, true)
            .Add(p => p.EnableLineHighlighting, true));

        var config = cut.Find(".osirion-syntax-highlighter-config");
        config.GetAttribute("data-enable-copy").ShouldBe("true");
        config.GetAttribute("data-enable-line-highlighting").ShouldBe("true");
        config.GetAttribute("data-show-line-numbers").ShouldBe("false");
    }

    [Fact]
    public void OsirionHtmlRenderer_CopyButtonOnSeveralBlocks_WrapsEveryBlock()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, CodeBlock + "<pre><code class=\"language-py\">x</code></pre>")
            .Add(p => p.EnableCopyButton, true));

        cut.FindAll(".osirion-code-wrapper").Select(wrapper => wrapper.GetAttribute("data-language"))
            .ShouldBe(["sql", "python"]);
    }

    // The opening tag is rewritten only when it parses attribute by attribute; anything else is left as written.
    [Fact]
    public void OsirionHtmlRenderer_ShowLineNumbersOnUnparsablePreTag_LeavesTheTagUnchanged()
    {
        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .Add(p => p.HtmlContent, "<pre a=\"1\"b><code class=\"language-js\">x</code></pre>")
            .Add(p => p.SanitizeHtml, false)
            .Add(p => p.ShowLineNumbers, true));

        cut.Find("article pre").HasAttribute("class").ShouldBeFalse();
    }
}
