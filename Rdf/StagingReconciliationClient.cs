using Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Dynamic;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Rdf;

public class StagingReconciliationClient(ISparqlClient sparqlClient) : IStagingReconciliationClient
{
    public async Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> FetchAsync(string code, int pageSize, int offset)
    {
        var currentAssembly = typeof(DriExport).Assembly;
        var baseName = $"{typeof(DriExport).Namespace}.Sparql.Staging";
        var recordsByCodeSparql = new EmbeddedSparqlResource(currentAssembly, baseName).GetSparql("ReconciliationResultSet");

        var sparql = new SparqlParameterizedString(recordsByCodeSparql);
        sparql.SetParameter("id", new LiteralNode(code));
        sparql.SetParameter("limit", new DecimalNode(pageSize));
        sparql.SetParameter("offset", new DecimalNode(offset));
        var results = await sparqlClient.GetResultSetAsync(sparql.ToString());
        var dynamicResults = new DynamicSparqlResultSet(results);

        return dynamicResults.Select(row =>
            Map.Select(kv => new KeyValuePair<ReconciliationFieldName, object?>(kv.Value.Field, Map[kv.Key].Conversion(row[kv.Key])))
            .Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value!));
    }

    private record RowInfo(ReconciliationFieldName Field, Func<object?, object?> Conversion);

    private static Dictionary<string, RowInfo> Map => new()
    {
        { "s", new(ReconciliationFieldName.Id, ToUri) },
        { "t", new(ReconciliationFieldName.FileFolder, ToUri) },
        { Vocabulary.ImportLocation.Uri.Segments.Last(), new(ReconciliationFieldName.ImportLocation, ToText) },
        { Vocabulary.VariationName.Uri.Segments.Last(), new(ReconciliationFieldName.VariationName, ToText) },
        { Vocabulary.AccessConditionName.Uri.Segments.Last(), new(ReconciliationFieldName.AccessConditionName, ToText) },
        { "retentionType", new(ReconciliationFieldName.RetentionType, ToText) },
        { Vocabulary.SensitivityReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewDate, ToDateTime) },
        { Vocabulary.SensitivityReviewSensitiveName.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewSensitiveName, ToText) },
        { "isPublicName", new(ReconciliationFieldName.IsPublicName, ToBool) },
        { "isPublicDescription", new(ReconciliationFieldName.IsPublicDescription, ToBool) },
        { Vocabulary.SensitivityReviewSensitiveDescription.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewSensitiveDescription, ToText) },
        { Vocabulary.SensitivityReviewRestrictionReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewRestrictionReviewDate, ToDateTime) },
        { Vocabulary.SensitivityReviewRestrictionCalculationStartDate.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewRestrictionCalculationStartDate, ToDateTime) },
        { Vocabulary.SensitivityReviewRestrictionDuration.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewDuration, ToText) },
        { Vocabulary.SensitivityReviewRestrictionEndYear.Uri.Segments.Last(), new(ReconciliationFieldName.SensitivityReviewEndYear, ToInt) },
        { Vocabulary.LegislationSectionReference.Uri.Segments.Last(), new(ReconciliationFieldName.LegislationSectionReference, ToText) },
        { Vocabulary.RetentionRestrictionReviewDate.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionReviewDate, ToDateTime) },
        { Vocabulary.RetentionInstrumentNumber.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionInstrumentNumber, ToInt) },
        { Vocabulary.RetentionInstrumentSignatureDate.Uri.Segments.Last(), new(ReconciliationFieldName.RetentionInstrumentSignatureDate, ToDateTime) },
        { Vocabulary.GroundForRetentionCode.Uri.Segments.Last(), new(ReconciliationFieldName.GroundForRetentionCode, ToText) }
    };

    private static readonly Func<object?, object?> ToUri = result => result is Uri ? result as Uri : null;
    private static readonly Func<object?, object?> ToText = result => result is string ? result as string : null;
    private static readonly Func<object?, object?> ToDateTime = result => result is DateTimeOffset ? result as DateTimeOffset? : null;
    private static readonly Func<object?, object?> ToInt = result => result is long ? (int?)(result as long?) : null;
    private static readonly Func<object?, object?> ToBool = result => result is bool ? result as bool? : null;
}
