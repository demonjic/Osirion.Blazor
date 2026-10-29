using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Internal;
using Osirion.Blazor.Options;

namespace Osirion.Blazor.Extensions;

/// <summary>
/// Registers the site that <see cref="Components.OsirionSiteLayout"/> renders.
/// </summary>
public static class OsirionSiteBuilderExtensions
{
    /// <summary>
    /// Sets the site's name, menu and footer in code. The options are checked when the application starts.
    /// </summary>
    /// <param name="builder">The Osirion builder.</param>
    /// <param name="configure">Sets the site options.</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddOsirion(osirion =&gt; osirion.UseSite(site =&gt;
    /// {
    ///     site.Name = "My blog";
    ///     site.Menu.Add(new SiteMenuItem { Text = "Blog", Href = "/blog" });
    /// }));
    /// </code>
    /// </example>
    public static IOsirionBuilder UseSite(this IOsirionBuilder builder, Action<OsirionSiteOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        AddValidatedOptions(builder.Services).Configure(configure);
        return builder;
    }

    /// <summary>
    /// Reads the site's name, menu and footer from the <c>Osirion:Site</c> section of
    /// <paramref name="configuration"/>. The options are checked when the application starts.
    /// </summary>
    /// <param name="builder">The Osirion builder.</param>
    /// <param name="configuration">The application configuration (the root, not the section).</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    public static IOsirionBuilder UseSite(this IOsirionBuilder builder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        AddValidatedOptions(builder.Services).Bind(configuration.GetSection(OsirionSiteOptions.Section));
        return builder;
    }

    private static OptionsBuilder<OsirionSiteOptions> AddValidatedOptions(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<OsirionSiteOptions>, OsirionSiteOptionsValidator>());

        // Derived versions (Locales entries with DerivedFrom) are filled in before the validators run
        services.AddOsirionTransliteration();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<OsirionSiteOptions>, DerivedSiteLocales>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<OsirionSiteOptions>, DerivedSiteLocalesValidator>());
        return services.AddOptions<OsirionSiteOptions>().ValidateOnStart();
    }
}
