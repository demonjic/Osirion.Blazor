using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Transliteration;

public class SerbianCyrillicToLatinTransliteratorTests
{
    private readonly SerbianCyrillicToLatinTransliterator _transliterator = new();

    [Fact]
    public void Name_IsTheConfiguredName()
    {
        _transliterator.Name.ShouldBe("serbian-cyrillic-to-latin");
    }

    [Fact]
    public void Transliterate_UpperCaseAlphabet_IsTheLatinAlphabetInCapitals()
    {
        // Each letter alone is a one-letter word, so the digraphs stay in normal case
        var letters = "А Б В Г Д Ђ Е Ж З И Ј К Л Љ М Н Њ О П Р С Т Ћ У Ф Х Ц Ч Џ Ш";

        _transliterator.Transliterate(letters)
            .ShouldBe("A B V G D Đ E Ž Z I J K L Lj M N Nj O P R S T Ć U F H C Č Dž Š");
    }

    [Fact]
    public void Transliterate_LowerCaseAlphabet_IsTheLatinAlphabetInLowerCase()
    {
        var letters = "а б в г д ђ е ж з и ј к л љ м н њ о п р с т ћ у ф х ц ч џ ш";

        _transliterator.Transliterate(letters)
            .ShouldBe("a b v g d đ e ž z i j k l lj m n nj o p r s t ć u f h c č dž š");
    }

    [Theory]
    [InlineData("ЉУБАВ", "LJUBAV")]
    [InlineData("Љубав", "Ljubav")]
    [InlineData("ЊЕГОШ", "NJEGOŠ")]
    [InlineData("Његош", "Njegoš")]
    [InlineData("ЏЕП", "DŽEP")]
    [InlineData("Џеп", "Džep")]
    [InlineData("Џ", "Dž")]
    [InlineData("КОЊ", "KONJ")]
    [InlineData("Коњ", "Konj")]
    [InlineData("ЏЏ", "DŽDŽ")]
    public void Transliterate_Digraphs_FollowTheCaseOfTheirWord(string cyrillic, string latin)
    {
        _transliterator.Transliterate(cyrillic).ShouldBe(latin);
    }

    [Fact]
    public void Transliterate_DigraphBeforeAnotherWord_LooksOnlyAtItsOwnWord()
    {
        _transliterator.Transliterate("Љ УБАВ").ShouldBe("Lj UBAV");
    }

    [Fact]
    public void Transliterate_Sentence_KeepsPunctuationDigitsAndLatinText()
    {
        _transliterator.Transliterate("Добродошли на сајт 2026, Osirion!")
            .ShouldBe("Dobrodošli na sajt 2026, Osirion!");
    }

    [Fact]
    public void Transliterate_CyrillicOfOtherAlphabets_IsUnchanged()
    {
        _transliterator.Transliterate("ы э ё Ѓ ќ").ShouldBe("ы э ё Ѓ ќ");
    }

    [Fact]
    public void Transliterate_TextWithoutCyrillic_ReturnsTheSameInstance()
    {
        var text = "Plain Latin text";

        _transliterator.Transliterate(text).ShouldBeSameAs(text);
    }

    [Fact]
    public void Transliterate_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() => _transliterator.Transliterate(null!));
    }
}
