using Microsoft.AspNetCore.Components;
#if !NET10_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>Displays a content item and optional previous and next navigation links.</summary>
public partial class ContentView(IContentProviderManager contentProviderManager)
{
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    [Inject]
    private ILogger<ContentView> Logger { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// The source path of the item to load from the default provider (<c>ContentItem.Path</c>, for example
    /// <c>blog/getting-started.md</c>) while <see cref="Item"/> is not set. A null or white-space path loads nothing.
    /// </summary>
    [Parameter]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show an item that is not published (<c>published: false</c> in its front matter) when the
    /// component loads it by <see cref="Path"/>. Off by default: a draft then renders the not-found state and
    /// signals 404, as a missing item does. Turn it on only for previews you protect yourself; lists, the sitemap
    /// and feeds leave drafts out either way. An item you pass in is shown whatever its status.
    /// </summary>
    [Parameter]
    public bool ShowUnpublished { get; set; }

    /// <summary>
    /// The text shown while the item loads, in interactive render modes only. A statically rendered response is sent
    /// once the item has loaded.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading content...";

    /// <summary>
    /// The message of the not-found block, shown under a fixed <c>Content Not Found</c> heading and above a link back
    /// to <c>/</c>. The block appears when no item is found, when loading fails (the error is logged) and when
    /// neither <see cref="Item"/> nor <see cref="Path"/> is set.
    /// </summary>
    [Parameter]
    public string NotFoundText { get; set; } = "Content not found.";

    /// <summary>
    /// Builds a category link from the category name. Without it the link is <c>/category/{slug}</c>.
    /// </summary>
    [Parameter]
    public Func<string, string>? CategoryUrlFormatter { get; set; }

    /// <summary>
    /// Builds a tag link from the tag name. Without it the link is <c>/tag/{slug}</c>.
    /// </summary>
    [Parameter]
    public Func<string, string>? TagUrlFormatter { get; set; }

    /// <summary>
    /// Gets or sets how the author's page URL is built from the author's name. When set, the author's name links there
    /// with <c>rel="author"</c>.
    /// </summary>
    [Parameter]
    public Func<string, string>? AuthorUrlFormatter { get; set; }

    /// <summary>
    /// Builds the links of the previous and next items from the item. Without it a link is <c>/{item.Path}</c>, the
    /// source path, which is usually not the public address, so set it whenever navigation links are shown.
    /// </summary>
    [Parameter]
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>
    /// The item to render: header, sanitized body, categories, tags and navigation. An item the parent sets is never
    /// replaced by loading by <see cref="Path"/>.
    /// </summary>
    [Parameter]
    public ContentItem? Item { get; set; }

    /// <summary>
    /// The item the previous link points to. Loading by <see cref="Path"/> with <see cref="ShowNavigationLinks"/> on
    /// fills it, and clears what it filled when the path changes; a value you set is left alone until then.
    /// </summary>
    [Parameter]
    public ContentItem? PreviousItem { get; set; }

    /// <summary>
    /// The item the next link points to. See <see cref="PreviousItem"/>.
    /// </summary>
    [Parameter]
    public ContentItem? NextItem { get; set; }

    /// <summary>
    /// Whether the footer shows previous and next links. When the component loads by <see cref="Path"/> it also looks
    /// the neighbors up with a directory query built from the folder part of the path, newest first. For an item you
    /// pass in <see cref="Item"/>, set <see cref="PreviousItem"/> and <see cref="NextItem"/> yourself.
    /// </summary>
    [Parameter]
    public bool ShowNavigationLinks { get; set; } = false;

    /// <summary>
    /// The level of the heading that holds the title, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It defaults to
    /// <c>1</c>, the page's own heading; set <c>2</c> for a view embedded in a page that has its own <c>h1</c>, so the
    /// page keeps a single one. A value outside the range is moved to the nearest level. The title keeps its look at
    /// every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 1;

    private bool IsLoading { get; set; } = true;

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    // What this component loaded itself, so a later Path change can tell it apart from values the parent supplied.
    private string? _loadedPath;
    private ContentItem? _loadedItem;
    private ContentItem? _loadedPrevious;
    private ContentItem? _loadedNext;

    /// <summary>Performs the OnParametersSet operation asynchronously.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var itemSuppliedByParent = Item is not null && !ReferenceEquals(Item, _loadedItem);
        if (!itemSuppliedByParent && !string.IsNullOrWhiteSpace(Path)
            && (Item is null || !string.Equals(Path, _loadedPath, StringComparison.Ordinal)))
        {
            await LoadContentAsync();
        }
        else
        {
            IsLoading = false;
        }

        SignalWhenNotFound(Item is null);
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

    private async Task LoadContentAsync()
    {
        IsLoading = true;

        // The previous document's content and neighbours must not stay visible while, or after, the new one loads.
        Item = null;
        if (ReferenceEquals(PreviousItem, _loadedPrevious))
        {
            PreviousItem = null;
        }

        if (ReferenceEquals(NextItem, _loadedNext))
        {
            NextItem = null;
        }

        _loadedPath = Path;
        _loadedItem = _loadedPrevious = _loadedNext = null;
        try
        {
            var provider = contentProviderManager.GetDefaultProvider();
            if (provider is not null)
            {
                Item = PublishedContent.ShownOrNull(
                    await provider.GetItemByPathAsync(Path, RequestAborted), ShowUnpublished);
                _loadedItem = Item;

                if (Item is not null && ShowNavigationLinks)
                {
                    // If we need previous and next items, load them
                    var allItems = await provider.GetItemsByQueryAsync(new ContentQuery
                    {
                        Directory = System.IO.Path.GetDirectoryName(Path)?.Replace('\\', '/'),
                        SortBy = SortField.Date,
                        SortDirection = SortDirection.Descending
                    }, RequestAborted) ?? [];

                    var item = Item;
                    if (item is null)
                    {
                        return;
                    }

                    // Find the index manually
                    int currentIndex = -1;
                    for (int i = 0; i < allItems.Count; i++)
                    {
                        if (allItems[i].Path == item.Path)
                        {
                            currentIndex = i;
                            break;
                        }
                    }

                    if (currentIndex > 0)
                    {
                        PreviousItem = _loadedPrevious = allItems[currentIndex - 1];
                    }

                    if (currentIndex >= 0 && currentIndex < allItems.Count - 1)
                    {
                        NextItem = _loadedNext = allItems[currentIndex + 1];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load content {ContentPath}", Path);
            Item = null;
            _loadedItem = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetContentViewClass()
    {
        return $"osirion-content-view {Class}".Trim();
    }

    private readonly SanitizedContentCache _sanitizedContent = new();

    private MarkupString GetSanitizedContent(string? content)
    {
        return _sanitizedContent.Get(ServiceProvider, content);
    }

    private string GetCategoryUrl(string category)
    {
        return CategoryUrlFormatter?.Invoke(category) ?? ContentUrls.CategoryUrl(category);
    }

    private string GetTagUrl(string tag)
    {
        return TagUrlFormatter?.Invoke(tag) ?? ContentUrls.TagUrl(tag);
    }

    private string GetContentUrl(ContentItem item)
    {
        return ContentUrlFormatter?.Invoke(item) ?? $"/{item.Path}";
    }
}
