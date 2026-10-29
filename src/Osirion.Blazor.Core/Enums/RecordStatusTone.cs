namespace Osirion.Blazor.Components;

/// <summary>
/// The tone of the status on <see cref="OsirionRecordCard"/>. The tone colors the status, and the status text says
/// what it is, so the meaning never rests on color alone.
/// </summary>
public enum RecordStatusTone
{
    /// <summary>A status with no judgment, such as <c>Pending</c>.</summary>
    Neutral,

    /// <summary>A good outcome, such as <c>Approved</c>.</summary>
    Positive,

    /// <summary>A bad outcome, such as <c>Rejected</c>.</summary>
    Negative,

    /// <summary>An outcome that needs attention, such as <c>On hold</c>.</summary>
    Warning
}
