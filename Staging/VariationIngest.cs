using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;

namespace Staging;

public class VariationIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationIngest> logger) :
    StagingIngest<DriVariation>(sparqlClient, logger, "VariationGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariation dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.VariationDriId, driId) ?? CacheClient.NewId;
        var asset = await cacheClient.CacheFetch(CacheEntityKind.Asset, dri.AssetReference, cancellationToken);
        if (asset is null)
        {
            logger.AssetNotFound(dri.AssetReference);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationHasAsset, asset);
        GraphAssert.Text(graph, id, dri.VariationName, Vocabulary.VariationName);

        return graph;
    }
}
