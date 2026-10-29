using Osirion.Blazor.Core.Models;

namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// Turns a list of short phrases into <see cref="ChecklistItem"/>s, for the components that show phrases through
/// <see cref="Osirion.Blazor.Components.OsirionChecklist"/>: the hero's highlights and the feature card's items.
/// </summary>
internal static class ChecklistPhrases
{
    /// <summary>
    /// The phrases as checklist items, in order, without the empty ones; an empty list for <see langword="null"/>.
    /// </summary>
    public static IReadOnlyList<ChecklistItem> ToItems(IReadOnlyList<string>? phrases) =>
        phrases is null
            ? []
            : [.. phrases.Where(phrase => !string.IsNullOrWhiteSpace(phrase)).Select(phrase => new ChecklistItem(phrase))];
}
