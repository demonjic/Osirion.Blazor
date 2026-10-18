using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Core.Services;
using Osirion.Blazor.Examples.Shared.Docs;
using Osirion.Blazor.Examples.Shared.Themes;
using Shouldly;

namespace Osirion.Blazor.Examples.Tests.Site;

/// <summary>Requests the example site, hosted in memory by <see cref="SiteFactory"/>.</summary>
public partial class SiteTests(SiteFactory factory) : IClassFixture<SiteFactory>
{
    /// <summary>Every address the documentation had before it moved, and where it lives now.</summary>
    public static TheoryData<string, string> MovedAddresses()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "MovedAddresses.json");
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
        var data = new TheoryData<string, string>();
        foreach (var (old, current) in map)
        {
            data.Add(old, current);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(MovedAddresses))]
    public async Task MovedAddress_RedirectsPermanentlyToAPageThatAnswers(string oldAddress, string newAddress)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var redirect = await client.GetAsync(oldAddress);
        using var page = await client.GetAsync(newAddress);

        // Assert
        redirect.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
        redirect.Headers.Location?.OriginalString.ShouldBe(newAddress);
        page.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Outline_PlacesEveryDocumentationPage()
    {
        // Act
        var outline = await LoadOutlineAsync();

        // Assert
        outline.Pages.Count.ShouldBeGreaterThan(50);
        outline.Unplaced.Select(DocsOutline.UrlOf).ShouldBeEmpty();
        foreach (var group in DocsOutline.Groups.Where(group => group.PageUrl is not null))
        {
            outline.Find(group.PageUrl!).ShouldNotBeNull($"the {group.Key} group's page {group.PageUrl}");
        }
    }

    [Fact]
    public async Task DocumentationPages_LinkOnlyToAddressesThatAnswerDirectly()
    {
        // Arrange
        using var client = CreateClient();
        var outline = await LoadOutlineAsync();
        var linkedFrom = new Dictionary<string, string>(StringComparer.Ordinal);
        outline.Pages.ShouldNotBeEmpty();

        // Act
        var failures = new List<string>();
        foreach (var page in outline.Pages.Select(DocsOutline.UrlOf))
        {
            var html = await GetPageAsync(client, page, failures);
            foreach (var link in InternalLinks(html))
            {
                linkedFrom.TryAdd(link, page);
            }
        }

        foreach (var (link, source) in linkedFrom)
        {
            using var response = await client.GetAsync(link);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                failures.Add($"{link} answers {(int)response.StatusCode}, linked from {source}");
            }
        }

        // Assert
        failures.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("/components/no-such-component")]
    [InlineData("/components/Tabs")]
    [InlineData("/docs/no-such-page")]
    [InlineData("/xx/blog")]
    [InlineData("/counter")]
    [InlineData("/weather")]
    [InlineData("/no-such-page")]
    [InlineData("/a/b/c")]
    [InlineData("/tag/no-such-tag")]
    [InlineData("/category/no-such-category")]
    public async Task AddressWithoutAPage_AnswersNotFound(string address)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var response = await client.GetAsync(address);
        var html = await response.Content.ReadAsStringAsync();

        // Assert: the site's not-found page, not an empty 404
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        html.ShouldContain("<title>Page not found");
    }

    [Theory]
    [InlineData("/tag/seo")]
    [InlineData("/category/seo")]
    [InlineData("/tag")]
    [InlineData("/category")]
    public async Task ListPage_ForANameAPostUsesOrWithoutAName_AnswersOk(string address)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var response = await client.GetAsync(address);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/en", "/")]
    [InlineData("/en/blog", "/blog")]
    [InlineData("/en/blog/navigation-components", "/blog/navigation-components")]
    [InlineData("/en/docs/getting-started", "/docs/getting-started")]
    public async Task LocaleAddress_RedirectsPermanentlyToTheSamePathWithoutIt(string address, string target)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var response = await client.GetAsync(address);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MovedPermanently);
        response.Headers.Location?.OriginalString.ShouldBe(target);
    }

    [Fact]
    public async Task Changelog_RendersTheRepositoryChangelogWithItsReleasesInTheContents()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/changelog");

        // Assert
        html.ShouldContain("<h1 class=\"osirion-document-title\"");
        html.ShouldContain(">Changelog</h1>");
        html.ShouldContain(">Releases</h2>");
        html.ShouldContain("Unreleased");
        html.ShouldNotContain("date: 2025-04-23");
    }

    [Fact]
    public async Task Sitemap_ListsTheNewPages()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var sitemap = await client.GetStringAsync("/sitemap.xml");

        // Assert
        foreach (var path in new[] { "/components</loc>", "/themes</loc>", "/templates</loc>", "/changelog</loc>", "/docs/packages</loc>" })
        {
            sitemap.ShouldContain(path);
        }

        sitemap.ShouldNotContain("/docs/components");
    }

    [Fact]
    public async Task Gallery_ShowsEveryComponentGroupWithAnAnchorAndALinkToEachPage()
    {
        // Arrange
        using var client = CreateClient();
        var outline = await LoadOutlineAsync();

        // Act
        var html = await client.GetStringAsync(DocsOutline.GalleryUrl);

        // Assert
        foreach (var section in outline.Sections.Where(section => section.Group.InGallery && section.Members.Count > 0))
        {
            html.ShouldContain($"id=\"{section.Group.Key}\"");
            foreach (var member in section.Members)
            {
                html.ShouldContain($"href=\"{DocsOutline.UrlOf(member)}\"");
            }
        }
    }

    [Fact]
    public async Task ComponentPage_SidebarMarksThePageAndOpensOnlyItsGroup()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/components/tabs");
        var sidebar = SideNavigation().Match(html).Value;

        // Assert
        sidebar.ShouldContain("href=\"/components/tabs\" class=\"osirion-sidenav-link osirion-sidenav-active\" aria-current=\"page\"");
        sidebar.ShouldContain("href=\"/components/callout\"");
        sidebar.ShouldContain("href=\"/components/cards\"");
        sidebar.ShouldNotContain("href=\"/components/feature-card\"");
    }

    [Fact]
    public async Task ThemePage_OffersThePickerAndListsEveryTokenWithItsChip()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync(DocsOutline.ThemesUrl);

        // Assert
        html.ShouldContain("osirion-theme-picker-select");
        foreach (var token in ThemeTokens.Groups.SelectMany(group => group.Tokens))
        {
            html.ShouldContain($">{token.Property}</code>");
            html.ShouldContain($"background-color: var({token.Property})");
        }
    }

    [Fact]
    public async Task ComponentPage_ThemingHeadingLeadsToTheThemePage()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/components/palettes");
        var sidebar = SideNavigation().Match(html).Value;

        // Assert
        sidebar.ShouldContain("href=\"/themes\"");
    }

    [Fact]
    public async Task ContentDemos_AreRegisteredAndRenderOnTheirPage()
    {
        // Arrange
        using var client = CreateClient();
        var registry = factory.Services.GetRequiredService<IContentDemoRegistry>();
        var outline = await LoadOutlineAsync();
        var failures = new List<string>();
        var pagesWithDemos = outline.Pages.Where(page => page.OriginalMarkdown?.Contains("```demo", StringComparison.Ordinal) == true).ToList();

        // Act
        foreach (var page in pagesWithDemos)
        {
            var html = await client.GetStringAsync(DocsOutline.UrlOf(page));
            foreach (Match demo in DemoBlock().Matches(page.OriginalMarkdown!))
            {
                var key = demo.Groups["key"].Value;
                if (!registry.TryGet(key, out _))
                {
                    failures.Add($"{DocsOutline.UrlOf(page)}: no demo registered as {key}");
                }
                else if (!html.Contains("osirion-content-demo", StringComparison.Ordinal))
                {
                    failures.Add($"{DocsOutline.UrlOf(page)}: {key} rendered as a code block");
                }
            }
        }

        // Assert
        pagesWithDemos.ShouldNotBeEmpty();
        failures.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("/components/content-renderer")]
    [InlineData("/components/content-view")]
    [InlineData("/components/localized-content-view")]
    [InlineData("/components/content-page")]
    public async Task CmsPageDemos_ShowThisSitesOwnReleaseNotesPage(string address)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync(address);

        // Assert
        html.ShouldContain("Version 1.5.0 was released on 21 April 2025");
    }

    [Fact]
    public async Task AboutPage_ShowsTheProjectTheMaintainerTheLicenceAndHowToContribute()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/about");

        // Assert
        html.ShouldContain(">About Osirion.Blazor</h1>");
        html.ShouldContain(">The maintainer</h2>");
        html.ShouldContain("href=\"https://tridesetri.com\"");
        html.ShouldContain("href=\"https://hexavera.com\"");
        html.ShouldContain("href=\"https://demonjic.com\"");
        html.ShouldContain(">Licence</h2>");
        html.ShouldContain("href=\"https://github.com/demonjic/Osirion.Blazor/blob/master/CONTRIBUTING.md\"");
        html.ShouldContain("\"AboutPage\"");
    }

    [Fact]
    public async Task ShowcasePage_ListsTheThreeSitesWithAScreenshotAndALinkEach()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/showcase");

        // Assert
        html.ShouldContain(">Sites built with Osirion.Blazor</h1>");
        foreach (var (name, address, image) in new[]
        {
            ("Tridesetri", "https://tridesetri.com", "tridesetri.webp"),
            ("Hexavera", "https://hexavera.com", "hexavera.webp"),
            ("demonjic.com", "https://demonjic.com", "demonjic.webp"),
        })
        {
            html.ShouldContain($">{name}</a></h2>");
            html.ShouldContain($"href=\"{address}\"");
            html.ShouldContain($"/assets/images/showcase/{image}");
            using var picture = await client.GetAsync($"/assets/images/showcase/{image}");
            picture.StatusCode.ShouldBe(HttpStatusCode.OK);
            picture.Content.Headers.ContentLength.GetValueOrDefault().ShouldBeInRange(1, 200 * 1024);
        }

        html.ShouldNotContain("OsirionPricingTable");
    }

    [Fact]
    public async Task ContactPage_ListsTheChannelsAndOffersNoFormThatCouldNotDeliver()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/contact");

        // Assert
        html.ShouldContain("href=\"https://github.com/demonjic/Osirion.Blazor/issues/new/choose\"");
        html.ShouldContain("href=\"https://demonjic.com/contact\"");
        html.ShouldNotContain("osirion-contact-form-wrapper");
    }

    [Fact]
    public async Task PrivacyPolicy_NamesTheCookiesTheAnalyticsServicesAndHowToWithdrawConsent()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/privacy-policy");

        // Assert
        html.ShouldContain(">Privacy policy</h1>");
        foreach (var cookie in new[] { "osirion_cookie_consent", "osirion-preferred-theme", "osirion-preferred-palette", "osirion_announcement_" })
        {
            html.ShouldContain(cookie);
        }

        foreach (var service in new[] { "Google Analytics 4", "Microsoft Clarity", "Matomo", "Cloudflare" })
        {
            html.ShouldContain(service);
        }

        html.ShouldContain("Withdraw your consent");
        html.ShouldContain("Last updated: 17 October 2026");
    }

    [Fact]
    public async Task HomePage_HasOneHeadingAndLeadsToTheQuickestPathThemesPackagesComponentsAndPosts()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/");

        // Assert
        Regex.Matches(html, "<h1[ >]").Count.ShouldBe(1);
        foreach (var heading in new[] { "The quickest path", "Themes and frameworks", "What is in the box", "Featured components", "Server-rendered by default", "Latest posts" })
        {
            html.ShouldContain($">{heading}</h2>");
        }

        foreach (var link in new[] { "/docs/getting-started", "/themes", "/docs/packages", "/components", "/blog", "https://www.nuget.org/packages/Osirion.Blazor" })
        {
            html.ShouldContain($"href=\"{link}\"");
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/about")]
    [InlineData("/contact")]
    [InlineData("/privacy-policy")]
    [InlineData("/showcase")]
    public async Task SitePage_UsesNoEmOrEnDashesAndHasOneHeadingAndADescription(string address)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync(address);
        var body = CodeSample().Replace(html, string.Empty);

        // Assert
        Regex.Matches(html, "<h1[ >]").Count.ShouldBe(1);
        html.ShouldContain("name=\"description\"");
        body.ShouldNotContain((char)0x2014);
        body.ShouldNotContain((char)0x2013);
    }

    // Fails before the fix: the live demo's own title was a second h1 on the page.
    [Theory]
    [InlineData("/components/content-not-found")]
    [InlineData("/components/content-page")]
    public async Task ComponentPageWithAPageLevelDemo_HasOneHeading(string address)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync(address);

        // Assert
        Regex.Matches(html, "<h1[ >]").Count.ShouldBe(1);
    }

    [Fact]
    public async Task RobotsTxt_ServesTheContentRulesAsPlainTextAndNamesTheSitemap()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/robots.txt", UriKind.Relative));
        var robots = await response.Content.ReadAsStringAsync();
        var sitemap = await client.GetStringAsync("/sitemap.xml");

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/plain");
        robots.ShouldContain("Sitemap: https://getosirion.com/sitemap.xml");
        robots.ShouldContain("Disallow: /search");
        robots.ShouldNotContain("---");
        robots.ShouldNotContain("<p>");
        sitemap.ShouldNotContain("/robots</loc>");
    }

    [Fact]
    public async Task SitemapPages_DeclareTheirOwnAddressAsCanonical()
    {
        // Arrange
        using var client = CreateClient();
        var sitemap = await client.GetStringAsync("/sitemap.xml");
        var failures = new List<string>();

        // Act
        foreach (Match location in SitemapLocation().Matches(sitemap))
        {
            var path = new Uri(location.Groups["loc"].Value).AbsolutePath;
            var html = await GetPageAsync(client, path, failures);
            var canonical = Canonical().Match(html);
            if (html.Length > 0 && (!canonical.Success || new Uri(canonical.Groups["href"].Value).AbsolutePath.TrimEnd('/') != path.TrimEnd('/')))
            {
                failures.Add($"{path} declares {(canonical.Success ? canonical.Groups["href"].Value : "no canonical")}");
            }
        }

        // Assert
        failures.ShouldBeEmpty();
    }

    [Fact]
    public async Task Page_LinksPackageStylesheetsAndTheBlazorScriptByFingerprintedAddresses()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var html = await client.GetStringAsync("/components/breadcrumbs");
        var stylesheets = Stylesheet().Matches(html).Select(link => link.Groups["href"].Value).ToList();

        // Assert
        // A fingerprinted address changes with the file, so MapStaticAssets serves it as immutable for a year.
        stylesheets.ShouldContain(href => FingerprintedPackageStylesheet("Osirion.Blazor.Core/css/index.min").IsMatch(href));
        stylesheets.ShouldContain(href => FingerprintedPackageStylesheet("Osirion.Blazor.Core/css/osirion-html-renderer.min").IsMatch(href));
        stylesheets.ShouldContain(href => FingerprintedPackageStylesheet("Osirion.Blazor.Theming/css/adapters/bootstrap-adapter.min").IsMatch(href));
        stylesheets.ShouldContain(href => FingerprintedPackageStylesheet("Osirion.Blazor.Theming/css/index.min").IsMatch(href));
        stylesheets.ShouldNotContain(href => href.EndsWith(".min.css", StringComparison.Ordinal) && href.StartsWith("_content/", StringComparison.Ordinal));
        html.ShouldNotContain("src=\"_framework/blazor.web.js\"");
        foreach (var href in stylesheets.Where(href => href.StartsWith("_content/", StringComparison.Ordinal)))
        {
            using var response = await client.GetAsync(new Uri("/" + href, UriKind.Relative));
            response.StatusCode.ShouldBe(HttpStatusCode.OK, href);
        }
    }

    private static Regex FingerprintedPackageStylesheet(string path) =>
        new("^_content/" + Regex.Escape(path) + @"\.[a-z0-9]{10}\.css$", RegexOptions.None, TimeSpan.FromSeconds(1));

    [Fact]
    public async Task SitemapPages_HaveNoHeadingThatStartsWithANumber()
    {
        // Arrange
        using var client = CreateClient();
        var sitemap = await client.GetStringAsync("/sitemap.xml");
        var failures = new List<string>();

        // Act
        foreach (Match location in SitemapLocation().Matches(sitemap))
        {
            var path = new Uri(location.Groups["loc"].Value).AbsolutePath;
            var html = CodeSample().Replace(await GetPageAsync(client, path, failures), string.Empty);
            foreach (Match heading in Heading().Matches(html))
            {
                var text = WebUtility.HtmlDecode(Tag().Replace(heading.Groups["text"].Value, string.Empty)).Trim();
                if (NumberedHeading().IsMatch(text))
                {
                    failures.Add($"{path}: {text}");
                }
            }
        }

        // Assert
        // The table of contents numbers the sections itself, and a number in the text also lands in search results.
        failures.ShouldBeEmpty();
    }

    [Fact]
    public async Task SitemapPages_LinkOnlyToSectionsThatExist()
    {
        // Arrange
        using var client = CreateClient();
        var sitemap = await client.GetStringAsync("/sitemap.xml");
        var pages = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var links = new List<(string Source, string Target, string Fragment)>();
        var failures = new List<string>();

        // Act
        foreach (Match location in SitemapLocation().Matches(sitemap))
        {
            var path = new Uri(location.Groups["loc"].Value).AbsolutePath;
            var html = await GetPageAsync(client, path, failures);
            pages[path] = Ids(html);
            foreach (Match link in FragmentLink().Matches(CodeSample().Replace(html, string.Empty)))
            {
                var target = WebUtility.HtmlDecode(link.Groups["path"].Value);
                var fragment = Uri.UnescapeDataString(WebUtility.HtmlDecode(link.Groups["fragment"].Value));
                links.Add((path, target.Length == 0 ? path : target, fragment));
            }
        }

        foreach (var (source, target, fragment) in links)
        {
            if (!pages.TryGetValue(target, out var ids))
            {
                ids = Ids(await GetPageAsync(client, target, failures));
                pages[target] = ids;
            }

            if (!ids.Contains(fragment))
            {
                failures.Add($"{source} links to {target}#{fragment}, which has no such id");
            }
        }

        // Assert
        failures.Distinct().ShouldBeEmpty();
    }

    private static HashSet<string> Ids(string html) =>
        [.. IdAttribute().Matches(html).Select(id => WebUtility.HtmlDecode(id.Groups["id"].Value))];

    private HttpClient CreateClient() =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<DocsOutline> LoadOutlineAsync()
    {
        var provider = factory.Services.GetRequiredService<IContentProviderManager>().GetDefaultProvider();
        provider.ShouldNotBeNull();
        return DocsOutline.Build(await provider.GetAllItemsAsync());
    }

    private static async Task<string> GetPageAsync(HttpClient client, string address, List<string> failures)
    {
        using var response = await client.GetAsync(address);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            failures.Add($"{address} answers {(int)response.StatusCode}");
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync();
    }

    // Site addresses in href attributes, leaving out code samples, which show markup as text, and static files.
    private static IEnumerable<string> InternalLinks(string html)
    {
        var withoutCode = CodeSample().Replace(html, string.Empty);
        foreach (Match match in Href().Matches(withoutCode))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value);
            var path = href.Split('#', '?')[0];
            if (path.Length > 0 && !path.StartsWith("//", StringComparison.Ordinal) && !path.StartsWith("/_", StringComparison.Ordinal)
                && !Path.HasExtension(path))
            {
                yield return path;
            }
        }
    }

    [GeneratedRegex("""<pre\b.*?</pre>|<code\b.*?</code>""", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex CodeSample();

    [GeneratedRegex(""""<a\b[^>]*\shref="(?<href>/[^"]*)"""", RegexOptions.IgnoreCase)]
    private static partial Regex Href();

    [GeneratedRegex(@"^```demo?
(?<key>[a-z0-9-]+)?
```", RegexOptions.Multiline)]
    private static partial Regex DemoBlock();

    [GeneratedRegex(""""<link rel="stylesheet" href="(?<href>[^"]+)"""")]
    private static partial Regex Stylesheet();

    [GeneratedRegex("""<h(?<level>[1-6])\b[^>]*>(?<text>.*?)</h\k<level>>""", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Heading();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tag();

    // "1. Install", "1) Install", "1.2 Install", "Step 1: Install"; a version heading such as "4.2.0 - 2026-10-18" is not one.
    [GeneratedRegex("""^(?:\d+[.)]|\d+(?:\.\d+)+|Step\s+\d+[:.)]?)\s+(?=\p{L})""", RegexOptions.IgnoreCase)]
    private static partial Regex NumberedHeading();

    [GeneratedRegex(""""\shref="(?<path>/[^"#?]*)?#(?<fragment>[^"]+)"""", RegexOptions.IgnoreCase)]
    private static partial Regex FragmentLink();

    [GeneratedRegex(""""\sid="(?<id>[^"]+)"""")]
    private static partial Regex IdAttribute();

    [GeneratedRegex("""<loc>(?<loc>[^<]+)</loc>""")]
    private static partial Regex SitemapLocation();

    [GeneratedRegex(""""<link rel="canonical" href="(?<href>[^"]+)"""")]
    private static partial Regex Canonical();

    [GeneratedRegex("""<nav class="osirion-document-sidenav".*?</nav>""", RegexOptions.Singleline)]
    private static partial Regex SideNavigation();
}
