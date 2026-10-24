using Shouldly;

namespace Osirion.Blazor.Core.Tests.Configuration;

/// <summary>Checks rules in the stylesheets the package ships, which no component test renders.</summary>
public class ShippedStylesheetTests
{
    private const string FrameworkNoneBody =
        ":where(html[data-osirion-framework=None]) body{background-color:var(--osirion-background-primary,#fff);color:var(--osirion-text-primary,#111827);font-family:var(--osirion-font-family-base,system-ui,sans-serif)}";

    // Fails before the fix: no shipped stylesheet painted the body, so a site without a CSS framework kept the browser's
    // white page in the dark theme and drew light text on it.
    [Theory]
    [InlineData("index.min.css")]
    [InlineData("base/base-styles.min.css")]
    public void MinifiedStylesheet_FrameworkNone_PaintsTheBodyInTheThemeColors(string file)
    {
        // Act
        var css = File.ReadAllText(CoreCss(file));

        // Assert
        css.ShouldContain(FrameworkNoneBody);
    }

    [Fact]
    public void BodyRule_IsScopedToFrameworkNoneAtZeroSpecificity()
    {
        // Act
        var css = File.ReadAllText(CoreCss("index.min.css"));

        // Assert: a framework paints its own body, and any rule a site writes for body must win
        css.ShouldNotContain("}body{background", Case.Sensitive);
        css.ShouldContain(":where(html[data-osirion-framework=None]) body{");
    }

    // Fails before the fix: every text link in rendered Markdown carried the external-link icon, so a link to
    // /blog/another-post looked as if it left the site.
    [Fact]
    public void HtmlRenderer_ExternalLinkIcon_IsOnlyOnAddressesThatLeaveTheSite()
    {
        // Act
        var css = File.ReadAllText(CoreCss("osirion-html-renderer.min.css"));
        var iconRules = System.Text.RegularExpressions.Regex.Matches(css, @"([^{}]*)::after\{content:")
            .Select(match => match.Groups[1].Value)
            .Where(selector => selector.Contains(".osirion-html-renderer :where(a", StringComparison.Ordinal))
            .ToList();

        // Assert
        iconRules.ShouldNotBeEmpty();
        iconRules.ShouldAllBe(selector => selector.Contains("a:is([href^=\"http:\"],[href^=\"https:\"],[href^=\"//\"])", StringComparison.Ordinal));
    }

    // Fails before the fix: the medium card was 450 pixels wide whatever its column, so on a 375 pixel screen it pushed
    // the page 116 pixels wider than the screen.
    [Fact]
    public void FeatureCard_FixedSizes_NeverGrowPastTheirContainer()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Cards", "OsirionFeatureCard.razor.css")));

        // Assert
        css.ShouldContain(".osirion-feature-card,.osirion-feature-card-image-container,.osirion-feature-card-image{max-width:100%;}");
    }

    // Fails before the fix: the value took the accent as it is; green on the Bootstrap adapter's light card read 2.78:1
    // and indigo on its dark card 1.83:1, below the 3:1 large text needs.
    [Fact]
    public void MetricCard_Value_MovesTheAccentTowardTheTextColor()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Cards", "OsirionMetricCard.razor.css")));

        // Assert
        css.ShouldContain(".osirion-metric-card-value{margin-bottom:0.5rem;color:color-mix(insrgb,var(--osirion-metric-card-accent)65%,var(--osirion-text-primary,#111827));");
    }

    // Fails before the fix: the content of an OsirionReveal started at opacity 0 and only the script revealed it, and the
    // Down, Left and Right rules moved it 1.25rem with !important even when the script never ran, so with script blocked,
    // Animate off or reduced motion the content stayed invisible or offset.
    [Fact]
    public void Reveal_HiddenStateAppliesOnlyAfterTheScriptArmsTheElement()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Rendering", "OsirionReveal.razor.css")));
        var script = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ComponentCss("Rendering", "OsirionReveal.razor.css"))!, "OsirionReveal.razor.js"));

        // Assert
        css.ShouldNotContain("data-osirion-reveal");
        css.ShouldNotContain("!important");
        css.ShouldContain(".osirion-reveal.osirion-reveal-armed{opacity:0;");
        css.ShouldContain(".osirion-reveal-armed.osirion-reveal-left{transform:translateX(1.25rem);}");
        script.ShouldContain("classList.add('osirion-reveal-armed')");
    }

    // Fails before the fix: the code block header and copy button used #f8f8f2 text on a background taken from
    // --osirion-syntax-background-color, which only the palettes define, so in the light theme the button read 1.06:1
    // on white and under a light palette its light text sat on a light header.
    [Theory]
    [InlineData("osirion-html-renderer.css")]
    [InlineData("osirion-html-renderer.min.css")]
    public void HtmlRenderer_CodeHeaderAndCopyButton_UseOnePairOfBlockColors(string file)
    {
        // Act
        var css = Squash(File.ReadAllText(CoreCss(file)));

        // Assert: one pair, from the palette tokens, with a dark fallback that matches the Okaidia code theme
        css.ShouldContain("--osirion-code-block-background:var(--osirion-code-background,var(--osirion-syntax-background-color,#151b23));");
        css.ShouldContain("--osirion-code-block-text:var(--osirion-code-text,#f8f8f2);");
        System.Text.RegularExpressions.Regex.Matches(css, @"osirion-code-header\{[^}]*\}")[0].Value
            .ShouldContain("background-color:var(--osirion-code-block-background)");
        var button = System.Text.RegularExpressions.Regex.Matches(css, @"osirion-copy-button\{[^}]*\}")[0].Value;
        button.ShouldContain(";color:var(--osirion-code-block-text);");
        button.ShouldNotContain("#f8f8f2");
    }

    // Fails before the fix: the renderer's text link rules also reached the links inside an embedded demo, so a
    // component's own link color sat on the renderer's tinted link background (4.35:1 for the OsirionContentNotFound
    // suggestions on its centered panel) and a demo did not look as the component does on a page of its own.
    [Theory]
    [InlineData("osirion-html-renderer.css")]
    [InlineData("osirion-html-renderer.min.css")]
    public void HtmlRenderer_TextLinkRules_LeaveEmbeddedDemosAlone(string file)
    {
        // Act
        var css = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(CoreCss(file)), @"\s+", " ");
        var linkSelectors = System.Text.RegularExpressions.Regex.Matches(
                css,
                LinkSelectorPattern)
            .Select(match => match.Value)
            .ToList();

        // Assert
        linkSelectors.Count.ShouldBe(15);
        linkSelectors.ShouldAllBe(selector => selector.Contains(":not(.osirion-content-demo-preview a)", StringComparison.Ordinal));
    }

    // Fails before the fix: the text link selector weighed two classes and seven elements, so a site theme rule such as
    // ".prose a" lost to it and could restyle content links only with !important.
    [Theory]
    [InlineData("osirion-html-renderer.css")]
    [InlineData("osirion-html-renderer.min.css")]
    public void HtmlRenderer_TextLinkRules_WeighOneClassSoSiteThemesCanRestyleThem(string file)
    {
        // Act
        var css = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(CoreCss(file)), @"\s+", " ");

        // Assert: the link part sits in :where(), so only the renderer class counts
        System.Text.RegularExpressions.Regex.IsMatch(css, @"\.osirion-html-renderer a[:\s{]").ShouldBeFalse();
        System.Text.RegularExpressions.Regex.Matches(css, LinkSelectorPattern).Count.ShouldBeGreaterThan(0);
    }

    // Fails before the fix: printing drew a black box around every link with five !important declarations, which also
    // overrode any print rule of the site, and hid where an external link led.
    [Theory]
    [InlineData("osirion-html-renderer.css")]
    [InlineData("osirion-html-renderer.min.css")]
    public void HtmlRenderer_PrintRules_UseNoImportantAndPrintExternalAddresses(string file)
    {
        // Act
        var css = Squash(File.ReadAllText(CoreCss(file)));
        var printBlocks = System.Text.RegularExpressions.Regex.Matches(css, @"@mediaprint\{(?:[^{}]|\{[^{}]*\})*\}")
            .Select(match => match.Value)
            .ToList();
        var linkPrint = printBlocks.Single(block => block.Contains(":where(a", StringComparison.Ordinal));

        // Assert: no !important and no box; the external address follows the link, after the screen rules
        printBlocks.ShouldAllBe(block => !block.Contains("!important", StringComparison.Ordinal));
        linkPrint.ShouldNotContain("border");
        linkPrint.ShouldContain("content:\"(\"attr(href)\")\"");
        css.IndexOf(linkPrint, StringComparison.Ordinal).ShouldBeGreaterThan(css.LastIndexOf("::after{content:\"\"", StringComparison.Ordinal));
        css.EndsWith(linkPrint, StringComparison.Ordinal).ShouldBeTrue();
    }

    // The accessible code theme is part of the renderer's stylesheet, in place of the inline style element it wrote.
    // :where() keeps each rule at the specificity it had there, so the palettes' code rules still outrank it.
    [Theory]
    [InlineData("osirion-html-renderer.css")]
    [InlineData("osirion-html-renderer.min.css")]
    public void HtmlRenderer_AccessibleCodeTheme_IsInTheStylesheetUnderTheThemeClass(string file)
    {
        // Act
        var css = Squash(File.ReadAllText(CoreCss(file)));

        // Assert
        css.ShouldContain(":where(.osirion-wcag-theme)pre[class*=");
        css.ShouldContain(":where(.osirion-wcag-theme).token.keyword{color:#8be9fd");
        css.ShouldContain(":where(.osirion-wcag-theme).token.comment,:where(.osirion-wcag-theme).token.prolog,:where(.osirion-wcag-theme).token.doctype,:where(.osirion-wcag-theme).token.cdata{color:#8b949e");
    }

    /// <summary>A content link selector: the link part inside :where(), with its :is() and :not() arguments.</summary>
    private const string LinkSelectorPattern = @"\.osirion-html-renderer :where\(a(?:::?[\w-]+(?:\((?:[^()]|\([^()]*\))*\))?)+\)";

    // Fails before the fix: two seconds after a copy the script set the label to "Copy code to clipboard", so a screen
    // reader lost the language the markup gave it ("Copy C# code to clipboard").
    [Theory]
    [InlineData("osirion-copy-button.js")]
    [InlineData("osirion-copy-button.es5.js")]
    [InlineData("osirion-copy-button.es5.min.js")]
    public void CopyButtonScript_RestoresTheLabelItFound(string file)
    {
        // Act: the renderer loads the es5 minified build, so all three builds are held to it
        var script = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(CoreCss(file)))!, "js", file));

        // Assert
        script.ShouldContain("getAttribute(", Case.Sensitive);
        script.ShouldContain("aria-label", Case.Sensitive);
        script.ShouldNotContain("Copy code to clipboard");
    }

    // Fails before the fix: the scoped stylesheet held a full copy of the content typography (headings, lists, tables,
    // code blocks, the dark theme variables). Scoped CSS adds the component's attribute to the last part of every
    // selector, and the rendered content is a MarkupString without it, so none of those rules could ever match; the
    // typography that pages show comes from wwwroot/css/osirion-html-renderer.css, which the component links.
    [Fact]
    public void HtmlRenderer_ScopedStylesheet_HoldsOnlyRulesThatCanMatch()
    {
        // Act
        var css = System.Text.RegularExpressions.Regex.Replace(
            File.ReadAllText(ComponentCss("Rendering", "OsirionHtmlRenderer.razor.css")),
            @"/\*.*?\*/",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.Singleline);
        var selectors = System.Text.RegularExpressions.Regex.Matches(css, @"([^{}]+)\{")
            .Select(match => System.Text.RegularExpressions.Regex.Replace(match.Groups[1].Value, @"\s+", " ").Trim())
            .ToList();

        // Assert: the wrapper the component renders, or content reached through ::deep
        selectors.ShouldNotBeEmpty();
        selectors.ShouldAllBe(selector => selector == ".osirion-html-renderer" || selector.Contains("::deep", StringComparison.Ordinal));
    }

    // Fails before the fix: the social link animated every property ("transition: all"), its outline among them, so
    // for the first 0.2 seconds of keyboard focus the ring grew out of the unfocused outline (3px in the text color)
    // instead of showing the focus ring tokens.
    [Fact]
    public void Footer_Transitions_LeaveTheFocusRingOut()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Layout", "OsirionFooter.razor.css")));
        var transitions = System.Text.RegularExpressions.Regex.Matches(css, @"transition:([^;}]*)")
            .Select(match => match.Groups[1].Value)
            .ToList();

        // Assert
        transitions.ShouldNotBeEmpty();
        transitions.ShouldAllBe(value => !value.StartsWith("all", StringComparison.Ordinal) && !value.Contains(",all", StringComparison.Ordinal) && !value.Contains("outline", StringComparison.Ordinal));
    }

    // Fails before the fix: below 768px the bottom row was centered with no way for a site to change it.
    [Fact]
    public void Footer_BottomRowOnPhones_AlignsFromTheBottomAlignProperty()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Layout", "OsirionFooter.razor.css")));
        var phones = css[css.IndexOf("@media(max-width:767.98px){", StringComparison.Ordinal)..];

        // Assert: centered stays the default
        phones.ShouldContain(".osirion-footer-bottom-content{flex-direction:column;align-items:var(--osirion-footer-bottom-align,center);text-align:var(--osirion-footer-bottom-align,center);}");
        phones.ShouldContain(".osirion-footer-bottom-links{justify-content:var(--osirion-footer-bottom-align,center);");
    }

    // Fails before the fix: on phones the bottom row shrank to its content and stayed centered (the container centers
    // its children), so start and end only moved the text inside a centered box; and the legal links, a sibling of the
    // padded copyright, touched the screen edge. Measured in Edge at 375px.
    [Fact]
    public void Footer_BottomRowOnPhones_SpansTheFooterWithTheLinksPaddedLikeTheCopyright()
    {
        // Act
        var css = Squash(File.ReadAllText(ComponentCss("Layout", "OsirionFooter.razor.css")));
        var phones = css[css.IndexOf("@media(max-width:767.98px){", StringComparison.Ordinal)..];

        // Assert
        phones.ShouldContain(".osirion-footer-bottom{justify-self:stretch;}");
        phones.ShouldContain(".osirion-footer-bottom-links{justify-content:var(--osirion-footer-bottom-align,center);padding:0var(--osirion-spacing-4,1rem);}");
    }

    private static string Squash(string css) => System.Text.RegularExpressions.Regex.Replace(
        System.Text.RegularExpressions.Regex.Replace(css, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline),
        @"\s+",
        string.Empty);

    private static string ComponentCss(string folder, string file) =>
        Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(CoreCss(file))!)!, "..", "Components", folder, file);

    private static string CoreCss(string file)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return Path.Combine(directory.FullName, "src", "Osirion.Blazor.Core", "wwwroot", "css", file);
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }
}
