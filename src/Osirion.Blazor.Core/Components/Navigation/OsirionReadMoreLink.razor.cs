using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>
/// Renders an <c>a</c> element with text and an optional icon, for "read more" links in cards and lists and for small
/// calls to action. It needs no script and takes its link class from the configured CSS framework.
/// </summary>
public partial class OsirionReadMoreLink
{
    /// <summary>
    /// Gets or sets the link address. It is written to <c>href</c> as given, so pass only addresses you trust. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Href { get; set; } = "#";

    /// <summary>
    /// Gets or sets the visible text.
    /// </summary>
    [Parameter]
    public string Text { get; set; } = "Read more";

    /// <summary>
    /// Gets or sets the accessible name, written as <c>aria-label</c>. When empty, <see cref="Text"/> is used. Keep the
    /// visible text at its start, as in <c>Read more: Static SSR with Blazor</c>, so speech input users can say what
    /// they see.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Text"/> is rendered. Turn it off for an icon-only link and set
    /// <see cref="AriaLabel"/>.
    /// </summary>
    [Parameter]
    public bool ShowText { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the icon is rendered, the built-in one or <see cref="IconContent"/>.
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets which side of the text the icon sits on: <c>Left</c> or <c>Right</c>.
    /// </summary>
    [Parameter]
    public IconPosition IconPosition { get; set; } = IconPosition.Right;

    /// <summary>
    /// Gets or sets your own icon, rendered in place of the built-in one inside an <c>aria-hidden</c> span.
    /// </summary>
    [Parameter]
    public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Gets or sets the look and the built-in icon: <c>Default</c> (chevron), <c>Arrow</c>, <c>External</c>,
    /// <c>Download</c>, <c>Plain</c> (body text color, chevron) or <c>Button</c> (filled, chevron).
    /// </summary>
    [Parameter]
    public ReadMoreVariant Variant { get; set; } = ReadMoreVariant.Default;

    /// <summary>
    /// Gets or sets the size: <c>Small</c>, <c>Normal</c> or <c>Large</c>.
    /// </summary>
    [Parameter]
    public LinkSize Size { get; set; } = LinkSize.Normal;

    /// <summary>
    /// Gets or sets whether the link covers its nearest positioned ancestor, such as a card, so the whole card is
    /// clickable. The ancestor needs <c>position: relative</c>.
    /// </summary>
    [Parameter]
    public bool Stretched { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the link is displayed as a full-width block.
    /// </summary>
    [Parameter]
    public bool Block { get; set; } = false;

    /// <summary>
    /// Gets or sets the <c>target</c> attribute, for example <c>_blank</c>. No <c>rel</c> is added; pass it as an
    /// attribute when you need one.
    /// </summary>
    [Parameter]
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets whether the icon moves slightly on hover. Reduced motion turns it off.
    /// </summary>
    [Parameter]
    public bool Animated { get; set; } = true;

    /// <summary>
    /// Gets or sets extra content rendered inside the link, after the text and icon.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the CSS class for the link element
    /// </summary>
    private string GetLinkClass()
    {
        var classes = new List<string> { "osirion-read-more" };

        // Add framework-specific base classes
        classes.Add(GetFrameworkLinkClass());

        // Add variant class
        classes.Add($"osirion-read-more-{Variant.ToString().ToLower()}");

        // Add size class
        classes.Add($"osirion-read-more-{Size.ToString().ToLower()}");

        // Add layout classes
        if (Stretched)
        {
            classes.Add("osirion-read-more-stretched");
            classes.Add(GetFrameworkStretchedClass());
        }

        if (Block)
        {
            classes.Add("osirion-read-more-block");
        }

        if (Animated)
        {
            classes.Add("osirion-read-more-animated");
        }

        // Add icon position class
        if (ShowIcon)
        {
            classes.Add($"osirion-read-more-icon-{IconPosition.ToString().ToLower()}");
        }

        // Add custom class if provided
        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets framework-specific link classes
    /// </summary>
    private string GetFrameworkLinkClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => Stretched ? "" : "icon-link icon-link-hover", // Don't add icon-link when stretched
            CssFramework.FluentUI => "osirion-fluent-link",
            CssFramework.MudBlazor => "mud-link",
            CssFramework.Radzen => "rz-link",
            _ => "osirion-link"
        };
    }

    /// <summary>
    /// Gets framework-specific stretched link classes
    /// </summary>
    private string GetFrameworkStretchedClass()
    {
        return Framework switch
        {
            CssFramework.Bootstrap => "stretched-link",
            CssFramework.FluentUI => "osirion-fluent-stretched",
            CssFramework.MudBlazor => "mud-stretched",
            CssFramework.Radzen => "rz-stretched",
            _ => "osirion-stretched"
        };
    }

    /// <summary>
    /// Gets the inline style for the link element, or <see langword="null"/> without a <see cref="OsirionComponentBase.Style"/>,
    /// so the link carries no empty <c>style</c> attribute.
    /// </summary>
    private string? GetLinkStyle()
    {
        var styles = new List<string>();

        if (!string.IsNullOrWhiteSpace(Style))
        {
            styles.Add(Style);
        }

        return styles.Count > 0 ? string.Join("; ", styles) + ";" : null;
    }

    /// <summary>
    /// Gets the accessible name: <see cref="AriaLabel"/>, else the current <see cref="Text"/>. It is computed on every
    /// render and never written back into the parameter, so a changed <see cref="Text"/> is not left behind.
    /// </summary>
    private string GetEffectiveAriaLabel()
    {
        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            return AriaLabel;
        }

        return string.IsNullOrWhiteSpace(Text) ? "Read more" : Text;
    }

    /// <summary>
    /// Renders the default icon based on variant and framework
    /// </summary>
    private RenderFragment GetDefaultIcon() => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", "16");
        builder.AddAttribute(3, "height", "16");
        builder.AddAttribute(4, "fill", "currentColor");
        builder.AddAttribute(5, "viewBox", "0 0 16 16");
        builder.AddAttribute(6, "aria-hidden", "true");

        switch (Variant)
        {
            case ReadMoreVariant.Arrow:
                // Arrow right icon
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "fill-rule", "evenodd");
                builder.AddAttribute(9, "d", "M1 8a.5.5 0 0 1 .5-.5h11.793l-3.147-3.146a.5.5 0 0 1 .708-.708l4 4a.5.5 0 0 1 0 .708l-4 4a.5.5 0 0 1-.708-.708L13.293 8.5H1.5A.5.5 0 0 1 1 8");
                builder.CloseElement();
                break;

            case ReadMoreVariant.External:
                // External link icon
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "fill-rule", "evenodd");
                builder.AddAttribute(9, "d", "M8.636 3.5a.5.5 0 0 0-.5-.5H1.5A1.5 1.5 0 0 0 0 4.5v10A1.5 1.5 0 0 0 1.5 16h10a1.5 1.5 0 0 0 1.5-1.5V7.864a.5.5 0 0 0-1 0V14.5a.5.5 0 0 1-.5.5h-10a.5.5 0 0 1-.5-.5v-10a.5.5 0 0 1 .5-.5h6.636a.5.5 0 0 0 .5-.5");
                builder.CloseElement();
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "fill-rule", "evenodd");
                builder.AddAttribute(9, "d", "M16 .5a.5.5 0 0 0-.5-.5h-5a.5.5 0 0 0 0 1h3.793L6.146 9.146a.5.5 0 1 0 .708.708L15 1.707V5.5a.5.5 0 0 0 1 0z");
                builder.CloseElement();
                break;

            case ReadMoreVariant.Download:
                // Download icon
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "d", "M.5 9.9a.5.5 0 0 1 .5.5v2.5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.5a.5.5 0 0 1 1 0v2.5a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-2.5a.5.5 0 0 1 .5-.5");
                builder.CloseElement();
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "d", "M7.646 11.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 10.293V1.5a.5.5 0 0 0-1 0v8.793L5.354 8.146a.5.5 0 1 0-.708.708z");
                builder.CloseElement();
                break;

            default:
                // Default chevron right icon
                builder.OpenElement(7, "path");
                builder.AddAttribute(8, "fill-rule", "evenodd");
                builder.AddAttribute(9, "d", "M4.646 1.646a.5.5 0 0 1 .708 0l6 6a.5.5 0 0 1 0 .708l-6 6a.5.5 0 0 1-.708-.708L10.293 8 4.646 2.354a.5.5 0 0 1 0-.708");
                builder.CloseElement();
                break;
        }

        builder.CloseElement();
    };
}

/// <summary>
/// Read more link variants
/// </summary>
public enum ReadMoreVariant
{
    /// <summary>
    /// Default chevron right style
    /// </summary>
    Default,

    /// <summary>
    /// Arrow right style
    /// </summary>
    Arrow,

    /// <summary>
    /// External link style
    /// </summary>
    External,

    /// <summary>
    /// Download style
    /// </summary>
    Download,

    /// <summary>
    /// Body text color and normal weight. It keeps the chevron; set <c>ShowIcon</c> to <c>false</c> for text only.
    /// </summary>
    Plain,

    /// <summary>
    /// Button-like appearance
    /// </summary>
    Button
}

/// <summary>
/// Icon position relative to text
/// </summary>
public enum IconPosition
{
    /// <summary>
    /// Icon positioned to the left of text
    /// </summary>
    Left,

    /// <summary>
    /// Icon positioned to the right of text
    /// </summary>
    Right
}

/// <summary>
/// Link size options
/// </summary>
public enum LinkSize
{
    /// <summary>
    /// Small size
    /// </summary>
    Small,

    /// <summary>
    /// Normal size (default)
    /// </summary>
    Normal,

    /// <summary>
    /// Large size
    /// </summary>
    Large
}
