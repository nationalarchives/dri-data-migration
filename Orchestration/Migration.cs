using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orchestration;

public class Migration(ILogger<Migration> logger, IOptions<DriSettings> driSettings, IEnumerable<IEtl> etls)
{
    private readonly int limit = driSettings.Value.FetchPageSize;

    public async Task MigrateAsync(string code)
    {
        logger.MigrationStarted();
        foreach (var etl in etls)
        {
            await etl.RunAsync(code, limit);
        }
        logger.MigrationFinished();
    }
}
