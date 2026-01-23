using Microsoft.Extensions.Logging;
using VDS.RDF.Parsing;

namespace Staging;

internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Building record")]
    internal static partial void BuildingRecord(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Record built")]
    internal static partial void RecordBuilt(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Trace, Message = "Record updated")]
    internal static partial void RecordUpdated(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Record not ingested because graph couldn't be constructed")]
    internal static partial void RecordNotIngestedNoGraph(this ILogger logger);

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

    [LoggerMessage(EventId = 12, Level = LogLevel.Warning, Message = "Variation {variation} not found")]
    internal static partial void VariationNotFound(this ILogger logger, string variation);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "Unable to parse ground for retention code from {groundForRetention}")]
    internal static partial void UnableParseGroundForRetentionUri(this ILogger logger, Uri groundForRetention);

    [LoggerMessage(EventId = 14, Level = LogLevel.Warning, Message = "Ground for retention {code} not found")]
    internal static partial void GroundForRetentionNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 15, Level = LogLevel.Warning, Message = "Retention for {code} not found")]
    internal static partial void RetentionNotFound(this ILogger logger, Uri code);

    [LoggerMessage(EventId = 16, Level = LogLevel.Warning, Message = "Unrecognized cache entity type")]
    internal static partial void InvalidCacheEntityKind(this ILogger logger);

    [LoggerMessage(EventId = 17, Level = LogLevel.Debug, Message = "Asset {asset} missing RDF in the XML")]
    internal static partial void AssetXmlMissingRdf(this ILogger logger, Uri asset);

    [LoggerMessage(EventId = 18, Level = LogLevel.Debug, Message = "Variation {variation} missing RDF in the XML")]
    internal static partial void VariationXmlMissingRdf(this ILogger logger, string variation);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "Legislation {code} not found")]
    internal static partial void LegislationNotFound(this ILogger logger, string code);

    [LoggerMessage(EventId = 20, Level = LogLevel.Warning, Message = "Related variation {variation} not found")]
    internal static partial void RelatedVariationMissing(this ILogger logger, string variation);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "Alternative variation {variation} on {path} not found")]
    internal static partial void AlternativeVariationMissing(this ILogger logger, string variation, string path);

    [LoggerMessage(EventId = 22, Level = LogLevel.Warning, Message = "Associated variation {variation} not found")]
    internal static partial void AssociatedVariationNotFound(this ILogger logger, string variation);

    [LoggerMessage(EventId = 23, Level = LogLevel.Warning, Message = "Unrecognized {status} legal status")]
    internal static partial void UnrecognizedLegalStatus(this ILogger logger, string status);

    [LoggerMessage(EventId = 24, Level = LogLevel.Warning, Message = "Unrecognized {duration} film duration format")]
    internal static partial void UnrecognizedFilmDurationFormat(this ILogger logger, string duration);

    [LoggerMessage(EventId = 25, Level = LogLevel.Warning, Message = "Unrecognized {date} date format")]
    internal static partial void UnrecognizedDateFormat(this ILogger logger, string date);

    [LoggerMessage(EventId = 26, Level = LogLevel.Warning, Message = "Unrecognized {date} year, month, day format")]
    internal static partial void UnrecognizedYearMonthDayFormat(this ILogger logger, string date);

    [LoggerMessage(EventId = 27, Level = LogLevel.Warning, Message = "Unrecognized {split} image split value")]
    internal static partial void UnrecognizedImageSplitValue(this ILogger logger, string split);

    [LoggerMessage(EventId = 28, Level = LogLevel.Warning, Message = "Unrecognized {crop} image crop value")]
    internal static partial void UnrecognizedImageCropValue(this ILogger logger, string crop);

    [LoggerMessage(EventId = 29, Level = LogLevel.Warning, Message = "Unrecognized {deskew} image deskew value")]
    internal static partial void UnrecognizedImageDeskewValue(this ILogger logger, string deskew);

    [LoggerMessage(EventId = 30, Level = LogLevel.Warning, Message = "Invalid {integer} integer value")]
    internal static partial void InvalidIntegerValue(this ILogger logger, string integer);

    [LoggerMessage(EventId = 31, Level = LogLevel.Debug, Message = "Malformed RDF")]
    internal static partial void MalformedRdf(this ILogger logger, RdfParseException e);

    [LoggerMessage(EventId = 32, Level = LogLevel.Warning, Message = "Unable to load RDF: {message}")]
    internal static partial void UnableLoadRdf(this ILogger logger, string message);

    [LoggerMessage(EventId = 33, Level = LogLevel.Warning, Message = "Unrecognized {face} face format")]
    internal static partial void UnrecognizedFaceFormat(this ILogger logger, string face);

    [LoggerMessage(EventId = 34, Level = LogLevel.Warning, Message = "Unable to parse {dimension} dimension")]
    internal static partial void UnableParseDimension(this ILogger logger, string dimension);

    [LoggerMessage(EventId = 35, Level = LogLevel.Warning, Message = "Unable to establish sequence of related variation {variationPartialName}")]
    internal static partial void UnableEstablishRelatedVariationSequence(this ILogger logger, string variationPartialName);

    [LoggerMessage(EventId = 36, Level = LogLevel.Debug, Message = "Unable to reformat XML")]
    internal static partial void UnableAddRdfToXml(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 37, Level = LogLevel.Debug, Message = "Unable to add missing type to malformed RDF XML")]
    internal static partial void UnableAddMissingTypeToXmlRdf(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 38, Level = LogLevel.Warning, Message = "Unrecognized {kinship} kinship")]
    internal static partial void UnrecognizedKinship(this ILogger logger, Uri kinship);

    [LoggerMessage(EventId = 39, Level = LogLevel.Warning, Message = "Unable to parse RDF")]
    internal static partial void UnableParseRdf(this ILogger logger);

    [LoggerMessage(EventId = 40, Level = LogLevel.Warning, Message = "Unable to find person {person}")]
    internal static partial void PersonNotFound(this ILogger logger, string person);

    [LoggerMessage(EventId = 41, Level = LogLevel.Warning, Message = "Unable to find operator {operatorId}")]
    internal static partial void OperatorNotFound(this ILogger logger, string operatorId);

    [LoggerMessage(EventId = 42, Level = LogLevel.Warning, Message = "Unable to resolve related Id {relatedId}")]
    internal static partial void RelatedIdNotResolved(this ILogger logger, string relatedId);

    [LoggerMessage(EventId = 43, Level = LogLevel.Debug, Message = "Related Id resolution failed")]
    internal static partial void RelatedIdResolutionFailed(this ILogger logger, Exception e);

    [LoggerMessage(EventId = 44, Level = LogLevel.Warning, Message = "Unable to find related Id {relatedId}")]
    internal static partial void RelatedIdNotFound(this ILogger logger, string relatedId);

    [LoggerMessage(EventId = 45, Level = LogLevel.Warning, Message = "Asset tag {assetTag} not matched")]
    internal static partial void AssetTagTypeNotResolved(this ILogger logger, string assetTag);

    [LoggerMessage(EventId = 46, Level = LogLevel.Warning, Message = "Hash function {hash} not matched")]
    internal static partial void HashFunctionNotResolved(this ILogger logger, string hash);
}
