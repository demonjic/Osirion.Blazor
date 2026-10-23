using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Osirion.Blazor.Components;
using Shouldly;

namespace Osirion.Blazor.Core.Tests.Base;

/// <summary>
/// The Development warning for a captured attribute that resembles a parameter name. Each test uses its own attribute
/// names because the warning is logged once per component type and name for the whole process.
/// </summary>
public class OsirionComponentBaseUnknownParameterTests : OsirionComponentTestBase
{
    private readonly RecordingLoggerFactory _loggerFactory = new();

    public OsirionComponentBaseUnknownParameterTests()
    {
        Services.AddSingleton<ILoggerFactory>(_loggerFactory);
    }

    // Fails before the fix: the misspelled parameter was passed through without any warning.
    [Fact]
    public void MisspelledParameter_InDevelopment_LogsWarningNamingTheParameter()
    {
        UseEnvironment(Environments.Development);

        RenderStatic<OsirionHtmlRenderer>(parameters => parameters.AddUnmatched("Html", "<p>x</p>"));

        var warning = _loggerFactory.Entries.ShouldHaveSingleItem();
        warning.Level.ShouldBe(LogLevel.Warning);
        warning.Message.ShouldBe(
            "OsirionHtmlRenderer has no parameter 'Html'; did you mean 'HtmlContent'? The value was passed through as an HTML attribute.");
    }

    [Fact]
    public void MisspelledParameter_OutsideDevelopment_LogsNothing()
    {
        UseEnvironment(Environments.Production);

        RenderStatic<OsirionHtmlRenderer>(parameters => parameters.AddUnmatched("ShowLineNumber", true));

        _loggerFactory.Entries.ShouldBeEmpty();
    }

    // Fails before the fix: the control misspelling was not reported either.
    [Fact]
    public void HtmlAttributes_InDevelopment_AreNotReported()
    {
        UseEnvironment(Environments.Development);

        RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .AddUnmatched("id", "intro")
            .AddUnmatched("aria-label", "Introduction")
            .AddUnmatched("data-x", "1")
            .AddUnmatched("class", "lead")
            .AddUnmatched("title", "Intro")
            .AddUnmatched("onclick", "track()")
            .AddUnmatched("HtmlContnt", "<p>x</p>"));

        _loggerFactory.Entries.ShouldHaveSingleItem().Message.ShouldContain("'HtmlContnt'");
    }

    // Fails before the fix: the control misspelling was not reported either.
    [Fact]
    public void AttributeResemblingNoParameter_InDevelopment_IsNotReported()
    {
        UseEnvironment(Environments.Development);

        RenderStatic<OsirionHtmlRenderer>(parameters => parameters
            .AddUnmatched("Placement", "top")
            .AddUnmatched("SanitiseHtml", false));

        _loggerFactory.Entries.ShouldHaveSingleItem().Message
            .ShouldContain("has no parameter 'SanitiseHtml'; did you mean 'SanitizeHtml'?");
    }

    // Fails before the fix: nothing was logged at all.
    [Fact]
    public void MisspelledParameter_RenderedRepeatedly_IsLoggedOnce()
    {
        UseEnvironment(Environments.Development);

        var cut = RenderStatic<OsirionHtmlRenderer>(parameters => parameters.AddUnmatched("EnableDemo", false));
        cut.Render(parameters => parameters.AddUnmatched("EnableDemo", true));
        RenderStatic<OsirionHtmlRenderer>(parameters => parameters.AddUnmatched("EnableDemo", false));

        _loggerFactory.Entries.ShouldHaveSingleItem().Message.ShouldContain("did you mean 'EnableDemos'?");
    }
}
