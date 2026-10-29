using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Osirion.Blazor.Cms.Domain.ContentBlocks;
using Osirion.Blazor.Cms.Domain.ValueObjects;
using Osirion.Blazor.Cms.Web.Components;
using Osirion.Blazor.Cms.Web.Tests.Base;
using Osirion.Blazor.Core.Localization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Osirion.Blazor.Cms.Web.Tests.ContentBlocks;

/// <summary>
/// Renders blocks read from YAML the way a content file is read, with a log that keeps messages and switches for the
/// Development environment and the locale paths.
/// </summary>
public abstract class ContentBlockTestBase : CmsWebComponentTestBase
{
    protected const string SourcePath = "en/home.md";

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    protected ContentBlockTestBase()
    {
        Services.AddSingleton<ILoggerFactory>(Logs);
        Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
    }

    internal MessageRecordingLoggerFactory Logs { get; } = new();

    protected static ContentBlockSet Read(string yaml) =>
        ContentBlockParser.Parse(Deserializer.Deserialize<FrontMatter>(yaml), SourcePath);

    protected static IReadOnlyList<ContentBlock> Blocks(string yaml) => Read(yaml).Blocks;

    /// <summary>Makes the app run in Development, where problems render a notice.</summary>
    protected void UseDevelopment()
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        Services.AddSingleton(environment);
    }

    /// <summary>Registers locale paths that put every site path under <c>/sr</c>.</summary>
    protected void UseSerbianLocalePaths()
    {
        var paths = Substitute.For<IOsirionLocalePaths>();
        paths.LocalizeForCurrentCulture(Arg.Any<string>()).Returns(call => "/sr" + call.Arg<string>());
        Services.AddSingleton(paths);
    }

    protected IRenderedComponent<OsirionContentBlocks> RenderBlocks(string yaml, bool includeFaqStructuredData = true) =>
        RenderStatic<OsirionContentBlocks>(p => p
            .Add(c => c.Blocks, Blocks(yaml))
            .Add(c => c.SourcePath, SourcePath)
            .Add(c => c.IncludeFaqStructuredData, includeFaqStructuredData));
}

/// <summary>Keeps the level and the formatted message of every entry written through its loggers.</summary>
internal sealed class MessageRecordingLoggerFactory : ILoggerFactory, ILogger
{
    private readonly List<(LogLevel Level, string Message)> _entries = [];

    public IReadOnlyList<(LogLevel Level, string Message)> Entries => _entries;

    /// <summary>Gets the warnings and errors, leaving out what other components log at lower levels.</summary>
    public IReadOnlyList<(LogLevel Level, string Message)> Problems => _entries.Where(entry => entry.Level >= LogLevel.Warning).ToList();

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName) => this;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        _entries.Add((logLevel, formatter(state, exception)));

    public void Dispose()
    {
    }
}
