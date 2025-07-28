using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchestration;

public class Migration(ILogger<Migration> logger, IOptions<DriSettings> driSettings, IEnumerable<IEtl> etls) : IMigration
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        logger.MigrationStarted();
        foreach (var etl in etls)
        {
            await etl.RunAsync(driSettings.Value.Code, driSettings.Value.FetchPageSize, cancellationToken);
        }
        logger.MigrationFinished();
    }
}
