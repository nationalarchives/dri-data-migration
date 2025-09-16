using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAccessCondition(ILogger<EtlAccessCondition> logger,
    IDriRdfExporter driExport, IStagingIngest<DriAccessCondition> ingest) : IEtl
{
    public EtlStageType StageType => EtlStageType.AccessCondition;

    public async Task RunAsync(int _, CancellationToken cancellationToken)
    {
        var dri = await driExport.GetAccessConditionsAsync(cancellationToken);

        logger.IngestingAccessConditions(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedAccessConditions(ingestSize);
    }
}
