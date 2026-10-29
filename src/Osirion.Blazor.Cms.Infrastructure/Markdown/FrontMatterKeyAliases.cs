using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Infrastructure.Markdown;

/// <summary>
/// Reads the other names front matter uses for three keys into the canonical one, for items and directories alike:
/// <c>locale</c> and <c>language</c> for <c>lang</c>, <c>localization_id</c> and <c>content_id</c> for <c>id</c>, and
/// <c>url</c> for <c>permalink</c>.
/// </summary>
/// <remarks>
/// The canonical key wins when both are set; another name is read only when it is absent, in the order above. A value
/// that disagrees with the one used is logged as a warning naming the file and both keys. The other names are cleared
/// afterwards, so a saved file writes the canonical keys only. An empty <c>url</c> (<c>url: ""</c>) is the site root,
/// as <c>permalink: "/"</c> is.
/// </remarks>
internal static partial class FrontMatterKeyAliases
{
    private const string RootPermalink = "/";

    /// <summary>Copies the other names into the canonical keys of <paramref name="frontMatter"/> and clears them.</summary>
    /// <param name="frontMatter">The front matter of one file.</param>
    /// <param name="sourcePath">The file's path, for the warning.</param>
    /// <param name="logger">Where a conflict is logged.</param>
    public static void Apply(FrontMatter frontMatter, string sourcePath, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(frontMatter);

        frontMatter.Lang = Resolve(
            new Candidate("lang", Present(frontMatter.Lang)),
            [new Candidate("locale", Present(frontMatter.Locale)), new Candidate("language", Present(frontMatter.Language))],
            SameLanguage, sourcePath, logger) ?? string.Empty;

        frontMatter.Id = Resolve(
            new Candidate("id", Present(frontMatter.Id)),
            [new Candidate("localization_id", Present(frontMatter.LocalizationId)), new Candidate("content_id", Present(frontMatter.ContentId))],
            SameText, sourcePath, logger) ?? string.Empty;

        frontMatter.Permalink = Resolve(
            new Candidate("permalink", Present(frontMatter.Permalink)),
            [new Candidate("url", UrlAsPermalink(frontMatter.Url))],
            SameAddress, sourcePath, logger) ?? frontMatter.Permalink;

        frontMatter.Locale = null;
        frontMatter.Language = null;
        frontMatter.LocalizationId = null;
        frontMatter.ContentId = null;
        frontMatter.Url = null;
    }

    // The canonical value when set, otherwise the first other name that is set; a different value elsewhere is logged.
    private static string? Resolve(
        Candidate canonical, Candidate[] others, Func<string, string, bool> same, string sourcePath, ILogger logger)
    {
        var used = canonical.Value is not null ? canonical : others.FirstOrDefault(other => other.Value is not null);
        if (used.Value is null)
        {
            return null;
        }

        foreach (var other in others.Prepend(canonical))
        {
            if (other.Value is not null && other.Key != used.Key && !same(other.Value, used.Value))
            {
                LogConflict(logger, sourcePath, used.Key, used.Value, other.Key, other.Value);
            }
        }

        return used.Value;
    }

    private static string? Present(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // url: "" is the root, which permalink writes as "/"; a url left out (null) is not set.
    private static string? UrlAsPermalink(string? url) =>
        url is null ? null : string.IsNullOrWhiteSpace(url) ? RootPermalink : url.Trim();

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "The front matter of {SourcePath} sets {UsedKey} to '{UsedValue}', which is used, and {OtherKey} to '{OtherValue}', which is ignored")]
    private static partial void LogConflict(ILogger logger, string sourcePath, string usedKey, string usedValue, string otherKey, string otherValue);

    private readonly record struct Candidate(string Key, string? Value);

    private static bool SameText(string x, string y) => string.Equals(x, y, StringComparison.Ordinal);

    private static bool SameLanguage(string x, string y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

    // permalink and url compare without their leading and trailing slashes, which are optional.
    private static bool SameAddress(string x, string y) => string.Equals(x.Trim('/'), y.Trim('/'), StringComparison.Ordinal);
}
