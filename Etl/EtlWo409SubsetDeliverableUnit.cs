using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlWo409SubsetDeliverableUnit(ILogger<EtlWo409SubsetDeliverableUnit> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriWo409SubsetDeliverableUnit> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.Wo409SubsetDeliverableUnit;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriWo409SubsetDeliverableUnit> dri;
        do
        {
            dri = driExport.GetWo409SubsetDeliverableUnits(offset, cancellationToken).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingWo409SubsetDeliverableUnits(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedWo409SubsetDeliverableUnits(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
