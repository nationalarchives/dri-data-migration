using Api;
using System.Globalization;
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
            _ => throw new MigrationException($"Unrecognized reconciliation map type {mapType}")
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
        { Vocabulary.ImportLocation.Uri.LastSegment(), new(ReconciliationFieldName.Location, ToText) },
        { "reference", new(ReconciliationFieldName.Reference, ToText) },
        { Vocabulary.RedactedVariationSequence.Uri.LastSegment(), new(ReconciliationFieldName.RedactedVariationSequence, ToInt) },
        { Vocabulary.VariationName.Uri.LastSegment(), new(ReconciliationFieldName.Name, ToText) },
        { Vocabulary.AssetDriId.Uri.LastSegment(), new(ReconciliationFieldName.Id, ToText) },
        { "startCoveringDate", new(ReconciliationFieldName.CoveringDateStart, ToText) },
        { "endCoveringDate", new(ReconciliationFieldName.CoveringDateEnd, ToEnd) },
        { Vocabulary.AssetModifiedAt.Uri.LastSegment(), new(ReconciliationFieldName.ModifiedAt, ToDateTime) },
        { Vocabulary.AccessConditionCode.Uri.LastSegment(), new(ReconciliationFieldName.AccessConditionCode, ToText) },
        { "closureStatus", new(ReconciliationFieldName.ClosureStatus, ToText) },
        { Vocabulary.AccessConditionName.Uri.LastSegment(), new(ReconciliationFieldName.AccessConditionName, ToText) },
        { "retentionType", new(ReconciliationFieldName.RetentionType, ToText) },
        { Vocabulary.SensitivityReviewDate.Uri.LastSegment(), new(ReconciliationFieldName.FoiAssertedDate, ToDateTime) },
        { Vocabulary.SensitivityReviewSensitiveName.Uri.LastSegment(), new(ReconciliationFieldName.SensitiveName, ToText) },
        { Vocabulary.SensitivityReviewSensitiveDescription.Uri.LastSegment(), new(ReconciliationFieldName.SensitiveDescription, ToText) },
        { "isPublicName", new(ReconciliationFieldName.IsPublicName, ToRequiredBool) },
        { "isPublicDescription", new(ReconciliationFieldName.IsPublicDescription, ToRequiredBool) },
        { "missingSensitiveName", new(ReconciliationFieldName.MissingSensitiveNameInformation, ToBool) },
        { "missingSensitiveDescription", new(ReconciliationFieldName.MissingSensitiveDescriptionInformation, ToBool) },
        { Vocabulary.SensitivityReviewRestrictionCalculationStartDate.Uri.LastSegment(), new(ReconciliationFieldName.ClosureStartDate, ToDateTime) },
        { Vocabulary.SensitivityReviewRestrictionDuration.Uri.LastSegment(), new(ReconciliationFieldName.ClosurePeriod, ToTimeSpan) },
        { Vocabulary.SensitivityReviewRestrictionEndYear.Uri.LastSegment(), new(ReconciliationFieldName.ClosureEndYear, ToYear) },
        { Vocabulary.LegislationSectionReference.Uri.LastSegment(), new(ReconciliationFieldName.FoiExemptionReference, ToText) },
        { "retentionBodyName", new(ReconciliationFieldName.HeldBy, ToText) },
        { Vocabulary.RetentionInstrumentNumber.Uri.LastSegment(), new(ReconciliationFieldName.InstrumentNumber, ToInt) },
        { Vocabulary.RetentionInstrumentSignatureDate.Uri.LastSegment(), new(ReconciliationFieldName.InstrumentSignedDate, ToDateTime) },
        { Vocabulary.GroundForRetentionCode.Uri.LastSegment(), new(ReconciliationFieldName.GroundForRetentionCode, ToText) }
    };

    private static readonly Func<object?, object?> ToUri = result => result is Uri uri ? uri : null;
    private static readonly Func<object?, object?> ToEnd = result => result is string txt ? AdjustEndDate(txt) : null;
    private static readonly Func<object?, object?> ToText = result => result is string txt && !string.IsNullOrWhiteSpace(txt) ? txt : null;
    private static readonly Func<object?, object?> ToDateTime = result => result is DateTimeOffset dt ? dt : null;
    private static readonly Func<object?, object?> ToTimeSpan = result => result is TimeSpan ts ? ts : null;
    private static readonly Func<object?, object?> ToInt = result => result is long l ? (int)l : null;
    private static readonly Func<object?, object?> ToYear = result => result is ILiteralNode l && int.TryParse(l.Value, out var endYear) ? endYear : null;
    private static readonly Func<object?, object?> ToBool = result => result is bool b ? b : null;
    private static readonly Func<object?, object?> ToRequiredBool = result => result is null;

    private static string? AdjustEndDate(string? date)
    {
        if (date is null)
        {
            return null;
        }
        if (date.Length < 9)
        {
            var lastDay = DateTime.ParseExact($"{date}---01", "yyyy--MM---dd", CultureInfo.InvariantCulture)
                .AddMonths(1).AddDays(-1).Day;
            return $"{date}---{lastDay}";
        }
        return date;
    }
}
