using Explorer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using VDS.RDF;
using VDS.RDF.Dynamic;
using VDS.RDF.Query;

namespace Explorer.Pages.Search;

public class SearchModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://example.com/schema/>

        select ?variationName ?variationDriId ?assetReference ?subsetReference where {
            filter(contains(ucase(?text), ucase(@id)))
            filter(?t in (ex:Variation, ex:Asset, ex:Subset))
            {
                ?s ex:variationName ?variationName;
                	ex:variationDriId ?variationDriId.
            }
            union
            {
                ?s ex:assetReference ?assetReference
            }
            union
            {
            	?s ex:subsetReference ?subsetReference
        	}
            {
                select * where {
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
