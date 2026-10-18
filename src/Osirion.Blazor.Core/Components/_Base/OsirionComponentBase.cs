using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using BlazorJSComponents;

namespace Osirion.Blazor.Components;

/// <summary>
/// Base component class that provides common functionality for all Osirion.Blazor components.
/// </summary>
public abstract partial class OsirionComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets additional attributes that will be applied to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the component's color mode. <c>Light</c> or <c>Dark</c> writes it to <c>data-theme</c>
    /// (<c>data-bs-theme</c> under Bootstrap) on the root element; <c>System</c> writes nothing.
    /// </summary>
    [Parameter]
    public ThemeMode Theme { get; set; } = ThemeMode.System;

    /// <summary>
    /// Gets or sets whether the component should be considered interactive.
    /// </summary>
    /// <remarks>
    /// This is determined by <c>RendererInfo?.IsInteractive</c>.
    /// </remarks>
    [Obsolete("This parameter is ignored in .NET 9+ as interactivity is automatically determined.", true)]
    [Parameter]
    public bool SetInteractive { get; set; }

    [Inject]
    private IOptions<ThemingOptions> ThemingOptions { get; set; } = default!;

    private IDictionary<string, object>? _ownedAttributes;
    private string? _themeAttributeName;

    /// <summary>
    /// Indicates that the code is not running in the browser (WebAssembly runtime).
    /// </summary>
    /// <remarks>
    /// This is also <see langword="true"/> under Interactive Server rendering, where JavaScript interop is
    /// available, so it does not mean "not interactive". Use <see cref="IsInteractive"/> to ask whether the
    /// component runs interactively, and <see cref="IsRunningInBrowser"/> for the runtime question.
    /// </remarks>
    [Obsolete("IsServerSide is true under Interactive Server as well, so '!IsServerSide' skips JavaScript enhancement there. Use IsInteractive (is the component interactive) or IsRunningInBrowser (is the WebAssembly runtime executing it).")]
    protected bool IsServerSide => !OperatingSystem.IsBrowser();

    /// <summary>
    /// Indicates that the component code executes in the browser, on the WebAssembly runtime.
    /// </summary>
    protected bool IsRunningInBrowser => OperatingSystem.IsBrowser();

    /// <summary>
    /// Indicates whether the component is in an interactive rendering mode.
    /// </summary>
    /// <remarks>
    /// This is determined by <c>RendererInfo?.IsInteractive</c>.
    /// </remarks>
    protected bool IsInteractive { get; private set; }

    /// <summary>
    /// Gets the current theme mode.
    /// </summary>
    protected CssFramework Framework => ThemingOptions.Value.Framework;

    /// <inheritdoc />
    /// <remarks>
    /// Applies the theme attribute on every parameter update, so a changed <see cref="Theme"/> reaches the markup and
    /// the dictionary a parent supplied is copied instead of modified. In Development it also warns when a captured
    /// attribute resembles one of the component's parameters, the usual sign of a misspelled parameter name.
    /// </remarks>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        WarnAboutAttributesThatResembleParameters();
        ApplyThemeAttribute();

        return base.SetParametersAsync(ParameterView.Empty);
    }

    private void ApplyThemeAttribute()
    {
        if (!ReferenceEquals(Attributes, _ownedAttributes))
        {
            // A dictionary supplied by the parent belongs to the parent: work on a copy.
            _ownedAttributes = new Dictionary<string, object>(Attributes ?? new Dictionary<string, object>());
            Attributes = _ownedAttributes;
            _themeAttributeName = null;
        }

        var attributeName = Framework == CssFramework.Bootstrap ? "data-bs-theme" : "data-theme";
        if (_themeAttributeName is not null && _themeAttributeName != attributeName)
        {
            _ownedAttributes.Remove(_themeAttributeName);
            _themeAttributeName = null;
        }

        if (Theme == ThemeMode.System)
        {
            if (_themeAttributeName is not null)
            {
                _ownedAttributes.Remove(_themeAttributeName);
                _themeAttributeName = null;
            }

            return;
        }

        _ownedAttributes[attributeName] = Theme.ToString().ToLowerInvariant();
        _themeAttributeName = attributeName;
    }

    /// <summary>
    /// Initializes the component and determines its interactivity mode.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        IsInteractive = RendererInfo?.IsInteractive ?? false;
    }

    /// <summary>Creates a render fragment that loads a client-side script.</summary>
    protected RenderFragment LoadScript(string src) => builder =>
    {
        builder.OpenComponent<JS>(0);
        builder.AddAttribute(1, "Src", src);
        builder.CloseComponent();
    };
}
