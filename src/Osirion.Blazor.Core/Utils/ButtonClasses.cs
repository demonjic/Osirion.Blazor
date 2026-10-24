using Osirion.Blazor.Components;

namespace Osirion.Blazor.Core.Utils;

/// <summary>
/// The button classes of each CSS framework that <see cref="OsirionComponentBase"/> renders, for markup outside it,
/// such as a layout that inherits <see cref="Microsoft.AspNetCore.Components.LayoutComponentBase"/>.
/// </summary>
public static class ButtonClasses
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

    /// <summary>
    /// Gets the classes of a secondary button under <paramref name="framework"/>; an unknown framework gets the neutral
    /// Osirion classes. Every section's secondary button, such as <c>HeroSection</c>'s and
    /// <c>OsirionCallToActionSection</c>'s, takes these classes, so they look the same.
    /// </summary>
    public static string Secondary(CssFramework framework) => framework switch
    {
        CssFramework.Bootstrap => "btn btn-secondary",
        CssFramework.FluentUI => "osirion-button osirion-button-secondary",
        CssFramework.MudBlazor => "mud-button mud-button-filled mud-button-filled-secondary",
        CssFramework.Radzen => "rz-button rz-button-secondary",
        _ => "osirion-button osirion-button-secondary"
    };
}
