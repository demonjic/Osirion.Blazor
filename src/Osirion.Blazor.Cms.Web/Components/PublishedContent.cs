using Osirion.Blazor.Cms.Domain.Entities;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Decides whether a page or view shows an item it looked up itself by path or address. The providers' single-item
/// lookups return drafts, because the admin edits them, while lists, the sitemap and the feeds leave drafts out. The
/// public components therefore show only published items, so a draft answers like a missing one, unless the host
/// turns on previews with the component's <c>ShowUnpublished</c> parameter. An item the parent passes in is the
/// host's choice and is not checked.
/// </summary>
internal static class PublishedContent
{
    /// <summary>
    /// Returns <paramref name="item"/> when the component may show it: it is published
    /// (<see cref="ContentItem.IsPublished"/>), or <paramref name="showUnpublished"/> is on. Otherwise returns
    /// <see langword="null"/>, which the component renders as not found.
    /// </summary>
    /// <param name="item">The item the provider returned, or <see langword="null"/> when it found none.</param>
    /// <param name="showUnpublished">Whether the host asked to show drafts and other unpublished items.</param>
    /// <returns>The item, or <see langword="null"/> when it is missing or must not be shown.</returns>
    public static ContentItem? ShownOrNull(ContentItem? item, bool showUnpublished) =>
        item is not null && (showUnpublished || item.IsPublished) ? item : null;
}
