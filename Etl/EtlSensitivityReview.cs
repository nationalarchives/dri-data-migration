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

    public EtlStageType StageType => EtlStageType.SensitivityReview;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriSensitivityReview> dri;
        do
        {
            dri = (await driExport.GetSensitivityReviewsByCodeAsync(offset, cancellationToken)).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingSensitivityReview(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedSensitivityReview(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
