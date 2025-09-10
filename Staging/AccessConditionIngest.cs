using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class AccessConditionIngest(ISparqlClient sparqlClient, ILogger<AccessConditionIngest> logger, ICacheClient cacheClient)
    : StagingIngest<DriAccessCondition>(sparqlClient, logger, cacheClient, "AccessConditionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriAccessCondition dri, CancellationToken cancellationToken)
    {
        var code = new LiteralNode(GetUriFragment(dri.Link));
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.AccessConditionCode, code).FirstOrDefault()?.Subject ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AccessConditionCode, code);
        GraphAssert.Text(graph, id, dri.Name, Vocabulary.AccessConditionName);

        return Task.FromResult((Graph?)graph);
    }
}
