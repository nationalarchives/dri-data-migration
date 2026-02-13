using Microsoft.Extensions.Logging;

namespace Migration;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Critical, Message = "Migration failed")]
    internal static partial void MigrationFailed(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug)]
    internal static partial void MigrationFailedDetails(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 3, Level = LogLevel.Critical, Message = "Unhandled exception: {message}")]
    internal static partial void UnhandledException(this ILogger logger, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Critical, Message = "Migration failed with message {message}")]
    internal static partial void MigrationFailedWithMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "Process cancelled")]
    internal static partial void ProcessCancelled(this ILogger logger);

}
