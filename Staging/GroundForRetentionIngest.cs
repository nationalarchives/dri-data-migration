using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging;

public class GroundForRetentionIngest(ISparqlClient sparqlClient, ILogger<GroundForRetentionIngest> logger,
    IMeterFactory meterFactory) : StagingIngest<DriGroundForRetention>(sparqlClient, logger, meterFactory, "GroundForRetentionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriGroundForRetention dri, CancellationToken cancellationToken)
    {
        var code = new LiteralNode(dri.Code);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.GroundForRetentionCode, code) ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.GroundForRetentionCode, code);
        graph.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));

        return Task.FromResult((Graph?)graph);
    }

}
