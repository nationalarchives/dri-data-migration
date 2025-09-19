using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDriRdfExporter
{
    Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriAsset>> GetAssetsByCodeAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriLegislation>> GetLegislationsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCodeAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriSubset>> GetSubsetsByCodeAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriVariation>> GetVariationsByCodeAsync(int offset, CancellationToken cancellationToken);
}