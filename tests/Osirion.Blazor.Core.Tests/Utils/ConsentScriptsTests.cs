using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Osirion.Blazor.Core.Utils;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Utils;

public class ConsentScriptsTests
{
    private const string ModuleSource = "./_content/Osirion.Blazor.Core/js/osirion-consent.js";

    [Fact]
    public void HoldUntilConsent_InlineScript_IsHeldWithItsTextUnchanged()
    {
        const string text = "\n    window.dataLayer = window.dataLayer || [];\n    if (a < b && c > d) { go('x'); }\n";

        var held = Scripts(ConsentScripts.HoldUntilConsent($"<script>{text}</script>", "analytics"));

        held[0].GetAttribute("type").ShouldBe("text/plain");
        held[0].GetAttribute("data-osirion-consent").ShouldBe("analytics");
        held[0].HasAttribute("data-osirion-type").ShouldBeFalse();
        held[0].TextContent.ShouldBe(text);
    }

    [Fact]
    public void HoldUntilConsent_ExternalScript_KeepsEveryOtherAttribute()
    {
        const string markup = "<script async defer src=\"https://cdn.example/t.js?id=1&amp;b=2\" nonce=\"n0nce\" "
            + "integrity=\"sha384-abc\" crossorigin=\"anonymous\" data-website-id=\"42\"></script>";

        var held = Scripts(ConsentScripts.HoldUntilConsent(markup, "analytics"))[0];

        held.GetAttribute("src").ShouldBe("https://cdn.example/t.js?id=1&b=2");
        held.HasAttribute("async").ShouldBeTrue();
        held.HasAttribute("defer").ShouldBeTrue();
        held.GetAttribute("nonce").ShouldBe("n0nce");
        held.GetAttribute("integrity").ShouldBe("sha384-abc");
        held.GetAttribute("crossorigin").ShouldBe("anonymous");
        held.GetAttribute("data-website-id").ShouldBe("42");
        held.GetAttribute("type").ShouldBe("text/plain");
    }

    [Theory]
    [InlineData("module")]
    [InlineData("text/javascript")]
    public void HoldUntilConsent_ScriptWithAType_KeepsTheTypeForWhenItRuns(string type)
    {
        var held = Scripts(ConsentScripts.HoldUntilConsent($"<script type=\"{type}\">run()</script>", "analytics"))[0];

        held.GetAttribute("type").ShouldBe("text/plain");
        held.GetAttribute("data-osirion-type").ShouldBe(type);
    }

    [Fact]
    public void HoldUntilConsent_SeveralScripts_AreHeldInDocumentOrderFollowedByTheConsentModuleOnce()
    {
        var output = ConsentScripts.HoldUntilConsent("<script src=\"/a.js\"></script>\n<script>b()</script><div><script>c()</script></div>", "analytics");

        var scripts = Scripts(output);
        scripts.Select(s => s.GetAttribute("src") ?? s.TextContent).ShouldBe(["/a.js", "b()", "c()", ModuleSource]);
        var module = scripts[^1];
        module.GetAttribute("type").ShouldBe("module");
        module.HasAttribute("data-osirion-consent").ShouldBeFalse();
        module.TextContent.ShouldBeEmpty();
    }

    // A pixel or a preconnect link would reach the vendor before the visitor chose, so only scripts are kept.
    [Fact]
    public void HoldUntilConsent_OtherMarkup_IsLeftOut()
    {
        const string markup = "<link rel=\"preconnect\" href=\"https://vendor.example\">"
            + "<noscript><img src=\"https://vendor.example/pixel.gif\"></noscript>"
            + "<img src=\"https://vendor.example/p.gif\"><!-- note --><script>go()</script>";

        var output = ConsentScripts.HoldUntilConsent(markup, "analytics");

        Parse(output).Select(node => node.NodeName).ShouldBe(["SCRIPT", "#text", "SCRIPT"]);
        output.ShouldNotContain("vendor.example");
    }

    // Text that only looks like a script start tag (in a comment or an attribute) must not hide the real script after it.
    [Theory]
    [InlineData("<!-- <script> --><script src=\"https://t.example/x.js\"></script>")]
    [InlineData("<div title=\"<script>\"></div><script src=\"https://t.example/x.js\"></script>")]
    [InlineData("<style>a::after { content: '<script>'; }</style><script src=\"https://t.example/x.js\"></script>")]
    public void HoldUntilConsent_TagLookalikesBeforeAScript_StillHoldTheScript(string markup)
    {
        var scripts = Scripts(ConsentScripts.HoldUntilConsent(markup, "analytics"));

        scripts.Count.ShouldBe(2);
        scripts[0].GetAttribute("src").ShouldBe("https://t.example/x.js");
        scripts[0].GetAttribute("type").ShouldBe("text/plain");
    }

    [Fact]
    public void HoldUntilConsent_ScriptTextWithMarkupInside_ReadsBackTheSame()
    {
        const string text = "var s = '<!--<script>x</script>-->'; document.write('<\\/script>');";

        var held = Scripts(ConsentScripts.HoldUntilConsent($"<script>{text}</script>", "analytics"));

        held.Count.ShouldBe(2);
        held[0].TextContent.ShouldBe(text);
    }

    [Fact]
    public void HoldUntilConsent_CategoryAndAttributeValues_AreEncoded()
    {
        var output = ConsentScripts.HoldUntilConsent("<script data-x='a\"b'></script>", "stats\"><script>alert(1)</script>");

        var scripts = Scripts(output);
        scripts.Count.ShouldBe(2);
        scripts[0].GetAttribute("data-osirion-consent").ShouldBe("stats\"><script>alert(1)</script>");
        scripts[0].GetAttribute("data-x").ShouldBe("a\"b");
    }

    [Theory]
    [InlineData("")]
    [InlineData("<p>No script here</p>")]
    [InlineData("<noscript><script>never()</script></noscript>")]
    public void HoldUntilConsent_MarkupWithoutAScript_ReturnsEmpty(string markup)
    {
        ConsentScripts.HoldUntilConsent(markup, "analytics").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void HoldUntilConsent_NoCategory_Throws(string? category)
    {
        Should.Throw<ArgumentException>(() => ConsentScripts.HoldUntilConsent("<script></script>", category!));
    }

    [Fact]
    public void HoldUntilConsent_NullMarkup_Throws()
    {
        Should.Throw<ArgumentNullException>(() => ConsentScripts.HoldUntilConsent(null!, "analytics"));
    }

    private static INodeList Parse(string html)
    {
        var parser = new HtmlParser(new HtmlParserOptions { IsScripting = true });
        return parser.ParseFragment(html, parser.ParseDocument(string.Empty).Body!);
    }

    private static List<IElement> Scripts(string html) =>
        Parse(html).OfType<IElement>().SelectMany(node => node.QuerySelectorAll("script").Prepend(node))
            .Where(element => element.LocalName == "script").ToList();
}
