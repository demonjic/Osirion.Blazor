using Microsoft.AspNetCore.Components;
#if !NET10_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Services;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Components;
using System.Text.RegularExpressions;

namespace Osirion.Blazor.Cms.Components;

/// <summary>
/// LandingPage component for marketing and conversion-focused pages
/// </summary>
public partial class LandingPage
{
    [Inject]
    private ILogger<LandingPage> Logger { get; set; } = default!;

    [Inject]
    private IContentProviderManager ContentProviderManager { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// The source path of the item to load from the default provider while <see cref="Content"/> is not set. When
    /// nothing is found, the page shows <see cref="NotFoundText"/> and signals 404.
    /// </summary>
    [Parameter]
    public string? ContentPath { get; set; }

    /// <summary>
    /// Whether to show an item that is not published (<c>published: false</c> in its front matter) when the
    /// component loads it by <see cref="ContentPath"/>. Off by default: a draft then renders the not-found state and
    /// signals 404, as a missing item does. Turn it on only for previews you protect yourself; lists, the sitemap
    /// and feeds leave drafts out either way. An item you pass in is shown whatever its status.
    /// </summary>
    [Parameter]
    public bool ShowUnpublished { get; set; }

    /// <summary>
    /// The item that makes the page: its title and description are the hero's, its featured image the hero's image
    /// and its body follows the hero. Without an item the page shows what the other parameters give it. An item with
    /// content blocks (<c>blocks:</c>) renders its blocks in place of the hero, followed by its body only with
    /// <c>body: after</c>; the features, testimonials, custom content and call to action still follow.
    /// </summary>
    [Parameter]
    public ContentItem? Content { get; set; }

    // An item written as blocks replaces the hero and shows its body only with body: after.
    private bool ShowsBody => Content is not { HasBlocks: true } || Content.BodyPlacement == ContentBodyPlacement.AfterBlocks;

    /// <summary>
    /// Whether to show a hero with the title, the description, the image and up to two buttons.
    /// </summary>
    [Parameter]
    public bool ShowHero { get; set; } = true;

    /// <summary>
    /// The hero's title when there is no <see cref="Content"/>. The item's title wins when both are set.
    /// </summary>
    [Parameter]
    public string? HeroTitle { get; set; }

    /// <summary>
    /// The hero's subtitle when there is no <see cref="Content"/>, or when the item has no description.
    /// </summary>
    [Parameter]
    public string? HeroSubtitle { get; set; }

    /// <summary>
    /// The hero's image address when the item has no featured image.
    /// </summary>
    [Parameter]
    public string? HeroBackgroundImage { get; set; }

    /// <summary>
    /// The alignment of the hero's text: <c>left</c>, <c>center</c> or <c>right</c>, in any case. Any other value is
    /// read as <c>center</c>. The string is kept in 4.x; an enum is planned for 5.0.
    /// </summary>
    [Parameter]
    public string HeroAlignment { get; set; } = "center";

    /// <summary>
    /// The minimum height of the hero, as a CSS length such as <c>70vh</c> or <c>30rem</c>.
    /// </summary>
    [Parameter]
    public string HeroHeight { get; set; } = "70vh";

    /// <summary>
    /// Whether the hero shows its buttons. A button also needs its text and its address.
    /// </summary>
    [Parameter]
    public bool ShowHeroButtons { get; set; } = true;

    /// <summary>
    /// The text of the hero's primary button. The button needs <see cref="HeroPrimaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? HeroPrimaryButtonText { get; set; }

    /// <summary>
    /// The address of the hero's primary button.
    /// </summary>
    [Parameter]
    public string? HeroPrimaryButtonUrl { get; set; }

    /// <summary>
    /// The text of the hero's secondary button. The button needs <see cref="HeroSecondaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? HeroSecondaryButtonText { get; set; }

    /// <summary>
    /// The address of the hero's secondary button.
    /// </summary>
    [Parameter]
    public string? HeroSecondaryButtonUrl { get; set; }

    /// <summary>
    /// The width of the section that holds the item's body: <c>narrow</c>, <c>normal</c>, <c>wide</c> or <c>full</c>.
    /// </summary>
    [Parameter]
    public string ContentWidth { get; set; } = "normal";

    /// <summary>
    /// The text alignment of the body: <c>left</c>, <c>center</c> or <c>right</c>.
    /// </summary>
    [Parameter]
    public string ContentAlign { get; set; } = "left";

    /// <summary>
    /// The look of the section that holds the body: <c>default</c>, <c>highlight</c>, <c>bordered</c> or
    /// <c>background</c>.
    /// </summary>
    [Parameter]
    public string ContentVariant { get; set; } = "default";

    /// <summary>
    /// The padding of the section that holds the body: <c>none</c>, <c>small</c>, <c>normal</c> or <c>large</c>.
    /// </summary>
    [Parameter]
    public string ContentPadding { get; set; } = "normal";

    /// <summary>
    /// Whether to show the testimonials section, after the body and the features. It renders only when <see
    /// cref="TestimonialItems"/> has a testimonial with a quote.
    /// </summary>
    [Parameter]
    public bool ShowTestimonials { get; set; } = false;

    /// <summary>
    /// The heading of the testimonials section.
    /// </summary>
    [Parameter]
    public string TestimonialsSectionTitle { get; set; } = "What Our Customers Say";

    /// <summary>
    /// The layout of the testimonials: <c>grid</c>, cards in columns, or <c>list</c>, one under the other. Any other
    /// value is read as <c>grid</c>.
    /// </summary>
    [Parameter]
    public string TestimonialsLayout { get; set; } = "grid";

    /// <summary>
    /// Whether each testimonial shows its <see cref="TestimonialItem.ImageUrl"/>, as a small round picture that
    /// carries no alternative text because the author's name stands beside it.
    /// </summary>
    [Parameter]
    public bool ShowTestimonialImages { get; set; } = true;

    /// <summary>
    /// The testimonials. One without a quote is skipped.
    /// </summary>
    [Parameter]
    public IReadOnlyList<TestimonialItem>? TestimonialItems { get; set; }

    /// <summary>
    /// Whether to show the features section, after the body. It renders only when <see cref="FeatureItems"/> has an
    /// item.
    /// </summary>
    [Parameter]
    public bool ShowFeatures { get; set; } = false;

    /// <summary>
    /// The heading of the features section.
    /// </summary>
    [Parameter]
    public string FeaturesSectionTitle { get; set; } = "Features";

    /// <summary>
    /// The sentence under the features heading. None when null.
    /// </summary>
    [Parameter]
    public string? FeaturesSectionDescription { get; set; }

    /// <summary>
    /// The layout of the features: <c>grid</c>, cards in <see cref="FeaturesColumns"/> columns, or <c>list</c>, one
    /// under the other. Any other value is read as <c>grid</c>.
    /// </summary>
    [Parameter]
    public string FeaturesLayout { get; set; } = "grid";

    /// <summary>
    /// The number of columns of the <c>grid</c> layout, 1 to 6; a value outside the range is moved to the nearest one.
    /// Narrow screens show fewer columns.
    /// </summary>
    [Parameter]
    public int FeaturesColumns { get; set; } = 3;

    /// <summary>
    /// The features, each with an icon, a title that can link, and a description.
    /// </summary>
    [Parameter]
    public IReadOnlyList<FeatureItem>? FeatureItems { get; set; }

    /// <summary>
    /// Whether to show the call to action, the last section of the page. It renders only when a title, a description
    /// or a button (text and address) is set.
    /// </summary>
    [Parameter]
    public bool ShowCallToAction { get; set; } = true;

    /// <summary>
    /// The heading of the call to action.
    /// </summary>
    [Parameter]
    public string? CtaTitle { get; set; }

    /// <summary>
    /// The sentence under the call to action's heading.
    /// </summary>
    [Parameter]
    public string? CtaDescription { get; set; }

    /// <summary>
    /// The text of the call to action's primary button. The button needs <see cref="CtaPrimaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? CtaPrimaryButtonText { get; set; }

    /// <summary>
    /// The address of the call to action's primary button. It is written as given, so build it from values you
    /// control.
    /// </summary>
    [Parameter]
    public string? CtaPrimaryButtonUrl { get; set; }

    /// <summary>
    /// The text of the call to action's secondary button. The button needs <see cref="CtaSecondaryButtonUrl"/> too.
    /// </summary>
    [Parameter]
    public string? CtaSecondaryButtonText { get; set; }

    /// <summary>
    /// The address of the call to action's secondary button. It is written as given, so build it from values you
    /// control.
    /// </summary>
    [Parameter]
    public string? CtaSecondaryButtonUrl { get; set; }

    /// <summary>
    /// A CSS color for the call to action's background: a hex value, <c>rgb()</c>, <c>hsl()</c>, a color name or a
    /// <c>var()</c> reference. Anything else is ignored, so the value cannot add CSS declarations. The text color
    /// stays that of <see cref="CtaVariant"/>, so choose a color that keeps the text readable.
    /// </summary>
    [Parameter]
    public string? CtaBackgroundColor { get; set; }

    /// <summary>
    /// The look of the call to action: <c>primary</c>, the theme's action color behind light text, or
    /// <c>secondary</c>, a quiet panel. Any other value is read as <c>primary</c>.
    /// </summary>
    [Parameter]
    public string CtaVariant { get; set; } = "primary";

    /// <summary>
    /// HTML shown in a wide, highlighted section after the body. It is written through <c>OsirionHtmlRenderer</c>,
    /// which sanitizes it with the registered HTML sanitizer.
    /// </summary>
    [Parameter]
    public string? CustomContent { get; set; }

    /// <summary>
    /// The text shown while the item loads, in interactive render modes only. A statically rendered response is sent
    /// once the item has loaded.
    /// </summary>
    [Parameter]
    public string LoadingText { get; set; } = "Loading page...";

    /// <summary>
    /// The message shown when <see cref="ContentPath"/> finds no item. The page also signals 404.
    /// </summary>
    [Parameter]
    public string NotFoundText { get; set; } = "Page content not found.";

    /// <summary>
    /// Content shown in a wide section after the body and the custom content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether the component is loading
    /// </summary>
    private bool IsLoading { get; set; } = true;

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        if (Content is null && ContentPath is { } contentPath && !string.IsNullOrWhiteSpace(contentPath))
        {
            await LoadContentAsync();
        }
        else
        {
            IsLoading = false;
        }

        SignalWhenNotFound(Content is null && !string.IsNullOrWhiteSpace(ContentPath));
    }

#if !NET10_0_OR_GREATER
    /// <summary>The request context; only available during static server-side rendering.</summary>
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }
#endif

    // Under static SSR on .NET 9 a closed connection stops the provider calls; on .NET 10 and in interactive render
    // modes there is no request to follow.
#if NET10_0_OR_GREATER
    private static CancellationToken RequestAborted => CancellationToken.None;
#else
    private CancellationToken RequestAborted => HttpContext?.RequestAborted ?? CancellationToken.None;
#endif

    private readonly NotFoundSignal _notFound = new();

    // The not-found block is the page's answer, so the response says 404 with it instead of a soft 404.
    private void SignalWhenNotFound(bool notFound)
    {
        if (!notFound)
        {
            _notFound.Reset();
            return;
        }

#if NET10_0_OR_GREATER
        _notFound.Send(NavigationManager);
#else
        _notFound.Send(HttpContext);
#endif
    }

    private async Task LoadContentAsync()
    {
        IsLoading = true;
        try
        {
            var provider = ContentProviderManager.GetDefaultProvider();
            if (provider is not null && ContentPath is { } contentPath && !string.IsNullOrWhiteSpace(contentPath))
            {
                Content = PublishedContent.ShownOrNull(
                    await provider.GetItemByPathAsync(contentPath, RequestAborted), ShowUnpublished);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load landing page content {ContentPath}", ContentPath);
            Content = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetLandingPageClass()
    {
        return $"osirion-landing-page {Class}".Trim();
    }

    private Alignment HeroAlignmentValue => HeroAlignment?.Trim().ToLowerInvariant() switch
    {
        "left" => Alignment.Left,
        "right" => Alignment.Right,
        _ => Alignment.Center,
    };

    private IReadOnlyList<TestimonialItem> Testimonials =>
        TestimonialItems?.Where(testimonial => !string.IsNullOrWhiteSpace(testimonial.Quote)).ToList() ?? [];

    private int FeatureColumnCount => Math.Clamp(FeaturesColumns, 1, 6);

    private static string LayoutClass(string layout) =>
        string.Equals(layout?.Trim(), "list", StringComparison.OrdinalIgnoreCase)
            ? "osirion-landing-layout-list"
            : "osirion-landing-layout-grid";

    private static string TestimonialByline(TestimonialItem testimonial) =>
        string.Join(", ", new[] { testimonial.Role, testimonial.Company }.Where(part => !string.IsNullOrWhiteSpace(part)));

    private static bool HasButton(string? text, string? url) =>
        !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(url);

    private bool HasCallToAction =>
        !string.IsNullOrWhiteSpace(CtaTitle)
        || !string.IsNullOrWhiteSpace(CtaDescription)
        || HasButton(CtaPrimaryButtonText, CtaPrimaryButtonUrl)
        || HasButton(CtaSecondaryButtonText, CtaSecondaryButtonUrl);

    private string CtaVariantName =>
        string.Equals(CtaVariant?.Trim(), "secondary", StringComparison.OrdinalIgnoreCase) ? "secondary" : "primary";

    // The value lands in a style attribute, so only a color is accepted: a hex value, a color function, a name or a
    // variable. A semicolon, a colon or a quote would let the value add declarations.
    private string? CtaStyle =>
        CtaBackgroundColor?.Trim() is { Length: > 0 } color && CssColor().IsMatch(color)
            ? $"background-color: {color}"
            : null;

    [GeneratedRegex(@"^(#[0-9a-fA-F]{3,8}|(rgb|rgba|hsl|hsla|var)\([a-zA-Z0-9\s,.%/\-]+\)|[a-zA-Z]+)$", RegexOptions.None, matchTimeoutMilliseconds: 250)]
    private static partial Regex CssColor();
}

/// <summary>
/// Represents a testimonial item
/// </summary>
public class TestimonialItem
{
    /// <summary>
    /// Gets or sets the name of the person giving the testimonial
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the role of the person giving the testimonial
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the company of the person giving the testimonial
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// Gets or sets the quote text of the testimonial
    /// </summary>
    public string? Quote { get; set; }

    /// <summary>
    /// Gets or sets the image URL for the testimonial
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the rating from 1 to 5 that the testimonial's author gave. <c>LandingPage</c> does not show it.
    /// </summary>
    public int Rating { get; set; } = 5;
}

/// <summary>
/// Represents a feature item
/// </summary>
public class FeatureItem
{
    /// <summary>
    /// Gets or sets the title of the feature
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the feature
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon for the feature
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the URL for the feature icon
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the link URL for the feature
    /// </summary>
    public string? LinkUrl { get; set; }
}
