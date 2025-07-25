using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlAsset(ILogger<EtlAsset> logger, IDriExporter driExport,
    IStagingIngest<DriAsset> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        List<DriAsset> dri = [];
        int offset = 0;
        IEnumerable<DriAsset> page;
        do
        {
            page = await driExport.GetAssetsByCodeAsync(code, limit, offset);
            dri.AddRange(page);
            offset += limit;
        } while (page.Any() && page.Count() == limit);

        logger.IngestingAssets(dri.Count);
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedAssets(ingestSize);
    }
}
