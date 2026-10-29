namespace Osirion.Blazor.Cms.Domain.ContentBlocks;

/// <summary>A link shown as a button (<c>{ text, url }</c>).</summary>
/// <param name="Text">The button text.</param>
/// <param name="Url">
/// The destination: a site-relative path, an <c>https:</c> URL, <c>mailto:</c> or <c>tel:</c>, or a fragment of the
/// same page (<c>#pricing</c>).
/// </param>
public sealed record ContentBlockAction(string Text, string Url);

/// <summary>An image (<c>{ src, alt }</c>).</summary>
/// <param name="Src">The image address: a site-relative path or an <c>https:</c> URL.</param>
/// <param name="Alt">The text alternative; empty for a decorative image.</param>
public sealed record ContentBlockImage(string Src, string Alt);
