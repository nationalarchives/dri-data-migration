using Microsoft.Extensions.Logging;

namespace Dri;

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
}
