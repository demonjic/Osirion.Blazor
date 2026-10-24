using System.Globalization;
using System.Text.RegularExpressions;
using Osirion.Blazor.Palettes;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Palettes;

/// <summary>
/// WCAG 2.2 success criterion 1.4.11 (Non-text Contrast) asks 3:1 for the visual information that identifies a
/// component's state, and the focus ring is that information for a keyboard user. The ring is checked as the browser
/// draws it: a translucent color is composited over the surface behind it, and a token that points at another token is
/// followed the way the cascade resolves it.
/// </summary>
public partial class FocusRingContrastTests
{
    private const double NonTextContrast = 3;

    private static readonly string[] Surfaces = ["osirion-background-primary", "osirion-background-secondary", "osirion-background-tertiary", "osirion-card-background"];

    // Fails before the fix: the light theme's ring, rgba(59,130,246,.5), composited to 1.8:1 on white.
    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void Theme_FocusRing_ReachesNonTextContrastOnEverySurface(string mode)
    {
        // Arrange
        var tokens = ThemeTokens(mode);

        // Act & Assert
        RingFailures(tokens).ShouldBeEmpty();
    }

    // Fails before the fix: a Bootstrap page drew the ring at 25% opacity (Bootstrap's own focus ring color), MudBlazor
    // at 30% and FluentUI in the dark theme at 70%.
    [Theory]
    [InlineData("bootstrap", "light")]
    [InlineData("bootstrap", "dark")]
    [InlineData("fluentui", "light")]
    [InlineData("fluentui", "dark")]
    [InlineData("mudblazor", "light")]
    [InlineData("mudblazor", "dark")]
    [InlineData("radzen", "light")]
    [InlineData("radzen", "dark")]
    public void Adapter_FocusRing_ReachesNonTextContrastOnEverySurface(string adapter, string mode)
    {
        // Arrange: the theme gives the tokens, the adapter replaces the ones it declares
        var tokens = ThemeTokens(mode);
        var adapterRule = Rules(RepositoryFiles.ThemingCss($"adapters/{adapter}-adapter.min.css"))
            .First(rule => rule.Selector.Contains("dark", StringComparison.Ordinal) == (mode == "dark")
                && rule.Tokens.ContainsKey("osirion-background-primary"));
        foreach (var (name, value) in adapterRule.Tokens)
        {
            tokens[name] = value;
        }

        // Act & Assert
        RingFailures(tokens).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("light")]
    [InlineData("dark")]
    public void HighContrastTheme_FocusRing_ReachesNonTextContrast(string mode)
    {
        // Arrange: the prefers-contrast rule only replaces the ring, over the theme's own surfaces
        var tokens = ThemeTokens(mode);
        var rule = Rules(RepositoryFiles.ThemingCss("index.min.css"))
            .First(rule => rule.Selector.Contains("dark", StringComparison.Ordinal) == (mode == "dark")
                && rule.Tokens.ContainsKey("osirion-focus-ring-color")
                && !rule.Tokens.ContainsKey("osirion-background-primary"));
        tokens["osirion-focus-ring-color"] = rule.Tokens["osirion-focus-ring-color"];

        // Act & Assert
        RingFailures(tokens).ShouldBeEmpty();
    }

    // Fails before the fix: a page using only Osirion.Blazor.Core drew a translucent ring on white.
    [Fact]
    public void CoreDefaults_FocusRing_ReachesNonTextContrastOnEverySurface()
    {
        // Arrange
        var tokens = Rules(CoreCss("index.min.css"))
            .First(rule => rule.Selector == ":root" && rule.Tokens.ContainsKey("osirion-focus-ring-color")
                && rule.Tokens.ContainsKey("osirion-background-primary")).Tokens;

        // Act & Assert
        RingFailures(tokens).ShouldBeEmpty();
    }

    // Fails before the fix: the dark rule of the accessibility utilities drew rgba(96,165,250,.7).
    [Fact]
    public void CoreAccessibilityUtilities_DarkFocusRing_ReachesNonTextContrast()
    {
        // Arrange: the rule replaces the ring over the dark theme's surfaces
        var tokens = ThemeTokens("dark");
        var rule = Rules(CoreCss("utilities/a11y.min.css"))
            .First(rule => rule.Tokens.ContainsKey("osirion-focus-ring-color") && !rule.Tokens.ContainsKey("osirion-focus-ring-width")
                && rule.Selector.Contains("dark", StringComparison.Ordinal));
        tokens["osirion-focus-ring-color"] = rule.Tokens["osirion-focus-ring-color"];

        // Act & Assert
        RingFailures(tokens).ShouldBeEmpty();
    }

    // Fails before the fix: the base outline fell back to rgba(59,130,246,.5) when no token was defined.
    [Fact]
    public void CoreBaseOutline_FallbackColor_ReachesNonTextContrastOnWhite()
    {
        // Arrange
        var css = File.ReadAllText(CoreCss("index.min.css"));
        var fallback = BaseOutlineFallback().Match(css);
        fallback.Success.ShouldBeTrue();

        // Act
        var ratio = Rgb.Contrast(ParseColor(fallback.Groups["color"].Value, Rgb.Parse("#ffffff")), Rgb.Parse("#ffffff"));

        // Assert
        ratio.ShouldBeGreaterThanOrEqualTo(NonTextContrast, fallback.Groups["color"].Value);
    }

    [Fact]
    public void Resolver_CompositesATranslucentRingOverItsSurface()
    {
        // Arrange: 50% of #3b82f6 over white is #9dc0fa, 1.8:1
        var tokens = new Dictionary<string, string>(StringComparer.Ordinal) { ["osirion-ring"] = "rgba(59,130,246,.5)" };

        // Act
        var ring = Resolve(tokens, "osirion-ring", Rgb.Parse("#ffffff"));

        // Assert
        var color = ring ?? throw new InvalidOperationException("The ring does not resolve.");
        color.ToHex().ShouldBe("#9dc0fb");
        Rgb.Contrast(color, Rgb.Parse("#ffffff")).ShouldBeInRange(1.7, 1.9);
    }

    // Every surface the ring is drawn on that the tokens can name; the card surface of a framework that defines it with
    // a variable the page supplies (Bootstrap's --bs-card-bg) is left out.
    private static List<string> RingFailures(Dictionary<string, string> tokens)
    {
        var failures = new List<string>();
        foreach (var surface in Surfaces)
        {
            if (Resolve(tokens, surface, null) is not { } background)
            {
                continue;
            }

            var ring = Resolve(tokens, "osirion-focus-ring-color", background)
                ?? throw new InvalidOperationException("The focus ring color does not resolve.");
            var ratio = Rgb.Contrast(ring, background);
            if (ratio < NonTextContrast)
            {
                failures.Add($"focus ring {ring.ToHex()} on {surface} {background.ToHex()}: {ratio:0.00} < {NonTextContrast}");
            }
        }

        return failures;
    }

    private static Dictionary<string, string> ThemeTokens(string mode) =>
        new(Rules(RepositoryFiles.ThemingCss("index.min.css"))
            .First(rule => rule.Selector.Contains($":root[data-osirion-theme={mode}]", StringComparison.Ordinal)
                && rule.Tokens.ContainsKey("osirion-background-primary")).Tokens, StringComparer.Ordinal);

    private static string CoreCss(string file) => Path.Combine(RepositoryFiles.Source(), "Osirion.Blazor.Core", "wwwroot", "css", file);

    private static IEnumerable<(string Selector, Dictionary<string, string> Tokens)> Rules(string file) =>
        RulePattern().Matches(File.ReadAllText(file)).Select(rule => (
            rule.Groups["selector"].Value,
            DeclarationPattern().Matches(rule.Groups["body"].Value)
                .ToDictionary(declaration => declaration.Groups["name"].Value, declaration => declaration.Groups["value"].Value.Trim(), StringComparer.Ordinal)));

    // Follows var() references (using a fallback for a variable the page supplies) to a color, composited over the
    // backdrop when it is translucent. Returns null when a variable has neither a value nor a fallback.
    private static Rgb? Resolve(Dictionary<string, string> tokens, string name, Rgb? backdrop)
    {
        if (!tokens.TryGetValue(name, out var value))
        {
            return null;
        }

        for (var depth = 0; depth < 12; depth++)
        {
            if (VarReference().Match(value) is not { Success: true } reference)
            {
                return ParseColor(value, backdrop ?? Rgb.Parse("#ffffff"));
            }

            var target = reference.Groups["name"].Value;
            if (tokens.TryGetValue(target, out var next))
            {
                value = next;
            }
            else if (reference.Groups["fallback"].Success)
            {
                value = reference.Groups["fallback"].Value.Trim();
            }
            else
            {
                return null;
            }
        }

        throw new InvalidOperationException($"{name} does not resolve in 12 steps.");
    }

    private static Rgb ParseColor(string value, Rgb backdrop)
    {
        if (RgbFunction().Match(value) is { Success: true } function)
        {
            double Channel(string group) => double.Parse(function.Groups[group].Value, CultureInfo.InvariantCulture);
            var alpha = function.Groups["a"].Success ? Channel("a") : 1;
            var color = new Rgb(Channel("r") / 255, Channel("g") / 255, Channel("b") / 255);
            return new Rgb(
                color.R * alpha + backdrop.R * (1 - alpha),
                color.G * alpha + backdrop.G * (1 - alpha),
                color.B * alpha + backdrop.B * (1 - alpha));
        }

        return Rgb.Parse(value, backdrop);
    }

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex RulePattern();

    [GeneratedRegex(@"--(?<name>[\w-]+):(?<value>[^;}]+)")]
    private static partial Regex DeclarationPattern();

    [GeneratedRegex(@"^var\(--(?<name>[\w-]+)(?:,(?<fallback>.+))?\)$")]
    private static partial Regex VarReference();

    [GeneratedRegex(@"^rgba?\(\s*(?<r>[\d.]+)\s*,\s*(?<g>[\d.]+)\s*,\s*(?<b>[\d.]+)\s*(?:,\s*(?<a>[\d.]+)\s*)?\)$")]
    private static partial Regex RgbFunction();

    [GeneratedRegex(@"outline:var\(--osirion-focus-ring-width,3px\) solid var\(--osirion-focus-ring-color,(?<color>#[0-9a-fA-F]{3,6}|rgba?\([^)]*\))\)")]
    private static partial Regex BaseOutlineFallback();
}
