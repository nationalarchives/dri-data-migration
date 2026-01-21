using Api;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class RelationMapper
{
    internal static List<RecordOutput.RecordRelationship>? GetRelations(IGraph graph,
        string assetReference, long? redactedPresentationSequence, bool? isRedacted)
    {
        var relationships = new List<RecordOutput.RecordRelationship>();
        var descriptions = graph.GetTriplesWithPredicate(Vocabulary.AssetRelationDescription).ToList();
        var related = graph.GetTriplesWithPredicate(Vocabulary.AssetRelationIdentifier).Select(t =>
            new KeyValuePair<INode, string?>(t.Subject, graph.GetSingleText(t.Subject, Vocabulary.AssetRelationReference) ?? (t.Object as ILiteralNode)?.Value));
        foreach (var relation in related)
        {
            var description = descriptions.WithSubject(relation.Key).SingleOrDefault();
            relationships.Add(new RecordOutput.RecordRelationship(RecordOutput.RelationshipType.RelatedMaterial, relation.Value!, (description?.Object as ILiteralNode)?.Value));
            if (description is not null)
            {
                descriptions.Remove(description);
            }
        }
        foreach (var relation in descriptions)
        {
            relationships.Add(new RecordOutput.RecordRelationship(RecordOutput.RelationshipType.RelatedMaterial, null, (relation.Object as ILiteralNode)?.Value));
        }

        var separated = (graph.GetLiteralNodes(Vocabulary.AssetConnectedAssetNote).Select(l => l.Value)).ToList();
        var wo409Separated = Wo409Separated(ReferenceBuilder.Build(null, assetReference));
        if (wo409Separated is not null)
        {
            separated.Add(wo409Separated);
        }
        var variations = graph.GetUriNodes(Vocabulary.AssetHasVariation);
        List<string> variationRedactions = [];
        List<string> variationPresentation = [];
        if (redactedPresentationSequence is not null)
        {
            if (isRedacted == true)
            {
                variationRedactions.Add(ReferenceBuilder.Build(null, assetReference));
            }
            else
            {
                variationPresentation.Add(ReferenceBuilder.Build(null, assetReference));
            }
        }
        else
        {
            foreach (var variation in variations)
            {
                var redactedVariationSequence = graph.GetSingleLiteral(variation, Vocabulary.RedactedVariationSequence)?.AsValuedNode().AsInteger();
                var presentationVariationSequence = graph.GetSingleLiteral(variation, Vocabulary.PresentationVariationSequence)?.AsValuedNode().AsInteger();
                if (redactedVariationSequence is not null)
                {
                    variationRedactions.Add(ReferenceBuilder.Build(redactedVariationSequence.Value, assetReference));
                }
                if (presentationVariationSequence is not null)
                {
                    variationPresentation.Add(ReferenceBuilder.Build(presentationVariationSequence.Value, assetReference));
                }
            }
        }

        if (relationships.Count == 0 && separated.Count == 0 &&
            variationRedactions.Count == 0 && variationPresentation.Count == 0)
        {
            return null;
        }

        relationships.AddRange(AssignRelationship(separated, RecordOutput.RelationshipType.SeparatedMaterial));
        if (redactedPresentationSequence is null)
        {
            relationships.AddRange(AssignRelationship(variationRedactions, RecordOutput.RelationshipType.HasRedaction));
            relationships.AddRange(AssignRelationship(variationPresentation, RecordOutput.RelationshipType.HasReplacement));
        }
        else
        {
            relationships.AddRange(AssignRelationship(variationRedactions, RecordOutput.RelationshipType.RedactionOf));
            relationships.AddRange(AssignRelationship(variationPresentation, RecordOutput.RelationshipType.ReplacementOf));
        }

        return relationships;
    }

    private static IEnumerable<RecordOutput.RecordRelationship> AssignRelationship(IEnumerable<string>? references,
        RecordOutput.RelationshipType relationshipType) => references is null ? [] :
        references.Select(r => new RecordOutput.RecordRelationship(relationshipType, r));

    private static string? Wo409Separated(string assetReference) => assetReference switch
    {
        "WO 409/27/101/668" => "WO 409/27/101/1071",
        "WO 409/27/102/20" => "WO 409/27/102/1059",
        "WO 409/27/14/345" => "WO 409/27/14/537",
        "WO 409/27/30/300" => "WO 409/27/30/1058",
        "WO 409/27/4/46" => "WO 409/27/4/678",
        "WO 409/27/51/301" => "WO 409/27/51/738",
        "WO 409/27/70/26" => "WO 409/27/70/1074",
        "WO 409/27/93/12" => "WO 409/27/93/662",
        "WO 409/27/93/169" => "WO 409/27/93/663",
        "WO 409/27/93/278" => "WO 409/27/93/664",
        "WO 409/27/93/319" => "WO 409/27/93/665",

        "WO 409/27/101/1071" => "WO 409/27/101/668",
        "WO 409/27/102/1059" => "WO 409/27/102/20",
        "WO 409/27/14/537" => "WO 409/27/14/345",
        "WO 409/27/30/1058" => "WO 409/27/30/300",
        "WO 409/27/4/678" => "WO 409/27/4/46",
        "WO 409/27/51/738" => "WO 409/27/51/301",
        "WO 409/27/70/1074" => "WO 409/27/70/26",
        "WO 409/27/93/662" => "WO 409/27/93/12",
        "WO 409/27/93/663" => "WO 409/27/93/169",
        "WO 409/27/93/664" => "WO 409/27/93/278",
        "WO 409/27/93/665" => "WO 409/27/93/319",
        _ => null
    };
}