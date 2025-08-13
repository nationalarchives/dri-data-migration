using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IReconciliationSource
{
    Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken);
}