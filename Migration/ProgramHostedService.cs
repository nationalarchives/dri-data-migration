using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Migration;

public class ProgramHostedService(ILogger<ProgramHostedService> logger,
    IOptions<StagingSettings> stagingSettings, IOptions<ReconciliationSettings> reconciliationSettings,
    IDataProcessing dataProcessing, IDataComparison dataComparison, IHostApplicationLifetime applicationLifetime)
    : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (stagingSettings?.Value.Code is not null)
            {
                await dataProcessing.EtlAsync(cancellationToken);
            }
            else
            {
                if (reconciliationSettings?.Value.Code is not null)
                {
                    await dataComparison.ReconcileAsync(cancellationToken);
                }
            }
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
        }
        catch (TaskCanceledException e)
        {
            logger.ProcessCancelled();
            logger.MigrationFailedDetails(e);
        }
        catch (SqliteException e) when (e.SqliteErrorCode == SQLitePCL.raw.SQLITE_INTERRUPT)
        {
            logger.ProcessCancelled();
            logger.MigrationFailedDetails(e);
        }
        catch (Exception e)
        {
            logger.UnhandledException(e.Message);
            logger.MigrationFailedDetails(e);
        }
        applicationLifetime.StopApplication();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
