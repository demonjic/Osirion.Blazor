using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Web.Redirects;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Redirects;

/// <summary><see cref="OsirionRedirectOptions.ApplyUnderCulturePrefixes"/> on a site in English, Serbian and German.</summary>
public class CulturePrefixRedirectTests
{
    [Theory]
    [InlineData("/sr/pricing", "/sr/contact")]
    [InlineData("/SR/Pricing/", "/sr/contact")]
    [InlineData("/de/pricing", "/de/contact")]
    [InlineData("/pricing", "/contact")]
    public async Task FixedRedirect_UnderACulturePrefix_GoesToTheTargetUnderThatPrefix(string path, string location)
    {
        var (context, nextCalled) = await InvokeAsync(Options(applyUnderPrefixes: true), path);

        nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task FixedRedirect_WithAPrefixedSource_WinsOverTheOneWithout()
    {
        var options = Options(applyUnderPrefixes: true);
        options.Redirects["/sr/pricing"] = "/sr/cenovnik";

        var (context, _) = await InvokeAsync(options, "/sr/pricing");

        context.Response.Headers.Location.ToString().ShouldBe("/sr/cenovnik");
    }

    [Fact]
    public async Task FixedRedirect_ToAnAbsoluteUrl_IsUsedAsGivenUnderAPrefix()
    {
        var options = Options(applyUnderPrefixes: true);
        options.Redirects["/shop"] = "https://shop.example.org/";

        var (context, _) = await InvokeAsync(options, "/sr/shop");

        context.Response.Headers.Location.ToString().ShouldBe("https://shop.example.org/");
    }

    [Theory]
    [InlineData("/sr/modules/vera", "/sr/platform#predict")]
    [InlineData("/modules/vera", "/platform#predict")]
    public async Task FixedRedirect_TargetWithAFragment_KeepsItUnderThePrefix(string path, string location)
    {
        var options = Options(applyUnderPrefixes: true);
        options.Redirects["/modules/vera"] = "/platform#predict";

        var (context, _) = await InvokeAsync(options, path);

        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task FixedRedirect_UnderAPrefix_KeepsThePathBaseAndQuery()
    {
        var (context, _) = await InvokeAsync(Options(applyUnderPrefixes: true), "/sr/pricing", "?plan=2", "/site");

        context.Response.Headers.Location.ToString().ShouldBe("/site/sr/contact?plan=2");
    }

    // Guards the default: without the option a prefixed address is not redirected by an entry without a prefix.
    [Fact]
    public async Task FixedRedirect_OptionOff_DoesNotApplyUnderAPrefix()
    {
        var (context, nextCalled) = await InvokeAsync(Options(applyUnderPrefixes: false), "/sr/pricing");

        nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Fact]
    public async Task FixedRedirect_OptionOnWithoutLocalization_DoesNotApplyUnderAPrefix()
    {
        var (_, nextCalled) = await InvokeAsync(Options(applyUnderPrefixes: true), "/sr/pricing", withLocalization: false);

        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task FixedRedirect_PathThatIsOnlyAPrefix_IsNotRedirected()
    {
        var (_, nextCalled) = await InvokeAsync(Options(applyUnderPrefixes: true), "/sr");

        nextCalled.ShouldBeTrue();
    }

    private static OsirionRedirectOptions Options(bool applyUnderPrefixes)
    {
        var options = new OsirionRedirectOptions { ApplyUnderCulturePrefixes = applyUnderPrefixes };
        options.Redirects["/pricing"] = "/contact";
        return options;
    }

    private static async Task<(HttpContext Context, bool NextCalled)> InvokeAsync(
        OsirionRedirectOptions options, string path, string query = "", string pathBase = "", bool withLocalization = true)
    {
        var called = false;
        var middleware = new ContentRedirectMiddleware(_ => { called = true; return Task.CompletedTask; }, options);

        var services = new ServiceCollection();
        if (withLocalization)
        {
            services.AddOsirionLocalization(localization =>
            {
                localization.DefaultCulture = "en";
                localization.Cultures.Add(new LocalizationCulture { Name = "sr" });
                localization.Cultures.Add(new LocalizationCulture { Name = "de" });
            });
        }

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Method = "GET";
        context.Request.Path = path;
        context.Request.PathBase = pathBase;
        context.Request.QueryString = new QueryString(query);
        await middleware.InvokeAsync(context);
        return (context, called);
    }
}
