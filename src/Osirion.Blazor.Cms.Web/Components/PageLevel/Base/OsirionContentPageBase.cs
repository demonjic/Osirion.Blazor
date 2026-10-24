using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Components;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Components;

namespace Osirion.Blazor.Cms.Web.Components;

/// <summary>
/// Base class for content page components in the Osirion CMS.
/// Provides common parameters and properties for displaying content, hero sections, breadcrumbs, categories, tags, and loading states.
/// </summary>
public abstract partial class OsirionContentPageBase : OsirionComponentBase
{
    /// <summary>
    /// The locale of the content the page looks up (<c>en</c> by default): the item found with <c>ItemPath</c>,
    /// <c>ItemUrl</c> or <c>ItemSlug</c>, and the related items, must have it. Used by <c>ArticlePage</c>,
    /// <c>AboutPage</c>, <c>ContactPage</c> and <c>ContentListPage</c>. <c>HomePage</c> and <c>DocumentPage</c> show
    /// the item you pass as <c>Content</c>; <c>HomePage</c> lists the featured and recent items of this locale.
    /// </summary>
    [Parameter]
    public string? Locale { get; set; } = "en";

    /// <summary>
    /// A complete content query that finds the item in place of <c>ItemPath</c>, <c>ItemUrl</c>, <c>ItemSlug</c>,
    /// <see cref="Locale"/> and <see cref="DirectoryName"/>; the first match is shown. Used by <c>ArticlePage</c>,
    /// <c>AboutPage</c> and <c>ContactPage</c>. Changing it alone does not make a displayed page load again.
    /// </summary>
    [Parameter]
    public ContentQuery? Query { get; set; }

    #region SEO Metadata

    /// <summary>
    /// The schema.org types written as the page's structured data, passed to <c>SeoMetadataRenderer</c>. When it is
    /// null, <c>AboutPage</c> uses <c>AboutPage</c>, <c>ContactPage</c> uses <c>ContactPage</c>, <c>DocumentPage</c>
    /// uses <c>TechArticle</c> and the other pages leave the choice to the renderer; <c>ArticlePage</c> then writes
    /// <c>Article</c> when neither the configured <c>DefaultSchemaTypes</c> nor the front matter <c>type</c> names one.
    /// <c>DocumentPage</c> also takes the first value as the article's microdata type.
    /// </summary>
    [Parameter]
    public SchemaType[]? SchemaTypes { get; set; }

    /// <summary>
    /// Whether to write more than one structured data type when it applies, for example the main type and a
    /// breadcrumb list. Used by <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c>, <c>ContactPage</c> and
    /// <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public bool GenerateMultipleSchemas { get; set; } = true;

    /// <summary>
    /// The site name that structured data and titles use, passed to <c>SeoMetadataRenderer</c> as
    /// <c>SiteNameOverride</c>. Used by every page component except <c>LandingPage</c>; <c>ContentListPage</c> passes
    /// it to its list.
    /// </summary>
    [Parameter]
    public string? WebsiteName { get; set; }

    /// <summary>
    /// The site description, passed to <c>SeoMetadataRenderer</c>, which uses it as the feed description when the feed options set none. Without it the value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies.
    /// </summary>
    [Parameter]
    public string? SiteDescription { get; set; }

    /// <summary>
    /// The address of the site logo for the organization's structured data (square, at least 112 by 112 pixels is
    /// recommended). Used by <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c>, <c>ContactPage</c> and
    /// <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string? SiteLogoUrl { get; set; }

    /// <summary>
    /// The handle of the site for <c>twitter:site</c>, for example <c>@contoso</c>, passed to <c>SeoMetadataRenderer</c>. Without it the value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies.
    /// </summary>
    [Parameter]
    public string? TwitterSite { get; set; }

    /// <summary>
    /// The handle of the author for <c>twitter:creator</c>, for example <c>@author</c>, passed to <c>SeoMetadataRenderer</c>. Without it the value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies.
    /// </summary>
    [Parameter]
    public string? TwitterCreator { get; set; }

    /// <summary>
    /// The Facebook app ID written as <c>fb:app_id</c>, passed to <c>SeoMetadataRenderer</c>. Without it the value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies.
    /// </summary>
    [Parameter]
    public string? FacebookAppId { get; set; }

    /// <summary>
    /// Whether the crawler opt-in meta tags are written (<c>true</c>) or the opt-out tags (<c>false</c>), passed to <c>SeoMetadataRenderer</c> only when you set it. Without it the configured value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies, which is <c>true</c> unless changed.
    /// </summary>
    [Parameter]
    public bool AllowAiDiscovery { get; set; } = true;

    /// <summary>
    /// Whether the training tags opt in (<c>true</c>) or out (<c>false</c>) of AI training, passed to <c>SeoMetadataRenderer</c> only when you set it. Without it the configured value applies. It has no effect when <c>AllowAiDiscovery</c> is <c>false</c>.
    /// </summary>
    [Parameter]
    public bool AllowAiTraining { get; set; } = true;

    /// <summary>
    /// Whether search engines may index the page, passed to <c>SeoMetadataRenderer</c> only when you set it. <c>false</c> writes <c>noindex, nofollow</c> as the robots tags. Without it the configured value applies.
    /// </summary>
    [Parameter]
    public bool AllowSearchIndexing { get; set; } = true;

    /// <summary>
    /// Whether the summary meta tags (<c>entity-summary</c>, <c>content-type</c> and the like) are written, when the item has a description, passed to <c>SeoMetadataRenderer</c> only when you set it. Without it the configured value applies.
    /// </summary>
    [Parameter]
    public bool EnableGeoOptimization { get; set; } = true;

    /// <summary>
    /// Whether the generated answer tags are allowed, passed to <c>SeoMetadataRenderer</c> only when you set it. Without it the configured value applies.
    /// </summary>
    [Parameter]
    public bool EnableAeoOptimization { get; set; } = true;

    /// <summary>
    /// The image of the social cards for a page whose item has no featured image, passed to <c>SeoMetadataRenderer</c>. Without it the value of <c>AddOsirionSeoMetadata</c> or the <c>Osirion:Seo</c> section applies.
    /// </summary>
    [Parameter]
    public string? DefaultImageUrl { get; set; }

    /// <summary>
    /// The width in pixels of <see cref="DefaultImageUrl"/>, passed to <c>SeoMetadataRenderer</c>. The default <c>1200</c> yields to the configured width.
    /// </summary>
    [Parameter]
    public int DefaultImageWidth { get; set; } = 1200;

    /// <summary>
    /// The height in pixels of <see cref="DefaultImageUrl"/>, passed to <c>SeoMetadataRenderer</c>. The default <c>630</c> yields to the configured height.
    /// </summary>
    [Parameter]
    public int DefaultImageHeight { get; set; } = 630;

    /// <summary>
    /// The organization name for the structured data. Used by <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c>,
    /// <c>ContactPage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Name and address pairs that replace the breadcrumb list in the page's structured data. They do not change the
    /// visible breadcrumbs. Used by <c>ArticlePage</c> and <c>HomePage</c>.
    /// </summary>
    [Parameter]
    public List<(string Name, string Url)>? CustomBreadcrumbs { get; set; }

    /// <summary>
    /// The address of the previous page in a series, written as a <c>rel="prev"</c> link in the head. Used by
    /// <c>ArticlePage</c>, <c>HomePage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string? PrevPageUrl { get; set; }

    /// <summary>
    /// The address of the next page in a series, written as a <c>rel="next"</c> link in the head. Used by
    /// <c>ArticlePage</c>, <c>HomePage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string? NextPageUrl { get; set; }

    /// <summary>
    /// The addresses of the page's other language versions, written as <c>hreflang</c> links in the head. Used by
    /// every page component that writes head metadata except <c>LandingPage</c>.
    /// </summary>
    [Parameter]
    public List<string>? AlternateLanguageUrls { get; set; }

    #endregion

    #region Loading and Fetching

    /// <summary>
    /// Gets or sets whether the component is loading
    /// </summary>
    protected bool IsLoading { get; set; } = false;

    /// <summary>
    /// The text beside the spinner while an interactive render waits for the item (for <c>HomePage</c>, for its featured
    /// and recent items). A statically rendered response is sent once they have loaded, so it never contains it. Used by <c>ArticlePage</c>, <c>HomePage</c>,
    /// <c>DocumentPage</c>, <c>AboutPage</c> and <c>ContactPage</c>; <c>LandingPage</c> has its own.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading article...";

    /// <summary>
    /// Whether the loading spinner shows <see cref="LoadingText"/>.
    /// </summary>
    [Parameter]
    public bool ShowLoadingText { get; set; } = true;

    /// <summary>
    /// The heading of the not-found state, shown when the page finds no item. Used by <c>ArticlePage</c>,
    /// <c>HomePage</c>, <c>DocumentPage</c> and <c>AboutPage</c>.
    /// </summary>
    [Parameter]
    public string NotFoundTitle { get; set; } = "Page Not Found";

    /// <summary>
    /// The message of the not-found state. <c>LandingPage</c> has a parameter of its own.
    /// </summary>
    [Parameter]
    public string NotFoundText { get; set; } = "The page you're looking for doesn't exist or has been moved.";

    /// <summary>
    /// Whether the not-found state has a button back, to <see cref="BackLinkUrl"/> with the text <see
    /// cref="BackLinkText"/>. It is not shown when the address is empty.
    /// </summary>
    [Parameter]
    public bool ShowBackLink { get; set; } = true;

    /// <summary>
    /// The address of the back link in the not-found state. It is written as given, so build it from values you
    /// control.
    /// </summary>
    [Parameter]
    public string BackLinkUrl { get; set; } = "/";

    /// <summary>
    /// The text of the back link in the not-found state.
    /// </summary>
    [Parameter]
    public string BackLinkText { get; set; } = "Go to Homepage";

    #endregion

    #region Hero

    /// <summary>
    /// Whether the page's title and description appear in a <c>HeroSection</c> styled by the other <c>Hero</c>
    /// parameters. When it is off, <c>ArticlePage</c> shows no heading unless <c>HeaderContent</c> is set, while
    /// <c>AboutPage</c> and <c>ContactPage</c> show a plain <c>OsirionPageHeading</c>. <c>AboutPage</c> and
    /// <c>ContactPage</c> start with it off. <c>DocumentPage</c> never shows a hero.
    /// </summary>
    [Parameter]
    public bool ShowHero { get; set; } = true;

    /// <summary>
    /// Whether the hero image is the hero's background instead of a picture beside the text. Used by
    /// <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c> and <c>ContactPage</c> while <see cref="ShowHero"/> is
    /// on.
    /// </summary>
    [Parameter]
    public bool UseHeroBackgroundImage { get; set; }

    /// <summary>
    /// Whether an overlay darkens the hero's background image so the text stays readable. It applies only with <see
    /// cref="UseHeroBackgroundImage"/>.
    /// </summary>
    [Parameter]
    public bool HasOverlay { get; set; }

    /// <summary>
    /// The hero's image. <c>ArticlePage</c> falls back to the item's featured image when it is not set;
    /// <c>AboutPage</c> and <c>ContactPage</c> show no image without it; <c>HomePage</c> falls back to the item's
    /// featured image, as <c>ArticlePage</c> does.
    /// </summary>
    [Parameter]
    public string? HeroImageUrl { get; set; }

    /// <summary>
    /// The color mode of the hero: <c>Light</c>, <c>Dark</c> or <c>System</c>. Used by <c>ArticlePage</c>,
    /// <c>HomePage</c>, <c>AboutPage</c> and <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public ThemeMode HeroTheme { get; set; } = ThemeMode.System;

    /// <summary>
    /// How the hero's text is aligned, an <c>Alignment</c> value such as <c>Left</c> or <c>Center</c>. Used by
    /// <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c> and <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public Alignment HeroAlignment { get; set; } = Alignment.Left;

    /// <summary>
    /// Which side of the hero text the image sits on when it is not a background, an <c>Alignment</c> value
    /// (<c>Left</c> or <c>Right</c>). Used by <c>ArticlePage</c>, <c>HomePage</c>, <c>AboutPage</c> and
    /// <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public Alignment HeroImagePosition { get; set; } = Alignment.Right;

    /// <summary>
    /// The hero's layout: <c>Hero</c>, <c>Jumbotron</c> or <c>Minimal</c>. Used by <c>ArticlePage</c>,
    /// <c>HomePage</c>, <c>AboutPage</c> and <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public HeroVariant HeroVariant { get; set; } = HeroVariant.Hero;

    /// <summary>
    /// Whether the hero of <c>AboutPage</c> and <c>ContactPage</c> shows its buttons. <c>ArticlePage</c> and
    /// <c>HomePage</c> ignore it and show a button whenever its text and address are both set.
    /// </summary>
    [Parameter]
    public bool ShowHeroButtons { get; set; } = false;

    /// <summary>
    /// The text of the hero's primary button. The button needs <see cref="HeroPrimaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? HeroPrimaryButtonText { get; set; }

    /// <summary>
    /// The address of the hero's primary button. The button needs <see cref="HeroPrimaryButtonText"/> too.
    /// </summary>
    [Parameter]
    public string? HeroPrimaryButtonUrl { get; set; }

    /// <summary>
    /// The text of the hero's secondary button. The button needs <see cref="HeroSecondaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? HeroSecondaryButtonText { get; set; }

    /// <summary>
    /// The address of the hero's secondary button. The button needs <see cref="HeroSecondaryButtonText"/> too.
    /// </summary>
    [Parameter]
    public string? HeroSecondaryButtonUrl { get; set; }

    /// <summary>
    /// The decorative pattern behind the hero text, or <see langword="null"/> for none. Used by <c>ArticlePage</c>,
    /// <c>HomePage</c>, <c>AboutPage</c> and <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public BackgroundPatternType? HeroBackgroundPattern { get; set; } = BackgroundPatternType.Grid;

    #endregion

    #region Breadcrumbs

    /// <summary>
    /// Whether to show the visible breadcrumbs: <c>OsirionBreadcrumbs</c> in <c>ArticlePage</c>,
    /// <c>ContentBreadcrumbs</c> in <c>DocumentPage</c>. The other pages show none.
    /// </summary>
    [Parameter]
    public bool ShowBreadcrumbs { get; set; } = true;

    /// <summary>
    /// Whether the breadcrumbs start with a home link. Used by <c>ArticlePage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public bool ShowBreadcrumbHome { get; set; } = true;

    /// <summary>
    /// The text of the breadcrumbs' home link. Used by <c>ArticlePage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string BreadcrumbHomeText { get; set; } = "Home";

    /// <summary>
    /// The address of the breadcrumbs' home link. Used by <c>ArticlePage</c> and <c>DocumentPage</c>.
    /// </summary>
    [Parameter]
    public string BreadcrumbHomeUrl { get; set; } = "/";

    #endregion

    #region Directory

    /// <summary>
    /// The name of the directory the item must be in, as a filter of the query that finds it. Used by
    /// <c>ArticlePage</c>, <c>AboutPage</c> and <c>ContactPage</c>.
    /// </summary>
    [Parameter]
    public string? DirectoryName { get; set; }

    /// <summary>
    /// Builds the address of a breadcrumb that stands for a directory. Used by <c>DocumentPage</c>; the other pages
    /// do not read it.
    /// </summary>
    [Parameter]
    public Func<DirectoryItem, string>? DirectoryUrlFormatter { get; set; }

    #endregion

    #region Categories

    /// <summary>
    /// Whether the sidebar of <c>ArticlePage</c> lists the item's categories. The section appears only when the item
    /// has categories.
    /// </summary>
    [Parameter]
    public bool ShowCategoriesSidebar { get; set; } = true;

    /// <summary>
    /// The heading of the category list: in the sidebar of <c>ArticlePage</c>, and in the browse section of
    /// <c>HomePage</c>.
    /// </summary>
    [Parameter]
    public string CategoriesSectionTitle { get; set; } = "Categories";

    /// <summary>
    /// Builds a category link from the category name. Used by <c>ArticlePage</c> (sidebar) and <c>HomePage</c>
    /// (browse section); without it the link is <c>/category/{slug}</c>.
    /// </summary>
    [Parameter]
    public Func<string, string>? CategoryUrlFormatter { get; set; }

    #endregion

    #region Tags

    /// <summary>
    /// Whether the sidebar of <c>ArticlePage</c> lists the item's tags. The section appears only when the item has
    /// tags.
    /// </summary>
    [Parameter]
    public bool ShowTagsSidebar { get; set; } = true;

    /// <summary>
    /// The heading of the tag list: in the sidebar of <c>ArticlePage</c>, and in the browse section of
    /// <c>HomePage</c>.
    /// </summary>
    [Parameter]
    public string TagsSectionTitle { get; set; } = "Tags";

    /// <summary>
    /// Builds a tag link from the tag name. Used by <c>ArticlePage</c> (sidebar) and <c>HomePage</c> (browse
    /// section); without it the link is <c>/tag/{slug}</c>.
    /// </summary>
    [Parameter]
    public Func<string, string>? TagUrlFormatter { get; set; }

    /// <summary>
    /// <see cref="CategoryUrlFormatter"/> in the shape <c>CategoriesList</c> takes, or <see langword="null"/>.
    /// </summary>
    protected Func<ContentCategory, string>? CategoryListUrlFormatter =>
        CategoryUrlFormatter is { } formatter ? category => formatter(category.Name) : null;

    /// <summary>
    /// <see cref="TagUrlFormatter"/> in the shape <c>TagCloud</c> takes, or <see langword="null"/>.
    /// </summary>
    protected Func<ContentTag, string>? TagCloudUrlFormatter =>
        TagUrlFormatter is { } formatter ? tag => formatter(tag.Name) : null;

    #endregion

    /// <summary>Performs the ContentProviderManager operation.</summary>
    [Inject]
    protected IContentProviderManager ContentProviderManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the navigation manager
    /// </summary>
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = default!;

    private ILogger? _logger;

    /// <summary>Gets the logger for the concrete page type.</summary>
    protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

#if !NET10_0_OR_GREATER
    /// <summary>The request context; only available during static server-side rendering.</summary>
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
#endif

    private readonly NotFoundSignal _notFound = new();

    /// <summary>
    /// Tells the framework the requested content does not exist so a statically rendered response carries
    /// HTTP 404 instead of a 200 "soft 404". It has no effect once the response has started, which is why the page
    /// components do not use streaming rendering, and none during interactive rendering, where the not-found markup
    /// the component renders is all that can be shown.
    /// </summary>
    protected void SignalNotFound()
    {
#if NET10_0_OR_GREATER
        _notFound.Send(NavigationManager);
#else
        _notFound.Send(HttpContext);
#endif
    }

    /// <summary>Allows the next not-found result to signal again after content was found.</summary>
    protected void ResetNotFoundSignal() => _notFound.Reset();

    /// <summary>
    /// Renders the page's title as its <c>h1</c>: in a <see cref="HeroSection"/> styled by the hero parameters while
    /// <see cref="ShowHero"/> is on, otherwise in an <see cref="OsirionPageHeading"/>, with the description below the
    /// title.
    /// </summary>
    /// <param name="title">The page title.</param>
    /// <param name="description">The sentence under the title, or <see langword="null"/> for none.</param>
    protected RenderFragment RenderPageHeading(string title, string? description) => builder =>
    {
        if (ShowHero)
        {
            builder.OpenComponent<HeroSection>(0);
            builder.AddComponentParameter(1, nameof(HeroSection.Title), title);
            builder.AddComponentParameter(2, nameof(HeroSection.Subtitle), description);
            builder.AddComponentParameter(3, nameof(HeroSection.Variant), HeroVariant);
            builder.AddComponentParameter(4, nameof(HeroSection.Alignment), HeroAlignment);
            builder.AddComponentParameter(5, nameof(HeroSection.BackgroundPattern), HeroBackgroundPattern);
            builder.AddComponentParameter(6, nameof(HeroSection.Theme), HeroTheme);
            builder.AddComponentParameter(7, nameof(HeroSection.ImageUrl), HeroImageUrl);
            builder.AddComponentParameter(8, nameof(HeroSection.ImageAlt), title);
            builder.AddComponentParameter(9, nameof(HeroSection.ImagePosition), HeroImagePosition);
            builder.AddComponentParameter(10, nameof(HeroSection.UseBackgroundImage), UseHeroBackgroundImage);
            builder.AddComponentParameter(11, nameof(HeroSection.HasOverlay), HasOverlay);
            builder.AddComponentParameter(12, nameof(HeroSection.ShowPrimaryButton), ShowHeroButtons);
            builder.AddComponentParameter(13, nameof(HeroSection.ShowSecondaryButton), ShowHeroButtons);
            builder.AddComponentParameter(14, nameof(HeroSection.PrimaryButtonText), HeroPrimaryButtonText);
            builder.AddComponentParameter(15, nameof(HeroSection.PrimaryButtonUrl), HeroPrimaryButtonUrl);
            builder.AddComponentParameter(16, nameof(HeroSection.SecondaryButtonText), HeroSecondaryButtonText);
            builder.AddComponentParameter(17, nameof(HeroSection.SecondaryButtonUrl), HeroSecondaryButtonUrl);
            builder.AddComponentParameter(18, nameof(HeroSection.HasDivider), false);
            builder.CloseComponent();
            return;
        }

        builder.OpenComponent<OsirionPageHeading>(20);
        builder.AddComponentParameter(21, nameof(OsirionPageHeading.Title), title);
        builder.AddComponentParameter(22, nameof(OsirionPageHeading.Description), description);
        builder.CloseComponent();
    };
}
