using Api;

namespace Reconciliation;

public interface IStagingReconciliationClient
{
    Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> FetchAsync(
        ReconciliationMapType mapType, string code, int pageSize, int offset, CancellationToken cancellationToken);
}
