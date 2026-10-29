using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

/// <summary><see cref="LocalizationCulture.Aliases"/> and <see cref="IOsirionLocalePaths.CurrentCulture"/>.</summary>
public sealed class LocaleAliasesAndCurrentCultureTests : IDisposable
{
    private readonly CultureInfo _uiCulture = CultureInfo.CurrentUICulture;
    private bool _nextCalled;

    public void Dispose() => CultureInfo.CurrentUICulture = _uiCulture;

    [Theory]
    [InlineData("/sr_latn/kontakt", "/sr-latn/kontakt")]
    [InlineData("/SR-LATN-RS/kontakt", "/sr-latn/kontakt")]
    [InlineData("/sr-latn-rs", "/sr-latn")]
    [InlineData("/english/contact", "/contact")]
    public async Task Get_AliasPrefix_RedirectsPermanentlyToTheCanonicalPrefix(string path, string location)
    {
        var context = await SendAsync(path);

        _nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task Head_AliasPrefix_RedirectsWithThePathBaseAndQuery()
    {
        var context = await SendAsync("/sr_latn/kontakt", "HEAD", "?a=1", "/site");

        context.Response.Headers.Location.ToString().ShouldBe("/site/sr-latn/kontakt?a=1");
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task OtherMethods_UnderAnAlias_AreNotRedirected(string method)
    {
        var context = await SendAsync("/sr_latn/kontakt", method);

        _nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData("/sr_latn//example.com", "/sr-latn/example.com")]
    [InlineData("/sr_latn/\\example.com", "/sr-latn/example.com")]
    public async Task Redirect_FromAnAlias_NeverLeavesTheSite(string path, string location)
    {
        var context = await SendAsync(path);

        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Theory]
    [InlineData("/sr-latn/kontakt")]
    [InlineData("/sr/kontakt")]
    public async Task CanonicalPrefix_IsNotRedirected(string path)
    {
        await SendAsync(path);

        _nextCalled.ShouldBeTrue();
    }

    [Fact]
    public void GetErrors_AliasWithAnInvalidCharacter_NamesTheKey()
    {
        var options = Options();
        options.Cultures[2].Aliases.Add("sr latn");

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:2:Aliases:2 'sr latn' must be letters, digits, hyphens and underscores, for example 'sr_latn'."]);
    }

    [Fact]
    public void GetErrors_AliasThatIsAPrefix_NamesTheKey()
    {
        var options = Options();
        options.Cultures[2].Aliases.Add("SR");

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:2:Aliases:2 'SR' is the prefix or an alias of another culture."]);
    }

    [Fact]
    public void GetErrors_AliasOfTwoCultures_NamesTheSecond()
    {
        var options = Options();
        options.Cultures[1].Aliases.Add("SR_LATN");

        options.GetErrors().ShouldBe(["Osirion:Localization:Cultures:2:Aliases:1 'sr_latn' is the prefix or an alias of another culture."]);
    }

    [Theory]
    [InlineData("sr-Latn-RS", "sr-Latn", "sr-latn")]
    [InlineData("sr-Latn", "sr-Latn", "sr-latn")]
    [InlineData("sr-Cyrl-RS", "sr", "sr")]
    [InlineData("fr-FR", "en", "en")]
    public void CurrentCulture_MatchesTheRequestCultureThroughItsParents(string requestCulture, string culture, string prefix)
    {
        var paths = Create();
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(requestCulture);

        paths.CurrentCulture.Culture.Name.ShouldBe(culture);
        paths.CurrentCulture.Prefix.ShouldBe(prefix);
    }

    [Fact]
    public void CurrentCulture_OfAnotherImplementation_UsesTheInterfaceDefault()
    {
        IOsirionLocalePaths paths = new OnlyCultures(Create());
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sr-Latn-RS");

        paths.CurrentCulture.Culture.Name.ShouldBe("sr-Latn");
    }

    private static OsirionLocalizationOptions Options()
    {
        var options = new OsirionLocalizationOptions { DefaultCulture = "en" };
        options.Cultures.Add(new LocalizationCulture { Name = "en", Aliases = ["english"] });
        options.Cultures.Add(new LocalizationCulture { Name = "sr" });
        options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Aliases = ["sr-Latn-RS", "sr_latn"] });
        return options;
    }

    private static IOsirionLocalePaths Create() =>
        Services().BuildServiceProvider().GetRequiredService<IOsirionLocalePaths>();

    private static IServiceCollection Services() => new ServiceCollection().AddLogging().AddOsirionLocalization(options =>
    {
        var configured = Options();
        options.DefaultCulture = configured.DefaultCulture;
        options.Cultures.AddRange(configured.Cultures);
    });

    private async Task<HttpContext> SendAsync(string path, string method = "GET", string query = "", string pathBase = "")
    {
        var services = Services().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        app.UseOsirionLocalization();
        app.Run(_ =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.PathBase = pathBase;
        context.Request.QueryString = new QueryString(query);
        await app.Build()(context);
        return context;
    }

    // Implements only the members without a default, as a site's own implementation would.
    private sealed class OnlyCultures(IOsirionLocalePaths inner) : IOsirionLocalePaths
    {
        public IReadOnlyList<OsirionLocaleCulture> Cultures => inner.Cultures;

        public OsirionLocaleCulture DefaultCulture => inner.DefaultCulture;

        public bool TryGetCulture(string? path, [NotNullWhen(true)] out OsirionLocaleCulture? culture, out string remainder) =>
            inner.TryGetCulture(path, out culture, out remainder);

        public string Localize(string sitePath, CultureInfo culture) => inner.Localize(sitePath, culture);

        public string WithoutLocale(string sitePath) => inner.WithoutLocale(sitePath);

        public string LocalizeForCurrentCulture(string sitePath) => inner.LocalizeForCurrentCulture(sitePath);
    }
}
