using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public class LegislationIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger)
    : StagingIngest<DriLegislation>(cache, sparqlClient, logger, "LegislationGraph")
{
    internal override async Task<Graph> BuildAsync(IGraph existing, DriLegislation dri)
    {
        var legislation = new UriNode(dri.Link);
        var id = existing.GetTriplesWithPredicateObject(Vocabulary.LegislationHasUkLegislation, legislation).FirstOrDefault()?.Subject ?? NewId;

        var graph = new Graph();
        graph.Assert(id, Vocabulary.LegislationHasUkLegislation, legislation);
        if (!string.IsNullOrEmpty(dri.Section))
        {
            graph.Assert(id, Vocabulary.LegislationSectionReference, new LiteralNode(dri.Section));
        }

        return graph;
    }

}
