using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>Displays labeled detail cards in a responsive section.</summary>
public partial class OsirionContextCardGrid : OsirionComponentBase
{
    /// <summary>
    /// The section's heading, rendered at <see cref="HeadingLevel"/>; with an empty value no heading is rendered and
    /// the section has no <c>aria-labelledby</c>.
    /// </summary>
    [Parameter] public string Title { get; set; } = "Context by category";

    /// <summary>
    /// The level of the section's heading, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It defaults to <c>2</c>, a
    /// section under the page's own <c>h1</c>. Each card's label is a heading one level below, up to <c>h6</c>. A value
    /// outside the range is moved to the nearest level, and the headings keep their look at every level.
    /// </summary>
    [Parameter] public int HeadingLevel { get; set; } = 2;

    /// <summary>
    /// Supporting text under the heading; nothing is rendered when it is empty.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// The cards. With none, only the heading is rendered.
    /// </summary>
    [Parameter] public ContextCardItem[] Items { get; set; } = [];

    /// <summary>
    /// The <c>id</c> of the heading, which the section's <c>aria-labelledby</c> points at. Without a value each grid
    /// generates its own, so several grids on a page never repeat an id.
    /// </summary>
    [Parameter] public string? HeadingId { get; set; }

    /// <summary>
    /// The section's <c>id</c>, for in-page links.
    /// </summary>
    [Parameter] public string? Id { get; set; }

    /// <summary>
    /// Extra CSS classes on the section.
    /// </summary>
    [Parameter] public string? SectionClass { get; set; }

    /// <summary>
    /// CSS classes of the inner container; <c>container</c> is a Bootstrap class, so set another value for other
    /// frameworks.
    /// </summary>
    [Parameter] public string ContainerClass { get; set; } = "container";

    private readonly string _generatedHeadingId = $"osirion-context-card-grid-{Guid.NewGuid():N}";

    private string EffectiveHeadingId => string.IsNullOrWhiteSpace(HeadingId) ? _generatedHeadingId : HeadingId;

    private bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    private int LabelLevel => Math.Min(TitleLevel + 1, 6);

    /// <summary>Describes a card label, detail, and optional accent.</summary>
    public record ContextCardItem(string Label, string Description, string Accent = "green");
}
