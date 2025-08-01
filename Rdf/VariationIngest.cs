using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class VariationIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<VariationIngest> logger)
    : BaseStagingIngest<DriVariation>(cache, sparqlClient, logger, "VariationGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariation dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var id = existing.GetTriplesWithPredicate(Vocabulary.VariationHasAsset).FirstOrDefault()?.Subject ?? NewId;
        var asset = await CacheFetch(CacheEntityKind.Asset, dri.AssetReference, cancellationToken);
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
