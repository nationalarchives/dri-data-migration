using System;
using VDS.RDF;

namespace Api;

public static class Vocabulary
{
    public static readonly Uri Namespace = new("http://id.example.com/schema/");

    public static readonly IUriNode Subset = new UriNode(new(Namespace, "Subset"));
    public static readonly IUriNode SubsetName = new UriNode(new(Namespace, "subsetName"));
    public static readonly IUriNode SubsetReference = new UriNode(new(Namespace, "subsetReference"));
    public static readonly IUriNode SubsetHasBroaderSubset = new UriNode(new(Namespace, "subsetHasBroaderSubset"));
    public static readonly IUriNode SubsetHasNarrowerSubset = new UriNode(new(Namespace, "subsetHasNarrowerSubset"));
    public static readonly IUriNode SubsetHasAsset = new UriNode(new(Namespace, "subsetHasAsset"));
    public static readonly IUriNode SubsetHasSensitivityReview = new UriNode(new(Namespace, "subsetHasSensitivityReview"));
    public static readonly IUriNode SubsetHasRetention = new UriNode(new(Namespace, "subsetHasRetention"));

    public static readonly IUriNode Asset = new UriNode(new(Namespace, "Asset"));
    public static readonly IUriNode AssetDriId = new UriNode(new(Namespace, "assetDriId"));
    public static readonly IUriNode BatchDriId = new UriNode(new(Namespace, "batchDriId"));
    public static readonly IUriNode ConsignmentTdrId = new UriNode(new(Namespace, "consignmentTdrId"));
    public static readonly IUriNode AssetDriXml = new UriNode(new(Namespace, "assetDriXml"));
    public static readonly IUriNode Wo409SubsetDriXml = new UriNode(new(Namespace, "wo409SubsetDriXml"));
    public static readonly IUriNode AssetReference = new UriNode(new(Namespace, "assetReference"));
    public static readonly IUriNode AssetPastReference = new UriNode(new(Namespace, "assetPastReference"));
    public static readonly IUriNode AssetName = new UriNode(new(Namespace, "assetName"));
    public static readonly IUriNode AssetAlternativeName = new UriNode(new(Namespace, "assetAlternativeName"));
    public static readonly IUriNode AssetDescription = new UriNode(new(Namespace, "assetDescription"));
    public static readonly IUriNode AssetSummary = new UriNode(new(Namespace, "assetSummary"));
    public static readonly IUriNode AssetRelationDescription = new UriNode(new(Namespace, "assetRelationDescription"));
    public static readonly IUriNode AssetRelationIdentifier = new UriNode(new(Namespace, "assetRelationIdentifier"));
    public static readonly IUriNode AssetRelationReference = new UriNode(new(Namespace, "assetRelationReference"));
    public static readonly IUriNode AssetPhysicalDescription = new UriNode(new(Namespace, "assetPhysicalDescription"));
    public static readonly IUriNode AssetUsageRestrictionDescription = new UriNode(new(Namespace, "assetUsageRestrictionDescription"));
    public static readonly IUriNode AssetTag = new UriNode(new(Namespace, "assetTag"));
    public static readonly IUriNode AssetSourceInternalName = new UriNode(new(Namespace, "assetSourceInternalName"));
    public static readonly IUriNode AssetConnectedAssetNote = new UriNode(new(Namespace, "assetConnectedAssetNote"));
    public static readonly IUriNode AssetModifiedAt = new UriNode(new(Namespace, "assetModifiedAt"));
    public static readonly IUriNode AssetAlternativeModifiedAt = new UriNode(new(Namespace, "assetAlternativeModifiedAt"));
    public static readonly IUriNode AssetAlternativeModifiedAtNote = new UriNode(new(Namespace, "assetAlternativeModifiedAtNote"));
    public static readonly IUriNode InquiryEvidenceProviderName = new UriNode(new(Namespace, "inquiryEvidenceProviderName"));
    public static readonly IUriNode InquiryInvestigationName = new UriNode(new(Namespace, "inquiryInvestigationName"));
    public static readonly IUriNode CourtSessionDescription = new UriNode(new(Namespace, "courtSessionDescription"));
    public static readonly IUriNode CourtSessionDate = new UriNode(new(Namespace, "courtSessionDate"));
    public static readonly IUriNode InquirySessionDescription = new UriNode(new(Namespace, "inquirySessionDescription"));
    public static readonly IUriNode InquiryHearingDate = new UriNode(new(Namespace, "inquiryHearingDate"));
    public static readonly IUriNode FilmProductionCompanyName = new UriNode(new(Namespace, "filmProductionCompanyName"));
    public static readonly IUriNode FilmTitle = new UriNode(new(Namespace, "filmTitle"));
    public static readonly IUriNode FilmDuration = new UriNode(new(Namespace, "filmDuration"));
    public static readonly IUriNode PhotographerDescription = new UriNode(new(Namespace, "photographerDescription"));
    public static readonly IUriNode ImageSequenceStart = new UriNode(new(Namespace, "imageSequenceStart"));
    public static readonly IUriNode ImageSequenceEnd = new UriNode(new(Namespace, "imageSequenceEnd"));
    public static readonly IUriNode PaperNumber = new UriNode(new(Namespace, "paperNumber"));
    public static readonly IUriNode PoorLawUnionNumber = new UriNode(new(Namespace, "poorLawUnionNumber"));
    public static readonly IUriNode SealOwnerName = new UriNode(new(Namespace, "sealOwnerName"));
    public static readonly IUriNode SealColour = new UriNode(new(Namespace, "sealColour"));
    public static readonly IUriNode EmailAttachmentReference = new UriNode(new(Namespace, "emailAttachmentReference"));
    public static readonly IUriNode AssetHasSubset = new UriNode(new(Namespace, "assetHasSubset"));
    public static readonly IUriNode AssetHasVariation = new UriNode(new(Namespace, "assetHasVariation"));
    public static readonly IUriNode AssetHasRetention = new UriNode(new(Namespace, "assetHasRetention"));
    public static readonly IUriNode AssetHasSensitivityReview = new UriNode(new(Namespace, "assetHasSensitivityReview"));
    public static readonly IUriNode AssetHasLanguage = new UriNode(new(Namespace, "assetHasLanguage"));
    public static readonly IUriNode AssetHasLegalStatus = new UriNode(new(Namespace, "assetHasLegalStatus"));
    public static readonly IUriNode AssetHasCopyright = new UriNode(new(Namespace, "assetHasCopyright"));
    public static readonly IUriNode AssetHasAssociatedGeographicalPlace = new UriNode(new(Namespace, "assetHasAssociatedGeographicalPlace"));
    public static readonly IUriNode AssetHasCreation = new UriNode(new(Namespace, "assetHasCreation"));
    public static readonly IUriNode AssetHasOriginDateStart = new UriNode(new(Namespace, "assetHasOriginDateStart"));
    public static readonly IUriNode AssetHasOriginDateEnd = new UriNode(new(Namespace, "assetHasOriginDateEnd"));
    public static readonly IUriNode AssetHasOriginApproximateDateStart = new UriNode(new(Namespace, "assetHasOriginApproximateDateStart"));
    public static readonly IUriNode AssetHasOriginApproximateDateEnd = new UriNode(new(Namespace, "assetHasOriginApproximateDateEnd"));
    public static readonly IUriNode AssetHasAlternativeModifiedDateStart = new UriNode(new(Namespace, "assetHasAlternativeModifiedDateStart"));
    public static readonly IUriNode AssetHasAlternativeModifiedDateEnd = new UriNode(new(Namespace, "assetHasAlternativeModifiedDateEnd"));
    public static readonly IUriNode AssetHasDimension = new UriNode(new(Namespace, "assetHasDimension"));
    public static readonly IUriNode AssetHasChange = new UriNode(new(Namespace, "assetHasChange"));
    public static readonly IUriNode AssetHasPerson = new UriNode(new(Namespace, "assetHasPerson"));
    public static readonly IUriNode AssetHasVeteran = new UriNode(new(Namespace, "assetHasVeteran"));
    public static readonly IUriNode CourtAssetHasCourtCase = new UriNode(new(Namespace, "courtAssetHasCourtCase"));
    public static readonly IUriNode SealAssetHasSealCategory = new UriNode(new(Namespace, "sealAssetHasSealCategory"));
    public static readonly IUriNode SealAssetHasStartDate = new UriNode(new(Namespace, "sealAssetHasStartDate"));
    public static readonly IUriNode SealAssetHasEndDate = new UriNode(new(Namespace, "sealAssetHasEndDate"));
    public static readonly IUriNode SealAssetHasObverseStartDate = new UriNode(new(Namespace, "sealAssetHasObverseStartDate"));
    public static readonly IUriNode SealAssetHasObverseEndDate = new UriNode(new(Namespace, "sealAssetHasObverseEndDate"));
    public static readonly IUriNode SealAssetHasReverseStartDate = new UriNode(new(Namespace, "sealAssetHasReverseStartDate"));
    public static readonly IUriNode SealAssetHasReverseEndDate = new UriNode(new(Namespace, "sealAssetHasReverseEndDate"));
    public static readonly IUriNode SealAssetHasObverseDimension = new UriNode(new(Namespace, "sealAssetHasObverseDimension"));
    public static readonly IUriNode SealAssetHasReverseDimension = new UriNode(new(Namespace, "sealAssetHasReverseDimension"));
    public static readonly IUriNode AssetHasUkGovernmentWebArchive = new UriNode(new(Namespace, "assetHasUkGovernmentWebArchive"));
    public static readonly IUriNode InquiryAssetHasInquiryAppearance = new UriNode(new(Namespace, "inquiryAssetHasInquiryAppearance"));
    public static readonly IUriNode InquiryAssetHasEvidenceProvider = new UriNode(new(Namespace, "inquiryAssetHasEvidenceProvider"));
    public static readonly IUriNode InquiryAssetHasInquiryInvestigation = new UriNode(new(Namespace, "inquiryAssetHasInquiryInvestigation"));

    public static readonly IUriNode Variation = new UriNode(new(Namespace, "Variation"));
    public static readonly IUriNode VariationHasAsset = new UriNode(new(Namespace, "variationHasAsset"));
    public static readonly IUriNode VariationDriId = new UriNode(new(Namespace, "variationDriId"));
    public static readonly IUriNode VariationDriManifestationId = new UriNode(new(Namespace, "variationDriManifestationId"));
    public static readonly IUriNode VariationDriXml = new UriNode(new(Namespace, "variationDriXml"));
    public static readonly IUriNode VariationName = new UriNode(new(Namespace, "variationName"));
    public static readonly IUriNode VariationAlternativeName = new UriNode(new(Namespace, "variationAlternativeName"));
    public static readonly IUriNode VariationNote = new UriNode(new(Namespace, "variationNote"));
    public static readonly IUriNode VariationRelativeLocation = new UriNode(new(Namespace, "variationRelativeLocation"));
    public static readonly IUriNode VariationPhysicalConditionDescription = new UriNode(new(Namespace, "variationPhysicalConditionDescription"));
    public static readonly IUriNode VariationReferenceGoogleId = new UriNode(new(Namespace, "variationReferenceGoogleId"));
    public static readonly IUriNode VariationReferenceParentGoogleId = new UriNode(new(Namespace, "variationReferenceParentGoogleId"));
    public static readonly IUriNode VariationSequence = new UriNode(new(Namespace, "variationSequence"));
    public static readonly IUriNode RedactedVariationSequence = new UriNode(new(Namespace, "redactedVariationSequence"));
    public static readonly IUriNode PresentationVariationSequence = new UriNode(new(Namespace, "presentationVariationSequence"));
    public static readonly IUriNode ScannerOperatorIdentifier = new UriNode(new(Namespace, "scannerOperatorIdentifier"));
    public static readonly IUriNode ScannerIdentifier = new UriNode(new(Namespace, "scannerIdentifier"));
    public static readonly IUriNode VariationHasSensitivityReview = new UriNode(new(Namespace, "variationHasSensitivityReview"));
    public static readonly IUriNode VariationHasDatedNote = new UriNode(new(Namespace, "variationHasDatedNote"));
    public static readonly IUriNode VariationHasChange = new UriNode(new(Namespace, "variationHasChange"));
    public static readonly IUriNode ScannedVariationHasScannerGeographicalPlace = new UriNode(new(Namespace, "scannedVariationHasScannerGeographicalPlace"));
    public static readonly IUriNode ScannedVariationHasImageSplit = new UriNode(new(Namespace, "scannedVariationHasImageSplit"));
    public static readonly IUriNode ScannedVariationHasImageCrop = new UriNode(new(Namespace, "scannedVariationHasImageCrop"));
    public static readonly IUriNode ScannedVariationHasImageDeskew = new UriNode(new(Namespace, "scannedVariationHasImageDeskew"));

    public static readonly IUriNode CustodianshipStartAt = new UriNode(new(Namespace, "custodianshipStartAt"));
    public static readonly IUriNode ImportLocation = new UriNode(new(Namespace, "importLocation"));

    public static readonly IUriNode SensitivityReview = new UriNode(new(Namespace, "SensitivityReview"));
    public static readonly IUriNode SensitivityReviewDriId = new UriNode(new(Namespace, "sensitivityReviewDriId"));
    public static readonly IUriNode SensitivityReviewHasAsset = new UriNode(new(Namespace, "sensitivityReviewHasAsset"));
    public static readonly IUriNode SensitivityReviewHasSubset = new UriNode(new(Namespace, "sensitivityReviewHasSubset"));
    public static readonly IUriNode SensitivityReviewHasVariation = new UriNode(new(Namespace, "sensitivityReviewHasVariation"));
    public static readonly IUriNode SensitivityReviewDate = new UriNode(new(Namespace, "sensitivityReviewDate"));
    public static readonly IUriNode SensitivityReviewSensitiveName = new UriNode(new(Namespace, "sensitivityReviewSensitiveName"));
    public static readonly IUriNode SensitivityReviewSensitiveDescription = new UriNode(new(Namespace, "sensitivityReviewSensitiveDescription"));
    public static readonly IUriNode SensitivityReviewHasPastSensitivityReview = new UriNode(new(Namespace, "sensitivityReviewHasPastSensitivityReview"));
    public static readonly IUriNode SensitivityReviewHasSensitivityReviewRestriction = new UriNode(new(Namespace, "sensitivityReviewHasSensitivityReviewRestriction"));
    public static readonly IUriNode SensitivityReviewHasAccessCondition = new UriNode(new(Namespace, "sensitivityReviewHasAccessCondition"));
    public static readonly IUriNode SensitivityReviewHasChange = new UriNode(new(Namespace, "sensitivityReviewHasChange"));

    public static readonly IUriNode SensitivityReviewRestrictionCalculationStartDate = new UriNode(new(Namespace, "sensitivityReviewRestrictionCalculationStartDate"));
    public static readonly IUriNode SensitivityReviewRestrictionDuration = new UriNode(new(Namespace, "sensitivityReviewRestrictionDuration"));
    public static readonly IUriNode SensitivityReviewRestrictionEndYear = new UriNode(new(Namespace, "sensitivityReviewRestrictionEndYear"));
    public static readonly IUriNode SensitivityReviewRestrictionDescription = new UriNode(new(Namespace, "sensitivityReviewRestrictionDescription"));
    public static readonly IUriNode SensitivityReviewRestrictionReviewDate = new UriNode(new(Namespace, "sensitivityReviewRestrictionReviewDate"));
    public static readonly IUriNode SensitivityReviewRestrictionHasLegislation = new UriNode(new(Namespace, "sensitivityReviewRestrictionHasLegislation"));
    public static readonly IUriNode SensitivityReviewRestrictionHasRetentionRestriction = new UriNode(new(Namespace, "sensitivityReviewRestrictionHasRetentionRestriction"));

    public static readonly IUriNode LegislationHasUkLegislation = new UriNode(new(Namespace, "legislationHasUkLegislation"));
    public static readonly IUriNode LegislationSectionReference = new UriNode(new(Namespace, "legislationSectionReference"));

    public static readonly IUriNode AccessConditionCode = new UriNode(new(Namespace, "accessConditionCode"));
    public static readonly IUriNode AccessConditionName = new UriNode(new(Namespace, "accessConditionName"));

    public static readonly IUriNode RetentionRestrictionHasGroundForRetention = new UriNode(new(Namespace, "retentionRestrictionHasGroundForRetention"));
    public static readonly IUriNode RetentionInstrumentNumber = new UriNode(new(Namespace, "retentionInstrumentNumber"));
    public static readonly IUriNode RetentionInstrumentSignatureDate = new UriNode(new(Namespace, "retentionInstrumentSignatureDate"));
    public static readonly IUriNode RetentionRestrictionReviewDate = new UriNode(new(Namespace, "retentionRestrictionReviewDate"));

    public static readonly IUriNode GroundForRetentionCode = new UriNode(new(Namespace, "groundForRetentionCode"));
    public static readonly IUriNode GroundForRetentionDescription = new UriNode(new(Namespace, "groundForRetentionDescription"));

    public static readonly IUriNode LanguageName = new UriNode(new(Namespace, "languageName"));

    public static readonly IUriNode CopyrightTitle = new UriNode(new(Namespace, "copyrightTitle"));

    public static readonly IUriNode RetentionHasFormalBody = new UriNode(new(Namespace, "retentionHasFormalBody"));
    public static readonly IUriNode CreationHasFormalBody = new UriNode(new(Namespace, "creationHasFormalBody"));

    public static readonly IUriNode FormalBodyName = new UriNode(new(Namespace, "formalBodyName"));

    public static readonly IUriNode PublicRecord = new UriNode(new(Namespace, "PublicRecord"));
    public static readonly IUriNode NotPublicRecord = new UriNode(new(Namespace, "NotPublicRecord"));
    public static readonly IUriNode WelshPublicRecord = new UriNode(new(Namespace, "WelshPublicRecord"));
    public static readonly IUriNode NonRecordMaterial = new UriNode(new(Namespace, "NonRecordMaterial"));

    public static readonly IUriNode CourtCaseReference = new UriNode(new(Namespace, "courtCaseReference"));
    public static readonly IUriNode CourtCaseName = new UriNode(new(Namespace, "courtCaseName"));
    public static readonly IUriNode CourtCaseSummary = new UriNode(new(Namespace, "courtCaseSummary"));
    public static readonly IUriNode CourtCaseSummaryJudgment = new UriNode(new(Namespace, "courtCaseSummaryJudgment"));
    public static readonly IUriNode CourtCaseSummaryReasonsForJudgment = new UriNode(new(Namespace, "courtCaseSummaryReasonsForJudgment"));
    public static readonly IUriNode CourtCaseHearingStartDate = new UriNode(new(Namespace, "courtCaseHearingStartDate"));
    public static readonly IUriNode CourtCaseHearingEndDate = new UriNode(new(Namespace, "courtCaseHearingEndDate"));
    public static readonly IUriNode CourtCaseSequence = new UriNode(new(Namespace, "courtCaseSequence"));

    public static readonly IUriNode InquiryWitnessName = new UriNode(new(Namespace, "inquiryWitnessName"));
    public static readonly IUriNode InquiryWitnessAppearanceDescription = new UriNode(new(Namespace, "inquiryWitnessAppearanceDescription"));
    public static readonly IUriNode InquiryAppearanceSequence = new UriNode(new(Namespace, "inquiryAppearanceSequence"));
    public static readonly IUriNode InquiryAppearanceHasInquiryWitness = new UriNode(new(Namespace, "inquiryAppearanceHasInquiryWitness"));

    public static readonly IUriNode DatedNoteHasDate = new UriNode(new(Namespace, "datedNoteHasDate"));
    public static readonly IUriNode ArchivistNote = new UriNode(new(Namespace, "archivistNote"));
    public static readonly IUriNode ArchivistNoteAt = new UriNode(new(Namespace, "archivistNoteAt"));

    public static readonly IUriNode Year = new UriNode(new(Namespace, "year"));
    public static readonly IUriNode Month = new UriNode(new(Namespace, "month"));
    public static readonly IUriNode Day = new UriNode(new(Namespace, "day"));

    public static readonly IUriNode GeographicalPlaceName = new UriNode(new(Namespace, "geographicalPlaceName"));

    public static readonly IUriNode ImageSplit = new UriNode(new(Namespace, "ImageSplit"));
    public static readonly IUriNode CompositeImageSplit = new UriNode(new(Namespace, "CompositeImageSplit"));
    public static readonly IUriNode AutoImageCrop = new UriNode(new(Namespace, "AutoImageCrop"));
    public static readonly IUriNode ManualImageCrop = new UriNode(new(Namespace, "ManualImageCrop"));
    public static readonly IUriNode ImageDeskew = new UriNode(new(Namespace, "ImageDeskew"));
    public static readonly IUriNode AutoImageDeskew = new UriNode(new(Namespace, "AutoImageDeskew"));
    public static readonly IUriNode ManualImageDeskew = new UriNode(new(Namespace, "ManualImageDeskew"));

    public static readonly IUriNode SealCategoryName = new UriNode(new(Namespace, "sealCategoryName"));

    public static readonly IUriNode FragmentDimension = new UriNode(new(Namespace, "FragmentDimension"));
    public static readonly IUriNode FirstDimensionMillimetre = new UriNode(new(Namespace, "firstDimensionMillimetre"));
    public static readonly IUriNode SecondDimensionMillimetre = new UriNode(new(Namespace, "secondDimensionMillimetre"));

    public static readonly IUriNode ChangeDriId = new UriNode(new(Namespace, "changeDriId"));
    public static readonly IUriNode ChangeDateTime = new UriNode(new(Namespace, "changeDateTime"));
    public static readonly IUriNode ChangeDescription = new UriNode(new(Namespace, "changeDescription"));
    public static readonly IUriNode ChangeHasAsset = new UriNode(new(Namespace, "changeHasAsset"));
    public static readonly IUriNode ChangeHasVariation = new UriNode(new(Namespace, "changeHasVariation"));
    public static readonly IUriNode ChangeHasOperator = new UriNode(new(Namespace, "changeHasOperator"));

    public static readonly IUriNode OperatorIdentifier = new UriNode(new(Namespace, "operatorIdentifier"));
    public static readonly IUriNode OperatorName = new UriNode(new(Namespace, "operatorName"));

    public static readonly IUriNode PersonFullName = new UriNode(new(Namespace, "personFullName"));
    public static readonly IUriNode PersonGivenName = new UriNode(new(Namespace, "personGivenName"));
    public static readonly IUriNode PersonFamilyName = new UriNode(new(Namespace, "personFamilyName"));
    public static readonly IUriNode NationalRegistrationNumber = new UriNode(new(Namespace, "nationalRegistrationNumber"));
    public static readonly IUriNode SeamanServiceNumber = new UriNode(new(Namespace, "seamanServiceNumber"));
    public static readonly IUriNode PersonHasBattalionMembership = new UriNode(new(Namespace, "personHasBattalionMembership"));
    public static readonly IUriNode PersonHasContactPoint = new UriNode(new(Namespace, "personHasContactPoint"));
    public static readonly IUriNode PersonHasBirthAddress = new UriNode(new(Namespace, "personHasBirthAddress"));
    public static readonly IUriNode PersonHasDateOfBirth = new UriNode(new(Namespace, "personHasDateOfBirth"));
    public static readonly IUriNode PersonHasNextOfKinRelationship = new UriNode(new(Namespace, "personHasNextOfKinRelationship"));
    public static readonly IUriNode NextOfKinRelationshipHasNextOfKin = new UriNode(new(Namespace, "nextOfKinRelationshipHasNextOfKin"));
    public static readonly IUriNode BattalionMembershipHasBattalion = new UriNode(new(Namespace, "battalionMembershipHasBattalion"));

    public static readonly IUriNode NextOfKinRelationshipHasKinship = new UriNode(new(Namespace, "nextOfKinRelationshipHasKinship"));
    public static readonly IUriNode Wife = new UriNode(new(Namespace, "Wife"));
    public static readonly IUriNode Husband = new UriNode(new(Namespace, "Husband"));
    public static readonly IUriNode Mother = new UriNode(new(Namespace, "Mother"));
    public static readonly IUriNode Father = new UriNode(new(Namespace, "Father"));
    public static readonly IUriNode Sister = new UriNode(new(Namespace, "Sister"));
    public static readonly IUriNode Brother = new UriNode(new(Namespace, "Brother"));
    public static readonly IUriNode Uncle = new UriNode(new(Namespace, "Uncle"));
    public static readonly IUriNode Son = new UriNode(new(Namespace, "Son"));
    public static readonly IUriNode AdoptedSon = new UriNode(new(Namespace, "AdoptedSon"));
    public static readonly IUriNode AdoptedMother = new UriNode(new(Namespace, "AdoptedMother"));
    public static readonly IUriNode AdoptedDaughter = new UriNode(new(Namespace, "AdoptedDaughter"));
    public static readonly IUriNode Daughter = new UriNode(new(Namespace, "Daughter"));
    public static readonly IUriNode Grandmother = new UriNode(new(Namespace, "Grandmother"));
    public static readonly IUriNode Grandfather = new UriNode(new(Namespace, "Grandfather"));
    public static readonly IUriNode Grandson = new UriNode(new(Namespace, "Grandson"));
    public static readonly IUriNode Aunt = new UriNode(new(Namespace, "Aunt"));
    public static readonly IUriNode Stepmother = new UriNode(new(Namespace, "Stepmother"));
    public static readonly IUriNode Stepfather = new UriNode(new(Namespace, "Stepfather"));
    public static readonly IUriNode Stepuncle = new UriNode(new(Namespace, "Stepuncle"));
    public static readonly IUriNode Stepsister = new UriNode(new(Namespace, "Stepsister"));
    public static readonly IUriNode Stepbrother = new UriNode(new(Namespace, "Stepbrother"));
    public static readonly IUriNode HalfBrother = new UriNode(new(Namespace, "HalfBrother"));
    public static readonly IUriNode FosterFather = new UriNode(new(Namespace, "FosterFather"));
    public static readonly IUriNode FosterMother = new UriNode(new(Namespace, "FosterMother"));
    public static readonly IUriNode Stepson = new UriNode(new(Namespace, "Stepson"));
    public static readonly IUriNode Niece = new UriNode(new(Namespace, "Niece"));
    public static readonly IUriNode SisterInLaw = new UriNode(new(Namespace, "SisterInLaw"));
    public static readonly IUriNode BrotherInLaw = new UriNode(new(Namespace, "BrotherInLaw"));
    public static readonly IUriNode MotherInLaw = new UriNode(new(Namespace, "MotherInLaw"));
    public static readonly IUriNode FatherInLaw = new UriNode(new(Namespace, "FatherInLaw"));
    public static readonly IUriNode SonInLaw = new UriNode(new(Namespace, "SonInLaw"));
    public static readonly IUriNode Nephew = new UriNode(new(Namespace, "Nephew"));
    public static readonly IUriNode Cousin = new UriNode(new(Namespace, "Cousin"));
    public static readonly IUriNode Fiance = new UriNode(new(Namespace, "Fiance"));
    public static readonly IUriNode Fiancee = new UriNode(new(Namespace, "Fiancee"));
    public static readonly IUriNode NotRelated = new UriNode(new(Namespace, "NotRelated"));
    public static readonly IUriNode Friend = new UriNode(new(Namespace, "Friend"));
    public static readonly IUriNode Landlady = new UriNode(new(Namespace, "Landlady"));
    public static readonly IUriNode Housekeeper = new UriNode(new(Namespace, "Housekeeper"));
    public static readonly IUriNode Guardian = new UriNode(new(Namespace, "Guardian"));
    public static readonly IUriNode UndefinedKinship = new UriNode(new(Namespace, "UndefinedKinship"));

    public static readonly IUriNode BattalionName = new UriNode(new(Namespace, "battalionName"));

    public static readonly IUriNode AssetHasAssetTagType = new UriNode(new(Namespace, "assetHasAssetTagType"));
    public static readonly IUriNode BornDigitalAsset = new UriNode(new(Namespace, "BornDigitalAsset"));
    public static readonly IUriNode DigitisedAsset = new UriNode(new(Namespace, "DigitisedAsset"));
    public static readonly IUriNode SurrogateAsset = new UriNode(new(Namespace, "SurrogateAsset"));
}
