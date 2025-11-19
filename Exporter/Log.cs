using Api;
using Microsoft.Extensions.Logging;

namespace Exporter;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started {code} export as {scopeType}")]
    internal static partial void ExportStarted(this ILogger logger, ExportScopeType scopeType, string code);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Export finished")]
    internal static partial void ExportFinished(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Serializing record")]
    internal static partial void SerializingRecord(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Unable to serialize {reference}")]
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

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Exporting {count} XMLs")]
    internal static partial void ExportingXmls(this ILogger logger, int count);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "XMLs exported")]
    internal static partial void XmlsExported(this ILogger logger);

    [LoggerMessage(EventId = 13, Level = LogLevel.Trace, Message = "Serializing XML")]
    internal static partial void SerializingXml(this ILogger logger);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Fetching XMLs at {offset}")]
    internal static partial void GetXmls(this ILogger logger, int offset);

    [LoggerMessage(EventId = 15, Level = LogLevel.Warning, Message = "Unable to map record {asset}")]
    internal static partial void UnableRecordMap(this ILogger logger, Uri asset);

    [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "Record mapping problem")]
    internal static partial void RecordMappingProblem(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 17, Level = LogLevel.Warning, Message = "Unable to map XML {variation}")]
    internal static partial void UnableXmlMap(this ILogger logger, Uri variation);

    [LoggerMessage(EventId = 18, Level = LogLevel.Debug, Message = "XML mapping problem")]
    internal static partial void XmlMappingProblem(this ILogger logger, Exception e);
}
