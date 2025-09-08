using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etl;

public class Migration(ILogger<Migration> logger, IOptions<DriSettings> driSettings, IEnumerable<IEtl> etls) : IMigration
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        logger.MigrationStarted(driSettings.Value.Code);
        foreach (var etl in etls)
        {
            try
            {
                await etl.RunAsync(cancellationToken);
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
