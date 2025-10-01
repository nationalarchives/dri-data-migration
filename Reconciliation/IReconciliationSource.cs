namespace Reconciliation;

public interface IReconciliationSource
{
    Task<List<Dictionary<ReconciliationFieldName, object>>> GetExpectedDataAsync(CancellationToken cancellationToken);
}