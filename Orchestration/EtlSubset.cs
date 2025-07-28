using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlSubset(ILogger<EtlSubset> logger, IDriExporter driExport,
    IStagingIngest<DriSubset> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        var dri = (await driExport.GetBroadestSubsetsAsync(cancellationToken)).ToList();
        int offset = 0;
        IEnumerable<DriSubset> page;
        do
        {
            page = await driExport.GetSubsetsByCodeAsync(code, limit, offset, cancellationToken);
            dri.AddRange(page);
            offset += limit;
        } while (page.Any() && page.Count() == limit);

        logger.IngestingSubsets(dri.Count);
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedSubsets(ingestSize);
    }
}
