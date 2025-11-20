using System.Xml;
using VDS.RDF;

namespace Staging;

internal static class MalformedRdfRepair
{
    internal static XmlElement? GetRepairedRdf(XmlDocument doc)
    {
        var namespaceManager = new XmlNamespaceManager(doc.NameTable);
        namespaceManager.AddNamespace("rdf", NamespaceMapper.RDF);
        var rdf = doc.DocumentElement?.SelectSingleNode("descendant::rdf:RDF", namespaceManager) as XmlElement;
        if (rdf is null)
        {
            return null;
        }
        var description = rdf.FirstChild as XmlElement;
        if (description is null)
        {
            return null;
        }
        var blankNode = doc.CreateAttribute("rdf:parseType", NamespaceMapper.RDF);
        blankNode.Value = "Resource";
        var rapairedChildren = new List<XmlElement>();
        var oldChildren = new List<XmlElement>();
        foreach (var child in description.ChildNodes.OfType<XmlElement>())
        {
            oldChildren.Add(child);
            var clonedChild = (XmlElement)child.CloneNode(true);
            Repair(doc, clonedChild, blankNode, false);
            rapairedChildren.Add(clonedChild);
        }
        foreach (var child in oldChildren)
        {
            description.RemoveChild(child);
        }
        foreach (var child in rapairedChildren)
        {
            description.AppendChild(child);
        }
        return rdf;
    }

    internal static void Repair(XmlDocument doc, XmlElement node, XmlAttribute blankNode, bool isParentBlankNode)
    {
        bool markedBlankNode = IsBlankNode(node, isParentBlankNode);
        if (markedBlankNode)
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
                    var typedAttrName = "rdf:datatype";
                    if (node.ChildNodes.OfType<XmlElement>().Any() && !markedBlankNode)
                    {
                        typedAttrName = "rdf:ID";
                        markedBlankNode = true;
                        allowedAttributes.Add((XmlAttribute)blankNode.Clone());
                    }
                    var typedAttr = doc.CreateAttribute(typedAttrName, NamespaceMapper.RDF);
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

    private static bool IsBlankNode(XmlElement node, bool isParentBlankNode)
    {
        var isBlankNode = node.ChildNodes.OfType<XmlElement>().Count() > 1 &&
            (node.ParentNode is null || node.ParentNode.OfType<XmlElement>().Count() > 1);
        var isPartOfBlankNode = node.ParentNode is not null && node.ParentNode.OfType<XmlElement>().Count() > 1 &&
            node.ChildNodes.OfType<XmlElement>().Any() && node.ChildNodes.OfType<XmlElement>().All(e => e.FirstChild?.NodeType == XmlNodeType.Text);
        var isEmbeddedBlankNode = isParentBlankNode && node.ChildNodes.OfType<XmlElement>().Any();

        return isBlankNode || isPartOfBlankNode || isEmbeddedBlankNode;
    }
}
