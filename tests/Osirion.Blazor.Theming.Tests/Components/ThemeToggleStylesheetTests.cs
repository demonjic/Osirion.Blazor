using System.Text.RegularExpressions;
using Osirion.Blazor.Palettes;
using Osirion.Blazor.Theming.Tests.Palettes;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Components;

/// <summary>Checks the focus ring and spacing rules of ThemeToggle.razor.css.</summary>
public partial class ThemeToggleStylesheetTests
{
    private const string FocusRing =
        "outline:var(--osirion-focus-ring-width,2px)solidvar(--osirion-focus-ring-color,var(--osirion-primary-color,#2563eb));outline-offset:var(--osirion-focus-ring-offset,3px);";

    // Fails before the fix: the ring was "2px solid --osirion-primary-color !important", so a site that set
    // --osirion-focus-ring-color and --osirion-focus-ring-width, as every other component reads them, got another ring
    // here and could not change it without !important of its own.
    [Fact]
    public void FocusVisible_DrawsTheFocusRingTokens()
    {
        // Act
        var css = Stylesheet();

        // Assert
        css.ShouldContain(".theme-toggle-btn:focus-visible{" + FocusRing + "}");
    }

    // Fails before the fix: the token ring was the only focus indicator, and the library's default light token
    // (3px of rgba(59, 130, 246, 0.5)) reads about 1.8:1 on a white bar, the Bootstrap adapter's about 1.4:1. The
    // button's own 2px ring (its ::before, in the primary color) now shows at full strength on keyboard focus too.
    [Fact]
    public void FocusVisible_AlsoDrawsTheOpaquePrimaryRing()
    {
        // Act
        var css = Stylesheet();

        // Assert
        css.ShouldContain(".theme-toggle-btn:focus-visible::before{opacity:1;}");
        css.ShouldContain("background:linear-gradient(45deg,var(--osirion-primary-color,#2563eb),var(--osirion-primary-color-hover,#1d4ed8));");
    }

    // The primary ring sits on the bar around the button, so it needs the 3:1 a focus indicator asks for there.
    [Fact]
    public void PrimaryRing_ReachesNonTextContrastInBothComponentThemes()
    {
        // Arrange
        var themes = ComponentThemeColors().Matches(Stylesheet()).ToList();

        // Act
        var ratios = themes
            .Select(theme => Rgb.Contrast(Rgb.Parse(theme.Groups["primary"].Value), Rgb.Parse(theme.Groups["background"].Value)))
            .ToList();

        // Assert
        ratios.Count.ShouldBe(2);
        ratios.ShouldAllBe(ratio => ratio >= 3.0);
    }

    [Fact]
    public void Outline_IsNeverImportant()
    {
        // Act
        var css = Stylesheet();

        // Assert
        OutlineDeclaration().Matches(css).Select(match => match.Value).ShouldAllBe(declaration => !declaration.Contains("!important", StringComparison.Ordinal));
    }

    // Fails before the fix: "transition: all" animated the outline too, so in Edge a focused toggle first showed its
    // ring in the button's text color (the unfocused outline's color) and faded into the focus ring over 0.3 seconds.
    [Fact]
    public void Button_TransitionLeavesTheFocusRingOut()
    {
        // Act
        var body = ButtonRule().Match(Stylesheet()).Groups["body"].Value;

        // Assert
        var transition = Regex.Match(body, "transition:(?<value>[^;]*)").Groups["value"].Value;
        transition.ShouldNotBeEmpty();
        transition.ShouldNotStartWith("all");
        transition.ShouldNotContain("outline");
    }

    // Fails before the fix: the switcher padded itself by a fixed 10px, which a container (the menu's drawer) could
    // not take away to line the toggle up with its items.
    [Fact]
    public void Switcher_PaddingComesFromTheToken()
    {
        // Act
        var css = Stylesheet();

        // Assert
        css.ShouldContain(".theme-switcher{padding:var(--osirion-theme-toggle-padding,10px);}");
    }

    private static string Stylesheet() =>
        WhitespaceRun().Replace(CssComment().Replace(File.ReadAllText(RepositoryFiles.Theming("Components", "ThemeToggle.razor.css")), string.Empty), string.Empty);

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();

    [GeneratedRegex(@"outline(-[a-z]+)?:[^;}]*")]
    private static partial Regex OutlineDeclaration();

    [GeneratedRegex(@"(?<![\w\]-])\.theme-toggle-btn\{(?<body>[^}]*)\}")]
    private static partial Regex ButtonRule();

    [GeneratedRegex(@"--osirion-background-primary:(?<background>#[0-9a-f]{6})!important;[^}]*--osirion-primary-color:(?<primary>#[0-9a-f]{6})!important;")]
    private static partial Regex ComponentThemeColors();
}
