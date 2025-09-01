using Api;
using Microsoft.Extensions.Logging;
using Rdf;
using VDS.RDF;

namespace Staging;

public abstract class StagingIngest<T> : IStagingIngest<T> where T : IDriRecord
{
    internal readonly ISparqlClient sparqlClient;
    private readonly ILogger logger;
    private readonly string graphSparql;

    protected StagingIngest(ISparqlClient sparqlClient, ILogger logger, string sparqlFileName)
    {
        this.sparqlClient = sparqlClient;
        this.logger = logger;

        var currentAssembly = typeof(StagingIngest<>).Assembly;
        var baseName = $"{typeof(StagingIngest<>).Namespace}.Sparql";
        var embedded = new EmbeddedSparqlResource(currentAssembly, baseName);
        graphSparql = embedded.GetSparql(sparqlFileName);
    }

    internal virtual Task<Graph?> BuildAsync(IGraph existing, T dri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    internal virtual void PostIngest() { } //TODO: temp, to be removed

    public async Task<int> SetAsync(IEnumerable<T> records, CancellationToken cancellationToken)
    {
        var total = 0;
        foreach (var dri in records)
        {
            var existing = await sparqlClient.GetGraphAsync(graphSparql, new Dictionary<string, object> { { "id", dri.Id } }, cancellationToken);
            var proposed = await BuildAsync(existing, dri, cancellationToken);
            if (proposed is null)
            {
                logger.RecordNotIngestedNoGraph(dri.Id);
                continue;
            }
            var diff = existing.Difference(proposed);
            if (!diff.AddedTriples.Any() && !diff.RemovedTriples.Any())
            {
                continue;
            }

            await sparqlClient.ApplyDiffAsync(diff, cancellationToken);
            total++;
            logger.RecordUpdated(dri.Id);
        }
        PostIngest();
        return total;
    }
}
