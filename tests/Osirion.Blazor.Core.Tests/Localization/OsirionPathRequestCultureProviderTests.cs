using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Osirion.Blazor.Core.Localization;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Localization;

public class OsirionPathRequestCultureProviderTests
{
    private readonly OsirionPathRequestCultureProvider _provider = new(LocalePaths.Create());

    [Theory]
    [InlineData("/sr/contact", "sr-Latn")]
    [InlineData("/SR/contact", "sr-Latn")]
    [InlineData("/de", "de")]
    [InlineData("/contact", "en")]
    [InlineData("/", "en")]
    [InlineData("/srpski", "en")]
    public async Task DetermineProviderCultureResult_TakesTheCultureFromThePathPrefix(string path, string expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var result = await _provider.DetermineProviderCultureResult(context);

        result.ShouldNotBeNull();
        result.Cultures.ShouldHaveSingleItem().Value.ShouldBe(expected);
        result.UICultures.ShouldHaveSingleItem().Value.ShouldBe(expected);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_StatusCodePageReExecution_ReadsTheOriginalPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/not-found";
        var feature = Substitute.For<IStatusCodeReExecuteFeature>();
        feature.OriginalPath.Returns("/sr/missing");
        context.Features.Set(feature);

        var result = await _provider.DetermineProviderCultureResult(context);

        result!.UICultures.ShouldHaveSingleItem().Value.ShouldBe("sr-Latn");
    }

    [Fact]
    public async Task DetermineProviderCultureResult_ExceptionHandlerReExecution_ReadsTheOriginalPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/error";
        var feature = Substitute.For<IExceptionHandlerPathFeature>();
        feature.Path.Returns("/de/broken");
        context.Features.Set(feature);

        var result = await _provider.DetermineProviderCultureResult(context);

        result!.UICultures.ShouldHaveSingleItem().Value.ShouldBe("de");
    }
}
