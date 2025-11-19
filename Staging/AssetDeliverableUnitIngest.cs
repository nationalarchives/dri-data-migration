using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class AssetDeliverableUnitIngest(ICacheClient cacheClient, ISparqlClient sparqlClient,
    ILogger<AssetDeliverableUnitIngest> logger) :
    StagingIngest<DriAssetDeliverableUnit>(sparqlClient, logger, "AssetDeliverableUnitGraph")
{
    private readonly AssetDeliverableUnitXmlIngest xmlIngest = new(logger, cacheClient);

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
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.AssetDriXml);
            await xmlIngest.ExtractXmlData(graph, existing, id, dri.Xml, dri.FilesJson, cancellationToken);
        }

        return graph;
    }
}
