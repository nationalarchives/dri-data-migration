using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class GroundForRetentionIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<GroundForRetentionIngest> logger)
    : BaseStagingIngest<DriGroundForRetention>(cache, sparqlClient, logger, "GroundForRetentionGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriGroundForRetention dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var code = new LiteralNode(dri.Code);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.GroundForRetentionCode, code).FirstOrDefault()?.Subject ?? NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.GroundForRetentionCode, code);
        graph.Assert(id, Vocabulary.GroundForRetentionDescription, new LiteralNode(dri.Description));
        logger.RecordBuilt(dri.Id);

        return graph;
    }

}
