using Api;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;

namespace Explorer.Pages.Variation;

public class ItemModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://id.example.com/schema/>

        construct {
            ?variation a ex:Variation;
                ex:variationName ?variationName;
                ex:variationDriId ?variationDriId;
                ex:variationDriXml ?variationDriXml;
                ex:variationNote ?variationNote;
                ex:variationRelativeLocation ?variationRelativeLocation;
                ex:variationPhysicalConditionDescription ?variationPhysicalConditionDescription;
                ex:variationReferenceGoogleId ?variationReferenceGoogleId;
                ex:variationReferenceParentGoogleId ?variationReferenceParentGoogleId;
                ex:scannerOperatorIdentifier ?scannerOperatorIdentifier;
                ex:scannerIdentifier ?scannerIdentifier;
                ex:redactedVariationSequence ?redactedVariationSequence;
                ex:variationHasDatedNote ?datedNote;
                ex:scannedVariationHasScannerGeographicalPlace ?scannerGeographicalPlace;
                ex:scannedVariationHasImageSplit ?scannedVariationHasImageSplit;
                ex:scannedVariationHasImageCrop ?scannedVariationHasImageCrop;
                ex:scannedVariationHasImageDeskew ?scannedVariationHasImageDeskew;
                ex:variationHasSensitivityReview ?sr;
                ex:variationHasAsset ?asset;
                ex:variationHasChange ?change.
            ?datedNote ex:archivistNote ?archivistNote;
                ex:year ?datedNoteYear;
                ex:month ?datedNoteMonth;
                ex:day ?datedNoteDay.
            ?scannerGeographicalPlace ex:geographicalPlaceName ?scannerGeographicalPlaceName.
            ?asset ex:assetDriId ?assetDriId;
                ex:assetReference ?assetReference;
                ex:assetHasSubset ?subset.
            ?retention ex:retentionHasFormalBody ?retentionHasFormalBody.
            ?retentionHasFormalBody ex:formalBodyName ?retentionFormalBodyName.
            ?creation ex:creationHasFormalBody ?creationHasFormalBody.
            ?creationHasFormalBody ex:formalBodyName ?creationFormalBodyName.
            ?subset ex:subsetHasBroaderSubset ?broader.
            ?broader ex:subsetReference ?broaderSubsetReference.
            ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId;
                ex:sensitivityReviewHasAccessCondition ?accessCondition;
                ex:sensitivityReviewDate ?sensitivityReviewDate;
                ex:sensitivityReviewSensitiveName ?sensitivityReviewSensitiveName;
                ex:sensitivityReviewSensitiveDescription ?sensitivityReviewSensitiveDescription;
                ex:sensitivityReviewHasPastSensitivityReview ?pastSensitivityReview;
                ex:sensitivityReviewHasSensitivityReviewRestriction ?restriction.
            ?accessCondition ex:accessConditionCode ?accessConditionCode;
                ex:accessConditionName ?accessConditionName.
            ?pastSensitivityReview ex:sensitivityReviewDriId ?pastSensitivityReviewDriId.
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
            ?change ex:changeDriId ?changeDriId;
                ex:changeDescription ?changeDescription;
                ex:changeDateTime ?changeDateTime;
                ex:changeHasOperator ?operator.
            ?operator ex:operatorIdentifier ?operatorIdentifier;
                ex:operatorName ?operatorName.
        } where {
            bind(@id as ?variationDriId)
            ?variation ex:variationName ?variationName;
                ex:variationDriId ?variationDriId;
                ex:variationHasAsset ?asset.
            optional { ?variation ex:variationDriXml ?variationDriXml }
            optional { ?variation ex:variationNote ?variationNote }
            optional { ?variation ex:variationRelativeLocation ?variationRelativeLocation }
            optional { ?variation ex:variationPhysicalConditionDescription ?variationPhysicalConditionDescription }
            optional { ?variation ex:variationReferenceGoogleId ?variationReferenceGoogleId }
            optional { ?variation ex:variationReferenceParentGoogleId ?variationReferenceParentGoogleId }
            optional { ?variation ex:scannerOperatorIdentifier ?scannerOperatorIdentifier }
            optional { ?variation ex:scannerIdentifier ?scannerIdentifier }
            optional { ?variation ex:redactedVariationSequence ?redactedVariationSequence }
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
            ?asset ex:assetDriId ?assetDriId;
                ex:assetReference ?assetReference;
                ex:assetHasSubset ?subset.
            ?subset ex:subsetReference ?subsetReference.
            optional {
                ?subset ex:subsetHasBroaderSubset* ?broader.
                ?broader ex:subsetReference ?broaderSubsetReference.
            }
            optional {
                ?variation ex:variationHasSensitivityReview ?sr.
        	    ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId.
                filter not exists { ?futureSr ex:sensitivityReviewHasPastSensitivityReview ?sr }
                optional { 
                    ?sr ex:sensitivityReviewHasAccessCondition ?accessCondition.
                    ?accessCondition ex:accessConditionCode ?accessConditionCode;
                        ex:accessConditionName ?accessConditionName.
                }
                optional { ?sr ex:sensitivityReviewDate ?sensitivityReviewDate }
            	optional { ?sr ex:sensitivityReviewSensitiveName ?sensitivityReviewSensitiveName }
            	optional { ?sr ex:sensitivityReviewSensitiveDescription ?sensitivityReviewSensitiveDescription }
                optional {
                    ?sr ex:sensitivityReviewHasPastSensitivityReview ?pastSensitivityReview.
                    ?pastSensitivityReview ex:sensitivityReviewDriId ?pastSensitivityReviewDriId.
                }
                optional { 
                    ?sr ex:sensitivityReviewHasSensitivityReviewRestriction ?restriction.
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
            optional {
                ?variation ex:variationHasChange ?change.
                optional { ?change ex:changeDriId ?changeDriId }
                optional { ?change ex:changeDescription ?changeDescription }
                optional { ?change ex:changeDateTime ?changeDateTime }
                optional {
                    ?change ex:changeHasOperator ?operator.
                    optional { ?operator ex:operatorIdentifier ?operatorIdentifier }
                    optional { ?operator ex:operatorName ?operatorName }
                }
            }
        }
        """;
    public Models.Variation Item { get; set; }

    public async Task OnGet(string id)
    {
        var sparql = new SparqlParameterizedString(query);
        sparql.SetParameter("id", new LiteralNode(HttpUtility.UrlDecode(id)));

        var client = new SparqlQueryClient(httpClient, endpoint);
        var graph = await client.QueryWithResultGraphAsync(sparql.ToString());
        var subject = graph.GetTriplesWithObject(Vocabulary.Variation).Single().Subject;
        Item = new Models.Variation(subject, graph);
    }
}
