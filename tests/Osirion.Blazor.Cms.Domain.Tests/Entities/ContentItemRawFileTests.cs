using Osirion.Blazor.Cms.Domain.Entities;
using Osirion.Blazor.Cms.Domain.Repositories;
using Shouldly;

namespace Osirion.Blazor.Cms.Domain.Tests.Entities;

public class ContentItemRawFileTests
{
    [Theory]
    [InlineData("robots.txt", true)]
    [InlineData("en/ROBOTS.TXT", true)]
    [InlineData("data/menu.json", true)]
    [InlineData("config/site.yml", true)]
    [InlineData("en/about.md", false)]
    [InlineData("en/about.markdown", false)]
    [InlineData("", false)]
    public void IsRawFile_ReadsTheFileExtension(string path, bool expected)
    {
        var item = new ContentItem { Path = path };

        item.IsRawFile.ShouldBe(expected);
    }

    [Fact]
    public void ContentQuery_Clone_KeepsTheAddressAndTheRawFileFlag()
    {
        var query = new ContentQuery { Url = "robots", IncludeRawFiles = true };

        var clone = query.Clone();

        clone.Url.ShouldBe("robots");
        clone.IncludeRawFiles.ShouldBeTrue();
    }
}
