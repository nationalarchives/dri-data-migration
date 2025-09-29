using Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Migration;

public class ProgramHostedService(IOptions<StagingSettings> stagingSettings,
    IOptions<ReconciliationSettings> reconciliationSettings,
    IDataProcessing dataProcessing, IDataComparison dataComparison, IHostApplicationLifetime applicationLifetime)
    : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
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
        applicationLifetime.StopApplication();
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
