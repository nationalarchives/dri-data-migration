using Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            await etl.RunAsync(driSettings.Value.Code, driSettings.Value.FetchPageSize, cancellationToken);
        }
        logger.MigrationFinished();
    }
}
