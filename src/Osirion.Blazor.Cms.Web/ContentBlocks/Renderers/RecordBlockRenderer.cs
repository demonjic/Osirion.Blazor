using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Components;
using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="RecordBlock"/> as an <see cref="OsirionRecordCard"/>, on its own or as an aside. The
/// <c>caption</c> is the card's footnote, and the <c>tone</c> maps to the card's status tones: <c>neutral</c> and
/// <c>info</c> to neutral, <c>success</c> to positive, <c>warning</c> to warning, <c>danger</c> to negative.
/// </summary>
internal sealed class RecordBlockRenderer : ContentBlockRenderer<RecordBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var record = Model;
        builder.OpenComponent<OsirionRecordCard>(0);
        if (record.Id is not null)
        {
            builder.AddComponentParameter(1, "id", record.Id);
        }

        builder.AddComponentParameter(2, nameof(OsirionRecordCard.Title), NullIfEmpty(Context.PlainText(record.Title)));
        builder.AddComponentParameter(3, nameof(OsirionRecordCard.Status), NullIfEmpty(Context.PlainText(record.Status)));
        builder.AddComponentParameter(4, nameof(OsirionRecordCard.StatusTone), Tone(record.Tone));
        builder.AddComponentParameter(5, nameof(OsirionRecordCard.Rows),
            record.Rows.Select(row => new RecordCardRow(Context.PlainText(row.Label), Context.PlainText(row.Value))).ToList());
        builder.AddComponentParameter(6, nameof(OsirionRecordCard.Footnote), NullIfEmpty(Context.PlainText(record.Caption)));
        builder.CloseComponent();
    }

    /// <summary>Maps the contract's tone to the record card's status tone.</summary>
    private static RecordStatusTone Tone(ContentBlockTone tone) => tone switch
    {
        ContentBlockTone.Success => RecordStatusTone.Positive,
        ContentBlockTone.Warning => RecordStatusTone.Warning,
        ContentBlockTone.Danger => RecordStatusTone.Negative,
        _ => RecordStatusTone.Neutral,
    };

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;
}
