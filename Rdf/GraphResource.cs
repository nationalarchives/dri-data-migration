using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace Rdf;

static class GraphResource
{
    internal static async Task<IUriNode?> Subset(SparqlQueryClient client, string reference)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:subsetReference ?subsetReference.
            } where {
                bind(@id as ?subsetReference)
                ?s ex:subsetReference ?subsetReference.
            }
            """;

        return await GetSubject(client, sparql, reference);
    }

    internal static async Task<IUriNode?> Asset(SparqlQueryClient client, string reference)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:assetReference ?assetReference.
            } where {
                bind(@id as ?assetReference)
                ?s ex:assetReference ?assetReference.
            }
            """;

        return await GetSubject(client, sparql, reference);
    }

    internal static async Task<IUriNode?> Variation(SparqlQueryClient client, Uri id)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:variationDriId ?variationDriId.
            } where {
                bind(@id as ?variationDriId)
                ?s ex:variationDriId ?variationDriId.
            }
            """;

        return await GetSubject(client, sparql, id.ToString());
    }

    internal static async Task<IUriNode?> Retention(SparqlQueryClient client, Uri target)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?retention ex:importLocation ?importLocation
            } where {
                bind(uri(@id) as ?target)
                {
                    ?target ex:assetHasRetention ?retention.
                    ?retention ex:importLocation ?importLocation.
                }
                union
                {
                    ?target ex:subsetHasRetention ?retention.
                    ?retention ex:importLocation ?importLocation.
                }
            }
            """;

        return await GetSubject(client, sparql, target.ToString());
    }

    internal static async Task<Dictionary<string, IUriNode>> AccessConditions(SparqlQueryClient client)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:accessConditionCode ?accessConditionCode.
            } where {
                ?s ex:accessConditionCode ?accessConditionCode.
            }
            """;

        return await GetDictionary(client, sparql);
    }

    internal static async Task<Dictionary<Uri, IUriNode>> Legislations(SparqlQueryClient client)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:legislationHasUkLegislation ?legislationHasUkLegislation.
            } where {
                ?s ex:legislationHasUkLegislation ?legislationHasUkLegislation.
            }
            """;

        var graph = await GetGraph(client, sparql, []);

        return graph.Triples.Select(t =>
            new KeyValuePair<Uri, IUriNode>((t.Object as IUriNode)!.Uri, (t.Subject as IUriNode)!))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    internal static async Task<Dictionary<string, IUriNode>> GroundsForRetention(SparqlQueryClient client)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?s ex:groundForRetentionCode ?groundForRetentionCode.
            } where {
                ?s ex:groundForRetentionCode ?groundForRetentionCode.
            }
            """;

        return await GetDictionary(client, sparql);
    }

    internal static async Task<IUriNode?> SensitivityReview(SparqlQueryClient client, string id)
    {
        var sparql = """
            prefix ex: <http://example.com/schema/>
            
            construct {
                ?subset ex:sensitivityReviewDriId ?sensitivityReviewDriId.
            } where {
                bind(@id as ?sensitivityReviewDriId)
                ?subset ex:sensitivityReviewDriId ?sensitivityReviewDriId.
            }
            """;

        return await GetSubject(client, sparql, id);
    }

    internal static async Task<IGraph> GetGraph(SparqlQueryClient client, string sparql, Dictionary<string, object> parameters)
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

    private static async Task<IUriNode?> GetSubject(SparqlQueryClient client, string sparql, string id)
    {
        var graph = await GetGraph(client, sparql, new Dictionary<string, object> { { "id", id } });

        return graph.Triples.SubjectNodes.Cast<IUriNode>().FirstOrDefault();
    }

    private static async Task<Dictionary<string, IUriNode>> GetDictionary(SparqlQueryClient client, string sparql)
    {
        var graph = await GetGraph(client, sparql, []);

        return graph.Triples.Select(t =>
            new KeyValuePair<string, IUriNode>((t.Object as ILiteralNode).AsValuedNode().AsString(), (t.Subject as IUriNode)!))
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
