using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rdf;
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
    private readonly ILogger<RdfExporter> logger;
    private readonly IDriSparqlClient sparqlClient;
    private readonly EmbeddedResource embedded;
    private readonly DriSettings settings;

    public RdfExporter(ILogger<RdfExporter> logger, IOptions<DriSettings> driSettings,
        IDriSparqlClient sparqlClient)
    {
        this.logger = logger;
        settings = driSettings.Value;
        this.sparqlClient = sparqlClient;

        var currentAssembly = typeof(RdfExporter).Assembly;
        var baseName = $"{typeof(RdfExporter).Namespace}.Sparql";
        embedded = new(currentAssembly, baseName);
    }

    public Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync(CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.AccessCondition, MapAccessCondition, cancellationToken);

    public Task<IEnumerable<DriLegislation>> GetLegislationsAsync(CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.Legislation, MapLegislation, cancellationToken);

    public Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync(CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.GroundForRetention, MapGroundForRetention, cancellationToken);

    public Task<IEnumerable<DriSubset>> GetSubsetsAsync(int offset, CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.Subset, offset, Vocabulary.SubsetHasBroaderSubset, MapSubset, cancellationToken);

    public Task<IEnumerable<DriAsset>> GetAssetsAsync(int offset, CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.Asset, offset, Vocabulary.AssetReference, MapAsset, cancellationToken);

    public Task<IEnumerable<DriVariation>> GetVariationsAsync(int offset, CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.Variation, offset, Vocabulary.VariationName, MapVariation, cancellationToken);

    public Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsAsync(int offset, CancellationToken cancellationToken) =>
        GetAsync(EtlStageType.SensitivityReview, offset, Vocabulary.SensitivityReviewDriId, MapSensitivityReview, cancellationToken);

    private async Task<IEnumerable<T>> GetAsync<T>(EtlStageType stageType, Func<ISparqlResult, T> mapping,
        CancellationToken cancellationToken)
    {
        logger.FetchingRecords(stageType);
        var sparql = embedded.GetSparql($"Get{stageType}");
        var resultSet = await sparqlClient.GetResultSetAsync(sparql, cancellationToken);

        return resultSet.Results.Select(mapping);
    }

    private async Task<IEnumerable<T>> GetAsync<T>(EtlStageType stageType, int offset, IUriNode predicate,
        Func<IGraph, IUriNode, T> mapping, CancellationToken cancellationToken)
    {
        logger.FetchingRecordsOffset(stageType, offset);
        var sparql = embedded.GetSparql($"Get{stageType}");
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(predicate)
            .Select(t => t.Subject)
            .Cast<IUriNode>()
            .Select(s => mapping(graph, s));
    }

    private static Func<ISparqlResult, DriAccessCondition> MapAccessCondition =>
        result => new DriAccessCondition((result.Value("s") as IUriNode)!.Uri,
            result.Value("label").AsValuedNode().AsString());

    private static Func<ISparqlResult, DriLegislation> MapLegislation =>
        result => new DriLegislation((result.Value("legislation") as IUriNode)!.Uri,
            result.HasValue("label") ? result.Value("label")?.AsValuedNode().AsString() : null);

    private static Func<ISparqlResult, DriGroundForRetention> MapGroundForRetention =>
        result => new DriGroundForRetention(result.Value("label").AsValuedNode().AsString(),
            result.Value("comment").AsValuedNode().AsString());

    private static Func<IGraph, IUriNode, DriSubset> MapSubset => (graph, subject) =>
    {
        var reference = graph.GetSingleText(subject, Vocabulary.SubsetReference);
        var retention = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetHasRetention).SingleOrDefault().Object as IBlankNode;
        var directory = graph.GetSingleText(retention!, Vocabulary.ImportLocation);
        var broader = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetHasBroaderSubset).SingleOrDefault()?.Object as IBlankNode;
        string? parent = null;
        if (broader is not null)
        {
            parent = graph.GetSingleText(broader, Vocabulary.SubsetReference);
        }
        var transfer = graph.GetSingleUriNode(subject, Vocabulary.SubsetHasTransfer)?.Uri;

        return new DriSubset(reference!, directory, parent, transfer);
    };

    private static Func<IGraph, IUriNode, DriAsset> MapAsset => (graph, subject) =>
    {
        var id = graph.GetSingleUriNode(subject, Vocabulary.AssetDriId);
        var reference = graph.GetSingleText(subject, Vocabulary.AssetReference);
        var subset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasSubset).SingleOrDefault().Object as IBlankNode;
        var subsetReference = graph.GetSingleText(subset!, Vocabulary.SubsetReference);
        var retention = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasRetention).SingleOrDefault().Object as IBlankNode;
        var location = graph.GetSingleText(retention!, Vocabulary.ImportLocation);
        var transfer = graph.GetSingleUriNode(subject, Vocabulary.AssetHasTransfer)?.Uri;

        return new DriAsset(id!.Uri, reference!, location, subsetReference!, transfer);
    };

    private static Func<IGraph, IUriNode, DriVariation> MapVariation => (graph, subject) =>
    {
        var id = graph.GetSingleUriNode(subject, Vocabulary.VariationDriId);
        var asset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationHasAsset).SingleOrDefault().Object as IBlankNode;
        var assetReference = graph.GetSingleText(asset!, Vocabulary.AssetReference);
        var name = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationName).SingleOrDefault().Object as ILiteralNode;

        return new DriVariation(id!.Uri, name.AsValuedNode().AsString(), assetReference!);
    };

    private static Func<IGraph, IUriNode, DriSensitivityReview> MapSensitivityReview => (graph, subject) =>
    {
        var tempReferencePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-reference"));
        var tempIdPredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-id"));
        var tempTypePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-type"));
        var coalesceNode = graph.CreateUriNode(new Uri("http://not.found"));

        var id = graph.GetSingleUriNode(subject, Vocabulary.SensitivityReviewDriId);
        var reference = graph.GetSingleText(subject, tempReferencePredicate);
        var targetType = graph.GetSingleUriNode(subject, tempTypePredicate);
        var targetId = graph.GetSingleUriNode(subject, tempIdPredicate);
        var date = graph.GetSingleLiteral(subject, Vocabulary.SensitivityReviewDate);
        var sensitiveName = graph.GetSingleText(subject, Vocabulary.SensitivityReviewSensitiveName);
        var sensitiveDescription = graph.GetSingleText(subject, Vocabulary.SensitivityReviewSensitiveDescription);
        var past = graph.GetSingleUriNode(subject, Vocabulary.SensitivityReviewHasPastSensitivityReview);
        var changeDriId = graph.GetSingleUriNode(subject, Vocabulary.ChangeDriId);
        var changeDescription = graph.GetSingleText(subject, Vocabulary.ChangeDescription);
        var changeDateTime = graph.GetSingleLiteral(subject, Vocabulary.ChangeDateTime);
        var operatorId = graph.GetSingleUriNode(subject, Vocabulary.ChangeHasOperator);
        var operatorIdentifier = graph.GetSingleUriNode(operatorId ?? coalesceNode, Vocabulary.OperatorIdentifier);
        var operatorName = graph.GetSingleText(operatorId ?? coalesceNode, Vocabulary.OperatorName);

        if (targetType!.Uri.Fragment == "#DeliverableUnit")
        {
            return new DriSensitivityReview(id!.Uri, reference!,
                targetId!.Uri, targetType!.Uri, null, [], null, past?.Uri,
                sensitiveName, sensitiveDescription, date?.AsValuedNode().AsDateTimeOffset(),
                null, null, null, null, null, null, null, changeDriId?.Uri, changeDescription,
                changeDateTime?.AsValuedNode().AsDateTimeOffset(), operatorIdentifier?.Uri,
                operatorName);
        }

        var reviewDate = graph.GetSingleLiteral(subject, Vocabulary.SensitivityReviewRestrictionReviewDate);
        var startDate = graph.GetSingleLiteral(subject, Vocabulary.SensitivityReviewRestrictionCalculationStartDate);
        var duration = graph.GetSingleNumber(subject, Vocabulary.SensitivityReviewRestrictionDuration);
        var description = graph.GetSingleText(subject, Vocabulary.SensitivityReviewRestrictionDescription);
        var instrumentNumber = graph.GetSingleNumber(subject, Vocabulary.RetentionInstrumentNumber);
        var instrumentSignedDate = graph.GetSingleLiteral(subject, Vocabulary.RetentionInstrumentSignatureDate);
        var restrictionReviewDate = graph.GetSingleLiteral(subject, Vocabulary.RetentionRestrictionReviewDate);
        var groundCode = graph.GetSingleUriNode(subject, Vocabulary.GroundForRetentionCode);
        var accessCode = graph.GetSingleUriNode(subject, Vocabulary.AccessConditionCode);
        var legislationUris = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewRestrictionHasLegislation)
            .SelectMany(l => graph.GetTriplesWithSubjectPredicate(l.Object, Vocabulary.LegislationHasUkLegislation)
                .Select(t => t.Object).Cast<IUriNode>().Select(u => u.Uri));

        return new DriSensitivityReview(id!.Uri, reference!, targetId!.Uri, targetType!.Uri,
            accessCode!.Uri, legislationUris, reviewDate?.AsValuedNode().AsDateTimeOffset(),
            past?.Uri, sensitiveName, sensitiveDescription, date?.AsValuedNode().AsDateTimeOffset(),
            startDate?.AsValuedNode().AsDateTimeOffset(), duration, description, instrumentNumber,
            instrumentSignedDate?.AsValuedNode().AsDateTimeOffset(),
            restrictionReviewDate?.AsValuedNode().AsDateTimeOffset(), groundCode?.Uri,
            changeDriId?.Uri, changeDescription, changeDateTime?.AsValuedNode().AsDateTimeOffset(),
            operatorIdentifier?.Uri, operatorName);
    };
}
