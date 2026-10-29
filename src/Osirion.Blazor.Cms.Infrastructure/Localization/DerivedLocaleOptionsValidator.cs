using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Core.Transliteration;

namespace Osirion.Blazor.Cms.Infrastructure.Localization;

/// <summary>
/// Validates a provider's <see cref="ContentProviderOptions.DerivedLocales"/> when its options are first read, and at
/// start-up where the host validates options on start.
/// </summary>
/// <typeparam name="TOptions">The provider's options type.</typeparam>
internal sealed class DerivedLocaleOptionsValidator<TOptions>(TextTransliterators transliterators) : IValidateOptions<TOptions>
    where TOptions : ContentProviderOptions
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = DerivedLocaleValidation.Validate(options, transliterators);
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
