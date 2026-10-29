namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>Markdown copy in a plain section (<c>type: markdown</c>).</summary>
public sealed record MarkdownBlock : ContentBlock
{
    /// <summary>Gets the copy (<c>markdown</c>), block Markdown.</summary>
    public required string Markdown { get; init; }
}
