namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A grid of cards (<c>type: cards</c>).</summary>
public sealed record CardsBlock : ContentBlock
{
    /// <summary>The number of columns when <c>columns</c> is not set.</summary>
    public const int DefaultColumns = 3;

    /// <summary>Gets the number of columns on a wide screen (<c>columns</c>): 2, 3 or 4.</summary>
    public int Columns { get; init; } = DefaultColumns;

    /// <summary>Gets the cards, in order (<c>items</c>); never empty on a parsed block.</summary>
    public IReadOnlyList<CardsBlockItem> Items { get; init; } = [];
}

/// <summary>One card of a <see cref="CardsBlock"/>.</summary>
public sealed record CardsBlockItem
{
    /// <summary>Gets the card's heading (<c>title</c>).</summary>
    public required string Title { get; init; }

    /// <summary>Gets the card's text (<c>description</c>).</summary>
    public string? Description { get; init; }

    /// <summary>Gets the name of an icon from the site's icon set (<c>icon</c>).</summary>
    public string? Icon { get; init; }

    /// <summary>Gets where the card links to (<c>url</c>).</summary>
    public string? Url { get; init; }

    /// <summary>Gets the text of the card's link (<c>link_text</c>).</summary>
    public string? LinkText { get; init; }

    /// <summary>Gets the short points listed on the card (<c>items</c>).</summary>
    public IReadOnlyList<string> Items { get; init; } = [];
}
