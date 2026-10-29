using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Domain.Exceptions;

/// <summary>
/// Thrown when the <c>blocks</c> of a content item cannot be read or break a rule of the content blocks contract.
/// </summary>
/// <remarks>
/// The repositories load such an item as they load a file whose front matter is not valid YAML: they log this
/// exception, whose message lists every problem with the file, the block and the key, and leave the item out.
/// </remarks>
public sealed class ContentBlockException : DomainException
{
    /// <summary>Initializes the exception with the problems found in one file.</summary>
    /// <param name="sourcePath">The content item's path.</param>
    /// <param name="errors">The problems, at least one.</param>
    public ContentBlockException(string sourcePath, IReadOnlyList<ContentBlockError> errors)
        : base(BuildMessage(sourcePath, errors))
    {
        SourcePath = sourcePath;
        Errors = errors;
    }

    /// <summary>Gets the content item's path.</summary>
    public string SourcePath { get; }

    /// <summary>Gets every problem found, in the order of the blocks.</summary>
    public IReadOnlyList<ContentBlockError> Errors { get; }

    private static string BuildMessage(string sourcePath, IReadOnlyList<ContentBlockError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        var lines = errors.Select(error => $"  {error.Location}: {error.Message}");
        return $"The blocks of '{sourcePath}' are not valid ({errors.Count} problem(s)):{Environment.NewLine}"
            + string.Join(Environment.NewLine, lines);
    }
}
