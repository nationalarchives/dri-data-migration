using Api;
using VDS.RDF;

namespace Exporter;

internal static class FormalBodyNameMapper
{
    internal static string? GetBodyName(IGraph graph, IUriNode subject,
        IUriNode relationshipPredicate, IUriNode formalBodyPredicate)
    {
        var relationship = graph.GetSingleUriNode(subject, relationshipPredicate);
        if (relationship is null)
        {
            return null;
        }
        else
        {
            var hasFormalBody = graph.GetSingleUriNode(relationship, formalBodyPredicate);
            if (hasFormalBody is null)
            {
                return null;
            }
            else
            {
                return graph.GetSingleText(hasFormalBody, Vocabulary.FormalBodyName);
            }
        }
    }
}