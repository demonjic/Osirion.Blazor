namespace Osirion.Blazor.Components;

/// <summary>
/// The side of <see cref="OsirionSplitSection"/> the aside sits on, on screens wide enough to show both columns.
/// </summary>
public enum SplitAsidePosition
{
    /// <summary>After the copy: on the right in a left-to-right page.</summary>
    End,

    /// <summary>Before the copy: on the left in a left-to-right page.</summary>
    Start
}
