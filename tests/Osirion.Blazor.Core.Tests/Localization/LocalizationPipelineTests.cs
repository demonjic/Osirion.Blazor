using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Core.Extensions;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

/// <summary>Runs requests through the pipeline <c>UseOsirionLocalization</c> builds.</summary>
public class LocalizationPipelineTests
{
    private bool _nextCalled;
    private CultureInfo? _seenCulture;
    private CultureInfo? _seenUiCulture;

    [Theory]
    [InlineData("/sr/contact", "sr-Latn", "sr-Latn")]
    [InlineData("/de", "de", "de")]
    [InlineData("/contact", "en", "en")]
    public async Task Request_GetsTheCultureOfItsPrefixAndItsContentLanguage(string path, string culture, string contentLanguage)
    {
        var context = await SendAsync(path);

        _nextCalled.ShouldBeTrue();
        _seenUiCulture!.Name.ShouldBe(culture);
        _seenCulture!.Name.ShouldBe(culture);
        context.Response.Headers.ContentLanguage.ToString().ShouldBe(contentLanguage);
    }

    [Theory]
    [InlineData("/SR/contact", "/sr/contact")]
    [InlineData("/Sr", "/sr")]
    [InlineData("/DE/", "/de")]
    [InlineData("/en/contact", "/contact")]
    [InlineData("/en", "/")]
    [InlineData("/EN/contact", "/contact")]
    public async Task Get_NonCanonicalPrefix_RedirectsPermanently(string path, string location)
    {
        var context = await SendAsync(path);

        _nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task Head_NonCanonicalPrefix_RedirectsPermanently()
    {
        var context = await SendAsync("/en/contact", method: "HEAD");

        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe("/contact");
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task OtherMethods_AreNotRedirected(string method)
    {
        var context = await SendAsync("/en/contact", method: method);

        _nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Redirect_KeepsThePathBaseAndTheQuery()
    {
        var context = await SendAsync("/SR/contact", query: "?tab=2&x=%20y", pathBase: "/site");

        context.Response.Headers.Location.ToString().ShouldBe("/site/sr/contact?tab=2&x=%20y");
    }

    [Theory]
    [InlineData("/en//example.com", "/example.com")]
    [InlineData("/en/\\example.com", "/example.com")]
    [InlineData("/en/\\/\\example.com/path", "/example.com/path")]
    [InlineData("/SR//example.com", "/sr/example.com")]
    public async Task Redirect_NeverLeavesTheSite(string path, string location)
    {
        var context = await SendAsync(path);

        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Theory]
    [InlineData("/sr/contact")]
    [InlineData("/sr")]
    [InlineData("/contact")]
    [InlineData("/")]
    public async Task CanonicalAddress_IsNotRedirected(string path)
    {
        var context = await SendAsync(path);

        _nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Fact]
    public void UseOsirionLocalization_WithoutTheServices_ExplainsWhatIsMissing()
    {
        var app = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());

        Should.Throw<InvalidOperationException>(() => app.UseOsirionLocalization())
            .Message.ShouldContain("AddOsirionLocalization");
    }

    private async Task<HttpContext> SendAsync(string path, string query = "", string method = "GET", string pathBase = "")
    {
        var services = new ServiceCollection().AddLogging().AddSiteLocalization().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        app.UseOsirionLocalization();
        app.Run(_ =>
        {
            _nextCalled = true;
            _seenCulture = CultureInfo.CurrentCulture;
            _seenUiCulture = CultureInfo.CurrentUICulture;
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
}
