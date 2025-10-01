using VDS.RDF;

namespace Staging;

public interface ICacheClient
{
    Task<IUriNode?> CacheFetch(CacheEntityKind kind, IEnumerable<string> keys, CancellationToken cancellationToken);
    Task<IUriNode?> CacheFetch(CacheEntityKind kind, string key, CancellationToken cancellationToken);
    Task<IUriNode> CacheFetchOrNew(CacheEntityKind kind, IEnumerable<string> keys, IUriNode predicate, CancellationToken cancellationToken);
    Task<IUriNode> CacheFetchOrNew(CacheEntityKind kind, string key, IUriNode predicate, CancellationToken cancellationToken);

    Task<Dictionary<string, IUriNode>> AccessConditions(CancellationToken cancellationToken);
    Task<Dictionary<string, IUriNode>> Legislations(CancellationToken cancellationToken);
    Task<Dictionary<string, IUriNode>> GroundsForRetention(CancellationToken cancellationToken);
}
