using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Osirion.Blazor.Components;
using Osirion.Blazor.Theming.Services;

namespace Osirion.Blazor.Theming.Components;

/// <summary>Defines the ThemeToggle type.</summary>
public partial class ThemeToggle : IDisposable, IAsyncDisposable
{
    private const string EventsModulePath = "./_content/Osirion.Blazor.Theming/js/theme-toggle-events.js";

    private readonly string _subscriptionId = Guid.NewGuid().ToString("N");
    private DotNetObjectReference<ThemeChangeListener>? _listener;
    private IJSObjectReference? _eventsModule;

    [Inject] private IOptions<ThemingOptions> ThemingOptions { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the id of the toggle button. No id is written when it is <see langword="null"/>, so several toggles
    /// can share a page without repeating an id; set it only when a host stylesheet or script needs to address one.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets a callback raised with <c>light</c> or <c>dark</c> after the visitor switches the theme. It runs only
    /// when the toggle itself is interactive (Interactive Server, WebAssembly or Auto): under Static SSR the browser
    /// switches the theme and the server is not notified, so the callback is never raised there. Listen for the
    /// <c>osirion-theme-update</c> browser event on <c>window</c> to react in every render mode.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnThemeChanged { get; set; }

    /// <summary>
    /// Gets or sets what the button says while the light theme is on: its accessible name, its tooltip and its hidden
    /// text, in the language of the page. Defaults to <c>Switch to dark mode</c>.
    /// </summary>
    [Parameter]
    public string SwitchToDarkLabel { get; set; } = "Switch to dark mode";

    /// <summary>
    /// Gets or sets what the button says while the dark theme is on. Defaults to <c>Switch to light mode</c>.
    /// </summary>
    [Parameter]
    public string SwitchToLightLabel { get; set; } = "Switch to light mode";

    private string CurrentTheme => ThemeService.CurrentMode == ThemeMode.Dark ? "dark" : "light";

    // The button is named by what it does. Both labels are also written on the button, so the script can say the
    // other one after a switch without a round trip to the server.
    private string ActionLabel => CurrentTheme == "dark" ? SwitchToLightLabel : SwitchToDarkLabel;
    private string GetDefaultTheme() => ThemingOptions.Value.DefaultMode.ToString().ToLowerInvariant();
    private string GetFramework() => ThemingOptions.Value.Framework.ToString();

    /// <summary>Initializes the component state and required services.</summary>
    protected override void OnInitialized()
    {
        ThemeService.ThemeChanged += OnThemeChangedHandler;
        base.OnInitialized();
    }

    /// <summary>Subscribes to the browser's theme event when the callback can run.</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && IsInteractive && OnThemeChanged.HasDelegate)
        {
            _eventsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", EventsModulePath);
            _listener = DotNetObjectReference.Create(new ThemeChangeListener(this));
            await _eventsModule.InvokeVoidAsync("subscribe", _subscriptionId, _listener);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void OnThemeChangedHandler(object? sender, ThemeChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    /// <summary>Releases resources held by the component or service.</summary>
    public void Dispose()
    {
        ThemeService.ThemeChanged -= OnThemeChangedHandler;
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the browser subscription and the resources <see cref="Dispose"/> releases.</summary>
    public async ValueTask DisposeAsync()
    {
        Dispose();

        if (_eventsModule is not null)
        {
            try
            {
                await _eventsModule.InvokeVoidAsync("unsubscribe", _subscriptionId);
                await _eventsModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is gone, and the browser took its listener with it.
            }
        }

        _listener?.Dispose();
    }

    // Reached from the script through the DotNetObjectReference, so the component itself stays off the public surface.
    private sealed class ThemeChangeListener(ThemeToggle owner)
    {
        [JSInvokable]
        public Task NotifyThemeChanged(string theme) => owner.InvokeAsync(() => owner.OnThemeChanged.InvokeAsync(theme));
    }
}
