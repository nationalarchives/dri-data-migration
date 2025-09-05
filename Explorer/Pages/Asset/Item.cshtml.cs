using Api;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;

namespace Explorer.Pages.Asset;

public class ItemModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://id.example.com/schema/>

        construct {
            ?s a ex:Asset;
                ex:assetDriId ?assetDriId;
                ex:assetReference ?assetReference;
                ex:assetName ?assetName;
                ex:assetDriXml ?assetDriXml;
                ex:assetDescription ?assetDescription;
                ex:batchDriId ?batchDriId;
                ex:consignmentTdrId ?consignmentTdrId;
                ex:assetPastReference ?assetPastReference;
                ex:assetSummary ?assetSummary;
                ex:assetTag ?assetTag;
                ex:assetSourceInternalName ?assetSourceInternalName;
                ex:filmProductionCompanyName ?filmProductionCompanyName;
                ex:filmTitle ?filmTitle;
                ex:filmDuration ?filmDuration;
                ex:assetRelationDescription ?assetRelationDescription;
                ex:assetPhysicalDescription ?assetPhysicalDescription;
                ex:assetUsageRestrictionDescription ?assetUsageRestrictionDescription;
                ex:evidenceProviderName ?evidenceProviderName;
                ex:investigationName ?investigationName;
                ex:courtSessionDescription ?courtSessionDescription;
                ex:courtSessionDate ?courtSessionDate;
                ex:inquirySessionDescription ?inquirySessionDescription;
                ex:inquiryHearingDate ?inquiryHearingDate;
                ex:imageSequenceStart ?imageSequenceStart;
                ex:imageSequenceEnd ?imageSequenceEnd;
                ex:paperNumber ?paperNumber;
                ex:assetHasDimension ?dimension;
                ex:sealAssetHasObverseDimension ?obverseDimension;
                ex:sealAssetHasReverseDimension ?reverseDimension;
                ex:sealAssetHasSealCategory ?sealAssetHasSealCategory;
                ex:sealOwnerName ?sealOwnerName;
                ex:sealColour ?sealColour;
                ex:assetHasUkGovernmentWebArchive ?assetHasUkGovernmentWebArchive;
                ex:assetHasAssociatedGeographicalPlace ?associatedGeographicalPlace;
                ex:assetHasOriginDateStart ?originDateStart;
                ex:assetHasOriginDateEnd ?originDateEnd;
                ex:assetHasOriginApproximateDateStart ?originApproximateDateStart;
                ex:assetHasOriginApproximateDateEnd ?originApproximateDateEnd;
                ex:sealAssetHasStartDate ?sealDateStart;
                ex:sealAssetHasEndDate ?sealDateEnd;
                ex:sealAssetHasObverseStartDate ?sealObverseStartDate;
                ex:sealAssetHasObverseEndDate ?sealObverseEndDate;
                ex:sealAssetHasReverseStartDate ?sealReverseStartDate;
                ex:sealAssetHasReverseEndDate ?sealReverseEndDate;
                ex:assetHasLanguage ?language;
                ex:assetHasLegalStatus ?legalStatus;
                ex:assetHasCopyright ?copyright;
                ex:assetHasSubset ?subset;
                ex:assetHasVariation ?variation;
                ex:assetHasRetention ?retention;
                ex:assetHasCreation ?creation;
                ex:courtAssetHasCourtCase ?courtCase;
                ex:inquiryAssetHasInquiryAppearance ?inquiryAppearance;
                ex:assetHasSensitivityReview ?sr.
            ?language ex:languageName ?languageName.
            ?copyright ex:copyrightTitle ?copyrightTitle.
            ?creation ex:creationHasFormalBody ?creationHasFormalBody.
            ?creationHasFormalBody ex:formalBodyName ?creationFormalBodyName.
            ?subset ex:subsetReference ?subsetReference;
                ex:subsetHasBroaderSubset ?broader.
            ?broader ex:subsetReference ?broaderSubsetReference.
            ?variation ex:variationName ?variationName;
                ex:variationDriId ?variationDriId;
                ex:variationPastName ?variationPastName;
                ex:variationNote ?variationNote;
                ex:variationRelativeLocation ?variationRelativeLocation;
                ex:variationPhysicalConditionDescription ?variationPhysicalConditionDescription;
                ex:variationReferenceGoogleId ?variationReferenceGoogleId;
                ex:variationReferenceParentGoogleId ?variationReferenceParentGoogleId;
                ex:scannerOperatorIdentifier ?scannerOperatorIdentifier;
                ex:scannerIdentifier ?scannerIdentifier;
                ex:variationHasDatedNote ?datedNote;
                ex:scannedVariationHasScannerGeographicalPlace ?scannerGeographicalPlace;
                ex:scannedVariationHasImageSplit ?scannedVariationHasImageSplit;
                ex:scannedVariationHasImageCrop ?scannedVariationHasImageCrop;
                ex:scannedVariationHasImageDeskew ?scannedVariationHasImageDeskew;
                ex:variationHasRedactedVariation ?variationHasRedactedVariation;
                ex:variationHasSensitivityReview ?srv.
            ?datedNote ex:archivistNote ?archivistNote;
                ex:year ?datedNoteYear;
                ex:month ?datedNoteMonth;
                ex:day ?datedNoteDay.
            ?scannerGeographicalPlace ex:geographicalPlaceName ?scannerGeographicalPlaceName.
            ?variationHasRedactedVariation ex:variationDriId ?redactedVariationDriId.
            ?srv ex:sensitivityReviewDriId ?vSensitivityReviewDriId;
                ex:sensitivityReviewHasAccessCondition ?accessCondition;
                ex:sensitivityReviewDate ?vSensitivityReviewDate;
                ex:sensitivityReviewSensitiveName ?vSensitivityReviewSensitiveName;
                ex:sensitivityReviewSensitiveDescription ?vSensitivityReviewSensitiveDescription;
                ex:sensitivityReviewHasSensitivityReviewRestriction ?restriction.
            ?accessCondition ex:accessConditionCode ?accessConditionCode;
                ex:accessConditionName ?accessConditionName.
            ?restriction ex:sensitivityReviewRestrictionReviewDate ?sensitivityReviewRestrictionReviewDate;
                ex:sensitivityReviewRestrictionCalculationStartDate ?sensitivityReviewRestrictionCalculationStartDate;
                ex:sensitivityReviewRestrictionDuration ?sensitivityReviewRestrictionDuration;
                ex:sensitivityReviewRestrictionEndYear ?sensitivityReviewRestrictionEndYear;
                ex:sensitivityReviewRestrictionDescription ?sensitivityReviewRestrictionDescription;
                ex:sensitivityReviewRestrictionHasLegislation ?legislation;
                ex:sensitivityReviewRestrictionHasRetentionRestriction ?retentionRestriction.
            ?legislation ex:legislationHasUkLegislation ?legislationHasUkLegislation;
                ex:legislationSectionReference ?legislationSectionReference.
            ?retentionRestriction ex:retentionInstrumentNumber ?retentionInstrumentNumber;
                ex:retentionInstrumentSignatureDate ?retentionInstrumentSignatureDate;
                ex:retentionRestrictionReviewDate ?retentionRestrictionReviewDate;
                ex:retentionRestrictionHasGroundForRetention ?retentionRestrictionHasGroundForRetention;
                ex:retentionRestrictionHasRetention ?retention.
            ?retentionRestrictionHasGroundForRetention ex:groundForRetentionCode ?groundForRetentionCode;
                ex:groundForRetentionDescription ?groundForRetentionDescription.
            ?retention ex:custodianshipStartAt ?created;
                ex:importLocation ?importLocation;
                ex:retentionHasFormalBody ?retentionHasFormalBody.
            ?retentionHasFormalBody ex:formalBodyName ?retentionFormalBodyName.
            ?originDateStart ex:year ?startYear;
                ex:month ?startMonth;
                ex:day ?startDay.
            ?originDateEnd ex:year ?endYear;
                ex:month ?endMonth;
                ex:day ?endDay.
            ?originApproximateDateStart ex:year ?approximateStartYear;
                ex:month ?approximateStartMonth;
                ex:day ?approximateStartDay.
            ?originApproximateDateEnd ex:year ?approximateEndYear;
                ex:month ?approximateEndMonth;
                ex:day ?approximateEndDay.
            ?sealDateStart ex:year ?sealStartYear;
                ex:month ?sealStartMonth;
                ex:day ?sealStartDay.
            ?sealDateEnd ex:year ?sealEndYear;
                ex:month ?sealEndMonth;
                ex:day ?sealEndDay.
            ?sealObverseStartDate ex:year ?sealObverseStartYear;
                ex:month ?sealObverseStartMonth;
                ex:day ?sealObverseStartDay.
            ?sealObverseEndDate ex:year ?sealObverseEndYear;
                ex:month ?sealObverseEndMonth;
                ex:day ?sealObverseEndDay.
            ?sealReverseStartDate ex:year ?sealReverseStartYear;
                ex:month ?sealReverseStartMonth;
                ex:day ?sealReverseStartDay.
            ?sealReverseEndDate ex:year ?sealReverseEndYear;
                ex:month ?sealReverseEndMonth;
                ex:day ?sealReverseEndDay.
            ?dimension ex:firstDimensionMillimetre ?firstDimensionMillimetre;
                ex:secondDimensionMillimetre ?secondDimensionMillimetre.
            ?obverseDimension ex:firstDimensionMillimetre ?obverseFirstDimensionMillimetre;
                ex:secondDimensionMillimetre ?obverseSecondDimensionMillimetre.
            ?reverseDimension ex:firstDimensionMillimetre ?reverseFirstDimensionMillimetre;
                ex:secondDimensionMillimetre ?reverseSecondDimensionMillimetre.
            ?sealAssetHasSealCategory ex:sealCategoryName ?sealCategoryName.
            ?associatedGeographicalPlace ex:geographicalPlaceName ?geographicalPlaceName.
            ?courtCase ex:courtCaseReference ?courtCaseReference;
                ex:courtCaseName ?courtCaseName;
                ex:courtCaseSummaryJudgment ?courtCaseSummaryJudgment;
                ex:courtCaseSummaryReasonsForJudgment ?courtCaseSummaryReasonsForJudgment;
                ex:courtCaseHearingStartDate ?courtCaseHearingStartDate;
                ex:courtCaseHearingEndDate ?courtCaseHearingEndDate.
            ?inquiryAppearance ex:inquiryWitnessName ?inquiryWitnessName;
                ex:inquiryWitnessAppearanceDescription ?inquiryWitnessAppearanceDescription.
            ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId;
                ex:sensitivityReviewDate ?sensitivityReviewDate;
                ex:sensitivityReviewSensitiveName ?sensitivityReviewSensitiveName;
                ex:sensitivityReviewSensitiveDescription ?sensitivityReviewSensitiveDescription;
                ex:sensitivityReviewHasPastSensitivityReview ?pastSensitivityReview.
            ?pastSensitivityReview ex:sensitivityReviewDriId ?pastSensitivityReviewDriId.
        } where {
            bind(@code as ?assetReference)
            ?s ex:assetDriId ?assetDriId;
                ex:assetReference ?assetReference;
                ex:assetHasSubset ?subset.
            ?subset ex:subsetReference ?subsetReference.
            optional {
                ?subset ex:subsetHasBroaderSubset* ?broader.
                ?broader ex:subsetReference ?broaderSubsetReference.
            }
            optional { ?s ex:assetName ?assetName }
            optional { ?s ex:assetDriXml ?assetDriXml }
            optional { ?s ex:assetDescription ?assetDescription }
            optional { ?s ex:batchDriId ?batchDriId }
            optional { ?s ex:consignmentTdrId ?consignmentTdrId }
            optional { ?s ex:assetPastReference ?assetPastReference }
            optional { ?s ex:assetSummary ?assetSummary }
            optional { ?s ex:assetTag ?assetTag }
            optional { ?s ex:assetSourceInternalName ?assetSourceInternalName }
            optional { ?s ex:filmProductionCompanyName ?filmProductionCompanyName }
            optional { ?s ex:filmTitle ?filmTitle }
            optional { ?s ex:filmDuration ?filmDuration }
            optional { ?s ex:assetRelationDescription ?assetRelationDescription }
            optional { ?s ex:assetPhysicalDescription ?assetPhysicalDescription }
            optional { ?s ex:assetUsageRestrictionDescription ?assetUsageRestrictionDescription }
            optional { ?s ex:evidenceProviderName ?evidenceProviderName }
            optional { ?s ex:investigationName ?investigationName }
            optional { ?s ex:courtSessionDescription ?courtSessionDescription }
            optional { ?s ex:courtSessionDate ?courtSessionDate }
            optional { ?s ex:inquirySessionDescription ?inquirySessionDescription }
            optional { ?s ex:inquiryHearingDate ?inquiryHearingDate }
            optional { ?s ex:imageSequenceStart ?imageSequenceStart }
            optional { ?s ex:imageSequenceEnd ?imageSequenceEnd }
            optional { ?s ex:paperNumber ?paperNumber }
            optional { ?s ex:sealOwnerName ?sealOwnerName }
            optional { ?s ex:sealColour ?sealColour }
            optional { ?s ex:assetHasUkGovernmentWebArchive ?assetHasUkGovernmentWebArchive }
            optional {
                ?s ex:assetHasLanguage ?language.
                ?language ex:languageName ?languageName.
            }
            optional {
                ?s ex:assetHasCopyright ?copyright.
                ?copyright ex:copyrightTitle ?copyrightTitle.
            }
            optional { ?s ex:assetHasLegalStatus ?legalStatus }
            optional {
                ?s ex:assetHasVariation ?variation.
                ?variation ex:variationName ?variationName;
                    ex:variationDriId ?variationDriId.
                optional { ?variation ex:variationPastName ?variationPastName }
                optional { ?variation ex:variationNote ?variationNote }
                optional { ?variation ex:variationRelativeLocation ?variationRelativeLocation }
                optional { ?variation ex:variationPhysicalConditionDescription ?variationPhysicalConditionDescription }
                optional { ?variation ex:variationReferenceGoogleId ?variationReferenceGoogleId }
                optional { ?variation ex:variationReferenceParentGoogleId ?variationReferenceParentGoogleId }
                optional { ?variation ex:scannerOperatorIdentifier ?scannerOperatorIdentifier }
                optional { ?variation ex:scannerIdentifier ?scannerIdentifier }
                optional {
                    ?variation ex:variationHasDatedNote ?datedNote.
                    optional { ?datedNote ex:archivistNote ?archivistNote }
                    optional { ?datedNote ex:year ?datedNoteYear }
                    optional { ?datedNote ex:month ?datedNoteMonth }
                    optional { ?datedNote ex:day ?datedNoteDay }
                }
                optional {
                    ?variation ex:scannedVariationHasScannerGeographicalPlace ?scannerGeographicalPlace.
                    ?scannerGeographicalPlace ex:geographicalPlaceName ?scannerGeographicalPlaceName.
                }
                optional { ?variation ex:scannedVariationHasImageSplit ?scannedVariationHasImageSplit }
                optional { ?variation ex:scannedVariationHasImageCrop ?scannedVariationHasImageCrop }
                optional { ?variation ex:scannedVariationHasImageDeskew ?scannedVariationHasImageDeskew }
                optional { 
                    ?variation ex:variationHasRedactedVariation ?variationHasRedactedVariation.
                    ?variationHasRedactedVariation ex:variationDriId ?redactedVariationDriId.
                }
                optional {
                    ?variation ex:variationHasSensitivityReview ?srv.
        	        ?srv ex:sensitivityReviewDriId ?vSensitivityReviewDriId.
                    filter not exists { ?futureSr ex:sensitivityReviewHasPastSensitivityReview ?srv }
                    optional { 
                        ?srv ex:sensitivityReviewHasAccessCondition ?accessCondition.
                        ?accessCondition ex:accessConditionCode ?accessConditionCode;
                            ex:accessConditionName ?accessConditionName.
                    }
                    optional { ?srv ex:sensitivityReviewDate ?vSensitivityReviewDate }
            	    optional { ?srv ex:sensitivityReviewSensitiveName ?vSensitivityReviewSensitiveName }
            	    optional { ?srv ex:sensitivityReviewSensitiveDescription ?vSensitivityReviewSensitiveDescription }
                    optional { 
                        ?srv ex:sensitivityReviewHasSensitivityReviewRestriction ?restriction.
                        optional { ?restriction ex:sensitivityReviewRestrictionReviewDate ?sensitivityReviewRestrictionReviewDate }
                        optional { ?restriction ex:sensitivityReviewRestrictionCalculationStartDate ?sensitivityReviewRestrictionCalculationStartDate }
                        optional { ?restriction ex:sensitivityReviewRestrictionDuration ?sensitivityReviewRestrictionDuration }
                        optional { ?restriction ex:sensitivityReviewRestrictionEndYear ?sensitivityReviewRestrictionEndYear }
                        optional { ?restriction ex:sensitivityReviewRestrictionDescription ?sensitivityReviewRestrictionDescription }
                        optional { 
                            ?restriction ex:sensitivityReviewRestrictionHasLegislation ?legislation.
                            ?legislation ex:legislationHasUkLegislation ?legislationHasUkLegislation.
                            optional { ?legislation ex:legislationSectionReference ?legislationSectionReference }
                        }
                        optional {
                            ?restriction ex:sensitivityReviewRestrictionHasRetentionRestriction ?retentionRestriction.
                            optional { ?retentionRestriction ex:retentionInstrumentNumber ?retentionInstrumentNumber }
                            optional { ?retentionRestriction ex:retentionInstrumentSignatureDate ?retentionInstrumentSignatureDate }
                            optional { ?retentionRestriction ex:retentionRestrictionReviewDate ?retentionRestrictionReviewDate }
                            optional {
                                ?retentionRestriction ex:retentionRestrictionHasGroundForRetention ?retentionRestrictionHasGroundForRetention.
                                ?retentionRestrictionHasGroundForRetention ex:groundForRetentionCode ?groundForRetentionCode;
                                    ex:groundForRetentionDescription ?groundForRetentionDescription.
                            }
                            optional {
                                ?retentionRestriction ex:retentionRestrictionHasRetention ?retention.
                                optional { ?asset ex:assetHasRetention ?retention }
                                optional { ?subset ex:subsetHasRetention ?retention }
                            }
                        }
                    }
                }
            }
            optional {
                ?s ex:assetHasRetention ?retention.
        	    optional { ?retention ex:importLocation ?importLocation }
                optional { ?retention ex:custodianshipStartAt ?created }
                optional {
                    ?retention ex:retentionHasFormalBody ?retentionHasFormalBody.
                    ?retentionHasFormalBody ex:formalBodyName ?retentionFormalBodyName.
                }
            }
            optional {
                ?s ex:assetHasCreation ?creation.
                optional {
                    ?creation ex:creationHasFormalBody ?creationHasFormalBody.
                    ?creationHasFormalBody ex:formalBodyName ?creationFormalBodyName.
                }
            }
            optional {
                ?s ex:sealAssetHasSealCategory ?sealAssetHasSealCategory.
                ?sealAssetHasSealCategory ex:sealCategoryName ?sealCategoryName.
            }
            optional { 
                ?s ex:assetHasAssociatedGeographicalPlace ?associatedGeographicalPlace.
                ?associatedGeographicalPlace ex:geographicalPlaceName ?geographicalPlaceName.
            }
            optional {
                ?s ex:assetHasDimension ?dimension.
                optional { ?dimension ex:firstDimensionMillimetre ?firstDimensionMillimetre }
                optional { ?dimension ex:secondDimensionMillimetre ?secondDimensionMillimetre }
            }
            optional {
                ?s ex:sealAssetHasObverseDimension ?obverseDimension.
                optional { ?obverseDimension ex:firstDimensionMillimetre ?obverseFirstDimensionMillimetre }
                optional { ?obverseDimension ex:secondDimensionMillimetre ?obverseSecondDimensionMillimetre }
            }
            optional {
                ?s ex:sealAssetHasReverseDimension ?reverseDimension.
                optional { ?reverseDimension ex:firstDimensionMillimetre ?reverseFirstDimensionMillimetre }
                optional { ?reverseDimension ex:secondDimensionMillimetre ?reverseSecondDimensionMillimetre }
            }
            optional {
                ?s ex:courtAssetHasCourtCase ?courtCase.
                optional { ?courtCase ex:courtCaseReference ?courtCaseReference }
                optional { ?courtCase ex:courtCaseName ?courtCaseName }
                optional { ?courtCase ex:courtCaseSummaryJudgment ?courtCaseSummaryJudgment }
                optional { ?courtCase ex:courtCaseSummaryReasonsForJudgment ?courtCaseSummaryReasonsForJudgment }
                optional { ?courtCase ex:courtCaseHearingStartDate ?courtCaseHearingStartDate }
                optional { ?courtCase ex:courtCaseHearingEndDate ?courtCaseHearingEndDate }
            }
            optional {
                ?s ex:assetHasOriginDateStart ?originDateStart.
                optional { ?originDateStart ex:year ?startYear }
                optional { ?originDateStart ex:month ?startMonth }
                optional { ?originDateStart ex:day ?startDay }
            }
            optional {
                ?s ex:assetHasOriginDateEnd ?originDateEnd.
                optional { ?originDateEnd ex:year ?endYear }
                optional { ?originDateEnd ex:month ?endMonth }
                optional { ?originDateEnd ex:day ?endDay }
            }
            optional {
                ?s ex:assetHasOriginApproximateDateStart ?originApproximateDateStart.
                optional { ?originApproximateDateStart ex:year ?approximateStartYear }
                optional { ?originApproximateDateStart ex:month ?approximateStartMonth }
                optional { ?originApproximateDateStart ex:day ?approximateStartDay }
            }
            optional {
                ?s ex:assetHasOriginApproximateDateEnd ?originApproximateDateEnd.
                optional { ?originApproximateDateEnd ex:year ?approximateEndYear }
                optional { ?originApproximateDateEnd ex:month ?approximateEndMonth }
                optional { ?originApproximateDateEnd ex:day ?approximateEndDay }
            }
            optional {
                ?s ex:sealAssetHasStartDate ?sealDateStart.
                optional { ?sealDateStart ex:year ?sealStartYear }
                optional { ?sealDateStart ex:month ?sealStartMonth }
                optional { ?sealDateStart ex:day ?sealStartDay }
            }
            optional {
                ?s ex:sealAssetHasEndDate ?sealDateEnd.
                optional { ?sealDateEnd ex:year ?sealEndYear }
                optional { ?sealDateEnd ex:month ?sealEndMonth }
                optional { ?sealDateEnd ex:day ?sealEndDay }
            }
            optional {
                ?s ex:sealAssetHasObverseStartDate ?sealObverseStartDate.
                optional { ?sealObverseStartDate ex:year ?sealObverseStartYear }
                optional { ?sealObverseStartDate ex:month ?sealObverseStartMonth }
                optional { ?sealObverseStartDate ex:day ?sealObverseStartDay }
            }
            optional {
                ?s ex:sealAssetHasObverseEndDate ?sealObverseEndDate.
                optional { ?sealObverseEndDate ex:year ?sealObverseEndYear }
                optional { ?sealObverseEndDate ex:month ?sealObverseEndMonth }
                optional { ?sealObverseEndDate ex:day ?sealObverseEndDay }
            }
            optional {
                ?s ex:sealAssetHasReverseStartDate ?sealReverseStartDate.
                optional { ?sealReverseStartDate ex:year ?sealReverseStartYear }
                optional { ?sealReverseStartDate ex:month ?sealReverseStartMonth }
                optional { ?sealReverseStartDate ex:day ?sealReverseStartDay }
            }
            optional {
                ?s ex:sealAssetHasReverseEndDate ?sealReverseEndDate.
                optional { ?sealReverseEndDate ex:year ?sealReverseEndYear }
                optional { ?sealReverseEndDate ex:month ?sealReverseEndMonth }
                optional { ?sealReverseEndDate ex:day ?sealReverseEndDay }
            }
            optional {
                ?s ex:assetHasSensitivityReview ?sr.
        	    ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId.
                filter not exists { ?futureSr ex:sensitivityReviewHasPastSensitivityReview ?sr }
                optional { ?sr ex:sensitivityReviewDate ?sensitivityReviewDate }
            	optional { ?sr ex:sensitivityReviewSensitiveName ?sensitivityReviewSensitiveName }
            	optional { ?sr ex:sensitivityReviewSensitiveDescription ?sensitivityReviewSensitiveDescription }
                optional {
                    ?sr ex:sensitivityReviewHasPastSensitivityReview ?pastSensitivityReview.
                    ?pastSensitivityReview ex:sensitivityReviewDriId ?pastSensitivityReviewDriId.
                }
            }
            optional {
                ?s ex:inquiryAssetHasInquiryAppearance ?inquiryAppearance.
                optional { ?inquiryAppearance ex:inquiryWitnessName ?inquiryWitnessName }
                optional { ?inquiryAppearance ex:inquiryWitnessAppearanceDescription ?inquiryWitnessAppearanceDescription }
            }
        }
        """;
    public Models.Asset Item { get; set; }

    public async Task OnGet(string id)
    {
        var sparql = new SparqlParameterizedString(query);
        sparql.SetParameter("code", new LiteralNode(HttpUtility.UrlDecode(id)));

        var client = new SparqlQueryClient(httpClient, endpoint);
        var graph = await client.QueryWithResultGraphAsync(sparql.ToString());
        var subject = graph.GetTriplesWithObject(Vocabulary.Asset).Single().Subject;
        Item = new Models.Asset(subject, graph);
    }
}
