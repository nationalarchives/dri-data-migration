using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class GroundForRetentionIngest(ISparqlClient sparqlClient, ILogger<GroundForRetentionIngest> logger) :
    StagingIngest<DriGroundForRetention>(sparqlClient, logger, "GroundForRetentionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriGroundForRetention dri, CancellationToken cancellationToken)
    {
        var code = new LiteralNode(dri.Code);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.GroundForRetentionCode, code).FirstOrDefault()?.Subject ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.GroundForRetentionCode, code);
        graph.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));

        return Task.FromResult((Graph?)graph);
    }

}
