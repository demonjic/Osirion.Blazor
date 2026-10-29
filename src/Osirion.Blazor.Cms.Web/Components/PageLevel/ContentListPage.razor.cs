using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// A whole list page: a blog index, a category or a tag page. It opens with the title as the page's <c>h1</c> and a
/// short description, lists the items with pagination, writes the list's CollectionPage structured data and, when
/// <see cref="FeedUrl"/> is set, links the feed for readers and feed discovery. Categories and tags can be shown
/// beside the list. A category, tag or author that no item matches answers HTTP 404 (see
/// <see cref="ContentList.NotFoundWhenFilterMatchesNothing"/>). It renders without script.
/// </summary>
public partial class ContentListPage
{
    /// <summary>
    /// Gets or sets the page title, shown as the <c>h1</c> and used in the SEO metadata. Defaults to one built from
    /// the filter, for example <c>Tag: blazor</c> or the directory's name.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sentence under the title, also the meta description. Defaults to one built from the filter.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the image the page shares in its social cards, for example a card made for the blog index. Without
    /// it the page shares the image of the section's own <c>_index.md</c> when the list is shown at the section's
    /// address, then the first listed item's featured image, then the site-wide default image. See
    /// <see cref="ContentList.PageImageUrl"/>.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the content directory to list, for example <c>blog</c>.</summary>
    [Parameter]
    public string? Directory { get; set; }

    /// <summary>Gets or sets the category to list; the category page passes its route value here.</summary>
    [Parameter]
    public string? Category { get; set; }

    /// <summary>Gets or sets the tag to list; the tag page passes its route value here.</summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>Gets or sets the author whose items are listed, for an author's page.</summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>Gets or sets the locale of the listed items.</summary>
    [Parameter]
    public string? Locale { get; set; }

    /// <summary>Gets or sets whether the list is paged. Defaults to <see langword="true"/>.</summary>
    [Parameter]
    public bool ShowPagination { get; set; } = true;

    /// <summary>Gets or sets the number of items on a page. Defaults to 12, four rows of three cards.</summary>
    [Parameter]
    public int ItemsPerPage { get; set; } = 12;

    /// <summary>Gets or sets the sort field. Defaults to the publication date.</summary>
    [Parameter]
    public SortField SortBy { get; set; } = SortField.Date;

    /// <summary>Gets or sets the sort direction. Defaults to newest first.</summary>
    [Parameter]
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;

    /// <summary>Gets or sets the text shown when nothing matches.</summary>
    [Parameter]
    public string NoContentText { get; set; } = "Nothing has been published here yet.";

    /// <summary>Gets or sets how an item's URL is built. Defaults to the item's own URL.</summary>
    [Parameter]
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>Gets or sets how a page's URL is built. Defaults to the current URL with <c>?page=n</c>.</summary>
    [Parameter]
    public Func<int, string>? PaginationUrlFormatter { get; set; }

    /// <summary>Gets or sets the site name used in the SEO metadata.</summary>
    [Parameter]
    public string? WebsiteName { get; set; }

    /// <summary>
    /// Gets or sets the markup of one item, rendered in an ordered list instead of the built-in cards. See
    /// <see cref="ContentList.ItemTemplate"/>.
    /// </summary>
    [Parameter]
    public RenderFragment<ContentItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the page's header, shown above the list instead of the title and description, for example the
    /// title with a strip of series links under it. It must render the page's <c>h1</c>. <see cref="Title"/> and
    /// <see cref="Description"/> still name the page in its head metadata. When <see langword="null"/>, the heading
    /// is shown as before.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets the URL of the RSS feed, for example <c>/feed.xml</c> from <c>MapOsirionFeeds</c>. When set, the
    /// page links the feed in its head for feed readers and below the list for people.
    /// </summary>
    [Parameter]
    public string? FeedUrl { get; set; }

    /// <summary>Gets or sets the feed's title and the text of its link. Defaults to <c>RSS feed</c>.</summary>
    [Parameter]
    public string FeedTitle { get; set; } = "RSS feed";

    /// <summary>Gets or sets whether the categories are listed beside the list. Defaults to <see langword="false"/>.</summary>
    [Parameter]
    public bool ShowCategories { get; set; }

    /// <summary>Gets or sets the heading of the categories. Defaults to <c>Categories</c>.</summary>
    [Parameter]
    public string CategoriesTitle { get; set; } = "Categories";

    /// <summary>Gets or sets how a category's URL is built.</summary>
    [Parameter]
    public Func<ContentCategory, string>? CategoryUrlFormatter { get; set; }

    /// <summary>Gets or sets whether the tags are shown beside the list. Defaults to <see langword="false"/>.</summary>
    [Parameter]
    public bool ShowTags { get; set; }

    /// <summary>Gets or sets the heading of the tags. Defaults to <c>Tags</c>.</summary>
    [Parameter]
    public string TagsTitle { get; set; } = "Tags";

    /// <summary>Gets or sets how a tag's URL is built.</summary>
    [Parameter]
    public Func<ContentTag, string>? TagUrlFormatter { get; set; }

    /// <summary>Gets or sets the accessible name of the sidebar. Defaults to <c>Browse</c>.</summary>
    [Parameter]
    public string SidebarLabel { get; set; } = "Browse";

    private bool HasSidebar => ShowCategories || ShowTags;

    private string GetPageClass() => CombineCssClasses(
        HasSidebar ? "osirion-content-list-page osirion-content-list-page-with-sidebar" : "osirion-content-list-page");
}
