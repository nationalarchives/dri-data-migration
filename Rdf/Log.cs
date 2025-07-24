using Microsoft.Extensions.Logging;

namespace Rdf;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Fetching top subsets")]
    internal static partial void GetBroadestSubsets(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Fetching access conditions")]
    internal static partial void GetAccessConditions(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Fetching legislations")]
    internal static partial void GetLegislations(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Trace, Message = "Fetching grounds for retention")]
    internal static partial void GetGroundsForRetention(this ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Trace, Message = "Fetching subsets starting at {offset}")]
    internal static partial void GetSubsetsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 6, Level = LogLevel.Trace, Message = "Fetching assets starting at {offset}")]
    internal static partial void GetAssetsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 7, Level = LogLevel.Trace, Message = "Fetching variations starting at {offset}")]
    internal static partial void GetVariationsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 8, Level = LogLevel.Trace, Message = "Fetching sensitivity reviews at {offset}")]
    internal static partial void GetSensitivityReviewsByCode(this ILogger logger, int offset);

    [LoggerMessage(EventId = 9, Level = LogLevel.Trace, Message = "Building record {id}")]
    internal static partial void BuildingRecord(this ILogger logger, string id);

    [LoggerMessage(EventId = 9, Level = LogLevel.Trace, Message = "Record {id} built")]
    internal static partial void RecordBuilt(this ILogger logger, string id);

    [LoggerMessage(EventId = 10, Level = LogLevel.Trace, Message = "Record {id} updated")]
    internal static partial void RecordUpdated(this ILogger logger, string id);
}
