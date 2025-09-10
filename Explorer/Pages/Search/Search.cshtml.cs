using Microsoft.AspNetCore.Mvc.RazorPages;
using VDS.RDF;
using VDS.RDF.Dynamic;
using VDS.RDF.Query;

namespace Explorer.Pages.Search;

public class SearchModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://id.example.com/schema/>

        select * where {
            filter(contains(ucase(?text), ucase(@id)))
            filter(?t in (ex:Variation, ex:Asset, ex:Subset, ex:SensitivityReview))
            {
                optional { ?s ex:variationDriId ?variationDriId }
                optional { ?s ex:assetReference ?assetReference }
                optional { ?s ex:subsetReference ?subsetReference }
                optional { ?s ex:sensitivityReviewDriId ?sensitivityReviewDriId }
                {
                    ?s a ?t;
                        ex:name ?text.
                }
                union
                {
                    ?s a ?t;
                        ex:externalIdentifier ?text.
                }
            }
        }
        """;
    public DynamicSparqlResultSet Results { get; set; }

    public void OnGet()
    {
    }

    public async Task OnPostAsync(string q)
    {
        var sparql = new SparqlParameterizedString(query);
        sparql.SetParameter("id", new LiteralNode(q.Trim()));

        var client = new SparqlQueryClient(httpClient, endpoint);
        var resultSet = await client.QueryWithResultSetAsync(sparql.ToString());

        Results = new DynamicSparqlResultSet(resultSet);
    }
}
