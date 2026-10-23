using Osirion.Blazor.Components;

namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// The button classes of each CSS framework, shared by <see cref="OsirionComponentBase"/> and markup rendered outside
/// it.
/// </summary>
internal static class ButtonClasses
{
    /// <summary>
    /// Gets the classes of a primary button under <paramref name="framework"/>; an unknown framework gets the neutral
    /// Osirion classes.
    /// </summary>
    public static string Primary(CssFramework framework) => framework switch
    {
        CssFramework.Bootstrap => "btn btn-primary",
        CssFramework.FluentUI => "osirion-button osirion-button-primary",
        CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-primary",
        CssFramework.Radzen => "rz-button rz-button-primary",
        _ => "osirion-button osirion-button-primary"
    };
}
