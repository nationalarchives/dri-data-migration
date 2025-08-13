using Api;
using Microsoft.Extensions.Logging;

namespace Reconciliation;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started {mapType} reconciliation")]
    internal static partial void ReconciliationStarted(this ILogger logger, MapType mapType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Finished {mapType} reconciliation")]
    internal static partial void ReconciliationFinished(this ILogger logger, MapType mapType);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Folder {id} not found in the staging database")]
    internal static partial void ReconciliationFolderNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "File {id} not found in the staging database")]
    internal static partial void ReconciliationFileNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Additional folder {id} in the staging database")]
    internal static partial void ReconciliationFolderAdditional(this ILogger logger, string id);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "Additional file {id} in the staging database")]
    internal static partial void ReconciliationFileAdditional(this ILogger logger, string id);

    [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "Identified discrepancy on {id} (fields: {diffs})")]
    internal static partial void ReconciliationDiff(this ILogger logger, string id, IEnumerable<ReconciliationFieldName> diffs);

    [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "Reconciliation summary: {additionalFilesCount} additional files, {additionalFolderCount} additional folders, {missingFilesCount} missing files, {missingFolderCount} missing folders, {diffCount} items with different values")]
    internal static partial void ReconciliationTotalDiff(this ILogger logger,
        int additionalFilesCount, int additionalFolderCount, int missingFilesCount,
        int missingFolderCount, int diffCount);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Fetching records for reconciliation at {offset}")]
    internal static partial void GetReconciliationRecords(this ILogger logger, int offset);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Fetching data from source reconciliation file {file}")]
    internal static partial void GetReconciliationFile(this ILogger logger, string file);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Fetching data from Discovery API {uri}")]
    internal static partial void GetDiscoveryRecords(this ILogger logger, Uri uri);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Fetching records from mark {mark}")]
    internal static partial void GetDiscoveryRecordsPage(this ILogger logger, string mark);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Comparing records")]
    internal static partial void ComparingRecords(this ILogger logger);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Finding missing records")]
    internal static partial void FindingMissingRecords(this ILogger logger);

    [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "Unable to find matching source data retrieval")]
    internal static partial void UnableFindSource(this ILogger logger);
}
