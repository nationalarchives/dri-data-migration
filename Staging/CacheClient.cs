using Api;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    private readonly string languageSparql;
    private readonly string formalBodySparql;
    private readonly string copyrightSparql;
    private readonly string geographicalPlaceSparql;
    private readonly string sealCategorySparql;
    private readonly string operatorSparql;
    private readonly string battalionSparql;
    private readonly string evidenceProviderSparql;
    private readonly string investigationSparql;
    private readonly string witnessSparql;

    private readonly string accessConditionsSparql;
    private readonly string legislationsSparql;
    private readonly string groundsForRetentionSparql;

    private static readonly Uri idNamespace = new("http://id.example.com/");

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
        var embedded = new EmbeddedResource(currentAssembly, baseName);

        srSparql = embedded.GetSparql("GetSensitivityReview");
        assetSparql = embedded.GetSparql("GetAsset");
        subsetSparql = embedded.GetSparql("GetSubset");
        variationSparql = embedded.GetSparql("GetVariation");
        languageSparql = embedded.GetSparql("GetLanguage");
        formalBodySparql = embedded.GetSparql("GetFormalBody");
        copyrightSparql = embedded.GetSparql("GetCopyright");
        geographicalPlaceSparql = embedded.GetSparql("GetGeographicalPlace");
        sealCategorySparql = embedded.GetSparql("GetSealCategory");
        operatorSparql = embedded.GetSparql("GetOperator");
        battalionSparql = embedded.GetSparql("GetBattalion");
        evidenceProviderSparql = embedded.GetSparql("GetEvidenceProvider");
        investigationSparql = embedded.GetSparql("GetInvestigation");
        witnessSparql = embedded.GetSparql("GetWitness");

        accessConditionsSparql = embedded.GetSparql("GetAccessConditions");
        legislationsSparql = embedded.GetSparql("GetLegislations");
        groundsForRetentionSparql = embedded.GetSparql("GetGroundsForRetention");
    }

    public void CacheCreate(CacheEntityKind kind, string key, object value)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
        }
        else
        {
            cache.GetOrCreate(info.Key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                return value;
            });
        }
    }
    
    public object? CacheFetch(CacheEntityKind kind, string key)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return null;
        }

        return cache.Get(info.Key);
    }

    public async Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return null;
        }

        var item = (IUriNode?)cache.Get(info.Key) ?? await sparqlClient.GetSubjectAsync(
            info.Sparql, new Dictionary<string, string> { ["id"] = key }, cancellationToken);

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

    public async Task<IUriNode?> CacheFetchOrNew(CacheEntityKind kind, string key, IUriNode predicate, CancellationToken cancellationToken)
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
            var subject = await sparqlClient.GetSubjectAsync(
                info.Sparql, new Dictionary<string, string> { ["id"] = key }, cancellationToken);
            if (subject is null)
            {
                subject = NewId;
                var node = new LiteralNode(key);
                var triple = new Triple(subject, predicate, node);
                await sparqlClient.UpdateAsync(triple, cancellationToken);
            }

            return subject!;
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
        if (results.Count != 0)
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
        CacheEntityKind.SensitivityReview => new(srSparql, $"sensitivity-review-{key}"),
        CacheEntityKind.Subset => new(subsetSparql, $"subset-{key}"),
        CacheEntityKind.Variation => new(variationSparql, $"variation-{key}"),
        CacheEntityKind.Language => new(languageSparql, $"language-{key}"),
        CacheEntityKind.FormalBody => new(formalBodySparql, $"formal-body-{key}"),
        CacheEntityKind.Copyright => new(copyrightSparql, $"copyright-{key}"),
        CacheEntityKind.GeographicalPlace => new(geographicalPlaceSparql, $"geographical-place-{key}"),
        CacheEntityKind.SealCategory => new(sealCategorySparql, $"seal-category-{key}"),
        CacheEntityKind.Operator => new(operatorSparql, $"operator-{key}"),
        CacheEntityKind.Battalion => new(battalionSparql, $"battalion-{key}"),
        CacheEntityKind.AssetRelation => new(string.Empty, $"asset-relation-{key}"),
        CacheEntityKind.EvidenceProvider => new(evidenceProviderSparql, $"evidence-provider-{key}"),
        CacheEntityKind.Investigation => new(investigationSparql, $"investigation-{key}"),
        CacheEntityKind.Witness => new(witnessSparql, $"witness-{key}"),
        _ => null
    };

    private sealed record CacheFetchInfo(string Sparql, string Key);

    public static IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));
}
