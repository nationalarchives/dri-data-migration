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
    public static readonly IUriNode AssetReference = new UriNode(new(Namespace, "assetReference"));
    public static readonly IUriNode AssetName = new UriNode(new(Namespace, "assetName"));
    public static readonly IUriNode AssetDescription = new UriNode(new(Namespace, "assetDescription"));
    public static readonly IUriNode AssetHasSubset = new UriNode(new(Namespace, "assetHasSubset"));
    public static readonly IUriNode AssetHasVariation = new UriNode(new(Namespace, "assetHasVariation"));
    public static readonly IUriNode AssetHasRetention = new UriNode(new(Namespace, "assetHasRetention"));
    public static readonly IUriNode AssetHasSensitivityReview = new UriNode(new(Namespace, "assetHasSensitivityReview"));
    public static readonly IUriNode AssetHasLanguage = new UriNode(new(Namespace, "assetHasLanguage"));
    public static readonly IUriNode AssetHasLegalStatus = new UriNode(new(Namespace, "assetHasLegalStatus"));
    public static readonly IUriNode AssetHasCopyright = new UriNode(new(Namespace, "assetHasCopyright"));
    public static readonly IUriNode AssetHasCreation = new UriNode(new(Namespace, "assetHasCreation"));

    public static readonly IUriNode Variation = new UriNode(new(Namespace, "Variation"));
    public static readonly IUriNode VariationHasAsset = new UriNode(new(Namespace, "variationHasAsset"));
    public static readonly IUriNode VariationDriId = new UriNode(new(Namespace, "variationDriId"));
    public static readonly IUriNode VariationDriXml = new UriNode(new(Namespace, "variationDriXml"));
    public static readonly IUriNode VariationName = new UriNode(new(Namespace, "variationName"));
    public static readonly IUriNode VariationNote = new UriNode(new(Namespace, "variationNote"));
    public static readonly IUriNode VariationRelativeLocation = new UriNode(new(Namespace, "variationRelativeLocation"));
    public static readonly IUriNode VariationHasRedactedVariation = new UriNode(new(Namespace, "variationHasRedactedVariation"));
    public static readonly IUriNode VariationHasAlternativeVariation = new UriNode(new(Namespace, "variationHasAlternativeVariation"));
    public static readonly IUriNode VariationHasSensitivityReview = new UriNode(new(Namespace, "variationHasSensitivityReview"));

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
    public static readonly IUriNode RetentionRestrictionHasRetention = new UriNode(new(Namespace, "retentionRestrictionHasRetention"));
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
}
