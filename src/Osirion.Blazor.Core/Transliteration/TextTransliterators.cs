using System.Diagnostics.CodeAnalysis;

namespace Osirion.Blazor.Core.Transliteration;

/// <summary>
/// Resolves a registered <see cref="ITextTransliterator"/> by its <see cref="ITextTransliterator.Name"/>.
/// </summary>
/// <remarks>
/// Names are compared ignoring case. When two transliterators share a name, the one registered last wins, so a site
/// can replace a built-in transliterator by registering its own under the same name after
/// <see cref="Osirion.Blazor.Core.Extensions.TransliterationServiceCollectionExtensions.AddOsirionTransliteration"/>.
/// </remarks>
public sealed class TextTransliterators
{
    private readonly Dictionary<string, ITextTransliterator> _byName = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes the registry with every registered transliterator.</summary>
    /// <param name="transliterators">The transliterators, in registration order.</param>
    public TextTransliterators(IEnumerable<ITextTransliterator> transliterators)
    {
        ArgumentNullException.ThrowIfNull(transliterators);

        foreach (var transliterator in transliterators)
        {
            if (string.IsNullOrWhiteSpace(transliterator.Name))
            {
                throw new ArgumentException(
                    $"The transliterator {transliterator.GetType().FullName} has no name.", nameof(transliterators));
            }

            _byName[transliterator.Name] = transliterator;
        }
    }

    /// <summary>Gets the names of the registered transliterators.</summary>
    public IReadOnlyCollection<string> Names => _byName.Keys;

    /// <summary>Finds the transliterator registered under <paramref name="name"/>.</summary>
    /// <param name="name">The transliterator's name.</param>
    /// <param name="transliterator">The transliterator, when one is registered under the name.</param>
    /// <returns><see langword="true"/> when a transliterator has the name.</returns>
    public bool TryGet(string? name, [NotNullWhen(true)] out ITextTransliterator? transliterator)
    {
        transliterator = null;
        return !string.IsNullOrWhiteSpace(name) && _byName.TryGetValue(name, out transliterator);
    }
}
