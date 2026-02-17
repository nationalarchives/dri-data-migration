using Api;
using Microsoft.Extensions.Logging;

namespace Reconciliation;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started {mapType} reconciliation on {code}")]
    internal static partial void ReconciliationStarted(this ILogger logger, ReconciliationMapType mapType, string code);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Finished reconciliation")]
    internal static partial void ReconciliationFinished(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Missing folder")]
    internal static partial void ReconciliationFolderNotFound(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Missing file")]
    internal static partial void ReconciliationFileNotFound(this ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Additional folder")]
    internal static partial void ReconciliationFolderAdditional(this ILogger logger);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Additional file")]
    internal static partial void ReconciliationFileAdditional(this ILogger logger);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Expected value of {field} is {expected} but actual is {actual}")]
    internal static partial void ReconciliationDiffDetails(this ILogger logger, ReconciliationFieldName field, object expected, object actual);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Reconciliation summary: {additionalFilesCount} additional files, {additionalFolderCount} additional folders, {missingFilesCount} missing files, {missingFolderCount} missing folders, {diffCount} items with different values")]
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

    [LoggerMessage(EventId = 16, Level = LogLevel.Information, Message = "Retrieved {count} records")]
    internal static partial void ReconciliationRecordCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "No difference found")]
    internal static partial void ReconciliationNoDiff(this ILogger logger);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "{count} differences (field, expected value, actual value)")]
    internal static partial void DiffCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 19, Level = LogLevel.Information, Message = "{id}")]
    internal static partial void DiffRecord(this ILogger logger, string Id);

    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "\t{field}\t{expected}\t{actual}")]
    internal static partial void DiffDetails(this ILogger logger, ReconciliationFieldName field, object expected, object actual);

    [LoggerMessage(EventId = 21, Level = LogLevel.Information, Message = "{count} missing files")]
    internal static partial void MissingFilesCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 22, Level = LogLevel.Information, Message = "{count} missing folders")]
    internal static partial void MissingFoldersCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 23, Level = LogLevel.Information, Message = "{id}")]
    internal static partial void MissingRecord(this ILogger logger, string Id);

    [LoggerMessage(EventId = 24, Level = LogLevel.Information, Message = @"
Reconciles following fields:
Location        identifier
Name            file_name
FileFolder      folder
ModifiedAt      date_last_modified
CoveringDateEnd end_date")]
    internal static partial void MetadataReconciliationInfo(this ILogger logger);

    [LoggerMessage(EventId = 25, Level = LogLevel.Information, Message = @"
Reconciles following fields:
Location                identifier
FileFolder              folder
AccessConditionName     closure_type
RetentionType           retention_type
ClosurePeriod           closure_period
ClosureStartDate        closure_start_date
FoiExemptionReference   foi_exemption_code
FoiAssertedDate         foi_exemption_asserted
InstrumentNumber        RI_number
InstrumentSignedDate    RI_signed_date
GroundForRetentionCode  retention_justification
IsPublicName            title_public
SensitiveName           title_alternate
IsPublicDescription     description_public
SensitiveDescription    description_alternate")]
    internal static partial void ClosureReconciliationInfo(this ILogger logger);

    [LoggerMessage(EventId = 26, Level = LogLevel.Information, Message = @"
Reconciles following fields:
Id                  Id
Name                Title
Reference           Reference
CoveringDateStart   NumStartDate
CoveringDateEnd     NumEndDate
ClosureStatus       ClosureStatus
AccessConditionCode ClosureType
HeldBy              HeldBy
ClosurePeriod       ClosureType & ClosureCode
ClosureEndYear      ClosureType & ClosureCode")]
    internal static partial void DiscoveryReconciliationInfo(this ILogger logger);

    [LoggerMessage(EventId = 27, Level = LogLevel.Information, Message = "{count} additional files")]
    internal static partial void AdditionalFilesCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 28, Level = LogLevel.Information, Message = "{count} additional folders")]
    internal static partial void AdditionalFoldersCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 29, Level = LogLevel.Information, Message = "{id}")]
    internal static partial void AdditionalRecord(this ILogger logger, string Id);
}
