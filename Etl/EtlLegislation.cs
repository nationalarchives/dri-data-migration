using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlLegislation(ILogger<EtlLegislation> logger, IDriRdfExporter driExport,
    IStagingIngest<DriLegislation> ingest) : IEtl
{
    public EtlStageType StageType => EtlStageType.Legislation;

    public async Task RunAsync(int _, CancellationToken cancellationToken)
    {
        var dri = (await driExport.GetLegislationsAsync(cancellationToken)).ToList();

        logger.IngestingLegislations(dri.Count);
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedLegislations(ingestSize);
    }
}
