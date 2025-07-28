using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IMigration
{
    Task MigrateAsync(CancellationToken cancellationToken);
}