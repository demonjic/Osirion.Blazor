using System.Text.Json;
using System.Xml.Linq;
using Shouldly;

namespace Osirion.Blazor.Templates.Tests;

public class TemplatePackageTests
{
    private static readonly string Root = FindRoot();
    private static readonly string TemplatesFolder = Path.Combine(Root, "src", "Osirion.Blazor.Templates", "templates");

    // Files every template uses, copied into each generated project through the templates' second source.
    private static readonly string SharedFolder = Path.Combine(TemplatesFolder, "_shared");

    public static TheoryData<string> Templates()
    {
        var data = new TheoryData<string>();
        foreach (var folder in TemplateFolders())
        {
            data.Add(Path.GetFileName(folder));
        }

        return data;
    }

    [Fact]
    public void Package_HasTheSameVersionAsTheLibrary()
    {
        // Act
        var package = Version(Path.Combine(Root, "src", "Osirion.Blazor.Templates", "Osirion.Blazor.Templates.csproj"));

        // Assert
        package.ShouldBe(LibraryVersion());
    }

    // A template that referenced an older library would create sites without the fixes released with it; a newer one
    // would not restore until that version is published.
    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_ReferencesTheLibraryVersionItIsReleasedWith(string template)
    {
        // Arrange
        var project = XDocument.Load(ProjectFile(template));

        // Act
        var reference = project.Descendants("PackageReference").Single(e => (string?)e.Attribute("Include") == "Osirion.Blazor");

        // Assert
        ((string?)reference.Attribute("Version")).ShouldBe(LibraryVersion());
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_ShortNameIsTheFolderName_AndItsProjectIsTheSourceName(string template)
    {
        // Act
        var config = TemplateConfig(template);

        // Assert
        config.GetProperty("shortName").GetString().ShouldBe(template);
        config.GetProperty("identity").GetString().ShouldStartWith("Osirion.Blazor.Templates.");
        var sourceName = config.GetProperty("sourceName").GetString();
        Path.GetFileNameWithoutExtension(ProjectFile(template)).ShouldBe(sourceName);
        config.GetProperty("primaryOutputs")[0].GetProperty("path").GetString().ShouldBe(sourceName + ".csproj");
    }

    // Fails before the fix: with sourceName "OsirionBlog" every form of the name was the same, so dotnet new wrote the
    // namespace form into the stylesheet link. A project called My.Blog-Site linked My.Blog_Site.styles.css while the
    // bundle is My.Blog-Site.styles.css, and the site rendered without the components' styles.
    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_SourceNameHasAHyphen_SoTheStylesheetLinkKeepsTheProjectName(string template)
    {
        // Arrange
        var sourceName = TemplateConfig(template).GetProperty("sourceName").GetString()!;
        var app = File.ReadAllText(Path.Combine(TemplatesFolder, template, "Components", "App.razor"));

        // Assert: the raw form differs from the namespace form, and the link uses the raw form
        sourceName.ShouldContain('-');
        app.ShouldContain($"@Assets[\"{sourceName}.styles.css\"]");
    }

    [Fact]
    public void Templates_HaveDistinctIdentities()
    {
        // Act
        var identities = TemplateFolders()
            .Select(folder => TemplateConfig(Path.GetFileName(folder)).GetProperty("identity").GetString())
            .ToList();

        // Assert
        identities.ShouldBeUnique();
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_HasNoBuildOutputOrLocalState(string template)
    {
        // Act: only what dotnet new should copy belongs in the package
        var stray = new[] { Path.Combine(TemplatesFolder, template), SharedFolder }
            .SelectMany(folder => Directory.EnumerateFileSystemEntries(folder, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(folder, path).Replace('\\', '/')))
            .Where(path => path.StartsWith("bin", StringComparison.Ordinal) || path.StartsWith("obj", StringComparison.Ordinal)
                || path.StartsWith(".vs", StringComparison.Ordinal) || path.EndsWith(".user", StringComparison.Ordinal))
            .ToList();

        // Assert
        stray.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_ParameterDefaults_AppearInTheFilesTheyReplace(string template)
    {
        // Arrange: a parameter whose default is in no file would silently do nothing
        var config = TemplateConfig(template);
        var files = Directory.EnumerateFiles(Path.Combine(TemplatesFolder, template), "*", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(SharedFolder, "*", SearchOption.AllDirectories))
            .Where(path => !path.Contains(".template.config", StringComparison.Ordinal))
            .Select(File.ReadAllText)
            .ToList();

        // Act
        var unused = config.GetProperty("symbols").EnumerateObject()
            .Where(symbol => symbol.Value.TryGetProperty("replaces", out _))
            .Where(symbol => !files.Any(text => text.Contains(symbol.Value.GetProperty("replaces").GetString()!, StringComparison.Ordinal)))
            .Select(symbol => symbol.Name)
            .ToList();

        // Assert
        unused.ShouldBeEmpty();
    }

    // With the startup code copied into each template, Sonar measured 10.9% duplication on the second one; the shared
    // folder holds what every template uses. A file in both places would leave dotnet new to pick one.
    [Theory]
    [MemberData(nameof(Templates))]
    public void Template_TakesTheSharedFilesAndRepeatsNoneOfThem(string template)
    {
        // Arrange
        var sources = TemplateConfig(template).GetProperty("sources").EnumerateArray()
            .Select(source => source.GetProperty("source").GetString())
            .ToList();
        var own = Directory.EnumerateFiles(Path.Combine(TemplatesFolder, template), "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Path.Combine(TemplatesFolder, template), path));
        var shared = Directory.EnumerateFiles(SharedFolder, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(SharedFolder, path));

        // Assert
        sources.ShouldBe(["./", "../_shared/"]);
        own.Intersect(shared).ShouldBeEmpty();
    }

    // Fails before the fix: a project called Blog.One-Site or Docs.Two-Site did not build, because inside Blog.razor
    // and Docs.razor the name Blog or Docs means the page itself, so "@using Blog.One_Site" looked for a nested type.
    [Fact]
    public void SharedImports_QualifyTheProjectNamespaceWithGlobal()
    {
        // Act
        var imports = File.ReadAllLines(Path.Combine(SharedFolder, "Components", "_Imports.razor"))
            .Where(line => line.StartsWith("@using", StringComparison.Ordinal) && line.Contains("OsirionApp_CSharp", StringComparison.Ordinal))
            .ToList();

        // Assert
        imports.ShouldNotBeEmpty();
        imports.ShouldAllBe(line => line.StartsWith("@using global::", StringComparison.Ordinal));
    }

    // Fails before the fix: content/docs had no _index.md, so the folder crumb of every docs page was the folder's file
    // name, "Home / docs / Introduction". The folder index names the crumb and links it to the docs route, which sends
    // the reader to the first page; unpublished, it is not a page of its own in the navigation, the pager or the sitemap.
    [Fact]
    public void DocsTemplate_FolderIndex_NamesTheDocsCrumb_AndLinksItToTheDocsRoute()
    {
        // Arrange
        var template = Path.Combine(TemplatesFolder, "osirion-docs");
        var index = Path.Combine(template, "content", "docs", "_index.md");
        File.Exists(index).ShouldBeTrue("content/docs/_index.md names the docs folder in the breadcrumbs");
        var docsPage = File.ReadAllText(Path.Combine(template, "Components", "Pages", "Docs.razor"));

        // Act
        var frontMatter = FrontMatter(index);

        // Assert
        frontMatter.ShouldContainKeyAndValue("title", "Documentation");
        frontMatter.ShouldContainKeyAndValue("permalink", "/docs");
        frontMatter.ShouldContainKeyAndValue("published", "false");
        docsPage.ShouldContain("@page \"/docs\"");
    }

    private static IEnumerable<string> TemplateFolders() =>
        Directory.GetDirectories(TemplatesFolder).Where(folder => Directory.Exists(Path.Combine(folder, ".template.config")));

    // The project file is shared; a template may still bring its own.
    private static string ProjectFile(string template) =>
        Directory.GetFiles(Path.Combine(TemplatesFolder, template), "*.csproj").SingleOrDefault()
        ?? Directory.GetFiles(SharedFolder, "*.csproj").Single();

    private static JsonElement TemplateConfig(string template) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(TemplatesFolder, template, ".template.config", "template.json"))).RootElement;

    // The key: value lines between the two --- lines that open a Markdown file, with the quotes around a value removed.
    private static Dictionary<string, string> FrontMatter(string markdownFile) =>
        File.ReadLines(markdownFile)
            .Skip(1)
            .TakeWhile(line => line != "---")
            .Select(line => line.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim().Trim('"'), StringComparer.Ordinal);

    private static string LibraryVersion() => Version(Path.Combine(Root, "src", "Osirion.Blazor", "Osirion.Blazor.csproj"));

    private static string Version(string projectFile) =>
        XDocument.Load(projectFile).Descendants("Version").Single().Value;

    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Osirion.Blazor.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Osirion.Blazor.slnx not found above " + AppContext.BaseDirectory);
    }
}
