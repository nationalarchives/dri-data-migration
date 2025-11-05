using Api;
using Microsoft.Extensions.Logging;

namespace Reconciliation;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started {mapType} reconciliation on {code}")]
    internal static partial void ReconciliationStarted(this ILogger logger, ReconciliationMapType mapType, string code);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Finished reconciliation")]
    internal static partial void ReconciliationFinished(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Missing folder {id")]
    internal static partial void ReconciliationFolderNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Missing file {id}")]
    internal static partial void ReconciliationFileNotFound(this ILogger logger, string id);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Additional folder {id}")]
    internal static partial void ReconciliationFolderAdditional(this ILogger logger, string id);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "Additional file {id}")]
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

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Comparing {count} records")]
    internal static partial void ComparingRecords(this ILogger logger, int count);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Finding missing records")]
    internal static partial void FindingMissingRecords(this ILogger logger);

    [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "Unable to find matching source data retrieval")]
    internal static partial void UnableFindSource(this ILogger logger);

    //TODO: Verbose???
    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Expected value of {field} is {expected} but found {actual}")]
    internal static partial void ReconciliationDiffDetails(this ILogger logger, ReconciliationFieldName field, object expected, object actual);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "Retrieved {count} records")]
    internal static partial void ReconciliationRecordCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "No difference found")]
    internal static partial void ReconciliationNoDiff(this ILogger logger);
}
