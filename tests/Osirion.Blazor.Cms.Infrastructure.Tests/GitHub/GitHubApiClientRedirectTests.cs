using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.GitHub;
using Osirion.Blazor.Cms.Infrastructure.Tests.TestDoubles;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.GitHub;

/// <summary>
/// A configured repository that was renamed or transferred answers with a redirect. These tests prove the client
/// follows it with its token on the same host only, warns that the settings are out of date, and stops a loop.
/// </summary>
public class GitHubApiClientRedirectTests
{
    private const string Token = "redirect-test-token";
    private const string Owner = "old-owner";
    private const string NewLocation = "https://api.github.com/repositories/42/contents/a.md?ref=main";

    private readonly ConcurrentQueue<string?> _authorizations = new();
    private readonly RecordingLogger<GitHubApiClient> _logger = new();

    // A unique name per test, because the client warns once per configured repository for the whole process
    private readonly string _repository = "repo-" + Guid.NewGuid().ToString("N");

    // Fails before the fix: the redirect reached the client as an error, so the file was never read.
    [Theory]
    [InlineData(HttpStatusCode.MovedPermanently)]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.TemporaryRedirect)]
    [InlineData(HttpStatusCode.PermanentRedirect)]
    public async Task SameHostRedirect_IsFollowed_WithTheTokenOnTheNextRequest(HttpStatusCode status)
    {
        var handler = CreateHandler(request => IsOriginalRequest(request)
            ? Redirect(status, NewLocation)
            : FileResponse());
        var client = CreateClient(handler);

        var file = await client.GetFileContentAsync("a.md");

        file.ShouldNotBeNull().Path.ShouldBe("a.md");
        handler.Requests.Count.ShouldBe(2);
        handler.Requests[1].Uri.ShouldBe(new Uri(NewLocation));
        _authorizations.ShouldAllBe(authorization => authorization == "token " + Token);
    }

    // Fails before the fix: a relative Location was never followed.
    [Fact]
    public async Task RelativeLocation_IsResolvedAgainstTheApiHost()
    {
        var handler = CreateHandler(request => IsOriginalRequest(request)
            ? Redirect(HttpStatusCode.MovedPermanently, "/repositories/42/contents/a.md?ref=main")
            : FileResponse());
        var client = CreateClient(handler);

        await client.GetFileContentAsync("a.md");

        handler.Requests.Select(request => request.Uri).ShouldBe([handler.Requests[0].Uri, new Uri(NewLocation)]);
        _authorizations.ShouldAllBe(authorization => authorization == "token " + Token);
    }

    // Fails before the fix: the body of a write was not sent again to the new location.
    [Fact]
    public async Task RedirectedWrite_IsRepeatedWithTheSameMethodAndBody()
    {
        var handler = CreateHandler(request => IsOriginalRequest(request)
            ? Redirect(HttpStatusCode.TemporaryRedirect, "https://api.github.com/repositories/42/contents/a.md")
            : ScriptedHttpMessageHandler.Json("""{"content":{"path":"a.md","sha":"new"},"commit":{"sha":"c1"}}"""));
        var client = CreateClient(handler);

        var result = await client.CreateOrUpdateFileAsync("a.md", "hello", "Update a.md");

        result.ShouldNotBeNull();
        handler.Requests.Count.ShouldBe(2);
        handler.Requests.ShouldAllBe(request => request.Method == HttpMethod.Put);
        handler.Requests[1].Body.ShouldNotBeNull().ShouldBe(handler.Requests[0].Body);
        _authorizations.ShouldAllBe(authorization => authorization == "token " + Token);
    }

    // Fails before the fix: nothing was logged, so a moved repository looked like a plain rate limit.
    [Fact]
    public async Task Redirect_LogsOneWarning_NamingTheConfiguredRepositoryAndTheNewLocation()
    {
        var handler = CreateHandler(request => request.RequestUri!.AbsolutePath.StartsWith("/repos/", StringComparison.Ordinal)
            ? Redirect(HttpStatusCode.MovedPermanently, NewLocation)
            : FileResponse());
        var client = CreateClient(handler);

        await client.GetFileContentAsync("a.md");
        await client.GetFileContentAsync("a.md");

        var warning = _logger.Entries.Where(entry => entry.Level == LogLevel.Warning).ShouldHaveSingleItem();
        warning.Message.ShouldContain($"{Owner}/{_repository}");
        warning.Message.ShouldContain(NewLocation);
        warning.Message.ShouldContain("Owner");
        warning.Message.ShouldContain("Repository");
        _logger.Entries.ShouldAllBe(entry => !entry.Message.Contains(Token));
    }

    // Fails before the fix: the refused redirect was not explained in the log. The token never reaches another
    // host or a downgraded scheme, because such a redirect is not followed at all.
    [Theory]
    [InlineData("https://elsewhere.example.com/repositories/42/contents/a.md")]
    [InlineData("http://api.github.com/repositories/42/contents/a.md")]
    [InlineData("https://api.github.com:8443/repositories/42/contents/a.md")]
    public async Task RedirectToAnotherHost_IsNotFollowed_AndIsLogged(string location)
    {
        var handler = CreateHandler(request => IsOriginalRequest(request)
            ? Redirect(HttpStatusCode.Found, location)
            : FileResponse());
        var client = CreateClient(handler);

        await Should.ThrowAsync<HttpRequestException>(() => client.GetFileContentAsync("a.md"));

        handler.Requests.ShouldHaveSingleItem().Uri.Host.ShouldBe("api.github.com");
        _logger.Entries.ShouldContain(entry => entry.Level == LogLevel.Warning
            && entry.Message.Contains(location)
            && entry.Message.Contains("not followed"));
        _logger.Entries.ShouldAllBe(entry => !entry.Message.Contains(Token));
    }

    // Fails before the fix: the first redirect ended the request, so the limit was never reached.
    [Fact]
    public async Task RedirectLoop_StopsAfterTheLimit()
    {
        var hop = 0;
        var handler = CreateHandler(_ => Redirect(
            HttpStatusCode.MovedPermanently,
            $"https://api.github.com/repositories/42/loop/{Interlocked.Increment(ref hop)}"));
        var client = CreateClient(handler);

        await Should.ThrowAsync<HttpRequestException>(() => client.GetFileContentAsync("a.md"));

        // The first request and five redirects
        handler.Requests.Count.ShouldBe(6);
        _logger.Entries.ShouldContain(entry => entry.Level == LogLevel.Warning && entry.Message.Contains("redirects"));
    }

    // Passes before the fix as well (HttpClient honors the token while it buffers the response); it guards that
    // the redirect loop never sends another request once the caller has cancelled.
    [Fact]
    public async Task Redirect_AfterCancellation_IsNotFollowed()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = CreateHandler(_ =>
        {
            cancellation.Cancel();
            return Redirect(HttpStatusCode.MovedPermanently, NewLocation);
        });
        var client = CreateClient(handler);

        await Should.ThrowAsync<OperationCanceledException>(() => client.GetFileContentAsync("a.md", cancellation.Token));

        handler.Requests.Count.ShouldBe(1);
    }

    private ScriptedHttpMessageHandler CreateHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        return new ScriptedHttpMessageHandler(request =>
        {
            _authorizations.Enqueue(request.Headers.Authorization?.ToString());
            return respond(request);
        });
    }

    private GitHubApiClient CreateClient(HttpMessageHandler handler)
    {
        var options = Options.Create(new GitHubOptions { Owner = Owner, Repository = _repository, Branch = "main", ApiToken = Token });
        return new GitHubApiClient(new HttpClient(handler), options, _logger);
    }

    private bool IsOriginalRequest(HttpRequestMessage request)
    {
        return request.RequestUri!.AbsolutePath.StartsWith($"/repos/{Owner}/{_repository}/", StringComparison.Ordinal);
    }

    private static HttpResponseMessage Redirect(HttpStatusCode status, string location)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
        return response;
    }

    private static HttpResponseMessage FileResponse()
    {
        return ScriptedHttpMessageHandler.Json("""{"name":"a.md","path":"a.md","sha":"s1","content":"aGVsbG8=","encoding":"base64"}""");
    }
}
