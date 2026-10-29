using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Localization;

public class DerivedLocaleOptionsValidatorTests
{
    private readonly DerivedLocaleOptionsValidator<FileSystemOptions> _validator =
        new(new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]));

    [Fact]
    public void Validate_NoDerivedLocales_Succeeds()
    {
        _validator.Validate(Options.DefaultName, new FileSystemOptions()).Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ValidDerivedLocale_Succeeds()
    {
        _validator.Validate(Options.DefaultName, OptionsWith(Derived())).Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_DerivedLocaleAlsoListedInSupportedLocales_Succeeds()
    {
        var options = OptionsWith(Derived());
        options.SupportedLocales = ["en", "sr", "sr-Latn"];

        _validator.Validate(Options.DefaultName, options).Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_LocalizationDisabled_NamesEnableLocalization()
    {
        var options = OptionsWith(Derived());
        options.EnableLocalization = false;

        Failures(options).ShouldBe(["DerivedLocales needs EnableLocalization to be true."]);
    }

    [Theory]
    [InlineData("", "DerivedLocales[0].Locale is required.")]
    [InlineData("not-a-culture-xyz", "DerivedLocales[0].Locale 'not-a-culture-xyz' is not a culture name.")]
    public void Validate_LocaleNotACultureName_NamesTheKey(string locale, string failure)
    {
        Failures(OptionsWith(Derived(locale: locale))).ShouldBe([failure]);
    }

    [Fact]
    public void Validate_FromNotACultureName_NamesTheKey()
    {
        Failures(OptionsWith(Derived(from: "xx-not-real"))).ShouldBe(["DerivedLocales[0].From 'xx-not-real' is not a culture name."]);
    }

    [Fact]
    public void Validate_LocaleSameAsFrom_NamesTheKey()
    {
        Failures(OptionsWith(Derived(locale: "SR", from: "sr"))).ShouldBe(["DerivedLocales[0].Locale 'SR' must differ from From."]);
    }

    [Fact]
    public void Validate_FromNotASupportedLocale_NamesTheKeyAndTheList()
    {
        Failures(OptionsWith(Derived(from: "de"))).ShouldBe(["DerivedLocales[0].From 'de' is not one of SupportedLocales (en, sr)."]);
    }

    [Fact]
    public void Validate_FromIsAnotherDerivedLocale_NamesTheKey()
    {
        var options = OptionsWith(Derived(), Derived(locale: "sr-Latn-RS", from: "sr-Latn"));

        Failures(options).ShouldBe(["DerivedLocales[1].From 'sr-Latn' is itself a derived locale; derive from a locale content is written in."]);
    }

    [Fact]
    public void Validate_UnknownTransliteration_NamesTheKeyAndTheRegisteredNames()
    {
        Failures(OptionsWith(Derived(transliteration: "latin-to-cyrillic"))).ShouldBe(
            ["DerivedLocales[0].Transliteration 'latin-to-cyrillic' is not a registered transliterator (registered: serbian-cyrillic-to-latin)."]);
    }

    [Fact]
    public void Validate_LocaleDerivedTwice_NamesTheSecondEntry()
    {
        Failures(OptionsWith(Derived(), Derived(locale: "sr-latn"))).ShouldBe(["DerivedLocales[1].Locale 'sr-latn' is derived more than once."]);
    }

    [Fact]
    public void Validate_EmptyEntry_NamesIt()
    {
        Failures(OptionsWith([null!])).ShouldBe(["DerivedLocales[0] is empty."]);
    }

    [Fact]
    public void Validate_SeveralBrokenRules_ReportsEachOfThem()
    {
        Failures(OptionsWith(Derived(from: "de", transliteration: "none"))).Count.ShouldBe(2);
    }

    private List<string> Failures(FileSystemOptions options)
    {
        var result = _validator.Validate(Options.DefaultName, options);
        result.Failed.ShouldBeTrue();
        return result.Failures!.ToList();
    }

    private static FileSystemOptions OptionsWith(params DerivedLocaleOptions[] derivedLocales) => new()
    {
        EnableLocalization = true,
        SupportedLocales = ["en", "sr"],
        DerivedLocales = derivedLocales.ToList(),
    };

    private static DerivedLocaleOptions Derived(
        string locale = "sr-Latn", string from = "sr", string transliteration = SerbianCyrillicToLatinTransliterator.TransliteratorName)
    {
        return new DerivedLocaleOptions { Locale = locale, From = from, Transliteration = transliteration };
    }
}
