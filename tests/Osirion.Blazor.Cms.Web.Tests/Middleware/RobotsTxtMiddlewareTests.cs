using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Middleware;
using Shouldly;

namespace Osirion.Blazor.Cms.Web.Tests.Middleware;

public class RobotsTxtMiddlewareTests
{
    private const string DefaultRobots = "User-agent: *  \r\nDisallow: /search/ ";

    private readonly IContentProviderManager _manager = Substitute.For<IContentProviderManager>();
    private int _nextCalls;

    public RobotsTxtMiddlewareTests()
    {
        _manager.GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([]));
    }

    [Fact]
    public async Task RobotsTxtMiddleware_RobotsContentExists_WritesItAsPlainText()
    {
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Url == "robots"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>(
                [ContentItem.Create("robots", "Robots", "User-agent: *\nAllow: /", "robots.md", "test")]));
        var context = Request("/robots.txt");

        await CreateMiddleware().InvokeAsync(context);

        context.Response.ContentType.ShouldBe("text/plain");
        ReadBody(context).ShouldBe("User-agent: *\nAllow: /");
        _nextCalls.ShouldBe(0);
    }

    // A robots.txt in the content folder is a raw file, which page queries leave out; the generator asks for it.
    [Fact]
    public async Task RobotsTxtMiddleware_AsksForRawFiles()
    {
        var robots = ContentItem.Create("robots", "Robots", "User-agent: *", "robots.txt", "test");
        robots.SetOriginalMarkdown("User-agent: *\nDisallow: /drafts/");
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Url == "robots" && q.IncludeRawFiles), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([robots]));
        var context = Request("/robots.txt");

        await CreateMiddleware().InvokeAsync(context);

        ReadBody(context).ShouldBe("User-agent: *\nDisallow: /drafts/");
    }

    [Fact]
    public async Task RobotsTxtMiddleware_RobotsKeptAsMarkdown_WritesTheRawTextNotTheRenderedHtml()
    {
        // Fails before the fix: the middleware wrote ContentItem.Content, which is the sanitized HTML
        // (<p>User-agent: *</p>), under a text/plain content type.
        var robots = ContentItem.Create("robots", "Robots", "<p>User-agent: *<br>Disallow: /private/</p>", "robots.md", "test");
        robots.SetOriginalMarkdown("User-agent: *\nDisallow: /private/\nSitemap: https://example.com/sitemap.xml");
        _manager.GetContentByQueryAsync(Arg.Is<ContentQuery>(q => q.Url == "robots"), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ContentItem>>([robots]));
        var context = Request("/robots.txt");

        await CreateMiddleware().InvokeAsync(context);

        context.Response.ContentType.ShouldBe("text/plain");
        var body = ReadBody(context);
        body.ShouldStartWith("User-agent: *");
        body.ShouldNotContain("<");
        body.ShouldBe("User-agent: *\nDisallow: /private/\nSitemap: https://example.com/sitemap.xml");
    }

    [Fact]
    public async Task RobotsTxtMiddleware_NoRobotsContent_WritesTheDefaultRules()
    {
        var context = Request("/robots.txt");

        await CreateMiddleware().InvokeAsync(context);

        context.Response.ContentType.ShouldBe("text/plain");
        ReadBody(context).ShouldBe(DefaultRobots);
    }

    [Fact]
    public async Task RobotsTxtMiddleware_OtherPath_PassesTheRequestOnWithoutQuerying()
    {
        var context = Request("/blog/robots");

        await CreateMiddleware().InvokeAsync(context);

        _nextCalls.ShouldBe(1);
        ReadBody(context).ShouldBeEmpty();
        await _manager.DidNotReceive().GetContentByQueryAsync(Arg.Any<ContentQuery>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UseOsirionRobotsGenerator_Pipeline_AnswersRobotsTxtAndPassesOtherRequestsOn()
    {
        var services = new ServiceCollection().AddSingleton(_manager).BuildServiceProvider();
        var app = new ApplicationBuilder(services);

        app.UseOsirionRobotsGenerator().ShouldBeSameAs(app);
        app.Run(context =>
        {
            _nextCalls++;
            return Task.CompletedTask;
        });
        var pipeline = app.Build();
        var robots = Request("/robots.txt");
        var other = Request("/");

        await pipeline(robots);
        await pipeline(other);

        ReadBody(robots).ShouldBe(DefaultRobots);
        _nextCalls.ShouldBe(1);
    }

    private RobotsTxtMiddleware CreateMiddleware() =>
        new(_ =>
        {
            _nextCalls++;
            return Task.CompletedTask;
        }, _manager);

    private static DefaultHttpContext Request(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return reader.ReadToEnd();
    }
}
