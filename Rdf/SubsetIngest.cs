using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class SubsetIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger)
    : StagingIngest<DriSubset>(cache, sparqlClient, logger, "SubsetGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriSubset dri)
    {
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

        return graph;
    }

}
