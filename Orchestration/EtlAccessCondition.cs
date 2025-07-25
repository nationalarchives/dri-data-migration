using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlAccessCondition(ILogger<EtlAccessCondition> logger, IDriExporter driExport,
    IStagingIngest<DriAccessCondition> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        var dri = await driExport.GetAccessConditionsAsync();

        logger.IngestingAccessConditions(dri.Count());
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedAccessConditions(ingestSize);
    }
}
