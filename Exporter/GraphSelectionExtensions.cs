using VDS.RDF.Nodes;

namespace VDS.RDF;

internal static class GraphSelectionExtensions
{
    internal static ILiteralNode? GetSingleLiteral(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetLiteralNodes(graph, subject, predicate).SingleOrDefault();

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

    internal static IEnumerable<ILiteralNode> GetLiteralNodes(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<ILiteralNode>();

    internal static IEnumerable<IUriNode> GetUriNodes(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<IUriNode>();

    internal static IUriNode? GetSingleUriNode(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object as IUriNode;
}