using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class AssetIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger)
    : StagingIngest<DriAsset>(cache, sparqlClient, logger, "AssetGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriAsset dri)
    {
        var assetReference = new LiteralNode(dri.Reference);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.AssetReference, assetReference).FirstOrDefault()?.Subject ?? NewId;
        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.AssetHasRetention).FirstOrDefault()?.Object ?? NewId;

        var subset = await CacheFetch(CacheEntityKind.Subset, dri.SubsetReference);
        //TODO: handle null
        var graph = new Graph();
        graph.Assert(id, Vocabulary.AssetReference, assetReference);
        graph.Assert(id, Vocabulary.AssetHasSubset, subset);
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));

        return graph;
    }

}
