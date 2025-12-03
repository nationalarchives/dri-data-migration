using Api;
using Microsoft.Extensions.Logging;

namespace Exporter;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Started {code} export as {scopeType}")]
    internal static partial void ExportStarted(this ILogger logger, ExportScopeType scopeType, string code);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Export finished")]
    internal static partial void ExportFinished(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Exported {count} records")]
    internal static partial void ExportRecordCount(this ILogger logger, int count);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Unable to serialize {reference}")]
    internal static partial void UnableSerialize(this ILogger logger, string reference);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Serialization problem")]
    internal static partial void SerializationProblem(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Found {count} records")]
    internal static partial void RecordListFound(this ILogger logger, int count);

    [LoggerMessage(EventId = 7, Level = LogLevel.Trace, Message = "Generating record")]
    internal static partial void GeneratingRecord(this ILogger logger);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "Export path {path}")]
    internal static partial void ExportPath(this ILogger logger, string path);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "Unable to map record {asset}")]
    internal static partial void UnableRecordMap(this ILogger logger, Uri asset);

    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Record mapping problem")]
    internal static partial void RecordMappingProblem(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "Unable to map XML {variation}")]
    internal static partial void UnableXmlMap(this ILogger logger, Uri variation);

    [LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "XML mapping problem")]
    internal static partial void XmlMappingProblem(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "Fetching list of records")]
    internal static partial void GetRecordList(this ILogger logger);

    [LoggerMessage(EventId = 14, Level = LogLevel.Warning, Message = "Unable to fetch {id}")]
    internal static partial void UnableFindRecord(this ILogger logger, Uri id);

    [LoggerMessage(EventId = 15, Level = LogLevel.Warning, Message = "Content of the file {file} will be refreshed")]
    internal static partial void ExistingFileRecord(this ILogger logger, string file);

    [LoggerMessage(EventId = 16, Level = LogLevel.Warning, Message = "Unable to deserialize {file}")]
    internal static partial void UnableDeserialize(this ILogger logger, string file);
}
