using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Staging;

internal class MalformedRdfMissingCoverageType(ILogger logger)
{
    public Graph? GetRdf(XmlNode rdfNode, Graph rdf)
    {
        try
        {
            return LoadRdfWithMissingCoverageType(rdfNode, rdf);
        }
        catch (Exception e)
        {
            logger.UnableAddMissingTypeToXmlRdf(e);
            return null;
        }
    }

    private static Graph? LoadRdfWithMissingCoverageType(XmlNode rdfNode, Graph rdf)
    {
        var namespaceManager = new XmlNamespaceManager(rdfNode.OwnerDocument.NameTable);
        namespaceManager.AddNamespace("dcterms", IngestVocabulary.DctermsNamespace.ToString());
        var coverage = rdfNode.SelectSingleNode("descendant::dcterms:coverage", namespaceManager);
        if (coverage is not null)
        {
            var coverageTypeNode = rdfNode.OwnerDocument.CreateElement("tna:MissingType", IngestVocabulary.TnaNamespace.ToString());
            var coverageChild = coverage.FirstChild.Clone();
            coverageTypeNode.AppendChild(coverageChild);
            coverage.ReplaceChild(coverageTypeNode, coverage.FirstChild);
            new RdfXmlParser().Load(rdf, new StringReader(rdfNode.OuterXml));
            return rdf;
        }

        return null;
    }
}
