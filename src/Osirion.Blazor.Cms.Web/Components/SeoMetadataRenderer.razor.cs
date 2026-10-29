using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components.Seo;
using Osirion.Blazor.Cms.Web.Options;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Advanced SEO metadata renderer supporting traditional SEO, GEO (Generative Engine Optimization), and AEO (Answer Engine Optimization).
/// Implements comprehensive Schema.org structured data, Open Graph, Twitter Cards, and AI discovery meta tags.
/// </summary>
public partial class SeoMetadataRenderer(NavigationManager navigationManager)
{
    /// <summary>
    /// Injected SEO metadata options providing site-wide defaults
    /// </summary>
    [Inject]
    private IOptions<SeoMetadataOptions>? SeoOptions { get; set; }

    [Inject]
    private ILogger<SeoMetadataRenderer> Logger { get; set; } = default!;

    // Read for the optional services: IOsirionLocalePaths exists only on a site with locale routing, and the content
    // provider manager only on a site with content. [Inject] cannot express an optional service.
    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    // Core properties
    private string _siteName = string.Empty;
    private string _baseUrl = string.Empty;
    private string _currentUrl = string.Empty;
    private string _locale = "en_US";

    // Meta tags
    private string _metaTitle = string.Empty;
    private string? _metaDescription;
    private string? _metaKeywords;
    private IReadOnlyList<(string Name, string Value)> _extraMetaTags = [];

    // Open Graph
    private string? _ogTitle;
    private string? _ogDescription;
    private string? _ogImage;
    private string? _ogType;
    private int? _ogImageWidth;
    private int? _ogImageHeight;
    private string? _ogImageAlt;

    // The DefaultImageUrl the page passed, before the site-wide default fills the parameter: a list page's own image.
    private string? _pageImageUrl;

    // Twitter Card
    private string? _twitterTitle;
    private string? _twitterDescription;
    private string? _twitterImage;
    private string? _twitterCard;

    // SEO essentials
    private string? _canonicalUrl;
    private string? _robotsDirective;
    private string? _languageCode;
    private string? _referrerPolicy;

    // Structured data
    private string? _jsonLdContent;
    private List<string>? _additionalJsonLdSchemas;

    // GEO & AEO optimization
    private string? _entitySummary;
    private List<string>? _answersTo;
    private Dictionary<string, string>? _structuredAnswers;

    // Helper properties for defensive programming (Content or ContentItems scenarios)
    private bool _hasContent;
    private string? _author;
    private bool _isArticle;
    private DateTime? _publishDate;
    private DateTime? _modifiedDate;
    private int? _readTimeMinutes;
    private IReadOnlyList<string>? _tags;
    private IReadOnlyList<string>? _categories;

    // A page that is not a content item: a Title with neither Content nor ContentItems.
    private bool _isCodePage;

    // Locale routing, when the site has it; read once per parameter set.
    private IOsirionLocalePaths? _localePaths;

    // Set when a content item's translations are to be read for its hreflang links, after the synchronous metadata.
    private bool _loadTranslations;

    // Counts parameter sets, so translations read for an earlier item never replace the links of the current one.
    private int _parametersVersion;

    #region Parameters

    /// <summary>
    /// Gets or sets the item a single content page writes its metadata for. Set this or <see cref="ContentItems"/>, not both.
    /// With neither (or an empty list), the component writes only an empty page title and no other tag.
    /// </summary>
    [Parameter]
    public ContentItem? Content { get; set; }

    /// <summary>
    /// Gets or sets the items of a list page. The title becomes <c>{CollectionTitle} | {site name}</c>, <c>og:type</c> is
    /// <c>website</c>, and the JSON-LD holds an article per item, after a <c>CollectionPage</c> and a
    /// <c>BreadcrumbList</c> when <see cref="SchemaTypes"/> lists them. Each item's article is the item's front matter
    /// <c>type</c> when it is <c>Article</c>, <c>BlogPosting</c> or <c>TechArticle</c>, then the first of those three in
    /// <see cref="SchemaTypes"/>, then <c>Article</c>. An empty list counts as not set.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ContentItem>? ContentItems { get; set; }

    /// <summary>
    /// Gets or sets the title of a list page, used in the page title as <c>{CollectionTitle} | {site name}</c> and, cut to
    /// 60 characters, as <c>og:title</c>. Read only with <see cref="ContentItems"/>.
    /// </summary>
    [Parameter]
    public string? CollectionTitle { get; set; }

    /// <summary>
    /// Gets or sets the description of a list page: the meta description (cut to 160 characters), <c>og:description</c>
    /// (cut to 200) and the source of the summary tags. Read only with <see cref="ContentItems"/>.
    /// </summary>
    [Parameter]
    public string? CollectionDescription { get; set; }

    /// <summary>
    /// Gets or sets the language of a list page, as a BCP 47 tag such as <c>de</c> or <c>sr-Latn-RS</c>: the
    /// <c>og:locale</c>, the language meta tags and the language of the structured data. A page with
    /// <see cref="Content"/> takes its language from the item and ignores this. Without either, the page counts as
    /// English.
    /// </summary>
    [Parameter]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the site name written after the title and as <c>og:site_name</c>. When empty, the configured
    /// <c>SiteName</c> applies, then <see cref="OrganizationName"/>, then the host name of the site address.
    /// </summary>
    [Parameter]
    public string? SiteNameOverride { get; set; }

    /// <summary>
    /// Gets or sets a site description. The component accepts it but writes nothing from it. <c>MapOsirionFeeds</c>
    /// uses the configured <c>SiteDescription</c> as the feed description when the feed options set none.
    /// </summary>
    [Parameter]
    public string? SiteDescription { get; set; }

    /// <summary>
    /// Gets or sets the address of the publisher logo in structured data, absolute or relative to the site address.
    /// When empty, the configured value applies, then <c>/logo.png</c> on the site address.
    /// </summary>
    [Parameter]
    public string? SiteLogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the handle of the site for <c>twitter:site</c>, for example <c>@contoso</c>. Nothing is written when empty.
    /// </summary>
    [Parameter]
    public string? TwitterSite { get; set; }

    /// <summary>
    /// Gets or sets the handle of the author for <c>twitter:creator</c>, for example <c>@author</c>. Nothing is written when empty.
    /// </summary>
    [Parameter]
    public string? TwitterCreator { get; set; }

    /// <summary>
    /// Gets or sets the Facebook app ID written as <c>fb:app_id</c>. Nothing is written when empty.
    /// </summary>
    [Parameter]
    public string? FacebookAppId { get; set; }

    /// <summary>
    /// Gets or sets the JSON-LD types to generate, such as <c>Article</c>, <c>BlogPosting</c>, <c>TechArticle</c>,
    /// <c>WebPage</c>, <c>HowTo</c>, <c>BreadcrumbList</c> or <c>CollectionPage</c>. When empty, the configured
    /// <c>DefaultSchemaTypes</c> apply, then the front matter <c>type</c>, then <see cref="FallbackSchemaType"/>, then
    /// <c>WebPage</c>. On a list page only <c>CollectionPage</c> and <c>BreadcrumbList</c> write a schema of their own,
    /// and an article type names the type of the listed items. The first type also sets
    /// <c>og:type</c> when the front matter has no <c>og_type</c>. <c>FAQ</c> generates no data: use
    /// <see cref="EnableGeneratedFaq"/>. A single type gets a <c>BreadcrumbList</c> added when
    /// <see cref="GenerateMultipleSchemas"/> is on.
    /// </summary>
    [Parameter]
    public SchemaType[]? SchemaTypes { get; set; }

    /// <summary>
    /// Gets or sets whether the crawler opt-in meta tags are written (<c>true</c>) or the opt-out tags (<c>false</c>). With
    /// <c>false</c>, the training tag says disallow whatever <see cref="AllowAiTraining"/> is. These meta names are not a
    /// standard; use <c>robots.txt</c> to control crawlers. When not set, the configured value applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool AllowAiDiscovery { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the training tags (<c>ai-training</c> and <c>Google-Extended</c>) opt in (<c>true</c>) or out
    /// (<c>false</c>). Ignored when <see cref="AllowAiDiscovery"/> is <c>false</c>. When not set, the configured value
    /// applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool AllowAiTraining { get; set; } = true;

    /// <summary>
    /// Gets or sets whether search engines may index the page. <c>false</c> writes <c>noindex, nofollow</c> as <c>robots</c>,
    /// <c>googlebot</c> and <c>bingbot</c>; <c>true</c> writes the front matter <c>robots</c> value, or
    /// <c>index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1</c> when it has none. When not set,
    /// the configured value applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool AllowSearchIndexing { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the summary meta tags are written (<c>entity-summary</c>, <c>content-type</c>,
    /// <c>primary-category</c>, <c>content-author</c>, <c>content-expertise-level</c> and <c>content-freshness</c>). They are
    /// written only when the item or list has a description. When not set, the configured value applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool EnableGeoOptimization { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the generated answer tags are allowed. It must be on for <see cref="EnableGeneratedFaq"/> to
    /// have an effect. When not set, the configured value applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool EnableAeoOptimization { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a <c>BreadcrumbList</c> is added when exactly one schema type is generated for an item with a
    /// path. Several types given in <see cref="SchemaTypes"/> are used as listed. When not set, the configured value
    /// applies, then <c>true</c>.
    /// </summary>
    [Parameter]
    public bool GenerateMultipleSchemas { get; set; } = true;

    /// <summary>
    /// Gets or sets a page that identifies the author, such as an About page, named as the author <c>url</c> in structured
    /// data. It is meant for a site with one author: every article and every posting in a list names it. Without one the
    /// author is named alone, because a guessed address that answers 404 is worse than none.
    /// </summary>
    [Parameter]
    public string? AuthorUrl { get; set; }

    /// <summary>
    /// Gets or sets the social image (<c>og:image</c> and <c>twitter:image</c>). A relative address becomes absolute with
    /// the site address. On a content page it is used when the item has no <c>og_image_url</c>, no <c>image</c> and no
    /// featured image. On a list page (<see cref="ContentItems"/>) an image passed here is the list's own image and comes
    /// first; then the first item's featured image; then the configured <see cref="SeoMetadataOptions.DefaultImageUrl"/>,
    /// which as a site-wide default never replaces an item's image.
    /// </summary>
    [Parameter]
    public string? DefaultImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the width written as <c>og:image:width</c>, in pixels. When not set, the configured width applies,
    /// then <c>1200</c>.
    /// </summary>
    [Parameter]
    public int? DefaultImageWidth { get; set; }

    /// <summary>
    /// Gets or sets the height written as <c>og:image:height</c>, in pixels. When not set, the configured height applies,
    /// then <c>630</c>.
    /// </summary>
    [Parameter]
    public int? DefaultImageHeight { get; set; }

    /// <summary>
    /// Gets or sets the publisher name in structured data. It is also the site name when no site name is set.
    /// </summary>
    [Parameter]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the <c>applicationCategory</c> of the <see cref="SchemaType.SoftwareApplication"/> schema, one of the app
    /// types search engines support, such as <c>DeveloperApplication</c> or <c>BusinessApplication</c>.
    /// </summary>
    [Parameter]
    public string ApplicationCategory { get; set; } = "DeveloperApplication";

    /// <summary>
    /// Gets or sets the <c>hreflang</c> links of other language versions, each entry a language code and an address joined by
    /// a vertical bar, such as <c>de|https://example.com/de/page</c>. Malformed entries are skipped with a logged warning.
    /// When it is <see langword="null"/> on a site with locale routing (<c>IOsirionLocalePaths</c>), the links are written
    /// for the page instead: a content item links its published, indexable translations (the items with its
    /// <c>ContentId</c>), and any other page links its address in every configured culture, each with the culture name,
    /// then <c>x-default</c> with the default culture's address; nothing is written for fewer than two versions. A list
    /// passed here, even an empty one, always wins.
    /// </summary>
    [Parameter]
    public List<string>? AlternateLanguageUrls { get; set; }

    /// <summary>
    /// Gets or sets the address written as <c>rel="prev"</c>. Nothing is written when empty.
    /// </summary>
    [Parameter]
    public string? PrevPageUrl { get; set; }

    /// <summary>
    /// Gets or sets the address written as <c>rel="next"</c>. Nothing is written when empty.
    /// </summary>
    [Parameter]
    public string? NextPageUrl { get; set; }

    /// <summary>
    /// Gets or sets the name of the first <c>BreadcrumbList</c> entry. When empty, the configured value applies.
    /// </summary>
    [Parameter]
    public string BreadcrumbHomeText { get; set; } = "Home";

    /// <summary>
    /// Gets or sets the breadcrumb entries after the home entry, as name and address pairs, in place of the ones built from
    /// the item path.
    /// </summary>
    [Parameter]
    public List<(string Name, string Url)>? CustomBreadcrumbs { get; set; }

    /// <summary>
    /// Gets or sets the query parameters kept in the canonical URL, <c>og:url</c> and structured data addresses (for example
    /// <c>page</c> for paginated lists, where each page is its own document). All other parameters and the fragment are
    /// dropped. A canonical address declared in the front matter always wins.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<string>? CanonicalQueryParameters { get; set; }

    /// <summary>
    /// Gets or sets whether the viewport, theme-color and mobile web app meta tags are written. Off by default: the host
    /// document owns these, and writing them again duplicates its tags. When not set, the configured value applies, then <c>false</c>;
    /// a value set on the page wins over the configured one in both directions.
    /// </summary>
    [Parameter]
    public bool EmitMobileMetaTags { get; set; }

    /// <summary>
    /// Gets or sets whether FAQ structured data and answer meta tags are built from the title and description (requires
    /// <see cref="EnableAeoOptimization"/>). Off by default because the question is derived, not authored. When not set, the configured
    /// value applies, then <c>false</c>; a value set on the page wins over the configured one in both directions.
    /// </summary>
    [Parameter]
    public bool EnableGeneratedFaq { get; set; }

    /// <summary>
    /// Gets or sets the referrer policy written as <c>&lt;meta name="referrer"&gt;</c>, such as <c>strict-origin</c>.
    /// When empty, the configured <see cref="SeoMetadataOptions.ReferrerPolicy"/> applies; when that is empty too, no
    /// tag is written, so the site's <c>Referrer-Policy</c> response header (or the browser default,
    /// <c>strict-origin-when-cross-origin</c>) applies. A referrer meta tag replaces the header's policy for the whole
    /// document. A value that is not one of the policy tokens of the Referrer Policy specification writes nothing.
    /// </summary>
    [Parameter]
    public string? ReferrerPolicy { get; set; }

    /// <summary>
    /// Gets or sets the schema type of a content page when nothing else names one: <see cref="SchemaTypes"/>, the
    /// configured <c>DefaultSchemaTypes</c> and the front matter <c>type</c> all come first. When not set,
    /// <c>WebPage</c> applies. <c>ArticlePage</c> sets it to <see cref="SchemaType.Article"/>.
    /// </summary>
    [Parameter]
    public SchemaType? FallbackSchemaType { get; set; }

    /// <summary>
    /// Gets or sets the title of a page that is not a content item, such as a contact page written in Razor. With a
    /// title and neither <see cref="Content"/> nor <see cref="ContentItems"/>, the component writes the whole head for
    /// that page: the page title as <c>{Title} | {site name}</c>, the description, the canonical URL, Open Graph and
    /// Twitter tags (<c>og:type</c> <c>website</c>), a schema.org <c>WebPage</c> (or the <c>ContactPage</c> or
    /// <c>AboutPage</c> that <see cref="SchemaTypes"/> or <see cref="FallbackSchemaType"/> names) and the
    /// <c>hreflang</c> links. Ignored when <see cref="Content"/> or <see cref="ContentItems"/> is set.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of a page that is not a content item: the meta description (cut to 160 characters),
    /// <c>og:description</c> (cut to 200) and the description of its structured data. Read only with <see cref="Title"/>.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the social image of a page that is not a content item (<c>og:image</c> and <c>twitter:image</c>),
    /// absolute or relative to the site address. When empty, <see cref="DefaultImageUrl"/> applies. Read only with
    /// <see cref="Title"/>.
    /// </summary>
    [Parameter]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the canonical address of a page that is not a content item, as a path on the site such as
    /// <c>/contact</c>, made absolute with the site address, or an absolute <c>http</c> or <c>https</c> address. On a
    /// localized page pass the localized path (<c>/sr/contact</c>), for example from
    /// <c>IOsirionLocalePaths.LocalizeForCurrentCulture</c>. When empty, the current address is the canonical one, as on
    /// every other page. The <c>hreflang</c> links are built from it. Read only with <see cref="Title"/>.
    /// </summary>
    [Parameter]
    public string? CanonicalPath { get; set; }

    /// <summary>
    /// Gets or sets the robots directives of the page, written as <c>robots</c>, <c>googlebot</c> and <c>bingbot</c>,
    /// for example <c>noindex, follow</c> for a page that crawlers should follow but not index, or
    /// <c>max-snippet:50</c>. A comma-separated list of the directives search engines document: <c>all</c>,
    /// <c>none</c>, <c>index</c>, <c>noindex</c>, <c>follow</c>, <c>nofollow</c>, <c>noarchive</c>, <c>nocache</c>,
    /// <c>nosnippet</c>, <c>notranslate</c>, <c>noimageindex</c>, <c>indexifembedded</c>, <c>max-snippet:N</c>,
    /// <c>max-video-preview:N</c> (<c>N</c> at least <c>-1</c>), <c>max-image-preview:none|standard|large</c> and
    /// <c>unavailable_after:</c> with an ISO 8601 date. Any other value is ignored with a logged warning. A content
    /// item's front matter <c>robots</c> comes first, and <see cref="AllowSearchIndexing"/> set to <c>false</c> always
    /// writes <c>noindex, nofollow</c>. When empty, the page gets
    /// <c>index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1</c>.
    /// </summary>
    [Parameter]
    public string? Robots { get; set; }

    #endregion

    /// <summary>
    /// Alternate language versions as <c>(hreflang, href)</c> pairs parsed from <see cref="AlternateLanguageUrls"/>.
    /// </summary>
    private List<(string HrefLang, string Href)> _alternateLinks = [];

    /// <summary>The meta names the renderer writes for every page, which front matter <c>meta_tags</c> cannot repeat.</summary>
    private static readonly HashSet<string> RendererMetaNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "description", "keywords", "author", "generator", "language", "robots", "googlebot", "bingbot", "referrer"
    };

    private static readonly Dictionary<string, string> OgTypesBySchemaType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Article"] = "article",
        ["BlogPosting"] = "article",
        ["TechArticle"] = "article",
        ["ProfilePage"] = "profile",
        ["Product"] = "product",
        ["VideoObject"] = "video.other"
    };

    /// <summary>The policies of the Referrer Policy specification, the values a referrer meta tag may carry.</summary>
    private static readonly HashSet<string> ReferrerPolicyTokens = new(StringComparer.Ordinal)
    {
        "no-referrer", "no-referrer-when-downgrade", "same-origin", "origin", "strict-origin",
        "origin-when-cross-origin", "strict-origin-when-cross-origin", "unsafe-url"
    };

    /// <summary>The schema types a page that is not a content item can be written as: each describes a web page.</summary>
    private static readonly HashSet<SchemaType> CodePageSchemaTypes =
        [Cms.Web.Components.SchemaType.WebPage, Cms.Web.Components.SchemaType.ContactPage, Cms.Web.Components.SchemaType.AboutPage];

    /// <summary>The schema types a listed item can be written as: each takes the article properties.</summary>
    private static readonly HashSet<SchemaType> ArticleSchemaTypes =
        [Cms.Web.Components.SchemaType.Article, Cms.Web.Components.SchemaType.BlogPosting, Cms.Web.Components.SchemaType.TechArticle];

    /// <summary>The Open Graph image size Facebook and others recommend, written when neither a parameter nor an option sets one.</summary>
    private const int RecommendedImageWidth = 1200;

    private const int RecommendedImageHeight = 630;

    private readonly HashSet<string> _explicitParameters = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public override Task SetParametersAsync(ParameterView parameters)
    {
        _explicitParameters.Clear();
        foreach (var parameter in parameters)
        {
            _explicitParameters.Add(parameter.Name);
        }

        return base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// Recomputes all SEO metadata whenever the parameters are set, so a reused component instance never
    /// shows the title, canonical URL or structured data of the previous page.
    /// </summary>
    protected override void OnParametersSet()
    {
        ResetState();
        _parametersVersion++;
        _pageImageUrl = _explicitParameters.Contains(nameof(DefaultImageUrl)) ? DefaultImageUrl : null;
        var hasItems = ContentItems is not null && ContentItems.Any();
        _isCodePage = Content is null && !hasItems && !string.IsNullOrWhiteSpace(Title);

        // Require Content, ContentItems or the Title of a page that is not a content item
        if (Content is null && !hasItems && !_isCodePage) return;

        _localePaths = Services.GetService<IOsirionLocalePaths>();

        // Apply defaults from options if available
        ApplyDefaultsFromOptions();

        // Initialize helper properties based on whether we have single Content or ContentItems
        InitializeHelperProperties();

        // Core setup
        var siteUrl = ConfiguredSiteUrl();
        _siteName = SiteNameOverride ?? OrganizationName ?? SeoText.ExtractSiteName(siteUrl ?? navigationManager.BaseUri);
        _baseUrl = (siteUrl ?? navigationManager.BaseUri).TrimEnd('/');
        _currentUrl = siteUrl is null
            ? ResolvePageUrl()
            : _baseUrl + "/" + navigationManager.ToBaseRelativePath(ResolvePageUrl());
        var requestCulture = RequestCultureName();
        _locale = SeoText.ToOpenGraphLocale(SeoText.FirstNonBlank(Content?.Metadata?.SeoProperties?.Lang, Content?.Locale, Locale, requestCulture));
        _languageCode = Content?.Locale ?? Locale ?? requestCulture ?? "en";
        _alternateLinks = ParseAlternateLanguageUrls();

        // Build all metadata
        BuildMetadata();
        BuildOpenGraphMetadata();
        _isArticle = string.Equals(_ogType, "article", StringComparison.OrdinalIgnoreCase);
        BuildTwitterCardMetadata();
        BuildSeoEssentials();
        BuildLanguageVersions();
        _referrerPolicy = ResolveReferrerPolicy();

        // GEO & AEO optimization (before the structured data, which reuses the generated answers)
        if (EnableGeoOptimization)
        {
            BuildGeoOptimization();
        }

        if (EnableAeoOptimization && EnableGeneratedFaq)
        {
            BuildAeoOptimization();
        }

        // Generate structured data
        GenerateAllJsonLdSchemas();
        AddSiteSchemas();
        AddGeneratedFaqSchema();
    }

    /// <summary>
    /// Reads the translations of a content item for its <c>hreflang</c> links, when <see cref="BuildLanguageVersions"/>
    /// asked for them. A provider that has the items cached answers at once, so the head is complete in the first render.
    /// </summary>
    /// <returns>A task that completes when the links are set.</returns>
    protected override async Task OnParametersSetAsync()
    {
        if (!_loadTranslations || Content is null || _localePaths is null || _canonicalUrl is null)
        {
            return;
        }

        var version = _parametersVersion;
        var links = await LoadTranslationLinksAsync(Content, _localePaths, _canonicalUrl);
        if (version == _parametersVersion)
        {
            _alternateLinks = links;
        }
    }

    private void ResetState()
    {
        _hasContent = false;
        _isCodePage = false;
        _localePaths = null;
        _loadTranslations = false;
        _metaTitle = string.Empty;
        _metaDescription = null;
        _metaKeywords = null;
        _extraMetaTags = [];
        _ogTitle = null;
        _ogDescription = null;
        _ogImage = null;
        _ogType = null;
        _ogImageWidth = null;
        _ogImageHeight = null;
        _ogImageAlt = null;
        _twitterTitle = null;
        _twitterDescription = null;
        _twitterImage = null;
        _twitterCard = null;
        _canonicalUrl = null;
        _currentUrl = string.Empty;
        _robotsDirective = null;
        _languageCode = null;
        _referrerPolicy = null;
        _jsonLdContent = null;
        _additionalJsonLdSchemas = null;
        _entitySummary = null;
        _answersTo = null;
        _structuredAnswers = null;
        _author = null;
        _isArticle = false;
        _publishDate = null;
        _modifiedDate = null;
        _readTimeMinutes = null;
        _tags = null;
        _categories = null;
        _alternateLinks = [];
    }

    /// <summary>
    /// The URL this page declares for itself: the current address without its fragment and without query
    /// parameters, except those named in <see cref="CanonicalQueryParameters"/>. Tracking parameters such as
    /// <c>utm_source</c> therefore never produce a second canonical URL.
    /// </summary>
    private string ResolvePageUrl() =>
        ContentUrls.StripQueryAndFragment(navigationManager.Uri, CanonicalQueryParameters);

    /// <summary>
    /// The site's public address from <see cref="SeoMetadataOptions.SiteUrl"/>, ending in a slash like
    /// <see cref="NavigationManager.BaseUri"/>, or <see langword="null"/> when none is set or it is not an absolute
    /// http(s) address.
    /// </summary>
    private string? ConfiguredSiteUrl()
    {
        var configured = SeoOptions?.Value?.SiteUrl;
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        return uri.GetLeftPart(UriPartial.Path).TrimEnd('/') + "/";
    }

    private List<(string HrefLang, string Href)> ParseAlternateLanguageUrls()
    {
        var links = new List<(string, string)>();
        foreach (var entry in AlternateLanguageUrls ?? [])
        {
            var separator = entry?.IndexOf('|', StringComparison.Ordinal) ?? -1;
            if (separator <= 0 || separator == entry!.Length - 1)
            {
                Logger.LogWarning("Skipping malformed alternate language entry '{Entry}'; expected 'language|url'", entry);
                continue;
            }

            links.Add((entry[..separator].Trim(), entry[(separator + 1)..].Trim()));
        }

        return links;
    }

    /// <summary>
    /// The culture name of the address on a site with locale routing: the culture of its prefix, else the default
    /// culture. <see langword="null"/> without locale routing, where the page counts as English unless told otherwise.
    /// </summary>
    private string? RequestCultureName()
    {
        if (_localePaths is null)
        {
            return null;
        }

        var currentPath = "/" + navigationManager.ToBaseRelativePath(ContentUrls.StripQueryAndFragment(navigationManager.Uri));
        return _localePaths.TryGetCulture(currentPath, out var culture, out _)
            ? culture.Culture.Name
            : _localePaths.DefaultCulture.Culture.Name;
    }

    /// <summary>
    /// The <c>hreflang</c> links the page did not pass, on a site with locale routing: a page that is not a content item
    /// (a list page or a code page) in every culture, from its canonical path with the canonical query parameters; a
    /// content item from its translations, which <see cref="OnParametersSetAsync"/> reads.
    /// </summary>
    private void BuildLanguageVersions()
    {
        if (AlternateLanguageUrls is not null || _localePaths is null)
        {
            return;
        }

        if (Content is not null)
        {
            _loadTranslations = !string.IsNullOrWhiteSpace(Content.ContentId);
            return;
        }

        var sitePath = _isCodePage && LocalUrl.IsLocalPath(CanonicalPath)
            ? CanonicalPath!
            : "/" + navigationManager.ToBaseRelativePath(ResolvePageUrl());
        _alternateLinks = LanguageVersions.ForPage(_localePaths, sitePath, path => _baseUrl + path);
    }

    /// <summary>
    /// The <c>hreflang</c> links of a content item: its own canonical address in its culture, then each published,
    /// indexable translation (an item with the same <c>ContentId</c>) in a configured culture, at its URL from the site
    /// root with that culture's prefix. A culture without a translation is not linked; a failed read writes no links.
    /// </summary>
    private async Task<List<(string HrefLang, string Href)>> LoadTranslationLinksAsync(
        ContentItem content, IOsirionLocalePaths localePaths, string canonicalUrl)
    {
        var manager = Services.GetService<IContentProviderManager>();
        if (manager is null || LanguageVersions.CultureOf(localePaths, content.Locale) is not { } currentCulture)
        {
            return [];
        }

        IReadOnlyList<ContentItem>? translations;
        try
        {
            translations = await manager.GetContentByQueryAsync(
                new ContentQuery { LocalizationId = content.ContentId, IncludeUnpublished = false });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogWarning(exception, "Writing no hreflang links for content {ContentPath}; its translations could not be read", content.Path);
            return [];
        }

        var versions = new Dictionary<OsirionLocaleCulture, string> { [currentCulture] = canonicalUrl };
        foreach (var translation in translations ?? [])
        {
            if (LanguageVersions.IsNoIndex(translation)
                || !string.Equals(translation.ContentId, content.ContentId, StringComparison.OrdinalIgnoreCase)
                || LanguageVersions.CultureOf(localePaths, translation.Locale) is not { } culture)
            {
                continue;
            }

            versions.TryAdd(culture, _baseUrl + localePaths.Localize(ContentUrls.ItemUrl(translation), culture.Culture));
        }

        return LanguageVersions.Ordered(localePaths, versions);
    }

    /// <summary>
    /// Applies default values from injected SeoMetadataOptions if parameters are not explicitly set.
    /// </summary>
    private void ApplyDefaultsFromOptions()
    {
        var options = SeoOptions?.Value;
        if (options is null) return;

        ApplySiteDefaults(options);
        ApplyRenderingDefaults(options);
    }

    private void ApplySiteDefaults(SeoMetadataOptions options)
    {
        // Apply site-wide defaults only if parameters are not set
        SiteNameOverride ??= options.SiteName;
        SiteDescription ??= options.SiteDescription;
        SiteLogoUrl ??= options.SiteLogoUrl;
        TwitterSite ??= options.TwitterSite;
        TwitterCreator ??= options.TwitterCreator;
        FacebookAppId ??= options.FacebookAppId;
        DefaultImageUrl ??= options.DefaultImageUrl;
        OrganizationName ??= options.OrganizationName;
        AuthorUrl ??= options.AuthorUrl;
        ReferrerPolicy ??= options.ReferrerPolicy;
    }

    private void ApplyRenderingDefaults(SeoMetadataOptions options)
    {
        BreadcrumbHomeText = string.IsNullOrWhiteSpace(BreadcrumbHomeText) ? options.BreadcrumbHomeText : BreadcrumbHomeText;

        // Apply default settings if not explicitly overridden
        DefaultImageWidth ??= options.DefaultImageWidth;
        DefaultImageHeight ??= options.DefaultImageHeight;

        if (!_explicitParameters.Contains(nameof(EmitMobileMetaTags))) EmitMobileMetaTags = options.EmitMobileMetaTags;
        if (!_explicitParameters.Contains(nameof(EnableGeneratedFaq))) EnableGeneratedFaq = options.EnableGeneratedFaq;

        // The switches have defaults of their own, so a switch the page did not set takes the site's configuration;
        // reading the value alone cannot tell "left at true" from "set to true".
        if (!_explicitParameters.Contains(nameof(AllowAiDiscovery))) AllowAiDiscovery = options.AllowAiDiscovery;
        if (!_explicitParameters.Contains(nameof(AllowAiTraining))) AllowAiTraining = options.AllowAiTraining;
        if (!_explicitParameters.Contains(nameof(AllowSearchIndexing))) AllowSearchIndexing = options.AllowSearchIndexing;
        if (!_explicitParameters.Contains(nameof(EnableGeoOptimization))) EnableGeoOptimization = options.EnableGeoOptimization;
        if (!_explicitParameters.Contains(nameof(EnableAeoOptimization))) EnableAeoOptimization = options.EnableAeoOptimization;
        if (!_explicitParameters.Contains(nameof(GenerateMultipleSchemas))) GenerateMultipleSchemas = options.GenerateMultipleSchemas;

        // Apply schema types if not set
        SchemaTypes ??= options.DefaultSchemaTypes;
    }

    /// <summary>
    /// Initializes helper properties based on whether we have single Content or ContentItems.
    /// Provides defensive programming to avoid null reference exceptions in markup.
    /// </summary>
    private void InitializeHelperProperties()
    {
        _hasContent = Content is not null || (ContentItems?.Any() == true) || _isCodePage;

        if (Content is not null)
        {
            // Single content page
            _author = Content.Author;
            _publishDate = Content.PublishDate;
            _modifiedDate = Content.LastModified;
            _readTimeMinutes = Content.ReadTimeMinutes;
            _tags = Content.Tags;
            _categories = Content.Categories;
        }
        else if (ContentItems?.Any() == true)
        {
            // Collection page - extract from first item or use defaults
            var firstItem = ContentItems.First();
            _author = firstItem.Author;
            _publishDate = firstItem.PublishDate;
            _modifiedDate = ContentItems.Max(c => c.LastModified);
            _readTimeMinutes = null; // Collections don't have read time
            _tags = ContentItems.SelectMany(c => c.Tags).Distinct().ToList();
            _categories = ContentItems.SelectMany(c => c.Categories).Distinct().ToList();
        }
    }

    #region Metadata Builders

    private void BuildMetadata()
    {
        _metaTitle = BuildMetaTitle();

        if (ContentItems?.Any() == true)
        {
            // Collection page metadata
            _metaDescription = SeoText.TruncateDescription(CollectionDescription, 160);
            // Aggregate all unique tags from collection
            var allTags = ContentItems.SelectMany(c => c.Tags).Distinct().ToList();
            _metaKeywords = allTags.Any() ? string.Join(", ", allTags) : null;
        }
        else
        {
            // Single content page metadata
            _metaDescription = SeoText.TruncateDescription(
                SeoText.FirstNonBlank(Content?.Metadata?.SeoProperties?.Description, Content?.Description, CodePageDescription),
                160
            );
            _metaKeywords = Content?.Tags.Any() == true ? string.Join(", ", Content.Tags) : null;
        }

        _extraMetaTags = BuildExtraMetaTags();
    }

    /// <summary>
    /// The extra meta elements to write: the configured <see cref="SeoMetadataOptions.AdditionalMetaTags"/>, then the
    /// front matter <c>meta_tags</c> of a single item, which replace a configured value of the same name. A name must
    /// match <see cref="SeoText.IsSafeMetaName"/> and not be one the renderer writes itself, so an entry cannot repeat
    /// <c>robots</c> or <c>description</c> with another value. Skipped entries are logged.
    /// </summary>
    private List<(string Name, string Value)> BuildExtraMetaTags()
    {
        var candidates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in SeoOptions?.Value?.AdditionalMetaTags ?? [])
        {
            candidates[name] = value;
        }

        if (ContentItems?.Any() != true)
        {
            foreach (var (name, value) in Content?.Metadata?.SeoProperties?.MetaTags ?? [])
            {
                candidates[name] = value;
            }
        }

        var accepted = new List<(string, string)>();
        foreach (var (name, value) in candidates)
        {
            if (!SeoText.IsSafeMetaName(name) || RendererMetaNames.Contains(name))
            {
                Logger.LogWarning(
                    "Skipping meta tag '{Name}'; the name is not a plain meta name or the renderer writes it itself",
                    name);
                continue;
            }

            accepted.Add((name, value));
        }

        return accepted;
    }

    private void BuildOpenGraphMetadata()
    {
        if (ContentItems?.Any() == true)
        {
            BuildCollectionOpenGraphMetadata(ContentItems);
        }
        else
        {
            BuildContentOpenGraphMetadata();
        }

        _ogImageWidth = DefaultImageWidth ?? RecommendedImageWidth;
        _ogImageHeight = DefaultImageHeight ?? RecommendedImageHeight;
    }

    private void BuildCollectionOpenGraphMetadata(IReadOnlyList<ContentItem> contentItems)
    {
        _ogTitle = SeoText.TruncateTitle(CollectionTitle ?? _metaTitle, 60);
        _ogDescription = SeoText.TruncateDescription(CollectionDescription ?? _metaDescription, 200);
        // The list's own image, then its first item's, then the site-wide default (issue 291).
        _ogImage = NormalizeImageUrl(SeoText.FirstNonBlank(_pageImageUrl, contentItems.FirstOrDefault()?.FeaturedImageUrl, DefaultImageUrl));
        _ogType = "website";
        _ogImageAlt = CollectionTitle ?? _siteName;
    }

    private void BuildContentOpenGraphMetadata()
    {
        var seo = Content?.Metadata?.SeoProperties;
        _ogTitle = SeoText.TruncateTitle(SeoText.FirstNonBlank(seo?.OgTitle, Content?.Title, CodePageTitle, _metaTitle), 60);
        _ogDescription = SeoText.TruncateDescription(SeoText.FirstNonBlank(seo?.OgDescription, CodePageDescription, _metaDescription), 200);
        _ogImage = NormalizeImageUrl(SeoText.FirstNonBlank(seo?.OgImageUrl, seo?.Image, Content?.FeaturedImageUrl, CodePageImageUrl, DefaultImageUrl));
        _ogType = SeoText.FirstNonBlank(seo?.OgType, DetermineOgType());
        _ogImageAlt = SeoText.FirstNonBlank(Content?.Title, CodePageTitle, _siteName);
    }

    private void BuildTwitterCardMetadata()
    {
        if (ContentItems?.Any() == true)
        {
            // Collection page Twitter Card metadata
            _twitterCard = "summary_large_image";
            _twitterTitle = SeoText.TruncateTitle(_ogTitle ?? _metaTitle, 70);
            _twitterDescription = SeoText.TruncateDescription(_metaDescription, 200);
            _twitterImage = NormalizeImageUrl(_ogImage);
            return;
        }

        var seo = Content?.Metadata?.SeoProperties;
        _twitterCard = SeoText.FirstNonBlank(seo?.TwitterCard, "summary_large_image");
        _twitterTitle = SeoText.TruncateTitle(SeoText.FirstNonBlank(seo?.TwitterTitle, _ogTitle, _metaTitle), 70);
        _twitterDescription = SeoText.TruncateDescription(SeoText.FirstNonBlank(seo?.TwitterDescription, _metaDescription), 200);
        _twitterImage = NormalizeImageUrl(SeoText.FirstNonBlank(seo?.TwitterImageUrl, _ogImage));
    }

    private void BuildSeoEssentials()
    {
        _canonicalUrl = BuildCanonicalUrl();
        _robotsDirective = BuildRobotsDirective();
    }

    private void BuildGeoOptimization()
    {
        // GEO: Create concise entity summary for AI engines
        var description = Content?.Description ?? CollectionDescription ?? CodePageDescription;

        if (!string.IsNullOrWhiteSpace(description))
        {
            _entitySummary = SeoText.TruncateDescription(description, 280);
        }
    }

    /// <summary>
    /// AEO: records the question the page answers, either the title itself when it is a question, or a question
    /// derived from the title pattern ("Guide to X" becomes "How to X?"), with the description as its answer.
    /// </summary>
    private void BuildAeoOptimization()
    {
        _answersTo = [];
        _structuredAnswers = [];

        var title = Content?.Title ?? CollectionTitle ?? CodePageTitle;
        if (string.IsNullOrWhiteSpace(title)) return;

        var question = AnswerQuestions.IsQuestion(title, _languageCode)
            ? title
            : AnswerQuestions.GenerateImplicitQuestion(title, _languageCode);
        if (string.IsNullOrWhiteSpace(question)) return;

        _answersTo.Add(question);

        var description = Content?.Description ?? CollectionDescription ?? CodePageDescription;
        if (!string.IsNullOrWhiteSpace(description))
        {
            _structuredAnswers[question] = SeoText.TruncateDescription(description, 320) ?? string.Empty;
        }
    }

    #endregion

    #region Rendering Methods

    private static string RenderMetaTag(string name, string? content)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // HTML encode content to prevent XSS
        var encodedContent = System.Net.WebUtility.HtmlEncode(content);
        return $"<meta name=\"{name}\" content=\"{encodedContent}\" />";
    }

    private static string RenderPropertyTag(string property, string? content)
    {
        if (string.IsNullOrWhiteSpace(property) || string.IsNullOrWhiteSpace(content))
            return string.Empty;

        var encodedContent = System.Net.WebUtility.HtmlEncode(content);
        return $"<meta property=\"{property}\" content=\"{encodedContent}\" />";
    }

    private static string RenderMetaTag(string name, DateTime content)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return $"<meta name=\"{name}\" content=\"{content:yyyy-MM-dd}\" />";
    }

    #endregion

    #region Title and URL Builders

    private string BuildMetaTitle()
    {
        if (ContentItems?.Any() == true && !string.IsNullOrWhiteSpace(CollectionTitle))
        {
            // Collection page title
            return $"{CollectionTitle} | {_siteName}";
        }

        var title = SeoText.FirstNonBlank(Content?.Metadata?.SeoProperties?.Title, Content?.Title, CodePageTitle);

        if (string.IsNullOrWhiteSpace(title))
            return _siteName;

        // Never cut: Google shortens a title link itself "as needed, typically to fit the device width", and a cut
        // here took the site name with it. Open Graph and Twitter titles keep their own lengths below.
        return $"{title} | {_siteName}";
    }

    private string? BuildCanonicalUrl()
    {
        var declared = Content is null ? CodePageCanonicalUrl() : Content.Metadata?.SeoProperties?.Canonical;
        var canonical = !string.IsNullOrWhiteSpace(declared) ? declared : _currentUrl;

        // og:url and the structured data url must agree with the canonical URL.
        if (!string.IsNullOrWhiteSpace(canonical))
        {
            _currentUrl = canonical;
        }

        return string.IsNullOrWhiteSpace(canonical) ? null : canonical;
    }

    private string BuildRobotsDirective()
    {
        if (!AllowSearchIndexing)
            return "noindex, nofollow";

        if (!string.IsNullOrWhiteSpace(Content?.Metadata?.SeoProperties?.Robots))
            return Content.Metadata.SeoProperties.Robots;

        if (!string.IsNullOrWhiteSpace(Robots))
        {
            if (RobotsDirectives.Normalize(Robots) is { } directives)
                return directives;

            Logger.LogWarning("Ignoring Robots '{Robots}'; it is not a list of robots directives", Robots);
        }

        // Default: index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1
        return "index, follow, max-snippet:-1, max-image-preview:large, max-video-preview:-1";
    }

    /// <summary>
    /// The policy token of <see cref="ReferrerPolicy"/> in lower case, or <see langword="null"/> when none is set or
    /// it is not a token of the Referrer Policy specification, which a browser would ignore.
    /// </summary>
    private string? ResolveReferrerPolicy()
    {
        if (string.IsNullOrWhiteSpace(ReferrerPolicy))
            return null;

        var token = ReferrerPolicy.Trim().ToLowerInvariant();
        if (ReferrerPolicyTokens.Contains(token))
            return token;

        Logger.LogWarning("Skipping the referrer meta tag; '{ReferrerPolicy}' is not a referrer policy", ReferrerPolicy);
        return null;
    }

    /// <summary>
    /// The canonical address <see cref="CanonicalPath"/> names for a page that is not a content item: an absolute http(s)
    /// address as it is, a path on the site after the site address. Anything else is ignored with a logged warning, so
    /// the current address stays canonical.
    /// </summary>
    private string? CodePageCanonicalUrl()
    {
        if (!_isCodePage || string.IsNullOrWhiteSpace(CanonicalPath))
            return null;

        if (Uri.TryCreate(CanonicalPath, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttps || absolute.Scheme == Uri.UriSchemeHttp))
            return CanonicalPath;

        if (LocalUrl.IsLocalPath(CanonicalPath))
            return _baseUrl + CanonicalPath;

        Logger.LogWarning("Ignoring CanonicalPath '{CanonicalPath}'; expected a path on the site or an http(s) address", CanonicalPath);
        return null;
    }

    private string? NormalizeImageUrl(string? url) => SeoText.NormalizeImageUrl(url, _baseUrl);

    private string DetermineOgType() => OgTypesBySchemaType.GetValueOrDefault(PrimarySchemaTypeName, "website");

    #endregion

    #region Values Shown in the Markup

    /// <summary>
    /// The name of the first schema type, else the front matter type, else <see cref="FallbackSchemaType"/>, else
    /// WebPage. An empty <see cref="SchemaTypes"/> array, or a front matter type that is blank or names no
    /// <see cref="Cms.Web.Components.SchemaType"/>, counts as not set, as in <see cref="ResolveSchemaTypes"/>, so
    /// og:type and content-type agree with the JSON-LD.
    /// </summary>
    private string PrimarySchemaTypeName =>
        (_isCodePage
            ? CodePageSchemaType()
            : SchemaTypes is { Length: > 0 } schemaTypes
                ? schemaTypes[0]
                : DeclaredSchemaType(Content) ?? FallbackSchemaType ?? Cms.Web.Components.SchemaType.WebPage)
        .ToString();

    // The values of a page that is not a content item, null on every other page, so they never fill in for an item.
    private string? CodePageTitle => _isCodePage ? Title : null;

    private string? CodePageDescription => _isCodePage ? Description : null;

    private string? CodePageImageUrl => _isCodePage ? ImageUrl : null;

    private bool HasCategories => _categories?.Any() == true;

    private string? PublishedTimeUtc => _publishDate is { } published ? ContentUrls.FormatUtc(published) : null;

    private string? ModifiedTimeUtc => _modifiedDate is { } modified ? ContentUrls.FormatUtc(modified) : null;

    private string ContentFreshness =>
        _publishDate is { } published && (DateTime.UtcNow - published).TotalDays < 30 ? "recent" : "evergreen";

    #endregion

    #region JSON-LD Generation

    private SeoSchemaContext CreateSchemaContext() => new()
    {
        Content = Content,
        ContentItems = ContentItems,
        SiteName = _siteName,
        BaseUrl = _baseUrl,
        CurrentUrl = _currentUrl,
        LanguageCode = _languageCode,
        SiteLogoUrl = SiteLogoUrl,
        AuthorUrl = string.IsNullOrWhiteSpace(AuthorUrl) ? null : SeoText.NormalizeImageUrl(AuthorUrl, _baseUrl),
        BreadcrumbHomeText = BreadcrumbHomeText,
        CustomBreadcrumbs = CustomBreadcrumbs,
        CollectionTitle = CollectionTitle,
        CollectionDescription = CollectionDescription,
        MetaTitle = _metaTitle,
        MetaDescription = _metaDescription,
        PageTitle = CodePageTitle,
        PageDescription = CodePageDescription,
        OgImage = _ogImage,
        ApplicationCategory = ApplicationCategory,
        LinkToSite = SiteSchemasEnabled
    };

    /// <summary>Whether the site's own structured data is configured (<see cref="SeoMetadataOptions.EnableSiteSchemas"/>).</summary>
    private bool SiteSchemasEnabled => SeoOptions?.Value.EnableSiteSchemas == true;

    /// <summary>
    /// Whether the page is the home page: the site root, or on a site with locale routing a culture's root (<c>/sr</c>).
    /// The site's <c>WebSite</c> and <c>Organization</c> are written there.
    /// </summary>
    private bool IsHomePage()
    {
        var path = "/" + navigationManager.ToBaseRelativePath(ContentUrls.StripQueryAndFragment(navigationManager.Uri));
        return (_localePaths?.WithoutLocale(path) ?? path) == "/";
    }

    /// <summary>
    /// On the home page, with the site's own structured data on, writes the site's <c>WebSite</c> and
    /// <c>Organization</c> after the page's own schemas. The page writes neither type itself there (see
    /// <see cref="ResolveSchemaTypes"/>), so neither is written twice.
    /// </summary>
    private void AddSiteSchemas()
    {
        if (!SiteSchemasEnabled || !IsHomePage())
        {
            return;
        }

        var options = SeoOptions!.Value;
        var context = CreateSchemaContext();
        _additionalJsonLdSchemas ??= [];
        _additionalJsonLdSchemas.Add(SeoSchemas.Serialize(SiteSchemas.WebSite(context, SiteDescription, options.SearchUrlTemplate)));
        _additionalJsonLdSchemas.Add(SeoSchemas.Serialize(SiteSchemas.Organization(
            context, SeoText.FirstNonBlank(OrganizationName, _siteName)!, options.SameAs)));
    }

    private void GenerateAllJsonLdSchemas()
    {
        var context = CreateSchemaContext();

        if (_isCodePage)
        {
            _jsonLdContent = SeoSchemas.Serialize(ContentSchemas.CodePage(context, CodePageSchemaType().ToString()), context);
            return;
        }

        // Collection pages: without Content, OnParametersSet only gets here for a code page or when ContentItems has items
        if (Content is null)
        {
            GenerateCollectionJsonLd(context);
            return;
        }

        if (TryUseCustomJsonLd(Content))
            return;

        _additionalJsonLdSchemas = [];

        // First schema becomes primary, rest go to additional
        var generatedSchemas = ResolveSchemaTypes(Content)
            .Select(schemaType => SeoSchemas.Create(schemaType, context))
            .OfType<object>()
            .Select(schema => SeoSchemas.Serialize(schema, context))
            .ToList();
        if (generatedSchemas.Count > 0)
        {
            _jsonLdContent = generatedSchemas[0];
            _additionalJsonLdSchemas.AddRange(generatedSchemas.Skip(1));
        }

        GenerateCollectionItemSchemas(context);
    }

    /// <summary>
    /// The schema type of a page that is not a content item: the first web page type (<c>WebPage</c>,
    /// <c>ContactPage</c> or <c>AboutPage</c>) in <see cref="SchemaTypes"/>, then <see cref="FallbackSchemaType"/> when
    /// it is one, then <c>WebPage</c>. The other types describe an item the page does not have.
    /// </summary>
    private SchemaType CodePageSchemaType()
    {
        foreach (var schemaType in SchemaTypes ?? [])
        {
            if (CodePageSchemaTypes.Contains(schemaType))
                return schemaType;
        }

        return FallbackSchemaType is { } fallback && CodePageSchemaTypes.Contains(fallback)
            ? fallback
            : Cms.Web.Components.SchemaType.WebPage;
    }

    /// <summary>
    /// A list page writes the <c>CollectionPage</c> and the <c>BreadcrumbList</c> its <see cref="SchemaTypes"/> name,
    /// in their order, the first as the primary schema, then one article per item. The other types describe a single
    /// item and have no list page form.
    /// </summary>
    private void GenerateCollectionJsonLd(SeoSchemaContext context)
    {
        _additionalJsonLdSchemas = [];

        var pageSchemas = (SchemaTypes ?? [])
            .Distinct()
            .Where(type => type is Cms.Web.Components.SchemaType.CollectionPage or Cms.Web.Components.SchemaType.BreadcrumbList)
            .Select(type => SeoSchemas.Serialize(SeoSchemas.Create(type, context)!, context))
            .ToList();
        if (pageSchemas.Count > 0)
        {
            _jsonLdContent = pageSchemas[0];
            _additionalJsonLdSchemas.AddRange(pageSchemas.Skip(1));
        }

        GenerateCollectionItemSchemas(context);
    }

    /// <summary>
    /// Uses the custom JSON-LD from the front matter when it has one. It is written inside a script element, so it
    /// is re-serialized with HTML-sensitive characters escaped; anything that is not a JSON object or array is ignored.
    /// </summary>
    private bool TryUseCustomJsonLd(ContentItem content)
    {
        var customJsonLd = content.Metadata?.SeoProperties?.JsonLd;
        if (string.IsNullOrWhiteSpace(customJsonLd))
            return false;

        if (JsonLdNormalizer.TryNormalize(customJsonLd, out var normalizedJsonLd))
        {
            _jsonLdContent = normalizedJsonLd;
            return true;
        }

        Logger.LogWarning(
            "Ignoring custom JSON-LD for content {ContentPath} because it is not a valid JSON object or array; generated structured data is used instead",
            content.Path);
        return false;
    }

    /// <summary>
    /// The schema types to generate: the explicit <see cref="SchemaTypes"/>, else the front matter type, else
    /// <see cref="FallbackSchemaType"/>, else WebPage. A single type gets a BreadcrumbList added when
    /// <see cref="GenerateMultipleSchemas"/> is on.
    /// </summary>
    private List<SchemaType> ResolveSchemaTypes(ContentItem content)
    {
        List<SchemaType> schemaTypes = SchemaTypes?.Any() == true
            ? [.. SchemaTypes]
            : [DeclaredSchemaType(content) ?? FallbackSchemaType ?? Cms.Web.Components.SchemaType.WebPage];

        // When several schemas are given explicitly, the caller chose them all, so nothing is added.
        var shouldAutoAddBreadcrumbs = GenerateMultipleSchemas &&
                                       schemaTypes.Count == 1 &&
                                       content.Path != null &&
                                       !schemaTypes.Contains(Cms.Web.Components.SchemaType.BreadcrumbList);

        if (shouldAutoAddBreadcrumbs)
        {
            schemaTypes.Add(Cms.Web.Components.SchemaType.BreadcrumbList);
        }

        // The home page gets the site's own WebSite and Organization (AddSiteSchemas); a second copy of either
        // without its @id would describe the site twice.
        if (SiteSchemasEnabled && IsHomePage())
        {
            schemaTypes.RemoveAll(type => type is Cms.Web.Components.SchemaType.WebSite or Cms.Web.Components.SchemaType.Organization);
        }

        return schemaTypes;
    }

    /// <summary>
    /// Generates an article for each item in a collection (blog, category and tag pages), of the type
    /// <see cref="ListedItemSchemaType"/> resolves.
    /// </summary>
    private void GenerateCollectionItemSchemas(SeoSchemaContext context)
    {
        if (ContentItems?.Any() != true)
            return;

        var pageItemType = SchemaTypes?.Where(ArticleSchemaTypes.Contains).Select(type => (SchemaType?)type).FirstOrDefault();
        _additionalJsonLdSchemas ??= [];
        _additionalJsonLdSchemas.AddRange(ContentItems.Select(item => SeoSchemas.Serialize(
            NavigationSchemas.CollectionItemPosting(item, context, ListedItemSchemaType(item, pageItemType).ToString()), context)));
    }

    /// <summary>
    /// The type of a listed item: its front matter type when that is an article type, then the article type the page
    /// names, then Article. A list holds case studies and documentation as well as blog posts, so BlogPosting is
    /// written only when the item or the page says so.
    /// </summary>
    private static SchemaType ListedItemSchemaType(ContentItem item, SchemaType? pageItemType)
    {
        if (DeclaredSchemaType(item) is { } itemType && ArticleSchemaTypes.Contains(itemType))
        {
            return itemType;
        }

        return pageItemType ?? Cms.Web.Components.SchemaType.Article;
    }

    /// <summary>
    /// The schema type the item's front matter names, read ignoring case, or null when the front matter names none or
    /// a name that is not a <see cref="Cms.Web.Components.SchemaType"/> (for example NewsArticle).
    /// </summary>
    private static SchemaType? DeclaredSchemaType(ContentItem? content) =>
        Enum.TryParse<SchemaType>(content?.Metadata?.SeoProperties?.Type, ignoreCase: true, out var declaredType)
            ? declaredType
            : null;

    /// <summary>
    /// Adds a schema.org FAQPage built from the generated questions. Only runs when
    /// <see cref="EnableGeneratedFaq"/> is on, because the questions are derived from titles, not authored. A page
    /// with a <c>faq</c> content block gets none: the block writes the FAQPage from its authored questions, and a page
    /// has one FAQPage.
    /// </summary>
    private void AddGeneratedFaqSchema()
    {
        if (!EnableAeoOptimization || !EnableGeneratedFaq || _structuredAnswers is not { Count: > 0 }
            || Content?.Blocks.OfType<FaqBlock>().Any() == true)
        {
            return;
        }

        _additionalJsonLdSchemas ??= [];
        _additionalJsonLdSchemas.Add(SeoSchemas.Serialize(NavigationSchemas.FaqPage(_structuredAnswers)));
    }

    #endregion
}

/// <summary>
/// Defines the types of Schema.org structured data schemas that can be generated for SEO purposes.
/// </summary>
public enum SchemaType
{
    /// <summary>
    /// Schema.org Article type for news articles, blog posts, and editorial content.
    /// </summary>
    Article,

    /// <summary>
    /// Schema.org BlogPosting type specifically for blog posts.
    /// </summary>
    BlogPosting,

    /// <summary>
    /// Schema.org WebPage type for general web pages.
    /// </summary>
    WebPage,

    /// <summary>
    /// Schema.org Product type for e-commerce products and items for sale.
    /// </summary>
    Product,

    /// <summary>
    /// Schema.org Organization type for companies, businesses, and organizations.
    /// </summary>
    Organization,

    /// <summary>
    /// Schema.org Person type for individual people and profiles.
    /// </summary>
    Person,

    /// <summary>
    /// Schema.org LocalBusiness type for physical business locations with address and hours.
    /// </summary>
    LocalBusiness,

    /// <summary>
    /// Schema.org Event type for events, conferences, and gatherings.
    /// </summary>
    Event,

    /// <summary>
    /// Schema.org Recipe type for cooking recipes with ingredients and instructions.
    /// </summary>
    Recipe,

    /// <summary>
    /// Schema.org FAQPage type for frequently asked questions pages. Generates nothing on its own; use
    /// <c>EnableGeneratedFaq</c> or the <c>OsirionFaq</c> component. Google no longer shows FAQ rich results
    /// (since May 2026).
    /// </summary>
    FAQ,

    /// <summary>
    /// Schema.org HowTo type for step-by-step instructions and tutorials. Google no longer shows HowTo rich results
    /// (since September 2023).
    /// </summary>
    HowTo,

    /// <summary>
    /// Schema.org VideoObject type for video content.
    /// </summary>
    VideoObject,

    /// <summary>
    /// Schema.org Course type for educational courses and training programs.
    /// </summary>
    Course,

    /// <summary>
    /// Schema.org Review type for product or service reviews.
    /// </summary>
    Review,

    /// <summary>
    /// Schema.org BreadcrumbList type for navigation breadcrumbs.
    /// </summary>
    BreadcrumbList,

    /// <summary>
    /// Schema.org WebSite type with SearchAction for homepages and main site pages.
    /// </summary>
    WebSite,

    /// <summary>
    /// Schema.org CollectionPage type for pages listing multiple items (blog index, category, tag pages).
    /// </summary>
    CollectionPage,

    /// <summary>
    /// Schema.org TechArticle type for technical documentation and how-to articles: the Article schema with a more
    /// specific type.
    /// </summary>
    TechArticle,

    /// <summary>
    /// Schema.org SoftwareApplication type for a product page about an app or a library, with the
    /// <c>applicationCategory</c> from <see cref="SeoMetadataRenderer.ApplicationCategory"/>.
    /// </summary>
    SoftwareApplication,

    /// <summary>
    /// Schema.org ProfilePage type for an author's page, with the person as its main entity. The author (or the
    /// title) is the person's name, and tags that are absolute links become <c>sameAs</c> profiles.
    /// </summary>
    ProfilePage,

    /// <summary>
    /// Schema.org AboutPage type, with the site's organization as its main entity.
    /// </summary>
    AboutPage,

    /// <summary>
    /// Schema.org ContactPage type for a contact page.
    /// </summary>
    ContactPage
}
