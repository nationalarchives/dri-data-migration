using Api;
using VDS.RDF;

namespace Exporter;

internal static class RelationMapper
{
    internal static List<RecordOutput.RecordRelationship>? GetRelations(IGraph graph, IUriNode subject,
        string assetReference, long? redactedVariationSequence)
    {
        var related = graph.GetLiteralNodes(subject, Vocabulary.AssetRelationDescription).Select(l => l.Value)
            .Union(graph.GetLiteralNodes(subject, Vocabulary.AssetRelationIdentifier).Select(l => l.Value));
        var separated = graph.GetLiteralNodes(subject, Vocabulary.AssetConnectedAssetNote).Select(l => l.Value);
        var variations = graph.GetUriNodes(subject, Vocabulary.AssetHasVariation);
        List<string> variationRedactions = [];
        if (redactedVariationSequence is not null)
        {
            variationRedactions.Add(assetReference);
        }
        else
        {
            foreach (var variation in variations)
            {
                var sequence = graph.GetSingleLiteral(variation, Vocabulary.RedactedVariationSequence);
                if (sequence is not null)
                {
                    variationRedactions.Add($"{assetReference}/{sequence.Value}");
                }
            }
        }

        if (!related.Any() && !separated.Any() && !variationRedactions.Any())
        {
            return null;
        }

        var relationships = new List<RecordOutput.RecordRelationship>();
        relationships.AddRange(AssignRelationship(related, RecordOutput.RelationshipType.RelatedMaterial));
        relationships.AddRange(AssignRelationship(separated, RecordOutput.RelationshipType.SeparatedMaterial));
        if (redactedVariationSequence is null)
        {
            relationships.AddRange(AssignRelationship(variationRedactions, RecordOutput.RelationshipType.HasRedaction));
        }
        else
        {
            relationships.AddRange(AssignRelationship(variationRedactions, RecordOutput.RelationshipType.RedactionOf));
        }

        return relationships;
    }

    private static IEnumerable<RecordOutput.RecordRelationship> AssignRelationship(IEnumerable<string>? references,
        RecordOutput.RelationshipType relationshipType) => references is null ? [] :
        references.Select(r => new RecordOutput.RecordRelationship(relationshipType, r));
}