namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>One problem found in the blocks of a content item.</summary>
/// <param name="SourcePath">The content item's path, for example <c>en/home.md</c>.</param>
/// <param name="BlockIndex">
/// The position of the block in <c>blocks</c>, starting at 0, or <see langword="null"/> when the problem is not in one
/// block (the <c>blocks</c> or <c>body</c> key itself).
/// </param>
/// <param name="Key">The key with the problem, as a path inside the block, for example <c>items[1].url</c>.</param>
/// <param name="Message">What is wrong and what is allowed.</param>
public sealed record ContentBlockError(string SourcePath, int? BlockIndex, string Key, string Message)
{
    /// <summary>
    /// Returns the problem in one line that names the file, the block and the key, for example
    /// <c>en/home.md: blocks[2].primary_action.url: 'javascript:alert(1)' is not an allowed link ...</c>.
    /// </summary>
    public override string ToString() => $"{SourcePath}: {Location}: {Message}";

    /// <summary>Gets where the problem is, for example <c>blocks[2].primary_action.url</c> or <c>body</c>.</summary>
    public string Location => BlockIndex switch
    {
        null => Key,
        { } index when Key.Length == 0 => $"blocks[{index}]",
        { } index => $"blocks[{index}].{Key}",
    };
}
