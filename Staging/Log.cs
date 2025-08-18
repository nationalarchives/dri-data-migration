using Microsoft.Extensions.Logging;
using System;

namespace Staging;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Building record {id}")]
    internal static partial void BuildingRecord(this ILogger logger, string id);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Record {id} built")]
    internal static partial void RecordBuilt(this ILogger logger, string id);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Record {id} updated")]
    internal static partial void RecordUpdated(this ILogger logger, string id);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Record {id} not ingested because graph couldn't be counstructed")]
    internal static partial void RecordNotIngestedNoGraph(this ILogger logger, string id);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Subset {subset} not found")]
    internal static partial void SubsetNotFound(this ILogger logger, string subset);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "Asset {asset} not found")]
    internal static partial void AssetNotFound(this ILogger logger, string asset);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Access conditions not found")]
    internal static partial void MissingAccessConditions(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Legislations not found")]
    internal static partial void MissingLegislations(this ILogger logger);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error, Message = "Grounds for rejection not found")]
    internal static partial void MissingGroundsForRejection(this ILogger logger);

    [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "Unable to parse access condition code from {accessCondition}")]
    internal static partial void UnableParseAccessConditionUri(this ILogger logger, Uri accessCondition);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "Access condition {code} not found")]
    internal static partial void AccessConditionNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 12, Level = LogLevel.Warning, Message = "Asset {asset} not found")]
    internal static partial void VariationNotFound(this ILogger logger, string asset);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "Unable to parse ground for retention code from {groundForRetention}")]
    internal static partial void UnableParseGroundForRetentionUri(this ILogger logger, Uri groundForRetention);

    [LoggerMessage(EventId = 14, Level = LogLevel.Warning, Message = "Ground for retention {code} not found")]
    internal static partial void GroundForRetentionNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 15, Level = LogLevel.Warning, Message = "Retention for {code} not found")]
    internal static partial void RetentionNotFound(this ILogger logger, Uri code);

    [LoggerMessage(EventId = 16, Level = LogLevel.Warning, Message = "Unrecognized cache entity type")]
    internal static partial void InvalidCacheEntityKind(this ILogger logger);
}
