using System.Text.RegularExpressions;
using Osirion.Blazor.Palettes;
using Shouldly;

namespace Osirion.Blazor.Theming.Tests.Palettes;

/// <summary>
/// Bootstrap compiles its primary buttons' colors from Sass into --bs-btn-* variables that no Osirion token reaches,
/// so the Bootstrap adapter sets them from the palette's action colors while a palette is active.
/// </summary>
public partial class BootstrapPaletteButtonTests
{
    private const string PrimarySelector = ":where(:root[data-osirion-palette]) .btn.btn-primary";
    private const string OutlineSelector = ":where(:root[data-osirion-palette]) .btn.btn-outline-primary";

    private static readonly IReadOnlyList<PaletteVariant> FullVariants = PaletteStylesheet.BuildVariants(new List<string>())
        .Where(variant => variant.Tokens.Any(token => token.Key == "action-primary"))
        .ToList();

    private static readonly Dictionary<string, string> PrimaryButton = new(StringComparer.Ordinal)
    {
        ["color"] = "action-primary-text",
        ["bg"] = "action-primary",
        ["border-color"] = "action-primary",
        ["hover-color"] = "action-primary-text",
        ["hover-bg"] = "action-primary-hover",
        ["hover-border-color"] = "action-primary-hover",
        ["active-color"] = "action-primary-text",
        ["active-bg"] = "action-primary-active",
        ["active-border-color"] = "action-primary-active",
        ["disabled-color"] = "action-primary-text",
        ["disabled-bg"] = "action-primary",
        ["disabled-border-color"] = "action-primary",
    };

    // The resting text sits on the page, so it takes the link color, which every palette holds to its text target on
    // the page backgrounds; the action color reads only 3.64:1 to 4.09:1 there in three dark palettes.
    private static readonly Dictionary<string, string> OutlineButton = new(StringComparer.Ordinal)
    {
        ["color"] = "link-color",
        ["border-color"] = "action-primary",
        ["hover-color"] = "action-primary-text",
        ["hover-bg"] = "action-primary-hover",
        ["hover-border-color"] = "action-primary-hover",
        ["active-color"] = "action-primary-text",
        ["active-bg"] = "action-primary-active",
        ["active-border-color"] = "action-primary-active",
        ["disabled-color"] = "link-color",
        ["disabled-border-color"] = "action-primary",
    };

    public static TheoryData<string, string> FullVariantNames()
    {
        var data = new TheoryData<string, string>();
        foreach (var variant in FullVariants)
        {
            data.Add(variant.Palette, variant.Mode);
        }

        return data;
    }

    // Fails before the fix: the adapter had no button rule, so the buttons kept the color Bootstrap compiled from
    // the site's $primary under every palette.
    [Theory]
    [InlineData("bootstrap-adapter.css")]
    [InlineData("bootstrap-adapter.min.css")]
    public void Adapter_PrimaryButton_TakesThePaletteActionColors(string file)
    {
        // Arrange
        var css = File.ReadAllText(RepositoryFiles.ThemingCss("adapters/" + file));

        // Act
        var button = ButtonVariables(css, PrimarySelector);

        // Assert
        button.ShouldBe(PrimaryButton, ignoreOrder: true);
    }

    [Theory]
    [InlineData("bootstrap-adapter.css")]
    [InlineData("bootstrap-adapter.min.css")]
    public void Adapter_OutlinePrimaryButton_TakesThePaletteActionColors(string file)
    {
        // Arrange
        var css = File.ReadAllText(RepositoryFiles.ThemingCss("adapters/" + file));

        // Act
        var button = ButtonVariables(css, OutlineSelector);

        // Assert
        button.ShouldBe(OutlineButton, ignoreOrder: true);
    }

    // Bootstrap's focus ring is the button color at half strength; the palette names its own focus color.
    [Theory]
    [InlineData("bootstrap-adapter.css", PrimarySelector)]
    [InlineData("bootstrap-adapter.min.css", PrimarySelector)]
    [InlineData("bootstrap-adapter.css", OutlineSelector)]
    [InlineData("bootstrap-adapter.min.css", OutlineSelector)]
    public void Adapter_ButtonFocusRing_UsesThePaletteFocusColor(string file, string selector)
    {
        // Arrange
        var css = File.ReadAllText(RepositoryFiles.ThemingCss("adapters/" + file));

        // Act
        var shadow = Declarations(css, selector)["--bs-btn-focus-box-shadow"];

        // Assert
        shadow.ShouldContain("var(--osirion-action-primary-focus)");
    }

    [Theory]
    [MemberData(nameof(FullVariantNames))]
    public void PrimaryButtonText_ReachesThePaletteTextTarget(string palette, string mode)
    {
        // Arrange
        var variant = FullVariants.Single(v => v.Palette == palette && v.Mode == mode);
        var button = ButtonVariables(File.ReadAllText(RepositoryFiles.ThemingCss("adapters/bootstrap-adapter.min.css")), PrimarySelector);

        // Act
        var failures = Failures(variant, [
            (button["color"], button["bg"]),
            (button["hover-color"], button["hover-bg"]),
            (button["active-color"], button["active-bg"]),
        ]);

        // Assert
        failures.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(FullVariantNames))]
    public void OutlinePrimaryButtonText_ReachesThePaletteTextTarget(string palette, string mode)
    {
        // Arrange
        var variant = FullVariants.Single(v => v.Palette == palette && v.Mode == mode);
        var button = ButtonVariables(File.ReadAllText(RepositoryFiles.ThemingCss("adapters/bootstrap-adapter.min.css")), OutlineSelector);

        // Act: at rest the outline button has no fill, so its text sits on the page
        var failures = Failures(variant, [
            (button["color"], "background-primary"),
            (button["color"], "background-secondary"),
            (button["hover-color"], button["hover-bg"]),
            (button["active-color"], button["active-bg"]),
        ]);

        // Assert
        failures.ShouldBeEmpty();
    }

    private static List<string> Failures(PaletteVariant variant, IEnumerable<(string Foreground, string Background)> pairs)
    {
        var tokens = variant.Tokens.ToDictionary(token => token.Key, token => token.Value, StringComparer.Ordinal);

        return pairs
            .Select(pair => (pair, ratio: Rgb.Contrast(Rgb.Parse(tokens[pair.Foreground]), Rgb.Parse(tokens[pair.Background]))))
            .Where(result => result.ratio < variant.TextContrast)
            .Select(result => $"{result.pair.Foreground} on {result.pair.Background}: {result.ratio:0.00} < {variant.TextContrast}")
            .ToList();
    }

    // The button's color variables, each named without its --bs-btn- prefix, mapped to the Osirion token it reads.
    private static Dictionary<string, string> ButtonVariables(string css, string selector) =>
        Declarations(css, selector)
            .Where(declaration => declaration.Key != "--bs-btn-focus-box-shadow")
            .ToDictionary(
                declaration => declaration.Key["--bs-btn-".Length..],
                declaration => TokenReference().Match(declaration.Value) is { Success: true } reference
                    ? reference.Groups["name"].Value
                    : declaration.Value,
                StringComparer.Ordinal);

    private static Dictionary<string, string> Declarations(string css, string selector)
    {
        var rule = Rule().Matches(Comment().Replace(css, string.Empty))
            .SingleOrDefault(match => match.Groups["selector"].Value.Trim() == selector);
        rule.ShouldNotBeNull($"no rule for {selector}");

        return Declaration().Matches(rule.Groups["body"].Value)
            .ToDictionary(match => match.Groups["name"].Value, match => match.Groups["value"].Value.Trim(), StringComparer.Ordinal);
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")]
    private static partial Regex Rule();

    [GeneratedRegex(@"(?<name>--bs-btn-[\w-]+)\s*:\s*(?<value>[^;}]+)")]
    private static partial Regex Declaration();

    [GeneratedRegex(@"^var\(--osirion-(?<name>[\w-]+)\)$")]
    private static partial Regex TokenReference();
}
