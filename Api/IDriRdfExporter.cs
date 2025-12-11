using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDriRdfExporter
{
    Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriAsset>> GetAssetsAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriLegislation>> GetLegislationsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriSubset>> GetSubsetsAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriVariation>> GetVariationsAsync(int offset, CancellationToken cancellationToken);
}