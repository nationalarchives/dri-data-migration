using Api;
using Microsoft.Extensions.Logging;

namespace Orchestration;

public class EtlSensitivityReview(ILogger<EtlSensitivityReview> logger, IDriExport driExport,
    IStagingIngest<DriSensitivityReview> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit)
    {
        List<DriSensitivityReview> dri = [];
        int offset = 0;
        IEnumerable<DriSensitivityReview> page;
        do
        {
            page = await driExport.GetSensitivityReviewsByCodeAsync(code, limit, offset);
            dri.AddRange(page);
            offset += limit;
        } while (page.Any() && page.Count() == limit);

        logger.IngestingSensitivityReview(dri.Count);
        var ingestSize = await ingest.SetAsync(dri);
        logger.IngestedSensitivityReview(ingestSize);
    }
}
