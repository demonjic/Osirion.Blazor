namespace Osirion.Blazor.Core.Transliteration;

/// <summary>
/// Writes text in another script, character by character, without translating it.
/// </summary>
/// <remarks>
/// Register an implementation as a singleton <see cref="ITextTransliterator"/>; <see cref="TextTransliterators"/> then
/// resolves it by its <see cref="Name"/>. An implementation must be thread safe and leave every character it does not
/// know unchanged.
/// </remarks>
public interface ITextTransliterator
{
    /// <summary>
    /// Gets the name configuration uses for this transliterator, for example <c>serbian-cyrillic-to-latin</c>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Transliterates <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to transliterate.</param>
    /// <returns>The text in the target script; the same text when nothing in it changes.</returns>
    string Transliterate(string text);
}
