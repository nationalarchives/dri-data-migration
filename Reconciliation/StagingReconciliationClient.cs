using Api;
using VDS.RDF;
using VDS.RDF.Dynamic;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Reconciliation;

public class StagingReconciliationClient(IReconciliationSparqlClient sparqlClient) : IStagingReconciliationClient
{
    public async Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> FetchAsync(
        ReconciliationMapType mapType, string code, int pageSize, int offset, CancellationToken cancellationToken)
    {
        var sparqlFileName = mapType switch
        {
            ReconciliationMapType.Closure => "ReconciliationPreservicaClosure",
            ReconciliationMapType.Discovery => "ReconciliationDiscovery",
            ReconciliationMapType.Metadata => "ReconciliationPreservicaMetadata",
        };
        var currentAssembly = typeof(StagingReconciliationClient).Assembly;
        var baseName = $"{typeof(StagingReconciliationClient).Namespace}.Sparql";
        var recordsByCodeSparql = new EmbeddedResource(currentAssembly, baseName).GetSparql(sparqlFileName);

        var sparql = new SparqlParameterizedString(recordsByCodeSparql);
        sparql.SetParameter("id", new LiteralNode(code));
        sparql.SetParameter("limit", new DecimalNode(pageSize));
        sparql.SetParameter("offset", new DecimalNode(offset));
        var results = await sparqlClient.GetResultSetAsync(sparql.ToString(), cancellationToken);
        var dynamicResults = new DynamicSparqlResultSet(results);

        return dynamicResults.Select(row =>
            Map.Where(kv => row.ContainsKey(kv.Key))
            .Select(kv => new KeyValuePair<ReconciliationFieldName, object?>(kv.Value.Field, Map[kv.Key].Conversion(row[kv.Key])))
            .Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value!));
    }

    private record RowInfo(ReconciliationFieldName Field, Func<object?, object?> Conversion);

    private static Dictionary<string, RowInfo> Map => new()
    {
        { "t", new(ReconciliationFieldName.FileFolder, ToUri) },
        { Vocabulary.ImportLocation.Uri.Segments.Last(), new(ReconciliationFieldName.ImportLocation, ToText) },
        { "reference", new(ReconciliationFieldName.Reference, ToText) },
        { Vocabulary.RedactedVariationSequence.Uri.Segments.Last(), new(ReconciliationFieldName.RedactedVariationSequence, ToInt) },
        { Vocabulary.VariationName.Uri.Segments.Last(), new(ReconciliationFieldName.VariationName, ToText) },
        { Vocabulary.AssetDriId.Uri.Segments.Last(), new(ReconciliationFieldName.Id, ToText) },
        { "startOriginDate", new(ReconciliationFieldName.OriginStartDate, ToText) },
        { "endOriginDate", new(ReconciliationFieldName.OriginEndDate, ToText) },
        { Vocabulary.AccessConditionCode.Uri.Segments.Last(), new(ReconciliationFieldName.AccessConditionCode, ToText) },
        { "closureStatus", new(ReconciliationFieldName.ClosureStatus, ToText) },
        { Vocabulary.AccessConditionName.Uri.Segments.Last(), new(ReconciliationFieldName.AccessConditionName, ToText) },
        { "retentionType", new(ReconciliationFieldName.RetentionType, ToText) },
        { Vocabulary.SensitivityReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewDate, ToDateTime) },
        { Vocabulary.SensitivityReviewSensitiveName.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewSensitiveName, ToText) },
        { Vocabulary.SensitivityReviewSensitiveDescription.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewSensitiveDescription, ToText) },
        { "isPublicName", new(ReconciliationFieldName.IsPublicName, ToRequiredBool) },
        { "isPublicDescription", new(ReconciliationFieldName.IsPublicDescription, ToRequiredBool) },
        { Vocabulary.SensitivityReviewRestrictionReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, ToDateTime) },
        { Vocabulary.SensitivityReviewRestrictionCalculationStartDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, ToDateTime) },
        { Vocabulary.SensitivityReviewRestrictionDuration.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewDuration, ToTimeSpan) },
        { Vocabulary.SensitivityReviewRestrictionEndYear.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewEndYear, ToInt) },
        { Vocabulary.LegislationSectionReference.Uri.Segments.Last(), new(ReconciliationFieldName.LegislationSectionReference, ToText) },
        { "retentionBodyName", new(ReconciliationFieldName.RetentionBody, ToText) },
        { Vocabulary.RetentionRestrictionReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionReviewDate, ToDateTime) },
        { Vocabulary.RetentionInstrumentNumber.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionInstrumentNumber, ToInt) },
        { Vocabulary.RetentionInstrumentSignatureDate.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionInstrumentSignatureDate, ToDateTime) },
        { Vocabulary.GroundForRetentionCode.Uri.Segments.Last(), new(ReconciliationFieldName.GroundForRetentionCode, ToText) }
    };

    private static readonly Func<object?, object?> ToUri = result => result is Uri uri ? uri : null;
    private static readonly Func<object?, object?> ToText = result => result is string txt && !string.IsNullOrWhiteSpace(txt) ? txt : null;
    private static readonly Func<object?, object?> ToDateTime = result => result is DateTimeOffset dt ? dt : null;
    private static readonly Func<object?, object?> ToTimeSpan = result => result is TimeSpan ts ? ts : null;
    private static readonly Func<object?, object?> ToInt = result => result is long l ? (int)l : null;
    private static readonly Func<object?, object?> ToRequiredBool = result => result is null;
}
