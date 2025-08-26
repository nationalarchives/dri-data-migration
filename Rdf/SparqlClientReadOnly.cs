using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Rdf;

public abstract class SparqlClientReadOnly(HttpClient httpClient, Uri sparqlConnectionString)
{
    private readonly SparqlQueryClient client = new(httpClient, sparqlConnectionString);

    public async Task<IGraph> GetGraphAsync(string sparql, Dictionary<string, object> parameters,
        CancellationToken cancellationToken)
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

        return await client.QueryWithResultGraphAsync(parameterizedString.ToString(), cancellationToken);
    }

    public async Task<IGraph> GetGraphAsync(string sparql, object id, CancellationToken cancellationToken) =>
        await GetGraphAsync(sparql, new Dictionary<string, object> { { "id", id } }, cancellationToken);

    public async Task<SparqlResultSet> GetResultSetAsync(string sparql, CancellationToken cancellationToken) =>
        await client.QueryWithResultSetAsync(sparql, cancellationToken);

    public async Task<IUriNode?> GetSubjectAsync(string sparql, Dictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        var graph = await GetGraphAsync(sparql, parameters.ToDictionary(kv => kv.Key, kv => (object)kv.Value), cancellationToken);

        return graph.Triples.SubjectNodes.Cast<IUriNode>().SingleOrDefault();
    }

    public async Task<Dictionary<string, IUriNode>> GetDictionaryAsync(string sparql, CancellationToken cancellationToken)
    {
        var graph = await GetGraphAsync(sparql, [], cancellationToken);

        return graph.Triples.Select(t =>
            new KeyValuePair<string, IUriNode>(t.Object.AsValuedNode().AsString(), (t.Subject as IUriNode)!))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
