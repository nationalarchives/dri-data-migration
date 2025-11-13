using Microsoft.Extensions.Logging;
using System.Xml;
using VDS.RDF;

namespace Staging;

internal class MissingRdfOldNamespace(ILogger logger)
{
    internal XmlElement? GetRdfNode(XmlDocument doc)
    {
        try
        {
            return AddMissingRdfNode(doc);
        }
        catch (Exception e)
        {
            logger.UnableAddRdfToXml(e);
            return null;
        }
    }

    private static XmlElement? AddMissingRdfNode(XmlDocument doc)
    {
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("rdf", NamespaceMapper.RDF);
        namespaceManager.AddNamespace("t", "http://www.tessella.com/XIP/v4");
        namespaceManager.AddNamespace("tnaxm", IngestVocabulary.TnaNamespaceWithSlash.ToString());
        var metadata = doc.DocumentElement?.SelectSingleNode("descendant::t:Metadata", namespaceManager) ??
            doc.DocumentElement?.SelectSingleNode("descendant::tnaxm:metadata", namespaceManager)?.ParentNode;
        if (metadata is null)
        {
            return null;
        }
        var metadataChild = metadata.FirstChild;
        if (metadataChild is null)
        {
            return null;
        }
        var tna = metadataChild.Attributes?.GetNamedItem("xmlns:tnaxm") as XmlAttribute;
        if (tna is not null)
        {
            tna.Value = IngestVocabulary.TnaNamespace.ToString();
        }
        var missingRdf = doc.CreateElement("rdf:RDF", NamespaceMapper.RDF);
        var description = doc.CreateElement("rdf:Description", NamespaceMapper.RDF);
        var about = doc.CreateAttribute("rdf:about", NamespaceMapper.RDF);
        about.Value = "http://example.com/subject";
        description.Attributes.Append(about);
        var blankNode = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
        blankNode.Value = "Resource";
        foreach (var child in metadataChild.ChildNodes.OfType<XmlElement>())
        {
            if (child.LocalName == "provenance")
            {
                continue;
            }
            var clonedChild = (XmlElement)child.CloneNode(true);
            MalformedRdfRepair.Repair(doc, clonedChild, blankNode, false);
            description.AppendChild(clonedChild);
        }
        missingRdf.AppendChild(description);
        foreach (var child in metadataChild.ChildNodes.OfType<XmlElement>())
        {
            metadataChild.RemoveChild(child);
        }
        metadataChild.PrependChild(missingRdf);

        return missingRdf;
    }

}
