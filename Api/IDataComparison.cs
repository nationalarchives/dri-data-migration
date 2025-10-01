using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDataComparison
{
    Task ReconcileAsync(CancellationToken cancellationToken);
}