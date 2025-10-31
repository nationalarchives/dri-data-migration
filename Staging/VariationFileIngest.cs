using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

public class VariationFileIngest(ICacheClient cacheClient, ISparqlClient sparqlClient, ILogger<VariationFileIngest> logger) :
    StagingIngest<DriVariationFile>(sparqlClient, logger, "VariationFileGraph")
{
    private readonly VariationFileXmlIngest xmlIngest = new(logger, cacheClient);

    internal override async Task<Graph?> BuildAsync(IGraph existing, DriVariationFile dri, CancellationToken cancellationToken)
    {
        var driId = new LiteralNode(dri.Id);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.VariationDriId, driId);
        if (id is null)
        {
            logger.VariationNotFound(dri.Name); //TODO: sensitive information?
            return null;
        }

        var graph = new Graph();
        graph.Assert(id, Vocabulary.VariationDriId, driId);
        graph.Assert(id, Vocabulary.VariationRelativeLocation, new LiteralNode($"{dri.Location}/{dri.Name}", new Uri(XmlSpecsHelper.XmlSchemaDataTypeAnyUri)));
        GraphAssert.Text(graph, id, dri.ManifestationId, Vocabulary.VariationDriManifestationId);
        if (!string.IsNullOrEmpty(dri.Xml))
        {
            GraphAssert.Base64(graph, id, dri.Xml, Vocabulary.VariationDriXml);
            await xmlIngest.ExtractXmlData(graph, existing, id, dri.Xml, cancellationToken);
        }

        return graph;
    }

    internal override void PostIngest()
    {
        File.AppendAllLines("predicates-file.txt", xmlIngest.Predicates);
    }
}
