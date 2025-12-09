using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDriRdfExporter
{
    Task<IEnumerable<Uri>> GetListAsync(EtlStageType etlStageType, CancellationToken cancellationToken);
    Task<DriAccessCondition> GetAccessConditionAsync(Uri id, CancellationToken cancellationToken);
    Task<DriLegislation> GetLegislationAsync(Uri id, CancellationToken cancellationToken);
    Task<DriGroundForRetention> GetGroundForRetentionAsync(Uri id, CancellationToken cancellationToken);
    Task<DriSubset> GetSubsetAsync(Uri id, CancellationToken cancellationToken);
    Task<DriAsset> GetAssetAsync(Uri id, CancellationToken cancellationToken);
    Task<DriVariation> GetVariationAsync(Uri id, CancellationToken cancellationToken);
    Task<DriSensitivityReview> GetSensitivityReviewAsync(Uri id, CancellationToken cancellationToken);
}