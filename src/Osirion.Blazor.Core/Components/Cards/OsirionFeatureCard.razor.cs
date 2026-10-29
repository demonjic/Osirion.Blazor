using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// Feature card component that displays content with image, title, description, and optional action buttons.
/// Supports extensive customization for borders, shadows, transforms, and content alignment.
/// Fully SSR-compatible and framework-agnostic.
/// </summary>
public partial class OsirionFeatureCard : OsirionComponentBase
{
    #region Content Properties

    /// <summary>
    /// The card title, rendered as a paragraph with the class <c>osirion-feature-card-title</c>.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// The text under the title.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Whether <see cref="Description"/> is shown.
    /// </summary>
    [Parameter]
    public bool ShowDescription { get; set; } = true;

    /// <summary>
    /// Extra content between the description and the links.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// A small decorative icon above the title, such as an <c>svg</c>, in a <c>div</c> with the class
    /// <c>osirion-feature-card-icon</c> and <c>aria-hidden="true"</c>, so put no meaning in it that the title does not
    /// carry. Without an <see cref="ImageUrl"/> the card has no image column: it has a single column and its height
    /// follows its content.
    /// </summary>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Short phrases under the description, each behind a check mark, as an <see cref="OsirionChecklist"/> inside a
    /// <c>div</c> with the class <c>osirion-feature-card-items</c>. Empty phrases are left out.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Items { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// The read more link's target when <see cref="ReadMoreHref"/> is not set, and the link around the image when the
    /// image has alternative text and the read more link does not already go to the same place.
    /// </summary>
    [Parameter]
    public string? Url { get; set; }

    #endregion

    #region Image Properties

    /// <summary>
    /// The image source. Without it the card has no image column and is one column of text, as tall as its content.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The image's alternative text; when it is not set, <see cref="Title"/> is used, then <c>Feature image</c>.
    /// </summary>
    [Parameter]
    public string? ImageAlt { get; set; }

    /// <summary>
    /// Whether the image renders <c>loading="lazy"</c>. Set it to <see langword="false"/> for a card above the fold.
    /// </summary>
    [Parameter]
    public bool ImageLazyLoading { get; set; } = true;

    /// <summary>
    /// Whether the image zooms on hover.
    /// </summary>
    [Parameter]
    public bool ImageTransformOnHover { get; set; } = false;

    /// <summary>
    /// Where the image sits: <c>Top</c>, <c>Bottom</c>, <c>Left</c> or <c>Right</c>.
    /// </summary>
    [Parameter]
    public ImagePosition ImagePosition { get; set; } = ImagePosition.Left;

    #endregion

    #region Layout Properties

    /// <summary>
    /// The card's dimensions, from <c>ExtraExtraSmall</c> to <c>ExtraExtraLarge</c>.
    /// </summary>
    [Parameter]
    public CardSize CardSize { get; set; } = CardSize.Normal;

    /// <summary>
    /// Where the text sits in the card: one of nine positions, from <c>TopLeft</c> to <c>BottomRight</c>.
    /// </summary>
    [Parameter]
    public ContentAlignment ContentAlignment { get; set; } = ContentAlignment.CenterLeft;

    /// <summary>
    /// The text size, from <c>ExtraExtraSmall</c> to <c>ExtraExtraLarge</c>.
    /// </summary>
    [Parameter]
    public FontSizeVariant ContentFontSize { get; set; } = FontSizeVariant.Normal;

    #endregion

    #region Visual Effect Properties

    /// <summary>
    /// When the border shows: <c>Never</c>, <c>Always</c> or <c>OnHover</c>.
    /// </summary>
    [Parameter]
    public EffectTiming BorderTiming { get; set; } = EffectTiming.Always;

    /// <summary>
    /// The border color: <c>Primary</c>, <c>Secondary</c>, <c>Tertiary</c> or <c>Neutral</c>.
    /// </summary>
    [Parameter]
    public ThemeColor BorderColor { get; set; } = ThemeColor.Neutral;

    /// <summary>
    /// When the shadow shows: <c>Never</c>, <c>Always</c> or <c>OnHover</c>.
    /// </summary>
    [Parameter]
    public EffectTiming ShadowTiming { get; set; } = EffectTiming.OnHover;

    /// <summary>
    /// The shadow color: <c>Primary</c>, <c>Secondary</c>, <c>Tertiary</c> or <c>Neutral</c>.
    /// </summary>
    [Parameter]
    public ThemeColor ShadowColor { get; set; } = ThemeColor.Neutral;

    /// <summary>
    /// When the card lifts: <c>Never</c>, <c>Always</c> or <c>OnHover</c>.
    /// </summary>
    [Parameter]
    public EffectTiming TransformTiming { get; set; } = EffectTiming.OnHover;

    #endregion

    #region Action Properties

    /// <summary>
    /// Whether a read more link is shown; it needs <see cref="ReadMoreHref"/> or <see cref="Url"/>.
    /// </summary>
    [Parameter]
    public bool ShowReadMoreLink { get; set; } = false;

    /// <summary>
    /// The read more link's text.
    /// </summary>
    [Parameter]
    public string ReadMoreText { get; set; } = "Read more";

    /// <summary>
    /// The read more link's target; falls back to <see cref="Url"/>.
    /// </summary>
    [Parameter]
    public string? ReadMoreHref { get; set; }

    /// <summary>
    /// With pillow buttons shown, ends the read more link's clickable area 50 pixels above the bottom of the text, so
    /// the buttons stay clickable.
    /// </summary>
    [Parameter]
    public bool ReadMoreStretched { get; set; } = true;

    /// <summary>
    /// Whether <see cref="PillowButtons"/> are shown.
    /// </summary>
    [Parameter]
    public bool ShowPillowButtons { get; set; } = false;

    /// <summary>
    /// Small category-style links under the text.
    /// </summary>
    [Parameter]
    public List<PillowButton>? PillowButtons { get; set; }

    #endregion

    // The phrases of Items as checklist items, without the empty ones; built when the parameters are set.
    private IReadOnlyList<ChecklistItem> _checklistItems = [];

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _checklistItems = ChecklistPhrases.ToItems(Items);
    }

    #region CSS Class Generation

    /// <summary>
    /// Gets the combined CSS classes for the card.
    /// </summary>
    /// <returns>Space-separated CSS class string.</returns>
    protected string GetCardClasses()
    {
        var classes = new List<string>
    {
            "osirion-feature-card",
            GetLayoutClass(),
            GetCardSizeClass(),
            GetContentAlignmentClass(),
            GetContentFontSizeClass()
    };

        AddBorderClasses(classes);
        AddShadowClasses(classes);
        AddTransformClasses(classes);
        AddImageTransformClass(classes);

        return CombineCssClasses(string.Join(" ", classes));
    }

    private void AddBorderClasses(List<string> classes)
    {
        if (BorderTiming == EffectTiming.Never) return;

        classes.Add(GetBorderTimingClass());
        if (BorderColor != ThemeColor.Neutral)
            classes.Add(GetBorderColorClass());
    }

    private void AddShadowClasses(List<string> classes)
    {
        if (ShadowTiming == EffectTiming.Never) return;

        classes.Add(GetShadowTimingClass());
        if (ShadowColor != ThemeColor.Neutral)
            classes.Add(GetShadowColorClass());
    }

    private void AddTransformClasses(List<string> classes)
    {
        if (TransformTiming != EffectTiming.Never)
            classes.Add(GetTransformTimingClass());
    }

    private void AddImageTransformClass(List<string> classes)
    {
        if (ImageTransformOnHover)
            classes.Add("osirion-feature-card-image-transform");
    }

    #endregion

    #region CSS Class Mapping Methods

    private string GetImagePositionClass() => HasImage() ? ImagePosition switch
    {
        ImagePosition.Top => "osirion-feature-card-image-top",
        ImagePosition.Bottom => "osirion-feature-card-image-bottom",
        ImagePosition.Right => "osirion-feature-card-image-right",
        ImagePosition.Left => "osirion-feature-card-image-left",
        _ => "osirion-feature-card-image-left"
    } : "osirion-feature-card-image-left";

    private string GetCardSizeClass() => CardSize switch
    {
        CardSize.ExtraExtraLarge => "osirion-feature-card-xxl",
        CardSize.ExtraLarge => "osirion-feature-card-xl",
        CardSize.Large => "osirion-feature-card-lg",
        CardSize.Normal => "osirion-feature-card-md",
        CardSize.Small => "osirion-feature-card-sm",
        CardSize.ExtraSmall => "osirion-feature-card-xs",
        CardSize.ExtraExtraSmall => "osirion-feature-card-xxs",
        _ => "osirion-feature-card-md"
    };

    private string GetContentAlignmentClass() => ContentAlignment switch
    {
        ContentAlignment.TopLeft => "osirion-feature-card-content-top-left",
        ContentAlignment.TopCenter => "osirion-feature-card-content-top-center",
        ContentAlignment.TopRight => "osirion-feature-card-content-top-right",
        ContentAlignment.CenterLeft => "osirion-feature-card-content-center-left",
        ContentAlignment.Center => "osirion-feature-card-content-center",
        ContentAlignment.CenterRight => "osirion-feature-card-content-center-right",
        ContentAlignment.BottomLeft => "osirion-feature-card-content-bottom-left",
        ContentAlignment.BottomCenter => "osirion-feature-card-content-bottom-center",
        ContentAlignment.BottomRight => "osirion-feature-card-content-bottom-right",
        _ => "osirion-feature-card-content-center-left"
    };

    private string GetContentFontSizeClass() => ContentFontSize switch
    {
        FontSizeVariant.ExtraExtraSmall => "osirion-feature-card-font-xxs",
        FontSizeVariant.ExtraSmall => "osirion-feature-card-font-xs",
        FontSizeVariant.Small => "osirion-feature-card-font-sm",
        FontSizeVariant.Normal => "osirion-feature-card-font-md",
        FontSizeVariant.Large => "osirion-feature-card-font-lg",
        FontSizeVariant.ExtraLarge => "osirion-feature-card-font-xl",
        FontSizeVariant.ExtraExtraLarge => "osirion-feature-card-font-xxl",
        _ => "osirion-feature-card-font-md"
    };

    private string GetBorderTimingClass() => BorderTiming switch
    {
        EffectTiming.Always => "osirion-feature-card-border-always",
        EffectTiming.OnHover => "osirion-feature-card-border-hover",
        _ => string.Empty
    };

    private string GetBorderColorClass() => BorderColor switch
    {
        ThemeColor.Primary => "osirion-feature-card-border-primary",
        ThemeColor.Secondary => "osirion-feature-card-border-secondary",
        ThemeColor.Tertiary => "osirion-feature-card-border-tertiary",
        _ => string.Empty
    };

    private string GetShadowTimingClass() => ShadowTiming switch
    {
        EffectTiming.Always => "osirion-feature-card-shadow-always",
        EffectTiming.OnHover => "osirion-feature-card-shadow-hover",
        _ => string.Empty
    };

    private string GetShadowColorClass() => ShadowColor switch
    {
        ThemeColor.Primary => "osirion-feature-card-shadow-primary",
        ThemeColor.Secondary => "osirion-feature-card-shadow-secondary",
        ThemeColor.Tertiary => "osirion-feature-card-shadow-tertiary",
        _ => string.Empty
    };

    private string GetTransformTimingClass() => TransformTiming switch
    {
        EffectTiming.Always => "osirion-feature-card-transform-always",
        EffectTiming.OnHover => "osirion-feature-card-transform-hover",
        _ => string.Empty
    };

    #endregion

    #region URL Resolution

    /// <summary>
    /// Gets the appropriate href for the read more link.
    /// ReadMoreHref takes precedence over Url.
    /// </summary>
    /// <returns>The resolved read more URL or null.</returns>
    private string? GetReadMoreHref() => ReadMoreHref ?? Url;

    private string GetReadMoreStyles()
    {
        var justify = "";
        var stretched = (ReadMoreStretched && HasPillowButtons()) ? "bottom:50px;" : "";

        switch (ContentAlignment)
        {
            case ContentAlignment.TopCenter:
            case ContentAlignment.Center:
            case ContentAlignment.BottomCenter:
                justify = "justify-content: center;";
                break;
            case ContentAlignment.TopLeft:
            case ContentAlignment.CenterLeft:
            case ContentAlignment.BottomLeft:
                justify = "justify-content: flex-start;";
                break;
            case ContentAlignment.TopRight:
            case ContentAlignment.CenterRight:
            case ContentAlignment.BottomRight:
                justify = "justify-content: flex-end;";
                break;

        }

        return $"{justify} {stretched}";
    }

    #endregion

    #region Image Utilities

    /// <summary>
    /// Gets the placeholder image dimensions based on card size.
    /// </summary>
    /// <returns>Placeholder image dimension string.</returns>
    private string GetPlaceholderImageDimensions() => CardSize switch
    {
        CardSize.ExtraExtraLarge => "1200x1200",
        CardSize.ExtraLarge => "900x900",
        CardSize.Large => "600x600",
        CardSize.Normal => "450x450",
        CardSize.Small => "300x300",
        CardSize.ExtraSmall => "150x150",
        CardSize.ExtraExtraSmall => "75x75",
        _ => "450x450"
    };

    /// <summary>
    /// Generates a placeholder image URL for missing images.
    /// </summary>
    /// <returns>Placeholder image URL.</returns>
    private string GetPlaceholderImageUrl()
    {
        var dimensions = GetPlaceholderImageDimensions();
        return $"https://placehold.co/{dimensions}/1a1a1a/ffffff?text=No+Image";
    }

    /// <summary>
    /// Gets the resolved image alt text.
    /// Defaults to Title if ImageAlt is not specified.
    /// </summary>
    /// <returns>Image alt text or Title.</returns>
    private string GetImageAltText() => ImageAlt ?? Title ?? "Feature image";

    #endregion

    #region Validation

    /// <summary>
    /// Validates if an image is present and should be displayed.
    /// </summary>
    /// <returns>True if image should be displayed.</returns>
    private bool HasImage() => !string.IsNullOrWhiteSpace(ImageUrl);

    /// <summary>
    /// Whether the card shows its icon instead of an image column: an icon is set and there is no image.
    /// </summary>
    private bool IsIconCard() => Icon is not null && !HasImage();

    /// <summary>
    /// The card's layout: beside or above its image, one column with its icon, or one column with text only.
    /// </summary>
    private string GetLayoutClass()
    {
        if (HasImage())
        {
            return GetImagePositionClass();
        }

        return IsIconCard() ? "osirion-feature-card-with-icon" : "osirion-feature-card-without-image";
    }

    /// <summary>
    /// Whether the image is a link to <see cref="Url"/>. It is not when the read more link already goes there, so the
    /// card has one link to a place, nor when its alternative text is empty, which would leave the link without a name.
    /// </summary>
    private bool LinksImage() =>
        !string.IsNullOrWhiteSpace(Url)
        && !string.IsNullOrWhiteSpace(GetImageAltText())
        && !(HasReadMoreLink() && string.Equals(GetReadMoreHref(), Url, StringComparison.Ordinal));

    /// <summary>
    /// Whether a pillow button has a name for assistive technology: its text or its aria label. A button without
    /// either is left out rather than rendered as a link nobody can name.
    /// </summary>
    private static bool HasName(PillowButton button) =>
        !string.IsNullOrWhiteSpace(button.Text) || !string.IsNullOrWhiteSpace(button.AriaLabel);

    /// <summary>
    /// Whether the card has any item phrases to show.
    /// </summary>
    private bool HasItems() => _checklistItems.Count > 0;

    /// <summary>
    /// Validates if pillow buttons should be displayed.
    /// </summary>
    /// <returns>True if pillow buttons should be displayed.</returns>
    private bool HasPillowButtons() => PillowButtons?.Any() == true && ShowPillowButtons;

    /// <summary>
    /// Validates if read more link should be displayed.
    /// </summary>
    /// <returns>True if read more link should be displayed.</returns>
    private bool HasReadMoreLink() => ShowReadMoreLink && !string.IsNullOrWhiteSpace(GetReadMoreHref());

    #endregion
}

/// <summary>
/// Represents a pillow-style button with href and theming options.
/// </summary>
public class PillowButton
{
    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the button href.
    /// </summary>
    public string Href { get; set; } = "#";

    /// <summary>
    /// Gets or sets the button theme color.
    /// </summary>
    public ThemeColor Color { get; set; } = ThemeColor.Primary;

    /// <summary>
    /// Gets or sets the button target attribute.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the button aria-label for accessibility.
    /// </summary>
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Initializes a new instance of the PillowButton class.
    /// </summary>
    public PillowButton() { }

    /// <summary>
    /// Initializes a new instance of the PillowButton class with specified properties.
    /// </summary>
    /// <param name="text">The button text.</param>
    /// <param name="href">The button href.</param>
    /// <param name="color">The button theme color.</param>
    public PillowButton(string text, string href, ThemeColor color = ThemeColor.Primary)
    {
        Text = text;
        Href = href;
        Color = color;
    }
}
