using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlSubset(ILogger<EtlSubset> logger, IDriExport driExport,
    IStagingIngest<DriSubset> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        var dri = (await driExport.GetBroadestSubsetsAsync()).ToList();
        int offset = 0;
        IEnumerable<DriSubset> page;
        do
        {
            page = await driExport.GetSubsetsByCodeAsync(code, limit, offset);
            dri.AddRange(page);
            offset += limit;
        } while (page.Any() && page.Count() == limit);

        logger.IngestingSubsets(dri.Count);
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedSubsets(ingestSize);
    }
}
