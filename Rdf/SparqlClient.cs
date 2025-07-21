using Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace Rdf;

public class SparqlClient(HttpClient httpClient, Uri connectionString) : ISparqlClient
{
    private readonly SparqlQueryClient client = new(httpClient, connectionString);
    private readonly SparqlUpdateClient updateClient = new(httpClient, connectionString);

    public async Task<IGraph> GetGraphAsync(string sparql, Dictionary<string, object> parameters)
    {
        var parameterizedString = new SparqlParameterizedString(sparql);
        foreach (var kv in parameters)
        {
            var literal = kv.Value switch
            {
                string txt => new LiteralNode(txt),
                int number => new LongNode(number),
                _ => throw new ArgumentException($"Unrecognized type of {kv.Key}", nameof(parameters))
            };
            parameterizedString.SetParameter(kv.Key, literal);
        }

        return await client.QueryWithResultGraphAsync(parameterizedString.ToString());
    }

    public async Task<IGraph> GetGraphAsync(string sparql, object id) => await GetGraphAsync(sparql, new Dictionary<string, object> { { "id", id } });

    public async Task<SparqlResultSet> GetResultSetAsync(string sparql) => await client.QueryWithResultSetAsync(sparql);

    public async Task<IUriNode?> GetSubjectAsync(string sparql, object id)
    {
        var graph = await GetGraphAsync(sparql, new Dictionary<string, object> { { "id", id } });

        return graph.Triples.SubjectNodes.Cast<IUriNode>().FirstOrDefault();
    }

    public async Task<Dictionary<string, IUriNode>> GetDictionary(string sparql)
    {
        var graph = await GetGraphAsync(sparql, []);

        return graph.Triples.Select(t =>
            new KeyValuePair<string, IUriNode>(t.Object.AsValuedNode().AsString(), (t.Subject as IUriNode)!))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public async Task ApplyDiffAsync(GraphDiffReport diffReport) => await updateClient.UpdateAsync(diffReport.AsUpdate().ToString());

}
