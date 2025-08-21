using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Staging;

public class VariationIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationIngest> logger)
    : BaseStagingIngest<DriVariation>(sparqlClient, logger, "VariationGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariation dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var id = existing.GetTriplesWithPredicate(Vocabulary.VariationHasAsset).FirstOrDefault()?.Subject ?? BaseIngest.NewId;
        var asset = await cacheClient.CacheFetch(CacheEntityKind.Asset, dri.AssetReference, cancellationToken);
        if (asset is null)
        {
            logger.AssetNotFound(dri.AssetReference);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationHasAsset, asset);
        graph.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        graph.Assert(id, Vocabulary.VariationName, new LiteralNode(dri.VariationName));
        logger.RecordBuilt(dri.Id);

        return graph;
    }
}
