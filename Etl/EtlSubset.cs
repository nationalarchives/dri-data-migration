using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSubset(ILogger<EtlSubset> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriSubset> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.Subset;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        IEnumerable<DriSubset> dri;
        do
        {
            dri = await driExport.GetSubsetsByCodeAsync(offset, cancellationToken);
            offset += settings.FetchPageSize;
            logger.IngestingSubsets(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSubsets(ingestSize);
        } while (dri.Any() && dri.Count() == settings.FetchPageSize);
    }
}
