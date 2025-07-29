using Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Migration;

public class ProgramHostedService(IOptions<StagingSettings> stagingSettings,
    IOptions<ReconciliationSettings> reconciliationSettings,
    IMigration migration, IReconciliation reconciliation, IHostApplicationLifetime applicationLifetime)
    : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        if (stagingSettings?.Value.Code is not null)
        {
            await migration.MigrateAsync(cancellationToken);
        }
        else
        {
            if (reconciliationSettings?.Value.Code is not null)
            {
                await reconciliation.ReconcileAsync(cancellationToken);
            }
            else
            {
                ProgramCommandLineProvider.PrintHelp();
            }
        }
        applicationLifetime.StopApplication();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
