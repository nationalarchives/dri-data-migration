using Microsoft.AspNetCore.Mvc.RazorPages;
using VDS.RDF.Query;

namespace Explorer.Pages.Subset;

public class ListModel(HttpClient httpClient, IConfiguration configuration) : PageModel
{
    private readonly Uri endpoint = new(configuration.GetConnectionString("Sparql"));
    private readonly string query = """
        prefix ex: <http://id.example.com/schema/>

        construct {
            ?s a ex:Subset;
                ex:subsetReference ?subsetReference;
                ex:subsetName ?subsetName.
        } where {
            ?s ex:subsetReference ?subsetReference.
            optional { ?s ex:subsetName ?subsetName }
            filter not exists { ?s ex:subsetHasBroaderSubset ?parent }
        }
        """;

    public Models.Subsets List { get; set; }

    public async Task OnGetAsync()
    {
        var client = new SparqlQueryClient(httpClient, endpoint);
        var graph = await client.QueryWithResultGraphAsync(query);
        List = new Models.Subsets(graph);
    }

}
