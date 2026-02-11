using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;

namespace Staging;

public class AssetIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<AssetIngest> logger) :
    StagingIngest<DriAsset>(sparqlClient, logger, "AssetGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriAsset dri, CancellationToken cancellationToken)
    {
        var assetReference = new LiteralNode(dri.Reference);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.AssetReference, assetReference) ?? CacheClient.NewId;
        var retention = existing.GetSingleUriNode(id, Vocabulary.AssetHasRetention) ?? CacheClient.NewId;

        var subset = await cacheClient.CacheFetch(CacheEntityKind.Subset, dri.SubsetReference, cancellationToken);
        if (subset is null && dri.SubsetReference == "WO 409")//Special case
        {
            subset = await cacheClient.CacheFetchOrNew(CacheEntityKind.Subset, dri.SubsetReference, Vocabulary.SubsetReference, cancellationToken);
        }
        if (subset is null)
        {
            logger.SubsetNotFound(dri.SubsetReference);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AssetDriId, new LiteralNode(dri.Id));
        graph.Assert(id, Vocabulary.AssetReference, assetReference);
        graph.Assert(id, Vocabulary.AssetHasSubset, subset);
        graph.Assert(id, Vocabulary.AssetHasRetention, retention);
        if (!string.IsNullOrEmpty(dri.Directory))
        {
            graph.Assert(retention, Vocabulary.ImportLocation, new LiteralNode(dri.Directory));
        }
        if (dri.TransferringBody is not null)
        {
            var transfer = existing.GetSingleUriNode(id, Vocabulary.AssetHasTransfer) ?? CacheClient.NewId;
            graph.Assert(id, Vocabulary.AssetHasTransfer, transfer);
            var bodyName = dri.TransferringBody.LastSegment().Replace('_', ' ');
            var bodyId = await cacheClient.CacheFetchOrNew(CacheEntityKind.FormalBody, bodyName, Vocabulary.FormalBodyName, cancellationToken);
            graph.Assert(transfer, Vocabulary.TransferHasFormalBody, bodyId);
        }
        if (dri.CreationBody is not null)
        {
            var creation = existing.GetSingleUriNode(id, Vocabulary.AssetHasCreation) ?? CacheClient.NewId;
            graph.Assert(id, Vocabulary.AssetHasCreation, creation);
            var bodyName = dri.CreationBody.LastSegment().Replace('_', ' ');
            var bodyId = await cacheClient.CacheFetchOrNew(CacheEntityKind.FormalBody, bodyName, Vocabulary.FormalBodyName, cancellationToken);
            graph.Assert(creation, Vocabulary.CreationHasFormalBody, bodyId);
        }

        return graph;
    }
}
