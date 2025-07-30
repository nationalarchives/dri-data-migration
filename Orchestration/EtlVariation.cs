using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlVariation(ILogger<EtlVariation> logger, IDriExporter driExport,
    IStagingIngest<DriVariation> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        int offset = 0;
        IEnumerable<DriVariation> dri;
        do
        {
            dri = await driExport.GetVariationsByCodeAsync(code, limit, offset, cancellationToken);
            offset += limit;
            logger.IngestingVariations(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedVariations(ingestSize);
        } while (dri.Any() && dri.Count() == limit);
    }
}
