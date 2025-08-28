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
    private static readonly Uri idNamespace = new("http://id.example.com/");

    public static IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));

    public static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;

    public static void AssertLiteral(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && !string.IsNullOrWhiteSpace(foundNode.Value))
        {
            graph.Assert(id, immediatePredicate, new LiteralNode(foundNode.Value));
        }
    }

    public static void AssertDate(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && !string.IsNullOrWhiteSpace(foundNode.Value))
        {
            if (TryParseDate(foundNode.Value, out var dt))
            {
                graph.Assert(id, immediatePredicate, new DateNode(dt));
            }
            else
            {
                throw new ArgumentException(foundNode.Value);
            }
        }
    }

    public static async Task<IUriNode?> AssertAsync(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, CacheEntityKind cacheEntityKind,
        IUriNode immediatePredicate, IUriNode foundPredicate,
        ICacheClient cacheClient, CancellationToken cancellationToken)
    {
        var node = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (node is null)
        {
            return null;
        }
        var name = node switch
        {
            ILiteralNode literalNode => literalNode.Value,
            IUriNode uriNode => uriNode.Uri.Segments.Last().Replace('_', ' '),
            _ => null
        };
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        var nodeId = await cacheClient.CacheFetchOrNew(cacheEntityKind, [name], foundPredicate, cancellationToken);
        graph.Assert(id, immediatePredicate, nodeId);
        
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

    public static bool TryParseDate(string date, out DateTimeOffset dt)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            dt = default;
            return false;
        }
        if (DateTimeOffset.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt1))
        {
            dt = dt1;
            return true;
        }
        if (DateTimeOffset.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt2))
        {
            dt = dt2;
            return true;
        }

        dt = default;
        return false;
    }
}
