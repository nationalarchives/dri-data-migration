﻿using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF;

public static class GraphSelectionExtensions
{
    public static ILiteralNode? GetSingleLiteral(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetLiteralNodes(graph, subject, predicate).SingleOrDefault();

    public static DateTimeOffset? GetSingleDate(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsDateTimeOffset();

    public static long? GetSingleNumber(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsInteger();

    public static string? GetSingleText(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        GetSingleLiteral(graph, subject, predicate)?.Value;

    public static ILiteralNode? GetSingleTransitiveLiteral(this IGraph graph,
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

    public static IEnumerable<ILiteralNode> GetLiteralNodes(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<ILiteralNode>();

    public static IEnumerable<IUriNode> GetUriNodes(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<IUriNode>();

    public static IUriNode? GetSingleUriNode(this IGraph graph, IUriNode subject, IUriNode predicate) =>
        graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object as IUriNode;

    public static IUriNode? GetSingleUriNodeSubject(this IGraph graph, IUriNode predicate, INode obj) =>
        graph.GetTriplesWithPredicateObject(predicate, obj).SingleOrDefault()?.Subject as IUriNode;
}