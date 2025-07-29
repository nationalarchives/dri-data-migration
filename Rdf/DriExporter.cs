using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Rdf;

public class DriExporter : IDriExporter
{
    private readonly ILogger<DriExporter> logger;
    private readonly IDriSparqlClient sparqlClient;
    private readonly EmbeddedSparqlResource embedded;

    public DriExporter(ILogger<DriExporter> logger, IDriSparqlClient sparqlClient)
    {
        this.logger = logger;
        this.sparqlClient = sparqlClient;

        var currentAssembly = typeof(DriExporter).Assembly;
        var baseName = $"{typeof(DriExporter).Namespace}.Sparql.Dri";
        embedded = new(currentAssembly, baseName);
    }

    public async Task<IEnumerable<DriSubset>> GetBroadestSubsetsAsync(CancellationToken cancellationToken)
    {
        logger.GetBroadestSubsets();
        var sparql = embedded.GetSparql(nameof(GetBroadestSubsetsAsync));

        var result = await sparqlClient.GetResultSetAsync(sparql, cancellationToken);

        return result.Results.Select(s => new DriSubset(
            s.Value("directory").AsValuedNode().AsString(), s.Value("directory").AsValuedNode().AsString(), null));
    }

    public async Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync(CancellationToken cancellationToken)
    {
        logger.GetAccessConditions();
        var sparql = embedded.GetSparql(nameof(GetAccessConditionsAsync));

        var result = await sparqlClient.GetResultSetAsync(sparql, cancellationToken);

        return result.Results.Select(s => new DriAccessCondition(
            (s.Value("s") as IUriNode)!.Uri, s.Value("label").AsValuedNode().AsString()));
    }

    public async Task<IEnumerable<DriLegislation>> GetLegislationsAsync(CancellationToken cancellationToken)
    {
        logger.GetLegislations();
        var sparql = embedded.GetSparql(nameof(GetLegislationsAsync));

        var result = await sparqlClient.GetResultSetAsync(sparql, cancellationToken);

        return result.Results.Select(s => new DriLegislation(
            (s.Value("legislation") as IUriNode)!.Uri, s.HasValue("label") ? s.Value("label")?.AsValuedNode().AsString() : null));
    }

    public async Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync(CancellationToken cancellationToken)
    {
        logger.GetGroundsForRetention();
        var sparql = embedded.GetSparql(nameof(GetGroundsForRetentionAsync));

        var result = await sparqlClient.GetResultSetAsync(sparql, cancellationToken);

        return result.Results.Select(s => new DriGroundForRetention(
            s.Value("label").AsValuedNode().AsString(), s.Value("comment").AsValuedNode().AsString()));
    }

    public async Task<IEnumerable<DriSubset>> GetSubsetsByCodeAsync(
        string code, int pageSize, int offset, CancellationToken cancellationToken)
    {
        logger.GetSubsetsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetSubsetsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.Triples.SubjectNodes.Where(s => s is not BlankNode)
            .Cast<IUriNode>()
            .Select(s => SusbsetBySubject(graph, s));
    }

    public async Task<IEnumerable<DriAsset>> GetAssetsByCodeAsync(string code, int pageSize, int offset,
        CancellationToken cancellationToken)
    {
        logger.GetAssetsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetAssetsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.AssetReference)
            .Select(t => t.Subject as IBlankNode)
            .Select(s => AssetBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriVariation>> GetVariationsByCodeAsync(
        string code, int pageSize, int offset, CancellationToken cancellationToken)
    {
        logger.GetVariationsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetVariationsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.VariationName)
            .Select(t => t.Subject as IUriNode)
            .Select(s => VariationBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCodeAsync(
        string code, int pageSize, int offset, CancellationToken cancellationToken)
    {
        logger.GetSensitivityReviewsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetSensitivityReviewsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.SensitivityReviewDriId)
            .Select(t => t.Subject as IUriNode)
            .Select(s => SensitivityReviewBySubject(graph, s!));
    }

    private static DriSensitivityReview SensitivityReviewBySubject(IGraph graph, IUriNode subject)
    {
        var tempReferencePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-reference"));
        var tempIdPredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-id"));
        var tempTypePredicate = new UriNode(new Uri(Vocabulary.Namespace, "x-type"));

        var id = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewDriId).SingleOrDefault().Object as IUriNode;
        var reference = graph.GetTriplesWithSubjectPredicate(subject, tempReferencePredicate).SingleOrDefault().Object as ILiteralNode;
        var targetType = graph.GetTriplesWithSubjectPredicate(subject, tempTypePredicate).SingleOrDefault().Object as IUriNode;
        var targetId = graph.GetTriplesWithSubjectPredicate(subject, tempIdPredicate).SingleOrDefault().Object as IUriNode;
        var date = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewDate).SingleOrDefault()?.Object as ILiteralNode;
        var sensitiveName = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewSensitiveName).SingleOrDefault()?.Object as ILiteralNode;
        var sensitiveDescription = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewSensitiveDescription).SingleOrDefault()?.Object as ILiteralNode;
        var past = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewHasPastSensitivityReview).SingleOrDefault()?.Object as IUriNode;
        var restriction = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction).SingleOrDefault().Object as IBlankNode;
        var reviewDate = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate).SingleOrDefault()?.Object as ILiteralNode;
        var startDate = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate).SingleOrDefault()?.Object as ILiteralNode;
        var duration = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionDuration).SingleOrDefault()?.Object as ILiteralNode;
        var description = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionDescription).SingleOrDefault()?.Object as ILiteralNode;
        var retentionRestriction = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction).SingleOrDefault()?.Object as IBlankNode;
        var instrumentNumber = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionInstrumentNumber).SingleOrDefault()?.Object as ILiteralNode;
        var instrumentSignedDate = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate).SingleOrDefault()?.Object as ILiteralNode;
        var restrictionReviewDate = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate).SingleOrDefault()?.Object as ILiteralNode;
        var ground = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention).SingleOrDefault()?.Object as IBlankNode;
        var groundCode = graph.GetTriplesWithSubjectPredicate(ground, Vocabulary.GroundForRetentionCode).SingleOrDefault()?.Object as IUriNode;
        var condition = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewHasAccessCondition).SingleOrDefault().Object as IBlankNode;
        var accessCode = graph.GetTriplesWithSubjectPredicate(condition, Vocabulary.AccessConditionCode).SingleOrDefault().Object as IUriNode;
        var legislation = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation).SingleOrDefault().Object as IBlankNode;
        var legislations = graph.GetTriplesWithSubjectPredicate(legislation, Vocabulary.LegislationHasUkLegislation).Select(t => (t.Object as IUriNode)!.Uri);

        return new DriSensitivityReview(id!.Uri, reference.AsValuedNode().AsString(), targetId!.Uri, targetType!.Uri,
            accessCode!.Uri, legislations, reviewDate?.AsValuedNode().AsDateTimeOffset(),
            past?.Uri, sensitiveName?.AsValuedNode().AsString(), sensitiveDescription?.AsValuedNode().AsString(),
            date?.AsValuedNode().AsDateTimeOffset(), startDate?.AsValuedNode().AsDateTimeOffset(),
            duration?.AsValuedNode().AsInteger(), description?.AsValuedNode().AsString(),
            instrumentNumber?.AsValuedNode().AsInteger(), instrumentSignedDate?.AsValuedNode().AsDateTimeOffset(),
            restrictionReviewDate?.AsValuedNode().AsDateTimeOffset(), groundCode?.Uri);
    }

    private static DriVariation VariationBySubject(IGraph graph, IUriNode subject)
    {
        var id = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationDriId).SingleOrDefault().Object as IUriNode;
        var asset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationHasAsset).SingleOrDefault().Object as IBlankNode;
        var assetReference = graph.GetTriplesWithSubjectPredicate(asset, Vocabulary.AssetReference).SingleOrDefault().Object as ILiteralNode;
        var name = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationName).SingleOrDefault().Object as ILiteralNode;

        return new DriVariation(id!.Uri, name.AsValuedNode().AsString(), assetReference.AsValuedNode().AsString());
    }

    private static DriAsset AssetBySubject(IGraph graph, IBlankNode subject)
    {
        var reference = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetReference).SingleOrDefault().Object as ILiteralNode;
        var subset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasSubset).SingleOrDefault().Object as IBlankNode;
        var subsetReference = graph.GetTriplesWithSubjectPredicate(subset, Vocabulary.SubsetReference).SingleOrDefault().Object as ILiteralNode;
        var retention = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasRetention).SingleOrDefault().Object as IBlankNode;
        var location = graph.GetTriplesWithSubjectPredicate(retention, Vocabulary.ImportLocation).SingleOrDefault().Object as ILiteralNode;

        return new DriAsset(reference.AsValuedNode().AsString(), location.AsValuedNode().AsString(), subsetReference.AsValuedNode().AsString());
    }

    private static DriSubset SusbsetBySubject(IGraph graph, IUriNode subject)
    {
        var reference = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetReference).SingleOrDefault().Object as ILiteralNode;
        var retention = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetHasRetention).SingleOrDefault().Object as IBlankNode;
        var directory = graph.GetTriplesWithSubjectPredicate(retention, Vocabulary.ImportLocation).SingleOrDefault().Object as ILiteralNode;
        var broader = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetHasBroaderSubset).SingleOrDefault()?.Object as IBlankNode;
        ILiteralNode? parent = null;
        if (broader is not null)
        {
            parent = graph.GetTriplesWithSubjectPredicate(broader, Vocabulary.SubsetReference).SingleOrDefault()?.Object as ILiteralNode;
        }

        return new DriSubset(reference!.AsValuedNode().AsString(), directory.AsValuedNode().AsString(),
            parent?.AsValuedNode().AsString());
    }
}
