using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Staging;

public abstract class BaseStagingIngest<T> : IStagingIngest<T> where T : IDriRecord
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
    private readonly string languageSparql;
    private readonly string formalBodySparql;
    private readonly string copyrightSparql;

    protected BaseStagingIngest(IMemoryCache cache, ISparqlClient sparqlClient, ILogger logger, string sparqlFileName)
    {
        this.cache = cache;
        this.sparqlClient = sparqlClient;
        this.logger = logger;

        var currentAssembly = typeof(BaseStagingIngest<>).Assembly;
        var baseName = $"{typeof(BaseStagingIngest<>).Namespace}.Sparql";
        embedded = new(currentAssembly, baseName);

        graphSparql = embedded.GetSparql(sparqlFileName);
        srSparql = embedded.GetSparql("GetSensitivityReview");
        assetSparql = embedded.GetSparql("GetAsset");
        subsetSparql = embedded.GetSparql("GetSubset");
        variationSparql = embedded.GetSparql("GetVariation");
        retentionSparql = embedded.GetSparql("GetRetention");
        languageSparql = embedded.GetSparql("GetLanguage");
        formalBodySparql = embedded.GetSparql("GetFormalBody");
        copyrightSparql = embedded.GetSparql("GetCopyright");
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
        return total;
    }

    private CacheFetchInfo? ToCacheFetchInfo(CacheEntityKind kind, string key) => kind switch
    {
        CacheEntityKind.Asset => new(assetSparql, $"asset-{key}"),
        CacheEntityKind.SensititvityReview => new(srSparql, $"sensititvity-review-{key}"),
        CacheEntityKind.Subset => new(subsetSparql, $"subset-{key}"),
        CacheEntityKind.Variation => new(variationSparql, $"variation-{key}"),
        CacheEntityKind.Retention => new(retentionSparql, $"retention-{key}"),
        CacheEntityKind.Language => new(languageSparql, $"language-{key}"),
        CacheEntityKind.FormalBody => new(formalBodySparql, $"formal-body-{key}"),
        CacheEntityKind.Copyright => new(copyrightSparql, $"copyright-{key}"),
        _ => null
    };

    internal async Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return null;
        }

        var item = (IUriNode?)cache.Get(info.Key) ?? await sparqlClient.GetSubjectAsync(info.Sparql, key, cancellationToken);

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

    internal async Task<IUriNode> CacheFetchOrNew(CacheEntityKind kind, string key, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return NewId;
        }

        return await cache.GetOrCreateAsync(info.Key, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var subject = await sparqlClient.GetSubjectAsync(info.Sparql, key, cancellationToken);
            
            return subject ?? NewId;
        });
    }

    internal IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));

    internal static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;

    private sealed record CacheFetchInfo(string Sparql, string Key);

    internal enum CacheEntityKind
    {
        Asset,
        SensititvityReview,
        Subset,
        Variation,
        Retention,
        Language,
        FormalBody,
        Copyright
    }
}
