using System.Text.RegularExpressions;
using Osirion.Blazor.Palettes;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Palettes;

/// <summary>The Osirion palette is the light and dark theme files themselves; they are held to the same pairs.</summary>
public partial class OsirionThemeContrastTests
{
    // The theme files leave the menu tokens to Menu, which derives them from the link colour: without a palette its
    // active item reaches 4.36:1 in the light theme. The Osirion palette points them at the nav-active pair checked here.
    private static readonly string[] Excluded = ["menu-accent"];

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void ThemeFile_ReachesWcagAaForTextAndFocus(string mode)
    {
        // Arrange
        var tokens = ReadThemeBlock(mode);
        var pairs = PaletteBuilder.ContrastPairs(4.5)
            .Where(pair => !pair.Foreground.StartsWith("code-", StringComparison.Ordinal))
            .Where(pair => !Excluded.Contains(pair.Foreground))
            .ToList();

        // Act
        var failures = pairs
            .Select(pair => (pair, ratio: Rgb.Contrast(Resolve(tokens, pair.Foreground), Resolve(tokens, pair.Background))))
            .Where(result => result.ratio < result.pair.Minimum)
            .Select(result => $"{result.pair.Foreground} on {result.pair.Background}: {result.ratio:0.00} < {result.pair.Minimum}")
            .ToList();

        // Assert
        pairs.Count.ShouldBeGreaterThan(15);
        failures.ShouldBeEmpty();
    }

    /// <summary>Reads the custom properties of the theme's main block in the shipped stylesheet.</summary>
    private static Dictionary<string, string> ReadThemeBlock(string mode)
    {
        var css = File.ReadAllText(RepositoryFiles.ThemingCss("index.min.css"));
        var start = css.IndexOf($":root[data-osirion-theme={mode}]", StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        var open = css.IndexOf('{', start);
        var body = css[(open + 1)..css.IndexOf('}', open)];

        return Declaration().Matches(body).ToDictionary(match => match.Groups["name"].Value, match => match.Groups["value"].Value.Trim());
    }

    private static Rgb Resolve(Dictionary<string, string> tokens, string token)
    {
        var value = tokens[token];
        for (var depth = 0; depth < 10 && Reference().Match(value) is { Success: true } reference; depth++)
        {
            value = tokens[reference.Groups["name"].Value];
        }

        return Rgb.Parse(value);
    }

    [GeneratedRegex(@"--osirion-(?<name>[\w-]+):(?<value>[^;}]+)")]
    private static partial Regex Declaration();

    [GeneratedRegex(@"^var\(--osirion-(?<name>[\w-]+)\)$")]
    private static partial Regex Reference();
}
