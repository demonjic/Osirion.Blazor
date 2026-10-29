using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>Defines the ContentRenderer type.</summary>
public partial class ContentRenderer
{
    /// <summary>
    /// The item to render. Its <c>Content</c> (HTML) is rendered sanitized, with code highlighting. An item with content
    /// blocks renders them with <see cref="OsirionContentBlocks"/> instead, followed by its <c>Content</c> only when its
    /// front matter has <c>body: after</c>. Only the empty wrapper renders when the item or its content is null. The
    /// component loads nothing, so get the item from the content provider first.
    /// </summary>
    [Parameter]
    public ContentItem? Item { get; set; }

    /// <summary>
    /// Gets or sets whether to leave out the content's opening top-level heading (a Markdown <c># Title</c> line).
    /// Set it when the page already shows the item's title as its heading, so the title appears once and the page
    /// has a single <c>h1</c>.
    /// </summary>
    [Parameter]
    public bool OmitLeadingTitle { get; set; }

    // An item written as blocks shows its body only with body: after; any other item shows it as it always did.
    private bool ShowsBody => Item is not { HasBlocks: true } || Item.BodyPlacement == ContentBodyPlacement.AfterBlocks;

    private string GetContentViewClass()
    {
        return $"osirion-content-view {Class}".Trim();
    }
}
