using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>
/// Configures how content blocks render: a renderer per block type, the components a <c>component</c> block can name,
/// and a fallback for block types the library does not define. Configure it with
/// <c>AddOsirionContentBlocks</c>; it starts with the library's renderers.
/// </summary>
public sealed class ContentBlockOptions
{
    private readonly Dictionary<string, Type> _renderers = new(StringComparer.Ordinal)
    {
        [ContentBlockTypes.Hero] = typeof(HeroBlockRenderer),
        [ContentBlockTypes.Section] = typeof(SectionBlockRenderer),
        [ContentBlockTypes.Steps] = typeof(StepsBlockRenderer),
        [ContentBlockTypes.Cards] = typeof(CardsBlockRenderer),
        [ContentBlockTypes.Metrics] = typeof(MetricsBlockRenderer),
        [ContentBlockTypes.Faq] = typeof(FaqBlockRenderer),
        [ContentBlockTypes.Cta] = typeof(CtaBlockRenderer),
        [ContentBlockTypes.Image] = typeof(ImageBlockRenderer),
        [ContentBlockTypes.Markdown] = typeof(MarkdownBlockRenderer),
        [ContentBlockTypes.Component] = typeof(ComponentBlockRenderer),
    };

    private readonly Dictionary<string, Type> _components = new(StringComparer.Ordinal);

    internal IReadOnlyDictionary<string, Type> Renderers => _renderers;

    internal IReadOnlyDictionary<string, Type> Components => _components;

    internal Type? FallbackRenderer { get; private set; }

    /// <summary>
    /// Renders blocks of <paramref name="type"/> with <typeparamref name="TRenderer"/>, in place of the library's
    /// renderer for that type when it has one. A type the library does not define arrives as a
    /// <see cref="CustomContentBlock"/>.
    /// </summary>
    /// <typeparam name="TRenderer">The renderer component.</typeparam>
    /// <param name="type">The block type, lower case words joined by hyphens, for example <c>pricing-table</c>.</param>
    /// <returns>These options, for chaining.</returns>
    public ContentBlockOptions AddRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRenderer>(string type)
        where TRenderer : ContentBlockRenderer
    {
        _renderers[RequireName(type, nameof(type))] = typeof(TRenderer);
        return this;
    }

    /// <summary>
    /// Makes <typeparamref name="TComponent"/> the component a <c>component</c> block with <c>name: </c>
    /// <paramref name="name"/> renders. When the component has a parameter <c>Parameters</c> of type
    /// <c>IReadOnlyDictionary&lt;string, string&gt;</c>, it receives the block's <c>parameters</c>.
    /// </summary>
    /// <typeparam name="TComponent">The component.</typeparam>
    /// <param name="name">The name content uses, for example <c>product-tour</c>.</param>
    /// <returns>These options, for chaining.</returns>
    public ContentBlockOptions AddComponent<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(string name)
        where TComponent : IComponent
    {
        _components[RequireName(name, nameof(name))] = typeof(TComponent);
        return this;
    }

    /// <summary>
    /// Renders blocks whose type has no renderer with <typeparamref name="TRenderer"/>. Without a fallback such a block
    /// is a problem: it renders nothing and is logged as an error (and shown in Development).
    /// </summary>
    /// <typeparam name="TRenderer">The renderer component; it receives a <see cref="CustomContentBlock"/>.</typeparam>
    /// <returns>These options, for chaining.</returns>
    public ContentBlockOptions UseFallbackRenderer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRenderer>()
        where TRenderer : ContentBlockRenderer
    {
        FallbackRenderer = typeof(TRenderer);
        return this;
    }

    private static string RequireName(string name, string parameterName)
    {
        var words = name?.Split('-');
        if (words is null || !words.All(word => word.Length > 0 && word.All(character => character is (>= 'a' and <= 'z') or (>= '0' and <= '9'))))
        {
            throw new ArgumentException($"'{name}' must be lower case letters and digits, words joined by hyphens.", parameterName);
        }

        return name!;
    }
}
