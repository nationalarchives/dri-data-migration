using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class VariationIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationIngest> logger)
    : StagingIngest<DriVariation>(sparqlClient, logger, cacheClient, "VariationGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariation dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var id = existing.GetTriplesWithPredicate(Vocabulary.VariationHasAsset).FirstOrDefault()?.Subject ?? CacheClient.NewId;
        var asset = await cacheClient.CacheFetch(CacheEntityKind.Asset, dri.AssetReference, cancellationToken);
        if (asset is null)
        {
            logger.AssetNotFound(dri.AssetReference);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationHasAsset, asset);
        GraphAssert.Text(graph, id, new Dictionary<IUriNode, string?>()
        {
            [Vocabulary.VariationDriId] = dri.Id,
            [Vocabulary.VariationName] = dri.VariationName
        });
        logger.RecordBuilt(dri.Id);

        return graph;
    }
}
