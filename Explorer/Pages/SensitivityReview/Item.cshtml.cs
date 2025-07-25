using Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;

namespace Explorer.Pages.SensitivityReview;

public class ItemModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://example.com/schema/>

        construct {
            ?sr a ex:SensitivityReview;
                ex:sensitivityReviewDriId ?sensitivityReviewDriId;
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
            ?retentionRestriction ex:retentionInstrumentNumber ?retentionInstrumentNumber;
                ex:retentionInstrumentSignatureDate ?retentionInstrumentSignatureDate;
                ex:retentionRestrictionReviewDate ?retentionRestrictionReviewDate;
                ex:retentionRestrictionHasGroundForRetention ?retentionRestrictionHasGroundForRetention;
                ex:retentionRestrictionHasRetention ?retention.
        } where {
            bind(@code as ?sensitivityReviewDriId)
        	?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId.
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
                optional { ?restriction ex:sensitivityReviewRestrictionHasLegislation ?legislation }
                optional {
                    ?restriction ex:sensitivityReviewRestrictionHasRetentionRestriction ?retentionRestriction.
                    optional { ?retentionRestriction ex:retentionInstrumentNumber ?retentionInstrumentNumber }
                    optional { ?retentionRestriction ex:retentionInstrumentSignatureDate ?retentionInstrumentSignatureDate }
                    optional { ?retentionRestriction ex:retentionRestrictionReviewDate ?retentionRestrictionReviewDate }
                    optional { ?retentionRestriction ex:retentionRestrictionHasGroundForRetention ?retentionRestrictionHasGroundForRetention }
                    optional {
                        ?retentionRestriction ex:retentionRestrictionHasRetention ?retention.
                        optional { ?asset ex:assetHasRetention ?retention }
                        optional { ?subset ex:subsetHasRetention ?retention }
                    }
                }
            }
        }
        """;

    public Models.SensitivityReview Item { get; set; }

    public async Task OnGet(string id)
    {
        var sparql = new SparqlParameterizedString(query);
        sparql.SetParameter("code", new LiteralNode(HttpUtility.UrlDecode(id)));

        var client = new SparqlQueryClient(httpClient, endpoint);
        var graph = await client.QueryWithResultGraphAsync(sparql.ToString());
        var subject = graph.GetTriplesWithObject(Vocabulary.SensitivityReview).Single().Subject;
        Item = new Models.SensitivityReview(subject, graph);
    }
}
