using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>Defines the SearchBox type.</summary>
public partial class SearchBox
{
    /// <summary>Gets or sets the page the form submits to with GET. Defaults to <c>/search</c>.</summary>
    [Parameter]
    public string ActionUrl { get; set; } = "/search";

    /// <summary>
    /// Gets or sets the name of the query string parameter that carries the search text, which is also the name of
    /// the input. Defaults to <c>q</c>.
    /// </summary>
    [Parameter]
    public string QueryParameterName { get; set; } = "q";

    /// <summary>
    /// Gets or sets the text shown in the field, usually the query of the results page, so it stays in the field
    /// after a search.
    /// </summary>
    [Parameter]
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholder of the field. It is a hint only: the field's accessible name is
    /// <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Search content...";

    /// <summary>
    /// Gets or sets the visible text of the submit button. The button's accessible name is <see cref="SubmitLabel"/>.
    /// </summary>
    [Parameter]
    public string SearchButtonText { get; set; } = "Search";

    /// <summary>
    /// Gets or sets the accessible name of the submit button. Set it for another language, and keep the words of
    /// <see cref="SearchButtonText"/> in it, so a reader who names the button by what it shows reaches it. Defaults to
    /// <c>Submit search</c>.
    /// </summary>
    [Parameter]
    public string SubmitLabel { get; set; } = "Submit search";

    /// <summary>Gets or sets the heading above the form, rendered as an <c>h2</c>. No heading when <c>null</c>.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the text of the field's visually hidden label, which is its accessible name. Set it for another
    /// language.
    /// </summary>
    [Parameter]
    public string Label { get; set; } = "Search";

    /// <summary>
    /// Gets or sets the <c>id</c> of the input, which the label points to. Without it, the id is derived from
    /// <see cref="ActionUrl"/> and <see cref="QueryParameterName"/>, so it is the same on every render and in every
    /// process. Set it when one page holds two boxes with the same settings.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    private string InputId => string.IsNullOrWhiteSpace(Id)
        ? $"search-input-{ContentUrls.StableHash(ActionUrl + "?" + QueryParameterName)}"
        : Id;

    private string GetWrapperClass()
    {
        return $"osirion-search-container {Class}".Trim();
    }

    private string GetSearchBoxClass()
    {
        return "osirion-search-box";
    }
}
