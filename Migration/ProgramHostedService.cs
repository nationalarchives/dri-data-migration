using Api;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Migration;

public class ProgramHostedService(ILogger<ProgramHostedService> logger, IConfiguration configuration,
    IDataProcessing dataProcessing, IDataComparison dataComparison, IOutputGenerator generator,
    IHostApplicationLifetime applicationLifetime)
    : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var command = configuration.GetSection("app").GetValue<string>("command");
            switch (command)
            {
                case "migrate":
                    await dataProcessing.EtlAsync(cancellationToken);
                    break;
                case "reconcile":
                    await dataComparison.ReconcileAsync(cancellationToken);
                    break;
                case "export":
                    await generator.GenerateOutputAsync(cancellationToken);
                    break;
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
