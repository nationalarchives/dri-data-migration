using Api;
using Microsoft.Extensions.Logging;

namespace Dri;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetching {stage} records")]
    internal static partial void FetchingRecords(this ILogger logger, EtlStageType stage);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Fetching {stage} records at {offset}")]
    internal static partial void FetchingRecordsOffset(this ILogger logger, EtlStageType stage, int offset);
}
