namespace Exporter;

public class RecordOutput
{
    public required string RecordId { get; set; }
    public required string IaId { get; set; }
    public required string Reference { get; set; }
    public string? Title { get; set; }
    public string? TranslatedTitle { get; set; }
    public string? PublishedTitle { get; set; }
    public string? Description { get; set; }
    public string? PublishedDescription { get; set; }
    public string? PastReference { get; set; }
    public string? Summary { get; set; }
    public string? Tag { get; set; }
    public string? Arrangement { get; set; }
    public string? PublishedArrangement { get; set; }
    public string? ConsignmentId { get; set; }
    public string? DriBatchReference { get; set; }
    public string? SourceInternalName { get; set; }
    public string? ConnectedAssetNote { get; set; }
    public string? RelationDescription { get; set; }
    public string? PhysicalDescription { get; set; }
    public string? PaperNumber { get; set; }
    public string? UsageRestrictionDescription { get; set; }
    public Uri? UkGovernmentWebArchive { get; set; }
    public string? LegalStatus { get; set; }
    public string? Language { get; set; }
    public IEnumerable<string>? CopyrightHolders { get; set; }
    public string? HeldBy { get; set; }
    public string? CreatedBy { get; set; }
    public string? GeographicalPlace { get; set; }
    public string? CoveringDateStart { get; set; }
    public string? CoveringDateEnd { get; set; }
    public string? CoveringApproximateDateStart { get; set; }
    public string? CoveringApproximateDateEnd { get; set; }
    public string? FilmProductionCompanyName { get; set; }
    public string? FilmTitle { get; set; }
    public TimeSpan? FilmDuration { get; set; }
    public string? EvidenceProvider { get; set; }
    public string? Investigation { get; set; }
    public DateOnly? InquiryHearingDate { get; set; }
    public string? InquirySessionDescription { get; set; }
    public IEnumerable<InquiryAppearance>? InquiryAppearances { get; set; }
    public string? CourtSession { get; set; }
    public DateOnly? CourtSessionDate { get; set; }
    public IEnumerable<CourtCase>? CourtCases { get; set; }
    public string? SealOwnerName { get; set; }
    public string? SealColour { get; set; }
    public string? EmailAttachmentReference { get; set; }
    public string? SealCatagory { get; set; }
    public long? ImageSequenceEnd { get; set; }
    public long? ImageSequenceStart { get; set; }
    public Dimension? DimensionMm { get; set; }
    public Dimension? ObverseDimensionMm { get; set; }
    public Dimension? ReverseDimensionMm { get; set; }
    public string? SealStartDate { get; set; }
    public string? SealEndDate { get; set; }
    public string? SealObverseStartDate { get; set; }
    public string? SealObverseEndDate { get; set; }
    public string? SealReverseStartDate { get; set; }
    public string? SealReverseEndDate { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? DateOfBirth { get; set; }
    public string? BirthAddress { get; set; }
    public string? NationalRegistrationNumber { get; set; }
    public string? SeamanServiceNumber { get; set; }
    public string? BattalionName { get; set; }
    public string? NextOfKinName { get; set; }
    public IEnumerable<string>? NextOfKinTypes { get; set; }
    public bool? IsVeteran { get; set; }
    public SensitivityReview? Sensitivity { get; set; }
    public int DigitalFileCount { get; set; }
    public IEnumerable<Change>? AuditTrail { get; set; }
    public IEnumerable<Variation>? DigitalFiles { get; set; }
    public IEnumerable<RecordRelationship>? Relationships { get; set; }

    public class InquiryAppearance
    {
        public long? Sequence { get; set; }
        public string? WitnessName { get; set; }
        public string? AppearanceDescription { get; set; }
    }

    public class CourtCase
    {
        public long? Sequence { get; set; }
        public string? Name { get; set; }
        public string? Reference { get; set; }
        public string? Summary { get; set; }
        public string? SummaryJudgment { get; set; }
        public string? SummaryReasonsForJudgment { get; set; }
        public DateOnly? HearingStartDate { get; set; }
        public DateOnly? HearingEndDate { get; set; }
    }

    public class Dimension
    {
        public long? First { get; set; }
        public long? Second { get; set; }
        public bool IsFragment { get; set; } = false;
    }

    public class SensitivityReview
    {
        public bool HasSensitiveMetadata { get; set; }
        public DateOnly? FoiAssertedDate { get; set; }
        public string? SensitiveName { get; set; }
        public string? SensitiveDescription { get; set; }
        public string? AccessConditionName { get; set; }
        public string? AccessConditionCode { get; set; }
        public DateOnly? ClosureReviewDate { get; set; }
        public DateOnly? ClosureStartDate { get; set; }
        public int? ClosurePeriod { get; set; }
        public int? ClosureEndYear { get; set; }
        public string? ClosureDescription { get; set; }
        public IEnumerable<Legislation>? FoiExemptions { get; set; }
        public long? InstrumentNumber { get; set; }
        public DateOnly? InstrumentSignedDate { get; set; }
        public DateOnly? RetentionReconsiderDate { get; set; }
        public string? GroundForRetentionCode { get; set; }
        public string? GroundForRetentionDescription { get; set; }
    }

    public class Change
    {
        public string? DescriptionBase64 { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string? OperatorName { get; set; }
    }

    public class Variation
    {
        public required string FileName { get; set; }
        public long? SortOrder { get; set; }
        public long? RedactionSequence { get; set; }
        public string? Note { get; set; }
        public string? Location { get; set; }
        public string? PhysicalConditionDescription { get; set; }
        public string? ReferenceGoogleId { get; set; }
        public string? ReferenceParentGoogleId { get; set; }
        public string? ScannerOperatorIdentifier { get; set; }
        public string? ScannerIdentifier { get; set; }
        public string? ScannerGeographicalPlace { get; set; }
        public string? ScannedImageCrop { get; set; }
        public string? ScannedImageDeskew { get; set; }
        public string? ScannedImageSplit { get; set; }
        public IEnumerable<ArchivistNote>? ArchivistNotes { get; set; }
    }

    public class ArchivistNote
    {
        public string? Note { get; set; }
        public string? Date { get; set; }
    }

    public class Legislation
    {
        public required Uri Url { get; set; }
        public string? Reference { get; set; }
    }

    public record RecordRelationship(RelationshipType Relationship, string Reference);

    public enum RelationshipType
    {
        RedactionOf,
        HasRedaction,
        SeparatedMaterial,
        RelatedMaterial
    }
}