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
    private readonly string getSparql;
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

        getSparql = embedded.GetSparql("Get");
        accessConditionsSparql = embedded.GetSparql("GetAccessConditions");
        legislationsSparql = embedded.GetSparql("GetLegislations");
        groundsForRetentionSparql = embedded.GetSparql("GetGroundsForRetention");
    }

    public object? CacheCreate(string key, object value) =>
        cache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return value;
        });

    public object? CacheFetch(string key) => cache.Get(key);

    public async Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key, CancellationToken cancellationToken)
    {
        var info = ToCacheFetchInfo(kind, key);
        if (info is null)
        {
            logger.InvalidCacheEntityKind();
            return null;
        }

        var item = (IUriNode?)cache.Get(info.Key) ?? await sparqlClient.GetSubjectAsync(
            getSparql, new Dictionary<string, object> { ["id"] = key, ["predicate"] = info.Predicate.Uri }, cancellationToken);

        if (item is null)
        {
            return null;
        }
        return CacheCreate(info.Key, item) as IUriNode;
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
                getSparql, new Dictionary<string, object> { ["id"] = key, ["predicate"] = info.Predicate.Uri }, cancellationToken);
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

    private static CacheFetchInfo? ToCacheFetchInfo(CacheEntityKind kind, string key) => kind switch
    {
        CacheEntityKind.Asset => new(Vocabulary.AssetReference, $"asset-{key}"),
        CacheEntityKind.AssetDri => new(Vocabulary.AssetDriId, $"asset-dri-{key}"),
        CacheEntityKind.SensitivityReview => new(Vocabulary.SensitivityReviewDriId, $"sensitivity-review-{key}"),
        CacheEntityKind.Subset => new(Vocabulary.SubsetReference, $"subset-{key}"),
        CacheEntityKind.Variation => new(Vocabulary.VariationDriId, $"variation-{key}"),
        CacheEntityKind.Language => new(Vocabulary.LanguageName, $"language-{key}"),
        CacheEntityKind.FormalBody => new(Vocabulary.FormalBodyName, $"formal-body-{key}"),
        CacheEntityKind.Copyright => new(Vocabulary.CopyrightTitle, $"copyright-{key}"),
        CacheEntityKind.GeographicalPlace => new(Vocabulary.GeographicalPlaceName, $"geographical-place-{key}"),
        CacheEntityKind.SealCategory => new(Vocabulary.SealCategoryName, $"seal-category-{key}"),
        CacheEntityKind.Operator => new(Vocabulary.OperatorIdentifier, $"operator-{key}"),
        CacheEntityKind.Battalion => new(Vocabulary.BattalionName, $"battalion-{key}"),
        CacheEntityKind.EvidenceProvider => new(Vocabulary.InquiryEvidenceProviderName, $"evidence-provider-{key}"),
        CacheEntityKind.Investigation => new(Vocabulary.InquiryInvestigationName, $"investigation-{key}"),
        CacheEntityKind.Witness => new(Vocabulary.InquiryWitnessName, $"witness-{key}"),
        CacheEntityKind.NavyDivision => new(Vocabulary.NavyDivisionName, $"navy-division-{key}"),
        _ => null
    };

    private sealed record CacheFetchInfo(IUriNode Predicate, string Key);

    public static IUriNode NewId => new UriNode(new Uri(idNamespace, Guid.NewGuid().ToString()));
}
