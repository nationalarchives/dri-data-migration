using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDataComparison
{
    Task<ReconciliationSummary> ReconcileAsync(CancellationToken cancellationToken);
}