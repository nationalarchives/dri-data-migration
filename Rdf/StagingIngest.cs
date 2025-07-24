using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace Rdf;

public abstract class StagingIngest<T> : IStagingIngest<T> where T : IDriRecord
{
    private readonly IMemoryCache cache;
    internal readonly ISparqlClient sparqlClient;
    internal readonly EmbeddedSparqlResource embedded;
    private readonly ILogger logger;
    private readonly string graphSparql;
    private readonly Uri idNamespace = new("http://id.example.com/");
    private readonly string srSparql;
    private readonly string assetSparql;
    private readonly string subsetSparql;
    private readonly string variationSparql;
    private readonly string retentionSparql;

    protected StagingIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger, string sparqlFileName)
    {
        this.cache = cache;
        this.sparqlClient = sparqlClient;
        this.logger = logger;

        var currentAssembly = typeof(StagingIngest<>).Assembly;
        var baseName = $"{typeof(StagingIngest<>).Namespace}.Sparql.Staging";
        embedded = new(currentAssembly, baseName);

        graphSparql = embedded.GetSparql(sparqlFileName);
        srSparql = embedded.GetSparql("GetSensitivityReview");
        assetSparql = embedded.GetSparql("GetAsset");
        subsetSparql = embedded.GetSparql("GetSubset");
        variationSparql = embedded.GetSparql("GetVariation");
        retentionSparql = embedded.GetSparql("GetRetention");
    }

    public async Task SetAsync(IEnumerable<T> dri)
    {
        foreach (var item in dri)
        {
            var existing = await sparqlClient.GetGraphAsync(graphSparql, new Dictionary<string, object> { { "id", item.Id } });
            var proposed = await BuildAsync(existing, item);
            var diff = existing.Difference(proposed);
            if (!diff.AddedTriples.Any() && !diff.RemovedTriples.Any())
            {
                continue;
            }

            await sparqlClient.ApplyDiffAsync(diff);
            logger.RecordUpdated(item.Id);
        }
    }

    private CacheFetchInfo? ToCacheFetchInfo(CacheEntityKind kind, string key) => kind switch
    {
        CacheEntityKind.Asset => new(assetSparql, $"asset-{key}"),
        CacheEntityKind.SensititvityReview => new(srSparql, $"sensititvity-review-{key}"),
        CacheEntityKind.Subset => new(subsetSparql, $"subset-{key}"),
        CacheEntityKind.Variation => new(variationSparql, $"variation-{key}"),
        CacheEntityKind.Retention => new(retentionSparql, $"retention-{key}"),
        _ => null //TODO: handle null
    };

    internal async Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key)
    {
        var info = ToCacheFetchInfo(kind, key);
        //TODO: handle null
        var item = (IUriNode?)cache.Get(info.Key) ?? await sparqlClient.GetSubjectAsync(info.Sparql, key);

        if (item is null)
        {
            return null;
        }
        return cache.GetOrCreate(info.Key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return item;
        });
    }

    internal async Task<IUriNode> CacheFetchOrNew(CacheEntityKind kind, string key)
    {
        var info = ToCacheFetchInfo(kind, key);
        //TODO: handle null
        return await cache.GetOrCreateAsync(info.Key, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var subject = await sparqlClient.GetSubjectAsync(info.Sparql, info.Key);
            return subject is null ? NewId : subject;
        });
    }

    internal IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));

    internal static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.Substring(1) : null;

    internal virtual Task<Graph> BuildAsync(IGraph existing, T dri)
    {
        throw new NotImplementedException();
    }

    private sealed record CacheFetchInfo(string Sparql, string Key);

    internal enum CacheEntityKind
    {
        Asset,
        SensititvityReview,
        Subset,
        Variation,
        Retention
    }
}
