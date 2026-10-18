using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components.Seo;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>Defines the ContentList type.</summary>
public partial class ContentList : IDisposable
{
    [Inject]
    private IContentProviderManager ContentProviderManager { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ILogger<ContentList> Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the content directory path.
    /// </summary>
    [Parameter]
    public string? Directory { get; set; }

    /// <summary>
    /// Gets or sets the category filter.
    /// </summary>
    [Parameter]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the tag filter.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the author filter: only items whose <c>author</c> front matter matches, ignoring case. Use it for
    /// an author's page.
    /// </summary>
    [Parameter]
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets whether a <see cref="Category"/>, <see cref="Tag"/> or <see cref="Author"/> that no item matches
    /// answers HTTP 404, so <c>/tag/no-such-tag</c> is not indexed as an empty page. On .NET 10 the router then
    /// renders its not-found page, if it has one; otherwise the list shows <see cref="NoContentText"/>. Without a
    /// filter an empty list stays a normal page, and a failed load never answers 404. Defaults to <see langword="false"/>; turn it on when the list is the page, as
    /// <see cref="ContentListPage"/> does, and leave it off for a list embedded in another page.
    /// </summary>
    [Parameter]
    public bool NotFoundWhenFilterMatchesNothing { get; set; }

    /// <summary>
    /// Gets or sets the search query for filtering content.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "q")]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets whether to show only featured content.
    /// </summary>
    [Parameter]
    public bool? OnlyFeatured { get; set; }

    /// <summary>
    /// Gets or sets the number of featured items to display.
    /// </summary>
    [Parameter]
    public int? FeaturedCount { get; set; }

    /// <summary>
    /// Gets or sets the text to display when content is loading.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading content...";

    /// <summary>
    /// Gets or sets the text to display when no content is available.
    /// </summary>
    [Parameter]
    public string NoContentText { get; set; } = "No content available.";

    /// <summary>
    /// Gets or sets the text for the "Read more" link.
    /// </summary>
    [Parameter]
    public string ReadMoreText { get; set; } = "Read more";

    /// <summary>
    /// Gets or sets the content URL formatter function.
    /// </summary>
    [Parameter]
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>
    /// Gets or sets whether to show pagination.
    /// </summary>
    [Parameter]
    public bool ShowPagination { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of items to display per page.
    /// </summary>
    [Parameter]
    public int ItemsPerPage { get; set; } = 10;

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    [SupplyParameterFromQuery(Name = "page")]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Gets or sets the event callback for page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> PageChanged { get; set; }

    /// <summary>
    /// Gets or sets the pagination URL formatter function.
    /// </summary>
    [Parameter]
    public Func<int, string>? PaginationUrlFormatter { get; set; }

    /// <summary>
    /// Gets or sets the locale for content filtering.
    /// </summary>
    [Parameter]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the directory ID for content filtering.
    /// </summary>
    [Parameter]
    public string? DirectoryId { get; set; }

    /// <summary>
    /// Gets or sets the path segment to ignore in URL construction.
    /// </summary>
    [Parameter]
    public string? IgnorePathSegment { get; set; }

    /// <summary>
    /// Gets or sets the custom path replacement pattern (regex).
    /// </summary>
    [Parameter]
    public string? PathReplacePattern { get; set; }

    /// <summary>
    /// Gets or sets the sort field for content items.
    /// </summary>
    [Parameter]
    public SortField SortBy { get; set; } = SortField.Date;

    /// <summary>
    /// Gets or sets the sort direction for content items.
    /// </summary>
    [Parameter]
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;

    /// <summary>
    /// Gets or sets the callback when a content item is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<ContentItem> OnItemSelected { get; set; }

    #region SEO Parameters

    /// <summary>
    /// Gets or sets the page title for SEO (used in CollectionPage schema).
    /// </summary>
    [Parameter]
    public string? PageTitle { get; set; }

    /// <summary>
    /// Gets or sets the page description for SEO.
    /// </summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <summary>
    /// Gets or sets the image the list page shares in its social cards (<c>og:image</c> and <c>twitter:image</c>). A
    /// relative address becomes absolute with the site address. Without it the list shares the image of the page it is
    /// shown on, such as a section's <c>_index.md</c> (its <c>og_image_url</c>, <c>image</c> or featured image), then the
    /// first listed item's featured image, then the site-wide default image of <c>AddOsirionSeoMetadata</c> or the
    /// <c>Osirion:Seo</c> section.
    /// </summary>
    [Parameter]
    public string? PageImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the website name override for SEO.
    /// </summary>
    [Parameter]
    public string? WebsiteName { get; set; }

    /// <summary>
    /// Gets or sets whether the list opens with its title as the page's <c>h1</c>, followed by its description. The
    /// title and description are <see cref="PageTitle"/> and <see cref="PageDescription"/>, or the ones built from the
    /// filter, the same text the SEO metadata uses. Defaults to <see langword="false"/>; turn it on when the list is
    /// the page and nothing else gives the page its heading.
    /// </summary>
    [Parameter]
    public bool ShowTitle { get; set; }

    /// <summary>
    /// Gets or sets the markup of one item. When set, the items render as an ordered list with this template in each
    /// entry instead of the built-in cards; paging, the title, feeds and SEO metadata work as before.
    /// </summary>
    [Parameter]
    public RenderFragment<ContentItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether to enable SEO metadata rendering (default: true).
    /// </summary>
    [Parameter]
    public bool EnableSeoMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets custom schema types to generate. If not set, defaults to CollectionPage.
    /// </summary>
    [Parameter]
    public SchemaType[]? SchemaTypes { get; set; }

    #endregion

    /// <summary>
    /// Gets or sets a value indicating whether the component is currently loading content.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of content items to display.
    /// </summary>
    protected IReadOnlyList<ContentItem>? ContentItems { get; set; }

    /// <summary>
    /// Gets or sets the total number of content items (before pagination).
    /// </summary>
    protected int TotalItems { get; set; }

    /// <summary>
    /// Gets the total number of pages based on item count and page size.
    /// </summary>
    protected int TotalPages => (int)Math.Ceiling(TotalItems / (double)EffectiveItemsPerPage);

    private int EffectiveItemsPerPage => Math.Max(1, ItemsPerPage);

    private static readonly string[] PageQueryParameters = ["page"];

    private string CurrentUrl { get; set; } = "/";

    private string CurrentPathAndQuery { get; set; } = "/";

#if !NET10_0_OR_GREATER
    /// <summary>The request context; only available during static server-side rendering.</summary>
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
#endif

    private readonly NotFoundSignal _notFound = new();

    // The item at the address the list is shown on, such as a section's _index.md, left out of the list.
    private ContentItem? _currentPageItem;

    ///// <summary>
    ///// Initializes the component.
    ///// </summary>
    //protected override async Task OnInitializedAsync()
    //{
    // Implement IDisposable pattern here
    //}

    /// <summary>
    /// Processes changes to component parameters.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        var uri = new Uri(NavigationManager.Uri);
        CurrentUrl = uri.AbsolutePath;
        CurrentPathAndQuery = uri.PathAndQuery;

        await LoadContentAsync();
    }

    /// <summary>
    /// Loads content items based on current parameters.
    /// </summary>
    protected async Task LoadContentAsync()
    {
        IsLoading = true;
        _currentPageItem = null;
        var loaded = false;
#if NET10_0_OR_GREATER
        var cancellationToken = CancellationToken.None;
#else
        // Under static SSR a closed connection stops the query; interactively there is no request to follow.
        var cancellationToken = HttpContext?.RequestAborted ?? CancellationToken.None;
#endif
        try
        {
            var provider = ContentProviderManager.GetDefaultProvider();
            if (provider is not null)
            {
                var query = new ContentQuery
                {
                    Directory = Directory,
                    Category = Category,
                    Tag = Tag,
                    Author = Author,
                    SearchQuery = SearchQuery,
                    Locale = Locale,
                    DirectoryId = DirectoryId,
                    SortBy = SortBy,
                    SortDirection = SortDirection
                };

                if (OnlyFeatured == true)
                {
                    query.IsFeatured = true;
                }

                if (FeaturedCount.HasValue)
                {
                    query.IsFeatured = true;
                    query.Take = FeaturedCount.Value;
                }
                else if (ShowPagination)
                {
                    // One fetch: the full result gives the total and the requested page is sliced from it.
                    var allItems = WithoutCurrentPage(await provider.GetItemsByQueryAsync(query, cancellationToken) ?? []);
                    TotalItems = allItems.Count;

                    // page=0, negative or past the last page (including int overflow) falls back to the nearest valid page.
                    CurrentPage = Math.Clamp(CurrentPage, 1, Math.Max(1, TotalPages));
                    ContentItems = allItems
                        .Skip((CurrentPage - 1) * EffectiveItemsPerPage)
                        .Take(EffectiveItemsPerPage)
                        .ToList();
                    loaded = true;
                    return;
                }

                var items = await provider.GetItemsByQueryAsync(query, cancellationToken);
                ContentItems = items is null ? null : WithoutCurrentPage(items);
                loaded = true;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The request was aborted; there is no visitor left to render the list for.
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load the content list (directory {Directory}, category {Category}, tag {Tag})", Directory, Category, Tag);
            ContentItems = Array.Empty<ContentItem>();
            TotalItems = 0;
        }
        finally
        {
            IsLoading = false;
            if (loaded)
            {
                SignalNotFoundWhenTheFilterMatchesNothing();
            }

            StateHasChanged();
        }
    }

    // Only a successful load reaches this: a provider that is missing or failed says nothing about the filter.
    private void SignalNotFoundWhenTheFilterMatchesNothing()
    {
        var hasFilter = !string.IsNullOrWhiteSpace(Category) || !string.IsNullOrWhiteSpace(Tag) || !string.IsNullOrWhiteSpace(Author);
        if (!NotFoundWhenFilterMatchesNothing || !hasFilter)
        {
            return;
        }

        if (ContentItems is { Count: > 0 })
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

    /// <summary>
    /// Gets the CSS class for the component.
    /// </summary>
    protected string GetContentListClass()
    {
        return $"osirion-content-list {Class}".Trim();
    }

    /// <summary>
    /// Gets the URL for a content item.
    /// </summary>
    protected string GetContentUrl(ContentItem item) => ContentUrls.ItemUrl(item, ContentUrlFormatter);

    /// <summary>
    /// Removes the item the list is shown on. A section's index page (<c>_index.md</c>) has the section's own URL, so
    /// the section's list would otherwise link to the page the reader is already on. The removed item is kept as the
    /// page's own item, whose image the list page shares.
    /// </summary>
    private List<ContentItem> WithoutCurrentPage(IEnumerable<ContentItem> items)
    {
        var currentPath = PathOf(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        var localePrefix = string.IsNullOrWhiteSpace(Locale) ? null : Locale.Trim('/') + "/";
        var currentPathWithoutLocale = localePrefix is not null && currentPath.StartsWith(localePrefix, StringComparison.OrdinalIgnoreCase)
            ? currentPath[localePrefix.Length..]
            : currentPath;

        var listed = new List<ContentItem>();
        foreach (var item in items)
        {
            var itemPath = PathOf(GetContentUrl(item));
            if (string.Equals(itemPath, currentPath, StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemPath, currentPathWithoutLocale, StringComparison.OrdinalIgnoreCase))
            {
                _currentPageItem ??= item;
                continue;
            }

            listed.Add(item);
        }

        return listed;
    }

    /// <summary>
    /// Gets the image the list page shares: <see cref="PageImageUrl"/>, then the image of the page the list is shown on
    /// (its <c>og_image_url</c>, <c>image</c> or featured image). Without either the SEO metadata falls back to the first
    /// item's featured image, then to the site-wide default image.
    /// </summary>
    protected string? GetComputedPageImageUrl()
    {
        var seo = _currentPageItem?.Metadata?.SeoProperties;
        return SeoText.FirstNonBlank(PageImageUrl, seo?.OgImageUrl, seo?.Image, _currentPageItem?.FeaturedImageUrl);
    }

    // The path of a link or location, without query, fragment or surrounding slashes.
    private static string PathOf(string url)
    {
        var withoutQuery = ContentUrls.StripQueryAndFragment(url);
        var isWebUrl = Uri.TryCreate(withoutQuery, UriKind.Absolute, out var absolute) && absolute.Scheme is "http" or "https";
        return (isWebUrl ? absolute!.AbsolutePath : withoutQuery).Trim('/');
    }

    /// <summary>
    /// Gets the URL for a pagination link.
    /// </summary>
    protected string GetPaginationUrl(int page)
    {

        // Keep every other query parameter (search query, filters); page 1 is the bare URL.
        return PaginationUrlFormatter?.Invoke(page)
            ?? ContentUrls.WithQueryParameter(CurrentPathAndQuery, "page", page > 1 ? page.ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
    }

    /// <summary>
    /// Gets the computed page title for SEO based on filters and pagination.
    /// </summary>
    protected string GetComputedPageTitle()
    {
        if (!string.IsNullOrWhiteSpace(PageTitle))
            return PageTitle!;

        var title = "";

        if (!string.IsNullOrWhiteSpace(Category))
            title = $"Category: {Category}";
        else if (!string.IsNullOrWhiteSpace(Tag))
            title = $"Tag: {Tag}";
        else if (!string.IsNullOrWhiteSpace(Directory))
            title = FormatDirectoryName(Directory);
        else
            title = "Blog";

        // Add page number if paginated and not first page
        if (ShowPagination && CurrentPage > 1)
            title += $" - Page {CurrentPage}";

        return title;
    }

    /// <summary>
    /// Formats a directory name for display (converts slug-case to Title Case).
    /// </summary>
    private string FormatDirectoryName(string directory)
    {
        // Remove leading/trailing slashes
        directory = directory.Trim('/');

        // Get last segment
        var lastSegment = directory.Split('/').LastOrDefault() ?? directory;

        // Convert slug-case to Title Case
        var words = lastSegment.Split('-', '_');
        var titleCased = string.Join(" ", words.Select(word =>
        {
            if (string.IsNullOrEmpty(word)) return word;
            return char.ToUpper(word[0]) + (word.Length > 1 ? word[1..].ToLower() : "");
        }));

        return titleCased;
    }

    /// <summary>
    /// Gets the computed page description for SEO.
    /// </summary>
    protected string? GetComputedPageDescription()
    {
        if (!string.IsNullOrWhiteSpace(PageDescription))
            return PageDescription;

        if (!string.IsNullOrWhiteSpace(Category))
            return $"Browse all articles in the {Category} category.";

        if (!string.IsNullOrWhiteSpace(Tag))
            return $"Browse all articles tagged with {Tag}.";

        if (!string.IsNullOrWhiteSpace(Directory))
            return $"Browse all articles in {FormatDirectoryName(Directory)}.";

        return "Browse our collection of articles and blog posts.";
    }

    /// <summary>
    /// Handles page change events.
    /// </summary>
    protected async Task OnPageChanged(int page)
    {
        if (page == CurrentPage)
        {
            return;
        }

        if (PageChanged.HasDelegate)
        {
            CurrentPage = page;
            await PageChanged.InvokeAsync(page);
            return;
        }

        // The page lives in the query string, so navigating is what changes it. Assigning the property alone
        // would not move the address bar, and the next parameter update would reset it.
        NavigationManager.NavigateTo(GetPaginationUrl(page));
    }

    /// <summary>
    /// Releases resources used by the component.
    /// </summary>
    public void Dispose()
    {
    }
}
