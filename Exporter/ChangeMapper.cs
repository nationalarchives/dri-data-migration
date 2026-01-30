using Api;
using Rdf;
using VDS.RDF;

namespace Exporter;

internal static class ChangeMapper
{
    internal static List<RecordOutput.Change>? GetAllChanges(IGraph asset, List<IUriNode> variations)
    {
        var changes = new List<RecordOutput.Change>();

        foreach (var changeSubject in asset.GetUriNodes(Vocabulary.AssetHasChange))
        {
            changes.Add(GenerateChange(asset, changeSubject));
        }

        var srSubject = SensitivityReviewMapper.FindCurrentSensitivityReview(asset, variations);
        while (srSubject is not null)
        {
            var currentSr = new RecordOutput.SensitivityReview();
            SensitivityReviewMapper.Populate(currentSr, asset, srSubject);
            srSubject = asset.GetSingleUriNode(srSubject, Vocabulary.SensitivityReviewHasPastSensitivityReview);
            if (srSubject is not null)
            {
                var pastSr = new RecordOutput.SensitivityReview();
                SensitivityReviewMapper.Populate(pastSr, asset, srSubject);
                var pastSubject = asset.GetSingleUriNode(srSubject, Vocabulary.SensitivityReviewHasChange);
                RecordOutput.Change srChange;
                if (pastSubject is not null)
                {
                    srChange = GenerateChange(asset, pastSubject);
                    srChange.Sensitivity = GenerateSrDiff(currentSr, pastSr);
                }
                else
                {
                    srChange = new()
                    {
                        Sensitivity = GenerateSrDiff(currentSr, pastSr)
                    };
                }
                changes.Add(srChange);
            }
        }

        if (changes.Count == 0)
        {
            return null;
        }

        return changes;
    }

    private static RecordOutput.Change GenerateChange(IGraph asset, IUriNode change)
    {
        var changeDescription = asset.GetSingleText(change, Vocabulary.ChangeDescription);
        var changeDateTime = asset.GetSingleDate(change, Vocabulary.ChangeDateTime);
        var operatorName = asset.GetSingleTransitiveLiteral(change, Vocabulary.ChangeHasOperator, Vocabulary.OperatorName)?.Value;

        return new()
        {
            DescriptionBase64 = changeDescription,
            Timestamp = changeDateTime,
            OperatorName = operatorName,
        };
    }

    private static RecordOutput.SensitivityReviewDiff GenerateSrDiff(RecordOutput.SensitivityReview current, RecordOutput.SensitivityReview past) =>
        new()
        {
            AccessConditionCode = GenerateDiff(current.AccessConditionCode, past.AccessConditionCode),
            AccessConditionName = GenerateDiff(current.AccessConditionName, past.AccessConditionName),
            ClosureDescription = GenerateDiff(current.ClosureDescription, past.ClosureDescription),
            ClosureEndYear = GenerateDiff(current.ClosureEndYear, past.ClosureEndYear),
            ClosurePeriod = GenerateDiff(current.ClosurePeriod, past.ClosurePeriod),
            ClosureReviewDate = GenerateDiff(current.ClosureReviewDate, past.ClosureReviewDate),
            ClosureStartDate = GenerateDiff(current.ClosureStartDate, past.ClosureStartDate),
            FoiAssertedDate = GenerateDiff(current.FoiAssertedDate, past.FoiAssertedDate),
            FoiExemptions = GenerateDiff(current.FoiExemptions, past.FoiExemptions),
            GroundForRetentionCode = GenerateDiff(current.GroundForRetentionCode, past.GroundForRetentionCode),
            GroundForRetentionDescription = GenerateDiff(current.GroundForRetentionDescription, past.GroundForRetentionDescription),
            InstrumentNumber = GenerateDiff(current.InstrumentNumber, past.InstrumentNumber),
            InstrumentSignedDate = GenerateDiff(current.InstrumentSignedDate, past.InstrumentSignedDate),
            RetentionReconsiderDate = GenerateDiff(current.RetentionReconsiderDate, past.RetentionReconsiderDate),
            SensitiveDescription = GenerateDiff(current.SensitiveDescription, past.SensitiveDescription),
            SensitiveName = GenerateDiff(current.SensitiveName, past.SensitiveName)
        };

    private static RecordOutput.Diff? GenerateDiff<T>(T? current, T? oldValue)
    {
        var none = current is null && oldValue is null;
        var onlyOne = (current is not null && oldValue is null) ||
            (oldValue is not null && current is null);
        var all = current is not null && oldValue is not null;
        var diff = new RecordOutput.Diff(oldValue, current);

        return none ? null :
            onlyOne ? diff :
            all ? HasDiff(current!, oldValue!) ? null : diff : null;
    }

    private static bool HasDiff<T>(T current, T oldValue) where T: notnull
    {
        if (current is IEnumerable<RecordOutput.Legislation> c &&
            oldValue is IEnumerable<RecordOutput.Legislation> o)
        {
            return !c.Select(l => l.Reference).Except(o.Select(l => l.Reference)).Any();
        }

        return current.Equals(oldValue);
    }
}