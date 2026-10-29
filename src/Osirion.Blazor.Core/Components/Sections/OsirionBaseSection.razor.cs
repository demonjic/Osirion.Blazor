using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// Base section component for creating structured content sections with optional headers, descriptions, and background styling.
/// This component provides consistent styling and layout for different CSS frameworks.
/// </summary>
public partial class OsirionBaseSection
{
    /// <summary>
    /// The section's content, inside the container below the heading.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The section's <c>id</c>, for in-page links; the section keeps a 3rem scroll margin so a link does not land
    /// under a sticky header.
    /// </summary>
    [Parameter, EditorRequired]
    public string? Id { get; set; }

    /// <summary>
    /// The heading, rendered as an <c>h2</c>. Written as raw HTML without sanitizing, so pass only text you control;
    /// use <see cref="TitleContent"/> for text you do not control. Ignored when <see cref="TitleContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Your own heading markup in place of <see cref="Title"/>. Blazor encodes any text in it.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// A short label above the heading, such as <c>Features</c> or <c>How it works</c>, as plain text in a paragraph
    /// with the class <c>osirion-section-eyebrow</c>. Shown only together with a title.
    /// </summary>
    [Parameter]
    public string? Eyebrow { get; set; }

    /// <summary>
    /// The band behind the section: <c>Default</c> (the page's background), <c>Tinted</c>
    /// (<c>--osirion-background-secondary</c>) or <c>Dark</c>, which gives the section the dark theme's tokens through
    /// <c>data-theme</c> (<c>data-bs-theme</c> under Bootstrap) so its text, cards and buttons turn dark with it. An
    /// explicit <see cref="OsirionComponentBase.Theme"/> and <see cref="BackgroundColor"/> still win.
    /// </summary>
    [Parameter]
    public SectionVariant Variant { get; set; } = SectionVariant.Default;

    /// <summary>
    /// The paragraph under the heading. Written as raw HTML without sanitizing, so pass only text you control. Shown
    /// only together with a title, and ignored when <see cref="DescriptionContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Your own markup in place of <see cref="Description"/>. Shown only together with a title.
    /// </summary>
    [Parameter]
    public RenderFragment? DescriptionContent { get; set; }

    /// <summary>
    /// The class of the inner container; when it is empty the configured CSS framework's container class is used.
    /// </summary>
    [Parameter]
    public string? ContainerClass { get; set; }

    /// <summary>
    /// The alignment of the heading and the content: <c>Left</c>, <c>Center</c>, <c>Right</c> or <c>Justify</c>.
    /// </summary>
    [Parameter]
    public Alignment TextAlignment { get; set; } = Alignment.Center;

    /// <summary>
    /// The background image, CSS-escaped and written as a <c>url()</c> to the inline style. The text turns white while
    /// it is set.
    /// </summary>
    [Parameter]
    public string? BackgroundImageUrl { get; set; }

    /// <summary>
    /// A CSS color for the background, written to the inline style as given.
    /// </summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// A CSS color for the text, written to the inline style as given.
    /// </summary>
    [Parameter]
    public string? TextColor { get; set; }

    /// <summary>
    /// Whether a dark overlay is drawn over the background image so the text stays readable. Has no effect without
    /// <see cref="BackgroundImageUrl"/>.
    /// </summary>
    [Parameter]
    public bool ShowOverlay { get; set; } = true;

    /// <summary>
    /// A decorative pattern, drawn when <see cref="ShowPattern"/> is on.
    /// </summary>
    [Parameter]
    public BackgroundPatternType? BackgroundPattern { get; set; }

    /// <summary>
    /// Whether <see cref="BackgroundPattern"/> is drawn behind the content.
    /// </summary>
    [Parameter]
    public bool ShowPattern { get; set; } = false;

    /// <summary>
    /// The vertical padding: <c>None</c>, <c>Small</c> (2rem), <c>Medium</c> (3rem) or <c>Large</c> (4rem); smaller on
    /// narrow screens.
    /// </summary>
    [Parameter]
    public SectionPadding Padding { get; set; } = SectionPadding.Medium;

    /// <summary>
    /// Whether a shadow under the section separates it from the next.
    /// </summary>
    [Parameter]
    public bool HasDivider { get; set; } = false;

    /// <summary>
    /// Gets the CSS class for the section element
    /// </summary>
    private string GetSectionClass()
    {
        var classes = new List<string> { "osirion-section" };

        // Add text alignment class
        classes.Add($"osirion-section-align-{TextAlignment.ToString().ToLower()}");

        // Add padding class
        classes.Add($"osirion-section-padding-{Padding.ToString().ToLower()}");

        // Add background styling classes
        if (!string.IsNullOrWhiteSpace(BackgroundImageUrl))
        {
            classes.Add("osirion-section-with-background");
        }

        if (ShowPattern && BackgroundPattern.HasValue)
        {
            classes.Add("osirion-section-with-pattern");
        }

        if (HasDivider)
        {
            classes.Add("osirion-section-divider");
        }

        // The default band adds no class, so a section without a variant keeps its markup.
        if (Variant != SectionVariant.Default)
        {
            classes.Add(Variant == SectionVariant.Tinted ? "osirion-section-variant-tinted" : "osirion-section-variant-dark");
        }

        // Add custom class if provided
        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the CSS class for the container div
    /// </summary>
    private string GetContainerClass()
    {
        if (!string.IsNullOrWhiteSpace(ContainerClass))
        {
            return ContainerClass;
        }

        // Use framework-specific container classes
        return Framework switch
        {
            CssFramework.Bootstrap => "container-xxl",
            CssFramework.FluentUI => "osirion-container",
            CssFramework.MudBlazor => "mud-container",
            CssFramework.Radzen => "rz-container",
            _ => "osirion-container"
        };
    }

    /// <summary>
    /// The value of a theme attribute for the dark band: <c>dark</c> for the attribute the configured CSS framework
    /// reads, and <see langword="null"/> (no attribute) otherwise. An explicit <see cref="OsirionComponentBase.Theme"/>
    /// is written by the base class instead, so it wins over the band.
    /// </summary>
    private string? GetDarkBandTheme(string attributeName)
    {
        var isDarkBand = Variant == SectionVariant.Dark && Theme == ThemeMode.System;
        return isDarkBand && attributeName == ThemeAttributeName ? "dark" : null;
    }

    /// <summary>
    /// Gets the CSS class for the header section
    /// </summary>
    private string GetHeaderClass()
    {
        var classes = new List<string> { "osirion-section-header" };

        // Add text alignment class for header
        classes.Add($"osirion-section-header-{TextAlignment.ToString().ToLower()}");

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the inline style for the section element
    /// </summary>
    private string GetSectionStyle()
    {
        var styles = new List<string>();

        if (!string.IsNullOrWhiteSpace(BackgroundImageUrl))
        {
            styles.Add($"background-image: {CssUrl.Format(BackgroundImageUrl)}");
        }

        if (!string.IsNullOrWhiteSpace(BackgroundColor))
        {
            styles.Add($"background-color: {BackgroundColor}");
        }

        if (!string.IsNullOrWhiteSpace(TextColor))
        {
            styles.Add($"color: {TextColor}");
        }

        if (!string.IsNullOrWhiteSpace(Style))
        {
            styles.Add(Style);
        }

        return string.Join("; ", styles) + (styles.Count > 0 ? ";" : "");
    }
}

/// <summary>
/// Enumeration for section padding sizes
/// </summary>
public enum SectionPadding
{
    /// <summary>
    /// No padding
    /// </summary>
    None,

    /// <summary>
    /// Small padding
    /// </summary>
    Small,

    /// <summary>
    /// Medium padding (default)
    /// </summary>
    Medium,

    /// <summary>
    /// Large padding
    /// </summary>
    Large
}
