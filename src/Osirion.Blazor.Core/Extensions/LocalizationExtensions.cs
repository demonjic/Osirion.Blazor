using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Core.Extensions;

/// <summary>
/// Serves a site in several languages, each at its own addresses: <c>/contact</c> in the default culture and
/// <c>/sr/contact</c> in Serbian. The culture comes from the first path segment alone; the path is not rewritten, so
/// pages declare their localized routes (<c>@page "/contact"</c> and <c>@page "/sr/contact"</c>).
/// </summary>
public static class LocalizationExtensions
{
    /// <summary>
    /// Reads the site's cultures from the <c>Osirion:Localization</c> section of <paramref name="configuration"/> and
    /// registers <see cref="IOsirionLocalePaths"/>. The options are checked when the application starts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration (the root, not the section).</param>
    /// <returns>The same <paramref name="services"/>, for chaining.</returns>
    public static IServiceCollection AddOsirionLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        AddValidatedOptions(services).Bind(configuration.GetSection(OsirionLocalizationOptions.Section));
        return services;
    }

    /// <summary>
    /// Sets the site's cultures in code and registers <see cref="IOsirionLocalePaths"/>. The options are checked when
    /// the application starts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Sets the localization options.</param>
    /// <returns>The same <paramref name="services"/>, for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOsirionLocalization(localization =&gt;
    /// {
    ///     localization.DefaultCulture = "en";
    ///     localization.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddOsirionLocalization(this IServiceCollection services, Action<OsirionLocalizationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        AddValidatedOptions(services).Configure(configure);
        return services;
    }

    /// <summary>
    /// Sets the culture of each request from the first segment of its path, through ASP.NET Core's request
    /// localization with <see cref="OsirionPathRequestCultureProvider"/> as its only provider, and writes it to the
    /// <c>Content-Language</c> response header. GET and HEAD requests for a prefix in another letter case
    /// (<c>/SR/contact</c>) or for the default culture's prefix (<c>/en/contact</c>) are first redirected (301) to the
    /// canonical address (<c>/sr/contact</c>, <c>/contact</c>), keeping the path base and the query string.
    /// </summary>
    /// <remarks>
    /// Call it after <c>UseExceptionHandler</c> and <c>UseStatusCodePagesWithReExecute</c>, so the requests they
    /// re-execute pass through it and an error page speaks the language of the address that was asked for, and before
    /// <c>MapRazorComponents</c> and anything else that reads the culture. No cookie and no <c>Accept-Language</c> header
    /// is read, so one address always answers with the same page, for caches and crawlers alike. Needs
    /// <see cref="AddOsirionLocalization(IServiceCollection, IConfiguration)"/>.
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/>, for chaining.</returns>
    /// <exception cref="InvalidOperationException">The localization services are not registered.</exception>
    /// <exception cref="OptionsValidationException">The localization options are not valid.</exception>
    /// <example>
    /// <code>
    /// app.UseExceptionHandler("/error");
    /// app.UseStatusCodePagesWithReExecute("/not-found");
    /// app.UseOsirionLocalization();
    /// app.UseAntiforgery();
    /// app.MapRazorComponents&lt;App&gt;();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseOsirionLocalization(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var localePaths = app.ApplicationServices.GetService<IOsirionLocalePaths>()
            ?? throw new InvalidOperationException(
                "UseOsirionLocalization needs the localization services: call services.AddOsirionLocalization(configuration) or AddOsirion(configuration) with an Osirion:Localization section.");

        var cultures = localePaths.Cultures.Select(culture => culture.Culture).ToList();
        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(localePaths.DefaultCulture.Culture),
            SupportedCultures = cultures,
            SupportedUICultures = cultures,
            ApplyCurrentCultureToResponseHeaders = true,
            RequestCultureProviders = [new OsirionPathRequestCultureProvider(localePaths)],
        };

        app.UseMiddleware<LocalePrefixRedirectMiddleware>(localePaths);
        return app.UseRequestLocalization(options);
    }

    private static OptionsBuilder<OsirionLocalizationOptions> AddValidatedOptions(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<OsirionLocalizationOptions>, OsirionLocalizationOptionsValidator>());
        services.TryAddSingleton<IOsirionLocalePaths, OsirionLocalePaths>();
        return services.AddOptions<OsirionLocalizationOptions>().ValidateOnStart();
    }
}
