using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlSubset(ILogger<EtlSubset> logger, IDriExporter driExport,
    IStagingIngest<DriSubset> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        var dri = await driExport.GetBroadestSubsetsAsync(cancellationToken);
        logger.IngestingBroadestSubsets(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedBroadestSubsets(ingestSize);

        int offset = 0;
        do
        {
            dri = await driExport.GetSubsetsByCodeAsync(code, limit, offset, cancellationToken);
            offset += limit;
            logger.IngestingSubsets(dri.Count());
            ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSubsets(ingestSize);
        } while (dri.Any() && dri.Count() == limit);
    }
}
