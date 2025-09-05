using System.Text.Json.Serialization;

namespace Explorer.Models;

public class FlatJsonAsset
{
    public string Id { get; set; }
    public string Reference { get; set; }
    public string? Name { get; set; }
    public string? PastReference { get; set; }
    public string? Description { get; set; }
    public string? Summary { get; set; }
    public string? Tag { get; set; }
    public string? ConsignmentId { get; set; }
    public string? BatchId { get; set; }
    public string? SourceInternalName { get; set; }
    public string? RelationDescription { get; set; }
    public string? PhysicalDescription { get; set; }
    public string? PaperNumber { get; set; }
    public string? UsageRestrictionDescription { get; set; }
    public Uri? UkGovernmentWebArchive { get; set; }
    public string? LegalStatus { get; set; }
    public string? Language { get; set; }
    public IEnumerable<string>? Copyrights { get; set; }
    public string? RetentionImportLocation { get; set; }
    public string? RetentionRetentionBody { get; set; }
    public string? CreatedBy { get; set; }
    public string? GeographicalPlace { get; set; }
    public string? OriginDateStart { get; set; }
    public string? OriginDateEnd { get; set; }
    public string? OriginApproximateDateStart { get; set; }
    public string? OriginApproximateDateEnd { get; set; }
    public string? FilmProductionCompanyName { get; set; }
    public string? FilmTitle { get; set; }
    public TimeSpan? FilmDuration { get; set; }
    public string? EvidenceProvider { get; set; }
    public string? Investigation { get; set; }
    public DateTimeOffset? InquiryHearingDate { get; set; }
    public string? InquirySessionDescription { get; set; }
    public IEnumerable<InquiryAppearanceRecord>? InquiryAppearances { get; set; }
    public string? CourtSession { get; set; }
    public DateTimeOffset? CourtSessionDate { get; set; }
    public IEnumerable<CourtCaseRecord>? CourtCases { get; set; }
    public string? SealOwnerName { get; set; }
    public string? SealColour { get; set; }
    public string? SealCatagory { get; set; }
    public long? ImageSequenceEnd { get; set; }
    public long? ImageSequenceStart { get; set; }
    public DimensionRecord? DimensionMm { get; set; }
    public DimensionRecord? ObverseDimensionMm { get; set; }
    public DimensionRecord? ReverseDimensionMm { get; set; }
    public string? SealStartDate { get; set; }
    public string? SealEndDate { get; set; }
    public string? SealObverseStartDate { get; set; }
    public string? SealObverseEndDate { get; set; }
    public string? SealReverseStartDate { get; set; }
    public string? SealReverseEndDate { get; set; }
    public string? SensitiveName { get; set; }
    public string? SensitiveDescription { get; set; }
    public IEnumerable<VariationRecord>? Variations { get; set; }

    public static IEnumerable<FlatJsonAsset> FromAsset(Asset asset)
    {
        var redactedIds = asset.Variations.SelectMany(v => v.Redacted.Select(r => r.Id.Single()));

        var template = new FlatJsonAsset()
        {
            Id = asset.Id.Single(),
            Reference = asset.Reference.Single(),
            Name = asset.Name.SingleOrDefault(),
            PastReference = asset.PastReference.SingleOrDefault(),
            Description = asset.Description.SingleOrDefault(),
            Summary = asset.Summary.SingleOrDefault(),
            Tag = asset.Tag.SingleOrDefault(),
            ConsignmentId = asset.ConsignmentId.SingleOrDefault(),
            BatchId = asset.BatchId.SingleOrDefault(),
            SourceInternalName = asset.SourceInternalName.SingleOrDefault(),
            RelationDescription = asset.RelationDescription.SingleOrDefault(),
            PhysicalDescription = asset.PhysicalDescription.SingleOrDefault(),
            PaperNumber = asset.PaperNumber.SingleOrDefault(),
            UsageRestrictionDescription = asset.UsageRestrictionDescription.SingleOrDefault(),
            UkGovernmentWebArchive = asset.UkGovernmentWebArchive.SingleOrDefault()?.Uri,
            LegalStatus = asset.LegalStatus.SingleOrDefault()?.Uri.Segments.Last(),
            Language = asset.Language.SingleOrDefault()?.Name.SingleOrDefault(),
            Copyrights = asset.Copyrights.Select(c => c.Title.SingleOrDefault()),
            RetentionImportLocation = asset.Retention.SingleOrDefault()?.ImportLocation.SingleOrDefault(),
            RetentionRetentionBody = asset.Retention.SingleOrDefault()?.RetentionBody.SingleOrDefault()?.Name.SingleOrDefault(),
            CreatedBy = asset.Creation.SingleOrDefault()?.CreationBody.SingleOrDefault()?.Name.SingleOrDefault(),
            GeographicalPlace = asset.GeographicalPlace.SingleOrDefault()?.Name.SingleOrDefault(),
            OriginDateStart = asset.OriginDateStart.SingleOrDefault()?.ToDate(),
            OriginDateEnd = asset.OriginDateEnd.SingleOrDefault()?.ToDate(),
            OriginApproximateDateStart = asset.OriginApproximateDateStart.SingleOrDefault()?.ToDate(),
            OriginApproximateDateEnd = asset.OriginApproximateDateEnd.SingleOrDefault()?.ToDate(),
            FilmProductionCompanyName = asset.FilmProductionCompanyName.SingleOrDefault(),
            FilmTitle = asset.FilmTitle.SingleOrDefault(),
            FilmDuration = asset.FilmDuration.SingleOrDefault(),
            EvidenceProvider = asset.EvidenceProvider.SingleOrDefault(),
            Investigation = asset.Investigation.SingleOrDefault(),
            InquiryHearingDate = asset.InquiryHearingDate.SingleOrDefault(),
            InquirySessionDescription = asset.InquirySessionDescription.SingleOrDefault(),
            InquiryAppearances = InquiryAppearanceRecord.FromInquiryAppearances(asset.InquiryAppearances),
            CourtSession = asset.CourtSession.SingleOrDefault(),
            CourtSessionDate = asset.CourtSessionDate.SingleOrDefault(),
            CourtCases = CourtCaseRecord.FromCourtCases(asset.CourtCases),
            SealOwnerName = asset.SealOwnerName.SingleOrDefault(),
            SealColour = asset.SealColour.SingleOrDefault(),
            SealCatagory = asset.SealCatagory.SingleOrDefault()?.Name.SingleOrDefault(),
            ImageSequenceStart = asset.ImageSequenceStart.SingleOrDefault(),
            ImageSequenceEnd = asset.ImageSequenceEnd.SingleOrDefault(),
            DimensionMm = DimensionRecord.FromDimmension(asset.Dimension.SingleOrDefault()),
            ObverseDimensionMm = DimensionRecord.FromDimmension(asset.ObverseDimension.SingleOrDefault()),
            ReverseDimensionMm = DimensionRecord.FromDimmension(asset.ReverseDimension.SingleOrDefault()),
            SealStartDate = asset.SealStartDate.SingleOrDefault()?.ToDate(),
            SealEndDate = asset.SealEndDate.SingleOrDefault()?.ToDate(),
            SealObverseStartDate = asset.SealObverseStartDate.SingleOrDefault()?.ToDate(),
            SealObverseEndDate = asset.SealObverseEndDate.SingleOrDefault()?.ToDate(),
            SealReverseStartDate = asset.SealReverseStartDate.SingleOrDefault()?.ToDate(),
            SealReverseEndDate = asset.SealReverseEndDate.SingleOrDefault()?.ToDate(),
            SensitiveName = asset.SensitivityReviews.SingleOrDefault()?.SensitiveName.SingleOrDefault(),
            SensitiveDescription = asset.SensitivityReviews.SingleOrDefault()?.SensitiveDescription.SingleOrDefault()
        };

        var assets = new List<FlatJsonAsset>();

        var unredactedVariations = asset.Variations.Where(v => !redactedIds.Contains(v.Id.Single()))
            .Select(VariationRecord.FromVariation);
        var unredatcedAsset = template.DeepCopy();
        unredatcedAsset.Variations = unredactedVariations;
        assets.Add(unredatcedAsset);

        foreach (var redactedVariation in asset.Variations.Where(v => redactedIds.Contains(v.Id.Single())))
        {
            var redactedAsset = template.DeepCopy();
            redactedAsset.Variations = [VariationRecord.FromVariation(redactedVariation)];
            assets.Add(redactedAsset);
        }

        return assets;
    }

    private FlatJsonAsset DeepCopy()
    {
        var deep = (FlatJsonAsset)MemberwiseClone();
        deep.InquiryAppearances = InquiryAppearances?.Select(i => i.DeepCopy());
        deep.CourtCases = CourtCases?.Select(i => i.DeepCopy());
        deep.DimensionMm = DimensionMm?.DeepCopy();
        deep.ObverseDimensionMm = ObverseDimensionMm?.DeepCopy();
        deep.ReverseDimensionMm = ReverseDimensionMm?.DeepCopy();
        deep.Variations = Variations?.Select(v => v.DeepCopy());

        return deep;
    }

    public record InquiryAppearanceRecord(string? WitnessName, string? AppearanceDescription)
    {
        public static IEnumerable<InquiryAppearanceRecord> FromInquiryAppearances(ICollection<InquiryAppearance> inquiryAppearances) =>
            inquiryAppearances.Select(i => new InquiryAppearanceRecord(i.WitnessName.SingleOrDefault(), i.AppearanceDescription.SingleOrDefault()));

        internal InquiryAppearanceRecord DeepCopy() => (InquiryAppearanceRecord)MemberwiseClone();
    }

    public record CourtCaseRecord(string? Name, string? Reference, string? SummaryJudgment,
        string? SummaryReasonsForJudgment, DateTimeOffset? HearingStartDate, DateTimeOffset? HearingEndDate)
    {
        public static IEnumerable<CourtCaseRecord> FromCourtCases(ICollection<CourtCase> courtCases) =>
            courtCases.Select(c => new CourtCaseRecord(c.Name.SingleOrDefault(), c.Reference.SingleOrDefault(),
                c.SummaryJudgment.SingleOrDefault(), c.SummaryReasonsForJudgment.SingleOrDefault(),
                c.HearingStartDate.SingleOrDefault(), c.HearingEndDate.SingleOrDefault()));

        internal CourtCaseRecord DeepCopy() => (CourtCaseRecord)MemberwiseClone();
    }

    public record DimensionRecord(long? First, long? Second)
    {
        public static DimensionRecord? FromDimmension(Dimension? dimension) => dimension is null ? null :
            new DimensionRecord(dimension.FirstDimensionMillimetre.SingleOrDefault(),
                dimension.SecondDimensionMillimetre.SingleOrDefault());

        internal DimensionRecord DeepCopy() => (DimensionRecord)MemberwiseClone();
    }

    public record VariationRecord(string Id, string Name, string? PastName, string? Note,
        string? Location, string? PhysicalConditionDescription, string? ReferenceGoogleId,
        string? ReferenceParentGoogleId, string? ScannerOperatorIdentifier,
        string? ScannerIdentifier, string? ArchivistNote, string? DatedNote,
        string? ScannerGeographicalPlace, string? ScannedImageCrop,
        string? ScannedImageDeskew, string? ScannedImageSplit, Sr? SensitiveReview)
    {
        public static VariationRecord FromVariation(Variation variation) =>
            new(variation.Id.Single(), variation.Name.Single(),
                variation.PastName.SingleOrDefault(), variation.Note.SingleOrDefault(),
                variation.Location.SingleOrDefault()?.Value,
                variation.PhysicalConditionDescription.SingleOrDefault(),
                variation.ReferenceGoogleId.SingleOrDefault(),
                variation.ReferenceParentGoogleId.SingleOrDefault(),
                variation.ScannerOperatorIdentifier.SingleOrDefault(),
                variation.ScannerIdentifier.SingleOrDefault(),
                variation.DatedNote.SingleOrDefault()?.ArchivistNote.SingleOrDefault(),
                variation.DatedNote.SingleOrDefault()?.Date.SingleOrDefault()?.ToDate(),
                variation.ScannerGeographicalPlace.SingleOrDefault()?.Name.SingleOrDefault(),
                variation.ScannedVariationHasImageCrop.SingleOrDefault()?.Uri.Segments.Last(),
                variation.ScannedVariationHasImageDeskew.SingleOrDefault()?.Uri.Segments.Last(),
                variation.ScannedVariationHasImageSplit.SingleOrDefault()?.Uri.Segments.Last(),
                Sr.FromSensitiveReview(variation.SensitivityReviews.SingleOrDefault()));

        internal VariationRecord DeepCopy() => (VariationRecord)MemberwiseClone() with
        {
            SensitiveReview = SensitiveReview?.DeepCopy()
        };
    };

    public record Sr(string DriId, DateTimeOffset? Date, string? SensitiveName,
        string? SensitiveDescription, string? AccessConditionName,
        string? AccessConditionCode, DateTimeOffset? ReviewDate,
        DateTimeOffset? CalculationStartDate, TimeSpan? Duration,
        long? EndYear, string? Description, IEnumerable<LegislationRecord>? Legislations,
        long? InstrumentNumber, DateTimeOffset? InstrumentSignedDate,
        DateTimeOffset? RetentionRestrictionReviewDate, string? GroundForRetentionCode,
        string? GroundForRetentionDescription)
    {
        public static Sr? FromSensitiveReview(SensitivityReview? sr) => sr is null ? null :
            new(sr.DriId.Single(), sr.Date.SingleOrDefault(), sr.SensitiveName.SingleOrDefault(),
                sr.SensitiveDescription.SingleOrDefault(),
                sr.AccessCondition.SingleOrDefault()?.Name.SingleOrDefault(),
                sr.AccessCondition.SingleOrDefault()?.Code.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.ReviewDate.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.CalculationStartDate.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.Duration.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.EndYear.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.Description.SingleOrDefault(),
                LegislationRecord.FromLegislation(sr.Restriction.SingleOrDefault()?.UkLegislations),
                sr.Restriction.SingleOrDefault()?.RetentionRestriction.SingleOrDefault()?.InstrumentNumber.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.RetentionRestriction.SingleOrDefault()?.InstrumentSignedDate.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.RetentionRestriction.SingleOrDefault()?.ReviewDate.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.RetentionRestriction.SingleOrDefault()?.GroundForRetention.SingleOrDefault()?.Code.SingleOrDefault(),
                sr.Restriction.SingleOrDefault()?.RetentionRestriction.SingleOrDefault()?.GroundForRetention.SingleOrDefault()?.Description.SingleOrDefault());

        internal Sr DeepCopy() => (Sr)MemberwiseClone() with
        {
            Legislations = Legislations?.Select(l => l.DeepCopy())
        };
    }

    public record LegislationRecord(Uri Url, string? Reference)
    {
        public static IEnumerable<LegislationRecord>? FromLegislation(ICollection<UkLegislation>? legislations) =>
            legislations?.Select(l => new LegislationRecord(l.Legislation.Single().Uri, l.Reference.SingleOrDefault()));

        internal LegislationRecord DeepCopy() => (LegislationRecord)MemberwiseClone();
    }
}