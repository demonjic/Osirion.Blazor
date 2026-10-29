using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.ContentBlocks;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>
/// The base class of a component that renders one content block. <c>OsirionContentBlocks</c> creates it with the
/// block and the context of the page; register a derived class with
/// <see cref="ContentBlockOptions.AddRenderer{TRenderer}(string)"/> to render a block type, or with
/// <see cref="ContentBlockOptions.UseFallbackRenderer{TRenderer}"/> to render types nothing else renders.
/// </summary>
public abstract class ContentBlockRenderer : ComponentBase
{
    /// <summary>Gets or sets the block to render.</summary>
    [Parameter, EditorRequired]
    public ContentBlock Block { get; set; } = default!;

    /// <summary>
    /// Gets or sets the page's block context: Markdown, link localization, nested blocks and problem reporting.
    /// </summary>
    [Parameter, EditorRequired]
    public ContentBlockContext Context { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (Block is null || Context is null)
        {
            throw new InvalidOperationException($"{GetType().Name} needs its Block and Context parameters; OsirionContentBlocks sets both.");
        }
    }
}

/// <summary>A <see cref="ContentBlockRenderer"/> for one block class.</summary>
/// <typeparam name="TBlock">The block class the renderer renders.</typeparam>
public abstract class ContentBlockRenderer<TBlock> : ContentBlockRenderer
    where TBlock : ContentBlock
{
    /// <summary>Gets <see cref="ContentBlockRenderer.Block"/> as <typeparamref name="TBlock"/>.</summary>
    /// <exception cref="InvalidOperationException">The renderer was registered for a type of another class.</exception>
    protected TBlock Model => Block as TBlock
        ?? throw new InvalidOperationException(
            $"{GetType().Name} renders {typeof(TBlock).Name} blocks and was given a {Block.GetType().Name} of type '{Block.Type}'; check the type it is registered for.");
}
