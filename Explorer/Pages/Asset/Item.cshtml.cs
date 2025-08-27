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
                ex:assetRelationDescription ?assetRelationDescription;
                ex:assetPhysicalDescription ?assetPhysicalDescription;
                ex:assetUseRestrictionDescription ?assetUseRestrictionDescription;
                ex:evidenceProviderName ?evidenceProviderName;
                ex:investigationName ?investigationName;
                ex:courtSessionDescription ?courtSessionDescription;
                ex:assetHasLanguage ?language;
                ex:assetHasLegalStatus ?legalStatus;
                ex:assetHasCopyright ?copyright;
                ex:assetHasSubset ?subset;
                ex:assetHasVariation ?variation;
                ex:assetHasRetention ?retention;
                ex:assetHasCreation ?creation;
                ex:courtAssetHasCourtCase ?courtCase;
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
            ?courtCase ex:courtCaseReference ?courtCaseReference;
                ex:courtCaseName ?courtCaseName;
                ex:courtCaseSummaryJudgment ?courtCaseSummaryJudgment;
                ex:courtCaseSummaryReasonsForJudgment ?courtCaseSummaryReasonsForJudgment;
                ex:courtCaseHearingStartDate ?courtCaseHearingStartDate;
                ex:courtCaseHearingEndDate ?courtCaseHearingEndDate.
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
            optional { ?s ex:assetRelationDescription ?assetRelationDescription }
            optional { ?s ex:assetPhysicalDescription ?assetPhysicalDescription }
            optional { ?s ex:assetUseRestrictionDescription ?assetUseRestrictionDescription }
            optional { ?s ex:evidenceProviderName ?evidenceProviderName }
            optional { ?s ex:investigationName ?investigationName }
            optional { ?s ex:courtSessionDescription ?courtSessionDescription }
            optional { ?s ex:courtSessionDate ?courtSessionDate }
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
                ?s ex:courtAssetHasCourtCase ?courtCase.
                optional { ?courtCase ex:courtCaseReference ?courtCaseReference }
                optional { ?courtCase ex:courtCaseName ?courtCaseName }
                optional { ?courtCase ex:courtCaseSummaryJudgment ?courtCaseSummaryJudgment }
                optional { ?courtCase ex:courtCaseSummaryReasonsForJudgment ?courtCaseSummaryReasonsForJudgment }
                optional { ?courtCase ex:courtCaseHearingStartDate ?courtCaseHearingStartDate }
                optional { ?courtCase ex:courtCaseHearingEndDate ?courtCaseHearingEndDate }
            }
            optional {
                ?s ex:assetHasSensitivityReview ?sr.
        	    ?sr ex:sensitivityReviewDriId ?sensitivityReviewDriId.
                #filter not exists { ?futureSr ex:sensitivityReviewHasPastSensitivityReview ?sr }
                optional { ?sr ex:sensitivityReviewDate ?sensitivityReviewDate }
            	optional { ?sr ex:sensitivityReviewSensitiveName ?sensitivityReviewSensitiveName }
            	optional { ?sr ex:sensitivityReviewSensitiveDescription ?sensitivityReviewSensitiveDescription }
                optional {
                    ?sr ex:sensitivityReviewHasPastSensitivityReview ?pastSensitivityReview.
                    ?pastSensitivityReview ex:sensitivityReviewDriId ?pastSensitivityReviewDriId.
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
