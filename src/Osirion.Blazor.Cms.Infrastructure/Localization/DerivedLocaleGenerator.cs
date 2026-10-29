using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using Osirion.Blazor.Core.Transliteration;

namespace Osirion.Blazor.Cms.Infrastructure.Localization;

/// <summary>
/// Adds the items of a provider's <see cref="ContentProviderOptions.DerivedLocales"/> to the items it has just loaded.
/// The FileSystem and GitHub content repositories call it once per load, after reading every file, so both providers
/// derive content the same way.
/// </summary>
/// <remarks>
/// <para>
/// Every item of a derived locale's <see cref="DerivedLocaleOptions.From"/> locale gets a derived item unless an item
/// with the same localization id already exists in the derived locale: a real file always wins. The derived item
/// keeps the localization id, takes the derived locale, and has the path, id and URL a file of that locale would
/// have. It is marked with <see cref="ContentItem.DerivedFromLocale"/> and has no file and no markdown source.
/// </para>
/// <para>
/// The title, description, categories, tags, series and the front matter's SEO and social titles and descriptions
/// are transliterated, and so is the rendered content (see the rules on the content's HTML in the localization docs).
/// The text of the item's content blocks is transliterated by <see cref="ContentBlockText.Map"/>; their links stay
/// as written, because the blocks localize site-relative links for the page's culture when they render.
/// The slug, the URL, the author, the dates, images and every other value stay as they are.
/// </para>
/// </remarks>
public sealed class DerivedLocaleGenerator
{
    private readonly TextTransliterators _transliterators;
    private readonly ILogger<DerivedLocaleGenerator> _logger;

    /// <summary>Initializes the generator.</summary>
    /// <param name="transliterators">Resolves the transliterator each derived locale names.</param>
    /// <param name="logger">The logger.</param>
    public DerivedLocaleGenerator(TextTransliterators transliterators, ILogger<DerivedLocaleGenerator> logger)
    {
        _transliterators = transliterators ?? throw new ArgumentNullException(nameof(transliterators));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Throws when the derived locales of <paramref name="options"/> break a rule, so a provider whose options were
    /// bound without the options validators (a named GitHub provider) fails as early as the others.
    /// </summary>
    /// <typeparam name="TOptions">The provider's options type.</typeparam>
    /// <param name="options">The provider options.</param>
    /// <exception cref="OptionsValidationException">Thrown with every failure when a rule is broken.</exception>
    internal void ThrowIfInvalid<TOptions>(TOptions options)
        where TOptions : ContentProviderOptions
    {
        var failures = DerivedLocaleValidation.Validate(options, _transliterators);
        if (failures.Count > 0)
        {
            throw new OptionsValidationException(Options.DefaultName, typeof(TOptions), failures);
        }
    }

    /// <summary>Adds the derived items to <paramref name="items"/>, the cache a load has just built.</summary>
    /// <param name="items">The loaded items, by id.</param>
    /// <param name="options">The provider options.</param>
    /// <param name="contentPath">The folder the provider's paths start with, before any locale folder.</param>
    internal void AddDerivedItems(IDictionary<string, ContentItem> items, ContentProviderOptions options, string contentPath)
    {
        if (!options.EnableLocalization || options.DerivedLocales is not { Count: > 0 })
        {
            return;
        }

        var supportedLocales = options.GetEffectiveSupportedLocales();
        foreach (var derivedLocale in options.DerivedLocales)
        {
            if (!_transliterators.TryGet(derivedLocale.Transliteration, out var transliterator))
            {
                // The options were validated; a registry changed since is the only way here
                _logger.LogWarning("No transliterator is registered as {Transliteration}; the locale {Locale} is not derived",
                    derivedLocale.Transliteration, derivedLocale.Locale);
                continue;
            }

            var added = AddDerivedLocale(items, derivedLocale, transliterator, supportedLocales, contentPath);
            _logger.LogDebug("Derived {Count} items in {Locale} from {From}", added, derivedLocale.Locale, derivedLocale.From);
        }
    }

    private static int AddDerivedLocale(
        IDictionary<string, ContentItem> items,
        DerivedLocaleOptions derivedLocale,
        ITextTransliterator transliterator,
        List<string> supportedLocales,
        string contentPath)
    {
        var translated = items.Values
            .Where(item => IsLocale(item, derivedLocale.Locale) && !string.IsNullOrWhiteSpace(item.ContentId))
            .Select(item => item.ContentId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sources = items.Values
            .Where(item => !item.IsDerived && IsLocale(item, derivedLocale.From))
            .ToList();

        var added = 0;
        foreach (var source in sources)
        {
            if (!string.IsNullOrWhiteSpace(source.ContentId) && translated.Contains(source.ContentId))
            {
                continue;
            }

            var path = RebasePath(source.Path, derivedLocale.Locale, supportedLocales, contentPath);
            var id = IdGenerator.GenerateStableId(path, source.ProviderId);
            if (items.ContainsKey(id))
            {
                // A file at the derived path itself, or a second source that maps to the same path
                continue;
            }

            var derived = source.DeriveForLocale(id, derivedLocale.Locale, path);
            derived.SetUrl(UrlGenerator.GenerateUrl(
                source.Metadata?.Permalink ?? string.Empty, path, source.Slug, enableLocalization: true, supportedLocales, contentPath));
            TransliterateText(derived, source, transliterator);
            derived.SetContent(TransliteratedHtml.Transform(source.Content, transliterator, derivedLocale.From, derivedLocale.Locale));
            derived.SetBlocks(ContentBlockText.Map(new ContentBlockSet(source.Blocks, source.BodyPlacement), transliterator.Transliterate));

            // The setters above move the date of the last change; the content changed when its source did
            derived.LastModified = source.LastModified;

            items[id] = derived;
            added++;
        }

        return added;
    }

    private static bool IsLocale(ContentItem item, string locale) =>
        string.Equals(item.Locale, locale, StringComparison.OrdinalIgnoreCase);

    // The path a file of the derived locale would have: the content folder, the derived locale's folder in place of
    // the source's (or added, for an item of the default locale kept outside a locale folder), then the rest.
    private static string RebasePath(string path, string locale, List<string> supportedLocales, string contentPath)
    {
        var normalizedPath = UrlGenerator.NormalizePath(path);
        var root = UrlGenerator.NormalizePath(contentPath);
        var prefix = root.Length > 0 && normalizedPath.StartsWith(root + "/", StringComparison.Ordinal) ? root + "/" : string.Empty;
        var relative = UrlGenerator.RemoveLocaleFromPath(normalizedPath[prefix.Length..], string.Empty, enableLocalization: true, supportedLocales);

        return $"{prefix}{locale}/{relative}";
    }

    private static void TransliterateText(ContentItem derived, ContentItem source, ITextTransliterator transliterator)
    {
        derived.SetTitle(transliterator.Transliterate(source.Title));
        derived.SetDescription(transliterator.Transliterate(source.Description));

        derived.ClearCategories();
        foreach (var category in source.Categories)
        {
            derived.AddCategory(transliterator.Transliterate(category));
        }

        derived.ClearTags();
        foreach (var tag in source.Tags)
        {
            derived.AddTag(transliterator.Transliterate(tag));
        }

        if (derived.Metadata is { } metadata)
        {
            TransliterateFrontMatter(metadata, transliterator);
        }
    }

    private static void TransliterateFrontMatter(FrontMatter metadata, ITextTransliterator transliterator)
    {
        metadata.Title = transliterator.Transliterate(metadata.Title ?? string.Empty);
        metadata.Description = transliterator.Transliterate(metadata.Description ?? string.Empty);
        metadata.Categories = metadata.Categories.Select(transliterator.Transliterate).ToList();
        metadata.Tags = metadata.Tags.Select(transliterator.Transliterate).ToList();
        if (metadata.Series is not null)
        {
            metadata.Series = transliterator.Transliterate(metadata.Series);
        }

        if (metadata.SeoProperties is { } seo)
        {
            seo.Title = transliterator.Transliterate(seo.Title ?? string.Empty);
            seo.Description = transliterator.Transliterate(seo.Description ?? string.Empty);
            seo.OgTitle = transliterator.Transliterate(seo.OgTitle ?? string.Empty);
            seo.OgDescription = transliterator.Transliterate(seo.OgDescription ?? string.Empty);
            seo.TwitterTitle = transliterator.Transliterate(seo.TwitterTitle ?? string.Empty);
            seo.TwitterDescription = transliterator.Transliterate(seo.TwitterDescription ?? string.Empty);
        }
    }
}
