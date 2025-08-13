using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAccessCondition(ILogger<EtlAccessCondition> logger, IDriExporter driExport,
    IStagingIngest<DriAccessCondition> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        var dri = await driExport.GetAccessConditionsAsync(cancellationToken);

        logger.IngestingAccessConditions(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedAccessConditions(ingestSize);
    }
}
