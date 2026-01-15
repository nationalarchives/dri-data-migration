using Api;
using Rdf;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static class RecordMapper
{
    public static RecordOutput Map(IGraph asset, List<IUriNode> variations,
        long? redactedVariationSequence)
    {
        var assetDriId = asset.GetSingleText(Vocabulary.AssetDriId)!;
        var assetReference = asset.GetSingleText(Vocabulary.AssetReference)!;
        var assetName = asset.GetSingleText(Vocabulary.AssetName);
        var assetAlternativeName = asset.GetSingleText(Vocabulary.AssetAlternativeName);
        var assetPastReference = asset.GetSingleText(Vocabulary.AssetPastReference);
        var assetDescription = asset.GetSingleText(Vocabulary.AssetDescription);
        var assetSummary = asset.GetSingleText(Vocabulary.AssetSummary);
        var assetTag = asset.GetSingleText(Vocabulary.AssetTag);
        var consignmentTdrId = asset.GetSingleText(Vocabulary.ConsignmentTdrId);
        var batchDriId = asset.GetSingleText(Vocabulary.BatchDriId);
        var assetSourceInternalName = asset.GetSingleText(Vocabulary.AssetSourceInternalName);
        var assetConnectedAssetNote = asset.GetSingleText(Vocabulary.AssetConnectedAssetNote);
        var assetModifiedAt = asset.GetSingleDate(Vocabulary.AssetModifiedAt);
        var assetAlternativeModifiedAt = asset.GetSingleDate(Vocabulary.AssetAlternativeModifiedAt);
        var assetHasAlternativeModifiedDateStart = YmdMapper.GetYmd(asset, Vocabulary.AssetHasAlternativeModifiedDateStart);
        var assetHasAlternativeModifiedDateEnd = YmdMapper.GetYmd(asset, Vocabulary.AssetHasAlternativeModifiedDateEnd);
        var assetAlternativeModifiedAtNote = asset.GetSingleText(Vocabulary.AssetAlternativeModifiedAtNote);
        var assetPhysicalDescription = asset.GetSingleText(Vocabulary.AssetPhysicalDescription);
        var paperNumber = asset.GetSingleText(Vocabulary.PaperNumber);
        var poorLawUnionNumber = asset.GetSingleText(Vocabulary.PoorLawUnionNumber);
        var assetUsageRestrictionDescription = asset.GetSingleText(Vocabulary.AssetUsageRestrictionDescription);
        var assetHasUkGovernmentWebArchive = asset.GetSingleUriNode(Vocabulary.AssetHasUkGovernmentWebArchive)?.Uri;
        var legalStatus = asset.GetSingleUriNode(Vocabulary.AssetHasLegalStatus)?.Uri;
        var filmProductionCompanyName = asset.GetSingleText(Vocabulary.FilmProductionCompanyName);
        var filmTitle = asset.GetSingleText(Vocabulary.FilmTitle);
        var filmDuration = asset.GetSingleLiteral(Vocabulary.FilmDuration)?.AsValuedNode().AsTimeSpan();
        var evidenceProviderName = asset.GetSingleText(Vocabulary.EvidenceProviderName);
        var investigationName = asset.GetSingleText(Vocabulary.InvestigationName);
        var inquiryHearingDate = asset.GetSingleDate(Vocabulary.InquiryHearingDate);
        var inquirySessionDescription = asset.GetSingleText(Vocabulary.InquirySessionDescription);
        var courtSessionDescription = asset.GetSingleText(Vocabulary.CourtSessionDescription);
        var courtSessionDate = asset.GetSingleDate(Vocabulary.CourtSessionDate);
        var sealOwnerName = asset.GetSingleText(Vocabulary.SealOwnerName);
        var sealColour = asset.GetSingleText(Vocabulary.SealColour);
        var emailAttachmentReference = asset.GetSingleText(Vocabulary.EmailAttachmentReference);
        var imageSequenceStart = asset.GetSingleNumber(Vocabulary.ImageSequenceStart);
        var imageSequenceEnd = asset.GetSingleNumber(Vocabulary.ImageSequenceEnd);
        var languageName = asset.GetSingleTransitiveLiteral(Vocabulary.AssetHasLanguage, Vocabulary.LanguageName)?.Value;
        var sealCategoryName = asset.GetSingleTransitiveLiteral(Vocabulary.SealAssetHasSealCategory, Vocabulary.SealCategoryName)?.Value;
        var geographicalPlaceName = asset.GetSingleTransitiveLiteral(Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName)?.Value;
        var assetHasOriginDateStart = YmdMapper.GetYmd(asset, Vocabulary.AssetHasOriginDateStart);
        var assetHasOriginDateEnd = YmdMapper.GetYmd(asset, Vocabulary.AssetHasOriginDateEnd);
        var assetHasOriginApproximateDateStart = YmdMapper.GetYmd(asset, Vocabulary.AssetHasOriginApproximateDateStart);
        var assetHasOriginApproximateDateEnd = YmdMapper.GetYmd(asset, Vocabulary.AssetHasOriginApproximateDateEnd);
        var sealAssetHasStartDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasStartDate);
        var sealAssetHasEndDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasEndDate);
        var sealAssetHasObverseStartDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasObverseStartDate);
        var sealAssetHasObverseEndDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasObverseEndDate);
        var sealAssetHasReverseStartDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasReverseStartDate);
        var sealAssetHasReverseEndDate = YmdMapper.GetYmd(asset, Vocabulary.SealAssetHasReverseEndDate);
        var assetHasDimension = DimensionMapper.GetDimension(asset, Vocabulary.AssetHasDimension);
        var sealAssetHasObverseDimension = DimensionMapper.GetDimension(asset, Vocabulary.SealAssetHasObverseDimension);
        var sealAssetHasReverseDimension = DimensionMapper.GetDimension(asset, Vocabulary.SealAssetHasReverseDimension);
        var inquiryAppearances = InquiryAppearanceMapper.GetInquiryAppearances(asset);
        var courtCases = CourtCaseMapper.GetCourtCases(asset);
        var retentionFormalBodyName = FormalBodyNameMapper.GetBodyName(asset, Vocabulary.AssetHasRetention, Vocabulary.RetentionHasFormalBody);
        var creationFormalBodyName = FormalBodyNameMapper.GetBodyName(asset, Vocabulary.AssetHasCreation, Vocabulary.CreationHasFormalBody);
        var changes = ChangeMapper.GetAllChanges(asset, variations);
        var files = VariationMapper.GetVariations(asset, variations);
        var location = LocationMapper.GetLocation(asset);
        var hasSameLocation = location.Original.Equals(location.SensitiveName);
        var sr = SensitivityReviewMapper.Get(asset, variations, hasSameLocation);
        var copyrightTitles = CopyrightMapper.GetCopyrights(asset);
        var relationships = RelationMapper.GetRelations(asset, assetReference, redactedVariationSequence);
        var recordId = GetRecordId(asset, variations)!;
        var person = PersonMapper.GetIndividual(asset);

        return new()
        {
            RecordId = recordId,
            IaId = BuildIaId(redactedVariationSequence, assetDriId),
            Reference = ReferenceBuilder.Build(redactedVariationSequence, assetReference),
            Title = assetName,
            TranslatedTitle = assetAlternativeName,
            PublishedTitle = sr.SensitiveName ?? assetName,
            Description = assetDescription,
            PublishedDescription = sr.SensitiveDescription ?? assetDescription,
            PastReference = assetPastReference,
            Summary = assetSummary,
            Tag = assetTag,
            Arrangement = string.IsNullOrWhiteSpace(location.Original) ? null : location.Original,
            PublishedArrangement = string.IsNullOrWhiteSpace(location.SensitiveName) ? null : location.SensitiveName,
            ConsignmentId = consignmentTdrId,
            DriBatchReference = batchDriId,
            SourceInternalName = assetSourceInternalName,
            ConnectedAssetNote = assetConnectedAssetNote,
            PhysicalDescription = assetPhysicalDescription,
            PaperNumber = paperNumber,
            PoorLawUnionNumber = poorLawUnionNumber,
            UsageRestrictionDescription = assetUsageRestrictionDescription,
            UkGovernmentWebArchive = assetHasUkGovernmentWebArchive,
            LegalStatus = legalStatus?.Segments.LastOrDefault(),
            Language = languageName,
            CopyrightHolders = copyrightTitles,
            HeldBy = retentionFormalBodyName,
            CreatedBy = creationFormalBodyName,
            DateLastModified = assetModifiedAt,
            CuratedModifiedAt = assetAlternativeModifiedAt,
            CuratedDateStart = assetHasAlternativeModifiedDateStart,
            CuratedDateEnd = assetHasAlternativeModifiedDateEnd,
            CuratedModifiedAtNote = assetAlternativeModifiedAtNote,
            GeographicalPlace = geographicalPlaceName,
            CoveringDateStart = assetHasOriginDateStart ?? assetModifiedAt?.ToString("yyyy-MM-dd"),
            CoveringDateEnd = assetHasOriginDateEnd ?? assetModifiedAt?.ToString("yyyy-MM-dd"),
            CoveringApproximateDateStart = assetHasOriginApproximateDateStart,
            CoveringApproximateDateEnd = assetHasOriginApproximateDateEnd,
            ProvidedCoveringDateStart = assetHasOriginDateStart,
            ProvidedCoveringDateEnd = assetHasOriginDateEnd,
            FilmProductionCompanyName = filmProductionCompanyName,
            FilmTitle = filmTitle,
            FilmDuration = filmDuration,
            EvidenceProvider = evidenceProviderName,
            Investigation = investigationName,
            InquiryHearingDate = ToDate(inquiryHearingDate),
            InquirySessionDescription = inquirySessionDescription,
            InquiryAppearances = inquiryAppearances,
            CourtSession = courtSessionDescription,
            CourtSessionDate = ToDate(courtSessionDate),
            CourtCases = courtCases,
            SealOwnerName = sealOwnerName,
            SealColour = sealColour,
            EmailAttachmentReference = emailAttachmentReference,
            SealCategory = sealCategoryName,
            ImageSequenceEnd = imageSequenceStart,
            ImageSequenceStart = imageSequenceEnd,
            DimensionMm = assetHasDimension,
            ObverseDimensionMm = sealAssetHasObverseDimension,
            ReverseDimensionMm = sealAssetHasReverseDimension,
            SealStartDate = sealAssetHasStartDate,
            SealEndDate = sealAssetHasEndDate,
            SealObverseStartDate = sealAssetHasObverseStartDate,
            SealObverseEndDate = sealAssetHasObverseEndDate,
            SealReverseStartDate = sealAssetHasReverseStartDate,
            SealReverseEndDate = sealAssetHasReverseEndDate,
            Address = person?.Address,
            BattalionName = person?.BattalionName,
            BirthAddress = person?.BirthAddress,
            DateOfBirth = person?.DateOfBirth,
            FamilyName = person?.FamilyName,
            FullName = person?.FullName,
            GivenName = person?.GivenName,
            IsVeteran = person?.IsVeteran,
            NationalRegistrationNumber = person?.NationalRegistrationNumber,
            NextOfKinName = person?.NextOfKinName,
            NextOfKinTypes = person?.NextOfKinTypes,
            SeamanServiceNumber = person?.SeamanServiceNumber,
            Sensitivity = sr,
            AuditTrail = changes,
            DigitalFileCount = variations.Count,
            DigitalFiles = files,
            Relationships = relationships
        };
    }

    private static string? GetRecordId(IGraph graph, List<IUriNode> variations) =>
        variations.Select(v => graph.GetSingleText(v, Vocabulary.VariationDriManifestationId)).FirstOrDefault();

    private static string BuildIaId(long? redactedSequence, string assetDriId) =>
        redactedSequence is null ? Guid.Parse(assetDriId).ToString("N") : $"{Guid.Parse(assetDriId):N}_{redactedSequence}";

    internal static DateOnly? ToDate(DateTimeOffset? dt) => dt is null ? null : DateOnly.FromDateTime(dt.Value.Date);
}
