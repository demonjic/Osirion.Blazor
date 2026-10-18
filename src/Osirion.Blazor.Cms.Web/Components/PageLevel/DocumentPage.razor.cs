using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;

namespace Osirion.Blazor.Cms.Components;

/// <summary>
/// DocumentPage component for displaying documentation with side navigation
/// </summary>
public partial class DocumentPage
{
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// The source path of the document to load from the default provider while <see cref="Content"/> is not set. When
    /// nothing is found, the page shows the not-found state and signals 404. A change of the path loads the new
    /// document, also on a page that is interactive; a <see cref="Content"/> you pass is never replaced by loading.
    /// </summary>
    [Parameter]
    public string? ContentPath { get; set; }

    /// <summary>
    /// The document to show. Pass it when you have loaded it, as this documentation site does; the page then loads
    /// only what is missing: the side navigation entries and the previous and next documents. Without <see
    /// cref="Content"/> and <see cref="ContentPath"/> the page renders its frame without a document.
    /// </summary>
    [Parameter]
    public ContentItem? Content { get; set; }

    /// <summary>
    /// Whether to show the side navigation, a list of pages beside the document. On narrow screens it folds behind a
    /// button.
    /// </summary>
    [Parameter]
    public bool ShowSideNavigation { get; set; } = true;

    /// <summary>
    /// The heading of the side navigation. No heading is shown when it is empty.
    /// </summary>
    [Parameter]
    public string? NavigationTitle { get; set; }

    /// <summary>
    /// Gets or sets the text of the button that shows the side navigation on narrow screens, where it is folded away
    /// so the page starts with its content. Defaults to <c>Documentation menu</c>.
    /// </summary>
    [Parameter]
    public string SideNavigationToggleText { get; set; } = "Documentation menu";

    /// <summary>
    /// The entries of the side navigation, with optional children. When it is null, the page lists the documents of
    /// the shown document's folder, in their <c>order</c>, linked with <see cref="ContentUrlFormatter"/>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<DocumentNavigationItem>? NavigationItems { get; set; }

    /// <summary>
    /// The path or address that marks an entry of the side navigation as the current page
    /// (<c>aria-current="page"</c>), compared without regard to case with each entry's <c>Path</c> or <c>Url</c>. It
    /// defaults to the shown document's path.
    /// </summary>
    [Parameter]
    public string? CurrentPath { get; set; }

    /// <summary>
    /// Whether to show the author and the "Last updated" date under the title. The date is the item's last
    /// modification, or its publication date when there is none, written in the language of the document's locale.
    /// </summary>
    [Parameter]
    public bool ShowMetadata { get; set; } = true;

    /// <summary>
    /// The label in front of the date under the title, for sites written in another language than English. Defaults
    /// to <c>Last updated:</c>.
    /// </summary>
    [Parameter]
    public string LastUpdatedText { get; set; } = "Last updated:";

    /// <summary>
    /// Whether to show a table of contents of the document's headings, between <see cref="TocMinLevel"/> and <see
    /// cref="TocMaxLevel"/>, above the body.
    /// </summary>
    [Parameter]
    public bool ShowTableOfContents { get; set; } = true;

    /// <summary>
    /// Gets or sets the table of contents title
    /// </summary>
    [Parameter]
    public string TableOfContentsTitle { get; set; } = "On this page";

    /// <summary>
    /// The highest heading level in the table of contents: <c>2</c> starts at <c>h2</c>.
    /// </summary>
    [Parameter]
    public int TocMinLevel { get; set; } = 2;

    /// <summary>
    /// The lowest heading level in the table of contents: <c>4</c> goes down to <c>h4</c>.
    /// </summary>
    [Parameter]
    public int TocMaxLevel { get; set; } = 4;

    /// <summary>
    /// Whether the table of contents numbers its entries, passed on to its <c>ShowNumbers</c>.
    /// </summary>
    [Parameter]
    public bool TocShowNumbers { get; set; } = true;

    /// <summary>
    /// Whether to show previous and next links under the document. When <see
    /// cref="OsirionContentDetailPageBase.PreviousItem"/> or <see cref="OsirionContentDetailPageBase.NextItem"/> is
    /// null, the page looks the missing one up in the document's folder, in <c>order</c>. A value you pass is kept.
    /// </summary>
    [Parameter]
    public bool ShowNavigationLinks { get; set; } = true;

    /// <summary>
    /// HTML shown under the document in a highlighted section. It is written through <c>OsirionHtmlRenderer</c>,
    /// which sanitizes it with the registered HTML sanitizer.
    /// </summary>
    [Parameter]
    public string? CustomFooterContent { get; set; }

    /// <summary>
    /// Gets or sets how the address of a page's source for editing is built, for example
    /// <c>item =&gt; $"https://github.com/owner/repo/edit/main/content/{item.Path}"</c>. When it returns an address, the
    /// page links to it after the body; no link is shown when it is <see langword="null"/> or returns nothing.
    /// </summary>
    [Parameter]
    public Func<ContentItem, string?>? EditUrlFormatter { get; set; }

    /// <summary>Gets or sets the text of the edit link. Defaults to <c>Edit this page</c>.</summary>
    [Parameter]
    public string EditLinkText { get; set; } = "Edit this page";

    /// <summary>
    /// Builds the links of the previous and next documents and of the folder entries the page lists itself from the
    /// item. Without it a link is <c>/{item.Url}</c>.
    /// </summary>
    [Parameter]
    public Func<ContentItem, string>? ContentUrlFormatter { get; set; }

    /// <summary>
    /// Builds the link of a side navigation entry. Without it the link is the entry's <c>Url</c>.
    /// </summary>
    [Parameter]
    public Func<DocumentNavigationItem, string>? NavigationUrlFormatter { get; set; }

    /// <inheritdoc/>
    protected override bool LoadsItem => false;

    // What the page loaded or derived itself, so a change of ContentPath replaces it while values the parent supplied
    // stay as they are.
    private string? _loadedPath;
    private ContentItem? _loadedContent;
    private ContentItem? _loadedPrevious;
    private ContentItem? _loadedNext;
    private string? _derivedCurrentPath;
    private ContentItem? _navigationContent;
    private (bool ShowNavigationLinks, bool ShowSideNavigation, bool OwnSideNavigation, bool PreviousSupplied, bool NextSupplied) _navigationInputs;

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        var contentSuppliedByParent = Content is not null && !ReferenceEquals(Content, _loadedContent);
        if (!contentSuppliedByParent && !string.IsNullOrWhiteSpace(ContentPath)
            && (Content is null || !string.Equals(ContentPath, _loadedPath, StringComparison.Ordinal)))
        {
            await LoadContentAsync();
        }
        else
        {
            if (Content is not null)
            {
                await LoadNavigationItemsAsync();
            }

            IsLoading = false;
        }

        // The page marks the shown document as current unless the parent names another path.
        if (Content is not null
            && (string.IsNullOrWhiteSpace(CurrentPath) || string.Equals(CurrentPath, _derivedCurrentPath, StringComparison.Ordinal)))
        {
            CurrentPath = _derivedCurrentPath = Content.Path;
        }

        if (Content is null && !string.IsNullOrWhiteSpace(ContentPath) && !IsLoading)
        {
            SignalNotFound();
        }
        else
        {
            ResetNotFoundSignal();
        }
    }

    private new async Task LoadContentAsync()
    {
        IsLoading = true;
        if (!string.Equals(ContentPath, _loadedPath, StringComparison.Ordinal))
        {
            ResetNotFoundSignal();
        }

        // The previous document and what was found around it must not stay while, or after, the new one loads.
        Content = null;
        ForgetLoadedNavigation();
        if (string.Equals(CurrentPath, _derivedCurrentPath, StringComparison.Ordinal))
        {
            CurrentPath = null;
        }

        _derivedCurrentPath = null;
        _loadedPath = ContentPath;
        _loadedContent = null;
        try
        {
            var provider = ContentProviderManager.GetDefaultProvider();
            if (provider is not null && ContentPath is { } contentPath && !string.IsNullOrWhiteSpace(contentPath))
            {
                Content = _loadedContent = await provider.GetItemByPathAsync(contentPath);

                if (Content is not null)
                {
                    await LoadNavigationItemsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load document {ContentPath}", ContentPath);
            Content = null;
            _loadedContent = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static readonly SchemaType[] DefaultSchemaTypes = [SchemaType.TechArticle];

    // Ties the narrow-screen toggle to the panel it folds; unique per instance, so two pages on one screen stay apart.
    private readonly string _sideNavigationId = "osirion-sidenav-" + Guid.NewGuid().ToString("N")[..8];

    private string SideNavigationToggleId => _sideNavigationId + "-toggle";

    private string SideNavigationPanelId => _sideNavigationId + "-panel";

    // The pages of the document's folder, listed in the side navigation when the parent gives no NavigationItems.
    private IReadOnlyList<DocumentNavigationItem>? _folderNavigation;

    private IReadOnlyList<DocumentNavigationItem>? SideNavigationItems => NavigationItems ?? _folderNavigation;

    // Clears the neighbors and the folder list the page found, leaving the ones the parent supplied.
    private void ForgetLoadedNavigation()
    {
        if (ReferenceEquals(PreviousItem, _loadedPrevious))
        {
            PreviousItem = null;
        }

        if (ReferenceEquals(NextItem, _loadedNext))
        {
            NextItem = null;
        }

        _loadedPrevious = _loadedNext = null;
        _folderNavigation = null;
        _navigationContent = null;
    }

    private async Task LoadNavigationItemsAsync()
    {
        if (Content is not { } content)
        {
            return;
        }

        var inputs = (
            ShowNavigationLinks,
            ShowSideNavigation,
            NavigationItems is null,
            PreviousItem is not null && !ReferenceEquals(PreviousItem, _loadedPrevious),
            NextItem is not null && !ReferenceEquals(NextItem, _loadedNext));
        if (ReferenceEquals(content, _navigationContent) && inputs == _navigationInputs)
        {
            return;
        }

        ForgetLoadedNavigation();
        _navigationContent = content;
        _navigationInputs = inputs;

        var needsPager = ShowNavigationLinks && (PreviousItem is null || NextItem is null);
        var needsSideNavigation = ShowSideNavigation && NavigationItems is null;
        if (!(needsPager || needsSideNavigation))
            return;

        try
        {
            var provider = ContentProviderManager.GetDefaultProvider();
            if (provider is not null)
            {
                var query = new ContentQuery
                {
                    Directory = content.Directory?.Path,
                    SortBy = SortField.Order,
                    SortDirection = SortDirection.Ascending
                };

                var items = await provider.GetItemsByQueryAsync(query) ?? [];

                if (needsSideNavigation)
                {
                    _folderNavigation = items
                        .Select(item => new DocumentNavigationItem
                        {
                            Title = item.Title,
                            Url = GetContentUrl(item),
                            Path = item.Path,
                            Order = item.OrderIndex,
                        })
                        .ToList();
                }

                if (!needsPager)
                {
                    return;
                }

                int currentIndex = -1;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Path == content.Path)
                    {
                        currentIndex = i;
                        break;
                    }
                }

                if (PreviousItem is null && currentIndex > 0)
                {
                    PreviousItem = _loadedPrevious = items[currentIndex - 1];
                }

                if (NextItem is null && currentIndex >= 0 && currentIndex < items.Count - 1)
                {
                    NextItem = _loadedNext = items[currentIndex + 1];
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load previous and next documents for {ContentPath}", content.Path);
        }
    }

    // schema.org type of the document article: the first requested schema type, TechArticle for documentation otherwise.
    private string GetItemType() => SchemaTypes is { Length: > 0 }
        ? SchemaTypes[0] == SchemaType.FAQ ? "FAQPage" : SchemaTypes[0].ToString()
        : "TechArticle";

    private string GetDocumentPageClass()
    {
        var classes = new List<string> { "osirion-document-page" };

        if (ShowSideNavigation)
        {
            classes.Add("osirion-document-with-sidenav");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    private string GetNavigationUrl(DocumentNavigationItem item)
    {
        return NavigationUrlFormatter?.Invoke(item) ?? item.Url;
    }

    private string GetNavigationLinkClass(DocumentNavigationItem item)
    {
        var isActive = IsCurrentPage(item);
        var classes = new List<string> { "osirion-sidenav-link" };

        if (isActive)
        {
            classes.Add("osirion-sidenav-active");
        }

        return string.Join(" ", classes);
    }

    private bool IsCurrentPage(DocumentNavigationItem item)
    {
        if (string.IsNullOrWhiteSpace(CurrentPath))
            return false;

        return item.Path?.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase) == true ||
               item.Url?.TrimStart('/').Equals(CurrentPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase) == true;
    }

    private bool IsParentOfCurrentPage(DocumentNavigationItem item)
    {
        if (string.IsNullOrWhiteSpace(CurrentPath) || item.Children is null)
            return false;

        return item.Children.Any(child => IsCurrentPage(child));
    }

    private string GetContentUrl(ContentItem item) => ContentUrls.ItemUrl(item, ContentUrlFormatter);
}

/// <summary>
/// Represents a navigation item in the document navigation
/// </summary>
public class DocumentNavigationItem
{
    /// <summary>Gets or sets the Title value.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the Url value.</summary>
    public string Url { get; set; } = string.Empty;
    /// <summary>Gets or sets the Path value.</summary>
    public string? Path { get; set; }
    /// <summary>Gets or sets the Order value.</summary>
    public int Order { get; set; }
    /// <summary>Gets or sets the IsExpanded value.</summary>
    public bool IsExpanded { get; set; }
    /// <summary>Gets or sets the Children value.</summary>
    public IReadOnlyList<DocumentNavigationItem>? Children { get; set; }
}
