using Api;
using Microsoft.Extensions.Logging;

namespace Etl;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Migration stage {etlStageType} started")]
    internal static partial void StartEtl(this ILogger logger, EtlStageType etlStageType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Found {size} records")]
    internal static partial void FoundRecords(this ILogger logger, int size);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Skipped {offset} records")]
    internal static partial void SkippedRecords(this ILogger logger, int offset);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Ingested {ingested} out of {total} records")]
    internal static partial void IngestedRecords(this ILogger logger, int ingested, int total);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "{code} migration started")]
    internal static partial void MigrationStarted(this ILogger logger, string code);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Migration finished")]
    internal static partial void MigrationFinished(this ILogger logger);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Migration stage {stage} skipped")]
    internal static partial void EtlStageSkipped(this ILogger logger, EtlStageType stage);
}
