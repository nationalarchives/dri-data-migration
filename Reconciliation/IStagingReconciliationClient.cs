namespace Reconciliation;

public interface IStagingReconciliationClient
{
    Task<IEnumerable<Dictionary<ReconciliationFieldName, object>>> FetchAsync(
        string code, int pageSize, int offset, CancellationToken cancellationToken);
}
