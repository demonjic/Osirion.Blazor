using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Redirects;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Redirects;

public class ContentRedirectMiddlewareTests
{
    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();

    public ContentRedirectMiddlewareTests()
    {
        var moved = ContentItem.Create("tabs", "Tabs", "<p>Body</p>", "components/tabs.md", "test");
        moved.Url = "components/tabs";
        moved.Metadata = new FrontMatter { Title = "Tabs", RedirectFrom = ["/docs/components/core/content/tabs", "docs/old-tabs/"] };
        var live = ContentItem.Create("live", "Live", "<p>Body</p>", "docs/live.md", "test");
        live.Url = "docs/live";
        live.Metadata = new FrontMatter { Title = "Live" };
        var shadowing = ContentItem.Create("other", "Other", "<p>Body</p>", "components/other.md", "test");
        shadowing.Url = "components/other";
        shadowing.Metadata = new FrontMatter { Title = "Other", RedirectFrom = ["/docs/live"] };

        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContentItem> { moved, live, shadowing });
    }

    // Fails before the fix: the fragment was escaped into the path (Location: /platform%23predict).
    [Theory]
    [InlineData("/platform#predict", "", "/platform#predict")]
    [InlineData("/platform#predict", "?utm=mail", "/platform?utm=mail#predict")]
    [InlineData("/platform?tab=2#predict", "?utm=mail", "/platform?tab=2#predict")]
    [InlineData("/platform?tab=2", "", "/platform?tab=2")]
    [InlineData("/plat form#a", "", "/plat%20form#a")]
    public async Task FixedRedirect_TargetWithQueryOrFragment_KeepsThemAsWritten(string target, string query, string location)
    {
        var options = new OsirionRedirectOptions();
        options.Redirects["/modules/vera"] = target;

        var (context, _) = await InvokeAsync(options, "/modules/vera", query);

        context.Response.Headers.Location.ToString().ShouldBe(location);
    }

    [Fact]
    public async Task RedirectFrom_OldUrl_RedirectsPermanentlyToTheCurrentUrlKeepingTheQuery()
    {
        var (context, nextCalled) = await InvokeAsync(new OsirionRedirectOptions(), "/docs/components/core/content/tabs", "?tab=2");

        nextCalled.ShouldBeFalse();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.ToString().ShouldBe("/components/tabs?tab=2");
    }

    [Theory]
    [InlineData("/docs/old-tabs")]
    [InlineData("/DOCS/Old-Tabs/")]
    public async Task RedirectFrom_PathsAreMatchedIgnoringCaseAndTrailingSlash(string path)
    {
        var (context, _) = await InvokeAsync(new OsirionRedirectOptions(), path);

        context.Response.Headers.Location.ToString().ShouldBe("/components/tabs");
    }

    [Fact]
    public async Task RedirectFrom_AddressOfAPublishedPage_IsNotRedirected()
    {
        var (context, nextCalled) = await InvokeAsync(new OsirionRedirectOptions(), "/docs/live");

        nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Fact]
    public async Task FixedRedirect_ToAbsoluteUrl_IsUsedAsGiven()
    {
        var options = new OsirionRedirectOptions();
        options.Redirects["/docs/components/core"] = "https://example.com/components";

        var (context, _) = await InvokeAsync(options, "/docs/components/core", "?x=1");

        context.Response.Headers.Location.ToString().ShouldBe("https://example.com/components");
    }

    [Fact]
    public async Task Temporary_AnswersWith302()
    {
        var (context, _) = await InvokeAsync(new OsirionRedirectOptions { Permanent = false }, "/docs/old-tabs");

        context.Response.StatusCode.ShouldBe(StatusCodes.Status302Found);
    }

    [Fact]
    public async Task PathBase_IsKeptInTheLocation()
    {
        var (context, _) = await InvokeAsync(new OsirionRedirectOptions(), "/docs/old-tabs", pathBase: "/site");

        context.Response.Headers.Location.ToString().ShouldBe("/site/components/tabs");
    }

    [Theory]
    [InlineData("POST", "/docs/old-tabs")]
    [InlineData("GET", "/unknown")]
    [InlineData("GET", "/")]
    public async Task OtherRequests_PassThrough(string method, string path)
    {
        var (context, nextCalled) = await InvokeAsync(new OsirionRedirectOptions(), path, method: method);

        nextCalled.ShouldBeTrue();
        context.Response.Headers.Location.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Map_IsBuiltOnceWithinTheCacheDuration()
    {
        var middleware = CreateMiddleware(new OsirionRedirectOptions(), out _);

        await middleware.InvokeAsync(CreateContext("/docs/old-tabs"));
        await middleware.InvokeAsync(CreateContext("/docs/components/core/content/tabs"));

        await _manager.Received(1).GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ContentReadFails_FixedRedirectsStillWorkAndTheFailureIsNotCached()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("GitHub is down"));
        var options = new OsirionRedirectOptions();
        options.Redirects["/old"] = "/new";
        var middleware = CreateMiddleware(options, out _);

        var first = CreateContext("/old");
        await middleware.InvokeAsync(first);
        await middleware.InvokeAsync(CreateContext("/old"));

        first.Response.Headers.Location.ToString().ShouldBe("/new");
        await _manager.Received(2).GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("https://evil.example/old", "/new")]
    [InlineData("//evil.example", "/new")]
    [InlineData("/", "/new")]
    [InlineData("/old", "javascript:alert(1)")]
    [InlineData("/old", "//evil.example")]
    public void Validate_InvalidFixedRedirect_Throws(string source, string target)
    {
        var options = new OsirionRedirectOptions();
        options.Redirects[source] = target;

        Should.Throw<ArgumentException>(options.Validate);
    }

    private async Task<(HttpContext Context, bool NextCalled)> InvokeAsync(
        OsirionRedirectOptions options, string path, string query = "", string method = "GET", string pathBase = "")
    {
        var middleware = CreateMiddleware(options, out var nextCalled);
        var context = CreateContext(path, query, method, pathBase);
        await middleware.InvokeAsync(context);
        return (context, nextCalled());
    }

    private ContentRedirectMiddleware CreateMiddleware(OsirionRedirectOptions options, out Func<bool> nextCalled)
    {
        var called = false;
        nextCalled = () => called;
        return new ContentRedirectMiddleware(_ => { called = true; return Task.CompletedTask; }, options);
    }

    private DefaultHttpContext CreateContext(string path, string query = "", string method = "GET", string pathBase = "")
    {
        var services = new ServiceCollection();
        services.AddSingleton(_manager);
        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.PathBase = pathBase;
        context.Request.QueryString = new QueryString(query);
        return context;
    }
}
