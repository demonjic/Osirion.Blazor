using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Cms.Domain.ValueObjects;

namespace Osirion.Blazor.Cms.Admin.Features.ContentEditor.Components.Shared;

/// <summary>Defines the public member type.</summary>
public partial class BasicMetadataForm
{
    /// <summary>Performs the Metadata operation.</summary>
    [Parameter]
    public FrontMatter Metadata { get; set; } = FrontMatter.Create("New Post");

    /// <summary>Gets or sets the MetadataChanged value.</summary>
    [Parameter]
    public EventCallback<FrontMatter> MetadataChanged { get; set; }

    private bool ImageLoadError { get; set; } = false;

    // The field shows and takes the moment in UTC: the front matter reads a moment without an offset as UTC
    private DateTime PostDate
    {
        get
        {
            if (FrontMatterDates.TryParse(Metadata.Date, out var date))
            {
                return date;
            }
            return DateTime.UtcNow;
        }
        set
        {
            Metadata = Metadata.WithDate(value);
            NotifyMetadataChanged().ConfigureAwait(false);
        }
    }

    /// <summary>Performs the OnParametersSet operation.</summary>
    protected override void OnParametersSet()
    {
        Metadata ??= FrontMatter.Create("New Post");
        ImageLoadError = false;
    }

    private async Task NotifyMetadataChanged()
    {
        if (MetadataChanged.HasDelegate)
        {
            await MetadataChanged.InvokeAsync(Metadata);
        }
    }

    private async Task OnCategoriesChanged(List<string> categories)
    {
        Metadata = Metadata.WithCategories(categories);
        await NotifyMetadataChanged();
    }

    private async Task OnTagsChanged(List<string> tags)
    {
        Metadata = Metadata.WithTags(tags);
        await NotifyMetadataChanged();
    }
}
