using Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Dri;

public class RdfExporter : IDriRdfExporter
{
    private readonly DriSettings settings;
    private readonly IDriSparqlClient sparqlClient;
    private readonly EmbeddedResource embedded;
    private readonly string sparqlAccessCondition;
    private readonly string sparqlLegislation;
    private readonly string sparqlGroundForRetention;
    private readonly string sparqlSubset;
    private readonly string sparqlAsset;
    private readonly string sparqlVariation;
    private readonly string sparqlSensitivityReview;

    public RdfExporter(IOptions<DriSettings> options, IDriSparqlClient sparqlClient)
    {
        settings = options.Value;
        this.sparqlClient = sparqlClient;

        var currentAssembly = typeof(RdfExporter).Assembly;
        var baseName = $"{typeof(RdfExporter).Namespace}.Sparql";
        embedded = new(currentAssembly, baseName);
        sparqlAccessCondition = embedded.GetSparql($"Get{EtlStageType.AccessCondition}");
        sparqlLegislation = embedded.GetSparql($"Get{EtlStageType.Legislation}");
        sparqlGroundForRetention = embedded.GetSparql($"Get{EtlStageType.GroundForRetention}");
        sparqlSubset = embedded.GetSparql($"Get{EtlStageType.Subset}");
        sparqlAsset = embedded.GetSparql($"Get{EtlStageType.Asset}");
        sparqlVariation = embedded.GetSparql($"Get{EtlStageType.Variation}");
        sparqlSensitivityReview = embedded.GetSparql($"Get{EtlStageType.SensitivityReview}");
    }

    public async Task<IEnumerable<Uri>> GetListAsync(EtlStageType etlStageType, CancellationToken cancellationToken)
    {
        var sparql = embedded.GetSparql($"List{etlStageType}");
        var parameterizedString = new SparqlParameterizedString(sparql);
        parameterizedString.SetParameter("id", new LiteralNode(settings.Code));

        var result = await sparqlClient.GetResultSetAsync(parameterizedString.ToString(), cancellationToken);

        return result.Results.Select(s => (s.Value("s") as IUriNode)!.Uri);
    }

    public Task<DriAccessCondition> GetAccessConditionAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlAccessCondition, id, MapAccessCondition, cancellationToken);

    public Task<DriLegislation> GetLegislationAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlLegislation, id, MapLegislation, cancellationToken);

    public Task<DriGroundForRetention> GetGroundForRetentionAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlGroundForRetention, id, MapGroundForRetention, cancellationToken);

    public Task<DriSubset> GetSubsetAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlSubset, id, MapSubset, cancellationToken);

    public Task<DriAsset> GetAssetAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlAsset, id, MapAsset, cancellationToken);

    public Task<DriVariation> GetVariationAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlVariation, id, MapVariation, cancellationToken);

    public Task<DriSensitivityReview> GetSensitivityReviewAsync(Uri id, CancellationToken cancellationToken) =>
        GetRecordAsync(sparqlSensitivityReview, id, MapSensitivityReview, cancellationToken);

    private async Task<T> GetRecordAsync<T>(string sparql, Uri id,
        Func<SparqlResultSet, T> mapping, CancellationToken cancellationToken) where T : IDriRecord
    {
        var parameterizedString = new SparqlParameterizedString(sparql);
        parameterizedString.SetParameter("id", new UriNode(id));
        var result = await sparqlClient.GetResultSetAsync(parameterizedString.ToString(), cancellationToken);

        return mapping(result);
    }

    private async Task<T> GetRecordAsync<T>(string sparql, Uri id,
        Func<IGraph, T> mapping, CancellationToken cancellationToken)
    {
        var result = await sparqlClient.GetGraphAsync(sparql, id, cancellationToken);
        
        return mapping(result);
    }

    private static DriAccessCondition MapAccessCondition(SparqlResultSet result) =>
        result.Results.Select(s => new DriAccessCondition(
            (s.Value("s") as IUriNode)!.Uri, s.Value("label").AsValuedNode().AsString()))
        .Single();

    private static DriLegislation MapLegislation(SparqlResultSet result) =>
        result.Results.Select(s => new DriLegislation(
            (s.Value("legislation") as IUriNode)!.Uri, s.HasValue("label") ? s.Value("label")?.AsValuedNode().AsString() : null))
        .Single();

    private static DriGroundForRetention MapGroundForRetention(SparqlResultSet result) =>
        result.Results.Select(s => new DriGroundForRetention(
            s.Value("label").AsValuedNode().AsString(), s.Value("comment").AsValuedNode().AsString()))
        .Single();

    private static DriSubset MapSubset(IGraph graph)
    {
        var reference = graph.GetTriplesWithPredicate(Vocabulary.SubsetReference)
            .SingleOrDefault(t => t.Subject is IUriNode)?.Object as ILiteralNode;
        var directory = graph.GetSingleText(Vocabulary.ImportLocation);
        var broader = graph.GetSingleBlankNode(Vocabulary.SubsetHasBroaderSubset);
        string? parent = null;
        if (broader is not null)
        {
            parent = graph.GetSingleText(broader, Vocabulary.SubsetReference);
        }

        return new DriSubset(reference!.AsValuedNode().AsString(), directory, parent);
    }

    private static DriAsset MapAsset(IGraph graph)
    {
        var id = graph.GetSingleUriNode(Vocabulary.AssetDriId);
        var reference = graph.GetSingleText(Vocabulary.AssetReference);
        var subsetReference = graph.GetSingleText(Vocabulary.SubsetReference);
        var location = graph.GetSingleText(Vocabulary.ImportLocation);

        return new DriAsset(id!.Uri, reference!, location, subsetReference!);
    }

    private static DriVariation MapVariation(IGraph graph)
    {
        var id = graph.GetSingleUriNode(Vocabulary.VariationDriId);
        var assetReference = graph.GetSingleText(Vocabulary.AssetReference);
        var name = graph.GetSingleText(Vocabulary.VariationName);

        return new DriVariation(id!.Uri, name!, assetReference!);
    }

    private static DriSensitivityReview MapSensitivityReview(IGraph graph)
    {
        var tempReferencePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-reference"));
        var tempIdPredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-id"));
        var tempTypePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-type"));

        var id = graph.GetSingleUriNode(Vocabulary.SensitivityReviewDriId);
        var reference = graph.GetSingleText(tempReferencePredicate);
        var targetType = graph.GetSingleUriNode(tempTypePredicate);
        var targetId = graph.GetSingleUriNode(tempIdPredicate);
        var date = graph.GetSingleDate(Vocabulary.SensitivityReviewDate);
        var sensitiveName = graph.GetSingleText(Vocabulary.SensitivityReviewSensitiveName);
        var sensitiveDescription = graph.GetSingleText(Vocabulary.SensitivityReviewSensitiveDescription);
        var past = graph.GetSingleUriNode(Vocabulary.SensitivityReviewHasPastSensitivityReview);
        var changeDriId = graph.GetSingleUriNode(Vocabulary.ChangeDriId);
        var changeDescription = graph.GetSingleText(Vocabulary.ChangeDescription);
        var changeDateTime = graph.GetSingleDate(Vocabulary.ChangeDateTime);
        var operatorIdentifier = graph.GetSingleUriNode(Vocabulary.OperatorIdentifier);
        var operatorName = graph.GetSingleText(Vocabulary.OperatorName);

        if (targetType!.Uri.Fragment == "#DeliverableUnit")
        {
            return new DriSensitivityReview(id!.Uri, reference!, targetId!.Uri,
                targetType!.Uri, null, [], null, past?.Uri, sensitiveName,
                sensitiveDescription, date, null, null, null, null, null, null,
                null, changeDriId?.Uri, changeDescription, changeDateTime,
                operatorIdentifier?.Uri, operatorName);
        }

        var reviewDate = graph.GetSingleDate(Vocabulary.SensitivityReviewRestrictionReviewDate);
        var startDate = graph.GetSingleDate(Vocabulary.SensitivityReviewRestrictionCalculationStartDate);
        var duration = graph.GetSingleNumber(Vocabulary.SensitivityReviewRestrictionDuration);
        var description = graph.GetSingleText(Vocabulary.SensitivityReviewRestrictionDescription);
        var instrumentNumber = graph.GetSingleNumber(Vocabulary.RetentionInstrumentNumber);
        var instrumentSignedDate = graph.GetSingleDate(Vocabulary.RetentionInstrumentSignatureDate);
        var restrictionReviewDate = graph.GetSingleDate(Vocabulary.RetentionRestrictionReviewDate);
        var groundCode = graph.GetSingleUriNode(Vocabulary.GroundForRetentionCode);
        var accessCode = graph.GetSingleUriNode(Vocabulary.AccessConditionCode);
        var legislationUris = graph.GetUriNodes(Vocabulary.LegislationHasUkLegislation).Select(u => u.Uri);

        return new DriSensitivityReview(id!.Uri, reference!, targetId!.Uri, targetType!.Uri,
            accessCode!.Uri, legislationUris, reviewDate, past?.Uri, sensitiveName,
            sensitiveDescription, date, startDate, duration, description, instrumentNumber,
            instrumentSignedDate, restrictionReviewDate, groundCode?.Uri, changeDriId?.Uri,
            changeDescription, changeDateTime, operatorIdentifier?.Uri, operatorName);
    }
}
