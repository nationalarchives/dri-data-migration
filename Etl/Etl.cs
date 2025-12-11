using Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public abstract class Etl<T>(ILogger logger, IStagingIngest<T> ingest) where T : IDriRecord
{
    private readonly Dictionary<Type, EtlStageType> stageTypes = new()
    {
        [typeof(DriAccessCondition)] = EtlStageType.AccessCondition,
        [typeof(DriLegislation)] = EtlStageType.Legislation,
        [typeof(DriGroundForRetention)] = EtlStageType.GroundForRetention,
        [typeof(DriSubset)] = EtlStageType.Subset,
        [typeof(DriAsset)] = EtlStageType.Asset,
        [typeof(DriVariation)] = EtlStageType.Variation,
        [typeof(DriAssetDeliverableUnit)] = EtlStageType.AssetDeliverableUnit,
        [typeof(DriWo409SubsetDeliverableUnit)] = EtlStageType.Wo409SubsetDeliverableUnit,
        [typeof(DriVariationFile)] = EtlStageType.VariationFile,
        [typeof(DriSensitivityReview)] = EtlStageType.SensitivityReview,
        [typeof(DriChange)] = EtlStageType.Change
    };

    public virtual EtlStageType StageType => stageTypes[typeof(T)];

    internal abstract Task<IEnumerable<T>> GetAsync(CancellationToken cancellationToken);

    internal abstract Task<IEnumerable<T>> GetAsync(int offset, CancellationToken cancellationToken);

    internal virtual async Task EtlAsync(CancellationToken cancellationToken)
    {
        var dri = (await GetAsync(cancellationToken)).ToList();

        logger.FoundRecords(dri.Count);
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedRecords(ingestSize);
    }

    internal virtual async Task EtlAsync(int offset, int fetchPageSize, CancellationToken cancellationToken)
    {
        List<T> dri;
        do
        {
            dri = (await GetAsync(offset, cancellationToken)).ToList();
            offset += fetchPageSize;
            logger.FoundRecords(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedRecords(ingestSize);
        } while (dri.Any() && dri.Count == fetchPageSize);
    }
}
