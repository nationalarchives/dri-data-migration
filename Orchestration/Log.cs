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

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Ingesting {size} top subsets")]
    internal static partial void IngestingBroadestSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Ingesting {size} subsets")]
    internal static partial void IngestingSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Ingesting {size} assets")]
    internal static partial void IngestingAssets(this ILogger logger, int size);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Ingesting {size} variations")]
    internal static partial void IngestingVariations(this ILogger logger, int size);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Ingesting {size} sensitivity reviews")]
    internal static partial void IngestingSensitivityReview(this ILogger logger, int size);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Updated {size} access conditions")]
    internal static partial void IngestedAccessConditions(this ILogger logger, int size);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Updated {size} legislations")]
    internal static partial void IngestedLegislations(this ILogger logger, int size);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Updated {size} grounds for retention")]
    internal static partial void IngestedGroundsForRetention(this ILogger logger, int size);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Updated {size} top subsets")]
    internal static partial void IngestedBroadestSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Updated {size} subsets")]
    internal static partial void IngestedSubsets(this ILogger logger, int size);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Updated {size} assets")]
    internal static partial void IngestedAssets(this ILogger logger, int size);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "Updated {size} variations")]
    internal static partial void IngestedVariations(this ILogger logger, int size);

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Updated {size} sensitivity reviews")]
    internal static partial void IngestedSensitivityReview(this ILogger logger, int size);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "{code} migration started")]
    internal static partial void MigrationStarted(this ILogger logger, string code);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "Migration finished")]
    internal static partial void MigrationFinished(this ILogger logger);

    [LoggerMessage(EventId = 19, Level = LogLevel.Information, Message = "Started {mapType} reconciliation")]
    internal static partial void ReconciliationStarted(this ILogger logger, MapType mapType);

    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "Finished {mapType} reconciliation")]
    internal static partial void ReconciliationFinished(this ILogger logger, MapType mapType);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Folder {id} not found in the staging database")]
    internal static partial void ReconciliationFolderNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 22, Level = LogLevel.Warning, Message = "File {id} not found in the staging database")]
    internal static partial void ReconciliationFileNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 23, Level = LogLevel.Warning, Message = "Additional folder {id} in the staging database")]
    internal static partial void ReconciliationFolderAdditional(this ILogger logger, string id);

    [LoggerMessage(EventId = 24, Level = LogLevel.Warning, Message = "Additional file {id} in the staging database")]
    internal static partial void ReconciliationFileAdditional(this ILogger logger, string id);

    [LoggerMessage(EventId = 25, Level = LogLevel.Warning, Message = "Identified discrepancy on {id} (fields: {diffs})")]
    internal static partial void ReconciliationDiff(this ILogger logger, string id, IEnumerable<ReconciliationFieldName> diffs);

    [LoggerMessage(EventId = 26, Level = LogLevel.Warning, Message = "Reconciliation summary: {additionalFilesCount} additional files, {additionalFolderCount} additional folders, {missingFilesCount} missing files, {missingFolderCount} missing folders, {diffCount} items with different values")]
    internal static partial void ReconciliationTotalDiff(this ILogger logger,
        int additionalFilesCount, int additionalFolderCount, int missingFilesCount,
        int missingFolderCount, int diffCount);

    [LoggerMessage(EventId = 27, Level = LogLevel.Information, Message = "Fetching records for reconciliation at {offset}")]
    internal static partial void GetReconciliationRecords(this ILogger logger, int offset);

    [LoggerMessage(EventId = 28, Level = LogLevel.Information, Message = "Fetching data from source reconciliation file {file}")]
    internal static partial void GetReconciliationFile(this ILogger logger, string file);

    [LoggerMessage(EventId = 29, Level = LogLevel.Error, Message = "Unrecognized map type")]
    internal static partial void InvalidMapType(this ILogger logger);
}
