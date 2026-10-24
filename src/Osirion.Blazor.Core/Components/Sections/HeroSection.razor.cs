using System.Globalization;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// HeroSection component for displaying hero content with optional background or side image and buttons
/// </summary>
public partial class HeroSection
{
    /// <summary>
    /// The main heading, rendered as an <c>h1</c> with the class <c>osirion-hero-title</c>. Ignored when <see
    /// cref="TitleContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Your own heading markup in place of <see cref="Title"/>, for when the page needs another heading level.
    /// </summary>
    [Parameter]
    public RenderFragment? TitleContent { get; set; }

    /// <summary>
    /// The line under the title, rendered as a paragraph. Ignored when <see cref="SubtitleContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Your own markup in place of <see cref="Subtitle"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? SubtitleContent { get; set; }

    /// <summary>
    /// A longer paragraph under the subtitle. Ignored when <see cref="SummaryContent"/> is set.
    /// </summary>
    [Parameter]
    public string? Summary { get; set; }

    /// <summary>
    /// Your own markup in place of <see cref="Summary"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? SummaryContent { get; set; }

    /// <summary>
    /// The image, shown beside the text or, with <see cref="UseBackgroundImage"/>, behind it. As a background it is
    /// CSS-escaped before it is written to the inline style.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The side image's alternative text; when it is not set, <see cref="Title"/> is used, then <c>Hero image</c>.
    /// Pass an empty string for a decorative image. A background image has no alternative text.
    /// </summary>
    [Parameter]
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Whether <see cref="ImageUrl"/> is the section's background image instead of a side image.
    /// </summary>
    [Parameter]
    public bool UseBackgroundImage { get; set; } = false;

    /// <summary>
    /// Whether a 40% black overlay is drawn over a background image and the text turns white. Has no effect without
    /// <see cref="UseBackgroundImage"/> and <see cref="ImageUrl"/>.
    /// </summary>
    [Parameter]
    public bool HasOverlay { get; set; } = false;

    /// <summary>
    /// A decorative pattern behind the content, drawn unless <see cref="UseBackgroundImage"/> is on.
    /// </summary>
    [Parameter]
    public BackgroundPatternType? BackgroundPattern { get; set; }

    /// <summary>
    /// The text alignment: <c>Left</c>, <c>Center</c> or <c>Right</c>. <c>Justify</c> is treated as <c>Left</c>.
    /// </summary>
    [Parameter]
    public Alignment Alignment { get; set; } = Alignment.Left;

    /// <summary>
    /// Whether the primary button is rendered; it also needs <see cref="PrimaryButtonText"/> and <see
    /// cref="PrimaryButtonUrl"/>.
    /// </summary>
    [Parameter]
    public bool ShowPrimaryButton { get; set; } = true;

    /// <summary>
    /// Whether the secondary button is rendered; it also needs <see cref="SecondaryButtonText"/> and <see
    /// cref="SecondaryButtonUrl"/>.
    /// </summary>
    [Parameter]
    public bool ShowSecondaryButton { get; set; } = true;

    /// <summary>
    /// The primary button's text.
    /// </summary>
    [Parameter]
    public string? PrimaryButtonText { get; set; }

    /// <summary>
    /// The primary button's address, written as given, so pass an address your application controls.
    /// </summary>
    [Parameter]
    public string? PrimaryButtonUrl { get; set; }

    /// <summary>
    /// The secondary button's text.
    /// </summary>
    [Parameter]
    public string? SecondaryButtonText { get; set; }

    /// <summary>
    /// The secondary button's address, written as given, so pass an address your application controls.
    /// </summary>
    [Parameter]
    public string? SecondaryButtonUrl { get; set; }

    /// <summary>
    /// A CSS color for the section's background, written to the inline style as given.
    /// </summary>
    [Parameter]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// A CSS color for the section's text, written to the inline style as given.
    /// </summary>
    [Parameter]
    public string? TextColor { get; set; }

    /// <summary>
    /// The section's minimum height as a CSS length. The default, <c>60vh</c>, comes from the stylesheet; another value
    /// is written to the inline style as given.
    /// </summary>
    [Parameter]
    public string MinHeight { get; set; } = DefaultMinHeight;

    /// <summary>The minimum height the stylesheet sets on <c>.osirion-hero-section</c>.</summary>
    private const string DefaultMinHeight = "60vh";

    /// <summary>
    /// The author name shown in the metadata line when <see cref="ShowMetadata"/> is on.
    /// </summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>
    /// The publication date shown in the metadata line when <see cref="ShowMetadata"/> is on.
    /// </summary>
    [Parameter]
    public DateTime? PublishDate { get; set; }

    /// <summary>
    /// The culture <see cref="PublishDate"/> is written in, normally the language of the page. When
    /// <see langword="null"/>, the current culture is used.
    /// </summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// The reading time in minutes, shown in the metadata line when <see cref="ShowMetadata"/> is on: <c>6</c> is
    /// shown as <c>6 min</c>.
    /// </summary>
    [Parameter]
    public string? ReadTime { get; set; }

    /// <summary>
    /// Whether <see cref="Author"/>, <see cref="PublishDate"/> and <see cref="ReadTime"/> are shown under the text.
    /// </summary>
    [Parameter]
    public bool ShowMetadata { get; set; } = false;

    /// <summary>
    /// The side of the text the side image sits on. <c>Left</c> and <c>Justify</c> are treated as <c>Right</c>, so the
    /// image is always on the right.
    /// </summary>
    [Parameter]
    public Alignment ImagePosition { get; set; } = Alignment.Right;

    /// <summary>
    /// <c>Hero</c>, <c>Jumbotron</c> or <c>Minimal</c>: changes the spacing, the text sizes and the side image's size.
    /// </summary>
    [Parameter]
    public HeroVariant Variant { get; set; } = HeroVariant.Hero;

    /// <summary>
    /// Extra content under the buttons, in a <c>div</c> with the class <c>osirion-hero-additional</c>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether a shadow in the primary color is drawn along the section's top edge.
    /// </summary>
    [Parameter]
    public bool HasDivider { get; set; } = true;

    /// <summary>
    /// Gets whether any buttons should be shown
    /// </summary>
    private bool HasButtons => (ShowPrimaryButton && !string.IsNullOrWhiteSpace(PrimaryButtonText) && !string.IsNullOrWhiteSpace(PrimaryButtonUrl)) ||
                               (ShowSecondaryButton && !string.IsNullOrWhiteSpace(SecondaryButtonText) && !string.IsNullOrWhiteSpace(SecondaryButtonUrl));

    private Alignment _imageposition => ImagePosition == Alignment.Left || ImagePosition == Alignment.Justify ? Alignment.Right : ImagePosition;
    private Alignment _alignment => Alignment == Alignment.Justify ? Alignment.Left : Alignment;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        var att = Attributes;
    }

    /// <summary>
    /// Gets the CSS class for the hero section
    /// </summary>
    private string GetHeroSectionClass()
    {
        var classes = new List<string> { "osirion-hero-section" };

        classes.Add($"osirion-hero-variant-{Variant.ToString().ToLower()}");
        classes.Add($"osirion-hero-align-{_alignment.ToString().ToLower()}");

        if (!string.IsNullOrWhiteSpace(ImageUrl))
        {
            if (UseBackgroundImage)
            {
                classes.Add("osirion-hero-with-background");
                if (HasOverlay)
                {
                    classes.Add("osirion-hero-with-overlay");
                }
            }
            else
            {
                classes.Add("osirion-hero-with-side-image");
                classes.Add($"osirion-hero-image-{_imageposition.ToString().ToLower()}");
            }
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        if (HasDivider)
        {
            classes.Add("hero-divider");
        }


        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the inline style of the section, or <see langword="null"/> when it needs none, so that a hero with the
    /// default settings writes no <c>style</c> attribute for a Content Security Policy to block.
    /// </summary>
    private string? GetBackgroundStyle()
    {
        var styles = new List<string>();

        if (UseBackgroundImage && !string.IsNullOrWhiteSpace(ImageUrl))
        {
            styles.Add($"background-image: {CssUrl.Format(ImageUrl)}");
        }

        if (!string.IsNullOrWhiteSpace(BackgroundColor))
        {
            styles.Add($"background-color: {BackgroundColor}");
        }

        if (!string.IsNullOrWhiteSpace(TextColor))
        {
            styles.Add($"color: {TextColor}");
        }

        if (MinHeight != DefaultMinHeight)
        {
            styles.Add($"min-height: {MinHeight}");
        }

        if (styles.Count == 0)
        {
            return string.IsNullOrWhiteSpace(Style) ? null : Style;
        }

        return string.Join("; ", styles) + ";" + Style;
    }

    private (string Width, string Heigth) GetImageSize()
    {
        return Variant switch
        {
            HeroVariant.Hero => ("600", "315"),
            HeroVariant.Jumbotron => ("600", "600"),
            HeroVariant.Minimal => ("315", "315"),
            _ => ("600", "315"),
        };
    }

    /// <summary>
    /// Formats the publish date for display
    /// </summary>
    private string FormatPublishDate()
    {
        return PublishDate?.ToString("MMM dd, yyyy") ?? string.Empty;
    }
}
