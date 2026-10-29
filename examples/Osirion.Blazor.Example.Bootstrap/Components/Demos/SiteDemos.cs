using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Services;

namespace Osirion.Blazor.Example.Bootstrap.Components.Demos;

/// <summary>
/// The live examples the site's Markdown embeds with a fenced <c>demo</c> block. Each demo is a component in this
/// folder; its file is embedded (see the project file), so the page's Code tab shows exactly what compiled.
/// </summary>
internal static class SiteDemos
{
    /// <summary>Registers every demo.</summary>
    /// <param name="demos">The registry from <c>AddOsirionContentDemos</c>.</param>
    public static void Register(ContentDemoRegistry demos)
    {
        Add<InstallTabsDemo>(demos, "install-tabs", "Install command tabs");
        Add<FeatureCardDemo>(demos, "feature-card", "A feature card");
        Add<TestimonialCardDemo>(demos, "testimonial-card", "A testimonial card");
        Add<SubscriptionCardDemo>(demos, "subscription-card", "A subscription card");
        Add<AuthorCardDemo>(demos, "author-card", "An author card");
        Add<MetricCardDemo>(demos, "metric-card", "Metric cards");
        Add<BreadcrumbsDemo>(demos, "breadcrumbs", "Breadcrumb trails");
        Add<ReadMoreLinkDemo>(demos, "read-more-link", "Read more links");
        Add<MenuDemo>(demos, "menu", "A menu with a submenu");
        Add<ArticleMetadataDemo>(demos, "article-metadata", "Article metadata");
        Add<CalloutDemo>(demos, "callout", "Callouts");
        Add<ChecklistDemo>(demos, "checklist", "A checklist");
        Add<AccordionFaqDemo>(demos, "accordion-and-faq", "An accordion and an FAQ");
        Add<ResponsiveImageDemo>(demos, "responsive-image", "A responsive image");
        Add<VideoEmbedDemo>(demos, "video-embed", "A video embed");
        Add<ReadingProgressDemo>(demos, "reading-progress", "A reading progress bar");
        Add<RevealDemo>(demos, "reveal", "Reveal on scroll");
        Add<HtmlRendererDemo>(demos, "html-renderer", "Sanitized HTML with code");
        Add<ContentDemoDemo>(demos, "content-demo", "A registered demo");
        Add<HeroSectionDemo>(demos, "hero-section", "A hero section");
        Add<BaseSectionDemo>(demos, "base-section", "A base section");
        Add<ContactInfoSectionDemo>(demos, "contact-info-section", "A contact info section");
        Add<PricingTableDemo>(demos, "pricing-table", "A pricing table and feature comparison");
        Add<StepsDemo>(demos, "steps", "Numbered steps");
        Add<CallToActionSectionDemo>(demos, "call-to-action-section", "A closing call to action");
        Add<MetricGridDemo>(demos, "metric-grid", "A metric grid");
        Add<ContextCardGridDemo>(demos, "context-card-grid", "A context card grid");
        Add<InfiniteLogoCarouselDemo>(demos, "infinite-logo-carousel", "A logo carousel");
        Add<TestimonialCarouselDemo>(demos, "testimonial-carousel", "A testimonial carousel");
        Add<PageLayoutDemo>(demos, "page-layout", "A page layout in a frame");
        Add<PageLoadingDemo>(demos, "page-loading", "Page loading spinners");
        Add<FooterDemo>(demos, "footer", "A footer");
        Add<StickySidebarDemo>(demos, "sticky-sidebar", "A sticky sidebar");
        Add<BackgroundPatternDemo>(demos, "background-pattern", "Background patterns");
        Add<CategoriesListDemo>(demos, "categories-list", "A categories list");
        Add<TagCloudDemo>(demos, "tag-cloud", "A tag cloud");
        Add<ContentBreadcrumbsDemo>(demos, "content-breadcrumbs", "Content breadcrumbs");
        Add<DirectoryNavigationDemo>(demos, "directory-navigation", "A directory navigation");
        Add<LocalizedNavigationDemo>(demos, "localized-navigation", "A localized navigation tree");
        Add<OsirionContentNavigationDemo>(demos, "osirion-content-navigation", "Previous and next links");
        Add<TableOfContentsDemo>(demos, "table-of-contents", "A table of contents");
        Add<SearchBoxDemo>(demos, "search-box", "A search box");
        Add<ContactFormDemo>(demos, "contact-form", "A contact form that sends nothing");
        Add<ContentNotFoundDemo>(demos, "content-not-found", "A content not found state");
        Add<ThemeToggleDemo>(demos, "theme-toggle", "A theme toggle");
        Add<ThemePickerDemo>(demos, "theme-picker", "A palette picker");
        Add<ContentRendererDemo>(demos, "content-renderer", "A content body");
        Add<ContentViewDemo>(demos, "content-view", "A content view");
        Add<LocalizedContentViewDemo>(demos, "localized-content-view", "A localized content view");
        Add<ContentPageDemo>(demos, "content-page", "A content page in a frame");
    }

    private static void Add<TDemo>(ContentDemoRegistry demos, string key, string title)
        where TDemo : IComponent =>
        demos.Add<TDemo>(key, title: title, source: ContentDemoRegistry.ReadEmbeddedSource(
            typeof(TDemo).Assembly, $"{typeof(TDemo).Namespace}.{typeof(TDemo).Name}.razor"));
}
