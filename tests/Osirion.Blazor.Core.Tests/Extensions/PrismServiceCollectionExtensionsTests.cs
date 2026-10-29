using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Osirion.Blazor.Core.Configuration;
using Osirion.Blazor.Core.Extensions;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Extensions;

public class PrismServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/lib/prism/1.29.0/")]
    [InlineData("https://static.example.com/prism/1.29.0")]
    public void AddOsirionPrism_PathOnTheSiteOrHttpsAddress_IsAccepted(string? baseUrl)
    {
        // Arrange
        using var provider = new ServiceCollection()
            .AddOsirionPrism(options => options.BaseUrl = baseUrl)
            .BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<PrismOptions>>().Value;

        // Assert
        options.BaseUrl.ShouldBe(baseUrl);
    }

    // A script address from configuration: a protocol-relative, plain http or javascript: value would load code from
    // an unintended origin or not at all, so it fails when the options are read (at startup, with ValidateOnStart).
    [Theory]
    [InlineData("//cdn.example.com/prism/")]
    [InlineData("http://cdn.example.com/prism/")]
    [InlineData("javascript:alert(1)")]
    [InlineData("lib/prism/")]
    // Browsers strip tabs and line breaks from an address before parsing it, so this becomes //cdn.example.com
    [InlineData("/\t/cdn.example.com/prism/")]
    [InlineData("/\n/cdn.example.com/prism/")]
    public void AddOsirionPrism_OtherAddress_FailsValidation(string baseUrl)
    {
        // Arrange
        using var provider = new ServiceCollection()
            .AddOsirionPrism(options => options.BaseUrl = baseUrl)
            .BuildServiceProvider();

        // Act
        var exception = Should.Throw<OptionsValidationException>(() => provider.GetRequiredService<IOptions<PrismOptions>>().Value);

        // Assert
        exception.Message.ShouldContain("BaseUrl");
    }

    [Fact]
    public void AddOsirionPrism_NullConfigure_Throws()
    {
        Should.Throw<ArgumentNullException>(() => new ServiceCollection().AddOsirionPrism(null!));
    }
}
