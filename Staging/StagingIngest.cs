using Api;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using VDS.RDF;

namespace Staging;

public abstract class StagingIngest<T> : IStagingIngest<T> where T : IDriRecord
{
    internal readonly ISparqlClient sparqlClient;
    private readonly ILogger logger;
    private readonly string graphSparql;
    private readonly Counter<int> ingestedMetric;
    private readonly Counter<int> processedMetric;
    private readonly Gauge<int> triplesAddedMetric;
    private readonly Gauge<int> triplesRemovedMetric;

    protected StagingIngest(ISparqlClient sparqlClient, ILogger logger, IMeterFactory meterFactory, string sparqlFileName)
    {
        this.sparqlClient = sparqlClient;
        this.logger = logger;

        var currentAssembly = typeof(StagingIngest<>).Assembly;
        var baseName = $"{typeof(StagingIngest<>).Namespace}.Sparql";
        var embedded = new EmbeddedResource(currentAssembly, baseName);
        graphSparql = embedded.GetSparql(sparqlFileName);
        
        var meter = meterFactory.Create("Migration.Staging");
        processedMetric = meter.CreateCounter<int>("staging.processed", description: "Number of processed records");
        ingestedMetric = meter.CreateCounter<int>("staging.ingested", description: "Number of ingested records");
        triplesAddedMetric = meter.CreateGauge<int>("staging.triples_added", description: "Number of added triples per record");
        triplesRemovedMetric = meter.CreateGauge<int>("staging.triples_removed", description: "Number of removed triples per record");
        
    }

    internal virtual Task<Graph?> BuildAsync(IGraph existing, T dri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<int> SetAsync(IEnumerable<T> records, CancellationToken cancellationToken)
    {
        var total = 0;
        foreach (var dri in records)
        {
            using (logger.BeginScope(("RecordId", dri.Id)))
            {
                var existing = await sparqlClient.GetGraphAsync(graphSparql, new Dictionary<string, object> { { "id", dri.Id } }, cancellationToken);
                logger.BuildingRecord();
                var proposed = await BuildAsync(existing, dri, cancellationToken);
                logger.RecordBuilt();
                if (proposed is null)
                {
                    logger.RecordNotIngestedNoGraph();
                    continue;
                }
                processedMetric.Add(1);
                var diff = existing.Difference(proposed);
                if (!diff.AddedTriples.Any() && !diff.RemovedTriples.Any())
                {
                    continue;
                }
                if (diff.AddedTriples.Any())
                {
                    triplesAddedMetric.Record(diff.AddedTriples.Count());
                }
                if (diff.RemovedTriples.Any())
                {
                    triplesRemovedMetric.Record(diff.RemovedTriples.Count());
                }

                await sparqlClient.ApplyDiffAsync(diff, cancellationToken);
                total++;
                ingestedMetric.Add(1);
                logger.RecordUpdated();
            }
        }
        return total;
    }
}
