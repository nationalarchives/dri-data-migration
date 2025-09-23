using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlChange(ILogger<EtlChange> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriChange> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.Change;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriChange> dri;
        do
        {
            dri = driExport.GetChanges(offset, cancellationToken).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingChanges(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedChanges(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
