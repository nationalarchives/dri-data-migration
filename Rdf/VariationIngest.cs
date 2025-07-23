using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class VariationIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger)
    : StagingIngest<DriVariation>(cache, sparqlClient, logger, "VariationGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriVariation dri)
    {
        var id = existing.GetTriplesWithPredicate(Vocabulary.VariationHasAsset).FirstOrDefault()?.Subject ?? NewId;
        var asset = await CacheFetch(CacheEntityKind.Asset, dri.AssetReference);
        //TODO: handle null
        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationHasAsset, asset);
        graph.Assert(id, Vocabulary.VariationDriId, new LiteralNode(dri.Id));
        graph.Assert(id, Vocabulary.VariationName, new LiteralNode(dri.VariationName));

        return graph;
    }
}
