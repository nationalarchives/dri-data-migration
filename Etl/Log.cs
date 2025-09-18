using Microsoft.Extensions.Logging;
using System;

namespace Etl;

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
    internal static partial void IngestingSensitivityReviews(this ILogger logger, int size);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Updated {size} access conditions")]
    internal static partial void IngestedAccessConditions(this ILogger logger, int size);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Updated {size} legislations")]
    internal static partial void IngestedLegislations(this ILogger logger, int size);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Updated {size} grounds for retention")]
    internal static partial void IngestedGroundsForRetention(this ILogger logger, int size);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Updated {size} subsets")]
    internal static partial void IngestedSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Updated {size} assets")]
    internal static partial void IngestedAssets(this ILogger logger, int size);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Updated {size} variations")]
    internal static partial void IngestedVariations(this ILogger logger, int size);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Updated {size} sensitivity reviews")]
    internal static partial void IngestedSensitivityReviews(this ILogger logger, int size);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "{code} migration started")]
    internal static partial void MigrationStarted(this ILogger logger, string code);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Migration finished")]
    internal static partial void MigrationFinished(this ILogger logger);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "Ingesting {size} deliverable units")]
    internal static partial void IngestingDeliverableUnits(this ILogger logger, int size);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "Updated {size} deliverable units")]
    internal static partial void IngestedDeliverableUnits(this ILogger logger, int size);

    [LoggerMessage(EventId = 19, Level = LogLevel.Information, Message = "Ingesting {size} files")]
    internal static partial void IngestingFiles(this ILogger logger, int size);

    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "Updated {size} files")]
    internal static partial void IngestedFiles(this ILogger logger, int size);

    [LoggerMessage(EventId = 21, Level = LogLevel.Critical, Message = "Migration failed")]
    internal static partial void MigrationFailed(this ILogger logger);

    [LoggerMessage(EventId = 22, Level = LogLevel.Debug)]
    internal static partial void MigrationFailedDetails(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 23, Level = LogLevel.Critical, Message = "Unhandled exception: {message}")]
    internal static partial void UnhandledException(this ILogger logger, string message);

    [LoggerMessage(EventId = 24, Level = LogLevel.Critical, Message = "Migration failed with message {message}")]
    internal static partial void MigrationFailedWithMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 25, Level = LogLevel.Critical, Message = "Process cancelled")]
    internal static partial void ProcessCancelled(this ILogger logger);

    [LoggerMessage(EventId = 26, Level = LogLevel.Information, Message = "Migration stage {stage} skipped")]
    internal static partial void EtlStageSkipped(this ILogger logger, Api.EtlStageType stage);
}
