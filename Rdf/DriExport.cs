using Api;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Rdf;

public class DriExport(HttpClient httpClient, IOptions<DriSettings> options)
{
    private readonly Assembly currentAssembly = typeof(DriExport).Assembly;
    private readonly string baseName = $"{typeof(DriExport).Namespace}.Sparql.Dri";
    private readonly SparqlQueryClient client = new(httpClient, new Uri(options.Value.SparqlConnectionString));

    public async Task<IEnumerable<DriSubset>> GetBroadestSubsets()
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetBroadestSubsets));

        var result = await client.QueryWithResultSetAsync(sparql);

        return result.Results.Select(s => new DriSubset(
            s.Value("code").AsValuedNode().AsString(), s.Value("code").AsValuedNode().AsString(), null));
    }

    public async Task<IEnumerable<DriAccessCondition>> GetAccessConditions()
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetAccessConditions));

        var result = await client.QueryWithResultSetAsync(sparql);

        return result.Results.Select(s => new DriAccessCondition(
            GetUriFragment(s.Value("c") as IUriNode)!, s.Value("label").AsValuedNode().AsString()));
    }

    public async Task<IEnumerable<DriLegislation>> GetLegislations()
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetLegislations));

        var result = await client.QueryWithResultSetAsync(sparql);

        return result.Results.Select(s => new DriLegislation(
            (s.Value("legislation") as IUriNode)!.Uri, s.HasValue("label") ? s.Value("label")?.AsValuedNode().AsString() : null));
    }

    public async Task<IEnumerable<DriGroundForRetention>> GetGroundForRetentions()
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetGroundForRetentions));

        var result = await client.QueryWithResultSetAsync(sparql);

        return result.Results.Select(s => new DriGroundForRetention(
            s.Value("label").AsValuedNode().AsString(), s.Value("comment").AsValuedNode().AsString()));
    }

    public async Task<IEnumerable<DriSubset>> GetSubsetsByCode(string code, int pageSize, int offset)
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetSubsetsByCode));
        var graph = await GraphResource.GetGraph(client, sparql, new Dictionary<string, object>
        { 
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        });

        return graph.Triples.SubjectNodes.Where(s => s is not BlankNode)
            .Cast<IUriNode>()
            .Select(s => SusbsetBySubject(graph, s));
    }

    public async Task<IEnumerable<DriAsset>> GetAssetsByCode(string code, int pageSize, int offset)
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetAssetsByCode));
        var graph = await GraphResource.GetGraph(client, sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        });

        return graph.GetTriplesWithPredicate(Vocabulary.AssetReference)
            .Select(t => t.Subject as IBlankNode)
            .Select(s => AssetBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriVariation>> GetVariationsByCode(string code, int pageSize, int offset)
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetVariationsByCode));
        var graph = await GraphResource.GetGraph(client, sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        });

        return graph.GetTriplesWithPredicate(Vocabulary.VariationName)
            .Select(t => t.Subject as IUriNode)
            .Select(s => VariationBySubject(graph, s!));
    }

    public async Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCode(string code, int pageSize, int offset)
    {
        var sparql = SparqlResource.GetEmbeddedSparql(currentAssembly, baseName, nameof(GetSensitivityReviewsByCode));
        var graph = await GraphResource.GetGraph(client, sparql, new Dictionary<string, object>
        {
            { "id", code },
            { "limit", pageSize },
            { "offset", offset}
        });

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
        var instrumentSignedDate = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionInstrumentSignedDate).SingleOrDefault()?.Object as ILiteralNode;
        var restrictionReviewDate = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate).SingleOrDefault()?.Object as ILiteralNode;
        var ground = graph.GetTriplesWithSubjectPredicate(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention).SingleOrDefault()?.Object as IBlankNode;
        var groundCode = graph.GetTriplesWithSubjectPredicate(ground, Vocabulary.GroundForRetentionCode).SingleOrDefault()?.Object as IUriNode;
        var condition = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SensitivityReviewHasAccessCondition).SingleOrDefault().Object as IBlankNode;
        var accessCode = graph.GetTriplesWithSubjectPredicate(condition, Vocabulary.AccessConditionCode).SingleOrDefault().Object as IUriNode;
        var legislation = graph.GetTriplesWithSubjectPredicate(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation).SingleOrDefault().Object as IBlankNode;
        var legislations = graph.GetTriplesWithSubjectPredicate(legislation, Vocabulary.LegislationHasUkLegislation).Select(t => (t.Object as IUriNode)!.Uri);

        var targetReference = (targetType!.Uri.Fragment == "#DeliverableUnit") ? reference.AsValuedNode().AsString() : null;

        return new DriSensitivityReview(id!.Uri.ToString(), targetReference, targetId!.Uri,
            GetUriFragment(accessCode)!, legislations, reviewDate?.AsValuedNode().AsDateTimeOffset(),
            past?.Uri.ToString(), sensitiveName?.AsValuedNode().AsString(), sensitiveDescription?.AsValuedNode().AsString(),
            date?.AsValuedNode().AsDateTimeOffset(), startDate?.AsValuedNode().AsDateTimeOffset(),
            duration?.AsValuedNode().AsInteger(), description?.AsValuedNode().AsString(),
            instrumentNumber?.AsValuedNode().AsInteger(), instrumentSignedDate?.AsValuedNode().AsDateTimeOffset(),
            restrictionReviewDate?.AsValuedNode().AsDateTimeOffset(), GetUriFragment(groundCode));
    }

    private static DriVariation VariationBySubject(IGraph graph, IUriNode subject)
    {
        var id = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationDriId).SingleOrDefault().Object as IUriNode;
        var asset = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationHasAsset).SingleOrDefault().Object as IBlankNode;
        var assetReference = graph.GetTriplesWithSubjectPredicate(asset, Vocabulary.AssetReference).SingleOrDefault().Object as ILiteralNode;
        var name = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.VariationName).SingleOrDefault().Object as ILiteralNode;

        return new DriVariation(id!.Uri.ToString(), name.AsValuedNode().AsString(), assetReference.AsValuedNode().AsString());
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
        var collection = graph.GetTriplesWithSubjectPredicate(subject, Vocabulary.SubsetHasBroaderSubset).SingleOrDefault()?.Object as IBlankNode;
        ILiteralNode? parent = null;
        if (collection is not null)
        {
            parent = graph.GetTriplesWithSubjectPredicate(collection, Vocabulary.SubsetReference).SingleOrDefault()?.Object as ILiteralNode;
        }

        return new DriSubset(reference!.AsValuedNode().AsString(), directory.AsValuedNode().AsString(),
            parent?.AsValuedNode().AsString());
    }

    private static string? GetUriFragment(IUriNode? node) => node?.Uri.Fragment.Substring(1);
}
