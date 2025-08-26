using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Staging;

public class AccessConditionIngest(ISparqlClient sparqlClient, ILogger<AccessConditionIngest> logger)
    : StagingIngest<DriAccessCondition>(sparqlClient, logger, "AccessConditionGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriAccessCondition dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var code = new LiteralNode(BaseIngest.GetUriFragment(dri.Link));
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.AccessConditionCode, code).FirstOrDefault()?.Subject ?? BaseIngest.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.AccessConditionCode, code);
        graph.Assert(id, Vocabulary.AccessConditionName, new LiteralNode(dri.Name));
        logger.RecordBuilt(dri.Id);

        return Task.FromResult((Graph?)graph);
    }
}
