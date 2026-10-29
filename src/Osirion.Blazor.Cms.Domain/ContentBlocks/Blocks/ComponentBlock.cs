namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A component the site registers by name (<c>type: component</c>).</summary>
public sealed record ComponentBlock : ContentBlock
{
    /// <summary>Gets the name the component is registered under (<c>name</c>).</summary>
    public required string Name { get; init; }

    /// <summary>Gets the values passed to the component (<c>parameters</c>), as written.</summary>
    public IReadOnlyDictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();
}
