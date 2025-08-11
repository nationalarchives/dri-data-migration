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
        prefix ex: <http://example.com/schema/>

        construct {
            ?variation a ex:Variation;
                ex:variationName ?variationName;
                ex:variationDriId ?variationDriId;
                ex:variationHasSensitivityReview ?sr;
                ex:variationHasAsset ?asset.
            ?asset ex:assetReference ?assetReference;
                ex:assetHasSubset ?subset.
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
        } where {
            bind(@id as ?variationDriId)
            ?variation ex:variationName ?variationName;
                ex:variationDriId ?variationDriId;
                ex:variationHasAsset ?asset.
            ?asset ex:assetReference ?assetReference;
                ex:assetHasSubset ?subset.
            ?subset ex:subsetReference ?subsetReference.
            optional {
                ?subset ex:subsetHasBroaderSubset* ?broader.
                ?broader ex:subsetReference ?broaderSubsetReference.
            }
            optional {
                ?variation ex:variationHasSensitivityReview ?sr.
        	    ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId.
                # filter not exists { ?futureSr ex:sensitivityReviewHasPastSensitivityReview ?sr }
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
