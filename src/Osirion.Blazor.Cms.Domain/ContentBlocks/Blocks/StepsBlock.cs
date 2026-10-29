namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>An ordered sequence of steps (<c>type: steps</c>).</summary>
public sealed record StepsBlock : ContentBlock
{
    /// <summary>Gets the steps, in order (<c>items</c>); never empty on a parsed block.</summary>
    public IReadOnlyList<StepsBlockItem> Items { get; init; } = [];
}

/// <summary>One step of a <see cref="StepsBlock"/>.</summary>
/// <param name="Title">The step's name (<c>title</c>).</param>
/// <param name="Description">What happens in the step (<c>description</c>).</param>
public sealed record StepsBlockItem(string Title, string? Description);
