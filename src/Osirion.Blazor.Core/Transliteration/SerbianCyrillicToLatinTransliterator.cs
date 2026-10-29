using System.Buffers;
using System.Collections.Frozen;
using System.Text;

namespace Osirion.Blazor.Core.Transliteration;

/// <summary>
/// Writes Serbian Cyrillic in the Serbian Latin alphabet (Gaj's Latin), one letter for one letter.
/// </summary>
/// <remarks>
/// <para>
/// The thirty letters map to the Latin letters of the same position in the alphabet. Љ, Њ and Џ become two letters:
/// <c>Lj</c>, <c>Nj</c> and <c>Dž</c> in a word written in normal case (and in a word of that one letter), and
/// <c>LJ</c>, <c>NJ</c> and <c>DŽ</c> in a word written in capitals, which is when the letter next to it in the same
/// word is upper case: ЉУБАВ is LJUBAV, Љубав is Ljubav.
/// </para>
/// <para>Every other character, including Cyrillic letters other alphabets use, is left unchanged.</para>
/// </remarks>
public sealed class SerbianCyrillicToLatinTransliterator : ITextTransliterator
{
    /// <summary>The name configuration uses for this transliterator.</summary>
    public const string TransliteratorName = "serbian-cyrillic-to-latin";

    private const string CyrillicAlphabet = "АБВГДЂЕЖЗИЈКЛЉМНЊОПРСТЋУФХЦЧЏШ";

    private static readonly string[] LatinAlphabet =
    [
        "A", "B", "V", "G", "D", "Đ", "E", "Ž", "Z", "I", "J", "K", "L", "Lj", "M",
        "N", "Nj", "O", "P", "R", "S", "T", "Ć", "U", "F", "H", "C", "Č", "Dž", "Š",
    ];

    private static readonly FrozenDictionary<char, string> Letters = CreateLetters();

    private static readonly SearchValues<char> CyrillicLetters = SearchValues.Create(Letters.Keys.ToArray());

    /// <inheritdoc/>
    public string Name => TransliteratorName;

    /// <inheritdoc/>
    public string Transliterate(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var first = text.AsSpan().IndexOfAny(CyrillicLetters);
        if (first < 0)
        {
            return text;
        }

        var latin = new StringBuilder(text.Length + 8);
        latin.Append(text, 0, first);
        for (var index = first; index < text.Length; index++)
        {
            var character = text[index];
            if (!Letters.TryGetValue(character, out var replacement))
            {
                latin.Append(character);
                continue;
            }

            // Only the upper case Љ, Њ and Џ have a two letter replacement whose second letter depends on the word
            latin.Append(replacement.Length > 1 && char.IsUpper(character) && IsInWordWrittenInCapitals(text, index)
                ? replacement.ToUpperInvariant()
                : replacement);
        }

        return latin.ToString();
    }

    // The letter after decides; at the end of a word the letter before does. A word of one letter is not in capitals.
    private static bool IsInWordWrittenInCapitals(string text, int index)
    {
        if (index + 1 < text.Length && char.IsLetter(text[index + 1]))
        {
            return char.IsUpper(text[index + 1]);
        }

        return index > 0 && char.IsLetter(text[index - 1]) && char.IsUpper(text[index - 1]);
    }

    private static FrozenDictionary<char, string> CreateLetters()
    {
        var letters = new Dictionary<char, string>(CyrillicAlphabet.Length * 2);
        for (var position = 0; position < CyrillicAlphabet.Length; position++)
        {
            var upper = CyrillicAlphabet[position];
            var latin = LatinAlphabet[position];
            letters.Add(upper, latin);
            letters.Add(char.ToLowerInvariant(upper), latin.ToLowerInvariant());
        }

        return letters.ToFrozenDictionary();
    }
}
