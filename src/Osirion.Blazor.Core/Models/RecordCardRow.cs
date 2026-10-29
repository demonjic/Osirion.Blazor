namespace Osirion.Blazor.Core.Models;

/// <summary>
/// A labelled row of <see cref="Osirion.Blazor.Components.OsirionRecordCard"/>.
/// </summary>
/// <param name="Label">What the row is, for example <c>Order number</c>.</param>
/// <param name="Value">The row's value, for example <c>A-1042</c>, as plain text.</param>
public sealed record RecordCardRow(string Label, string Value);
