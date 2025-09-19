using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriRdfExporter driExport,
    IStagingIngest<DriGroundForRetention> ingest) : IEtl
{
    public EtlStageType StageType => EtlStageType.GroundForRetention;

    public async Task RunAsync(int _, CancellationToken cancellationToken)
    {
        var dri = (await driExport.GetGroundsForRetentionAsync(cancellationToken)).ToList();

        logger.IngestingGroundsForRetention(dri.Count);
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedGroundsForRetention(ingestSize);
    }
}
