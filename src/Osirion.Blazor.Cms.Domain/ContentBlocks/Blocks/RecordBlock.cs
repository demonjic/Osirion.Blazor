namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A record card: a status and labelled values, such as a sample decision (<c>type: record</c>).</summary>
public sealed record RecordBlock : ContentBlock
{
    /// <summary>Gets the status label (<c>status</c>).</summary>
    public string? Status { get; init; }

    /// <summary>Gets the tone of the status (<c>tone</c>).</summary>
    public ContentBlockTone Tone { get; init; }

    /// <summary>Gets the labelled values, in order (<c>rows</c>).</summary>
    public IReadOnlyList<RecordBlockRow> Rows { get; init; } = [];

    /// <summary>Gets the note under the record (<c>caption</c>), inline Markdown.</summary>
    public string? Caption { get; init; }
}

/// <summary>One row of a <see cref="RecordBlock"/>.</summary>
/// <param name="Label">The row's label (<c>label</c>).</param>
/// <param name="Value">The row's value (<c>value</c>).</param>
public sealed record RecordBlockRow(string Label, string Value);
