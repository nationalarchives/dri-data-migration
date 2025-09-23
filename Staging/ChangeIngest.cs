using Api;
using Microsoft.Extensions.Logging;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class ChangeIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<SubsetIngest> logger)
    : StagingIngest<DriChange>(sparqlClient, logger, cacheClient, "ChangeGraph")
{
    internal override async Task<Graph?> BuildAsync(IGraph existing, DriChange dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.ChangeDriId, driId).FirstOrDefault()?.Subject ?? CacheClient.NewId;
        var graph = new Graph();
        graph.Assert(id, Vocabulary.ChangeDriId, driId);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dri.Diff));
        graph.Assert(id, Vocabulary.ChangeDescription, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
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
        GraphAssert.Text(graph, person, dri.FullName, Vocabulary.OperatorName);

        return graph;
    }
}
