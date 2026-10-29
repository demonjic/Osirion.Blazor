using Microsoft.AspNetCore.Components.Rendering;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks.Renderers;

/// <summary>
/// Renders a <see cref="ComponentBlock"/> with the component the site registered under its <c>name</c>, passing
/// <c>parameters</c> to the component's <c>Parameters</c> parameter when it has one. An unknown name, or parameters for
/// a component that takes none, is a problem.
/// </summary>
internal sealed class ComponentBlockRenderer : ContentBlockRenderer<ComponentBlock>
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var block = Model;
        if (!Context.Registry.TryGetComponent(block.Name, out var component))
        {
            builder.AddContent(0, Context.Problem(block, $"no component is registered as '{block.Name}'"));
            return;
        }

        var acceptsParameters = ContentBlockRegistry.AcceptsParameters(component);
        if (block.Parameters.Count > 0 && !acceptsParameters)
        {
            builder.AddContent(1, Context.Problem(block,
                $"the component '{block.Name}' has no Parameters parameter of type IReadOnlyDictionary<string, string> to take the block's parameters"));
            return;
        }

        builder.OpenComponent(2, component);
        if (acceptsParameters)
        {
            builder.AddComponentParameter(3, ContentBlockRegistry.ParametersParameterName, block.Parameters);
        }

        builder.CloseComponent();
    }
}
