using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSubset(ILogger<EtlSubset> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriSubset> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var dri = await driExport.GetBroadestSubsetsAsync(cancellationToken);
        logger.IngestingBroadestSubsets(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedBroadestSubsets(ingestSize);

        int offset = 0;
        do
        {
            dri = await driExport.GetSubsetsByCodeAsync(offset, cancellationToken);
            offset += settings.FetchPageSize;
            logger.IngestingSubsets(dri.Count());
            ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSubsets(ingestSize);
        } while (dri.Any() && dri.Count() == settings.FetchPageSize);
    }
}
