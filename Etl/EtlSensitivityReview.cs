using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlSensitivityReview(ILogger<EtlSensitivityReview> logger, IOptions<DriSettings> driSettings,
    IDriRdfExporter driExport, IStagingIngest<DriSensitivityReview> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int offset = 0;
        IEnumerable<DriSensitivityReview> dri;
        do
        {
            dri = await driExport.GetSensitivityReviewsByCodeAsync(offset, cancellationToken);
            offset += settings.FetchPageSize;
            logger.IngestingSensitivityReview(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSensitivityReview(ingestSize);
        } while (dri.Any() && dri.Count() == settings.FetchPageSize);
    }
}
