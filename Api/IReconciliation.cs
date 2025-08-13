using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IReconciliation
{
    Task<ReconciliationSummary> ReconcileAsync(CancellationToken cancellationToken);
}