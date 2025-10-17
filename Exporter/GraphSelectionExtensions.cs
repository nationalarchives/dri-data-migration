using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF;

internal static class GraphSelectionExtensions
{
    private static INode? GetSingle(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object;

    internal static ILiteralNode? GetSingleLiteral(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingle(graph, subject, predicate) as ILiteralNode;

    internal static DateTimeOffset? GetSingleDate(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsDateTimeOffset();

    internal static long? GetSingleNumber(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsInteger();

    internal static string? GetSingleText(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.Value;

    internal static ILiteralNode? GetSingleTransitiveLiteral(this IGraph graph,
        IUriNode subject, IUriNode relationshipPredicate,
        IUriNode immediatePredicate)
    {
        var parent = GetSingleUriNode(graph, subject, relationshipPredicate);
        if (parent is null)
        {
            return null;
        }

        return GetSingleLiteral(graph, parent, immediatePredicate);
    }

    internal static IEnumerable<IUriNode> GetUriNodes(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<IUriNode>();

    internal static IUriNode? GetSingleUriNode(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object as IUriNode;
}