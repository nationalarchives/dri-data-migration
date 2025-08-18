using Api;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlGroundForRetention(ILogger<EtlGroundForRetention> logger, IDriRdfExporter driExport,
    IStagingIngest<DriGroundForRetention> ingest) : IEtl
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var dri = await driExport.GetGroundsForRetentionAsync(cancellationToken);

        logger.IngestingGroundsForRetention(dri.Count());
        var ingestSize = await ingest.SetAsync(dri, cancellationToken);
        logger.IngestedGroundsForRetention(ingestSize);
    }
}
