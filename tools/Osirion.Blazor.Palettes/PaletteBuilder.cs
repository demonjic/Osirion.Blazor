namespace Osirion.Blazor.Palettes;

/// <summary>One palette variant as written to the stylesheet: the selectors it applies under and its tokens.</summary>
/// <param name="Palette">The palette id.</param>
/// <param name="Mode"><c>light</c> or <c>dark</c>.</param>
/// <param name="Selectors">The selectors the variant's tokens are declared under.</param>
/// <param name="Tokens">Token names without the <c>--osirion-</c> prefix, and their <c>#rrggbb</c> values (or <c>var()</c> references to theme tokens), in writing order.</param>
/// <param name="TextContrast">The contrast ratio the variant's text pairs are held to.</param>
internal sealed record PaletteVariant(
    string Palette,
    string Mode,
    IReadOnlyList<string> Selectors,
    IReadOnlyList<KeyValuePair<string, string>> Tokens,
    double TextContrast);

/// <summary>Two tokens whose colors are checked against each other, and the ratio they must reach.</summary>
/// <param name="Foreground">The foreground token, without the <c>--osirion-</c> prefix.</param>
/// <param name="Background">The background token, without the <c>--osirion-</c> prefix.</param>
/// <param name="Minimum">The minimum WCAG 2 contrast ratio.</param>
internal sealed record ContrastPair(string Foreground, string Background, double Minimum);

/// <summary>Turns a <see cref="PaletteSpec"/> into the full set of <c>--osirion-*</c> color tokens.</summary>
internal static class PaletteBuilder
{
    /// <summary>WCAG 2.2 1.4.11: the focus indicator and other user interface parts against what is next to them.</summary>
    public const double NonTextContrast = 3;

    private static readonly string[] Steps = ["50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950"];

    // Where each neutral step sits between the background (0) and the text color (1). Light themes start the
    // scale at the page background; dark themes, as Osirion's dark theme does, put step 50 below it.
    private static readonly double[] LightNeutral = [0.03, 0.06, 0.12, 0.2, 0.36, 0.5, 0.62, 0.75, 0.87, 1, 1.06];
    private static readonly double[] DarkNeutral = [-0.03, 0, 0.08, 0.16, 0.28, 0.42, 0.56, 0.7, 0.84, 1, 1.06];

    // Primary steps below 600 mix the accent into the background; steps above it mix it towards black on a light
    // page and towards white on a dark one, the direction Osirion's own scales use.
    private static readonly double[] PrimaryToBackground = [0.92, 0.84, 0.7, 0.5, 0.25, 0.1];
    private static readonly double[] PrimaryBeyond = [0.2, 0.4, 0.55, 0.7];

    /// <summary>The token pairs every palette variant must keep apart; text pairs use the variant's text target.</summary>
    public static IReadOnlyList<ContrastPair> ContrastPairs(double textContrast) =>
    [
        new("text-primary", "background-primary", textContrast),
        new("text-primary", "background-secondary", textContrast),
        new("text-primary", "background-tertiary", textContrast),
        new("text-secondary", "background-primary", textContrast),
        new("text-secondary", "background-secondary", textContrast),
        new("text-tertiary", "background-primary", textContrast),
        new("text-tertiary", "background-secondary", textContrast),
        new("link-color", "background-primary", textContrast),
        new("link-color", "background-secondary", textContrast),
        new("action-primary-text", "action-primary", textContrast),
        new("action-primary-text", "action-primary-hover", textContrast),
        new("action-primary-text", "action-primary-active", textContrast),
        new("action-secondary-text", "action-secondary", textContrast),
        new("action-secondary-text", "action-secondary-hover", textContrast),
        new("nav-active-text", "nav-active-background", textContrast),
        new("menu-accent", "menu-active-background", textContrast),
        new("status-success-text", "status-success-bg", textContrast),
        new("status-warning-text", "status-warning-bg", textContrast),
        new("status-danger-text", "status-danger-bg", textContrast),
        new("status-info-text", "status-info-bg", textContrast),
        new("input-text", "input-background", textContrast),
        new("border-color-focus", "background-primary", NonTextContrast),
        new("focus-ring-color", "background-primary", NonTextContrast),
        .. CodeContrastPairs(textContrast),
    ];

    /// <summary>The code color pairs: every syntax color against the code background.</summary>
    public static IReadOnlyList<ContrastPair> CodeContrastPairs(double textContrast) =>
        CodeTokens.Select(token => new ContrastPair(token, "code-background", textContrast)).ToList();

    private static readonly string[] CodeTokens =
    [
        "code-text", "code-comment", "code-keyword", "code-string", "code-number", "code-function", "code-type",
        "code-variable", "code-tag", "code-attribute", "code-regex", "code-inserted", "code-deleted",
    ];

    /// <summary>Builds a full palette variant. Each color moved to reach its contrast target is reported to <paramref name="adjustments"/>.</summary>
    public static PaletteVariant Build(PaletteSpec spec, ICollection<string> adjustments)
    {
        var background = Rgb.Parse(spec.Background);
        Rgb Color(string hex) => Rgb.Parse(hex, background);
        var dark = spec.Mode == "dark";
        var tokens = new Tokens(spec.Palette, spec.Mode, adjustments);

        var surface = Color(spec.Surface);
        var surfaceAlt = Color(spec.SurfaceAlt);
        var muted = Color(spec.Muted);
        var text = Color(spec.Text);
        var accent = Color(spec.Accent);
        var accentText = Color(spec.AccentText);

        AddNeutralScale(tokens, background, text, dark);
        AddPrimaryScale(tokens, background, accent, dark);

        tokens.Set("background-primary", background);
        tokens.Set("background-secondary", surface);
        tokens.Set("background-tertiary", surfaceAlt);
        tokens.Set("background-muted", muted);
        tokens.Set("background-inverted", text);

        var target = spec.TextContrast;
        var hardest = HarderBackground(text, background, surface, surfaceAlt);
        tokens.Set("text-primary", tokens.Readable("text-primary", text, hardest, target));
        tokens.Set("text-secondary", tokens.Readable("text-secondary", Color(spec.TextSecondary), HarderBackground(text, background, surface), target));
        tokens.Set("text-tertiary", tokens.Readable("text-tertiary", Color(spec.TextMuted), HarderBackground(text, background, surface), target));
        tokens.Set("text-inverted", background);
        tokens.Set("text-disabled", Color(spec.TextDisabled));

        tokens.Set("border-color", Color(spec.Border));
        tokens.Set("border-color-hover", Color(spec.BorderStrong));
        var focus = tokens.Readable("border-color-focus", Color(spec.Focus), background, NonTextContrast);
        tokens.Set("border-color-focus", focus);

        // The button color moves, not its label: a label is white or black, the button carries the palette's hue.
        var action = tokens.Readable("action-primary", accent, accentText, target);
        var actionHover = tokens.Readable("action-primary-hover", Color(spec.AccentHover), accentText, target);
        tokens.Set("action-primary", action);
        tokens.Set("action-primary-hover", actionHover);
        tokens.Set("action-primary-active", tokens.Readable("action-primary-active", Rgb.Mix(actionHover, accentText.Luminance > 0.5 ? new Rgb(0, 0, 0) : new Rgb(1, 1, 1), 0.12), accentText, target));
        tokens.Set("action-primary-text", accentText);
        tokens.Set("action-primary-focus", focus);
        tokens.Set("action-primary-disabled", Rgb.Mix(background, accent, 0.4));

        tokens.Set("action-secondary", surfaceAlt);
        tokens.Set("action-secondary-hover", muted);
        tokens.Set("action-secondary-active", Rgb.Mix(muted, text, 0.1));
        tokens.Set("action-secondary-text", tokens.Readable("action-secondary-text", text, HarderBackground(text, surfaceAlt, muted), target));
        tokens.Set("action-secondary-focus", focus);
        tokens.Set("action-secondary-disabled", surfaceAlt);

        var link = tokens.Readable("link-color", Color(spec.Link), HarderBackground(Color(spec.Link), background, surface), target);
        tokens.Set("link-color", link);
        tokens.Set("link-hover-color", link);
        tokens.Set("link-active-color", link);
        tokens.Set("link-visited-color", link);

        AddStatus(tokens, "success", Color(spec.Success), spec.SuccessBackground, background, target);
        AddStatus(tokens, "warning", Color(spec.Warning), spec.WarningBackground, background, target);
        AddStatus(tokens, "danger", Color(spec.Danger), spec.DangerBackground, background, target);
        AddStatus(tokens, "info", Color(spec.Info), spec.InfoBackground, background, target);

        var navActiveBackground = Color(spec.NavActiveBackground);
        tokens.Set("nav-background", background);
        tokens.Set("nav-border", Color(spec.Border));
        tokens.Set("nav-text", tokens.Readable("nav-text", Color(spec.TextSecondary), background, target));
        tokens.Set("nav-active-background", navActiveBackground);
        tokens.Set("nav-active-text", tokens.Readable("nav-active-text", Color(spec.NavActiveText), navActiveBackground, target));
        tokens.Set("nav-active-border", action);
        tokens.Set("nav-hover-background", surface);
        tokens.Set("nav-hover-text", tokens.Readable("nav-hover-text", text, surface, target));

        // Menu draws its active item in the accent on a tint of that accent; left to the component, the tint is
        // mixed from the link color and takes it below the target. The palette sets both.
        tokens.Set("menu-active-background", navActiveBackground);
        tokens.Set("menu-accent", tokens.Readable("menu-accent", link, HarderBackground(link, background, surface, navActiveBackground), target));

        tokens.Set("focus-ring-color", focus);
        var inputBackground = spec.InputBackground is null ? background : Color(spec.InputBackground);
        tokens.Set("input-background", inputBackground);
        tokens.Set("input-border", Color(spec.BorderStrong));
        tokens.Set("input-text", tokens.Readable("input-text", text, inputBackground, target));
        tokens.Set("input-placeholder", tokens.Readable("input-placeholder", Color(spec.TextMuted), inputBackground, target));
        tokens.Set("input-focus-border", focus);

        tokens.Set("active-background", navActiveBackground);
        tokens.Set("color-background-active", navActiveBackground);
        tokens.Set("active-text", tokens.Readable("active-text", action, navActiveBackground, target));
        tokens.Set("color-active", tokens.Readable("color-active", action, navActiveBackground, target));

        AddCode(tokens, spec.Code, target);

        foreach (var (name, value) in spec.Extra)
        {
            tokens.Set(name, Rgb.Parse(value));
        }

        return new PaletteVariant(spec.Palette, spec.Mode, Selectors([spec.Palette, .. spec.AlsoFor], spec.Mode), tokens.Values, target);
    }

    /// <summary>
    /// Builds the code colors of the Osirion palette, whose other tokens come from the theme files, and points Menu's
    /// active item at the theme's own active pair, which the theme files hold to 4.5:1.
    /// </summary>
    public static PaletteVariant BuildCodeOnly(string palette, string mode, CodeColors code, ICollection<string> adjustments)
    {
        var tokens = new Tokens(palette, mode, adjustments);
        const double target = 4.5;
        AddCode(tokens, code, target);
        tokens.SetReference("menu-active-background", "nav-active-background");
        tokens.SetReference("menu-accent", "nav-active-text");
        return new PaletteVariant(palette, mode, Selectors([palette], mode), tokens.Values, target);
    }

    private static IReadOnlyList<string> Selectors(IEnumerable<string> palettes, string mode) =>
        palettes.Select(palette => $":root[data-osirion-palette=\"{palette}\"][data-osirion-theme=\"{mode}\"]").ToList();

    private static Rgb HarderBackground(Rgb foreground, params Rgb[] backgrounds) =>
        backgrounds.MinBy(background => Rgb.Contrast(foreground, background));

    private static void AddNeutralScale(Tokens tokens, Rgb background, Rgb text, bool dark)
    {
        var positions = dark ? DarkNeutral : LightNeutral;
        for (var i = 0; i < Steps.Length; i++)
        {
            tokens.Set($"color-neutral-{Steps[i]}", Rgb.Mix(background, text, positions[i]));
        }
    }

    private static void AddPrimaryScale(Tokens tokens, Rgb background, Rgb accent, bool dark)
    {
        for (var i = 0; i < PrimaryToBackground.Length; i++)
        {
            tokens.Set($"color-primary-{Steps[i]}", Rgb.Mix(accent, background, PrimaryToBackground[i]));
        }

        tokens.Set("color-primary-600", accent);
        var far = dark ? new Rgb(1, 1, 1) : new Rgb(0, 0, 0);
        for (var i = 0; i < PrimaryBeyond.Length; i++)
        {
            tokens.Set($"color-primary-{Steps[7 + i]}", Rgb.Mix(accent, far, PrimaryBeyond[i]));
        }
    }

    private static void AddStatus(Tokens tokens, string name, Rgb color, string? backgroundHex, Rgb page, double target)
    {
        var background = backgroundHex is null ? Rgb.Mix(page, color, 0.12) : Rgb.Parse(backgroundHex, page);
        var text = tokens.Readable($"status-{name}-text", color, HarderBackground(color, background, page), target);
        tokens.Set($"status-{name}", text);
        tokens.Set($"status-{name}-bg", background);
        tokens.Set($"status-{name}-border", Rgb.Mix(page, color, 0.35));
        tokens.Set($"status-{name}-text", text);
        tokens.Set($"status-{name}-focus", color);
        tokens.Set($"status-{name}-hover", text);
    }

    private static void AddCode(Tokens tokens, CodeColors code, double target)
    {
        var background = Rgb.Parse(code.Background);
        tokens.Set("code-background", background);
        tokens.Set("syntax-background-color", background);
        Code("code-text", code.Text);
        Code("code-comment", code.Comment);
        Code("code-keyword", code.Keyword);
        Code("code-string", code.String);
        Code("code-number", code.Number);
        Code("code-function", code.Function);
        Code("code-type", code.Type);
        Code("code-variable", code.Variable);
        Code("code-tag", code.Tag);
        Code("code-attribute", code.Attribute);
        Code("code-regex", code.Regex);
        Code("code-inserted", code.Inserted);
        Code("code-deleted", code.Deleted);

        void Code(string token, string hex) => tokens.Set(token, tokens.Readable(token, Rgb.Parse(hex, background), background, target));
    }

    /// <summary>The tokens of one variant, in writing order, with the contrast adjustments reported as they happen.</summary>
    private sealed class Tokens(string palette, string mode, ICollection<string> adjustments)
    {
        private readonly List<KeyValuePair<string, string>> _values = [];

        public IReadOnlyList<KeyValuePair<string, string>> Values => _values;

        public void Set(string name, Rgb color) => Set(name, color.ToHex());

        /// <summary>Sets a token to another token's value, for tokens whose color the theme files own.</summary>
        public void SetReference(string name, string token) => Set(name, $"var(--osirion-{token})");

        private void Set(string name, string value)
        {
            var index = _values.FindIndex(pair => pair.Key == name);
            var entry = new KeyValuePair<string, string>(name, value);
            if (index >= 0)
            {
                _values[index] = entry;
            }
            else
            {
                _values.Add(entry);
            }
        }

        public Rgb Readable(string token, Rgb color, Rgb against, double minimum)
        {
            var adjusted = color.EnsureContrast(against, minimum);
            if (adjusted.ToHex() != color.ToHex())
            {
                adjustments.Add(
                    $"{palette} {mode} {token}: {color.ToHex()} -> {adjusted.ToHex()} " +
                    $"({Rgb.Contrast(color, against):0.00} -> {Rgb.Contrast(adjusted, against):0.00} against {against.ToHex()})");
            }

            return adjusted;
        }
    }
}
