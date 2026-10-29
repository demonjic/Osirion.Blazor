namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>The page's opening section, with the page heading (<c>type: hero</c>).</summary>
public sealed record HeroBlock : ContentBlock
{
    /// <summary>Gets the line under the heading (<c>subtitle</c>), inline Markdown.</summary>
    public string? Subtitle { get; init; }

    /// <summary>Gets the short points shown with the copy (<c>highlights</c>).</summary>
    public IReadOnlyList<string> Highlights { get; init; } = [];

    /// <summary>Gets the main button (<c>primary_action</c>).</summary>
    public ContentBlockAction? PrimaryAction { get; init; }

    /// <summary>Gets the second button (<c>secondary_action</c>).</summary>
    public ContentBlockAction? SecondaryAction { get; init; }

    /// <summary>Gets the image beside the copy (<c>image</c>).</summary>
    public ContentBlockImage? Image { get; init; }

    /// <summary>
    /// Gets the block shown beside the copy instead of <see cref="Image"/> (<c>aside</c>): a <see cref="RecordBlock"/>
    /// or an <see cref="ImageBlock"/>.
    /// </summary>
    public ContentBlock? Aside { get; init; }
}
