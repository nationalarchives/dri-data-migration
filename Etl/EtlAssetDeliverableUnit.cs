using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAssetDeliverableUnit(ILogger<EtlAssetDeliverableUnit> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriAssetDeliverableUnit> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.AssetDeliverableUnit;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriAssetDeliverableUnit> dri;
        do
        {
            dri = driExport.GetAssetDeliverableUnits(offset, cancellationToken).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingDeliverableUnits(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedDeliverableUnits(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
