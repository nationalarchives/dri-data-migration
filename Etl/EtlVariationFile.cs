using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class EtlVariationFile(ILogger<EtlVariationFile> logger, IOptions<DriSettings> driSettings,
    IDriSqlExporter driExport, IStagingIngest<DriVariationFile> ingest) : IEtl
{
    private readonly DriSettings settings = driSettings.Value;

    public EtlStageType StageType => EtlStageType.VariationFile;

    public async Task RunAsync(int offset, CancellationToken cancellationToken)
    {
        List<DriVariationFile> dri;
        do
        {
            dri = driExport.GetVariationFiles(offset).ToList();
            offset += settings.FetchPageSize;
            logger.IngestingFiles(dri.Count);
            var ingestSize = await ingest.SetAsync(dri, cancellationToken);
            logger.IngestedFiles(ingestSize);
        } while (dri.Any() && dri.Count == settings.FetchPageSize);
    }
}
