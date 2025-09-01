using Api;
using Microsoft.Extensions.Logging;
using VDS.RDF;

namespace Staging;

public class LegislationIngest(ISparqlClient sparqlClient, ILogger<LegislationIngest> logger, ICacheClient cacheClient)
    : StagingIngest<DriLegislation>(sparqlClient, logger, cacheClient, "LegislationGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriLegislation dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var legislation = new UriNode(dri.Link);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.LegislationHasUkLegislation, legislation).FirstOrDefault()?.Subject ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.LegislationHasUkLegislation, legislation);
        GraphAssert.Text(graph, id, dri.Section, Vocabulary.LegislationSectionReference);
        logger.RecordBuilt(dri.Id);

        return Task.FromResult((Graph?)graph);
    }

}
