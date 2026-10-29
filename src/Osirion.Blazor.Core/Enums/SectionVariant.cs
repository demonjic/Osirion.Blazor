namespace Osirion.Blazor.Components;

/// <summary>
/// The background band of <see cref="OsirionBaseSection"/>.
/// </summary>
public enum SectionVariant
{
    /// <summary>The page's own background, <c>--osirion-background-primary</c>.</summary>
    Default,

    /// <summary>A lightly tinted band in <c>--osirion-background-secondary</c>, to set a section apart.</summary>
    Tinted,

    /// <summary>A dark band: the section takes the dark theme's tokens, whatever the page's theme.</summary>
    Dark
}
