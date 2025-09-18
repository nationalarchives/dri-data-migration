using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlAsset(ILogger<EtlAsset> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriAsset> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.Asset;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriAsset> dri;
        do
        {
            dri = (await driExport.GetAssetsByCodeAsync(offset, cancellationToken)).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingAssets(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedAssets(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
