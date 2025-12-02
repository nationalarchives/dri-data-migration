using Api;
using VDS.RDF;

namespace Exporter;

internal static class ChangeMapper
{
    internal static List<RecordOutput.Change>? GetAllChanges(IGraph graph, IUriNode subject,
        List<IUriNode> variations)
    {
        var changeSubjects = graph.GetUriNodes(subject, Vocabulary.AssetHasChange).ToList();
        foreach (var sr in graph.GetUriNodes(subject, Vocabulary.AssetHasSensitivityReview))
        {
            var srChanges = graph.GetUriNodes(sr, Vocabulary.SensitivityReviewHasChange).ToList();
            changeSubjects.AddRange(srChanges);
        }
        foreach (var variation in variations)
        {
            var variationChanges = graph.GetUriNodes(variation, Vocabulary.VariationHasChange).ToList();
            changeSubjects.AddRange(variationChanges);
            foreach (var sr in graph.GetUriNodes(variation, Vocabulary.VariationHasSensitivityReview))
            {
                var srChanges = graph.GetUriNodes(sr, Vocabulary.SensitivityReviewHasChange).ToList();
                changeSubjects.AddRange(srChanges);
            }
        }
        if (changeSubjects.Count == 0)
        {
            return null;
        }

        var changes = new List<RecordOutput.Change>();
        foreach (var change in changeSubjects)
        {
            var changeDescription = graph.GetSingleText(change, Vocabulary.ChangeDescription);
            var changeDateTime = graph.GetSingleDate(change, Vocabulary.ChangeDateTime);
            var operatorName = graph.GetSingleTransitiveLiteral(change, Vocabulary.ChangeHasOperator, Vocabulary.OperatorName)?.Value;

            changes.Add(new()
            {
                DescriptionBase64 = changeDescription,
                Timestamp = changeDateTime,
                OperatorName = operatorName,
            });
        }

        return changes;
    }
}