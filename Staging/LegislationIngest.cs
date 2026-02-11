using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging;

public class LegislationIngest(ISparqlClient sparqlClient, ILogger<LegislationIngest> logger, IMeterFactory meterFactory) :
    StagingIngest<DriLegislation>(sparqlClient, logger, meterFactory, "LegislationGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriLegislation dri, CancellationToken cancellationToken)
    {
        var legislation = new UriNode(dri.Link);
        var id = existing.GetSingleUriNodeSubject(Vocabulary.LegislationHasUkLegislation, legislation) ?? CacheClient.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.LegislationHasUkLegislation, legislation);
        GraphAssert.Text(graph, id, dri.Section, Vocabulary.LegislationSectionReference);

        return Task.FromResult((Graph?)graph);
    }

}
