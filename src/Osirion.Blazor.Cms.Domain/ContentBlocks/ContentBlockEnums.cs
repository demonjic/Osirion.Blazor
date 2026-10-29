namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>The band style of a block (<c>variant</c>).</summary>
public enum ContentBlockVariant
{
    /// <summary>The page's own background (<c>default</c>).</summary>
    Default,

    /// <summary>A lightly tinted band (<c>tinted</c>).</summary>
    Tinted,

    /// <summary>A dark band (<c>dark</c>).</summary>
    Dark,
}

/// <summary>The tone of a record's status (<c>tone</c>).</summary>
public enum ContentBlockTone
{
    /// <summary>No judgement (<c>neutral</c>).</summary>
    Neutral,

    /// <summary>A good outcome (<c>success</c>).</summary>
    Success,

    /// <summary>Needs attention (<c>warning</c>).</summary>
    Warning,

    /// <summary>A refusal or a failure (<c>danger</c>).</summary>
    Danger,

    /// <summary>Information (<c>info</c>).</summary>
    Info,
}

/// <summary>The side a split block's aside is shown on (<c>aside_position</c>).</summary>
public enum ContentBlockAsidePosition
{
    /// <summary>After the copy, on the right in a left-to-right language (<c>end</c>).</summary>
    End,

    /// <summary>Before the copy (<c>start</c>).</summary>
    Start,
}

/// <summary>Whether a content item with blocks also renders its Markdown body (<c>body</c>).</summary>
public enum ContentBodyPlacement
{
    /// <summary>Only the blocks render; the body is not shown. The default when <c>body</c> is not set.</summary>
    Omitted,

    /// <summary>The body renders after the blocks (<c>body: after</c>).</summary>
    AfterBlocks,
}
