using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.Components;

public abstract partial class OsirionContentPageBase
{
    // The switches of SeoMetadataRenderer that the site-wide options also set. The renderer keeps the configured value
    // for a switch it was not given, so a page passes one on only when its own parameter was set; passing the page's
    // default would override the configuration.
    private static readonly string[] SeoSwitchParameters =
    [
        nameof(AllowAiDiscovery),
        nameof(AllowAiTraining),
        nameof(AllowSearchIndexing),
        nameof(EnableGeoOptimization),
        nameof(EnableAeoOptimization),
    ];

    private HashSet<string> _suppliedSeoSwitches = [];

    /// <inheritdoc/>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        var supplied = new HashSet<string>(_suppliedSeoSwitches);
        foreach (var parameter in parameters)
        {
            if (Array.IndexOf(SeoSwitchParameters, parameter.Name) >= 0)
            {
                supplied.Add(parameter.Name);
            }
        }

        _suppliedSeoSwitches = supplied;
        return base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// The SEO parameters of this page that <c>SeoMetadataRenderer</c> shares, as a splat for the renderer's markup
    /// (<c>@attributes="SeoRendererParameters"</c>): the handles, the site description, the default image and its size,
    /// the canonical path, and each of the indexing and optimization switches the page was given. A parameter the page was not given is left
    /// out, so the renderer applies the site-wide configuration for it.
    /// </summary>
    protected IReadOnlyDictionary<string, object> SeoRendererParameters
    {
        get
        {
            var parameters = new Dictionary<string, object>();
            AddWhenSet(parameters, nameof(SiteDescription), SiteDescription);
            AddWhenSet(parameters, nameof(TwitterSite), TwitterSite);
            AddWhenSet(parameters, nameof(TwitterCreator), TwitterCreator);
            AddWhenSet(parameters, nameof(FacebookAppId), FacebookAppId);
            AddWhenSet(parameters, nameof(DefaultImageUrl), DefaultImageUrl);
            AddWhenSet(parameters, nameof(CanonicalPath), CanonicalPath);
            parameters[nameof(DefaultImageWidth)] = DefaultImageWidth;
            parameters[nameof(DefaultImageHeight)] = DefaultImageHeight;
            AddSwitch(parameters, nameof(AllowAiDiscovery), AllowAiDiscovery);
            AddSwitch(parameters, nameof(AllowAiTraining), AllowAiTraining);
            AddSwitch(parameters, nameof(AllowSearchIndexing), AllowSearchIndexing);
            AddSwitch(parameters, nameof(EnableGeoOptimization), EnableGeoOptimization);
            AddSwitch(parameters, nameof(EnableAeoOptimization), EnableAeoOptimization);
            return parameters;
        }
    }

    private static void AddWhenSet(Dictionary<string, object> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters[name] = value;
        }
    }

    private void AddSwitch(Dictionary<string, object> parameters, string name, bool value)
    {
        if (_suppliedSeoSwitches.Contains(name))
        {
            parameters[name] = value;
        }
    }

    /// <summary>
    /// Renders the not-found state of the page: an <c>OsirionContentNotFound</c> with the code <c>404</c>, <see
    /// cref="NotFoundTitle"/>, <see cref="NotFoundText"/> and, with <see cref="ShowBackLink"/> on and an address in
    /// <see cref="BackLinkUrl"/>, a button to <see cref="BackLinkUrl"/> labeled <see cref="BackLinkText"/>.
    /// </summary>
    protected RenderFragment RenderNotFound => builder =>
    {
        var backLink = ShowBackLink && !string.IsNullOrWhiteSpace(BackLinkUrl);
        builder.OpenComponent<OsirionContentNotFound>(0);
        builder.AddComponentParameter(1, nameof(OsirionContentNotFound.ErrorCode), "404");
        builder.AddComponentParameter(2, nameof(OsirionContentNotFound.Title), NotFoundTitle);
        builder.AddComponentParameter(3, nameof(OsirionContentNotFound.Message), NotFoundText);
        builder.AddComponentParameter(4, nameof(OsirionContentNotFound.PrimaryButtonText), backLink ? BackLinkText : null);
        builder.AddComponentParameter(5, nameof(OsirionContentNotFound.PrimaryButtonUrl), backLink ? BackLinkUrl : null);
        builder.CloseComponent();
    };
}
