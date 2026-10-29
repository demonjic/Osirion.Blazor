using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;

namespace Osirion.Blazor.Core.Tests.Localization;

/// <summary>Builds the locale paths of a site in English (default, at the root), Serbian (<c>/sr</c>) and German.</summary>
internal static class LocalePaths
{
    public static IServiceCollection AddSiteLocalization(this IServiceCollection services) =>
        services.AddOsirionLocalization(Configure);

    public static IOsirionLocalePaths Create() =>
        new ServiceCollection().AddSiteLocalization().BuildServiceProvider().GetRequiredService<IOsirionLocalePaths>();

    public static void Configure(OsirionLocalizationOptions options)
    {
        options.DefaultCulture = "en";
        options.Cultures.Add(new LocalizationCulture { Name = "en", DisplayName = "English" });
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "sr" });
        options.Cultures.Add(new LocalizationCulture { Name = "de" });
    }
}
