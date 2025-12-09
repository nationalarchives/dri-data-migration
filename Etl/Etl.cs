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

    internal abstract T Get(string id, CancellationToken cancellationToken);

    internal abstract Task<T> GetAsync(Uri id, CancellationToken cancellationToken);

    internal virtual Task<bool> IngestAsync(T record, CancellationToken cancellationToken) =>
        ingest.SetAsync(record, cancellationToken);

    internal virtual async Task EtlRdfSourceAsync(IDriRdfExporter rdfExporter, int offset,
        CancellationToken cancellationToken)
    {
        var ids = (await rdfExporter.GetListAsync(StageType, cancellationToken)).ToList();
        logger.FoundRecords(ids.Count);
        if (offset > 0)
        {
            logger.SkippedRecords(offset);
        }
        int recordCount = 0;
        int ingestedRecord = 0;
        foreach (var id in ids.Skip(offset))
        {
            using (logger.BeginScope(("RecordId", id)))
            {
                var dri = await GetAsync(id, cancellationToken);
                var ingested = await IngestAsync(dri, cancellationToken);
                if (ingested)
                {
                    ingestedRecord++;
                }
                recordCount++;
                if (recordCount % 500 == 0)
                {
                    logger.IngestedRecords(ingestedRecord, recordCount);
                }
            }
        }
        if (recordCount % 500 != 0)
        {
            logger.IngestedRecords(ingestedRecord, recordCount);
        }
    }

    internal virtual async Task EtlSqlSourceAsync(IDriSqlExporter sqlExporter, int offset,
        CancellationToken cancellationToken)
    {
        var ids = (sqlExporter.GetList(StageType, cancellationToken)).ToList();
        logger.FoundRecords(ids.Count);
        if (offset > 0)
        {
            logger.SkippedRecords(offset);
        }
        int recordCount = 0;
        int ingestedRecord = 0;
        foreach (var id in ids.Skip(offset))
        {
            using (logger.BeginScope(("RecordId", id)))
            {
                var dri = Get(id, cancellationToken);
                var ingested = await IngestAsync(dri, cancellationToken);
                if (ingested)
                {
                    ingestedRecord++;
                }
                recordCount++;
                if (recordCount % 500 == 0)
                {
                    logger.IngestedRecords(ingestedRecord, recordCount);
                }
            }
        }
        if (recordCount % 500 != 0)
        {
            logger.IngestedRecords(ingestedRecord, recordCount);
        }
    }
}