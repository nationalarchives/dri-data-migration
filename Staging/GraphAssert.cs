using Api;
using Microsoft.Extensions.Logging;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

public class GraphAssert(ILogger logger, ICacheClient cacheClient)
{
    public static void Text(IGraph graph, INode id, Dictionary<IUriNode, string?> predicates)
    {
        foreach (var predicate in predicates)
        {
            Text(graph, id, predicate.Value, predicate.Key);
        }
    }

    public static void Text(IGraph graph, INode id, string? value, IUriNode immediatePredicate)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            graph.Assert(id, immediatePredicate, new LiteralNode(value));
        }
    }

    public static void Text(IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Text(graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    public static void Text(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode)
        {
            Text(graph, id, foundNode.Value, immediatePredicate);
        }
    }

    public static void MultiText(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        foreach (var found in rdf.GetTriplesWithPredicate(findPredicate).Select(t => t.Object).Cast<ILiteralNode>())
        {
            Text(graph, id, found.Value, immediatePredicate);
        }
    }

    public static void Base64(IGraph graph, INode id, string? value, IUriNode immediatePredicate)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            graph.Assert(id, immediatePredicate, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        }
    }

    public static void Integer(IGraph graph, INode id, int? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new LongNode(value.Value));
        }
    }

    public void Integer(IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Integer(graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    public void Integer(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && !string.IsNullOrWhiteSpace(foundNode.Value))
        {
            if (int.TryParse(foundNode.Value, out var value))
            {
                graph.Assert(id, immediatePredicate, new LongNode(value));
            }
            else
            {
                logger.InvalidIntegerValue(foundNode.Value);
            }
        }
    }

    public static void Date(IGraph graph, INode id, Dictionary<IUriNode, DateTimeOffset?> predicates)
    {
        foreach (var predicate in predicates)
        {
            Date(graph, id, predicate.Value, predicate.Key);
        }
    }

    public static void Date(IGraph graph, INode id, DateTimeOffset? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new DateNode(value.Value));
        }
    }

    public void Date(IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Date(graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    public void Date(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && !string.IsNullOrWhiteSpace(foundNode.Value))
        {
            if (DateParser.TryParseDate(foundNode.Value, out var dt))
            {
                graph.Assert(id, immediatePredicate, new DateNode(dt));
            }
            else
            {
                logger.UnrecognizedDateFormat(foundNode.Value);
            }
        }
    }

    internal static void YearMonthDay(IGraph graph, INode id, int? year, int? month, int? day)
    {
        if (year is not null)
        {
            graph.Assert(id, Vocabulary.Year, new LiteralNode(year.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
            if (month is not null)
            {
                graph.Assert(id, Vocabulary.Month, new LiteralNode($"--{month.Value.ToString().PadLeft(2, '0')}", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gMonth")));
            }
            if (day is not null)
            {
                graph.Assert(id, Vocabulary.Day, new LiteralNode($"---{day.Value.ToString().PadLeft(2, '0')}", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gDay")));
            }
        }
    }

    public async Task<IUriNode?> ExistingOrNewWithRelationshipAsync(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, CacheEntityKind cacheEntityKind,
        IUriNode immediatePredicate, IUriNode foundPredicate,
        CancellationToken cancellationToken)
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
}
