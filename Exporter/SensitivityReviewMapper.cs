using Api;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class SensitivityReviewMapper
{
    internal static RecordOutput.SensitivityReview Get(IGraph asset,
        List<IUriNode> variations, bool hasSameLocation)
    {
        var sr = new RecordOutput.SensitivityReview
        {
            SensitiveName = asset.GetSingleTransitiveLiteral(Vocabulary.AssetHasSensitivityReview,
                Vocabulary.SensitivityReviewSensitiveName)?.Value,
            SensitiveDescription = asset.GetSingleTransitiveLiteral(Vocabulary.AssetHasSensitivityReview,
                Vocabulary.SensitivityReviewSensitiveDescription)?.Value
        };
        var srSubject = FindCurrentSensitivityReview(asset, variations);
        if (srSubject is not null)
        {
            Populate(sr, asset, srSubject);
        }
        sr.HasSensitiveMetadata = !hasSameLocation || sr.SensitiveName is not null ||
            sr.SensitiveDescription is not null;
        return sr;
    }

    internal static void Populate(RecordOutput.SensitivityReview sr, IGraph asset, IUriNode srSubject)
    {
        sr.SensitiveName ??= asset.GetSingleText(srSubject, Vocabulary.SensitivityReviewSensitiveName);
        sr.SensitiveDescription ??= asset.GetSingleText(srSubject, Vocabulary.SensitivityReviewSensitiveDescription);
        sr.AccessConditionCode = asset.GetSingleTransitiveLiteral(srSubject, Vocabulary.SensitivityReviewHasAccessCondition,
            Vocabulary.AccessConditionCode)?.Value;
        sr.AccessConditionName = asset.GetSingleTransitiveLiteral(srSubject, Vocabulary.SensitivityReviewHasAccessCondition,
            Vocabulary.AccessConditionName)?.Value;
        sr.FoiAssertedDate = RecordMapper.ToDate(
            asset.GetSingleDate(srSubject, Vocabulary.SensitivityReviewDate));
        var restriction = asset.GetSingleUriNode(srSubject, Vocabulary.SensitivityReviewHasSensitivityReviewRestriction);
        if (restriction is not null)
        {
            sr.ClosureReviewDate = RecordMapper.ToDate(
                asset.GetSingleDate(restriction, Vocabulary.SensitivityReviewRestrictionReviewDate));
            sr.ClosureStartDate = RecordMapper.ToDate(
                asset.GetSingleDate(restriction, Vocabulary.SensitivityReviewRestrictionCalculationStartDate));
            sr.ClosurePeriod = (int?)asset.GetSingleLiteral(restriction, Vocabulary.SensitivityReviewRestrictionDuration)
                ?.AsValuedNode().AsTimeSpan().TotalDays / 365;
            var year = asset.GetSingleText(restriction, Vocabulary.SensitivityReviewRestrictionEndYear);
            if (year is not null && int.TryParse(year, out var endYear))
            {
                sr.ClosureEndYear = endYear;
            }
            sr.ClosureDescription = asset.GetSingleText(restriction, Vocabulary.SensitivityReviewRestrictionDescription);

            var legislations = new List<RecordOutput.Legislation>();
            foreach (var legislation in asset.GetUriNodes(restriction, Vocabulary.SensitivityReviewRestrictionHasLegislation))
            {
                var legislationHasUkLegislation = asset.GetSingleUriNode(legislation, Vocabulary.LegislationHasUkLegislation)?.Uri;
                var legislationSectionReference = asset.GetSingleText(legislation, Vocabulary.LegislationSectionReference);
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

            var retentionRestriction = asset.GetSingleUriNode(restriction, Vocabulary.SensitivityReviewRestrictionHasRetentionRestriction);
            if (retentionRestriction is not null)
            {
                sr.InstrumentNumber = asset.GetSingleNumber(retentionRestriction, Vocabulary.RetentionInstrumentNumber);
                sr.InstrumentSignedDate = RecordMapper.ToDate(
                    asset.GetSingleDate(retentionRestriction, Vocabulary.RetentionInstrumentSignatureDate));
                sr.RetentionReconsiderDate = RecordMapper.ToDate(
                    asset.GetSingleDate(retentionRestriction, Vocabulary.RetentionRestrictionReviewDate));
                sr.GroundForRetentionCode = asset.GetSingleTransitiveLiteral(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention,
                    Vocabulary.GroundForRetentionCode)?.Value;
                sr.GroundForRetentionDescription = asset.GetSingleTransitiveLiteral(retentionRestriction, Vocabulary.RetentionRestrictionHasGroundForRetention,
                    Vocabulary.GroundForRetentionDescription)?.Value;
            }
        }
    }

    internal static IUriNode? FindCurrentSensitivityReview(IGraph asset, List<IUriNode> variations)
    {
        foreach (var variation in variations)
        {
            var srSubject = asset.GetUriNodes(variation, Vocabulary.VariationHasSensitivityReview)
                ?.SingleOrDefault(s => !asset.GetTriplesWithPredicateObject(Vocabulary.SensitivityReviewHasPastSensitivityReview, s).Any());
            if (srSubject is not null)
            {
                return srSubject;
            }
        }

        return null;
    }
}