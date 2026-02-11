using Rdf;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Rdf;

public static class GraphSelectionExtensions
{
    extension(IGraph graph)
    {
        public ILiteralNode? GetSingleLiteral(INode subject, IUriNode predicate) =>
            GetLiteralNodes(graph, subject, predicate).SingleOrDefault();

        public ILiteralNode? GetSingleLiteral(IUriNode predicate) =>
            GetLiteralNodes(graph, predicate).SingleOrDefault();

        public DateTimeOffset? GetSingleDate(IUriNode subject, IUriNode predicate) =>
            GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsDateTimeOffset();

        public DateTimeOffset? GetSingleDate(IUriNode predicate) =>
            GetSingleLiteral(graph, predicate)?.AsValuedNode().AsDateTimeOffset();

        public long? GetSingleNumber(IUriNode subject, IUriNode predicate) =>
            GetSingleLiteral(graph, subject, predicate)?.AsValuedNode().AsInteger();

        public long? GetSingleNumber(IUriNode predicate) =>
            GetSingleLiteral(graph, predicate)?.AsValuedNode().AsInteger();

        public string? GetSingleText(INode subject, IUriNode predicate) =>
            GetSingleLiteral(graph, subject, predicate)?.Value;

        public string? GetSingleText(IUriNode predicate) =>
            GetSingleLiteral(graph, predicate)?.Value;

        public ILiteralNode? GetSingleTransitiveLiteral(
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

        public ILiteralNode? GetSingleTransitiveLiteral(
            IUriNode relationshipPredicate, IUriNode immediatePredicate)
        {
            var parent = GetSingleUriNode(graph, relationshipPredicate);
            if (parent is null)
            {
                return null;
            }

            return GetSingleLiteral(graph, parent, immediatePredicate);
        }

        public IEnumerable<ILiteralNode> GetLiteralNodes(INode subject, IUriNode predicate) =>
            graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<ILiteralNode>();

        public IEnumerable<ILiteralNode> GetLiteralNodes(IUriNode predicate) =>
            graph.GetTriplesWithPredicate(predicate).Select(t => t.Object).Cast<ILiteralNode>();

        public IEnumerable<IUriNode> GetUriNodes(IUriNode subject, IUriNode predicate) =>
            graph.GetTriplesWithSubjectPredicate(subject, predicate).Select(t => t.Object).Cast<IUriNode>();

        public IEnumerable<IUriNode> GetUriNodes(IUriNode predicate) =>
            graph.GetTriplesWithPredicate(predicate).Select(t => t.Object).Cast<IUriNode>();

        public IUriNode? GetSingleUriNode(IUriNode subject, IUriNode predicate) =>
            graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object as IUriNode;

        public IUriNode? GetSingleUriNode(IUriNode predicate) =>
            graph.GetTriplesWithPredicate(predicate).SingleOrDefault()?.Object as IUriNode;

        public IUriNode? GetSingleUriNodeSubject(IUriNode predicate, INode obj) =>
            graph.GetTriplesWithPredicateObject(predicate, obj).SingleOrDefault()?.Subject as IUriNode;

        public IBlankNode? GetSingleBlankNode(INode subject, IUriNode predicate) =>
            graph.GetTriplesWithSubjectPredicate(subject, predicate).SingleOrDefault()?.Object as IBlankNode;
    }
}