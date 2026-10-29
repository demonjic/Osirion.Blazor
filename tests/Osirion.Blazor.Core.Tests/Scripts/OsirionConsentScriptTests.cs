using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using BlazorJSComponents;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Handlers;
using Osirion.Blazor.Core.Utils;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Scripts;

/// <summary>
/// Reads the shipped consent script, which has no script test runner, and checks it against what the C# side writes:
/// the consent endpoint's cookie, the browser evaluated banner and the scripts <see cref="ConsentScripts"/> holds. Apart
/// from the first test, these are guard tests that pass on the script as it shipped: they fail when one side of the
/// contract changes without the other. What the script does in a browser is checked by driving a page in Microsoft Edge.
/// </summary>
public partial class OsirionConsentScriptTests : BunitContext
{
    private static readonly string Script = File.ReadAllText(ScriptPath());

    public OsirionConsentScriptTests()
    {
        Services.AddSingleton(Options.Create(new ThemingOptions()));
        Services.AddJSComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
        SetRendererInfo(new RendererInfo("Static", false));
    }

    // Fails before the fix: the server rendered banner opens for a plain link to a page with ?customize-cookies, which is
    // what LegalLinks and FooterLink render, but the script ignored the query, so that link could not reopen the banner.
    [Fact]
    public void OsirionConsentScript_CustomizeCookiesQuery_IsReadAndRemovedFromTheAddress()
    {
        Script.ShouldContain("'customize-cookies'", Case.Sensitive);
        Script.ShouldContain("window.history.replaceState(window.history.state, ''", Case.Sensitive);
    }

    [Fact]
    public void OsirionConsentScript_ConsentCookie_IsWrittenInTheEndpointFormat()
    {
        // The endpoint serializes CookieConsentData, so its property order is the key order of the cookie's JSON.
        var endpointKeys = JsonDocument.Parse(JsonSerializer.Serialize(new CookieConsentData())).RootElement
            .EnumerateObject().Select(property => property.Name).ToList();

        var literal = ConsentLiteral().Match(Script);

        literal.Success.ShouldBeTrue();
        ObjectKey().Matches(literal.Groups["body"].Value).Select(key => key.Groups["key"].Value).ShouldBe(endpointKeys);
        Constant("COOKIE_VERSION").ShouldBe("1.0");
        Script.ShouldContain("document.cookie = 'osirion_cookie_consent=' + escapeLikeTheEndpoint(JSON.stringify(consent))", Case.Sensitive);
        Script.ShouldContain("+ '; path=/; max-age=' + days * SECONDS_PER_DAY + '; SameSite=Lax' + secure;", Case.Sensitive);
        Script.ShouldContain("const secure = window.location.protocol === 'https:' ? '; Secure' : '';", Case.Sensitive);
    }

    [Theory]
    [InlineData("BANNER")]
    [InlineData("CHOICE_BUTTON")]
    [InlineData("REGION")]
    [InlineData("PANEL")]
    public void OsirionConsentScript_BannerSelector_FindsItInTheBrowserBanner(string constant)
    {
        var cut = RenderBrowserBanner();

        cut.FindAll(Constant(constant)).ShouldNotBeEmpty();
    }

    [Fact]
    public void OsirionConsentScript_BrowserBanner_WritesOnlyAttributesAndChoicesTheScriptReads()
    {
        var cut = RenderBrowserBanner();

        var attributes = cut.FindAll("*").SelectMany(element => element.Attributes).Select(attribute => attribute.Name)
            .Where(name => name.StartsWith("data-osirion-", StringComparison.Ordinal)).Distinct().ToList();
        attributes.ShouldNotBeEmpty();
        attributes.ShouldAllBe(name => Script.Contains("[" + name + "]") || Script.Contains("'" + name + "'"));
        cut.FindAll("button").Select(button => button.GetAttribute("value")).ShouldAllBe(value => Script.Contains("'" + value + "'"));
        cut.FindAll("input[type=checkbox]").Select(box => box.GetAttribute("name")!)
            .ShouldAllBe(name => name.StartsWith(Constant("CATEGORY_PREFIX"), StringComparison.Ordinal));
    }

    [Fact]
    public void OsirionConsentScript_HeldSelector_FindsEveryScriptConsentScriptsHolds()
    {
        var held = ConsentScripts.HoldUntilConsent("<script>a()</script><script type=\"module\" src=\"/b.js\"></script>", "analytics");

        var document = new HtmlParser().ParseDocument(held);

        document.QuerySelectorAll(Constant("HELD")).Length.ShouldBe(2);
        document.QuerySelectorAll("script").SelectMany(script => script.Attributes).Select(attribute => attribute.Name)
            .Where(name => name.StartsWith("data-osirion-", StringComparison.Ordinal)).Distinct()
            .ShouldAllBe(name => Script.Contains("'" + name + "'"));
    }

    // The names sites write in their own markup and code, as the cookie consent docs give them.
    [Fact]
    public void OsirionConsentScript_PublicHooks_KeepTheirDocumentedNames()
    {
        Constant("OPENER").ShouldBe("[data-osirion-consent-open]");
        Script.ShouldContain("new CustomEvent('osirion-consent-change'", Case.Sensitive);
    }

    private IRenderedComponent<OsirionCookieConsent> RenderBrowserBanner() =>
        Render<OsirionCookieConsent>(p => p.Add(c => c.Evaluation, ConsentEvaluation.Browser));

    private static string Constant(string name)
    {
        var match = Regex.Match(Script, @"const " + name + @" = '(?<value>[^']*)';");
        match.Success.ShouldBeTrue("osirion-consent.js has no constant " + name);
        return match.Groups["value"].Value;
    }

    [GeneratedRegex(@"const consent = \{(?<body>[^}]*)\};")]
    private static partial Regex ConsentLiteral();

    [GeneratedRegex(@"^\s*(?<key>\w+):", RegexOptions.Multiline)]
    private static partial Regex ObjectKey();

    private static string ScriptPath()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return Path.Combine(directory.FullName, "src", "Osirion.Blazor.Core", "wwwroot", "js", "osirion-consent.js");
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }
}
