using Microsoft.Extensions.Logging;
using System;

namespace Exporter;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Export {code} started")]
    internal static partial void ExportStarted(this ILogger logger, string code);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Export finished")]
    internal static partial void ExportFinished(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Serializing record")]
    internal static partial void SerializingRecord(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Unable to serialize {reference} record")]
    internal static partial void UnableSerialize(this ILogger logger, string reference);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Serialization problem")]
    internal static partial void SerializationProblem(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Exporting {count} records")]
    internal static partial void ExportingRecords(this ILogger logger, int count);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Records exported")]
    internal static partial void RecordsExported(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Fetching records at {offset}")]
    internal static partial void GetRecords(this ILogger logger, int offset);

    [LoggerMessage(EventId = 9, Level = LogLevel.Trace, Message = "Mapping {record} record")]
    internal static partial void MappingRecord(this ILogger logger, Uri record);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Export path {path}")]
    internal static partial void ExportPath(this ILogger logger, string path);
}
