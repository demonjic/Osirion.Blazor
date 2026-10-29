namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A section with a heading and Markdown copy (<c>type: section</c>).</summary>
public sealed record SectionBlock : ContentBlock
{
    /// <summary>Gets the section's copy (<c>markdown</c>), block Markdown.</summary>
    public string? Markdown { get; init; }
}
