using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class Migration(ILogger<Migration> logger, IOptions<DriSettings> driSettings, IEnumerable<IEtl> etls) : IMigration
{
    private readonly DriSettings settings = driSettings.Value;

    public async Task MigrateAsync(CancellationToken cancellationToken)
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

            try
            {
                await etl.RunAsync(offset, cancellationToken);
            }
            catch (MigrationException e)
            {
                if (string.IsNullOrWhiteSpace(e.Message))
                {
                    logger.MigrationFailed();
                }
                else
                {
                    logger.MigrationFailedWithMessage(e.Message);
                }
                logger.MigrationFailedDetails(e);
                return;
            }
            catch (TaskCanceledException e)
            {
                logger.ProcessCancelled();
                logger.MigrationFailedDetails(e);
                return;
            }
            catch (Exception e)
            {
                logger.UnhandledException(e.Message);
                logger.MigrationFailedDetails(e);
                return;
            }
        }
        logger.MigrationFinished();
    }
}
