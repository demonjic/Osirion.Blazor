using System.Xml.Linq;
using Shouldly;

namespace Osirion.Blazor.Cms.Admin.Tests.Packaging;

/// <summary>
/// The package's build file that lets a site leave the admin's scoped styles out of its <c>{App}.styles.css</c>. The
/// build itself is checked by hand with a site that references the packed packages (see the pull request); these tests
/// keep the file packed and its switch off unless a site turns it on.
/// </summary>
public class AdminStylesOptOutTests
{
    private const string TargetsName = "Osirion.Blazor.Cms.Admin.targets";

    private static readonly string ProjectFolder = Path.Combine(FindRoot(), "src", "Osirion.Blazor.Cms.Admin");

    // Fails before the fix: no file let a site drop the admin's 70 KB bundle, which every public page downloaded
    // through the app's scoped style bundle because the umbrella package references the admin.
    [Fact]
    public void Package_ShipsTheTargetsFileToEveryConsumer()
    {
        // Arrange
        var project = XDocument.Load(Path.Combine(ProjectFolder, "Osirion.Blazor.Cms.Admin.csproj"));

        // Act
        var packed = project.Descendants("None")
            .SingleOrDefault(item => ((string?)item.Attribute("Include"))?.Replace('/', '\\') == $"buildTransitive\\{TargetsName}");

        // Assert: buildTransitive reaches sites that get the admin through Osirion.Blazor or Osirion.Blazor.Cms
        packed.ShouldNotBeNull();
        ((string?)packed.Attribute("Pack")).ShouldBe("true");
        ((string?)packed.Attribute("PackagePath"))!.TrimEnd('\\', '/').ShouldBe("buildTransitive");
        File.Exists(Path.Combine(ProjectFolder, "buildTransitive", TargetsName)).ShouldBeTrue();
    }

    [Fact]
    public void TargetsFile_RemovesOnlyTheAdminBundle_AndOnlyWhenTheSiteSetsTheSwitchToFalse()
    {
        // Arrange
        var targets = XDocument.Load(Path.Combine(ProjectFolder, "buildTransitive", TargetsName));

        // Act
        var target = targets.Root!.Elements("Target").Single();
        var removal = target.Descendants("_ScopedCssProjectBundles").Single();

        // Assert: it runs once the SDK has collected the referenced bundles, before it writes the app bundle
        ((string?)target.Attribute("AfterTargets")).ShouldBe("_CollectAllScopedCssAssets");
        ((string?)target.Attribute("Condition")).ShouldBe("'$(OsirionCmsAdminStyles)' == 'false'");
        ((string?)removal.Attribute("Remove")).ShouldBe("@(_ScopedCssProjectBundles)");
        ((string?)removal.Attribute("Condition")).ShouldBe("'%(_ScopedCssProjectBundles.SourceId)' == 'Osirion.Blazor.Cms.Admin'");
    }

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
