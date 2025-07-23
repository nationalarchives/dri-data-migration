using Microsoft.Extensions.Logging;

namespace Rdf;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Record {id} updated")]
    internal static partial void RecordUpdated(this ILogger logger, string id);
}
