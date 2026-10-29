namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>
/// A block whose type the library does not define. A site renders it with a renderer registered for its type or
/// with its fallback renderer.
/// </summary>
/// <remarks>
/// The common keys are read and validated as for every block; the other keys are kept in <see cref="Values"/>
/// unchanged, so a site renderer validates them itself.
/// </remarks>
public sealed record CustomContentBlock : ContentBlock
{
    /// <summary>
    /// Gets the block's other keys and their values as read from YAML: a <see cref="string"/>, a list, a map, or
    /// <see langword="null"/>.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
}
