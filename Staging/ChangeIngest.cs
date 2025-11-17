using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Staging;

public class ChangeIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<ChangeIngest> logger) :
    StagingIngest<DriChange>(sparqlClient, logger, "ChangeGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriChange dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.ChangeDriId, driId) ?? CacheClient.NewId;
        var graph = new Graph();
        graph.Assert(id, Vocabulary.ChangeDriId, driId);
        GraphAssert.Base64(graph, id, dri.Diff, Vocabulary.ChangeDescription);
        graph.Assert(id, Vocabulary.ChangeDateTime, new DateTimeNode(dri.Timestamp));
        if (dri.Table == "DeliverableUnit")
        {
            var asset = await cacheClient.CacheFetch(CacheEntityKind.Asset, dri.Reference, cancellationToken);
            if (asset is null)
            {
                logger.AssetNotFound(dri.Reference);
            }
            else
            {
                graph.Assert(id, Vocabulary.ChangeHasAsset, asset);
            }
        }
        if (dri.Table == "DigitalFile")
        {
            var variation = await cacheClient.CacheFetch(CacheEntityKind.Variation, dri.Reference, cancellationToken);
            if (variation is null)
            {
                logger.VariationNotFound(dri.Reference);
            }
            else
            {
                graph.Assert(id, Vocabulary.ChangeHasVariation, variation);
            }
        }
        var person = await cacheClient.CacheFetchOrNew(CacheEntityKind.Operator, dri.UserName, Vocabulary.OperatorIdentifier, cancellationToken);
        graph.Assert(id, Vocabulary.ChangeHasOperator, person);
        if (dri.FullName.Contains(' '))
        {
            GraphAssert.Text(graph, person, dri.FullName, Vocabulary.OperatorName);
        }
        else
        {
            GraphAssert.Text(graph, person, existing, Vocabulary.OperatorName, Vocabulary.OperatorName);
        }

        return graph;
    }
}
