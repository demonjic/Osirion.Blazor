using Microsoft.Extensions.Configuration;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Extensions;

/// <summary>
/// Registers the site's language versions, whose addresses carry the culture in their first path segment. Pair it with
/// <c>app.UseOsirionLocalization()</c>.
/// </summary>
public static class OsirionLocalizationBuilderExtensions
{
    /// <summary>
    /// Sets the site's cultures in code. The options are checked when the application starts.
    /// </summary>
    /// <param name="builder">The Osirion builder.</param>
    /// <param name="configure">Sets the localization options.</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    public static IOsirionBuilder UseLocalization(this IOsirionBuilder builder, Action<OsirionLocalizationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOsirionLocalization(configure);
        return builder;
    }

    /// <summary>
    /// Reads the site's cultures from the <c>Osirion:Localization</c> section of <paramref name="configuration"/>. The
    /// options are checked when the application starts.
    /// </summary>
    /// <param name="builder">The Osirion builder.</param>
    /// <param name="configuration">The application configuration (the root, not the section).</param>
    /// <returns>The same <paramref name="builder"/>, for chaining.</returns>
    public static IOsirionBuilder UseLocalization(this IOsirionBuilder builder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOsirionLocalization(configuration);
        return builder;
    }
}
