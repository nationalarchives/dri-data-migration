using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

internal class RdfXmlLoader(ILogger logger)
{
    private readonly MissingRdfOldNamespace missingRdfOldNamespace = new(logger);

    internal IGraph? GetRdf(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        return GetRdf(doc);
    }

    internal IGraph? GetRdf(XmlDocument doc)
    {
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("rdf", NamespaceMapper.RDF);
        var rdfNode = doc.DocumentElement?.SelectSingleNode("descendant::rdf:RDF", namespaceManager) as XmlElement;
        rdfNode ??= missingRdfOldNamespace.GetRdfNode(doc);
        if (rdfNode is not null)
        {
            return ParseRdf(doc, rdfNode, false);
        }
        return null;
    }

    private Graph? ParseRdf(XmlDocument doc, XmlElement rdfNode, bool requiresRepairing)
    {
        var graph = new Graph
        {
            BaseUri = new Uri("http://example.com")
        };
        graph.NamespaceMap.AddNamespace("tna", IngestVocabulary.TnaNamespace);
        try
        {
            var rdfXml = (requiresRepairing?
                MalformedRdfRepair.GetRepairedRdf(doc):rdfNode)
                ?.OuterXml.Replace("rdf:datetype", "rdf:datatype");
            if (string.IsNullOrWhiteSpace(rdfXml))
            {
                return null;
            }
            new RdfXmlParser().Load(graph, new StringReader(rdfXml));
            return graph;
        }
        catch (RdfParseException e)
        {
            logger.MalformedRdf(e);
            if (requiresRepairing)
            {
                return null;
            }
            return ParseRdf(doc, rdfNode, true);
        }
        catch (Exception e)
        {
            logger.UnableLoadRdf(e.Message);
            return null;
        }
    }
}
