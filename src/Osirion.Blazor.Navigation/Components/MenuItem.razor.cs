using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Osirion.Blazor.Navigation.Internal;

namespace Osirion.Blazor.Navigation.Components;

/// <summary>
/// Renders one menu entry: a link, or a parent that opens a submenu. It marks itself active when the current path
/// matches its <see cref="Href"/>. The submenu opens with CSS only.
/// </summary>
public partial class MenuItem : IDisposable
{
    /// <summary>
    /// Gets or sets the link text.
    /// </summary>
    [Parameter]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets supporting text under the link text.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the name of a built-in icon: <c>home</c>, <c>blog</c>, <c>document</c>, <c>docs</c>,
    /// <c>documentation</c>, <c>user</c>, <c>profile</c>, <c>contact</c>, <c>about</c>, <c>info</c>, <c>settings</c>,
    /// <c>gear</c> or <c>dashboard</c>. Any other name renders no icon. <see cref="IconTemplate"/> takes precedence.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets your own icon markup, rendered instead of <see cref="Icon"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? IconTemplate { get; set; }

    /// <summary>
    /// Gets or sets the link address. Only a relative URL or the <c>http</c>, <c>https</c>, <c>mailto</c> and
    /// <c>tel</c> schemes get an <c>href</c>; with another scheme, such as <c>javascript:</c>, the link is rendered
    /// without one. A parent with <see cref="HasSubmenu"/> and no address (empty or <c>#</c>) renders as the submenu
    /// toggle itself instead of a dead link.
    /// </summary>
    [Parameter]
    public string Href { get; set; } = "#";

    /// <summary>
    /// Gets or sets whether the item is forced into the active state, with the <c>aria-current="page"</c> attribute
    /// and the active style, whatever the current URL is.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets whether the item is disabled: it renders no <c>href</c>, sets <c>aria-disabled</c> and ignores
    /// <see cref="OnClick"/>.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="ChildContent"/> is rendered as a submenu behind a disclosure toggle.
    /// </summary>
    [Parameter]
    public bool HasSubmenu { get; set; }

    /// <summary>
    /// Gets or sets the accessible name of the submenu's toggle, with <c>{0}</c> where the item's <see cref="Text"/>
    /// goes. Defaults to <c>Toggle {0} submenu</c>; set it in the page's language, for example
    /// <c>Otvori podmeni {0}</c>, so the name matches the language of the page (WCAG 3.1.2).
    /// </summary>
    [Parameter]
    public string ToggleSubmenuLabel { get; set; } = DefaultToggleSubmenuLabel;

    /// <summary>The default of <see cref="ToggleSubmenuLabel"/>.</summary>
    public const string DefaultToggleSubmenuLabel = "Toggle {0} submenu";

    /// <summary>
    /// Gets or sets the submenu content: menu items, or groups and other parts for a mega submenu. It is rendered only
    /// when <see cref="HasSubmenu"/> is on.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets optional aside content for mega submenus, rendered as a
    /// dedicated right-hand column. Typically a <see cref="MenuPanel"/> or
    /// <see cref="MenuImageCard"/>. Only used when <see cref="SubmenuVariant"/> is Mega.
    /// </summary>
    [Parameter]
    public RenderFragment? SubmenuAside { get; set; }

    /// <summary>
    /// Gets or sets the submenu presentation: <c>Regular</c> for a list or <c>Mega</c> for a grid of groups.
    /// </summary>
    [Parameter]
    public SubmenuVariant SubmenuVariant { get; set; } = SubmenuVariant.Regular;

    /// <summary>
    /// Gets or sets how the current path is compared with <see cref="Href"/>: <c>All</c> needs the same path,
    /// <c>Prefix</c> also matches child paths on a segment boundary. The query string, the fragment and surrounding
    /// slashes are ignored.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; } = NavLinkMatch.All;

    /// <summary>
    /// Gets or sets the <c>target</c> attribute of the link. <c>_blank</c> also adds <c>rel="noopener noreferrer"</c>.
    /// </summary>
    [Parameter]
    public string? Target { get; set; }

    /// <summary>
    /// Gets or sets the callback raised when the link is clicked. It runs only in an interactive render mode. A parent
    /// that has it renders as a link, not as the submenu toggle, even without an <see cref="Href"/>.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the base of the submenu and toggle ids, <c>submenu-{Id}</c>. Without it a generated value is used.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets whether the active state is detected from the current URL. Under static SSR it is computed once
    /// per request; in an interactive render mode the item also follows later navigations.
    /// </summary>
    [Parameter]
    public bool AutoDetectActive { get; set; } = true;

    /// <summary>
    /// Navigation manager for URL matching and navigation.
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly string _generatedId = Guid.NewGuid().ToString("N")[..8];

    // Derived render state. [Parameter] properties are never written by the component, so a parent
    // re-render cannot be overridden by a stale computed value.
    private bool _isActive;
    private string? _renderedHref;
    private bool _isToggleOnly;
    private bool _isSubscribed;

    /// <summary>
    /// Gets the unique identifier for the submenu.
    /// </summary>
    internal string SubmenuId => $"submenu-{Id ?? _generatedId}";

    /// <summary>
    /// Gets the identifier for the submenu toggle control.
    /// </summary>
    internal string SubmenuToggleId => $"{SubmenuId}-toggle";

    // Replaced, not formatted, so a label with other braces never throws.
    private string SubmenuToggleLabel => ToggleSubmenuLabel.Replace("{0}", Text, StringComparison.Ordinal);

    internal string SubmenuVariantClass => SubmenuVariant switch
    {
        SubmenuVariant.Mega => "osirion-submenu-mega",
        _ => "osirion-submenu-regular"
    };

    // Bootstrap Icons path data for the built-in icon names. A lookup keeps a long switch out of the
    // markup; aliases share one entry.
    private static readonly Dictionary<string, IconPath[]> BuiltInIcons = CreateBuiltInIcons();

    private string WrapperCssClass =>
        $"osirion-menu-item-wrapper {(HasSubmenu ? "osirion-menu-item-has-submenu" : "")} {(HasSubmenu && SubmenuVariant == SubmenuVariant.Mega ? "osirion-menu-item-has-mega" : "")} {(_isActive ? "osirion-menu-item-wrapper-active" : "")}";

    private string LinkCssClass =>
        $"osirion-menu-item {(_isActive ? "osirion-menu-item-active" : "")} {(Disabled ? "osirion-menu-item-disabled" : "")} {Class}";

    private string? RenderedTarget => _renderedHref is null ? null : Target;

    private string? RenderedRel =>
        _renderedHref is not null && string.Equals(Target, "_blank", StringComparison.OrdinalIgnoreCase)
            ? "noopener noreferrer"
            : null;

    // An anchor without href is not a link to assistive technology, so the role is restored explicitly.
    private string? LinkRole => _renderedHref is null ? "link" : null;

    private string AriaDisabled => Disabled ? "true" : "false";

    private string? AriaCurrent => _isActive ? "page" : null;

    private IconPath[]? BuiltInIconPaths =>
        BuiltInIcons.GetValueOrDefault(Icon?.ToLowerInvariant() ?? string.Empty);

    /// <summary>Initializes the component state and required services.</summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // Static SSR renders once per request, so only an interactive item can see the location change.
        if (IsInteractive && AutoDetectActive)
        {
            NavigationManager.LocationChanged += OnLocationChanged;
            _isSubscribed = true;
        }
    }

    /// <summary>Recomputes the effective link target and active state from the current parameters.</summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        RefreshRenderState();
    }

    private void RefreshRenderState()
    {
        // A disabled item and a rejected URL (for example a javascript: scheme) render no href at all.
        _renderedHref = Disabled ? null : SafeHref.Sanitize(Href);
        _isActive = IsActive || (AutoDetectActive && MatchesCurrentLocation());

        // A parent that only groups its submenu has nowhere to go, so the whole item toggles the submenu.
        _isToggleOnly = HasSubmenu && !Disabled && !OnClick.HasDelegate
            && (string.IsNullOrWhiteSpace(Href) || Href == "#");
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var wasActive = _isActive;
        RefreshRenderState();

        if (wasActive != _isActive)
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private bool MatchesCurrentLocation()
    {
        if (string.IsNullOrWhiteSpace(Href) || Href == "#" || Disabled)
        {
            return false;
        }

        var currentPath = TrimToPath(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        var targetPath = TrimToPath(Href);

        return Match switch
        {
            NavLinkMatch.All => string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase),
            NavLinkMatch.Prefix => targetPath.Length == 0
                || string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase)
                || currentPath.StartsWith(targetPath + "/", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    // Compares paths only: a query string or fragment must not change the match, and neither must
    // a leading or trailing slash.
    private static string TrimToPath(string value)
    {
        var end = value.AsSpan().IndexOfAny('?', '#');
        var path = end >= 0 ? value[..end] : value;
        return path.Trim('/');
    }

    /// <summary>
    /// Handles the click event on the menu item.
    /// </summary>
    /// <param name="args">Mouse event arguments</param>
    private async Task OnClickHandler(MouseEventArgs args)
    {
        if (!Disabled && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }
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

    private static Dictionary<string, IconPath[]> CreateBuiltInIcons()
    {
        IconPath[] home = [new("M8.707 1.5a1 1 0 0 0-1.414 0L.646 8.146a.5.5 0 0 0 .708.708L2 8.207V13.5A1.5 1.5 0 0 0 3.5 15h9a1.5 1.5 0 0 0 1.5-1.5V8.207l.646.647a.5.5 0 0 0 .708-.708L13 5.793V2.5a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5v1.293zM13 7.207V13.5a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V7.207l5-5z")];
        IconPath[] blog =
        [
            new("M5 10.5a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5m0-2a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5m0-2a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5m0-2a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5"),
            new("M3 0h10a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2v-1h1v1a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V2a1 1 0 0 0-1-1H3a1 1 0 0 0-1 1v1H1V2a2 2 0 0 1 2-2"),
            new("M1 5v-.5a.5.5 0 0 1 1 0V5h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1zm0 3v-.5a.5.5 0 0 1 1 0V8h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1zm0 3v-.5a.5.5 0 0 1 1 0v.5h.5a.5.5 0 0 1 0 1h-2a.5.5 0 0 1 0-1z")
        ];
        IconPath[] document =
        [
            new("M5.5 7a.5.5 0 0 0 0 1h5a.5.5 0 0 0 0-1zM5 9.5a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 0 1h-5a.5.5 0 0 1-.5-.5m0 2a.5.5 0 0 1 .5-.5h2a.5.5 0 0 1 0 1h-2a.5.5 0 0 1-.5-.5"),
            new("M9.5 0H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V4.5zm0 1v2A1.5 1.5 0 0 0 11 4.5h2V14a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1z")
        ];
        IconPath[] documentation = [new("M1 2.828c.885-.37 2.154-.769 3.388-.893 1.33-.134 2.458.063 3.112.752v9.746c-.935-.53-2.12-.603-3.213-.493-1.18.12-2.37.461-3.287.811zm7.5-.141c.654-.689 1.782-.886 3.112-.752 1.234.124 2.503.523 3.388.893v9.923c-.918-.35-2.107-.692-3.287-.81-1.094-.111-2.278-.039-3.213.492zM8 1.783C7.015.936 5.587.81 4.287.94c-1.514.153-3.042.672-3.994 1.105A.5.5 0 0 0 0 2.5v11a.5.5 0 0 0 .707.455c.882-.4 2.303-.881 3.68-1.02 1.409-.142 2.59.087 3.223.877a.5.5 0 0 0 .78 0c.633-.79 1.814-1.019 3.222-.877 1.378.139 2.8.62 3.681 1.02A.5.5 0 0 0 16 13.5v-11a.5.5 0 0 0-.293-.455c-.952-.433-2.48-.952-3.994-1.105C10.413.809 8.985.936 8 1.783")];
        IconPath[] profile = [new("M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10s-3.516.68-4.168 1.332c-.678.678-.83 1.418-.832 1.664z")];
        IconPath[] contact = [new("M3.654 1.328a.678.678 0 0 0-1.015-.063L1.605 2.3c-.483.484-.661 1.169-.45 1.77a17.6 17.6 0 0 0 4.168 6.608 17.6 17.6 0 0 0 6.608 4.168c.601.211 1.286.033 1.77-.45l1.034-1.034a.678.678 0 0 0-.063-1.015l-2.307-1.794a.68.68 0 0 0-.58-.122l-2.19.547a1.75 1.75 0 0 1-1.657-.459L5.482 8.062a1.75 1.75 0 0 1-.46-1.657l.548-2.19a.68.68 0 0 0-.122-.58zM1.884.511a1.745 1.745 0 0 1 2.612.163L6.29 2.98c.329.423.445.974.315 1.494l-.547 2.19a.68.68 0 0 0 .178.643l2.457 2.457a.68.68 0 0 0 .644.178l2.189-.547a1.75 1.75 0 0 1 1.494.315l2.306 1.794c.829.645.905 1.87.163 2.611l-1.034 1.034c-.74.74-1.846 1.065-2.877.702a18.6 18.6 0 0 1-7.01-4.42 18.6 18.6 0 0 1-4.42-7.009c-.362-1.03-.037-2.137.703-2.877z")];
        IconPath[] info =
        [
            new("M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"),
            new("m8.93 6.588-2.29.287-.082.38.45.083c.294.07.352.176.288.469l-.738 3.468c-.194.897.105 1.319.808 1.319.545 0 1.178-.252 1.465-.598l.088-.416c-.2.176-.492.246-.686.246-.275 0-.375-.193-.304-.533zM9 4.5a1 1 0 1 1-2 0 1 1 0 0 1 2 0")
        ];
        IconPath[] settings =
        [
            new("M8 4.754a3.246 3.246 0 1 0 0 6.492 3.246 3.246 0 0 0 0-6.492M5.754 8a2.246 2.246 0 1 1 4.492 0 2.246 2.246 0 0 1-4.492 0"),
            new("M9.796 1.343c-.527-1.79-3.065-1.79-3.592 0l-.094.319a.873.873 0 0 1-1.255.52l-.292-.16c-1.64-.892-3.433.902-2.54 2.541l.159.292a.873.873 0 0 1-.52 1.255l-.319.094c-1.79.527-1.79 3.065 0 3.592l.319.094a.873.873 0 0 1 .52 1.255l-.16.292c-.892 1.64.901 3.434 2.541 2.54l.292-.159a.873.873 0 0 1 1.255.52l.094.319c.527 1.79 3.065 1.79 3.592 0l.094-.319a.873.873 0 0 1 1.255-.52l.292.16c1.64.893 3.434-.902 2.54-2.541l-.159-.292a.873.873 0 0 1 .52-1.255l.319-.094c1.79-.527 1.79-3.065 0-3.592l-.319-.094a.873.873 0 0 1-.52-1.255l.16-.292c.893-1.64-.902-3.433-2.541-2.54l-.292.159a.873.873 0 0 1-1.255-.52zm-2.633.283c.246-.835 1.428-.835 1.674 0l.094.319a1.873 1.873 0 0 0 2.693 1.115l.291-.16c.764-.415 1.6.42 1.184 1.185l-.159.292a1.873 1.873 0 0 0 1.116 2.692l.318.094c.835.246.835 1.428 0 1.674l-.319.094a1.873 1.873 0 0 0-1.115 2.693l.16.291c.415.764-.42 1.6-1.185 1.184l-.291-.159a1.873 1.873 0 0 0-2.693 1.116l-.094.318c-.246.835-1.428.835-1.674 0l-.094-.319a1.873 1.873 0 0 0-2.692-1.115l-.292.16c-.764.415-1.6-.42-1.184-1.185l.159-.291A1.873 1.873 0 0 0 1.945 8.93l-.319-.094c-.835-.246-.835-1.428 0-1.674l.319-.094A1.873 1.873 0 0 0 3.06 4.377l-.16-.292c-.415-.764.42-1.6 1.185-1.184l.292.159a1.873 1.873 0 0 0 2.692-1.115z")
        ];
        IconPath[] dashboard =
        [
            new("M8 4a.5.5 0 0 1 .5.5V6a.5.5 0 0 1-1 0V4.5A.5.5 0 0 1 8 4M3.732 5.732a.5.5 0 0 1 .707 0l.915.914a.5.5 0 1 1-.708.708l-.914-.915a.5.5 0 0 1 0-.707M2 10a.5.5 0 0 1 .5-.5h1.586a.5.5 0 0 1 0 1H2.5A.5.5 0 0 1 2 10m9.5 0a.5.5 0 0 1 .5-.5h1.5a.5.5 0 0 1 0 1H12a.5.5 0 0 1-.5-.5m.754-4.246a.39.39 0 0 0-.527-.02L7.547 9.31a.91.91 0 1 0 1.302 1.258l3.434-4.297a.39.39 0 0 0-.029-.518z"),
            new("M0 10a8 8 0 1 1 15.547 2.661c-.442 1.253-1.845 1.602-2.932 1.25C11.309 13.488 9.475 13 8 13c-1.474 0-3.31.488-4.615.911-1.087.352-2.49.003-2.932-1.25A7.99 7.99 0 0 1 0 10m8-7a7 7 0 0 0-6.603 9.329c.203.575.923.876 1.68.63C4.397 12.533 6.358 12 8 12s3.604.532 4.923.96c.757.245 1.477-.056 1.68-.631A7 7 0 0 0 8 3", "evenodd")
        ];

        return new Dictionary<string, IconPath[]>(StringComparer.Ordinal)
        {
            ["home"] = home,
            ["blog"] = blog,
            ["document"] = document,
            ["docs"] = documentation,
            ["documentation"] = documentation,
            ["user"] = profile,
            ["profile"] = profile,
            ["contact"] = contact,
            ["about"] = info,
            ["info"] = info,
            ["settings"] = settings,
            ["gear"] = settings,
            ["dashboard"] = dashboard
        };
    }

    private readonly record struct IconPath(string Data, string? FillRule = null);
}
