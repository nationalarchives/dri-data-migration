using Api;
using Microsoft.Extensions.Logging;
using System.Globalization;
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
            if (TryParseDate(foundNode.Value, out var dt))
            {
                graph.Assert(id, immediatePredicate, new DateNode(dt));
            }
            else
            {
                logger.UnrecognizedDateFormat(foundNode.Value);
            }
        }
    }

    public bool YearMonthDay(IGraph graph, IUriNode predicate, INode id, INode dateId, string date)
    {
        graph.Assert(id, predicate, dateId);
        if (TryParseDate(date, out var dt))
        {
            graph.Assert(dateId, Vocabulary.Year, new LiteralNode(dt.Year.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
            graph.Assert(dateId, Vocabulary.Month, new LiteralNode($"--{dt.Month}", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gMonth")));
            graph.Assert(dateId, Vocabulary.Day, new LiteralNode($"---{dt.Day}", new Uri($"{XmlSpecsHelper.NamespaceXmlSchema}gDay")));

            return true;
        }

        if (int.TryParse(date, out var year))
        {
            graph.Assert(dateId, Vocabulary.Year, new LiteralNode(year.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeYear)));
            return true;
        }

        if (date.StartsWith('[') && date.IndexOf(']') == date.Length - 1)
        {
            return YearMonthDay(graph, predicate, id, dateId, date.Remove(date.Length - 1, 1).Remove(0, 1));
        }

        logger.UnrecognizedYearMonthDayFormat(date);
        return false;
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

    public static bool TryParseDate(string date, out DateTimeOffset dt)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            dt = default;
            return false;
        }
        date = date.Replace(" Sept ", " Sep ");
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
        if (DateTimeOffset.TryParseExact(date, "\\[yyyy MMM d\\]", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt3))
        {
            dt = dt3;
            return true;
        }
        if (DateTimeOffset.TryParseExact(date, "\\[yyyy MMMM d\\]", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt4))
        {
            dt = dt4;
            return true;
        }

        dt = default;
        return false;
    }
}
