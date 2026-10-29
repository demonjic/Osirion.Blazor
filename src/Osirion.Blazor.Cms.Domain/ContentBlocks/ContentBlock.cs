namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// One block of a content item whose front matter has <c>blocks:</c>: a page section such as a hero, a list of
/// steps or a call to action, rendered in order by <c>OsirionContentBlocks</c>.
/// </summary>
/// <remarks>
/// Blocks are read from the front matter by <see cref="ContentBlockParser"/>, which validates them, so a block on a
/// loaded item always has a known <see cref="Variant"/>, an <see cref="Id"/> that matches <c>[a-z0-9-]+</c> and links
/// with an allowed scheme. Text values are plain text or inline Markdown, as written in the file; the renderer
/// formats and sanitizes them.
/// </remarks>
public abstract record ContentBlock
{
    /// <summary>Gets the block type as written in the file (<c>type</c>), for example <c>hero</c>.</summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the position of the block in the item's <c>blocks</c> list, starting at 0. A block nested in another
    /// (an <c>aside</c>) has the position of the block that holds it.
    /// </summary>
    public int Index { get; init; }

    /// <summary>Gets the section's anchor (<c>id</c>), or <see langword="null"/> when the block has none.</summary>
    public string? Id { get; init; }

    /// <summary>Gets the band style (<c>variant</c>).</summary>
    public ContentBlockVariant Variant { get; init; }

    /// <summary>Gets the small label above the title (<c>eyebrow</c>).</summary>
    public string? Eyebrow { get; init; }

    /// <summary>Gets the block heading (<c>title</c>).</summary>
    public string? Title { get; init; }

    /// <summary>Gets the lead paragraph under the title (<c>description</c>), inline Markdown.</summary>
    public string? Description { get; init; }
}
