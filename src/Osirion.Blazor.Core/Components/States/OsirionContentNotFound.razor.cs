using Microsoft.AspNetCore.Components;

namespace Osirion.Blazor.Components;

/// <summary>
/// Component for displaying "content not found" states with customizable messaging and actions
/// </summary>
public partial class OsirionContentNotFound
{
    /// <summary>
    /// Gets or sets the large code above the title, such as <c>404</c>. Null or empty hides it, which suits an empty state.
    /// </summary>
    [Parameter]
    public string? ErrorCode { get; set; } = "404";

    /// <summary>
    /// Gets or sets the heading, rendered at <see cref="HeadingLevel"/>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Page Not Found";

    /// <summary>
    /// Gets or sets the level of the heading that holds the title, <c>1</c> to <c>6</c> (<c>h1</c> to <c>h6</c>). It
    /// defaults to <c>1</c>, the page's own heading; set <c>2</c> for an empty state inside a page that has its own
    /// <c>h1</c>, so the page keeps a single one. A value outside the range is moved to the nearest level. The title
    /// keeps its look at every level.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 1;

    private int TitleLevel => Math.Clamp(HeadingLevel, 1, 6);

    /// <summary>
    /// Gets or sets the paragraph under the title. Empty hides it.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The page you're looking for doesn't exist or has been moved.";

    /// <summary>
    /// Gets or sets whether an icon is shown above the text.
    /// </summary>
    [Parameter]
    public bool ShowIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the built-in magnifier icon is shown when neither <see cref="Icon"/> nor <see cref="IconUrl"/> is
    /// set.
    /// </summary>
    [Parameter]
    public bool UseDefaultIcon { get; set; } = true;

    /// <summary>
    /// Gets or sets your own icon markup. It takes precedence over <see cref="IconUrl"/> and the default icon.
    /// </summary>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets the address of an image used as the icon, when <see cref="Icon"/> is not set.
    /// </summary>
    [Parameter]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets the alternative text of the <see cref="IconUrl"/> image.
    /// </summary>
    [Parameter]
    public string IconAlt { get; set; } = "Not Found";

    /// <summary>
    /// Gets or sets the text of the primary button, a link to <see cref="PrimaryButtonUrl"/>. Null or empty hides the button.
    /// </summary>
    [Parameter]
    public string? PrimaryButtonText { get; set; } = "Go to Homepage";

    /// <summary>
    /// Gets or sets the address of the primary button. An address that is not a local path or an http, https or mailto URL leaves the button out.
    /// </summary>
    [Parameter]
    public string PrimaryButtonUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets the text of the outlined secondary button, a link to <see cref="SecondaryButtonUrl"/>. Null or empty, the
    /// default, hides it.
    /// </summary>
    [Parameter]
    public string? SecondaryButtonText { get; set; }

    /// <summary>
    /// Gets or sets the address of the secondary button. An unsafe address leaves the button out.
    /// </summary>
    [Parameter]
    public string SecondaryButtonUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets more buttons or links, rendered after the two buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Gets or sets whether the <see cref="Suggestions"/> list is shown.
    /// </summary>
    [Parameter]
    public bool ShowSuggestions { get; set; } = false;

    /// <summary>
    /// Gets or sets the text above the suggestions.
    /// </summary>
    [Parameter]
    public string SuggestionsTitle { get; set; } = "Here are some helpful links:";

    /// <summary>
    /// Gets or sets the links to suggest. They are shown only when <see cref="ShowSuggestions"/> is true and the list is not
    /// empty.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ContentNotFoundSuggestion>? Suggestions { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="SearchContent"/> is rendered.
    /// </summary>
    [Parameter]
    public bool ShowSearchBox { get; set; } = false;

    /// <summary>
    /// Gets or sets a search form of your choice. It is rendered only when <see cref="ShowSearchBox"/> is true.
    /// </summary>
    [Parameter]
    public RenderFragment? SearchContent { get; set; }

    /// <summary>
    /// Gets or sets whether the contact line is shown. It also needs <see cref="ContactEmail"/>.
    /// </summary>
    [Parameter]
    public bool ShowContactInfo { get; set; } = false;

    /// <summary>
    /// Gets or sets the text before the email address.
    /// </summary>
    [Parameter]
    public string ContactMessage { get; set; } = "Need help? Contact us at";

    /// <summary>
    /// Gets or sets the address in the contact line, written as a <c>mailto:</c> link.
    /// </summary>
    [Parameter]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Gets or sets the look: <c>default</c> (plain), <c>minimal</c> (smaller text and icon), <c>centered</c> (a rounded panel in the
    /// secondary background) or <c>hero</c> (full window height with a gradient). Added as the class
    /// <c>osirion-content-not-found-{Variant}</c>.
    /// </summary>
    [Parameter]
    public string Variant { get; set; } = "default";

    /// <summary>
    /// Gets or sets the minimum height: <c>small</c>, <c>medium</c>, <c>large</c> or <c>full</c> (the window below the header). Added
    /// as the class <c>osirion-content-not-found-{Size}</c>.
    /// </summary>
    [Parameter]
    public string Size { get; set; } = "medium";

    /// <summary>
    /// Gets or sets a decorative pattern behind the content. Null, the default, shows none.
    /// </summary>
    [Parameter]
    public BackgroundPatternType? BackgroundPattern { get; set; }

    /// <summary>
    /// Gets or sets other content, rendered after the search slot and before the buttons.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the CSS class for the component
    /// </summary>
    private string GetContentNotFoundClass()
    {
        var classes = new List<string> { "osirion-content-not-found" };

        classes.Add($"osirion-content-not-found-{Variant}");
        classes.Add($"osirion-content-not-found-{Size}");

        if (!string.IsNullOrWhiteSpace(ErrorCode))
        {
            classes.Add("osirion-content-not-found-with-code");
        }

        if (BackgroundPattern is not null)
        {
            classes.Add("osirion-content-not-found-with-background");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Creates a default 404 configuration
    /// </summary>
    /// <remarks>
    /// The returned instance only holds the preset values. Blazor creates component instances itself, so it cannot be rendered;
    /// set the same parameters in markup.
    /// </remarks>
    public static OsirionContentNotFound Create404()
    {
        return new OsirionContentNotFound
        {
            ErrorCode = "404",
            Title = "Page Not Found",
            Message = "The page you're looking for doesn't exist or has been moved.",
            PrimaryButtonText = "Go to Homepage",
            PrimaryButtonUrl = "/"
        };
    }

    /// <summary>
    /// Creates a default 403 configuration
    /// </summary>
    /// <remarks>
    /// The returned instance only holds the preset values and cannot be rendered. Its button leads to the home page.
    /// </remarks>
    public static OsirionContentNotFound Create403()
    {
        return new OsirionContentNotFound
        {
            ErrorCode = "403",
            Title = "Access Denied",
            Message = "You don't have permission to access this resource.",
            // A javascript: address is not a link: it is unsafe and does nothing without script, so lead to the home page.
            PrimaryButtonText = "Go to Homepage",
            PrimaryButtonUrl = "/"
        };
    }

    /// <summary>
    /// Creates a default empty search results configuration
    /// </summary>
    public static OsirionContentNotFound CreateEmptySearch(string searchTerm)
    {
        return new OsirionContentNotFound
        {
            Title = "No Results Found",
            Message = $"We couldn't find any results for \"{searchTerm}\". Try adjusting your search or browse our categories.",
            ShowSearchBox = true,
            UseDefaultIcon = true
        };
    }

    /// <summary>
    /// Creates a default empty content configuration
    /// </summary>
    public static OsirionContentNotFound CreateEmptyContent()
    {
        return new OsirionContentNotFound
        {
            Title = "No Content Available",
            Message = "There's no content to display at the moment. Please check back later.",
            ShowIcon = true,
            UseDefaultIcon = true
        };
    }
}

/// <summary>
/// Represents a content suggestion link
/// </summary>
public class ContentNotFoundSuggestion
{
    /// <summary>
    /// Gets or sets the suggestion text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggestion URL. An unsafe address renders the text without a link.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggestion description (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets target. Default "_self".
    /// </summary>
    public string? Target { get; set; } = "_self";
}
