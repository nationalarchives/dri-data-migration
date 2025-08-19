using Api;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;

namespace Explorer.Pages.Asset;
//http://localhost:5093/asset/ACE%2F1%2FFRC%2FZ - multiple sr check
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
                ex:assetHasLanguage ?language;
                ex:assetHasLegalStatus ?legalStatus;
                ex:assetHasCopyright ?copyright;
                ex:assetHasSubset ?subset;
                ex:assetHasVariation ?variation;
                ex:assetHasRetention ?retention;
                ex:assetHasCreation ?creation;
                ex:assetHasSensitivityReview ?sr.
            ?language ex:languageName ?languageName.
            ?copyright ex:copyrightTitle ?copyrightTitle.
            ?creation ex:creationHasFormalBody ?creationHasFormalBody.
            ?creationHasFormalBody ex:formalBodyName ?creationFormalBodyName.
            ?subset ex:subsetReference ?subsetReference;
                ex:subsetHasBroaderSubset ?broader.
            ?broader ex:subsetReference ?broaderSubsetReference.
            ?variation ex:variationName ?variationName;
                ex:variationDriId ?variationDriId.
            ?retention ex:custodianshipStartAt ?created;
                ex:importLocation ?importLocation;
                ex:retentionHasFormalBody ?retentionHasFormalBody.
            ?retentionHasFormalBody ex:formalBodyName ?retentionFormalBodyName.
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
                ?s ex:assetHasSensitivityReview ?sr.
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
