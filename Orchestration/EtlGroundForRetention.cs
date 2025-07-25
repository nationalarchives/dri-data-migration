using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriExport driExport,
    IStagingIngest<DriGroundForRetention> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        var dri = await driExport.GetGroundsForRetentionAsync();

        logger.IngestingGroundsForRetention(dri.Count());
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedGroundsForRetention(ingestSize);
    }
}
