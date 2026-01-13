using VDS.RDF;

namespace Staging;

public interface IAssetDeliverableUnitRelation
{
    Task AddAssetRelationAsync(IGraph graph, IGraph rdf, IUriNode id, ICacheClient cacheClient, CancellationToken cancellationToken);
}