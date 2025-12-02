using Api;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class SensitivityReviewMapper
{
    internal static RecordOutput.SensitivityReview GetSensitivityReview(IGraph graph,
        IUriNode subject, List<IUriNode> variations, bool hasSameLocation)
    {
        var sr = new RecordOutput.SensitivityReview();
        //TODO: Needs rework if past sensitive reviews are to be included
        sr.SensitiveName = graph.GetSingleTransitiveLiteral(subject, Vocabulary.AssetHasSensitivityReview,
            Vocabulary.SensitivityReviewSensitiveName)?.Value;
        sr.SensitiveDescription = graph.GetSingleTransitiveLiteral(subject, Vocabulary.AssetHasSensitivityReview,
            Vocabulary.SensitivityReviewSensitiveDescription)?.Value;
        IUriNode? srSubject = null;
        foreach (var variation in variations)
        {
            srSubject = graph.GetSingleUriNode(variation, Vocabulary.VariationHasSensitivityReview);
            if (srSubject is not null)
            {
                break;
            }
        }
        if (srSubject is not null)
        {
            sr.SensitiveName ??= graph.GetSingleText(srSubject, Vocabulary.SensitivityReviewSensitiveName);
            sr.SensitiveDescription ??= graph.GetSingleText(srSubject, Vocabulary.SensitivityReviewSensitiveDescription);
            sr.AccessConditionCode = graph.GetSingleTransitiveLiteral(srSubject, Vocabulary.SensitivityReviewHasAccessCondition,
                Vocabulary.AccessConditionCode)?.Value;
            sr.AccessConditionName = graph.GetSingleTransitiveLiteral(srSubject, Vocabulary.SensitivityReviewHasAccessCondition,
                Vocabulary.AccessConditionName)?.Value;
            sr.FoiAssertedDate = RecordMapper.ToDate(
                graph.GetSingleDate(srSubject, Vocabulary.SensitivityReviewDate));
            var restriction = graph.GetTriplesWithSubjectPredicate(srSubject, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction)
                .SingleOrDefault()?.Object as IUriNode;
            if (restriction is not null)
            {
                sr.ClosureReviewDate = RecordMapper.ToDate(
                    graph.GetSingleDate(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate));
                sr.ClosureStartDate = RecordMapper.ToDate(
                    graph.GetSingleDate(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate));
                sr.ClosurePeriod = (int?)graph.GetSingleLiteral(restriction, Vocabulary.SensitivityReviewRestrictionDuration)
                    ?.AsValuedNode().AsTimeSpan().TotalDays / 365;
                var year = graph.GetSingleText(restriction, Vocabulary.SensitivityReviewRestrictionEndYear);
                if (year is not null && int.TryParse(year, out var endYear))
                {
                    sr.ClosureEndYear = endYear;
                }
                sr.ClosureDescription = graph.GetSingleText(restriction, Vocabulary.SensitivityReviewRestrictionDescription);

                var legislations = new List<RecordOutput.Legislation>();
                foreach (var legislation in graph.GetUriNodes(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation))
                {
                    var legislationHasUkLegislation = graph.GetSingleUriNode(legislation, Vocabulary.LegislationHasUkLegislation)?.Uri;
                    var legislationSectionReference = graph.GetSingleText(legislation, Vocabulary.LegislationSectionReference);
                    legislations.Add(new()
                    {
                        Url = legislationHasUkLegislation!,
                        Reference = legislationSectionReference
                    });
                }
                if (legislations.Count > 0)
                {
                    sr.FoiExemptions = legislations;
                }

                var retentionRestriction = graph.GetSingleUriNode(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction);
                if (retentionRestriction is not null)
                {
                    sr.InstrumentNumber = graph.GetSingleNumber(retentionRestriction, Vocabulary.RetentionInstrumentNumber);
                    sr.InstrumentSignedDate = RecordMapper.ToDate(
                        graph.GetSingleDate(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate));
                    sr.RetentionReconsiderDate = RecordMapper.ToDate(
                        graph.GetSingleDate(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate));
                    sr.GroundForRetentionCode = graph.GetSingleTransitiveLiteral(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention,
                        Vocabulary.GroundForRetentionCode)?.Value;
                    sr.GroundForRetentionDescription = graph.GetSingleTransitiveLiteral(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention,
                        Vocabulary.GroundForRetentionDescription)?.Value;
                }
            }
        }
        sr.HasSensitiveMetadata = !hasSameLocation || sr.SensitiveName is not null ||
            sr.SensitiveDescription is not null;
        return sr;
    }
}