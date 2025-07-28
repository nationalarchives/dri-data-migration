using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Ingesting {size} access conditions")]
    internal static partial void IngestingAccessConditions(this ILogger logger, int size);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Ingesting {size} legislations")]
    internal static partial void IngestingLegislations(this ILogger logger, int size);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Ingesting {size} grounds for retention")]
    internal static partial void IngestingGroundsForRetention(this ILogger logger, int size);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Ingesting {size} subsets")]
    internal static partial void IngestingSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Ingesting {size} assets")]
    internal static partial void IngestingAssets(this ILogger logger, int size);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Ingesting {size} variations")]
    internal static partial void IngestingVariations(this ILogger logger, int size);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Ingesting {size} sensitivity reviews")]
    internal static partial void IngestingSensitivityReview(this ILogger logger, int size);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Ingested {size} access conditions")]
    internal static partial void IngestedAccessConditions(this ILogger logger, int size);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Ingested {size} legislations")]
    internal static partial void IngestedLegislations(this ILogger logger, int size);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Ingested {size} grounds for retention")]
    internal static partial void IngestedGroundsForRetention(this ILogger logger, int size);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Ingested {size} subsets")]
    internal static partial void IngestedSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Ingested {size} assets")]
    internal static partial void IngestedAssets(this ILogger logger, int size);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Ingested {size} variations")]
    internal static partial void IngestedVariations(this ILogger logger, int size);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Ingested {size} sensitivity reviews")]
    internal static partial void IngestedSensitivityReview(this ILogger logger, int size);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "Migration started")]
    internal static partial void MigrationStarted(this ILogger logger);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Migration finished")]
    internal static partial void MigrationFinished(this ILogger logger);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "Started `{mapType}` reconciliation against `{fileLocation}`")]
    internal static partial void ReconciliationStarted(this ILogger logger, MapType mapType, string fileLocation);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "Finished `{mapType}` reconciliation")]
    internal static partial void ReconciliationFinished(this ILogger logger, MapType mapType);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "`{id}` not found in the staging database")]
    internal static partial void ReconciliationNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 20, Level = LogLevel.Warning, Message = "`{id}` fields[{diffs}] are different in the staging database")]
    internal static partial void ReconciliationDiff(this ILogger logger, string id, IEnumerable<ReconciliationFieldName> diffs);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Additional `{id}` in the staging database")]
    internal static partial void ReconciliationAdditional(this ILogger logger, string id);
}
