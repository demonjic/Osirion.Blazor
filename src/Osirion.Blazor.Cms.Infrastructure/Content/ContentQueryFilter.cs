using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Extensions;
using Osirion.Blazor.Cms.Domain.Interfaces.Content;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Domain.Repositories;
using System.Linq.Expressions;

namespace Osirion.Blazor.Cms.Infrastructure.Content;

/// <summary>
/// Implementation of IContentQueryFilter for filtering content queries
/// </summary>
public class ContentQueryFilter : IContentQueryFilter
{
    private readonly IPathUtilities _pathUtils;

    /// <summary>Performs the ContentQueryFilter operation.</summary>
    public ContentQueryFilter(IPathUtilities pathUtils)
    {
        _pathUtils = pathUtils ?? throw new ArgumentNullException(nameof(pathUtils));
    }

    /// <inheritdoc/>
    public IQueryable<ContentItem> ApplyFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        var filteredItems = items;

        // A category or tag query lists content across the whole site, so the location filters do not apply to it
        if (CategoryOrTagHasNoValue(query.Category, query.Tag))
        {
            filteredItems = ApplyLocationFilters(filteredItems, query);
        }

        filteredItems = ApplyCategoryFilters(filteredItems, query);
        filteredItems = ApplyTagFilters(filteredItems, query);
        filteredItems = ApplyAttributeFilters(filteredItems, query);
        filteredItems = ApplyStatusFilter(filteredItems, query);
        filteredItems = ApplyRawFileFilter(filteredItems, query);
        filteredItems = ApplyDateFilters(filteredItems, query);
        filteredItems = ApplySearchFilter(filteredItems, query);
        filteredItems = ApplyLocalizationFilters(filteredItems, query);
        filteredItems = ApplyProviderFilter(filteredItems, items, query);
        filteredItems = ApplyIdFilters(filteredItems, query);
        filteredItems = ApplySorting(filteredItems, query);

        return ApplyPaging(filteredItems, query);
    }

    private IQueryable<ContentItem> ApplyLocationFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Path))
        {
            items = items.Where(item => item.Path.Contains(_pathUtils.NormalizePath(GetDirectoryPath(item.Path))));
        }

        items = ApplyDirectoryFilter(items, query);

        if (!string.IsNullOrWhiteSpace(query.DirectoryId))
        {
            items = items.Where(item =>
                item.Directory != null && item.Directory.Id == query.DirectoryId);
        }

        if (!string.IsNullOrWhiteSpace(query.Slug))
        {
            items = items.Where(item =>
                item.Slug != null && item.Slug == query.Slug);
        }

        if (!string.IsNullOrWhiteSpace(query.Url))
        {
            items = items.Where(item =>
                item.Url != null && item.Url.Equals(query.Url.Trim('/'), StringComparison.OrdinalIgnoreCase));
        }

        return items;
    }

    private IQueryable<ContentItem> ApplyDirectoryFilter(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Directory))
        {
            return items;
        }

        if (query.IncludeSubdirectories)
        {
            var directory = _pathUtils.NormalizePath(query.Directory);
            return items.Where(item => IsInDirectoryTree(_pathUtils.NormalizePath(GetDirectoryPath(item.Path)), directory));
        }

        // Only include items directly in this directory, named by its folder ("guides"), its path ("docs/guides") or its
        // display name. The display name alone failed once a folder's _index.md title replaced it.
        var wanted = _pathUtils.NormalizePath(query.Directory).Trim('/');
        return items.Where(item => item.Directory != null && IsDirectory(item.Directory, query.Directory, wanted));
    }

    private bool IsDirectory(DirectoryItem directory, string name, string path)
    {
        var directoryPath = _pathUtils.NormalizePath(directory.Path).Trim('/');
        var folder = directoryPath[(directoryPath.LastIndexOf('/') + 1)..];
        return directory.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            || folder.Equals(path, StringComparison.OrdinalIgnoreCase)
            || directoryPath.Equals(path, StringComparison.OrdinalIgnoreCase);
    }

    // Whole folder names only: "blog" must not take in "blog2".
    private static bool IsInDirectoryTree(string itemDirectory, string directory) =>
        itemDirectory.Equals(directory, StringComparison.OrdinalIgnoreCase)
        || itemDirectory.StartsWith(directory + "/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether a category or tag is the one a query names: by its name, ignoring case, or by its slug, the way an
    /// address carries it. Turning a slug back into a name cannot work, because a name may hold a hyphen of its own
    /// ("Multi-tenant platforms" is <c>multi-tenant-platforms</c>), and a partial match let "Dev" list "Web Dev".
    /// </summary>
    private static bool IsNamedBy(string value, string nameOrSlug) =>
        value.Trim().Equals(nameOrSlug, StringComparison.OrdinalIgnoreCase)
        || value.GenerateSlug().Equals(nameOrSlug, StringComparison.OrdinalIgnoreCase);

    private static IQueryable<ContentItem> ApplyCategoryFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var category = query.Category.Trim();
            items = items.Where(item => item.Categories.Any(c => IsNamedBy(c, category)));
        }

        if (query.Categories is not null && query.Categories.Any())
        {
            var normalizedCategories = query.Categories.Select(c => c.Trim()).ToList();
            items = items.Where(item =>
                normalizedCategories.All(c =>
                    item.Categories.Any(itemCat =>
                        itemCat.Trim().Equals(c, StringComparison.OrdinalIgnoreCase))));
        }

        return items;
    }

    private static IQueryable<ContentItem> ApplyTagFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();
            items = items.Where(item => item.Tags.Any(t => IsNamedBy(t, tag)));
        }

        if (query.Tags is not null && query.Tags.Any())
        {
            var normalizedTags = query.Tags.Select(t => t.Trim()).ToList();
            items = items.Where(item =>
                normalizedTags.All(t =>
                    item.Tags.Any(itemTag =>
                        itemTag.Trim().Equals(t, StringComparison.OrdinalIgnoreCase))));
        }

        return items;
    }

    private static IQueryable<ContentItem> ApplyAttributeFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (query.IsFeatured.HasValue)
        {
            items = items.Where(item => item.IsFeatured == query.IsFeatured.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Author))
        {
            items = items.Where(item =>
                item.Author.Equals(query.Author, StringComparison.OrdinalIgnoreCase));
        }

        return items;
    }

    private static IQueryable<ContentItem> ApplyStatusFilter(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (query.Status.HasValue)
        {
            return items.Where(item => item.Status == query.Status.Value);
        }

        if (!query.IncludeUnpublished)
        {
            // If not including unpublished, only show published content by default
            return items.Where(item => item.Status == ContentStatus.Published);
        }

        return items;
    }

    // A raw file (robots.txt loaded as content) is not a page: only a query that asks for raw files gets one.
    private static IQueryable<ContentItem> ApplyRawFileFilter(IQueryable<ContentItem> items, ContentQuery query) =>
        query.IncludeRawFiles ? items : items.Where(item => !item.IsRawFile);

    private static IQueryable<ContentItem> ApplyDateFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (query.DateFrom.HasValue)
        {
            items = items.Where(item => item.DateCreated >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            items = items.Where(item => item.DateCreated <= query.DateTo.Value);
        }

        return items;
    }

    private static IQueryable<ContentItem> ApplySearchFilter(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            return items;
        }

        // Ordinal, not culture-sensitive lowering: under tr-TR "I".ToLower() is a dotless i and "GUIDE" missed "Guide".
        var searchTerms = query.SearchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return items.Where(item =>
            searchTerms.Any(term =>
                (item.Title != null && item.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (item.Description != null && item.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (item.Content != null && item.Content.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                item.Categories.Any(c => c.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                item.Tags.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase))
            )
        );
    }

    private static IQueryable<ContentItem> ApplyLocalizationFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Locale))
        {
            items = items.Where(item =>
                item.Locale.Equals(query.Locale, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.LocalizationId))
        {
            items = items.Where(item =>
                item.ContentId.Equals(query.LocalizationId, StringComparison.OrdinalIgnoreCase));
        }

        return items;
    }

    /// <summary>
    /// The items handed to the filter come from one provider, so the first unfiltered item decides whether any match.
    /// </summary>
    private static IQueryable<ContentItem> ApplyProviderFilter(
        IQueryable<ContentItem> filteredItems,
        IQueryable<ContentItem> allItems,
        ContentQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.ProviderId) && query.ProviderId != allItems.FirstOrDefault()?.ProviderId)
        {
            // No item can match another provider, so the result is empty
            return filteredItems.Where(i => false);
        }

        return filteredItems;
    }

    private static IQueryable<ContentItem> ApplyIdFilters(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (query.IncludeIds is not null && query.IncludeIds.Any())
        {
            items = items.Where(item =>
                query.IncludeIds.Contains(item.Id));
        }

        if (query.ExcludeIds is not null && query.ExcludeIds.Any())
        {
            items = items.Where(item =>
                !query.ExcludeIds.Contains(item.Id));
        }

        return items;
    }

    private static IQueryable<ContentItem> ApplySorting(IQueryable<ContentItem> items, ContentQuery query)
    {
        var ascending = query.SortDirection == SortDirection.Ascending;

        // Date, Created and any unknown field sort by creation date
        return query.SortBy switch
        {
            SortField.Title => OrderBy(items, item => item.Title, ascending),
            SortField.Author => OrderBy(items, item => item.Author, ascending),
            SortField.LastModified => OrderBy(items, item => item.LastModified ?? item.PublishDate, ascending),
            SortField.Order => OrderBy(items, item => item.OrderIndex, ascending),
            SortField.PublishDate => OrderBy(items, item => item.PublishDate, ascending),
            SortField.Slug => OrderBy(items, item => item.Slug, ascending),
            SortField.ReadTime => OrderBy(items, item => item.ReadTimeMinutes, ascending),
            _ => OrderBy(items, item => item.DateCreated, ascending)
        };
    }

    private static IQueryable<ContentItem> OrderBy<TKey>(
        IQueryable<ContentItem> items,
        Expression<Func<ContentItem, TKey>> keySelector,
        bool ascending)
    {
        return ascending ? items.OrderBy(keySelector) : items.OrderByDescending(keySelector);
    }

    private static IQueryable<ContentItem> ApplyPaging(IQueryable<ContentItem> items, ContentQuery query)
    {
        if (query.Skip.HasValue && query.Skip.Value > 0)
        {
            items = items.Skip(query.Skip.Value);
        }

        if (query.Take.HasValue && query.Take.Value > 0)
        {
            items = items.Take(query.Take.Value);
        }

        return items;
    }

    /// <summary>
    /// Gets the directory path from a file path
    /// </summary>
    private string GetDirectoryPath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        var lastSlashIndex = filePath.LastIndexOf('/');
        if (lastSlashIndex >= 0)
        {
            return filePath.Substring(0, lastSlashIndex);
        }

        return string.Empty;
    }

    private bool CategoryOrTagHasNoValue(string? category, string? tag)
    {
        return string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(tag);
    }
}
