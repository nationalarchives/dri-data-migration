using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariation(ILogger<EtlVariation> logger, IOptions<DriSettings> driSettings, 
    IDriRdfExporter driExport, IStagingIngest<DriVariation> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.Variation;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        IEnumerable<DriVariation> dri;
        do
        {
            dri = await driExport.GetVariationsByCodeAsync(offset, cancellationToken);
            offset += settings.FetchPageSize;
            logger.IngestingVariations(dri.Count());
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedVariations(ingestSize);
        } while (dri.Any() && dri.Count() == settings.FetchPageSize);
    }
}
