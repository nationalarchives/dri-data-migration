using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlLegislation(ILogger<EtlLegislation> logger, IDriExporter driExport,
    IStagingIngest<DriLegislation> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        var dri = await driExport.GetLegislationsAsync();

        logger.IngestingLegislations(dri.Count());
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedLegislations(ingestSize);
    }
}
