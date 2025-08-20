namespace Explorer.Models;

public record SimpleVariation(string Catalogue, string AssetId,
    string VariationId, string? HeldBy, string Name, string? Description,
    string? Copyright, string? Creator, string? Location, string? ConsignmentId,
    string? BatchId, string? LegalStatus, string? ClosureStatus,
    IEnumerable<string> FoiExemptionCodes, long? ClosurePeriod,
    DateTimeOffset? ClosureStartDate, DateTimeOffset? ClosureReviewDate,
    DateTimeOffset? FoiExemptionAssertedDate);
