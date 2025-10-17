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
            Repair(doc, clonedChild, blankNode, false);
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

    private static void Repair(XmlDocument doc, XmlElement node, XmlAttribute blankNode, bool isParentBlankNode)
    {
        bool markedBlankNode = false;
        var isBlankNode = node.ChildNodes.OfType<XmlElement>().Count() > 1 &&
            (node.ParentNode is null || node.ParentNode.OfType<XmlElement>().Count() > 1);
        var isPartOfBlankNode = node.ParentNode is not null && node.ParentNode.OfType<XmlElement>().Count() > 1 &&
            node.ChildNodes.OfType<XmlElement>().Any();
        var isEmbeddedBlankNode = isParentBlankNode && node.ChildNodes.OfType<XmlElement>().Any();
        if (isBlankNode || isPartOfBlankNode || isEmbeddedBlankNode)
        {
            markedBlankNode = true;
            node.Attributes.Append((XmlAttribute)blankNode.Clone());
        }
        var allowedAttributes = new List<XmlAttribute>();
        if (node.Attributes?.Count > 0)
        {
            foreach (var attr in node.Attributes.OfType<XmlAttribute>())
            {
                if (attr.Name == "type")
                {
                    XmlAttribute typedAttr;
                    if (markedBlankNode)
                    {
                        typedAttr = doc.CreateAttribute("rdf:ID", NamespaceMapper.RDF);
                    }
                    else
                    {
                        typedAttr = doc.CreateAttribute("rdf:datatype", NamespaceMapper.RDF);
                    }
                    typedAttr.Value = string.IsNullOrWhiteSpace(attr.Value) ?
                        "Undefined" : attr.Value.Trim().Replace(" & ", "_").Replace(" and ", "_")
                        .Replace("; ", "_").Replace(", ", "_").Replace(' ', '-')
                        .Replace('(', '-').Replace(')', '-').Replace('\'', '-');
                    allowedAttributes.Add(typedAttr);
                }
                else
                {
                    if (!attr.Name.StartsWith("xsi:"))
                    {
                        allowedAttributes.Add(attr);
                    }
                }
            }
            node.RemoveAllAttributes();
            foreach (var attr in allowedAttributes)
            {
                node.Attributes.Append(attr);
            }
        }
        foreach (var childNode in node.ChildNodes.OfType<XmlElement>())
        {
            Repair(doc, childNode, blankNode, markedBlankNode);
        }
    }
}
