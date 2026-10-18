using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Interfaces;
using Osirion.Blazor.Cms.Domain.Models.GitHub;
using Osirion.Blazor.Cms.Domain.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Osirion.Blazor.Cms.Infrastructure.GitHub;

/// <summary>
/// GitHub API client implementation
/// </summary>
/// <remarks>
/// Owner, repository, branch and access token belong to this instance only. They are held in an immutable
/// snapshot that the Set* methods replace as a whole, and the Accept, User-Agent and Authorization headers
/// are set on each <see cref="HttpRequestMessage"/>. The <see cref="HttpClient.DefaultRequestHeaders"/> of
/// the underlying client are never modified, so two instances that share an <see cref="HttpClient"/> never
/// see each other's token, repository or branch.
/// </remarks>
public partial class GitHubApiClient : IGitHubApiClient
{
    /// <summary>
    /// The number of redirects one request follows before its last redirect response is returned
    /// </summary>
    internal const int MaxRedirects = 5;

    private const int MaxReportedRedirects = 1024;

    private const string DefaultApiUrl = "https://api.github.com";
    private const string GitHubMediaType = "application/vnd.github.v3+json";
    private const string UserAgentProduct = "OsirionBlogCMS";
    private const string UserAgentVersion = "2.0";
    private const string AuthorizationScheme = "token";
    private const string AcceptHeaderName = "Accept";
    private const string UserAgentHeaderName = "User-Agent";
    private const int BranchesPerPage = 100;
    private const int MaxBranchPages = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // The configured repositories whose redirect has been reported, so each is reported once per process
    private static readonly ConcurrentDictionary<string, byte> ReportedRedirects = new(StringComparer.OrdinalIgnoreCase);

    private readonly Func<HttpClient> _acquireHttpClient;
    private readonly ILogger<GitHubApiClient> _logger;
    private readonly Lock _settingsLock = new();
    private ClientSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubApiClient"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests</param>
    /// <param name="options">GitHub API configuration options</param>
    /// <param name="logger">The logger instance</param>
    /// <exception cref="ArgumentNullException">Thrown if required dependencies are null</exception>
    public GitHubApiClient(
    HttpClient httpClient,
    IOptions<GitHubOptions> options,
    ILogger<GitHubApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _acquireHttpClient = () => httpClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = ClientSettings.From(options.Value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubApiClient"/> class that takes a short-lived
    /// <see cref="HttpClient"/> from <paramref name="httpClientFactory"/> for every operation
    /// </summary>
    /// <param name="httpClientFactory">The factory that creates the named HTTP clients</param>
    /// <param name="httpClientName">The name of the HTTP client to create</param>
    /// <param name="options">GitHub API configuration options</param>
    /// <param name="logger">The logger instance</param>
    /// <exception cref="ArgumentNullException">Thrown if required dependencies are null</exception>
    internal GitHubApiClient(
        IHttpClientFactory httpClientFactory,
        string httpClientName,
        GitHubOptions options,
        ILogger<GitHubApiClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(httpClientName);
        ArgumentNullException.ThrowIfNull(options);

        _acquireHttpClient = () => httpClientFactory.CreateClient(httpClientName);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = ClientSettings.From(options);
    }

    private ClientSettings CurrentSettings => Volatile.Read(ref _settings);

    /// <summary>
    /// Gets a list of repositories the authenticated user has access to
    /// </summary>
    public async Task<List<GitHubRepository>?> GetRepositoriesAsync(CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            // Check if we have an access token
            if (string.IsNullOrWhiteSpace(settings.AccessToken))
            {
                throw new UnauthorizedAccessException("Access token is required to list repositories");
            }

            // Prepare the request: the authenticated user's repos, or the organization's repos when the owner is an organization
            string endpoint;

            // If the owner is set, try to get repositories for that organization/user
            if (!string.IsNullOrWhiteSpace(settings.Owner))
            {
                var owner = GitHubApiPath.EncodeSegment(settings.Owner, nameof(settings.Owner));
                endpoint = $"/users/{owner}/repos";

                // Check if the owner is an organization
                try
                {
                    using var orgResponse = await SendAsync(HttpMethod.Get, $"{settings.ApiUrl}/orgs/{owner}", settings, cancellationToken);
                    if (orgResponse.IsSuccessStatusCode)
                    {
                        endpoint = $"/orgs/{owner}/repos";
                    }
                }
                catch
                {
                    // If checking organization fails, default to user repos
                }
            }
            else
            {
                // Default to authenticated user's repos
                endpoint = "/user/repos";
            }

            // Make the API request
            var content = await GetResponseBodyAsync(HttpMethod.Get, $"{settings.ApiUrl}{endpoint}", settings, cancellationToken);
            var repositories = JsonSerializer.Deserialize<List<GitHubRepository>>(content, JsonOptions);

            return repositories ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub repositories");
            throw;
        }
    }

    /// <summary>
    /// Gets the branches for the current repository
    /// </summary>
    public async Task<List<GitHubBranch>?> GetBranchesAsync(CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            // GitHub returns 30 branches per page by default, so the configured branch can be on a later page
            var branches = new List<GitHubBranch>();
            for (var page = 1; page <= MaxBranchPages; page++)
            {
                var content = await GetResponseBodyAsync(
                    HttpMethod.Get,
                    $"{GetRepositoryUrl(settings)}/branches?per_page={BranchesPerPage}&page={page}",
                    settings,
                    cancellationToken);
                var pageItems = JsonSerializer.Deserialize<List<GitHubBranch>>(content, JsonOptions) ?? [];
                branches.AddRange(pageItems);

                if (pageItems.Count < BranchesPerPage)
                {
                    break;
                }
            }

            return branches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branches for repository {Owner}/{Repository}", settings.Owner, settings.Repository);
            throw;
        }
    }

    /// <summary>
    /// Gets the contents of the repository at the specified path
    /// </summary>
    /// <exception cref="ArgumentException">The path contains an empty, "." or ".." segment.</exception>
    public async Task<List<GitHubItem>?> GetRepositoryContentsAsync(string path, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var url = WithBranchReference(GetContentsUrl(settings, path), settings);
            var content = await GetResponseBodyAsync(HttpMethod.Get, url, settings, cancellationToken);

            // GitHub API can return either a single item or an array of items
            try
            {
                // Try to deserialize as array first
                var items = JsonSerializer.Deserialize<List<GitHubItem>>(content, JsonOptions);
                return items ?? [];
            }
            catch
            {
                // If that fails, try to deserialize as a single item
                var item = JsonSerializer.Deserialize<GitHubItem>(content, JsonOptions);
                return item is not null ? [item] : [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching repository contents at path {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Gets the content of a file in the repository
    /// </summary>
    /// <exception cref="ArgumentException">The path contains an empty, "." or ".." segment.</exception>
    public async Task<GitHubFileContent?> GetFileContentAsync(string path, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var url = WithBranchReference(GetContentsUrl(settings, path), settings);
            var content = await GetResponseBodyAsync(HttpMethod.Get, url, settings, cancellationToken);
            var fileContent = JsonSerializer.Deserialize<GitHubFileContent>(content, JsonOptions);

            if (fileContent is null)
            {
                _logger.LogError($"Failed to deserialize file content for {path}");
            }

            return fileContent ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching file content for {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Creates or updates a file in the repository
    /// </summary>
    /// <exception cref="ArgumentException">The path contains an empty, "." or ".." segment.</exception>
    public async Task<GitHubFileCommitResponse?> CreateOrUpdateFileAsync(string path, string content, string message, string? sha = null, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var url = GetContentsUrl(settings, path);

            // Convert content to base64
            var contentBytes = Encoding.UTF8.GetBytes(content);
            var base64Content = Convert.ToBase64String(contentBytes);

            // Create request body
            var requestBody = new
            {
                message,
                content = base64Content,
                branch = settings.Branch,
                sha // Include SHA only for updates
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var responseContent = await GetResponseBodyAsync(HttpMethod.Put, url, settings, cancellationToken, requestContent);
            var commitResponse = JsonSerializer.Deserialize<GitHubFileCommitResponse>(responseContent, JsonOptions);

            return commitResponse is null ? throw new Exception($"Failed to deserialize commit response for {path}") : commitResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating file {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Deletes a file from the repository
    /// </summary>
    /// <exception cref="ArgumentException">The path contains an empty, "." or ".." segment.</exception>
    /// <exception cref="InvalidOperationException">
    /// The owner, repository or access token is not set. No request is sent, so a token configured on the
    /// underlying <see cref="HttpClient"/> can never perform the delete.
    /// </exception>
    public async Task<GitHubFileCommitResponse> DeleteFileAsync(string path, string message, string sha, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                throw new InvalidOperationException("Repository owner, name and access token must be set before deleting a file.");
            }

            var url = GetContentsUrl(settings, path);

            // Create request body
            var requestBody = new
            {
                message,
                sha,
                branch = settings.Branch
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var responseContent = await GetResponseBodyAsync(HttpMethod.Delete, url, settings, cancellationToken, requestContent);
            var commitResponse = JsonSerializer.Deserialize<GitHubFileCommitResponse>(responseContent, JsonOptions);

            return commitResponse is null ? throw new Exception($"Failed to deserialize commit response for {path}") : commitResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Creates a new branch in the repository
    /// </summary>
    /// <exception cref="ArgumentException">A branch name is empty or contains an empty, "." or ".." segment.</exception>
    public async Task<GitHubBranch?> CreateBranchAsync(string branchName, string baseBranch, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            // Encode both names before the first request, so an invalid name never sends anything
            var repositoryUrl = GetRepositoryUrl(settings);
            var baseBranchUrl = $"{repositoryUrl}/git/refs/heads/{GitHubApiPath.EncodeBranchPath(baseBranch, nameof(baseBranch))}";
            var newBranchUrl = $"{repositoryUrl}/branches/{GitHubApiPath.EncodeBranchPath(branchName, nameof(branchName))}";

            // First, get the SHA of the latest commit on the base branch
            var refContent = await GetResponseBodyAsync(HttpMethod.Get, baseBranchUrl, settings, cancellationToken);
            var refData = JsonSerializer.Deserialize<GitHubRef>(refContent, JsonOptions);

            if (refData is null || refData.Object is null)
            {
                throw new Exception($"Failed to get reference for branch {baseBranch}");
            }

            // Create the new reference (branch)
            var requestBody = new
            {
                @ref = $"refs/heads/{branchName}",
                sha = refData.Object.Sha
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            await GetResponseBodyAsync(HttpMethod.Post, $"{repositoryUrl}/git/refs", settings, cancellationToken, requestContent);

            // Now get the details of the new branch
            var branchContent = await GetResponseBodyAsync(HttpMethod.Get, newBranchUrl, settings, cancellationToken);
            var branch = JsonSerializer.Deserialize<GitHubBranch>(branchContent, JsonOptions);

            return branch is null ? throw new Exception($"Failed to get details for new branch {branchName}") : branch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch {BranchName} from {BaseBranch}", branchName, baseBranch);
            throw;
        }
    }

    /// <summary>
    /// Creates a new pull request in the repository
    /// </summary>
    public async Task<GitHubPullRequest?> CreatePullRequestAsync(string title, string body, string head, string baseBranch, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var requestBody = new
            {
                title,
                body,
                head,
                @base = baseBranch
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var responseContent = await GetResponseBodyAsync(HttpMethod.Post, $"{GetRepositoryUrl(settings)}/pulls", settings, cancellationToken, requestContent);
            var pullRequest = JsonSerializer.Deserialize<GitHubPullRequest>(responseContent, JsonOptions);

            return pullRequest is null ? throw new Exception("Failed to deserialize pull request response") : pullRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pull request from {Head} to {BaseBranch}", head, baseBranch);
            throw;
        }
    }

    /// <summary>
    /// Searches for files in the repository
    /// </summary>
    public async Task<GitHubSearchResult?> SearchFilesAsync(string query, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            // Construct the search query
            var owner = GitHubApiPath.EncodeSegment(settings.Owner, nameof(settings.Owner));
            var repository = GitHubApiPath.EncodeSegment(settings.Repository, nameof(settings.Repository));
            var searchQuery = $"repo:{owner}/{repository}";

            if (!string.IsNullOrWhiteSpace(settings.Branch))
            {
                searchQuery += $"+branch:{GitHubApiPath.EncodeQueryValue(settings.Branch)}";
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                searchQuery += $"+{Uri.EscapeDataString(query)}";
            }

            var content = await GetResponseBodyAsync(HttpMethod.Get, $"{settings.ApiUrl}/search/code?q={searchQuery}", settings, cancellationToken);
            var searchResult = JsonSerializer.Deserialize<GitHubSearchResult>(content, JsonOptions);

            return searchResult ?? new GitHubSearchResult { Items = [] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching files with query {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Gets the recursive tree of the configured branch in a single request
    /// </summary>
    public async Task<GitHubTree?> GetTreeAsync(CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var reference = string.IsNullOrWhiteSpace(settings.Branch)
                ? "HEAD"
                : GitHubApiPath.EncodeBranchPath(settings.Branch, nameof(settings.Branch));
            var content = await GetResponseBodyAsync(HttpMethod.Get, $"{GetRepositoryUrl(settings)}/git/trees/{reference}?recursive=1", settings, cancellationToken);

            return JsonSerializer.Deserialize<GitHubTree>(content, JsonOptions);
        }
        catch (Exception ex) when (ex is not GitHubRateLimitException)
        {
            _logger.LogError(ex, "Error fetching the repository tree");
            throw;
        }
    }

    /// <summary>
    /// Gets the date of the last commit that changed a file on the configured branch
    /// </summary>
    /// <exception cref="ArgumentException">The path contains an empty, "." or ".." segment.</exception>
    public async Task<DateTime?> GetLastCommitDateAsync(string path, CancellationToken cancellationToken = default!)
    {
        var settings = CurrentSettings;

        try
        {
            if (!ValidateRepositoryConfig(settings))
            {
                return null;
            }

            var url = $"{GetRepositoryUrl(settings)}/commits?path={GitHubApiPath.EncodePath(path, nameof(path))}";
            if (!string.IsNullOrWhiteSpace(settings.Branch))
            {
                url += $"&sha={GitHubApiPath.EncodeQueryValue(settings.Branch)}";
            }

            var content = await GetResponseBodyAsync(HttpMethod.Get, $"{url}&per_page=1", settings, cancellationToken);

            // Only the newest commit's date is read, so the commit objects get no model of their own
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            {
                return null;
            }

            var commit = document.RootElement[0].TryGetProperty("commit", out var commitElement) ? commitElement : default;
            return commit.ValueKind == JsonValueKind.Object
                && commit.TryGetProperty("committer", out var committer)
                && committer.ValueKind == JsonValueKind.Object
                && committer.TryGetProperty("date", out var date)
                && date.TryGetDateTimeOffset(out var committed)
                    ? committed.UtcDateTime
                    : null;
        }
        catch (Exception ex) when (ex is not GitHubRateLimitException)
        {
            _logger.LogError(ex, "Error fetching the last commit date for {Path}", path);
            throw;
        }
    }

    /// <summary>
    /// Sets the owner and repository for subsequent requests made through this instance only
    /// </summary>
    public void SetRepository(string owner, string repository)
    {
        UpdateSettings(settings => settings with { Owner = owner, Repository = repository });
    }

    /// <summary>
    /// Sets the branch for subsequent requests made through this instance only
    /// </summary>
    public void SetBranch(string branch)
    {
        UpdateSettings(settings => settings with { Branch = branch });
    }

    /// <summary>
    /// Sets the access token for subsequent requests made through this instance only
    /// </summary>
    /// <remarks>
    /// The token is sent on each request message and the underlying <see cref="HttpClient"/> is never changed.
    /// An empty token removes authentication from this instance.
    /// </remarks>
    public void SetAccessToken(string token)
    {
        UpdateSettings(settings => settings with { AccessToken = token ?? string.Empty });
    }

    private void UpdateSettings(Func<ClientSettings, ClientSettings> update)
    {
        lock (_settingsLock)
        {
            Volatile.Write(ref _settings, update(_settings));
        }
    }

    private static string GetRepositoryUrl(ClientSettings settings)
    {
        var owner = GitHubApiPath.EncodeSegment(settings.Owner, nameof(settings.Owner));
        var repository = GitHubApiPath.EncodeSegment(settings.Repository, nameof(settings.Repository));
        return $"{settings.ApiUrl}/repos/{owner}/{repository}";
    }

    private static string GetContentsUrl(ClientSettings settings, string? path)
    {
        return $"{GetRepositoryUrl(settings)}/contents/{GitHubApiPath.EncodePath(path, nameof(path))}";
    }

    private static string WithBranchReference(string url, ClientSettings settings)
    {
        return string.IsNullOrWhiteSpace(settings.Branch)
            ? url
            : $"{url}?ref={GitHubApiPath.EncodeQueryValue(settings.Branch)}";
    }

    /// <summary>
    /// Sends a request and returns the response body, throwing for a non-success status code
    /// </summary>
    private async Task<string> GetResponseBodyAsync(
        HttpMethod method,
        string url,
        ClientSettings settings,
        CancellationToken cancellationToken,
        HttpContent? content = null)
    {
        using var response = await SendAsync(method, url, settings, cancellationToken, content);
        ThrowIfRateLimited(response);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    /// <summary>
    /// Throws <see cref="GitHubRateLimitException"/> for a 403 or 429 response that signals a rate limit
    /// </summary>
    /// <remarks>
    /// GitHub marks the primary limit with <c>x-ratelimit-remaining: 0</c> and a secondary limit with
    /// <c>retry-after</c>; both come with 403 or 429. A plain 403 (no such header) is a permission error and stays one.
    /// </remarks>
    private void ThrowIfRateLimited(HttpResponseMessage response)
    {
        if (response.StatusCode is not (System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.TooManyRequests))
        {
            return;
        }

        DateTimeOffset? resetAt = null;
        var limited = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests;

        if (response.Headers.RetryAfter is { } retryAfter)
        {
            limited = true;
            resetAt = retryAfter.Date ?? (retryAfter.Delta is { } delta ? DateTimeOffset.UtcNow + delta : null);
        }

        if (TryGetHeader(response, "x-ratelimit-remaining", out var remaining) && remaining == "0")
        {
            limited = true;
            if (resetAt is null && TryGetHeader(response, "x-ratelimit-reset", out var reset)
                && long.TryParse(reset, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var epochSeconds))
            {
                resetAt = DateTimeOffset.FromUnixTimeSeconds(epochSeconds);
            }
        }

        if (!limited)
        {
            return;
        }

        _logger.LogWarning("GitHub rate limit reached (status {StatusCode}); requests are allowed again at {ResetAt}", (int)response.StatusCode, resetAt);
        throw new GitHubRateLimitException(resetAt, response.StatusCode);
    }

    private static bool TryGetHeader(HttpResponseMessage response, string name, out string? value)
    {
        value = response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
        return value is not null;
    }

    /// <summary>
    /// Sends a request whose headers come from <paramref name="settings"/>, never from shared client state, and
    /// follows a redirect on the same scheme, host and port with those headers
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IHttpClientFactory"/> clients of the provider do not follow redirects themselves (see
    /// <see cref="GitHubHttpClientCookiePolicy"/>), because the framework drops the Authorization header on a
    /// redirect and every request after it would count against the server's shared unauthenticated limit.
    /// GitHub answers with a redirect when the configured repository was renamed or transferred.
    /// </para>
    /// <para>
    /// A redirect to another scheme, host or port is not followed and its response is returned, so the token never
    /// leaves the API host and no request silently continues without it. Following GitHub's guidance, a redirected
    /// request is repeated verbatim: the same method, headers and body.
    /// </para>
    /// </remarks>
    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        ClientSettings settings,
        CancellationToken cancellationToken,
        HttpContent? content = null)
    {
        var httpClient = _acquireHttpClient();
        var originalUri = new Uri(url, UriKind.Absolute);
        var requestUri = originalUri;

        try
        {
            for (var redirects = 0; ; redirects++)
            {
                var response = await SendOnceAsync(httpClient, method, requestUri, settings, content, cancellationToken);

                if (!IsRedirect(response.StatusCode) || response.Headers.Location is not { } location)
                {
                    return response;
                }

                var target = location.IsAbsoluteUri ? location : new Uri(requestUri, location);
                WarnRepositoryRedirectedOnce(settings, target);

                if (!IsSameOrigin(target, originalUri))
                {
                    LogRedirectToOtherOriginNotFollowed(_logger, (int)response.StatusCode, target);
                    return response;
                }

                if (redirects == MaxRedirects)
                {
                    LogTooManyRedirects(_logger, MaxRedirects, originalUri);
                    return response;
                }

                response.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                requestUri = target;
            }
        }
        finally
        {
            content?.Dispose();
        }
    }

    private static async Task<HttpResponseMessage> SendOnceAsync(
        HttpClient httpClient,
        HttpMethod method,
        Uri requestUri,
        ClientSettings settings,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, requestUri) { Content = content };
        try
        {
            ApplyHeaders(request, httpClient, settings);
            return await httpClient.SendAsync(request, cancellationToken);
        }
        finally
        {
            // Disposing the request would dispose its body, which a redirect sends again
            request.Content = null;
            request.Dispose();
        }
    }

    private static bool IsRedirect(System.Net.HttpStatusCode statusCode)
    {
        return statusCode is System.Net.HttpStatusCode.MovedPermanently
            or System.Net.HttpStatusCode.Found
            or System.Net.HttpStatusCode.TemporaryRedirect
            or System.Net.HttpStatusCode.PermanentRedirect;
    }

    private static bool IsSameOrigin(Uri target, Uri original)
    {
        return Uri.Compare(target, original, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
    }

    private void WarnRepositoryRedirectedOnce(ClientSettings settings, Uri target)
    {
        // Clients are created per operation, so the warning is kept to once per configured repository for the
        // process; past the cap the warning is logged every time rather than growing the set without bound.
        var key = $"{settings.ApiUrl}|{settings.Owner}/{settings.Repository}";
        if (ReportedRedirects.Count >= MaxReportedRedirects || ReportedRedirects.TryAdd(key, 0))
        {
            LogRepositoryRedirected(_logger, settings.Owner, settings.Repository, target);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "GitHub redirected requests for the configured repository {Owner}/{Repository} to {Location}. The repository was probably renamed or transferred; update the provider's Owner and Repository settings to its new name")]
    private static partial void LogRepositoryRedirected(ILogger logger, string owner, string repository, Uri location);

    [LoggerMessage(Level = LogLevel.Warning, Message = "GitHub redirect (status {StatusCode}) to {Location} was not followed, because it leaves the configured API host; the access token is only sent to that host")]
    private static partial void LogRedirectToOtherOriginNotFollowed(ILogger logger, int statusCode, Uri location);

    [LoggerMessage(Level = LogLevel.Warning, Message = "GitHub request to {Url} stopped after {MaxRedirects} redirects")]
    private static partial void LogTooManyRedirects(ILogger logger, int maxRedirects, Uri url);

    private static void ApplyHeaders(HttpRequestMessage request, HttpClient httpClient, ClientSettings settings)
    {
        // Accept and User-Agent configured by the host on the HttpClient take precedence
        if (!httpClient.DefaultRequestHeaders.Contains(AcceptHeaderName))
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(GitHubMediaType));
        }

        if (!httpClient.DefaultRequestHeaders.Contains(UserAgentHeaderName))
        {
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(UserAgentProduct, UserAgentVersion));
        }

        if (!string.IsNullOrWhiteSpace(settings.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthorizationScheme, settings.AccessToken);
        }
    }

    /// <summary>
    /// Validates that repository configuration is set
    /// </summary>
    private bool ValidateRepositoryConfig(ClientSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Owner) || string.IsNullOrWhiteSpace(settings.Repository))
        {
            _logger.LogError("Repository owner and name must be set before making requests");
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.AccessToken))
        {
            _logger.LogError("Access token is required for GitHub API requests");
            return false;
        }

        return true;
    }

    /// <summary>
    /// The values a request needs, replaced as a whole so a request never mixes values from two updates
    /// </summary>
    private sealed record ClientSettings(string Owner, string Repository, string Branch, string AccessToken, string ApiUrl)
    {
        public static ClientSettings From(GitHubOptions options)
        {
            return new ClientSettings(
                options.Owner,
                options.Repository,
                options.Branch,
                ResolveAccessToken(options),
                options.ApiUrl ?? DefaultApiUrl);
        }

        private static string ResolveAccessToken(GitHubOptions options)
        {
            // A GitHubProviderOptions instance can carry its own personal access token
            if (options is Domain.Options.Configuration.GitHubProviderOptions providerOptions)
            {
                return providerOptions.Authentication?.PersonalAccessToken ?? options.ApiToken ?? string.Empty;
            }

            // Plain GitHubOptions use the configured API token
            return options.ApiToken ?? string.Empty;
        }
    }
}

/// <summary>
/// Model for GitHub reference (used for branch creation)
/// </summary>
public class GitHubRef
{
    /// <summary>
    /// Gets or sets the reference name
    /// </summary>
    public string Ref { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference object
    /// </summary>
    public GitHubRefObject? Object { get; set; }
}

/// <summary>
/// Model for GitHub reference object (used for branch creation)
/// </summary>
public class GitHubRefObject
{
    /// <summary>
    /// Gets or sets the SHA hash
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
