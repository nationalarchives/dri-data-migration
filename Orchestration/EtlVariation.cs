using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlVariation(ILogger<EtlVariation> logger, IDriExport driExport,
    IStagingIngest<DriVariation> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        List<DriVariation> dri = [];
        int offset = 0;
        IEnumerable<DriVariation> page;
        do
        {
            page = await driExport.GetVariationsByCodeAsync(code, limit, offset);
            dri.AddRange(page);
            offset += limit;
        } while (page.Any() && page.Count() == limit);

        logger.IngestingVariations(dri.Count);
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedVariations(ingestSize);
    }
}
