# API Reference

[![Documentation](https://img.shields.io/badge/Documentation-API_Reference-blue)](https://github.com/demonjic/Osirion.Blazor/blob/master/docs/API_REFERENCE.md)
[![Version](https://img.shields.io/nuget/v/Osirion.Blazor)](https://www.nuget.org/packages/Osirion.Blazor)

Comprehensive API reference for all Osirion.Blazor packages, components, and services.

## Table of Contents

- [Core Components](#core-components)
- [Content Management](#content-management)
- [Analytics](#analytics)
- [Navigation](#navigation)
- [Theming](#theming)
- [Services](#services)
- [Models](#models)
- [Extensions](#extensions)

## Core Components

### HeroSection

A versatile hero section component for landing pages and article headers.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Main hero title |
| `Subtitle` | `string?` | `null` | Hero subtitle |
| `Summary` | `string?` | `null` | Hero description/summary |
| `ImageUrl` | `string?` | `null` | Image URL for hero section |
| `UseBackgroundImage` | `bool` | `false` | Display image as background vs. side image |
| `BackgroundPattern` | `BackgroundPatternType?` | `null` | Optional background pattern |
| `Alignment` | `Alignment` | `Alignment.Left` | Text alignment |
| `Variant` | `HeroVariant` | `HeroVariant.Hero` | Hero variant style |
| `ShowPrimaryButton` | `bool` | `false` | Show primary call-to-action button |
| `PrimaryButtonText` | `string?` | `null` | Primary button text |
| `PrimaryButtonUrl` | `string?` | `null` | Primary button URL |
| `ShowSecondaryButton` | `bool` | `false` | Show secondary button |
| `SecondaryButtonText` | `string?` | `null` | Secondary button text |
| `SecondaryButtonUrl` | `string?` | `null` | Secondary button URL |
| `MinHeight` | `string?` | `null` | Minimum height of hero section |
| `ShowMetadata` | `bool` | `false` | Display author/date metadata |
| `Author` | `string?` | `null` | Author name for metadata |
| `PublishDate` | `DateTime?` | `null` | Publication date |
| `ReadTime` | `string?` | `null` | Estimated reading time |

#### Usage

```razor
<HeroSection 
    Title="Welcome to Our Platform"
    Subtitle="Build amazing applications with ease"
    Summary="Experience the power of modern web development."
    ImageUrl="/images/hero.jpg"
    UseBackgroundImage="true"
    PrimaryButtonText="Get Started"
    PrimaryButtonUrl="/getting-started"
    SecondaryButtonText="Learn More"
    SecondaryButtonUrl="/docs"
    Variant="HeroVariant.Hero"
    Alignment="Alignment.Center" />
```

### OsirionBreadcrumbs

Automatically generates breadcrumb navigation from URL paths.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Path` | `string` | Required | URL path to generate breadcrumbs from |
| `ShowHome` | `bool` | `true` | Include home link in breadcrumbs |
| `HomeText` | `string` | `"Home"` | Text for home link |
| `HomeUrl` | `string` | `"/"` | URL for home link |
| `LinkLastItem` | `bool` | `false` | Make the last breadcrumb item clickable |
| `UrlPrefix` | `string?` | `null` | Prefix for generated URLs |
| `SegmentFormatter` | `Func<string, string>?` | `null` | Function to format URL segments |
| `CurrentPageText` | `string?` | `null` | Text of the last breadcrumb, the current page; pass the page title. When empty, the last segment is formatted like the others |

#### Usage

```razor
<OsirionBreadcrumbs 
    Path="/blog/web-development/blazor-components"
    ShowHome="true"
    HomeText="Home"
    HomeUrl="/"
    SegmentFormatter="@((segment) => segment.Replace("-", " ").ToTitleCase())" />
```

### OsirionCookieConsent

GDPR-compliant cookie consent management component.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string` | `"Cookie Consent"` | Consent dialog title |
| `Message` | `string` | Required | Main consent message |
| `AcceptButtonText` | `string` | `"Accept All"` | Accept button text |
| `DeclineButtonText` | `string` | `"Decline"` | Decline button text |
| `CustomizeButtonText` | `string` | `"Customize"` | Customize button text |
| `ShowDeclineButton` | `bool` | `true` | Show decline option |
| `ShowCustomizeButton` | `bool` | `true` | Show customize preferences |
| `PolicyLink` | `string?` | `null` | Link to privacy policy |
| `PolicyLinkText` | `string` | `"Privacy Policy"` | Privacy policy link text |
| `Categories` | `List<CookieCategory>` | Default categories | Cookie categories configuration |
| `Position` | `string` | `"bottom"` | Position of consent banner |
| `ConsentExpiryDays` | `int` | `365` | Days until consent expires |

#### Usage

```razor
<OsirionCookieConsent 
    Title="Privacy Preferences"
    Message="We use cookies to improve your experience on our website."
    PolicyLink="/privacy-policy"
    ShowCustomizeButton="true"
    Categories="@customCategories"
    ConsentExpiryDays="180" />
```

### InfiniteLogoCarousel

Self-contained logo carousel with zero external dependencies.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Carousel title |
| `CustomLogos` | `List<LogoItem>?` | `null` | Custom logo list |
| `AnimationDuration` | `int` | `60` | Animation duration in seconds |
| `Direction` | `AnimationDirection` | `AnimationDirection.Right` | Animation direction |
| `PauseOnHover` | `bool` | `true` | Pause animation on hover |
| `EnableGrayscale` | `bool` | `true` | Apply grayscale filter to logos |
| `LogoHeight` | `string` | `"60px"` | Height of logo images |

#### Usage

```razor
<InfiniteLogoCarousel 
    Title="Our Partners"
    CustomLogos="@partnerLogos"
    AnimationDuration="60"
    Direction="AnimationDirection.Right"
    PauseOnHover="true"
    EnableGrayscale="true" />
```

## Content Management

### ContentList

Displays lists of content items with filtering and sorting.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Directory` | `string?` | `null` | Filter by directory |
| `Category` | `string?` | `null` | Filter by category |
| `Tag` | `string?` | `null` | Filter by tag |
| `Query` | `string?` | `null` | Search query |
| `Count` | `int?` | `null` | Maximum number of items |
| `Skip` | `int` | `0` | Number of items to skip |
| `SortBy` | `SortField` | `SortField.Date` | Sort field |
| `SortDirection` | `SortDirection` | `SortDirection.Descending` | Sort direction |
| `FeaturedCount` | `int?` | `null` | Number of featured items |
| `ShowExcerpt` | `bool` | `true` | Display content excerpt |
| `ShowMetadata` | `bool` | `true` | Display metadata |
| `ShowReadMore` | `bool` | `true` | Show read more links |
| `ProviderId` | `string?` | `null` | Specific provider to use |

#### Usage

```razor
<ContentList 
    Directory="blog" 
    Count="10"
    SortBy="SortField.Date" 
    SortDirection="SortDirection.Descending"
    ShowExcerpt="true"
    ShowMetadata="true" />
```

### ContentView

Displays a single content item.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Path` | `string` | Required | Path to content item |
| `ShowMetadata` | `bool` | `true` | Display content metadata |
| `ShowTags` | `bool` | `true` | Display content tags |
| `ShowCategories` | `bool` | `true` | Display content categories |
| `ProviderId` | `string?` | `null` | Specific provider to use |

#### Usage

```razor
<ContentView 
    Path="blog/getting-started.md"
    ShowMetadata="true"
    ShowTags="true"
    ShowCategories="true" />
```

### SearchBox

Content search component with live search capabilities.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Placeholder` | `string` | `"Search..."` | Search input placeholder |
| `MinSearchLength` | `int` | `2` | Minimum characters for search |
| `SearchDelay` | `int` | `300` | Delay in milliseconds before search |
| `ShowResults` | `bool` | `true` | Show search results dropdown |
| `MaxResults` | `int` | `10` | Maximum results to display |
| `ProviderId` | `string?` | `null` | Specific provider to use |

#### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnSearch` | `EventCallback<string>` | Fired when search is performed |
| `OnResultSelected` | `EventCallback<ContentItem>` | Fired when result is selected |

#### Usage

```razor
<SearchBox 
    Placeholder="Search articles..."
    MinSearchLength="3"
    SearchDelay="500"
    MaxResults="5"
    OnSearch="@HandleSearch"
    OnResultSelected="@HandleResultSelected" />
```

## Analytics

### ClarityTracker

Microsoft Clarity analytics integration component.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SiteId` | `string?` | `null` | Clarity site ID |
| `Enabled` | `bool` | `true` | Enable tracking |
| `TrackerUrl` | `string` | `"https://www.clarity.ms/tag/"` | Clarity script URL |

#### Usage

```razor
<ClarityTracker SiteId="your-clarity-id" />
```

### MatomoTracker

Matomo analytics integration component.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SiteId` | `string` | Required | Matomo site ID |
| `TrackerUrl` | `string` | Required | Matomo tracker URL |
| `Enabled` | `bool` | `true` | Enable tracking |
| `TrackLinks` | `bool` | `true` | Track link clicks |
| `RequireConsent` | `bool` | `false` | Require cookie consent |

#### Usage

```razor
<MatomoTracker 
    SiteId="1"
    TrackerUrl="//analytics.example.com/"
    TrackLinks="true"
    RequireConsent="true" />
```

## Navigation

### EnhancedNavigation

Enhanced navigation with scroll restoration and smooth scrolling.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Behavior` | `ScrollBehavior` | `ScrollBehavior.Auto` | Scroll behavior |
| `ResetScrollOnNavigation` | `bool` | `true` | Reset scroll position on navigation |

#### Usage

```razor
<EnhancedNavigation 
    Behavior="ScrollBehavior.Smooth"
    ResetScrollOnNavigation="true" />
```

### ScrollToTop

"Back to top" button component.

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Position` | `Position` | `Position.BottomRight` | Button position |
| `Behavior` | `ScrollBehavior` | `ScrollBehavior.Smooth` | Scroll behavior |
| `VisibilityThreshold` | `int` | `300` | Pixels scrolled before showing |
| `ButtonText` | `string?` | `null` | Button text (if not using icon) |
| `UseIcon` | `bool` | `true` | Use icon instead of text |

#### Usage

```razor
<ScrollToTop 
    Position="Position.BottomRight"
    Behavior="ScrollBehavior.Smooth"
    VisibilityThreshold="400"
    UseIcon="true" />
```

## Services

### IContentProvider

Interface for content providers.

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetItemAsync(string path)` | `Task<ContentItem?>` | Get single content item |
| `GetItemsByQueryAsync(ContentQuery query)` | `Task<IEnumerable<ContentItem>>` | Get items by query |
| `GetCategoriesAsync()` | `Task<IEnumerable<string>>` | Get all categories |
| `GetTagsAsync()` | `Task<IEnumerable<string>>` | Get all tags |
| `SearchAsync(string query)` | `Task<IEnumerable<ContentItem>>` | Search content |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ProviderId` | `string` | Unique provider identifier |
| `IsEnabled` | `bool` | Whether provider is enabled |

### IAnalyticsService

Interface for analytics services.

#### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `TrackPageViewAsync(string? path)` | `Task` | Track page view |
| `TrackEventAsync(string category, string action, string? label, object? value)` | `Task` | Track custom event |
| `SetUserIdAsync(string userId)` | `Task` | Set user identifier |
| `SetCustomPropertyAsync(string name, string value)` | `Task` | Set custom property |

## Models

### ContentItem

Represents a content item.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique identifier |
| `Path` | `string` | Content path |
| `Title` | `string` | Content title |
| `Content` | `string` | Content body |
| `Excerpt` | `string?` | Content excerpt |
| `Author` | `string?` | Author name |
| `PublishDate` | `DateTime?` | Publication date |
| `LastModified` | `DateTime?` | Last modification date |
| `Categories` | `List<string>` | Content categories |
| `Tags` | `List<string>` | Content tags |
| `IsFeatured` | `bool` | Whether content is featured |
| `FeaturedImage` | `string?` | Featured image URL |
| `Metadata` | `Dictionary<string, object>` | Additional metadata |

### ContentQuery

Query parameters for content filtering and sorting.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Directory` | `string?` | Filter by directory |
| `Category` | `string?` | Filter by category |
| `Tag` | `string?` | Filter by tag |
| `SearchQuery` | `string?` | Search query |
| `FromDate` | `DateTime?` | Filter from date |
| `ToDate` | `DateTime?` | Filter to date |
| `Count` | `int?` | Maximum results |
| `Skip` | `int` | Number to skip |
| `SortBy` | `SortField` | Sort field |
| `SortDirection` | `SortDirection` | Sort direction |
| `IsFeatured` | `bool?` | Filter featured content |

### LogoItem

Represents a logo in the carousel.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Logo name |
| `ImageUrl` | `string` | Logo image URL |
| `Link` | `string?` | Optional link URL |
| `AltText` | `string?` | Alternative text |

### CookieCategory

Represents a cookie category for consent management.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Category identifier |
| `Name` | `string` | Category name |
| `Description` | `string` | Category description |
| `IsRequired` | `bool` | Whether category is required |
| `IsEnabled` | `bool` | Whether category is enabled |

## Enums

### HeroVariant

Hero section styling variants.

- `Hero` - Standard hero section
- `Jumbotron` - Large jumbotron style
- `Minimal` - Minimal hero style

### Alignment

Text alignment options.

- `Left` - Left alignment
- `Center` - Center alignment
- `Right` - Right alignment
- `Justify` - Justified alignment

### Position

Position options for components.

- `Top` - Top position
- `Bottom` - Bottom position
- `Left` - Left position
- `Right` - Right position
- `TopLeft` - Top-left corner
- `TopRight` - Top-right corner
- `BottomLeft` - Bottom-left corner
- `BottomRight` - Bottom-right corner

### ScrollBehavior

Scroll behavior options.

- `Auto` - Browser default
- `Smooth` - Smooth scrolling
- `Instant` - Instant scrolling

### SortField

Content sorting fields.

- `Title` - Sort by title
- `Date` - Sort by date
- `Author` - Sort by author
- `Category` - Sort by category

### SortDirection

Sort direction options.

- `Ascending` - Ascending order
- `Descending` - Descending order

### CssFramework

Supported CSS frameworks.

- `None` - No framework integration
- `Bootstrap` - Bootstrap integration
- `FluentUI` - Fluent UI integration
- `MudBlazor` - MudBlazor integration
- `Radzen` - Radzen integration

## Extensions

### Service Registration Extensions

#### AddOsirionBlazor

Fluent API for service registration.

```csharp
public static IServiceCollection AddOsirionBlazor(
    this IServiceCollection services,
    Action<IOsirionBuilder> configure)
```

#### AddGitHubCms

Register GitHub CMS provider.

```csharp
public static IServiceCollection AddGitHubCms(
    this IServiceCollection services,
    Action<GitHubOptions> configure)
```

#### AddScrollToTop

Register scroll to top functionality.

```csharp
public static IServiceCollection AddScrollToTop(
    this IServiceCollection services,
    Action<ScrollToTopOptions>? configure = null)
```

#### AddClarityTracker

Register Microsoft Clarity analytics.

```csharp
public static IServiceCollection AddClarityTracker(
    this IServiceCollection services,
    Action<ClarityOptions> configure)
```

### String Extensions

#### ToTitleCase

Convert string to title case.

```csharp
public static string ToTitleCase(this string input)
```

#### ToSlug

Convert string to URL-friendly slug.

```csharp
public static string ToSlug(this string input)
```

#### Truncate

Truncate string to specified length.

```csharp
public static string Truncate(this string input, int maxLength, string suffix = "...")
```

## Configuration

### GitHubOptions

Configuration for GitHub content provider.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Owner` | `string` | Required | GitHub repository owner |
| `Repository` | `string` | Required | GitHub repository name |
| `Branch` | `string` | `"main"` | Git branch |
| `ContentPath` | `string` | `"content"` | Content directory path |
| `ApiToken` | `string?` | `null` | GitHub API token |
| `UseCache` | `bool` | `true` | Enable caching |
| `CacheExpirationMinutes` | `int` | `60` | Cache expiration time |
| `EnableLocalization` | `bool` | `false` | Enable multi-language support |
| `DefaultLocale` | `string` | `"en"` | Default locale |
| `DerivedLocales` | `List<DerivedLocaleOptions>` | empty | Locales made from another by transliteration (`Locale`, `From`, `Transliteration`); see the localization guide |

### ClarityOptions

Configuration for Microsoft Clarity analytics.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SiteId` | `string` | Required | Clarity site ID |
| `Enabled` | `bool` | `true` | Enable tracking |
| `TrackerUrl` | `string` | Default URL | Clarity script URL |

### ScrollToTopOptions

Configuration for scroll to top functionality.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Position` | `Position` | `Position.BottomRight` | Button position |
| `Behavior` | `ScrollBehavior` | `ScrollBehavior.Smooth` | Scroll behavior |
| `VisibilityThreshold` | `int` | `300` | Show threshold in pixels |