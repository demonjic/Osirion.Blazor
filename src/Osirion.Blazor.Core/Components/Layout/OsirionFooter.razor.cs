using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>
/// Footer component for displaying site footer with links, social media, and copyright information
/// </summary>
public partial class OsirionFooter
{
    // Set by OsirionPageLayout around its footer slot, which already renders the <footer> landmark.
    [CascadingParameter(Name = OsirionPageLayout.FooterSlotCascadeName)]
    private bool InsidePageLayoutFooter { get; set; }

    private bool HasTopContent =>
        Logo is not null
        || Links?.Any() == true
        || (ShowSocialLinks && SocialLinks?.Any() == true)
        || LeftContent is not null
        || RightContent is not null;

    private bool CompanyIsExternal => CompanyUrl.StartsWith("http", StringComparison.Ordinal);

    /// <summary>
    /// Gets or sets the name linked in the default copyright line (<c>&amp;copy; year name. All rights reserved.</c>).
    /// Ignored when <see cref="Copyright"/> is set or <see cref="ShowCopyright"/> is <see langword="false"/>.
    /// </summary>
    [Parameter]
    public string CompanyName { get; set; } = "Company";

    /// <summary>
    /// Gets or sets the address the company name links to in the default copyright line. An address that starts with
    /// <c>http</c> opens in a new tab with <c>rel="noopener noreferrer"</c>; any other opens in the same tab.
    /// </summary>
    [Parameter]
    public string CompanyUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets the text under the logo, rendered as raw HTML, so pass only text you control. It is shown only when
    /// the top row renders, which needs at least one of <see cref="Logo"/>, <see cref="Links"/>,
    /// <see cref="SocialLinks"/> (while <see cref="ShowSocialLinks"/> is on), <see cref="LeftContent"/> or
    /// <see cref="RightContent"/>.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets custom copyright text, rendered as raw HTML, so pass only text you control. It replaces the default
    /// line built from <see cref="CompanyName"/>, <see cref="CompanyUrl"/> and the current year.
    /// </summary>
    [Parameter]
    public string? Copyright { get; set; }

    /// <summary>
    /// Gets or sets whether the default copyright line is shown when <see cref="Copyright"/> is empty.
    /// </summary>
    [Parameter]
    public bool ShowCopyright { get; set; } = true;

    /// <summary>
    /// Gets or sets the logo, shown in the top row above <see cref="Description"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? Logo { get; set; }

    /// <summary>
    /// Gets or sets free content for the left column of the top row, such as contact details. It renders above the logo
    /// and description.
    /// </summary>
    [Parameter]
    public RenderFragment? LeftContent { get; set; }

    /// <summary>
    /// Gets or sets free content for the right column of the top row.
    /// </summary>
    [Parameter]
    public RenderFragment? RightContent { get; set; }

    /// <summary>
    /// Gets or sets the link columns: each <c>FooterSection</c> has an optional <c>Title</c> (an <c>h2</c>) and a list of
    /// <c>FooterLink</c>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FooterSection>? Links { get; set; }

    /// <summary>
    /// Gets or sets the social links. Each shows its <c>Icon</c>, or its <c>Text</c> when there is no icon, and always
    /// opens in a new tab; set <c>AriaLabel</c> on icon-only links.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FooterSocialLink>? SocialLinks { get; set; }

    /// <summary>
    /// Gets or sets the links in the bottom row, such as privacy and terms. They sit in a <c>nav</c> named by
    /// <see cref="BottomLinksLabel"/>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FooterLink>? BottomLinks { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the <c>nav</c> around <see cref="BottomLinks"/>. Set it for another
    /// language. Defaults to <c>Footer legal navigation</c>.
    /// </summary>
    [Parameter]
    public string BottomLinksLabel { get; set; } = "Footer legal navigation";

    /// <summary>
    /// Gets or sets whether <see cref="SocialLinks"/> are shown.
    /// </summary>
    [Parameter]
    public bool ShowSocialLinks { get; set; } = true;

    /// <summary>
    /// Gets or sets the <c>h2</c> above the social links. An empty value hides it.
    /// </summary>
    [Parameter]
    public string SocialTitle { get; set; } = "Follow Us";

    /// <summary>
    /// Gets or sets whether a line separates the top and bottom rows. It shows only when both rows render.
    /// </summary>
    [Parameter]
    public bool ShowDivider { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the top row (logo, description, links, social links, left and right content) renders.
    /// </summary>
    [Parameter]
    public bool ShowTopSection { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the bottom row (copyright line and <see cref="BottomLinks"/>) renders.
    /// </summary>
    [Parameter]
    public bool ShowBottomSection { get; set; } = true;

    /// <summary>
    /// Gets or sets the variant, added as the class <c>osirion-footer-{value}</c>. The stylesheet styles <c>minimal</c>
    /// and <c>centered</c>; <c>default</c> and any other value add a class with no rules.
    /// </summary>
    [Parameter]
    public string Variant { get; set; } = "default";

    /// <summary>
    /// Gets or sets the number of link columns, added as the class <c>osirion-footer-grid-{value}</c>. The stylesheet
    /// defines <c>2-column</c>, <c>3-column</c> and <c>4-column</c>; <c>auto</c> and any other value use the default
    /// layout.
    /// </summary>
    [Parameter]
    public string GridLayout { get; set; } = "auto";

    /// <summary>
    /// Gets or sets extra content rendered below the bottom row.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether the footer is docked, adding the class <c>osirion-footer-docked-{DockingMode}</c>.
    /// </summary>
    [Parameter]
    public bool Docked { get; set; } = false;

    /// <summary>
    /// Gets or sets how a docked footer is placed: <c>"sticky"</c> adds <c>margin-top: auto</c>, which pushes the footer to
    /// the bottom of a flex column; <c>"fixed"</c> fixes it to the bottom of the viewport, where it covers the end of
    /// the page. Used only when <see cref="Docked"/> is <see langword="true"/>.
    /// </summary>
    [Parameter]
    public string DockingMode { get; set; } = "sticky";

    /// <summary>
    /// Gets the component theme for the data attribute.
    /// Returns the component's specific theme if set, otherwise null to use global theme.
    /// </summary>
    private string? GetComponentTheme()
    {
        return Theme switch
        {
            ThemeMode.Light => "light",
            ThemeMode.Dark => "dark",
            ThemeMode.System => null, // Use global theme
            _ => null
        };
    }

    /// <summary>
    /// Gets the CSS class for the footer
    /// </summary>
    private string GetFooterClass()
    {
        var classes = new List<string> { "osirion-footer" };

        classes.Add($"osirion-footer-{Variant}");
        //classes.Add($"osirion-footer-theme-{Theme}");
        classes.Add($"osirion-footer-grid-{GridLayout}");

        if (Docked)
        {
            classes.Add($"osirion-footer-docked-{DockingMode}");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }
}

/// <summary>
/// Represents a footer section with links
/// </summary>
public class FooterSection
{
    /// <summary>
    /// Gets or sets the section title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the links in this section
    /// </summary>
    public IReadOnlyList<FooterLink> Links { get; set; } = Array.Empty<FooterLink>();
}

/// <summary>
/// Represents a footer link
/// </summary>
public class FooterLink
{
    /// <summary>
    /// Gets or sets the link text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the link URL
    /// </summary>
    public string Href { get; set; } = "#";

    /// <summary>
    /// Gets or sets whether to open in a new tab
    /// </summary>
    public bool OpenInNewTab { get; set; }
}

/// <summary>
/// Represents a social media link
/// </summary>
public class FooterSocialLink : FooterLink
{
    /// <summary>
    /// Gets or sets the icon content
    /// </summary>
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets the aria label for accessibility
    /// </summary>
    public string AriaLabel { get; set; } = string.Empty;
}
