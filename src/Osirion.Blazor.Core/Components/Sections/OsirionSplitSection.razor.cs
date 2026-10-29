using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>
/// Copy beside an aside, such as a paragraph and a list next to an image or a sample record: two equal columns from
/// 768px, and below that the aside stacked under the copy. It renders without script; put it inside a section such
/// as <see cref="OsirionBaseSection"/> for the heading and the band.
/// </summary>
public partial class OsirionSplitSection
{
    /// <summary>
    /// The copy: text, lists and buttons. It comes first in reading order.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// The block beside the copy, such as an image, a card or a code sample. It comes after the copy in reading order
    /// and stacks under it on narrow screens.
    /// </summary>
    [Parameter]
    public RenderFragment? Aside { get; set; }

    /// <summary>
    /// The side the aside sits on when both columns show: <c>End</c> (the default, on the right in a left-to-right
    /// page) or <c>Start</c>. The reading order stays copy first either way.
    /// </summary>
    [Parameter]
    public SplitAsidePosition AsidePosition { get; set; } = SplitAsidePosition.End;

    private string GetSplitClass() => CombineCssClasses(AsidePosition == SplitAsidePosition.Start
        ? "osirion-split-section osirion-split-section-aside-start"
        : "osirion-split-section");
}
