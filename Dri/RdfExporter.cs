using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;

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

    public async Task<IEnumerable<DriSubset>> GetSubsetsByCodeAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetSubsetsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetSubsetsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.Triples.SubjectNodes.Where(s => s is not BlankNode)
            .Cast<IUriNode>()
            .Select(s => SusbsetBySubject(graph, s));
    }

    public async Task<IEnumerable<DriAsset>> GetAssetsByCodeAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetAssetsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetAssetsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.AssetReference)
            .Select(t => t.Subject as IUriNode)
            .Select(s => AssetBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriVariation>> GetVariationsByCodeAsync(int offset, CancellationToken cancellationToken)
    {
        logger.GetVariationsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetVariationsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
            { "offset", offset}
        }, cancellationToken);

        return graph.GetTriplesWithPredicate(Vocabulary.VariationName)
            .Select(t => t.Subject as IUriNode)
            .Select(s => VariationBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCodeAsync(
        int offset, CancellationToken cancellationToken)
    {
        logger.GetSensitivityReviewsByCode(offset);
        var sparql = embedded.GetSparql(nameof(GetSensitivityReviewsByCodeAsync));
        var graph = await sparqlClient.GetGraphAsync(sparql, new Dictionary<string, object>
        {
            { "id", settings.Code },
            { "limit", settings.FetchPageSize },
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
    }

    private static DriVariation VariationBySubject(IGraph graph, IUriNode subject)
    {
        var id = graph.GetSingleUriNode(subject, Vocabulary.VariationDriId);
        var asset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationHasAsset).SingleOrDefault().Object as IBlankNode;
        var assetReference = graph.GetSingleText(asset!, Vocabulary.AssetReference);
        var name = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationName).SingleOrDefault().Object as ILiteralNode;

        return new DriVariation(id!.Uri, name.AsValuedNode().AsString(), assetReference!);
    }

    private static DriAsset AssetBySubject(IGraph graph, IUriNode subject)
    {
        var id = graph.GetSingleUriNode(subject, Vocabulary.AssetDriId);
        var reference = graph.GetSingleText(subject, Vocabulary.AssetReference);
        var subset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasSubset).SingleOrDefault().Object as IBlankNode;
        var subsetReference = graph.GetSingleText(subset!, Vocabulary.SubsetReference);
        var retention = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.AssetHasRetention).SingleOrDefault().Object as IBlankNode;
        var location = graph.GetSingleText(retention!, Vocabulary.ImportLocation);

        return new DriAsset(id!.Uri, reference!, location, subsetReference!);
    }

    private static DriSubset SusbsetBySubject(IGraph graph, IUriNode subject)
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

        return new DriSubset(reference!, directory, parent);
    }
}
