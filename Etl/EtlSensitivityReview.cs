using Api;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSensitivityReview(ILogger<EtlSensitivityReview> logger, IDriExporter driExport,
    IStagingIngest<DriSensitivityReview> ingest) : IEtl
{
    public async Task RunAsync(string code, int limit, CancellationToken cancellationToken)
    {
        int offset = 0;
        IEnumerable<DriSensitivityReview> dri;
        do
        {
            dri = await driExport.GetSensitivityReviewsByCodeAsync(code, limit, offset, cancellationToken);
            offset += limit;
            logger.IngestingSensitivityReview(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSensitivityReview(ingestSize);
        } while (dri.Any() && dri.Count() == limit);
    }
}
