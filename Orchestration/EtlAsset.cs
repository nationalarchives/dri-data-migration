using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlAsset(ILogger<EtlAsset> logger, IDriExporter driExport,
    IStagingIngest<DriAsset> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        int offset = 0;
        IEnumerable<DriAsset> dri;
        do
        {
            dri = await driExport.GetAssetsByCodeAsync(code, limit, offset, cancellationToken);
            offset += limit;
            logger.IngestingAssets(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedAssets(ingestSize);
        } while (dri.Any() && dri.Count() == limit);
    }
}
