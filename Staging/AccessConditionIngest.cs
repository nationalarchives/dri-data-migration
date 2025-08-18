using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Staging;

public class AccessConditionIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger<AccessConditionIngest> logger)
    : BaseStagingIngest<DriAccessCondition>(cache, sparqlClient, logger, "AccessConditionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriAccessCondition dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var code = new LiteralNode(GetUriFragment(dri.Link));
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.AccessConditionCode, code).FirstOrDefault()?.Subject ?? NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AccessConditionCode, code);
        graph.Assert(id, Vocabulary.AccessConditionName, new LiteralNode(dri.Name));
        logger.RecordBuilt(dri.Id);

        return Task.FromResult((Graph?)graph);
    }
}
