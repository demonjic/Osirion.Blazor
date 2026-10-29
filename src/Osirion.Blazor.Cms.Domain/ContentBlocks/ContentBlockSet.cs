namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>The blocks of a content item, and whether its body renders after them.</summary>
/// <param name="Blocks">The blocks, in the order of the file.</param>
/// <param name="Body">Whether the item's Markdown body renders after the blocks.</param>
public sealed record ContentBlockSet(IReadOnlyList<ContentBlock> Blocks, ContentBodyPlacement Body)
{
    /// <summary>Gets the set of an item without <c>blocks</c>.</summary>
    public static ContentBlockSet Empty { get; } = new([], ContentBodyPlacement.Omitted);
}
