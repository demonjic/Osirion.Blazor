namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>Questions and answers (<c>type: faq</c>).</summary>
public sealed record FaqBlock : ContentBlock
{
    /// <summary>Gets the questions, in order (<c>items</c>); never empty on a parsed block.</summary>
    public IReadOnlyList<FaqBlockItem> Items { get; init; } = [];
}

/// <summary>One question of a <see cref="FaqBlock"/>.</summary>
/// <param name="Question">The question (<c>question</c>).</param>
/// <param name="Answer">The answer (<c>answer</c>), block Markdown.</param>
public sealed record FaqBlockItem(string Question, string Answer);
