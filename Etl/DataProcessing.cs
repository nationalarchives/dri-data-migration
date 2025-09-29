using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class DataProcessing(ILogger<DataProcessing> logger, IOptions<DriSettings> driSettings, IEnumerable<IEtl> etls) : IDataProcessing
{
    private readonly DriSettings settings = driSettings.Value;

    public async Task EtlAsync(CancellationToken cancellationToken)
    {
        logger.MigrationStarted(settings.Code);
        foreach (var etl in etls.OrderBy(e => (int)e.StageType))
        {
            if (settings.RestartFromStage.HasValue &&
                (int)etl.StageType < (int)settings.RestartFromStage)
            {
                logger.EtlStageSkipped(etl.StageType);
                continue;
            }
            int offset = 0;
            if (settings.RestartFromStage == etl.StageType)
            {
                offset = settings.RestartFromOffset;
            }

            await etl.RunAsync(offset, cancellationToken);
        }
        logger.MigrationFinished();
    }
}
