using Microsoft.Extensions.Options;

namespace Osirion.Blazor.Core.Localization;

/// <summary>Fails startup with every problem of the localization options, each naming its configuration key.</summary>
internal sealed class OsirionLocalizationOptionsValidator : IValidateOptions<OsirionLocalizationOptions>
{
    public ValidateOptionsResult Validate(string? name, OsirionLocalizationOptions options)
    {
        var errors = options.GetErrors();
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
