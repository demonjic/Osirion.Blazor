namespace Osirion.Blazor.Core.Models;

/// <summary>
/// An item of <see cref="Osirion.Blazor.Components.OsirionChecklist"/>.
/// </summary>
/// <param name="Title">The short phrase next to the check mark, for example <c>Works without script</c>.</param>
/// <param name="Description">An optional line under the phrase, as plain text.</param>
public sealed record ChecklistItem(string Title, string? Description = null);
