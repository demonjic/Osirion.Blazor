using Microsoft.Extensions.Logging;

namespace Osirion.Blazor.Core.Tests.Base;

/// <summary>
/// Collects the level and formatted message of every warning or worse written through any logger it creates. Lower
/// levels are left out because the renderer writes debug entries for every render.
/// </summary>
internal sealed class RecordingLoggerFactory : ILoggerFactory
{
    private readonly RecordingLogger _logger = new();

    public IReadOnlyList<(LogLevel Level, string Message)> Entries => _logger.Entries;

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose()
    {
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Entries.Add((logLevel, formatter(state, exception)));
            }
        }
    }
}
