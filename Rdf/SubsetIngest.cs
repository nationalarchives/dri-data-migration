using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class SubsetIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<SubsetIngest> logger)
    : BaseStagingIngest<DriSubset>(cache, sparqlClient, logger, "SubsetGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriSubset dri)
    {
        logger.BuildingRecord(dri.Id);
        var subsetReference = new LiteralNode(dri.Reference);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.SubsetReference, subsetReference).FirstOrDefault()?.Subject ?? NewId;
        var retention = existing.GetTriplesWithSubjectPredicate(id, Vocabulary.SubsetHasRetention).FirstOrDefault()?.Object ?? NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.SubsetReference, subsetReference);
        if (!string.IsNullOrEmpty(dri.Directory))
        {
            graph.Assert(id, Vocabulary.SubsetHasRetention, retention);
            graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));
        }
        if (!string.IsNullOrEmpty(dri.ParentReference))
        {
            var broaderId = await CacheFetchOrNew(CacheEntityKind.Subset, dri.ParentReference);
            graph.Assert(id, Vocabulary.SubsetHasBroaderSubset, broaderId);
            graph.Assert(broaderId, Vocabulary.SubsetReference, new LiteralNode(dri.ParentReference));
        }
        logger.RecordBuilt(dri.Id);

        return graph;
    }

}
