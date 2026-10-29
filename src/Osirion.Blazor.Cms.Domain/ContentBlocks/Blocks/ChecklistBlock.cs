namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A list of checked points (<c>type: checklist</c>).</summary>
public sealed record ChecklistBlock : ContentBlock
{
    /// <summary>Gets the points, in order (<c>items</c>); never empty on a parsed block.</summary>
    public IReadOnlyList<ChecklistBlockItem> Items { get; init; } = [];
}

/// <summary>One point of a <see cref="ChecklistBlock"/>.</summary>
/// <param name="Text">The point (<c>text</c>).</param>
/// <param name="Description">More about it (<c>description</c>).</param>
public sealed record ChecklistBlockItem(string Text, string? Description);
