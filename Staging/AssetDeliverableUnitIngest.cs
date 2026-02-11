using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging;

public class AssetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<AssetDeliverableUnitIngest> logger, IMeterFactory meterFactory,
    IAssetDeliverableUnitRelation assetDeliverableUnitRelation) :
    StagingIngest<DriAssetDeliverableUnit>(sparqlClient, logger, meterFactory, "AssetDeliverableUnitGraph")
{
    private readonly AssetDeliverableUnitXmlIngest xmlIngest = new(logger, cacheClient, assetDeliverableUnitRelation);

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriAssetDeliverableUnit dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.AssetDriId, driId);
        if (id is null)
        {
            logger.AssetNotFound(dri.Id);
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AssetDriId, driId);
        AddAssetType(graph, id, dri.AssetType);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.AssetDriXml);
            await xmlIngest.ExtractXmlData(graph, existing, id, dri.Xml, dri.FilesJson, cancellationToken);
        }

        return graph;
    }

    private void AddAssetType(IGraph graph, IUriNode id, string securityTag)
    {
        var assetType = securityTag switch
        {
            "BornDigital" or "open" => Vocabulary.BornDigitalAsset,
            "DigitisedRecords" => Vocabulary.DigitisedAsset,
            "Surrogate" => Vocabulary.SurrogateAsset,
            _ => null
        };

        if (assetType is null)
        {
            logger.AssetTagTypeNotResolved(securityTag);
        }
        else
        {
            graph.Assert(id, Vocabulary.AssetHasAssetTagType, assetType);
        }
    }
}
