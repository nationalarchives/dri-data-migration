using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Staging;

public class LegislationIngest(ISparqlClient sparqlClient, ILogger<LegislationIngest> logger)
    : BaseStagingIngest<DriLegislation>(sparqlClient, logger, "LegislationGraph")
{
    internal override Task<Graph?> BuildAsync(IGraph existing, DriLegislation dri, CancellationToken cancellationToken)
    {
        logger.BuildingRecord(dri.Id);
        var legislation = new UriNode(dri.Link);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.LegislationHasUkLegislation, legislation).FirstOrDefault()?.Subject ?? BaseIngest.NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.LegislationHasUkLegislation, legislation);
        if (!string.IsNullOrEmpty(dri.Section))
        {
            graph.Assert(id, Vocabulary.LegislationSectionReference, new LiteralNode(dri.Section));
        }
        logger.RecordBuilt(dri.Id);

        return Task.FromResult((Graph?)graph);
    }

}
