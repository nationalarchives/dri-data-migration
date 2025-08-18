using Microsoft.Extensions.Logging;

namespace DriSql;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetching deliverable units at {offset}")]
    internal static partial void GetDeliverableUnits(this ILogger logger, int offset);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Fetching files at {offset}")]
    internal static partial void GetFiles(this ILogger logger, int offset);
}
