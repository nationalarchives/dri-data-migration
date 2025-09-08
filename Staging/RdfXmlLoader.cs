using Api;
using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

public static class RdfXmlLoader
{
    public static IGraph? GetRdf(string xml, ILogger logger)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        return GetRdf(doc, logger);
    }

    public static IGraph? GetRdf(XmlDocument doc, ILogger logger)
    {
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("rdf", NamespaceMapper.RDF);
        var rdfNode = doc.DocumentElement.SelectSingleNode("descendant::rdf:RDF", namespaceManager);
        if (rdfNode is not null)
        {
            var rdf = new Graph
            {
                BaseUri = new Uri("http://example.com")
            };
            rdf.NamespaceMap.AddNamespace("tna", Vocabulary.TnaNamespace);
            try
            {
                new RdfXmlParser().Load(rdf, new StringReader(rdfNode.OuterXml));
                return rdf;
            }
            catch (RdfParseException)
            {
                return LoadRdfWithMissingCoverageType(rdfNode, rdf);
            }
            catch (Exception e)
            {
                logger.UnableLoadRdf(e.Message);
                return null;
            }
        }
        return null;
    }

    private static Graph? LoadRdfWithMissingCoverageType(XmlNode rdfNode, Graph rdf)
    {
        //Handle problem with malformed RDF: https://www.w3.org/TR/rdf-syntax-grammar/#nodeElement
        //MINT 20
        var namespaceManager = new XmlNamespaceManager(rdfNode.OwnerDocument.NameTable);
        namespaceManager.AddNamespace("dcterms", "http://purl.org/dc/terms/");
        var coverage = rdfNode.SelectSingleNode("descendant::dcterms:coverage", namespaceManager);
        if (coverage is not null)
        {
            var coverageTypeNode = rdfNode.OwnerDocument.CreateElement("tna:MissingType", Vocabulary.TnaNamespace.ToString());
            var coverageChild = coverage.FirstChild.Clone();
            coverageTypeNode.AppendChild(coverageChild);
            coverage.ReplaceChild(coverageTypeNode, coverage.FirstChild);
            new RdfXmlParser().Load(rdf, new StringReader(rdfNode.OuterXml));
            return rdf;
        }

        return null;
    }
}
