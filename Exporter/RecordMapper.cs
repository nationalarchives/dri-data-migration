using Api;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Exporter;

internal static partial class RecordMapper
{
    public static RecordOutput Map(IGraph graph, IUriNode asset,
        List<IUriNode> variations, long? redactedVariationSequence)
    {
        var assetDriId = graph.GetSingleText(asset, Vocabulary.AssetDriId);
        var assetReference = graph.GetSingleText(asset, Vocabulary.AssetReference);
        //TODO: Must be replaced with primary and secondary
        var assetName = graph.GetSingleText(asset, Vocabulary.AssetName);
        var assetPastReference = graph.GetSingleText(asset, Vocabulary.AssetPastReference);
        var assetDescription = graph.GetSingleText(asset, Vocabulary.AssetDescription);
        var assetSummary = graph.GetSingleText(asset, Vocabulary.AssetSummary);
        var assetTag = graph.GetSingleText(asset, Vocabulary.AssetTag);
        var consignmentTdrId = graph.GetSingleText(asset, Vocabulary.ConsignmentTdrId);
        var batchDriId = graph.GetSingleText(asset, Vocabulary.BatchDriId);
        var assetSourceInternalName = graph.GetSingleText(asset, Vocabulary.AssetSourceInternalName);
        var assetConnectedAssetNote = graph.GetSingleText(asset, Vocabulary.AssetConnectedAssetNote);
        var assetRelationDescription = graph.GetSingleText(asset, Vocabulary.AssetRelationDescription);
        var assetPhysicalDescription = graph.GetSingleText(asset, Vocabulary.AssetPhysicalDescription);
        var paperNumber = graph.GetSingleText(asset, Vocabulary.PaperNumber);
        var assetUsageRestrictionDescription = graph.GetSingleText(asset, Vocabulary.AssetUsageRestrictionDescription);
        var assetHasUkGovernmentWebArchive = graph.GetSingleUriNode(asset, Vocabulary.AssetHasUkGovernmentWebArchive)?.Uri;
        var legalStatus = graph.GetSingleUriNode(asset, Vocabulary.AssetHasLegalStatus)?.Uri;
        var filmProductionCompanyName = graph.GetSingleText(asset, Vocabulary.FilmProductionCompanyName);
        var filmTitle = graph.GetSingleText(asset, Vocabulary.FilmTitle);
        var filmDuration = graph.GetSingleLiteral(asset, Vocabulary.FilmDuration)?.AsValuedNode().AsTimeSpan();
        var evidenceProviderName = graph.GetSingleText(asset, Vocabulary.EvidenceProviderName);
        var investigationName = graph.GetSingleText(asset, Vocabulary.InvestigationName);
        var inquiryHearingDate = graph.GetSingleDate(asset, Vocabulary.InquiryHearingDate);
        var inquirySessionDescription = graph.GetSingleText(asset, Vocabulary.InquirySessionDescription);
        var courtSessionDescription = graph.GetSingleText(asset, Vocabulary.CourtSessionDescription);
        var courtSessionDate = graph.GetSingleDate(asset, Vocabulary.CourtSessionDate);
        var sealOwnerName = graph.GetSingleText(asset, Vocabulary.SealOwnerName);
        var sealColour = graph.GetSingleText(asset, Vocabulary.SealColour);
        var emailAttachmentReference = graph.GetSingleText(asset, Vocabulary.EmailAttachmentReference);
        var imageSequenceStart = graph.GetSingleNumber(asset, Vocabulary.ImageSequenceStart);
        var imageSequenceEnd = graph.GetSingleNumber(asset, Vocabulary.ImageSequenceEnd);
        var languageName = graph.GetSingleTransitiveLiteral(asset, Vocabulary.AssetHasLanguage, Vocabulary.LanguageName)?.Value;
        var sealCategoryName = graph.GetSingleTransitiveLiteral(asset, Vocabulary.SealAssetHasSealCategory, Vocabulary.SealCategoryName)?.Value;
        var geographicalPlaceName = graph.GetSingleTransitiveLiteral(asset, Vocabulary.AssetHasAssociatedGeographicalPlace, Vocabulary.GeographicalPlaceName)?.Value;
        var assetHasOriginDateStart = YmdMapper.GetYmd(graph, asset, Vocabulary.AssetHasOriginDateStart);
        var assetHasOriginDateEnd = YmdMapper.GetYmd(graph, asset, Vocabulary.AssetHasOriginDateEnd);
        var assetHasOriginApproximateDateStart = YmdMapper.GetYmd(graph, asset, Vocabulary.AssetHasOriginApproximateDateStart);
        var assetHasOriginApproximateDateEnd = YmdMapper.GetYmd(graph, asset, Vocabulary.AssetHasOriginApproximateDateEnd);
        var sealAssetHasStartDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasStartDate);
        var sealAssetHasEndDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasEndDate);
        var sealAssetHasObverseStartDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasObverseStartDate);
        var sealAssetHasObverseEndDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasObverseEndDate);
        var sealAssetHasReverseStartDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasReverseStartDate);
        var sealAssetHasReverseEndDate = YmdMapper.GetYmd(graph, asset, Vocabulary.SealAssetHasReverseEndDate);
        var assetHasDimension = DimensionMapper.GetDimension(graph, asset, Vocabulary.AssetHasDimension);
        var sealAssetHasObverseDimension = DimensionMapper.GetDimension(graph, asset, Vocabulary.SealAssetHasObverseDimension);
        var sealAssetHasReverseDimension = DimensionMapper.GetDimension(graph, asset, Vocabulary.SealAssetHasReverseDimension);
        var inquiryAppearances = InquiryAppearanceMapper.GetInquiryAppearances(graph, asset);
        var courtCases = CourtCaseMapper.GetCourtCases(graph, asset);
        var retentionFormalBodyName = FormalBodyNameMapper.GetBodyName(graph, asset, Vocabulary.AssetHasRetention, Vocabulary.RetentionHasFormalBody);
        var creationFormalBodyName = FormalBodyNameMapper.GetBodyName(graph, asset, Vocabulary.AssetHasCreation, Vocabulary.CreationHasFormalBody);
        var changes = ChangeMapper.GetAllChanges(graph, asset, variations);
        var files = VariationMapper.GetVariations(graph, variations);
        var sr = SensitivityReviewMapper.GetSensitivityReview(graph, asset, variations);
        var copyrightTitles = CopyrightMapper.GetCopyrights(graph, asset);
        var location = LocationMapper.GetLocation(graph, asset);

        return new()
        {
            RecordId = assetDriId,//TODO: Temporary, to be replaced
            IaId = BuildIaId(redactedVariationSequence, assetDriId),
            Reference = BuildReference(redactedVariationSequence, assetReference),
            Title = assetName,
            TranslatedTitle = null,
            PublishedTitle = sr.SensitiveName ?? assetName,
            Description = assetDescription,
            PublishedDescription = sr.SensitiveDescription ?? assetDescription,
            PastReference = assetPastReference,
            Summary = assetSummary,
            Tag = assetTag,
            Arrangement = location.Original,
            PublishedArrangement = location.SensitiveName,
            ConsignmentId = consignmentTdrId,
            DriBatchReference = batchDriId,
            SourceInternalName = assetSourceInternalName,
            ConnectedAssetNote = assetConnectedAssetNote,
            RelationDescription = assetRelationDescription,
            PhysicalDescription = assetPhysicalDescription,
            PaperNumber = paperNumber,
            UsageRestrictionDescription = assetUsageRestrictionDescription,
            UkGovernmentWebArchive = assetHasUkGovernmentWebArchive,
            LegalStatus = legalStatus?.Segments.LastOrDefault(),
            Language = languageName,
            CopyrightHolders = copyrightTitles,
            HeldBy = retentionFormalBodyName,
            CreatedBy = creationFormalBodyName,
            GeographicalPlace = geographicalPlaceName,
            CoveringDateStart = assetHasOriginDateStart,
            CoveringDateEnd = assetHasOriginDateEnd,
            CoveringApproximateDateStart = assetHasOriginApproximateDateStart,
            CoveringApproximateDateEnd = assetHasOriginApproximateDateEnd,
            FilmProductionCompanyName = filmProductionCompanyName,
            FilmTitle = filmTitle,
            FilmDuration = filmDuration,
            EvidenceProvider = evidenceProviderName,
            Investigation = investigationName,
            InquiryHearingDate = inquiryHearingDate,
            InquirySessionDescription = inquirySessionDescription,
            InquiryAppearances = inquiryAppearances,
            CourtSession = courtSessionDescription,
            CourtSessionDate = courtSessionDate,
            CourtCases = courtCases,
            SealOwnerName = sealOwnerName,
            SealColour = sealColour,
            EmailAttachmentReference = emailAttachmentReference,
            SealCatagory = sealCategoryName,
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
            FoiAssertedDate = sr.FoiAssertedDate,
            AccessConditionName = sr.AccessConditionName,
            AccessConditionCode = sr.AccessConditionCode,
            ClosureReviewDate = sr.ReviewDate,
            ClosureStartDate = sr.ClosureStartDate,
            ClosurePeriod = sr.ClosurePeriod,
            ClosureEndYear = sr.EndYear,
            ClosureDescription = sr.Description,
            FoiExemptions = sr.FoiExemptions,
            InstrumentNumber = sr.InstrumentNumber,
            InstrumentSignedDate = sr.InstrumentSignedDate,
            RetentionReconsiderDate = sr.RetentionReconsiderDate,
            GroundForRetentionCode = sr.GroundForRetentionCode,
            GroundForRetentionDescription = sr.GroundForRetentionDescription,
            DigitalFileCount = variations.Count,
            Changes = changes,
            DigitalFiles = files,
            Relationships = null,
        };
    }

    private static string? BuildIaId(long? redactedSequence, string assetDriId) =>
        redactedSequence is null ? Guid.Parse(assetDriId).ToString("N") : $"{Guid.Parse(assetDriId):N}_{redactedSequence}";

    private static string? BuildReference(long? redactedSequence, string assetReference) =>
        redactedSequence is null ? assetReference : $"{assetReference}/{redactedSequence}";
}
