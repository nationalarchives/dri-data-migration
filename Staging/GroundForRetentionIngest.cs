using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class GroundForRetentionIngest(ISparqlClient sparqlClient, ILogger<GroundForRetentionIngest> logger, ICacheClient cacheClient)
    : StagingIngest<DriGroundForRetention>(sparqlClient, logger, cacheClient, "GroundForRetentionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriGroundForRetention dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var code = new LiteralNode(dri.Code);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.GroundForRetentionCode, code).FirstOrDefault()?.Subject ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.GroundForRetentionCode, code);
        graph.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));
        logger.RecordBuilt(dri.Id);

        return Task.FromResult((Graph?)graph);
    }

}
