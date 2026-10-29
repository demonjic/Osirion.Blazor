using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.Exceptions;
using Osirion.Blazor.Cms.Domain.Repositories;
using Osirion.Blazor.Cms.Infrastructure.DependencyInjection;
using Osirion.Blazor.Cms.Infrastructure.FileSystem;
using Osirion.Blazor.Cms.Infrastructure.Providers;
using Shouldly;

namespace Osirion.Blazor.Cms.Infrastructure.Tests.Localization;

/// <summary>
/// A FileSystem provider registered the way a site registers it, with a derived locale in its settings, over a real
/// folder of Markdown files.
/// </summary>
public sealed class FileSystemDerivedLocaleTests : IDisposable
{
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "osirion-derived-" + Guid.NewGuid().ToString("N"));
    private ServiceProvider? _services;

    public FileSystemDerivedLocaleTests()
    {
        System.IO.Directory.CreateDirectory(_basePath);
    }

    public void Dispose()
    {
        _services?.Dispose();
        System.IO.Directory.Delete(_basePath, recursive: true);
    }

    [Fact]
    public async Task GetAllItemsAsync_CyrillicPage_IsServedInLatinToo()
    {
        Write("sr/blog/prvi.md", "Први пост", "tags: [Вести]\n", "Добро дошли на [блог](/sr/blog).\n\n```\nкод\n```\n");
        var provider = CreateProvider();

        var items = await provider.GetAllItemsAsync();

        var source = items.Single(item => item.Locale == "sr");
        var derived = items.Single(item => item.Locale == "sr-Latn");
        derived.IsDerived.ShouldBeTrue();
        derived.Title.ShouldBe("Prvi post");
        derived.Tags.ShouldBe(["Vesti"]);
        derived.ContentId.ShouldBe(source.ContentId);
        derived.Url.ShouldBe(source.Url);
        derived.Path.ShouldBe("sr-Latn/blog/prvi.md");
        derived.Content.ShouldContain("Dobro došli na <a href=\"/sr-latn/blog\">blog</a>.");
        derived.Content.ShouldContain("<code>код");
    }

    [Fact]
    public async Task GetAllItemsAsync_CyrillicPageWithBlocks_HasALatinTwinWithTransliteratedBlocks()
    {
        Write("sr/pocetna.md", "Почетна", """
            body: after
            blocks:
              - type: hero
                id: vrh
                variant: dark
                eyebrow: "Систем"
                title: "Свака одлука"
                subtitle: "Приступ и присуство"
                highlights: ["Брзо"]
                primary_action: { text: "Закажите", url: "/kontakt" }
                image: { src: "/slike/a.png", alt: "Погон" }
              - type: cards
                columns: 2
                items:
                  - { title: "Правила", description: "Опис", icon: shield, url: "/platforma", link_text: "Више", items: ["Ставка"] }
              - type: faq
                items: [{ question: "Зашто?", answer: "Зато **што**." }]
              - type: component
                name: obilazak
                parameters: { naslov: "Ћирилица" }
              - type: markdown
                markdown: "Види [страну](/sr/страна) и `код`."

            """, "Тело.\n");
        var provider = CreateProvider();

        var items = await provider.GetAllItemsAsync();

        var source = items.Single(item => item.Locale == "sr");
        var derived = items.Single(item => item.Locale == "sr-Latn");
        derived.BodyPlacement.ShouldBe(ContentBodyPlacement.AfterBlocks);
        var hero = derived.Blocks[0].ShouldBeOfType<HeroBlock>();
        (hero.Id, hero.Variant, hero.Eyebrow, hero.Title, hero.Subtitle).ShouldBe(("vrh", ContentBlockVariant.Dark, "Sistem", "Svaka odluka", "Pristup i prisustvo"));
        hero.Highlights.ShouldBe(["Brzo"]);
        hero.PrimaryAction.ShouldBe(new ContentBlockAction("Zakažite", "/kontakt"));
        hero.Image.ShouldBe(new ContentBlockImage("/slike/a.png", "Pogon"));
        var card = derived.Blocks[1].ShouldBeOfType<CardsBlock>().Items.ShouldHaveSingleItem();
        (card.Title, card.Description, card.Icon, card.Url, card.LinkText).ShouldBe(("Pravila", "Opis", "shield", "/platforma", "Više"));
        card.Items.ShouldBe(["Stavka"]);
        derived.Blocks[2].ShouldBeOfType<FaqBlock>().Items.ShouldBe([new FaqBlockItem("Zašto?", "Zato **što**.")]);
        var component = derived.Blocks[3].ShouldBeOfType<ComponentBlock>();
        component.Name.ShouldBe("obilazak");
        component.Parameters["naslov"].ShouldBe("Ћирилица");
        derived.Blocks[4].ShouldBeOfType<MarkdownBlock>().Markdown.ShouldBe("Vidi [stranu](/sr/страна) i `код`.");
        source.Blocks[0].ShouldBeOfType<HeroBlock>().Title.ShouldBe("Свака одлука");
    }

    [Fact]
    public async Task GetAllItemsAsync_FileInTheDerivedLocaleFolder_WinsOverTheDerivedItem()
    {
        Write("sr/o-nama.md", "О нама", string.Empty, "Текст.\n");
        Write("sr-latn/o-nama.md", "O nama (ručno)", string.Empty, "Ručno napisan tekst.\n");
        var provider = CreateProvider();

        var items = await provider.GetAllItemsAsync();

        items.Count.ShouldBe(2);
        var latin = items.Single(item => item.Locale.Equals("sr-Latn", StringComparison.OrdinalIgnoreCase));
        latin.IsDerived.ShouldBeFalse();
        latin.Title.ShouldBe("O nama (ručno)");
    }

    [Fact]
    public async Task GetLocalizationInfoAsync_DerivedItemsExist_ReportsTheDerivedLocale()
    {
        Write("sr/o-nama.md", "О нама", string.Empty, "Текст.\n");
        var provider = CreateProvider();

        var info = await provider.GetLocalizationInfoAsync();

        info.AvailableLocales.Select(locale => locale.Code).ShouldBe(["sr", "sr-Latn"]);
    }

    [Fact]
    public async Task GetContentTranslationsAsync_IncludesTheDerivedItem()
    {
        Write("sr/o-nama.md", "О нама", string.Empty, "Текст.\n");
        var provider = CreateProvider();
        var source = (await provider.GetAllItemsAsync()).Single(item => item.Locale == "sr");

        var translations = await provider.GetContentTranslationsAsync(source.ContentId);

        translations.Keys.ShouldBe(["sr", "sr-Latn"], ignoreOrder: true);
        translations["sr-Latn"].Title.ShouldBe("O nama");
    }

    [Fact]
    public async Task GetItemsByQueryAsync_DerivedLocale_ListsTheDerivedItemsByTagAndCategory()
    {
        Write("sr/vesti/prva.md", "Прва вест", "categories: [Новости]\ntags: [Важно]\n", "Текст.\n");
        Write("sr/vesti/druga.md", "Друга вест", "categories: [Остало]\n", "Текст.\n");
        var provider = CreateProvider();

        var byLocale = await provider.GetItemsByQueryAsync(new ContentQuery { Locale = "sr-Latn" });
        var byTag = await provider.GetItemsByQueryAsync(new ContentQuery { Locale = "sr-Latn", Tag = "Važno" });
        var byCategory = await provider.GetItemsByQueryAsync(new ContentQuery { Locale = "sr-Latn", Category = "Novosti" });

        byLocale!.Select(item => item.Title).ShouldBe(["Prva vest", "Druga vest"], ignoreOrder: true);
        byTag!.Single().Title.ShouldBe("Prva vest");
        byCategory!.Single().Title.ShouldBe("Prva vest");
        (await provider.GetTagsAsync()).Select(tag => tag.Name).ShouldContain("Važno");
        (await provider.GetCategoriesAsync()).Select(category => category.Name).ShouldContain("Novosti");
    }

    [Fact]
    public async Task SaveAsync_DerivedItem_IsRefusedAndNoFileIsWritten()
    {
        Write("sr/o-nama.md", "О нама", string.Empty, "Текст.\n");
        CreateProvider();
        var repository = _services!.GetRequiredService<FileSystemContentRepository>();
        var derived = (await repository.GetAllAsync()).Single(item => item.IsDerived);

        var exception = await Should.ThrowAsync<ContentProviderException>(() => repository.SaveAsync(derived));

        exception.Message.ShouldContain("read only");
        System.IO.Directory.Exists(Path.Combine(_basePath, "sr-Latn")).ShouldBeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DerivedItem_IsRefused()
    {
        Write("sr/o-nama.md", "О нама", string.Empty, "Текст.\n");
        CreateProvider();
        var repository = _services!.GetRequiredService<FileSystemContentRepository>();
        var derived = (await repository.GetAllAsync()).Single(item => item.IsDerived);

        var exception = await Should.ThrowAsync<ContentProviderException>(() => repository.DeleteAsync(derived.Id));

        exception.Message.ShouldContain("read only");
        File.Exists(Path.Combine(_basePath, "sr", "o-nama.md")).ShouldBeTrue();
    }

    [Fact]
    public void Resolve_InvalidDerivedLocale_FailsWithTheKeyNamed()
    {
        var provider = BuildServices(new Dictionary<string, string?>
        {
            ["Osirion:Cms:Web:FileSystem:DerivedLocales:0:Transliteration"] = "unknown",
        });

        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<FileSystemContentProvider>());

        exception.Failures.ShouldContain(failure => failure.StartsWith("DerivedLocales[0].Transliteration 'unknown'", StringComparison.Ordinal));
    }

    private FileSystemContentProvider CreateProvider() =>
        BuildServices([]).GetRequiredService<FileSystemContentProvider>();

    private ServiceProvider BuildServices(Dictionary<string, string?> overrides)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Osirion:Cms:Web:FileSystem:BasePath"] = _basePath,
            ["Osirion:Cms:Web:FileSystem:WatchForChanges"] = "false",
            ["Osirion:Cms:Web:FileSystem:EnableLocalization"] = "true",
            ["Osirion:Cms:Web:FileSystem:DefaultLocale"] = "sr",
            ["Osirion:Cms:Web:FileSystem:SupportedLocales:0"] = "sr",
            ["Osirion:Cms:Web:FileSystem:DerivedLocales:0:Locale"] = "sr-Latn",
            ["Osirion:Cms:Web:FileSystem:DerivedLocales:0:From"] = "sr",
            ["Osirion:Cms:Web:FileSystem:DerivedLocales:0:Transliteration"] = "serbian-cyrillic-to-latin",
        };
        foreach (var (key, value) in overrides)
        {
            settings[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCms(configuration);
        services.AddFileSystemContentProvider(configuration);

        _services = services.BuildServiceProvider();
        return _services;
    }

    private void Write(string relativePath, string title, string frontMatter, string body)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, $"---\ntitle: \"{title}\"\n{frontMatter}---\n\n{body}");
    }
}
