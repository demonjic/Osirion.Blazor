using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Interfaces.Directory;
using Osirion.Blazor.Cms.Infrastructure.Markdown;
using System.Globalization;
using System.Text;

namespace Osirion.Blazor.Cms.Infrastructure.Directory;

/// <summary>
/// Implementation of IDirectoryMetadataProcessor for processing directory metadata
/// </summary>
public class DirectoryMetadataProcessor : IDirectoryMetadataProcessor
{
    private readonly IFrontMatterExtractor _frontMatterExtractor;
    private readonly IMarkdownProcessor _markdownProcessor;
    private readonly ILogger<DirectoryMetadataProcessor> _logger;

    /// <summary>Performs the DirectoryMetadataProcessor operation.</summary>
    public DirectoryMetadataProcessor(
        IFrontMatterExtractor frontMatterExtractor,
        IMarkdownProcessor markdownProcessor,
        ILogger<DirectoryMetadataProcessor> logger)
    {
        _frontMatterExtractor = frontMatterExtractor;
        _markdownProcessor = markdownProcessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public DirectoryItem ProcessMetadata(DirectoryItem directory, string metadataContent)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        if (string.IsNullOrWhiteSpace(metadataContent))
            return directory;

        try
        {
            var frontmatter = _markdownProcessor.ExtractFrontMatterAndContent(metadataContent).FrontMatter;
            if (frontmatter is not null)
            {
                FrontMatterKeyAliases.Apply(frontmatter, directory.Path, _logger);
            }

            directory.SetMetadata(frontmatter);
            // Front matter without a title parses to an empty one, which SetName rejects and which would skip every key below
            if (!string.IsNullOrWhiteSpace(frontmatter?.Title))
            {
                directory.SetName(frontmatter.Title);
            }

            directory.SetDescription(frontmatter?.Description ?? directory.Description);
            directory.SetOrder(frontmatter?.Order ?? directory.Order);
            directory.SetLocale(string.IsNullOrWhiteSpace(frontmatter?.Lang) ? directory.Locale : frontmatter.Lang);
            directory.SetUrl(frontmatter?.Permalink ?? directory.Url);
            directory.SetFeaturedImage(frontmatter?.FeaturedImage ?? directory.FeaturedImageUrl);
            return directory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing directory metadata for {DirectoryId}", directory.Id);
            return directory;
        }
    }

    /// <inheritdoc/>
    public string GenerateMetadataContent(DirectoryItem directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory));

        var frontMatter = new StringBuilder();
        frontMatter.AppendLine("---");

        // Add metadata
        frontMatter.AppendLine($"title: \"{EscapeYamlString(directory.Name)}\"");

        if (!string.IsNullOrWhiteSpace(directory.Description))
            frontMatter.AppendLine($"description: \"{EscapeYamlString(directory.Description)}\"");

        if (directory.Order != 0)
            frontMatter.AppendLine($"order: {directory.Order}");

        if (!string.IsNullOrWhiteSpace(directory.Locale))
            frontMatter.AppendLine($"locale: \"{EscapeYamlString(directory.Locale)}\"");

        if (!string.IsNullOrWhiteSpace(directory.Url))
            frontMatter.AppendLine($"url: \"{EscapeYamlString(directory.Url)}\"");

        // Add custom metadata. Only plain keys are written, so a key cannot break the YAML or add other keys.
        foreach (var meta in directory.CustomMetadata)
        {
            if (!IsWritableCustomKey(meta.Key))
                continue;

            frontMatter.AppendLine($"{meta.Key}: {FormatYamlValue(meta.Value)}");
        }

        frontMatter.AppendLine("---");
        frontMatter.AppendLine();

        frontMatter.AppendLine($"# {directory.Name}");
        frontMatter.AppendLine();

        if (!string.IsNullOrWhiteSpace(directory.Description))
        {
            frontMatter.AppendLine(directory.Description);
        }

        return frontMatter.ToString();
    }

    /// <summary>
    /// Escapes special characters in YAML strings
    /// </summary>
    private static readonly HashSet<string> StandardKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "title", "description", "order", "locale", "url"
    };

    private static bool IsWritableCustomKey(string key)
    {
        return !string.IsNullOrEmpty(key)
            && !StandardKeys.Contains(key)
            && key.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-');
    }

    private string FormatYamlValue(object? value)
    {
        return value switch
        {
            bool boolValue => boolValue ? "true" : "false",
            int or long or double or decimal => Convert.ToString(value, CultureInfo.InvariantCulture)!,
            _ => $"\"{EscapeYamlString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)}\""
        };
    }

    private string EscapeYamlString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
