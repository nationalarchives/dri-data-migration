using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class SubsetIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<SubsetIngest> logger) :
    StagingIngest<DriSubset>(sparqlClient, logger, "SubsetGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriSubset dri, CancellationToken cancellationToken)
    {
        var subsetReference = new LiteralNode(dri.Reference);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.SubsetReference, subsetReference) ?? CacheClient.NewId;
        var retention = existing.GetSingleUriNode(id, Vocabulary.SubsetHasRetention) ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.SubsetReference, subsetReference);
        if (!string.IsNullOrEmpty(dri.Directory))
        {
            graph.Assert(id, Vocabulary.SubsetHasRetention, retention);
            graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));
        }
        if (!string.IsNullOrEmpty(dri.ParentReference))
        {
            var broaderId = await cacheClient.CacheFetchOrNew(CacheEntityKind.Subset, dri.ParentReference, Vocabulary.SubsetReference, cancellationToken);
            graph.Assert(id, Vocabulary.SubsetHasBroaderSubset, broaderId);
        }

        return graph;
    }

}
