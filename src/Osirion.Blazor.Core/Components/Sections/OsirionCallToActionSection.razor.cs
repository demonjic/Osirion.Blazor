using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// The closing call to action of a page: a heading, a short text and up to two links styled as buttons, which take
/// the classes of the configured CSS framework. It renders without script, and renders nothing when it has nothing
/// to show.
/// </summary>
public partial class OsirionCallToActionSection
{
    /// <summary>
    /// The heading, rendered at <see cref="HeadingLevel"/>; it also names the section for screen readers. With an
    /// empty value no heading is rendered and the section has no accessible name.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// The short text under the heading, as a paragraph; nothing is rendered when it is empty.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// The label of the primary button. The button is rendered only with a label and a safe
    /// <see cref="PrimaryButtonUrl"/>.
    /// </summary>
    [Parameter]
    public string? PrimaryButtonText { get; set; }

    /// <summary>
    /// Where the primary button goes: a path on the site or an http, https or mailto address. Any other address, such
    /// as <c>javascript:</c>, leaves the button out.
    /// </summary>
    [Parameter]
    public string? PrimaryButtonUrl { get; set; }

    /// <summary>
    /// The label of the secondary button, rendered after the primary one. The button is rendered only with a label and
    /// a safe <see cref="SecondaryButtonUrl"/>.
    /// </summary>
    [Parameter]
    public string? SecondaryButtonText { get; set; }

    /// <summary>
    /// Where the secondary button goes, under the same rules as <see cref="PrimaryButtonUrl"/>.
    /// </summary>
    [Parameter]
    public string? SecondaryButtonUrl { get; set; }

    /// <summary>
    /// The level of the heading, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It defaults to <c>2</c>, a section
    /// under the page's own <c>h1</c>. A value outside the range is moved to the nearest level. The heading keeps its
    /// look at every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    /// <summary>
    /// Other content, rendered after the description and before the buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    // Generated once per instance, so the heading id and aria-labelledby agree on every render and two sections on a
    // page never repeat an id.
    private readonly string _headingId = $"osirion-call-to-action-{Guid.NewGuid():N}";

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    private bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    private bool HasPrimaryButton => IsButton(PrimaryButtonText, PrimaryButtonUrl);

    private bool HasSecondaryButton => IsButton(SecondaryButtonText, SecondaryButtonUrl);

    private bool HasButtons => HasPrimaryButton || HasSecondaryButton;

    private bool HasContent => HasTitle || HasDescription || ChildContent is not null || HasButtons;

    private static bool IsButton(string? text, string? url) => !string.IsNullOrWhiteSpace(text) && SafeLink.IsSafe(url);

    private string GetSectionClass() => CombineCssClasses("osirion-call-to-action-section");
}
