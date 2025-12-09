using Api;
using Microsoft.Extensions.Logging;
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
        var embedded = new EmbeddedResource(currentAssembly, baseName);
        graphSparql = embedded.GetSparql(sparqlFileName);
    }

    internal virtual Task<Graph?> BuildAsync(IGraph existing, T dri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SetAsync(T record, CancellationToken cancellationToken)
    {
        var existing = await sparqlClient.GetGraphAsync(graphSparql, record.Id, cancellationToken);
        logger.BuildingRecord();
        var proposed = await BuildAsync(existing, record, cancellationToken);
        logger.RecordBuilt();
        if (proposed is null)
        {
            logger.RecordNotIngestedNoGraph();
            return false;
        }
        var diff = existing.Difference(proposed);
        if (!diff.AddedTriples.Any() && !diff.RemovedTriples.Any())
        {
            return false;
        }

        await sparqlClient.ApplyDiffAsync(diff, cancellationToken);
        logger.RecordUpdated();
        return true;
    }

    internal static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;
}
