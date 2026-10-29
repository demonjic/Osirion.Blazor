using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Components;

/// <summary>
/// A framed sample record, such as a decision, an order or a ticket, for a page that shows what a product produces: a
/// title, a status with a tone, labelled rows and a footnote such as <c>Sample data</c>. It renders a <c>figure</c>
/// with a description list, without script, and renders nothing when it has nothing to show.
/// </summary>
public partial class OsirionRecordCard
{
    /// <summary>
    /// The record's name, for example <c>Order A-1042</c>, as a heading at <see cref="HeadingLevel"/>.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// The level of the title, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It defaults to <c>3</c>, a card inside
    /// a section that has its own <c>h2</c>. A value outside the range is moved to the nearest level. The title keeps
    /// its look at every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 3;

    /// <summary>
    /// The record's status as text, for example <c>Approved</c>, shown as a badge beside the title. The text carries
    /// the meaning; <see cref="StatusTone"/> only colors it.
    /// </summary>
    [Parameter]
    public string? Status { get; set; }

    /// <summary>
    /// The color of the status: <c>Neutral</c> (the default), <c>Positive</c>, <c>Negative</c> or <c>Warning</c>.
    /// </summary>
    [Parameter]
    public RecordStatusTone StatusTone { get; set; } = RecordStatusTone.Neutral;

    /// <summary>
    /// The labelled values, in order, rendered as a description list.
    /// </summary>
    [Parameter]
    public IReadOnlyList<RecordCardRow> Rows { get; set; } = [];

    /// <summary>
    /// A short note under the rows, for example <c>Sample data</c>, rendered as the figure's caption.
    /// </summary>
    [Parameter]
    public string? Footnote { get; set; }

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    private bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    private bool HasStatus => !string.IsNullOrWhiteSpace(Status);

    private bool HasRows => Rows is { Count: > 0 };

    private bool HasFootnote => !string.IsNullOrWhiteSpace(Footnote);

    private bool HasContent => HasTitle || HasStatus || HasRows || HasFootnote;

    private string GetToneName() => StatusTone switch
    {
        RecordStatusTone.Positive => "positive",
        RecordStatusTone.Negative => "negative",
        RecordStatusTone.Warning => "warning",
        _ => "neutral"
    };

    private string GetRecordCardClass() => CombineCssClasses("osirion-record-card");
}
