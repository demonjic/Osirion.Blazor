using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Localization;

public class TransliteratedHtmlTests
{
    private static readonly SerbianCyrillicToLatinTransliterator Serbian = new();

    private static string Transform(string html) => TransliteratedHtml.Transform(html, Serbian, "sr", "sr-Latn");

    [Fact]
    public void Transform_TextNodes_AreTransliteratedAndTheMarkupAroundThemIsKept()
    {
        var html = "<h2 id=\"uvod\" class=\"lead\">Увод</h2>\n<p>Добар <strong>дан</strong>, <em>свете</em>!</p>";

        Transform(html).ShouldBe("<h2 id=\"uvod\" class=\"lead\">Uvod</h2>\n<p>Dobar <strong>dan</strong>, <em>svete</em>!</p>");
    }

    [Fact]
    public void Transform_TextAttributes_AreTransliterated()
    {
        var html = "<img src=\"/images/most.png\" alt=\"Мост\" title=\"Стари мост\">"
            + "<input placeholder=\"Претрага\" aria-label=\"Поље претраге\" name=\"упит\">";

        Transform(html).ShouldBe("<img src=\"/images/most.png\" alt=\"Most\" title=\"Stari most\">"
            + "<input placeholder=\"Pretraga\" aria-label=\"Polje pretrage\" name=\"упит\">");
    }

    [Theory]
    [InlineData("code")]
    [InlineData("pre")]
    [InlineData("kbd")]
    [InlineData("samp")]
    public void Transform_CodeElements_AreUnchanged(string element)
    {
        var html = $"<div>Текст <{element} title=\"Наслов\">код</{element}></div>";

        Transform(html).ShouldBe($"<div>Tekst <{element} title=\"Наслов\">код</{element}></div>");
    }

    [Fact]
    public void Transform_CodeNestedInPre_IsUnchanged()
    {
        var html = "<pre><code class=\"language-csharp\">var поздрав = \"Здраво\";\n<span>коментар</span></code></pre><p>Крај</p>";

        Transform(html).ShouldBe("<pre><code class=\"language-csharp\">var поздрав = \"Здраво\";\n<span>коментар</span></code></pre><p>Kraj</p>");
    }

    [Fact]
    public void Transform_ScriptAndStyle_AreUnchanged()
    {
        var html = "<script>var т = 'текст';</script><style>.к::after { content: 'текст'; }</style><p>Текст</p>";

        Transform(html).ShouldBe("<script>var т = 'текст';</script><style>.к::after { content: 'текст'; }</style><p>Tekst</p>");
    }

    [Fact]
    public void Transform_ElementMarkedTranslateNo_IsUnchangedWithEverythingInside()
    {
        var html = "<p>Име: <span translate=\"no\" title=\"Назив\">Хексавера <b>бренд</b></span> и даље</p>";

        Transform(html).ShouldBe("<p>Ime: <span translate=\"no\" title=\"Назив\">Хексавера <b>бренд</b></span> i dalje</p>");
    }

    [Fact]
    public void Transform_Autolink_KeepsItsTextAndAddress()
    {
        var html = "<p>Видети <a href=\"https://пример.срб/страна\">https://пример.срб/страна</a></p>";

        Transform(html).ShouldBe("<p>Videti <a href=\"https://пример.срб/страна\">https://пример.срб/страна</a></p>");
    }

    [Fact]
    public void Transform_AutolinkWithPercentEncodedAddress_KeepsItsText()
    {
        var html = "<p><a href=\"https://example.org/%D0%B1%D0%BB%D0%BE%D0%B3\">https://example.org/блог</a></p>";

        Transform(html).ShouldBe(html);
    }

    [Fact]
    public void Transform_MailAutolink_KeepsItsText()
    {
        var html = "<p>Пишите на <a href=\"mailto:инфо@пример.срб\">инфо@пример.срб</a></p>";

        Transform(html).ShouldBe("<p>Pišite na <a href=\"mailto:инфо@пример.срб\">инфо@пример.срб</a></p>");
    }

    [Fact]
    public void Transform_ExternalLinkWithCyrillicAddress_KeepsTheAddressAndTransliteratesTheText()
    {
        var html = "<p><a href=\"https://sr.wikipedia.org/wiki/Србија\">Србија</a></p>";

        Transform(html).ShouldBe("<p><a href=\"https://sr.wikipedia.org/wiki/Србија\">Srbija</a></p>");
    }

    [Theory]
    [InlineData("https://example.org/sr/blog")]
    [InlineData("//example.org/sr/blog")]
    [InlineData("mailto:office@example.org")]
    [InlineData("tel:+381111234567")]
    [InlineData("#sr")]
    [InlineData("sr/blog")]
    [InlineData("../sr/blog")]
    public void Transform_AddressThatIsNotAPathOnTheSite_IsUnchanged(string href)
    {
        var html = $"<p><a href=\"{href}\">Веза</a></p>";

        Transform(html).ShouldBe($"<p><a href=\"{href}\">Veza</a></p>");
    }

    [Theory]
    [InlineData("/sr/feed.xml")]
    [InlineData("/sr/files/cenovnik.pdf")]
    [InlineData("/sr/images/logo.png?v=2")]
    public void Transform_LinkToAFile_IsUnchanged(string href)
    {
        var html = $"<p><a href=\"{href}\">Датотека</a></p>";

        Transform(html).ShouldBe($"<p><a href=\"{href}\">Datoteka</a></p>");
    }

    [Fact]
    public void Transform_ImageSource_IsUnchanged()
    {
        var html = "<p><img src=\"/sr/images/most.png\" alt=\"Мост\"></p>";

        Transform(html).ShouldBe("<p><img src=\"/sr/images/most.png\" alt=\"Most\"></p>");
    }

    [Theory]
    [InlineData("/sr", "/sr-Latn")]
    [InlineData("/sr/", "/sr-Latn/")]
    [InlineData("/sr/blog/prvi-post", "/sr-Latn/blog/prvi-post")]
    [InlineData("/SR/blog", "/sr-Latn/blog")]
    [InlineData("/sr/blog?strana=2#komentari", "/sr-Latn/blog?strana=2#komentari")]
    [InlineData("/sr/блог", "/sr-Latn/блог")]
    public void Transform_LinkToAPageInTheSourceLocale_MovesUnderTheDerivedLocale(string href, string expected)
    {
        var html = $"<p><a href=\"{href}\">Блог</a></p>";

        Transform(html).ShouldBe($"<p><a href=\"{expected}\">Blog</a></p>");
    }

    [Theory]
    [InlineData("/blog/prvi-post")]
    [InlineData("/en/blog")]
    [InlineData("/srpski/blog")]
    [InlineData("/")]
    public void Transform_LinkToAPageWithoutTheSourceLocaleSegment_IsUnchanged(string href)
    {
        var html = $"<p><a href=\"{href}\">Блог</a></p>";

        Transform(html).ShouldBe($"<p><a href=\"{href}\">Blog</a></p>");
    }

    [Fact]
    public void Transform_NothingToChange_ReturnsTheSameInstance()
    {
        var html = "<p>Plain <a href=\"/en/blog\">text</a><br/></p>";

        Transform(html).ShouldBeSameAs(html);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Transform_EmptyContent_IsReturnedAsItCame(string? html)
    {
        Transform(html!).ShouldBe(html);
    }
}
