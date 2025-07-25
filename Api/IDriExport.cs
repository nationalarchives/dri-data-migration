using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api;

public interface IDriExporter
{
    Task<IEnumerable<DriAccessCondition>> GetAccessConditionsAsync();
    Task<IEnumerable<DriAsset>> GetAssetsByCodeAsync(string code, int pageSize, int offset);
    Task<IEnumerable<DriSubset>> GetBroadestSubsetsAsync();
    Task<IEnumerable<DriGroundForRetention>> GetGroundsForRetentionAsync();
    Task<IEnumerable<DriLegislation>> GetLegislationsAsync();
    Task<IEnumerable<DriSensitivityReview>> GetSensitivityReviewsByCodeAsync(string code, int pageSize, int offset);
    Task<IEnumerable<DriSubset>> GetSubsetsByCodeAsync(string code, int pageSize, int offset);
    Task<IEnumerable<DriVariation>> GetVariationsByCodeAsync(string code, int pageSize, int offset);
}