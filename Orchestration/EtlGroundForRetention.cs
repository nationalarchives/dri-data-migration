using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriExporter driExport,
    IStagingIngest<DriGroundForRetention> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        var dri = await driExport.GetGroundsForRetentionAsync(cancellationToken);

        logger.IngestingGroundsForRetention(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedGroundsForRetention(ingestSize);
    }
}
