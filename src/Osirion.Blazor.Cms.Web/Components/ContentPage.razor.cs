using Microsoft.AspNetCore.Components;
#if !NET10_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Models;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using System.Text.RegularExpressions;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>Defines the ContentPage type.</summary>
public partial class ContentPage
{
    [Inject]
    private IContentProviderManager ContentProviderManager { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ILogger<ContentPage> Logger { get; set; } = default!;

    /// <summary>
    /// The URL of the item to load from the default content provider, as the provider stores it: no leading slash, no
    /// file extension and, for localized content, no locale folder (<c>blog/my-post</c>). The match is exact,
    /// including case. <see cref="IgnorePathSegment"/> and <see cref="PathReplacePattern"/> are applied first.
    /// Without a path, the page shows the item in <see cref="ContentItem"/>, or the not-found state when that is null
    /// too.
    /// </summary>
    [Parameter]
    public string? Path { get; set; }

    /// <summary>
    /// The item shown. When <see cref="Path"/> loads, the page sets this property and replaces a value you passed; it
    /// is not two-way bound, so use <see cref="OnContentLoaded"/> to receive the item. Without a path the page shows
    /// the item you pass and loads no neighbors, related items or default share links.
    /// </summary>
    [Parameter]
    public ContentItem? ContentItem { get; set; }

    /// <summary>
    /// The item the previous link points to. When <see cref="ShowNavigationLinks"/> is on and this or <see
    /// cref="NextItem"/> is null, the page looks both up with a query for the item's directory and locale, newest
    /// first, and replaces your values with what it finds.
    /// </summary>
    [Parameter]
    public ContentItem? PreviousItem { get; set; }

    /// <summary>
    /// The item the next link points to. See <see cref="PreviousItem"/> for how a missing neighbor is looked up.
    /// </summary>
    [Parameter]
    public ContentItem? NextItem { get; set; }

    /// <summary>
    /// The items shown in the related section. When <see cref="ShowRelatedContent"/> is on and this is null or empty,
    /// the page picks up to three items of the same locale that share a tag or a category with the item, most shared
    /// first, and sets this property.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ContentItem>? RelatedItems { get; set; }

    /// <summary>
    /// The text of the loading state, shown while the page waits for the item. It is not part of a statically
    /// rendered response, which is sent once the item has loaded.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading content...";

    /// <summary>
    /// The heading of the not-found state, shown when no item matches or the provider fails.
    /// </summary>
    [Parameter]
    public string NotFoundTitle { get; set; } = "Content Not Found";

    /// <summary>
    /// The message of the not-found state.
    /// </summary>
    [Parameter]
    public string NotFoundText { get; set; } = "The requested content could not be found.";

    /// <summary>
    /// Whether the not-found state has a link back, to <see cref="BackLinkUrl"/> with the text <see
    /// cref="BackLinkText"/>. It is not shown when the URL is empty.
    /// </summary>
    [Parameter]
    public bool ShowBackLink { get; set; } = true;

    /// <summary>
    /// The address of the back link in the not-found state.
    /// </summary>
    [Parameter]
    public string BackLinkUrl { get; set; } = "/";

    /// <summary>
    /// The text of the back link in the not-found state.
    /// </summary>
    [Parameter]
    public string BackLinkText { get; set; } = "Back to Home";

    /// <summary>
    /// Whether to render <c>ContentBreadcrumbs</c> for the item above the article.
    /// </summary>
    [Parameter]
    public bool ShowBreadcrumbs { get; set; } = true;

    /// <summary>
    /// Whether the breadcrumbs start with a home link.
    /// </summary>
    [Parameter]
    public bool ShowBreadcrumbHome { get; set; } = true;

    /// <summary>
    /// The address of the breadcrumbs' home link.
    /// </summary>
    [Parameter]
    public string BreadcrumbHomeUrl { get; set; } = "/";

    /// <summary>
    /// The text of the breadcrumbs' home link.
    /// </summary>
    [Parameter]
    public string BreadcrumbHomeText { get; set; } = "Home";

    /// <summary>
    /// Has no effect: the markup does not read it. The header with the author, date and reading time always renders.
    /// </summary>
    [Parameter]
    public bool ShowMetadata { get; set; } = true;

    /// <summary>
    /// Has no effect: the markup does not read it.
    /// </summary>
    [Parameter]
    public string? FeaturedImageCaption { get; set; }

    /// <summary>
    /// Has no effect: the markup does not read it.
    /// </summary>
    [Parameter]
    public bool ShowFooter { get; set; } = true;

    /// <summary>
    /// Whether the sidebar lists categories. The list renders only when the item has categories, and it holds every
    /// category of the site, not only the item's, without counts.
    /// </summary>
    [Parameter]
    public bool ShowCategories { get; set; } = true;

    /// <summary>
    /// The heading of the sidebar's category list.
    /// </summary>
    [Parameter]
    public string CategoriesTitle { get; set; } = "Categories:";

    /// <summary>
    /// Whether the sidebar shows a tag cloud. It renders only when the item has tags, and it holds every tag of the
    /// site, not only the item's, without counts.
    /// </summary>
    [Parameter]
    public bool ShowTags { get; set; } = true;

    /// <summary>
    /// The heading of the sidebar's tag cloud.
    /// </summary>
    [Parameter]
    public string TagsTitle { get; set; } = "Tags:";

    /// <summary>
    /// Whether to show the previous and next links under the article. Missing neighbors are looked up by the page,
    /// see <see cref="PreviousItem"/>.
    /// </summary>
    [Parameter]
    public bool ShowNavigationLinks { get; set; } = true;

    /// <summary>
    /// Has no effect: the markup does not read it. The previous link is labeled <c>Previous</c>.
    /// </summary>
    [Parameter]
    public string PreviousItemLabel { get; set; } = "Previous";

    /// <summary>
    /// Has no effect: the markup does not read it. The next link is labeled <c>Next</c>.
    /// </summary>
    [Parameter]
    public string NextItemLabel { get; set; } = "Next";

    /// <summary>
    /// Whether to show a share section. Without <see cref="ShareLinks"/> it offers Twitter, Facebook and LinkedIn
    /// links that carry the page's absolute address without query string or fragment; those defaults are created only
    /// when the page loads by <see cref="Path"/>.
    /// </summary>
    [Parameter]
    public bool ShowShareLinks { get; set; } = false;

    /// <summary>
    /// The heading of the share section.
    /// </summary>
    [Parameter]
    public string ShareLinksTitle { get; set; } = "Share:";

    /// <summary>
    /// The share links to show, each with <c>Name</c> (also the suffix of its CSS class), <c>Label</c>, <c>Url</c>
    /// and <c>Icon</c>. <c>Icon</c> is written as raw HTML without sanitizing, so never fill it from user or content
    /// input. The links open in a new tab with <c>rel="noopener noreferrer"</c>.
    /// </summary>
    [Parameter]
    public List<ShareLink> ShareLinks { get; set; } = new();

    /// <summary>
    /// HTML written after the article and the share links as raw markup, without sanitizing. Use trusted markup only.
    /// Nothing renders when it is null or white space.
    /// </summary>
    [Parameter]
    public string? FooterContent { get; set; }

    /// <summary>
    /// Whether to show related items, from <see cref="RelatedItems"/> or, when that is empty, found by the page.
    /// </summary>
    [Parameter]
    public bool ShowRelatedContent { get; set; } = false;

    /// <summary>
    /// The heading of the related section.
    /// </summary>
    [Parameter]
    public string RelatedContentTitle { get; set; } = "Related Content";

    /// <summary>
    /// The schema.org type written into the article's <c>itemtype</c> attribute (<c>https://schema.org/Article</c> by
    /// default). ContentPage writes no head metadata; render a <c>SeoMetadataRenderer</c> for that.
    /// </summary>
    [Parameter]
    public string SchemaType { get; set; } = "Article";

    /// <summary>
    /// The level of the heading that holds the article title, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>), passed to
    /// the <c>LocalizedContentView</c> inside. It defaults to <c>1</c>, the page's own heading; set <c>2</c> for a page
    /// embedded in another that has its own <c>h1</c>, so that page keeps a single one. A value outside the range is
    /// moved to the nearest level. The title keeps its look at every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 1;

    /// <summary>
    /// Has no effect: the page renders no directory links.
    /// </summary>
    [Parameter]
    public Func<DirectoryItem, string>? DirectoryUrlFormatter { get; set; }

    /// <summary>
    /// Builds a category link from the category name, for the links under the article and the sidebar list. Without
    /// it the links under the article are <c>/{locale}/category/{slug}</c> and the sidebar list uses its own default.
    /// </summary>
    [Parameter]
    public Func<string, string>? CategoryUrlFormatter { get; set; }

    /// <summary>
    /// Builds a tag link from the tag name, for the links under the article and the tag cloud. Without it the links
    /// under the article are <c>/{locale}/tag/{slug}</c> and the tag cloud uses its own default.
    /// </summary>
    [Parameter]
    public Func<string, string>? TagUrlFormatter { get; set; }

    /// <summary>
    /// Builds the links of the previous, next and related items from the item. Without it a previous or next link is
    /// <c>/{item.Path}</c> and a related link is the item's source path with <see cref="IgnorePathSegment"/> removed,
    /// <see cref="PathReplacePattern"/> applied and the <c>.md</c> or <c>.markdown</c> extension dropped.
    /// </summary>
    [Parameter]
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>
    /// A path segment that the public address leaves out. It is put in front of <see cref="Path"/> (as
    /// <c>segment/path</c>) when the path does not already contain it, before the lookup, and it is removed from the
    /// default links of related items.
    /// </summary>
    [Parameter]
    public string? IgnorePathSegment { get; set; }

    /// <summary>
    /// A regular expression and its replacement, separated by one vertical bar (<c>pattern|replacement</c>), applied
    /// to <see cref="Path"/> before the lookup and to the default links of related items. A value without exactly one
    /// bar is ignored, so the pattern cannot contain a bar, and a pattern that runs longer than 250 milliseconds is
    /// ignored too.
    /// </summary>
    [Parameter]
    public string? PathReplacePattern { get; set; }

    /// <summary>
    /// Raised with the item after the page loaded it by <see cref="Path"/>. It is not raised when nothing is found or
    /// when you passed <see cref="ContentItem"/> yourself.
    /// </summary>
    [Parameter]
    public EventCallback<ContentItem> OnContentLoaded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is loading.
    /// </summary>
    protected bool IsLoading { get; set; }

    /// <summary>
    /// Processes changes to component parameters.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Path)
            && (ContentItem is null || !string.Equals(Path, _loadedPath, StringComparison.Ordinal)))
        {
            await LoadContentAsync();
        }

        SignalWhenNotFound(ContentItem is null && !IsLoading);
        await base.OnParametersSetAsync();
    }

#if !NET10_0_OR_GREATER
    /// <summary>The request context; only available during static server-side rendering.</summary>
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
#endif

    // Under static SSR on .NET 9 a closed connection stops the provider calls; on .NET 10 and in interactive render
    // modes there is no request to follow.
#if NET10_0_OR_GREATER
    private static CancellationToken RequestAborted => CancellationToken.None;
#else
    private CancellationToken RequestAborted => HttpContext?.RequestAborted ?? CancellationToken.None;
#endif

    private readonly NotFoundSignal _notFound = new();

    // The not-found block is the page's answer, so the response says 404 with it instead of a soft 404.
    private void SignalWhenNotFound(bool notFound)
    {
        if (!notFound)
        {
            _notFound.Reset();
            return;
        }

#if NET10_0_OR_GREATER
        _notFound.Send(NavigationManager);
#else
        _notFound.Send(HttpContext);
#endif
    }

    // What this component produced itself, so a later Path change replaces it without touching values the parent supplied.
    private string? _loadedPath;
    private ContentItem? _loadedPrevious;
    private ContentItem? _loadedNext;
    private IReadOnlyList<ContentItem>? _loadedRelated;
    private List<ShareLink>? _defaultShareLinks;

    /// <summary>
    /// Loads the content item based on the specified path.
    /// </summary>
    private async Task LoadContentAsync()
    {
        IsLoading = true;
        StateHasChanged();

        ForgetLoadedValues();

        try
        {
            var provider = ContentProviderManager.GetDefaultProvider();
            if (provider is not null && !string.IsNullOrWhiteSpace(Path))
            {
                ContentItem = await provider.GetItemByUrlAsync(BuildSearchPath(Path), RequestAborted);
                await LoadSupplementaryContentAsync(provider);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load content {ContentPath}", Path);
            ContentItem = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Clears the values loaded for the previous path, leaving the ones the parent supplied.
    /// </summary>
    private void ForgetLoadedValues()
    {
        if (ReferenceEquals(PreviousItem, _loadedPrevious))
        {
            PreviousItem = null;
        }

        if (ReferenceEquals(NextItem, _loadedNext))
        {
            NextItem = null;
        }

        if (ReferenceEquals(RelatedItems, _loadedRelated))
        {
            RelatedItems = null;
        }

        if (ReferenceEquals(ShareLinks, _defaultShareLinks))
        {
            ShareLinks = new();
        }

        _loadedPath = Path;
        _loadedPrevious = _loadedNext = null;
        _loadedRelated = null;
        _defaultShareLinks = null;
    }

    /// <summary>
    /// Builds the URL to query from the page path: the ignored segment is added back, then the replace pattern applied.
    /// </summary>
    private string BuildSearchPath(string path)
    {
        var searchPath = path;
        if (!string.IsNullOrWhiteSpace(IgnorePathSegment) && !searchPath.Contains(IgnorePathSegment))
        {
            searchPath = $"{IgnorePathSegment}/{searchPath}";
        }

        return ApplyPathReplacePattern(searchPath);
    }

    /// <summary>
    /// Applies <see cref="PathReplacePattern"/> ("pattern|replacement") to a path; any other shape leaves it unchanged.
    /// </summary>
    private string ApplyPathReplacePattern(string path)
    {
        if (string.IsNullOrWhiteSpace(PathReplacePattern))
        {
            return path;
        }

        var parts = PathReplacePattern.Split('|');
        if (parts.Length != 2)
        {
            return path;
        }

        // The path comes from the URL, so a pattern that backtracks badly must not hold the request.
        try
        {
            return Regex.Replace(path, parts[0], parts[1], RegexOptions.None, PathReplaceTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            return path;
        }
    }

    private static readonly TimeSpan PathReplaceTimeout = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Loads what the page shows around the content: neighbours, related items and default share links.
    /// </summary>
    private async Task LoadSupplementaryContentAsync(IContentProvider provider)
    {
        if (ContentItem is { } item)
        {
            if (ShowNavigationLinks && (PreviousItem is null || NextItem is null))
            {
                await LoadNavigationItemsAsync(provider, item);
            }

            if (ShowRelatedContent && (RelatedItems is null || !RelatedItems.Any()))
            {
                await LoadRelatedItemsAsync(provider, item);
            }

            if (OnContentLoaded.HasDelegate)
            {
                await OnContentLoaded.InvokeAsync(item);
            }
        }

        if (ShowShareLinks && !ShareLinks.Any())
        {
            // Default share links if none provided and sharing is enabled
            InitializeDefaultShareLinks();
        }
    }

    /// <summary>
    /// Loads navigation items (previous and next) for the content item.
    /// </summary>
    private async Task LoadNavigationItemsAsync(IContentProvider provider, ContentItem item)
    {
        try
        {
            // Get items from the same directory and same locale for navigation
            var query = new ContentQuery
            {
                Directory = item.Directory?.Path,
                Locale = item.Locale,
                SortBy = SortField.Date,
                SortDirection = SortDirection.Descending
            };

            var items = await provider.GetItemsByQueryAsync(query, RequestAborted) ?? [];

            // Find the current item's index
            int currentIndex = -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Path == item.Path)
                {
                    currentIndex = i;
                    break;
                }
            }

            // Set previous and next items
            if (currentIndex > 0)
            {
                PreviousItem = _loadedPrevious = items[currentIndex - 1];
            }

            if (currentIndex >= 0 && currentIndex < items.Count - 1)
            {
                NextItem = _loadedNext = items[currentIndex + 1];
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load previous and next items for {ContentPath}", item.Path);
        }
    }

    /// <summary>
    /// Loads related content items based on tags and categories.
    /// </summary>
    private async Task LoadRelatedItemsAsync(IContentProvider provider, ContentItem item)
    {
        try
        {
            if (!item.Tags.Any() && !item.Categories.Any())
            {
                return;
            }

            // Get items with matching tags or categories
            var query = new ContentQuery
            {
                Locale = item.Locale,
                SortBy = SortField.Date,
                SortDirection = SortDirection.Descending
            };

            var allItems = await provider.GetItemsByQueryAsync(query, RequestAborted) ?? [];

            // Find related items (excluding the current item)
            var related = allItems
                .Where(i => i.Path != item.Path)
                .Where(i =>
                    i.Tags.Intersect(item.Tags, StringComparer.OrdinalIgnoreCase).Any() ||
                    i.Categories.Intersect(item.Categories, StringComparer.OrdinalIgnoreCase).Any())
                .OrderByDescending(i =>
                    i.Tags.Intersect(item.Tags, StringComparer.OrdinalIgnoreCase).Count() +
                    i.Categories.Intersect(item.Categories, StringComparer.OrdinalIgnoreCase).Count())
                .Take(3)
                .ToList();

            RelatedItems = _loadedRelated = related;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load related items for {ContentPath}", item.Path);
        }
    }

    /// <summary>
    /// Initializes default share links if none are provided.
    /// </summary>
    private void InitializeDefaultShareLinks()
    {
        // The absolute address of this page, without query string or fragment.
        var currentUrl = Uri.EscapeDataString(ContentUrls.StripQueryAndFragment(NavigationManager.Uri));
        var title = Uri.EscapeDataString(ContentItem?.Title ?? "");

        ShareLinks = _defaultShareLinks = new List<ShareLink>
        {
            new ShareLink
            {
                Name = "Twitter",
                Label = "Twitter",
                Url = $"https://twitter.com/intent/tweet?url={currentUrl}&text={title}",
                Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M22 4s-.7 2.1-2 3.4c1.6 10-9.4 17.3-18 11.6 2.2.1 4.4-.6 6-2C3 15.5.5 9.6 3 5c2.2 2.6 5.6 4.1 9 4-.9-4.2 4-6.6 7-3.8 1.1 0 3-1.2 3-1.2z\"></path></svg>"
            },
            new ShareLink
            {
                Name = "Facebook",
                Label = "Facebook",
                Url = $"https://www.facebook.com/sharer/sharer.php?u={currentUrl}",
                Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z\"></path></svg>"
            },
            new ShareLink
            {
                Name = "LinkedIn",
                Label = "LinkedIn",
                Url = $"https://www.linkedin.com/shareArticle?mini=true&url={currentUrl}&title={title}",
                Icon = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z\"></path><rect x=\"2\" y=\"9\" width=\"4\" height=\"12\"></rect><circle cx=\"4\" cy=\"4\" r=\"2\"></circle></svg>"
            }
        };
    }

    /// <summary>
    /// Gets the CSS class for the component.
    /// </summary>
    protected string GetContentPageClass()
    {
        return $"osirion-content-page-container {Class}".Trim();
    }

    // CategoriesList and TagCloud format from the category and tag models; adapt the string formatters so a host's
    // CategoryUrlFormatter and TagUrlFormatter apply to the sidebar links as well.
    private Func<ContentCategory, string>? CategoryListUrlFormatter =>
        CategoryUrlFormatter is { } formatter ? category => formatter(category.Name) : null;

    private Func<ContentTag, string>? TagCloudUrlFormatter =>
        TagUrlFormatter is { } formatter ? tag => formatter(tag.Name) : null;

    /// <summary>
    /// Gets the URL for a directory.
    /// </summary>
    protected string GetDirectoryUrl(DirectoryItem directory)
    {
        return DirectoryUrlFormatter?.Invoke(directory) ?? $"/{directory.Path}";
    }

    /// <summary>
    /// Gets the URL for a category.
    /// </summary>
    protected string GetCategoryUrl(string category)
    {
        return CategoryUrlFormatter?.Invoke(category) ?? ContentUrls.CategoryUrl(category);
    }

    /// <summary>
    /// Gets the URL for a tag.
    /// </summary>
    protected string GetTagUrl(string tag)
    {
        return TagUrlFormatter?.Invoke(tag) ?? ContentUrls.TagUrl(tag);
    }

    /// <summary>
    /// Gets the URL for a content item.
    /// </summary>
    protected string GetContentUrl(ContentItem item)
    {
        if (ContentUrlFormatter is not null)
        {
            return ContentUrlFormatter(item);
        }

        var path = item.Path;

        // Remove the IgnorePathSegment if specified
        if (!string.IsNullOrWhiteSpace(IgnorePathSegment) && path.Contains(IgnorePathSegment))
        {
            path = path.Replace($"{IgnorePathSegment}/", "");
        }

        path = ApplyPathReplacePattern(path);

        // Remove file extension if present
        if (path.EndsWith(".md") || path.EndsWith(".markdown"))
        {
            path = path.Substring(0, path.LastIndexOf('.'));
        }

        return $"/{path}";
    }
}
