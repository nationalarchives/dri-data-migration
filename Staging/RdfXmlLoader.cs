using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

internal class RdfXmlLoader(ILogger logger)
{
    private readonly MissingRdfOldNamespace missingRdfOldNamespace = new(logger);
    private readonly MalformedRdfMissingCoverageType malformedRdfMissingCoverageType = new(logger);

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
        var rdfNode = doc.DocumentElement?.SelectSingleNode("descendant::rdf:RDF", namespaceManager);
        if (rdfNode is null)
        {
            rdfNode = missingRdfOldNamespace.GetRdfNode(doc);
        }
        if (rdfNode is not null)
        {
            var rdf = new Graph
            {
                BaseUri = new Uri("http://example.com")
            };
            rdf.NamespaceMap.AddNamespace("tna", IngestVocabulary.TnaNamespace);
            var rdfXml = rdfNode.OuterXml.Replace("rdf:datetype", "rdf:datatype");
            try
            {
                new RdfXmlParser().Load(rdf, new StringReader(rdfXml));
                return rdf;
            }
            catch (RdfParseException e)
            {
                logger.MalformedRdf(e);
                return malformedRdfMissingCoverageType.GetRdf(rdfNode, rdf);
            }
            catch (Exception e)
            {
                logger.UnableLoadRdf(e.Message);
                return null;
            }
        }
        return null;
    }
}
