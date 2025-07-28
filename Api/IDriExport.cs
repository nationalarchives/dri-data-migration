using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDriExporter
{
    Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriAsset>> GetAssetsByCodeAsync(string code, int pageSize, int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriSubset>> GetBroadestSubsetsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriLegislation>> GetLegislationsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCodeAsync(string code, int pageSize, int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriSubset>> GetSubsetsByCodeAsync(string code, int pageSize, int offset, CancellationToken cancellationToken);
    Task<IEnumerable<DriVariation>> GetVariationsByCodeAsync(string code, int pageSize, int offset, CancellationToken cancellationToken);
}