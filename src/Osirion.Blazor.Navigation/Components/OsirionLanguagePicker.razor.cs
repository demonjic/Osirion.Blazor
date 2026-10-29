using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
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
/// section), which registers <see cref="IOsirionLocalePaths"/>. The query string of the current address is kept. On a
/// page the framework re-executes for a status code or an error (<c>UseStatusCodePagesWithReExecute</c>,
/// <c>UseExceptionHandler</c>) the picker reads the address the visitor asked for, as the culture provider does, so a
/// not-found page under <c>/sr/</c> marks Serbian and links that address in each language.
/// </remarks>
public partial class OsirionLanguagePicker : IDisposable
{
    private IReadOnlyList<LanguageLink> _links = [];
    private bool _isSubscribed;

    [Inject]
    private IOsirionLocalePaths LocalePaths { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    // Present under static rendering, where a re-executed status page keeps the address the visitor asked for.
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>Gets or sets the accessible name of the navigation landmark. Defaults to <c>Language</c>.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Language";

    /// <summary>
    /// Gets or sets whether the picker is a disclosure that shows the current language and opens the list
    /// (<c>details</c> and <c>summary</c>, so it needs no script and opens with Enter or Space), for a header where a
    /// row of language names would wrap the menu. Defaults to <see langword="false"/>, a row of links.
    /// </summary>
    [Parameter]
    public bool Compact { get; set; }

    private string NavClass => Compact ? "osirion-language-picker osirion-language-picker-compact" : "osirion-language-picker";

    private LanguageLink CurrentLink => _links.FirstOrDefault(link => link.IsCurrent) ?? _links[0];

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
        var relative = OriginalRequestPath() ?? NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
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

    // The path and query the visitor asked for, below the path base, when this render re-executes the request for a
    // status code or an error page; null otherwise.
    private string? OriginalRequestPath()
    {
        if (HttpContext?.Features.Get<IStatusCodeReExecuteFeature>() is { } reExecute)
        {
            return reExecute.OriginalPath + reExecute.OriginalQueryString;
        }

        return HttpContext?.Features.Get<IExceptionHandlerPathFeature>()?.Path;
    }

    private sealed record LanguageLink(string Name, string DisplayName, string Href, bool IsCurrent);
}
