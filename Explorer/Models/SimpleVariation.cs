namespace Explorer.Models;

public record SimpleVariation(string Catalogue, string AssetId,
    string VariationId, string? HeldBy, string Name, string? Description,
    IEnumerable<string> Copyrights, string? Creator, string? Location, string? ConsignmentId,
    string? BatchId, string? LegalStatus, string? ClosureStatus,
    IEnumerable<string> FoiExemptionCodes, long? ClosurePeriod,
    DateTimeOffset? ClosureStartDate, DateTimeOffset? ClosureReviewDate,
    DateTimeOffset? FoiExemptionAssertedDate)
{
    public static SimpleVariation FromVariation(Variation variation)
    {
        var asset = variation.Asset.Single();
        var srLast = variation.SensitivityReviews.Single(s => !variation.SensitivityReviews.Any(past => past.Past.SingleOrDefault()?.DriId == s.DriId));
        var period = srLast.Restriction.SingleOrDefault().EndYear.SingleOrDefault();
        if (period is null)
        {
            var duration = srLast.Restriction.SingleOrDefault().Duration.SingleOrDefault();
            if (duration is not null)
            {
                period = duration.Value.Days / 365;
            }
        }

        return new SimpleVariation(asset.Reference.Single(),
            asset.Id.Single(), variation.Id.Single(),
            asset.Retention.SingleOrDefault()?.RetentionBody.SingleOrDefault()?.Name.SingleOrDefault(),
            variation.Name.Single(), asset.Description.SingleOrDefault(),
            asset.Copyrights.SelectMany(c => c.Title),
            asset.Creation.SingleOrDefault()?.CreationBody.SingleOrDefault()?.Name.SingleOrDefault(),
            variation.Location.SingleOrDefault()?.Value, asset.ConsignmentId.SingleOrDefault(),
            asset.BatchId.SingleOrDefault(),
            asset.LegalStatus.SingleOrDefault()?.Uri.Segments.Last(),
            srLast.AccessCondition.SingleOrDefault()?.Name.SingleOrDefault(),
            srLast.Restriction.SingleOrDefault()?.UkLegislations.SelectMany(uk => uk.Reference),
            period, srLast.Restriction.SingleOrDefault()?.CalculationStartDate.SingleOrDefault(),
            srLast.Restriction.SingleOrDefault()?.ReviewDate.SingleOrDefault(), srLast.Date.SingleOrDefault());
    }
}
