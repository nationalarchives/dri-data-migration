using Api;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public static class BaseIngest
{
    public static readonly Uri TnaNamespace = new("http://nationalarchives.gov.uk/metadata/tna#");
    private static readonly Uri idNamespace = new(Vocabulary.Namespace.AbsoluteUri);

    public static IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));

    public static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;

    public static void AssertLiteral(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode)
        {
            graph.Assert(id, immediatePredicate, new LiteralNode(foundNode.Value));
        }
    }

    public static void AssertDate(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, string format, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && !string.IsNullOrWhiteSpace(foundNode.Value))
        {
            if (DateTimeOffset.TryParseExact(foundNode.Value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            {
                graph.Assert(id, immediatePredicate, new DateNode(dt));
            }
            else
            {//TODO: handle different format
            }
        }
    }

    public static async Task<IUriNode?> AssertAsync(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, CacheEntityKind cacheEntityKind,
        IUriNode immediatePredicate, IUriNode? foundPredicate,
        ICacheClient cacheClient, CancellationToken cancellationToken,
        string? additionalCacheParameter = null)
    {
        var node = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (node is null)
        {
            return null;
        }
        var name = node switch
        {//TODO: handle default
            ILiteralNode literalNode => literalNode.Value,
            IUriNode uriNode => uriNode.Uri.Segments.Last().Replace('_', ' '),
            _ => throw new ArgumentException(node.ToString())
        };
        var parameters = additionalCacheParameter is null ? [name] : new string[] { name, additionalCacheParameter };
        var nodeId = await cacheClient.CacheFetchOrNew(cacheEntityKind, parameters, cancellationToken);
        graph.Assert(id, immediatePredicate, nodeId);
        if (foundPredicate is not null)
        {
            graph.Assert(nodeId, foundPredicate, new LiteralNode(name));
        }
        return nodeId;
    }

    public static IGraph? GetRdf(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        var rdfElement = doc.GetElementsByTagName("rdf:RDF");
        if (rdfElement.Count == 1)
        {
            var rdf = new Graph
            {
                BaseUri = new Uri("http://example.com")
            };
            rdf.NamespaceMap.AddNamespace("tna", TnaNamespace);
            new RdfXmlParser().Load(rdf, new StringReader(rdfElement[0].OuterXml));
            return rdf;
        }
        return null;
    }
}
