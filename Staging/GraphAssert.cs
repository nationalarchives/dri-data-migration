using Api;
using Microsoft.Extensions.Logging;
using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;

namespace Staging;

internal static class GraphAssert
{
    internal static void Text(IGraph graph, INode id, Dictionary<IUriNode, string?> predicates)
    {
        foreach (var predicate in predicates)
        {
            Text(graph, id, predicate.Value, predicate.Key);
        }
    }

    internal static void Text(IGraph graph, INode id, string? value, IUriNode immediatePredicate)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            graph.Assert(id, immediatePredicate, new LiteralNode(value.ReplaceLineEndings("\n")));
        }
    }

    internal static void Text(IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Text(graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    internal static void Text(IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode)
        {
            Text(graph, id, foundNode.Value, immediatePredicate);
        }
    }

    internal static void Base64(IGraph graph, INode id, string? value, IUriNode immediatePredicate)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            graph.Assert(id, immediatePredicate, new LiteralNode(base64, new Uri(XmlSpecsHelper.XmlSchemaDataTypeBase64Binary)));
        }
    }

    internal static void Integer(IGraph graph, INode id, int? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new LongNode(value.Value));
        }
    }

    internal static void Integer(IGraph graph, INode id, long? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new LongNode(value.Value));
        }
    }

    internal static void Integer(ILogger logger, IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Integer(logger, graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    internal static void Integer(ILogger logger, IGraph graph, INode id, IGraph rdf,
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

    internal static void Date(IGraph graph, INode id, Dictionary<IUriNode, DateTimeOffset?> predicates)
    {
        foreach (var predicate in predicates)
        {
            Date(graph, id, predicate.Value, predicate.Key);
        }
    }

    internal static void Date(IGraph graph, INode id, DateTimeOffset? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new DateNode(value.Value));
        }
    }

    internal static void Date(DateParser dateParser, IGraph graph, INode id, IGraph rdf,
        Dictionary<IUriNode, IUriNode> predicates)
    {
        foreach (var predicate in predicates)
        {
            Date(dateParser, graph, id, rdf, predicate.Key, predicate.Value);
        }
    }

    internal static void Date(DateParser dateParser, IGraph graph,
        INode id, IGraph rdf, IUriNode findPredicate, IUriNode immediatePredicate)
    {
        var found = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (found is ILiteralNode foundNode && dateParser.TryParseDate(foundNode.Value, out var dt))
        {
            graph.Assert(id, immediatePredicate, new DateNode(new DateTimeOffset((int)dt!.Year!, (int)dt!.Month!, (int)dt!.Day!, 0, 0, 0, TimeSpan.Zero)));
        }
    }

    internal static void DateTime(IGraph graph, INode id, DateTimeOffset? value, IUriNode immediatePredicate)
    {
        if (value.HasValue)
        {
            graph.Assert(id, immediatePredicate, new DateTimeNode(value.Value));
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

    internal static async Task<IUriNode?> ExistingOrNewWithRelationshipAsync(ICacheClient cacheClient,
        IGraph graph, INode id, IGraph rdf,
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
        var nodeId = await cacheClient.CacheFetchOrNew(cacheEntityKind, name, foundPredicate, cancellationToken);
        graph.Assert(id, immediatePredicate, nodeId);

        return nodeId;
    }

    internal static async Task ExistingOrNewWithRelationshipAsync(ICacheClient cacheClient,
        IGraph graph, INode id, IGraph rdf,
        IUriNode findPredicate, CacheEntityKind cacheEntityKind,
        IUriNode immediatePredicate, IUriNode foundPredicate,
        string[] splitOn,
        CancellationToken cancellationToken)
    {
        var node = rdf.GetTriplesWithPredicate(findPredicate).SingleOrDefault()?.Object;
        if (node is null)
        {
            return;
        }
        var name = node switch
        {
            ILiteralNode literalNode => literalNode.Value,
            IUriNode uriNode => uriNode.Uri.Segments.Last().Replace('_', ' '),
            _ => null
        };
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }
        foreach (var item in name.Split(splitOn, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var nodeId = await cacheClient.CacheFetchOrNew(cacheEntityKind, item, foundPredicate, cancellationToken);
            graph.Assert(id, immediatePredicate, nodeId);
        }
    }
}
