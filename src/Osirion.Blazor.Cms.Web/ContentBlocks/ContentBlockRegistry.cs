using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>
/// Finds the renderer of a block type and the component of a <c>component</c> block's name. Built from
/// <see cref="ContentBlockOptions"/> by <c>AddOsirionContentBlocks</c>; without that call <c>OsirionContentBlocks</c>
/// uses <see cref="Default"/>, the library's renderers alone.
/// </summary>
public sealed class ContentBlockRegistry
{
    // The renderers of these types compose components that are not in the library yet. Until they are, a block of
    // one of these types renders nothing and is logged as a warning, unless the site registers its own renderer.
    private static readonly HashSet<string> TypesAwaitingRenderer = new(StringComparer.Ordinal)
    {
        ContentBlockTypes.Split,
        ContentBlockTypes.Checklist,
        ContentBlockTypes.Record,
    };

    private const string ParametersName = "Parameters";

    private readonly IReadOnlyDictionary<string, Type> _renderers;
    private readonly IReadOnlyDictionary<string, Type> _components;

    /// <summary>Initializes the registry from configured options.</summary>
    /// <param name="options">The options.</param>
    public ContentBlockRegistry(ContentBlockOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _renderers = new Dictionary<string, Type>(options.Renderers, StringComparer.Ordinal);
        _components = new Dictionary<string, Type>(options.Components, StringComparer.Ordinal);
        FallbackRenderer = options.FallbackRenderer;
    }

    /// <summary>Gets the registry with the library's renderers, no components and no fallback.</summary>
    public static ContentBlockRegistry Default { get; } = new(new ContentBlockOptions());

    /// <summary>Gets the renderer of block types that have none, or <see langword="null"/>.</summary>
    public Type? FallbackRenderer { get; }

    /// <summary>Finds the renderer registered for <paramref name="type"/>.</summary>
    /// <param name="type">The block type.</param>
    /// <param name="renderer">The renderer component type.</param>
    /// <returns>Whether a renderer is registered.</returns>
    public bool TryGetRenderer(string type, [NotNullWhen(true)] out Type? renderer) =>
        _renderers.TryGetValue(type, out renderer);

    /// <summary>Finds the component registered as <paramref name="name"/>.</summary>
    /// <param name="name">The name a <c>component</c> block uses.</param>
    /// <param name="component">The component type.</param>
    /// <returns>Whether a component is registered.</returns>
    public bool TryGetComponent(string name, [NotNullWhen(true)] out Type? component) =>
        _components.TryGetValue(name, out component);

    /// <summary>Returns whether <paramref name="type"/> is a library type whose renderer is not available yet.</summary>
    internal static bool AwaitsRenderer(string type) => TypesAwaitingRenderer.Contains(type);

    /// <summary>
    /// Returns whether <paramref name="component"/> has a <c>Parameters</c> parameter that takes the block's
    /// <c>parameters</c>.
    /// </summary>
    internal static bool AcceptsParameters([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type component) =>
        component.GetProperty(ParametersName, BindingFlags.Public | BindingFlags.Instance) is { } property
        && property.IsDefined(typeof(ParameterAttribute), inherit: true)
        && property.PropertyType == typeof(IReadOnlyDictionary<string, string>);

    /// <summary>The name of the parameter that receives a <c>component</c> block's <c>parameters</c>.</summary>
    internal static string ParametersParameterName => ParametersName;
}
