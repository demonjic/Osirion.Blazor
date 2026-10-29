using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Enums;
using Osirion.Blazor.Cms.Domain.Options;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Infrastructure.Localization;
using Osirion.Blazor.Cms.Infrastructure.Utilities;
using Osirion.Blazor.Core.Extensions;
using Osirion.Blazor.Core.Localization;
using Osirion.Blazor.Core.Transliteration;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Localization;

/// <summary>
/// The step both content repositories run after a load: <see cref="DerivedLocaleGenerator.AddDerivedItems"/>.
/// </summary>
public class DerivedLocaleGeneratorTests
{
    private const string ProviderId = "provider";
    private static readonly DateTime Created = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Modified = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Utc);

    private readonly DerivedLocaleGenerator _generator = new(
        new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]),
        NullLogger<DerivedLocaleGenerator>.Instance);

    [Fact]
    public void AddDerivedItems_SourceItem_GetsAnItemInTheDerivedLocale()
    {
        var source = Item("sr/blog/prvi.md", "sr", "blog/prvi", "Први пост");
        var items = Cache(source);

        _generator.AddDerivedItems(items, Options(), contentPath: string.Empty);

        items.Count.ShouldBe(2);
        var derived = items.Values.Single(item => item.Locale == "sr-Latn");
        derived.Title.ShouldBe("Prvi post");
        derived.ContentId.ShouldBe(source.ContentId);
        derived.Path.ShouldBe("sr-Latn/blog/prvi.md");
        derived.Url.ShouldBe("blog/prvi");
        derived.Slug.ShouldBe(source.Slug);
        derived.Id.ShouldBe(IdGenerator.GenerateStableId("sr-Latn/blog/prvi.md", ProviderId));
        derived.ProviderId.ShouldBe(ProviderId);
    }

    [Fact]
    public void AddDerivedItems_DerivedItem_IsMarkedAndHasNoFile()
    {
        var items = Cache(Item("sr/o-nama.md", "sr", "o-nama", "О нама"));

        _generator.AddDerivedItems(items, Options(), string.Empty);

        var derived = items.Values.Single(item => item.Locale == "sr-Latn");
        derived.IsDerived.ShouldBeTrue();
        derived.DerivedFromLocale.ShouldBe("sr");
        derived.ProviderSpecificId.ShouldBeNull();
        derived.OriginalMarkdown.ShouldBeNull();
        items.Values.Single(item => item.Locale == "sr").IsDerived.ShouldBeFalse();
    }

    [Fact]
    public void AddDerivedItems_RealFileInTheDerivedLocale_Wins()
    {
        var source = Item("sr/o-nama.md", "sr", "o-nama", "О нама");
        // A folder named sr-latn is the derived locale too
        var real = Item("sr-latn/o-nama.md", "sr-latn", "o-nama", "O nama, napisano rukom");
        var items = Cache(source, real);

        _generator.AddDerivedItems(items, Options(), string.Empty);

        items.Count.ShouldBe(2);
        items.Values.ShouldNotContain(item => item.IsDerived);
    }

    [Fact]
    public void AddDerivedItems_OnlyTheItemsWithoutATranslation_AreDerived()
    {
        var items = Cache(
            Item("sr/a.md", "sr", "a", "А"),
            Item("sr/b.md", "sr", "b", "Б"),
            Item("sr-Latn/b.md", "sr-Latn", "b", "B ručno"),
            Item("en/a.md", "en", "a", "A"));

        _generator.AddDerivedItems(items, Options(), string.Empty);

        items.Values.Where(item => item.IsDerived).Select(item => item.Path).ShouldBe(["sr-Latn/a.md"]);
    }

    [Fact]
    public void AddDerivedItems_ItemOfTheDefaultLocaleOutsideALocaleFolder_GetsTheDerivedLocaleFolder()
    {
        var items = Cache(Item("blog/prvi.md", "sr", "blog/prvi", "Први"));

        _generator.AddDerivedItems(items, Options(), string.Empty);

        var derived = items.Values.Single(item => item.IsDerived);
        derived.Path.ShouldBe("sr-Latn/blog/prvi.md");
        derived.Url.ShouldBe("blog/prvi");
    }

    [Fact]
    public void AddDerivedItems_PathsUnderAContentFolder_KeepTheContentFolder()
    {
        var items = Cache(Item("content/sr/blog/prvi.md", "sr", "blog/prvi", "Први"));

        _generator.AddDerivedItems(items, Options(), contentPath: "content");

        var derived = items.Values.Single(item => item.IsDerived);
        derived.Path.ShouldBe("content/sr-Latn/blog/prvi.md");
        derived.Url.ShouldBe("blog/prvi");
    }

    [Fact]
    public void AddDerivedItems_Permalink_IsKept()
    {
        var source = Item("sr/kontakt.md", "sr", "kontakt", "Контакт");
        source.Metadata!.Permalink = "/kontakt-strana/";
        var items = Cache(source);

        _generator.AddDerivedItems(items, Options(), string.Empty);

        items.Values.Single(item => item.IsDerived).Url.ShouldBe("kontakt-strana");
    }

    [Fact]
    public void AddDerivedItems_TextFields_AreTransliterated()
    {
        var source = Item("sr/vodic.md", "sr", "vodic", "Водич");
        source.SetDescription("Кратак опис");
        source.Metadata!.Description = "Кратак опис";
        source.AddCategory("Упутства");
        source.AddTag("Почетак");
        source.Metadata.Categories = ["Упутства"];
        source.Metadata.Tags = ["Почетак"];
        source.Metadata.Series = "Основе";
        source.Metadata.SeoProperties.Title = "СЕО наслов";
        source.Metadata.SeoProperties.Description = "СЕО опис";
        source.Metadata.SeoProperties.OgTitle = "Друштвени наслов";
        var items = Cache(source);

        _generator.AddDerivedItems(items, Options(), string.Empty);

        var derived = items.Values.Single(item => item.IsDerived);
        derived.Description.ShouldBe("Kratak opis");
        derived.Categories.ShouldBe(["Uputstva"]);
        derived.Tags.ShouldBe(["Početak"]);
        derived.Metadata!.Title.ShouldBe("Vodič");
        derived.Metadata.Description.ShouldBe("Kratak opis");
        derived.Metadata.Categories.ShouldBe(["Uputstva"]);
        derived.Metadata.Tags.ShouldBe(["Početak"]);
        derived.Metadata.Series.ShouldBe("Osnove");
        derived.Metadata.Lang.ShouldBe("sr-Latn");
        derived.Metadata.SeoProperties.Title.ShouldBe("SEO naslov");
        derived.Metadata.SeoProperties.Description.ShouldBe("SEO opis");
        derived.Metadata.SeoProperties.OgTitle.ShouldBe("Društveni naslov");
    }

    [Fact]
    public void AddDerivedItems_SourceItem_IsNotChanged()
    {
        var source = Item("sr/vodic.md", "sr", "vodic", "Водич");
        source.AddTag("Почетак");
        var items = Cache(source);

        _generator.AddDerivedItems(items, Options(), string.Empty);

        source.Title.ShouldBe("Водич");
        source.Tags.ShouldBe(["Почетак"]);
        source.Metadata!.Title.ShouldBe("Водич");
        source.Metadata.Lang.ShouldBe("sr");
        source.Content.ShouldBe("<p>Водич</p>");
    }

    [Fact]
    public void AddDerivedItems_Content_IsTransliteratedHtml()
    {
        var items = Cache(Item("sr/vodic.md", "sr", "vodic", "Водич"));

        _generator.AddDerivedItems(items, Options(), string.Empty);

        items.Values.Single(item => item.IsDerived).Content.ShouldBe("<p>Vodič</p>");
    }

    [Fact]
    public void AddDerivedItems_SiteLocalizationRegistered_MovesLinksUnderTheConfiguredPrefix()
    {
        var paths = new ServiceCollection()
            .AddOsirionLocalization(options =>
            {
                options.DefaultCulture = "en";
                options.Cultures.Add(new LocalizationCulture { Name = "sr" });
                options.Cultures.Add(new LocalizationCulture { Name = "sr-Latn", Prefix = "lat" });
            })
            .BuildServiceProvider()
            .GetRequiredService<IOsirionLocalePaths>();
        var generator = new DerivedLocaleGenerator(
            new TextTransliterators([new SerbianCyrillicToLatinTransliterator()]), paths, NullLogger<DerivedLocaleGenerator>.Instance);
        var source = Item("sr/vodic.md", "sr", "vodic", "Водич");
        source.SetContent("<p><a href=\"/sr/kontakt\">Контакт</a></p>");
        var items = Cache(source);

        generator.AddDerivedItems(items, Options(), string.Empty);

        items.Values.Single(item => item.IsDerived).Content.ShouldBe("<p><a href=\"/lat/kontakt\">Kontakt</a></p>");
    }

    [Fact]
    public void AddDerivedItems_DatesAuthorAndStatus_AreTheSource()
    {
        var source = Item("sr/nacrt.md", "sr", "nacrt", "Нацрт");
        source.SetAuthor("Аутор");
        source.SetStatus(ContentStatus.Draft);
        source.SetLastModifiedDate(Modified);
        var items = Cache(source);

        _generator.AddDerivedItems(items, Options(), string.Empty);

        var derived = items.Values.Single(item => item.IsDerived);
        derived.DateCreated.ShouldBe(Created);
        derived.LastModified.ShouldBe(Modified);
        derived.Author.ShouldBe("Аутор");
        derived.Status.ShouldBe(ContentStatus.Draft);
    }

    [Fact]
    public void AddDerivedItems_LocalizationDisabled_DerivesNothing()
    {
        var items = Cache(Item("sr/a.md", "sr", "a", "А"));
        var options = Options();
        options.EnableLocalization = false;

        _generator.AddDerivedItems(items, options, string.Empty);

        items.Count.ShouldBe(1);
    }

    [Fact]
    public void AddDerivedItems_NoDerivedLocales_DerivesNothing()
    {
        var items = Cache(Item("sr/a.md", "sr", "a", "А"));

        _generator.AddDerivedItems(items, new FileSystemOptions { EnableLocalization = true, SupportedLocales = ["sr"] }, string.Empty);

        items.Count.ShouldBe(1);
    }

    private static FileSystemOptions Options() => new()
    {
        EnableLocalization = true,
        DefaultLocale = "sr",
        SupportedLocales = ["sr", "en"],
        DerivedLocales =
        [
            new DerivedLocaleOptions
            {
                Locale = "sr-Latn",
                From = "sr",
                Transliteration = SerbianCyrillicToLatinTransliterator.TransliteratorName,
            },
        ],
    };

    private static Dictionary<string, ContentItem> Cache(params ContentItem[] items) =>
        items.ToDictionary(item => item.Id);

    private static ContentItem Item(string path, string locale, string url, string title)
    {
        var item = ContentItem.Create(IdGenerator.GenerateStableId(path, ProviderId), title, $"<p>{title}</p>", path, ProviderId);
        item.SetProviderSpecificId(path);
        item.Metadata = new FrontMatter { Title = title, Lang = locale };
        item.SetContent($"<p>{title}</p>", $"{title}\n");
        item.SetLocale(locale);
        item.SetSlug(Path.GetFileNameWithoutExtension(path));
        item.SetUrl(url);
        item.SetContentId("loc-" + url);
        item.SetCreatedDate(Created);
        return item;
    }
}
