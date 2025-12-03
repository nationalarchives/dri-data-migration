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
        var separated = (graph.GetLiteralNodes(subject, Vocabulary.AssetConnectedAssetNote).Select(l => l.Value)).ToList();
        var wo409Separated = Wo409Separated(ReferenceBuilder.Build(null, assetReference));
        if (wo409Separated is not null)
        {
            separated.Add(wo409Separated);
        }
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

    private static string? Wo409Separated(string assetReference) => assetReference switch
    {
        "WO/409/27/101/668" => "WO/409/27/101/1071",
        "WO/409/27/102/20" => "WO/409/27/102/1059",
        "WO/409/27/14/345" => "WO/409/27/14/537",
        "WO/409/27/30/300" => "WO/409/27/30/1058",
        "WO/409/27/4/46" => "WO/409/27/4/678",
        "WO/409/27/51/301" => "WO/409/27/51/738",
        "WO/409/27/70/26" => "WO/409/27/70/1074",
        "WO/409/27/93/12" => "WO/409/27/93/662",
        "WO/409/27/93/169" => "WO/409/27/93/663",
        "WO/409/27/93/278" => "WO/409/27/93/664",
        "WO/409/27/93/319" => "WO/409/27/93/665",

        "WO/409/27/101/1071" => "WO/409/27/101/668",
        "WO/409/27/102/1059" => "WO/409/27/102/20",
        "WO/409/27/14/537" => "WO/409/27/14/345",
        "WO/409/27/30/1058" => "WO/409/27/30/300",
        "WO/409/27/4/678" => "WO/409/27/4/46",
        "WO/409/27/51/738" => "WO/409/27/51/301",
        "WO/409/27/70/1074" => "WO/409/27/70/26",
        "WO/409/27/93/662" => "WO/409/27/93/12",
        "WO/409/27/93/663" => "WO/409/27/93/169",
        "WO/409/27/93/664" => "WO/409/27/93/278",
        "WO/409/27/93/665" => "WO/409/27/93/319",
        _ => null
    };
}