namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A call to action (<c>type: cta</c>).</summary>
public sealed record CtaBlock : ContentBlock
{
    /// <summary>Gets the main button (<c>primary_action</c>).</summary>
    public ContentBlockAction? PrimaryAction { get; init; }

    /// <summary>Gets the second button (<c>secondary_action</c>).</summary>
    public ContentBlockAction? SecondaryAction { get; init; }
}
