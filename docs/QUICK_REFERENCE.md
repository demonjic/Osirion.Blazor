# Quick Reference Guide

[![Documentation](https://img.shields.io/badge/Documentation-Complete-green)](https://github.com/demonjic/Osirion.Blazor/tree/master/docs)
[![Version](https://img.shields.io/nuget/v/Osirion.Blazor)](https://www.nuget.org/packages/Osirion.Blazor)

This guide provides a quick overview of all components available in the Osirion.Blazor ecosystem.

## ?? Quick Setup

```csharp
// In Program.cs - Full setup with fluent API
builder.Services.AddOsirionBlazor(osirion => {
    osirion
        .AddGitHubCms(options => {
            options.Owner = "username";
            options.Repository = "content-repo";
        })
        .AddScrollToTop()
        .AddClarityTracker(options => {
            options.SiteId = "clarity-id";
        })
        .AddOsirionStyle(CssFramework.Bootstrap);
});
```

```razor
@using Osirion.Blazor.Components

<!-- In your layout -->
<OsirionStyles Framework="CssFramework.Bootstrap" />
<EnhancedNavigation Behavior="ScrollBehavior.Smooth" />
<ScrollToTop Position="Position.BottomRight" />
<OsirionCookieConsent PolicyLink="/privacy-policy" />
```

## ?? Core Components

### Layout & Structure

#### HeroSection
**Purpose**: Compelling hero sections for landing pages and articles  
**Package**: `Osirion.Blazor.Core`  
**Documentation**: [Hero Section Guide](HERO_SECTION.md)

```razor
<HeroSection 
    Title="Welcome to Our Platform"
    Subtitle="Build amazing applications"
    PrimaryButtonText="Get Started"
    PrimaryButtonUrl="/start"
    ImageUrl="/hero.jpg"
    UseBackgroundImage="true" />
```

#### OsirionPageLayout
**Purpose**: Complete page layout with header, sidebar, and footer  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionPageLayout HasSidebar="true" HasFooter="true">
    <HeaderContent><!-- Header --></HeaderContent>
    <SidebarContent><!-- Sidebar --></SidebarContent>
    <MainContent><!-- Main content --></MainContent>
    <FooterContent><!-- Footer --></FooterContent>
</OsirionPageLayout>
```

#### OsirionFooter
**Purpose**: Flexible footer component  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionFooter>
    <FooterContent>
        <!-- Custom footer content -->
    </FooterContent>
</OsirionFooter>
```

#### OsirionStickySidebar
**Purpose**: Sidebar that sticks during scrolling  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionStickySidebar Position="Position.Left">
    <!-- Sidebar content -->
</OsirionStickySidebar>
```

#### OsirionBackgroundPattern
**Purpose**: Decorative background patterns  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionBackgroundPattern 
    Pattern="BackgroundPatternType.Dots"
    Opacity="0.1" />
```

### Navigation & Breadcrumbs

#### OsirionBreadcrumbs
**Purpose**: Automatic breadcrumb navigation from URL paths  
**Package**: `Osirion.Blazor.Core`  
**Documentation**: [Breadcrumbs Guide](BREADCRUMBS.md)

```razor
<OsirionBreadcrumbs 
    Path="/blog/web-development/blazor-components"
    ShowHome="true"
    SegmentFormatter="@FormatSegment" />
```

#### EnhancedNavigation
**Purpose**: Enhanced navigation with scroll restoration  
**Package**: `Osirion.Blazor.Navigation`  
**Documentation**: [Navigation Guide](NAVIGATION.md)

```razor
<EnhancedNavigation 
    Behavior="ScrollBehavior.Smooth"
    ResetScrollOnNavigation="true" />
```

#### ScrollToTop
**Purpose**: "Back to top" button  
**Package**: `Osirion.Blazor.Navigation`  
**Documentation**: [Navigation Guide](NAVIGATION.md)

```razor
<ScrollToTop 
    Position="Position.BottomRight"
    Behavior="ScrollBehavior.Smooth"
    VisibilityThreshold="300" />
```

#### Menu Components
**Purpose**: Hierarchical navigation menus  
**Package**: `Osirion.Blazor.Navigation`

```razor
<Menu>
    <MenuGroup Title="Documentation">
        <MenuItem Text="Getting Started" Url="/docs/start" />
        <MenuItem Text="Components" Url="/docs/components" />
        <MenuDivider />
        <MenuItem Text="API Reference" Url="/docs/api" />
    </MenuGroup>
</Menu>
```

### Content & Media

#### OsirionArticleMetadata
**Purpose**: Display article metadata (author, date, read time)  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionArticleMetadata 
    Author="John Doe"
    PublishDate="@DateTime.Now"
    ReadTime="5 min read" />
```

#### OsirionHtmlRenderer
**Purpose**: Safe HTML content rendering  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionHtmlRenderer 
    HtmlContent="@htmlString"
    SanitizeHtml="true" />
```

#### InfiniteLogoCarousel
**Purpose**: Self-contained logo carousel with zero dependencies  
**Package**: `Osirion.Blazor.Core`  
**Documentation**: [Logo Carousel Guide](INFINITE_LOGO_CAROUSEL.md)

```razor
<InfiniteLogoCarousel 
    Title="Our Partners"
    CustomLogos="@partnerLogos"
    AnimationDuration="60"
    PauseOnHover="true" />
```

### State & Feedback

#### OsirionContentNotFound
**Purpose**: User-friendly 404 error pages  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionContentNotFound 
    Title="Page Not Found"
    Message="The page you're looking for doesn't exist."
    ShowBackButton="true" />
```

#### OsirionPageLoading
**Purpose**: Loading indicators for page transitions  
**Package**: `Osirion.Blazor.Core`

```razor
<OsirionPageLoading 
    Message="Loading content..."
    ShowSpinner="true" />
```

### Privacy & Compliance

#### OsirionCookieConsent
**Purpose**: GDPR-compliant cookie consent management  
**Package**: `Osirion.Blazor.Core`  
**Documentation**: [Cookie Consent Guide](COOKIE_CONSENT.md)

```razor
<OsirionCookieConsent 
    Title="Privacy Preferences"
    PolicyLink="/privacy-policy"
    ShowCustomizeButton="true"
    Categories="@cookieCategories" />
```

## ?? Analytics Components

### Clarity Tracker
**Purpose**: Microsoft Clarity analytics integration  
**Package**: `Osirion.Blazor.Analytics`  
**Documentation**: [Analytics Guide](ANALYTICS.md)

```razor
<ClarityTracker SiteId="your-clarity-id" />
```

### Matomo Tracker
**Purpose**: Matomo analytics integration  
**Package**: `Osirion.Blazor.Analytics`

```razor
<MatomoTracker 
    SiteId="1"
    TrackerUrl="//analytics.example.com/" />
```

### GA4 Tracker
**Purpose**: Google Analytics 4 integration  
**Package**: `Osirion.Blazor.Analytics`

```razor
<GA4Tracker 
    MeasurementId="G-XXXXXXXXXX"
    AnonymizeIp="true" />
```

### Yandex Metrica
**Purpose**: Yandex Metrica analytics  
**Package**: `Osirion.Blazor.Analytics`

```razor
<YandexMetricaTracker 
    CounterId="12345678"
    WebVisor="true" />
```

### Plausible
**Purpose**: Plausible Analytics, cookie-free  
**Package**: `Osirion.Blazor.Analytics`

```razor
@* Program.cs: analytics.AddPlausible(o => o.ScriptUrl = "https://plausible.io/js/pa-XXXXX.js") *@
<PlausibleTracker />
```

### Umami
**Purpose**: Umami analytics, cookie-free, cloud or self-hosted  
**Package**: `Osirion.Blazor.Analytics`

```razor
@* Program.cs: analytics.AddUmami(o => o.SiteId = "your-website-id") *@
<UmamiTracker />
```

## ?? Content Management

### Content Display

#### ContentList
**Purpose**: Display lists of content items  
**Package**: `Osirion.Blazor.Cms`  
**Documentation**: [GitHub CMS Guide](GITHUB_CMS.md)

```razor
<ContentList Directory="blog" Count="10" />
<ContentList Category="tutorials" />
<ContentList Tag="blazor" />
<ContentList FeaturedCount="3" />
```

#### ContentView
**Purpose**: Display single content items  
**Package**: `Osirion.Blazor.Cms`

```razor
<ContentView Path="blog/my-post.md" />
```

#### CategoriesList
**Purpose**: Display content categories  
**Package**: `Osirion.Blazor.Cms`

```razor
<CategoriesList />
```

#### TagCloud
**Purpose**: Display content tags  
**Package**: `Osirion.Blazor.Cms`

```razor
<TagCloud MaxTags="20" />
```

#### SearchBox
**Purpose**: Content search functionality  
**Package**: `Osirion.Blazor.Cms`

```razor
<SearchBox Placeholder="Search articles..." />
```

#### DirectoryNavigation
**Purpose**: Navigation based on content structure  
**Package**: `Osirion.Blazor.Cms`

```razor
<DirectoryNavigation CurrentDirectory="blog" />
```

### Content Editing

#### MarkdownEditorPreview
**Purpose**: Combined markdown editor with live preview  
**Package**: `Osirion.Blazor.Cms.Admin`  
**Documentation**: [Markdown Editor Guide](MARKDOWN_EDITOR.md)

```razor
<MarkdownEditorPreview 
    Content="@content"
    ContentChanged="@((c) => content = c)"
    ShowToolbar="true"
    SyncScroll="true" />
```

#### MarkdownEditor
**Purpose**: Standalone markdown editor  
**Package**: `Osirion.Blazor.Cms.Admin`

```razor
<MarkdownEditor 
    Content="@content"
    ContentChanged="@UpdateContent"
    Placeholder="Enter markdown..." />
```

#### MarkdownPreview
**Purpose**: Standalone markdown preview  
**Package**: `Osirion.Blazor.Cms.Admin`

```razor
<MarkdownPreview 
    Markdown="@markdownContent"
    Pipeline="@customPipeline" />
```

### SEO & Metadata

#### SeoMetadataRenderer
**Purpose**: SEO metadata and structured data  
**Package**: `Osirion.Blazor.Cms`

```razor
<SeoMetadataRenderer 
    Content="@currentContent"
    SiteName="My Website"
    BaseUrl="https://mysite.com" />
```

#### LocalizedNavigation
**Purpose**: Multi-language navigation  
**Package**: `Osirion.Blazor.Cms`

```razor
<LocalizedNavigation 
    CurrentLocale="en"
    OnLocaleChanged="@HandleLocaleChange" />
```

#### LocalizedContentView
**Purpose**: Multi-language content display  
**Package**: `Osirion.Blazor.Cms`

```razor
<LocalizedContentView 
    Path="@contentPath"
    CurrentLocale="@currentLocale" />
```

## ?? Theming & Styling

### OsirionStyles
**Purpose**: CSS framework integration and theming  
**Package**: `Osirion.Blazor.Theming`  
**Documentation**: [Styling Guide](STYLING.md)

```razor
<OsirionStyles 
    Framework="CssFramework.Bootstrap"
    CustomVariables="--primary-color: #007bff;" />
```

### Framework Integration

```csharp
// Bootstrap
builder.Services.AddOsirionStyle(CssFramework.Bootstrap);

// Fluent UI
builder.Services.AddOsirionStyle(CssFramework.FluentUI);

// MudBlazor
builder.Services.AddOsirionStyle(CssFramework.MudBlazor);

// Radzen
builder.Services.AddOsirionStyle(CssFramework.Radzen);
```

## ??? Service Registration Patterns

### Fluent API (Recommended)

```csharp
builder.Services.AddOsirionBlazor(osirion => {
    osirion
        .AddGitHubCms(options => {
            options.Owner = "username";
            options.Repository = "content-repo";
        })
        .AddScrollToTop(options => {
            options.Position = Position.BottomRight;
        })
        .AddClarityTracker(options => {
            options.SiteId = "clarity-id";
        })
        .AddOsirionStyle(CssFramework.Bootstrap);
});
```

### Configuration-Based

```csharp
builder.Services.AddOsirionBlazor(builder.Configuration);
```

With `appsettings.json`:

```json
{
  "Osirion": {
    "GitHubCms": {
      "Owner": "username",
      "Repository": "content-repo"
    },
    "Analytics": {
      "Clarity": {
        "SiteId": "clarity-id"
      }
    },
    "Theming": {
      "Framework": "Bootstrap"
    }
  }
}
```

### Individual Services

```csharp
// Individual service registration
builder.Services.AddGitHubCms(options => { ... });
builder.Services.AddScrollToTop();
builder.Services.AddClarityTracker(options => { ... });
builder.Services.AddOsirionStyle(CssFramework.Bootstrap);
```

## ?? Common Use Cases

### Landing Page

```razor
<!-- Hero section -->
<HeroSection 
    Title="Transform Your Business"
    Subtitle="Digital Solutions for Modern Enterprises"
    PrimaryButtonText="Start Free Trial"
    PrimaryButtonUrl="/trial"
    ImageUrl="/hero.jpg"
    UseBackgroundImage="true" />

<!-- Partner logos -->
<InfiniteLogoCarousel 
    Title="Trusted by Industry Leaders"
    CustomLogos="@partnerLogos" />

<!-- Footer -->
<OsirionFooter>
    <FooterContent>
        <!-- Footer content -->
    </FooterContent>
</OsirionFooter>
```

### Blog Layout

```razor
<!-- Navigation and breadcrumbs -->
<EnhancedNavigation />
<OsirionBreadcrumbs Path="@Navigation.Uri" />

<!-- Article hero -->
<HeroSection 
    Title="@article.Title"
    Author="@article.Author"
    PublishDate="@article.Date"
    ShowMetadata="true"
    Variant="HeroVariant.Minimal" />

<!-- Content and sidebar -->
<div class="container">
    <div class="row">
        <div class="col-md-8">
            <ContentView Path="@article.Path" />
        </div>
        <div class="col-md-4">
            <CategoriesList />
            <TagCloud />
            <ContentList FeaturedCount="5" />
        </div>
    </div>
</div>

<!-- Scroll to top -->
<ScrollToTop />
```

### Admin Interface

```razor
<!-- Content editor -->
<div class="admin-layout">
    <div class="editor-section">
        <MarkdownEditorPreview 
            Content="@post.Content"
            ContentChanged="@UpdateContent"
            ShowToolbar="true" />
    </div>
    
    <div class="metadata-section">
        <!-- Metadata forms -->
    </div>
</div>
```

### Documentation Site

```razor
<!-- Breadcrumb navigation -->
<OsirionBreadcrumbs 
    Path="/docs/components/navigation"
    HomeText="Documentation"
    HomeUrl="/docs" />

<!-- Content with sidebar -->
<OsirionPageLayout HasSidebar="true">
    <SidebarContent>
        <DirectoryNavigation />
    </SidebarContent>
    <MainContent>
        <ContentView Path="@currentPath" />
    </MainContent>
</OsirionPageLayout>

<!-- Search functionality -->
<SearchBox Placeholder="Search documentation..." />
```

## ?? Configuration Examples

### Complete Configuration

```json
{
  "Osirion": {
    "GitHubCms": {
      "Owner": "your-username",
      "Repository": "content-repo",
      "ContentPath": "content",
      "Branch": "main",
      "CacheExpirationMinutes": 60,
      "EnableLocalization": true,
      "DefaultLocale": "en"
    },
    "Analytics": {
      "Clarity": {
        "SiteId": "your-clarity-id",
        "Enabled": true
      },
      "Matomo": {
        "SiteId": "1",
        "TrackerUrl": "//analytics.example.com/",
        "Enabled": true
      }
    },
    "Navigation": {
      "Enhanced": {
        "Behavior": "Smooth",
        "ResetScrollOnNavigation": true
      },
      "ScrollToTop": {
        "Position": "BottomRight",
        "VisibilityThreshold": 300
      }
    },
    "Theming": {
      "Framework": "Bootstrap",
      "CustomVariables": "--primary-color: #007bff;"
    }
  }
}
```

## ?? Documentation Links

- [?? Main Documentation](../README.md)
- [?? Analytics Guide](ANALYTICS.md)
- [?? Navigation Guide](NAVIGATION.md)
- [?? GitHub CMS Guide](GITHUB_CMS.md)
- [?? Styling Guide](STYLING.md)
- [?? Core Components](../src/Osirion.Blazor.Core/README.md)
- [?? Migration Guide](MIGRATION.md)

### Component-Specific Guides

- [?? Hero Section](HERO_SECTION.md)
- [?? Breadcrumbs](BREADCRUMBS.md)
- [?? Cookie Consent](COOKIE_CONSENT.md)
- [?? Logo Carousel](INFINITE_LOGO_CAROUSEL.md)
- [?? Markdown Editor](MARKDOWN_EDITOR.md)

## ?? What's New in v1.5.0

- ? **Fluent API**: Streamlined service registration
- ?? **CSS Framework Integration**: Automatic Bootstrap, FluentUI, MudBlazor, Radzen support
- ?? **New Core Components**: HeroSection, Breadcrumbs, Cookie Consent
- ?? **Enhanced Documentation**: Component-specific guides with examples
- ?? **Improved Configuration**: Better separation of concerns

## ?? Getting Help

- ?? Check the [Documentation](../README.md)
- ?? Report issues on [GitHub](https://github.com/demonjic/Osirion.Blazor/issues)
- ?? Contact via the [Repository](https://github.com/demonjic/Osirion.Blazor)