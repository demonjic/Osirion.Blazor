using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Navigation.Components;

/// <summary>
/// Links the current page in each of the site's languages, for a site whose addresses carry the culture in their first
/// path segment (<c>/contact</c>, <c>/sr/contact</c>). Each link is a plain anchor with <c>hreflang</c> and <c>lang</c>,
/// so it works under static rendering without a script; the current language is marked with <c>aria-current</c>.
/// Renders nothing when the site has fewer than two cultures.
/// </summary>
/// <remarks>
/// Needs <c>AddOsirionLocalization</c> (or <c>AddOsirion(configuration)</c> with an <c>Osirion:Localization</c>
/// section), which registers <see cref="IOsirionLocalePaths"/>. The query string of the current address is kept.
/// </remarks>
public partial class OsirionLanguagePicker : IDisposable
{
    private IReadOnlyList<LanguageLink> _links = [];
    private bool _isSubscribed;

    [Inject]
    private IOsirionLocalePaths LocalePaths { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>Gets or sets the accessible name of the navigation landmark. Defaults to <c>Language</c>.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Language";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Static SSR renders once per request, so only an interactive picker can see the location change.
        if (IsInteractive)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
            _isSubscribed = true;
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _links = BuildLinks();
    }

    /// <summary>Releases the location change subscription.</summary>
    public void Dispose()
    {
        if (_isSubscribed)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
            _isSubscribed = false;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _links = BuildLinks();
        _ = InvokeAsync(StateHasChanged);
    }

    private List<LanguageLink> BuildLinks()
    {
        var relative = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        var fragment = relative.IndexOf('#', StringComparison.Ordinal);

        // Leading slashes and backslashes collapse, so an address such as //example.com never becomes a link to
        // another host.
        var sitePath = "/" + (fragment < 0 ? relative : relative[..fragment]).TrimStart('/', '\\');
        var query = sitePath.IndexOf('?', StringComparison.Ordinal);
        var current = LocalePaths.TryGetCulture(query < 0 ? sitePath : sitePath[..query], out var prefixed, out _)
            ? prefixed
            : LocalePaths.DefaultCulture;

        // The links are written from the application's path base, never from the request's host.
        var pathBase = new Uri(NavigationManager.BaseUri).AbsolutePath.TrimEnd('/');
        var page = LocalePaths.WithoutLocale(sitePath);
        return LocalePaths.Cultures
            .Select(culture => new LanguageLink(
                culture.Culture.Name,
                culture.DisplayName,
                pathBase + LocalePaths.Localize(page, culture.Culture),
                culture == current))
            .ToList();
    }

    private sealed record LanguageLink(string Name, string DisplayName, string Href, bool IsCurrent);
}
