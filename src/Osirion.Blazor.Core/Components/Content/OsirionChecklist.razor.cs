using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// A list of short phrases, each behind a check mark and with an optional line of description, under an optional
/// heading. It renders a plain <c>ul</c> without script, and renders nothing when it has nothing to show.
/// </summary>
public partial class OsirionChecklist
{
    /// <summary>
    /// The heading above the list, rendered at <see cref="HeadingLevel"/>; it also names the list for screen readers.
    /// With an empty value no heading is rendered.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// The level of the heading, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It defaults to <c>3</c>, a list inside
    /// a section that has its own <c>h2</c>. A value outside the range is moved to the nearest level. The heading keeps
    /// its look at every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 3;

    /// <summary>
    /// The items, in order. With none, only the heading is rendered.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ChecklistItem> Items { get; set; } = [];

    // Generated once per instance, so the heading id and aria-labelledby agree on every render and two lists on a page
    // never repeat an id.
    private readonly string _headingId = $"osirion-checklist-{Guid.NewGuid():N}";

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    private bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private bool HasItems => Items is { Count: > 0 };

    private string GetChecklistClass() => CombineCssClasses("osirion-checklist");
}
