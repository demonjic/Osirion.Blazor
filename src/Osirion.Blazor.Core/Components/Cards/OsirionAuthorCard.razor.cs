using Microsoft.AspNetCore.Components;
using Osirion.Blazor.Core.Models;
using Osirion.Blazor.Core.Utils;

namespace Osirion.Blazor.Components;

/// <summary>
/// Introduces the author of an article or the person an author page is about: avatar, name, role, short bio and
/// links. It renders without script.
/// </summary>
public partial class OsirionAuthorCard
{
    /// <summary>Gets or sets the author's name; the initials come from its first two words. Required.</summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the author's role or title, for example <c>Principal engineer</c>.</summary>
    [Parameter]
    public string? Role { get; set; }

    /// <summary>Gets or sets a short biography, shown as plain text.</summary>
    [Parameter]
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the avatar image. Without one, the author's initials are shown. The image is decorative (the name
    /// is next to it), so it has an empty <c>alt</c>.
    /// </summary>
    [Parameter]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the author's page, for example the listing of their articles. The name links to it with
    /// <c>rel="author"</c>.
    /// </summary>
    [Parameter]
    public string? ProfileUrl { get; set; }

    /// <summary>
    /// Gets or sets links such as the author's site or social profiles, rendered with <c>rel="me"</c>. A link with an
    /// empty label, or with an address that is not a site path or an http, https, mailto or tel address, is left out.
    /// </summary>
    [Parameter]
    public IReadOnlyList<AuthorLink>? Links { get; set; }

    private IReadOnlyList<AuthorLink> SafeLinks { get; set; } = [];

    private string Initials { get; set; } = string.Empty;

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        SafeLinks = Links?.Where(link => !string.IsNullOrWhiteSpace(link.Label) && SafeLink.IsSafe(link.Url)).ToArray() ?? [];
        Initials = string.Concat(Name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));
    }

    private string GetAuthorCardClass() =>
        string.IsNullOrWhiteSpace(Class) ? "osirion-author-card" : $"osirion-author-card {Class}";
}
