using System.Text.RegularExpressions;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Palettes;

/// <summary>Component styles must read tokens the themes define, or their fallback shows in both themes.</summary>
public partial class ThemeTokenTests
{
    // Fails before the fix: eleven components read --osirion-background-active, which no theme defined, so its light
    // fallback (#e0f2fe) stayed in the dark theme under light text (DocumentPage's active item: 2.21:1).
    // Fails before the fix: the high-contrast dark block set the label to var(--osirion-color-black), which the dark
    // theme's reversed scale resolves to white: 3.68:1 on the bright blue, and 3.06:1 on the pressed color.
    [Theory]
    [InlineData("index.css")]
    [InlineData("index.min.css")]
    [InlineData("themes/dark-theme.css")]
    [InlineData("themes/dark-theme.min.css")]
    public void HighContrastDarkBlock_WritesTheButtonLabelInBlack(string file)
    {
        var css = System.Text.RegularExpressions.Regex.Replace(File.ReadAllText(RepositoryFiles.ThemingCss(file)), @"\s+", string.Empty);
        var dark = System.Text.RegularExpressions.Regex.Match(css, @"@media\(prefers-contrast:more\)\{[^{]*data-theme=""?dark""?[^{]*\{(?<body>[^}]*)\}");

        dark.Success.ShouldBeTrue();
        dark.Groups["body"].Value.ShouldContain("--osirion-action-primary-text:#000000");
    }

    [Fact]
    public void ComponentStyles_DoNotReadTheUndefinedActiveBackgroundToken()
    {
        // Act
        var offenders = ComponentStylesheets()
            .Where(file => File.ReadAllText(file).Contains("var(--osirion-background-active", StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(RepositoryFiles.Source(), file))
            .ToList();

        // Assert
        offenders.ShouldBeEmpty();
    }

    // Fails before the fix: hover states read --osirion-background-hover, which no theme defined, so a hovered item in
    // the dark theme turned light grey under light text.
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void ThemeFile_DefinesTheHoverBackgroundToken(string mode)
    {
        // Act
        var css = File.ReadAllText(RepositoryFiles.ThemingCss("index.min.css"));
        var start = css.IndexOf($":root[data-osirion-theme={mode}]", StringComparison.Ordinal);
        var body = css[css.IndexOf('{', start)..css.IndexOf('}', start)];

        // Assert
        HoverToken().IsMatch(body).ShouldBeTrue();
    }

    // Fails before the fix: 74 rules colored text with --osirion-action-primary, a button fill that the palettes and
    // adapters only hold to contrast under the button's own label. Under the Bootstrap adapter in the dark theme a read
    // more link read 2.83:1 and the not-found page's links 3.47:1. Text takes --osirion-link-color, which every theme
    // and palette keeps at 4.5:1 on the page.
    [Fact]
    public void Stylesheets_ColorTextWithTheLinkColorNotTheButtonColor()
    {
        // Act
        var offenders = Directory.EnumerateFiles(RepositoryFiles.Source(), "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".css", StringComparison.Ordinal) || file.EndsWith(".scss", StringComparison.Ordinal))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(file => ButtonColorText().Matches(File.ReadAllText(file))
                .Select(match => $"{Path.GetRelativePath(RepositoryFiles.Source(), file)}: {match.Value}"))
            .ToList();

        // Assert
        offenders.ShouldBeEmpty();
    }

    private static IEnumerable<string> ComponentStylesheets() =>
        Directory.EnumerateFiles(RepositoryFiles.Source(), "*.razor.css", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

    [GeneratedRegex(@"(?<![\w-])color:\s*var\(--osirion-action-primary(-hover)?(?=[,)])")]
    private static partial Regex ButtonColorText();

    [GeneratedRegex(@"--osirion-background-hover:var\(--osirion-nav-hover-background\)")]
    private static partial Regex HoverToken();
}
