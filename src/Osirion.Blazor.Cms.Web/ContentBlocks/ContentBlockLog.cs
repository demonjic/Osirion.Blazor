using Microsoft.Extensions.Logging;

namespace Osirion.Blazor.Cms.Web.ContentBlocks;

/// <summary>The log messages of content block rendering.</summary>
internal static partial class ContentBlockLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Content block {Location} of {SourcePath} is not rendered: {Reason}")]
    public static partial void BlockNotRendered(ILogger logger, string sourcePath, string location, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Content block {Location} of {SourcePath} has type {BlockType}, which has no renderer yet; it renders nothing")]
    public static partial void TypeAwaitingRenderer(ILogger logger, string sourcePath, string location, string blockType);
}
