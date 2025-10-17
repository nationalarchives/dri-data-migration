using Api;
using System.Collections.Generic;
using System.Linq;
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
            var changeDriId = graph.GetSingleText(change, Vocabulary.ChangeDriId);
            var changeDescription = graph.GetSingleText(change, Vocabulary.ChangeDescription);
            var changeDateTime = graph.GetSingleDate(change, Vocabulary.ChangeDateTime);
            var operatorName = graph.GetSingleText(change, Vocabulary.OperatorName);
            var operatorIdentifier = graph.GetSingleText(change, Vocabulary.OperatorIdentifier);

            changes.Add(new()
            {
                DriId = changeDriId!,
                Description = changeDescription,
                Timestamp = changeDateTime,
                OperatorName = operatorName,
                OperatorIdentifier = operatorIdentifier
            });
        }

        return changes;
    }
}