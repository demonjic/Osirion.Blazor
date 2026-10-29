namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>An image with an optional caption (<c>type: image</c>).</summary>
public sealed record ImageBlock : ContentBlock
{
    /// <summary>Gets the image address (<c>src</c>): a site-relative path or an <c>https:</c> URL.</summary>
    public required string Src { get; init; }

    /// <summary>Gets the text alternative (<c>alt</c>); empty for a decorative image.</summary>
    public required string Alt { get; init; }

    /// <summary>Gets the caption (<c>caption</c>), inline Markdown.</summary>
    public string? Caption { get; init; }
}
