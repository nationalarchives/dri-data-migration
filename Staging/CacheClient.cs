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

public class CacheClient : ICacheClient
{
    private readonly ILogger<CacheClient> logger;
    private readonly IMemoryCache cache;
    private readonly ISparqlClient sparqlClient;
    private readonly string srSparql;
    private readonly string assetSparql;
    private readonly string subsetSparql;
    private readonly string variationSparql;
    private readonly string retentionSparql;
    private readonly string languageSparql;
    private readonly string formalBodySparql;
    private readonly string copyrightSparql;
    private readonly string causingSoftwareSparql;
    private readonly string variationByAssetAndPartialPathSparql;

    private readonly string accessConditionsSparql;
    private readonly string legislationsSparql;
    private readonly string groundsForRetentionSparql;

    private Dictionary<string, IUriNode>? accessConditions = [];
    private Dictionary<string, IUriNode>? legislations = [];
    private Dictionary<string, IUriNode>? groundsForRetention = [];

    public CacheClient(ILogger<CacheClient> logger, IMemoryCache cache, ISparqlClient sparqlClient)
    {
        this.logger = logger;
        this.cache = cache;
        this.sparqlClient = sparqlClient;

        var currentAssembly = typeof(CacheClient).Assembly;
        var baseName = $"{typeof(CacheClient).Namespace}.Sparql";
        var embedded = new EmbeddedSparqlResource(currentAssembly, baseName);

        srSparql = embedded.GetSparql("GetSensitivityReview");
        assetSparql = embedded.GetSparql("GetAsset");
        subsetSparql = embedded.GetSparql("GetSubset");
        variationSparql = embedded.GetSparql("GetVariation");
        retentionSparql = embedded.GetSparql("GetRetention");
        languageSparql = embedded.GetSparql("GetLanguage");
        formalBodySparql = embedded.GetSparql("GetFormalBody");
        copyrightSparql = embedded.GetSparql("GetCopyright");
        causingSoftwareSparql = embedded.GetSparql("GetCausingSoftware");
        variationByAssetAndPartialPathSparql = embedded.GetSparql("GetVariationByAssetAndPartialPath");

        accessConditionsSparql = embedded.GetSparql("GetAccessConditions");
        legislationsSparql = embedded.GetSparql("GetLegislations");
        groundsForRetentionSparql = embedded.GetSparql("GetGroundsForRetention");
    }

    public async Task<IUriNode?> CacheFetch(CacheEntityKind kind, IEnumerable<string> keys, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, string.Join('|', keys));
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return null;
        }

        Dictionary<string, object> parameters;
        if (keys.Count() == 1)
        {
            parameters = new Dictionary<string, object> { ["id"] = keys.First() };
        }
        else
        {
            parameters = keys.Select((k, i) => new KeyValuePair<string, object>($"id{i}", k)).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        var item = (IUriNode?)cache.Get(info.Key) ?? await sparqlClient.GetSubjectAsync(info.Sparql, parameters, cancellationToken);

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

    public Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key, CancellationToken cancellationToken) =>
        CacheFetch(kind, [key], cancellationToken);

    public async Task<IUriNode> CacheFetchOrNew(CacheEntityKind kind, string key, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return BaseIngest.NewId;
        }

        return await cache.GetOrCreateAsync(info.Key, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            var subject = await sparqlClient.GetSubjectAsync(info.Sparql, new Dictionary<string, object> { ["id"] = key }, cancellationToken);

            return subject ?? BaseIngest.NewId;
        });
    }

    public async Task<Dictionary<string, IUriNode>> AccessConditions(CancellationToken cancellationToken)
    {
        accessConditions = await GetDictionaryAsync(accessConditions!, accessConditionsSparql, cancellationToken);
        return accessConditions;
    }

    public async Task<Dictionary<string, IUriNode>> Legislations(CancellationToken cancellationToken)
    {
        legislations = await GetDictionaryAsync(legislations!, legislationsSparql, cancellationToken);
        return legislations;
    }

    public async Task<Dictionary<string, IUriNode>> GroundsForRetention(CancellationToken cancellationToken)
    {
        groundsForRetention = await GetDictionaryAsync(groundsForRetention!, groundsForRetentionSparql, cancellationToken);
        return groundsForRetention;
    }

    private async Task<Dictionary<string, IUriNode>> GetDictionaryAsync(Dictionary<string, IUriNode> results, string sparql, CancellationToken cancellationToken)
    {
        if (results.Any())
        {
            return results;
        }
        else
        {
            return await sparqlClient.GetDictionaryAsync(sparql, cancellationToken);
        }
    }

    internal static string? GetUriFragment(Uri? uri) => uri?.Fragment.Length > 1 ? uri.Fragment.TrimStart('#') : null;

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
        CacheEntityKind.CausingSoftware => new(causingSoftwareSparql, $"causing-software-{key}"),
        CacheEntityKind.VariationByPartialPathAndAsset => new(variationByAssetAndPartialPathSparql, $"variation-{key}"),
        _ => null
    };

    private sealed record CacheFetchInfo(string Sparql, string Key);
}
