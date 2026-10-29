namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A section with copy on one side and a block on the other (<c>type: split</c>).</summary>
public sealed record SplitBlock : ContentBlock
{
    /// <summary>Gets the copy (<c>markdown</c>), block Markdown.</summary>
    public string? Markdown { get; init; }

    /// <summary>
    /// Gets the block beside the copy (<c>aside</c>): a <see cref="RecordBlock"/>, <see cref="ChecklistBlock"/>,
    /// <see cref="ImageBlock"/>, <see cref="MetricsBlock"/> or <see cref="MarkdownBlock"/>.
    /// </summary>
    public ContentBlock? Aside { get; init; }

    /// <summary>Gets the side the aside is shown on (<c>aside_position</c>).</summary>
    public ContentBlockAsidePosition AsidePosition { get; init; }
}
