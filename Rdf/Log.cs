using Microsoft.Extensions.Logging;
using System;

namespace Rdf;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Fetching top subsets")]
    internal static partial void GetBroadestSubsets(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Fetching access conditions")]
    internal static partial void GetAccessConditions(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Fetching legislations")]
    internal static partial void GetLegislations(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Fetching grounds for retention")]
    internal static partial void GetGroundsForRetention(this ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Fetching subsets starting at {offset}")]
    internal static partial void GetSubsetsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Fetching assets starting at {offset}")]
    internal static partial void GetAssetsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Fetching variations starting at {offset}")]
    internal static partial void GetVariationsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Fetching sensitivity reviews at {offset}")]
    internal static partial void GetSensitivityReviewsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 9, Level = LogLevel.Trace, Message = "Building record {id}")]
    internal static partial void BuildingRecord(this ILogger logger, string id);

    [LoggerMessage(EventId = 10, Level = LogLevel.Trace, Message = "Record {id} built")]
    internal static partial void RecordBuilt(this ILogger logger, string id);

    [LoggerMessage(EventId = 11, Level = LogLevel.Trace, Message = "Record {id} updated")]
    internal static partial void RecordUpdated(this ILogger logger, string id);

    [LoggerMessage(EventId = 12, Level = LogLevel.Warning, Message = "Record {id} not ingested because graph couldn't be counstructed")]
    internal static partial void RecordNotIngestedNoGraph(this ILogger logger, string id);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "Subset {subset} not found")]
    internal static partial void SubsetNotFound(this ILogger logger, string subset);

    [LoggerMessage(EventId = 14, Level = LogLevel.Warning, Message = "Asset {asset} not found")]
    internal static partial void AssetNotFound(this ILogger logger, string asset);

    [LoggerMessage(EventId = 15, Level = LogLevel.Error, Message = "Access conditions not found")]
    internal static partial void MissingAccessConditions(this ILogger logger);

    [LoggerMessage(EventId = 16, Level = LogLevel.Error, Message = "Legislations not found")]
    internal static partial void MissingLegislations(this ILogger logger);

    [LoggerMessage(EventId = 17, Level = LogLevel.Error, Message = "Grounds for rejection not found")]
    internal static partial void MissingGroundsForRejection(this ILogger logger);

    [LoggerMessage(EventId = 18, Level = LogLevel.Warning, Message = "Unable to parse access condition code from {accessCondition}")]
    internal static partial void UnableParseAccessConditionUri(this ILogger logger, Uri accessCondition);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "Access condition {code} not found")]
    internal static partial void AccessConditionNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 20, Level = LogLevel.Warning, Message = "Asset {asset} not found")]
    internal static partial void VariationNotFound(this ILogger logger, string asset);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Unable to parse ground for retention code from {groundForRetention}")]
    internal static partial void UnableParseGroundForRetentionUri(this ILogger logger, Uri groundForRetention);

    [LoggerMessage(EventId = 22, Level = LogLevel.Warning, Message = "Ground for retention {code} not found")]
    internal static partial void GroundForRetentionNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 23, Level = LogLevel.Warning, Message = "Retention for {code} not found")]
    internal static partial void RetentionNotFound(this ILogger logger, Uri code);

    [LoggerMessage(EventId = 24, Level = LogLevel.Warning, Message = "Unrecognized cache entity type")]
    internal static partial void InvalidCacheEntityKind(this ILogger logger);
}
