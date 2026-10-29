using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Transliteration;

public class TextTransliteratorsTests
{
    [Fact]
    public void AddOsirionTransliteration_ResolvesTheBuiltInByName()
    {
        using var provider = new ServiceCollection().AddOsirionTransliteration().BuildServiceProvider();

        var registry = provider.GetRequiredService<TextTransliterators>();

        registry.TryGet("serbian-cyrillic-to-latin", out var transliterator).ShouldBeTrue();
        transliterator.ShouldBeOfType<SerbianCyrillicToLatinTransliterator>();
    }

    [Fact]
    public void AddOsirionTransliteration_CalledTwice_RegistersTheBuiltInOnce()
    {
        using var provider = new ServiceCollection()
            .AddOsirionTransliteration()
            .AddOsirionTransliteration()
            .BuildServiceProvider();

        provider.GetServices<ITextTransliterator>().Count().ShouldBe(1);
    }

    [Fact]
    public void TryGet_SiteTransliteratorAddedAfterwards_IsResolvedByItsName()
    {
        var custom = Transliterator("custom");
        var services = new ServiceCollection().AddOsirionTransliteration();
        services.AddSingleton(custom);
        using var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<TextTransliterators>();

        registry.TryGet("custom", out var transliterator).ShouldBeTrue();
        transliterator.ShouldBeSameAs(custom);
        registry.Names.ShouldBe(["serbian-cyrillic-to-latin", "custom"], ignoreOrder: true);
    }

    [Fact]
    public void TryGet_NameInAnotherCase_FindsTheTransliterator()
    {
        var registry = new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]);

        registry.TryGet("Serbian-Cyrillic-To-Latin", out _).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    public void TryGet_UnknownOrEmptyName_FindsNothing(string? name)
    {
        var registry = new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]);

        registry.TryGet(name, out var transliterator).ShouldBeFalse();
        transliterator.ShouldBeNull();
    }

    [Fact]
    public void TryGet_TwoWithOneName_ResolvesTheOneRegisteredLast()
    {
        var replacement = Transliterator(SerbianCyrillicToLatinTransliterator.TransliteratorName);

        var registry = new TextTransliterators([new SerbianCyrillicToLatinTransliterator(), replacement]);

        registry.TryGet(SerbianCyrillicToLatinTransliterator.TransliteratorName, out var transliterator).ShouldBeTrue();
        transliterator.ShouldBeSameAs(replacement);
    }

    [Fact]
    public void Constructor_TransliteratorWithoutName_Throws()
    {
        Should.Throw<ArgumentException>(() => new TextTransliterators([Transliterator(" ")]));
    }

    private static ITextTransliterator Transliterator(string name)
    {
        var transliterator = Substitute.For<ITextTransliterator>();
        transliterator.Name.Returns(name);
        return transliterator;
    }
}
