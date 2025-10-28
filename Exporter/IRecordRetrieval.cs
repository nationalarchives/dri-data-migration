namespace Exporter;

public interface IRecordRetrieval
{
    Task<IEnumerable<RecordOutput>> GetAsync(int offset, CancellationToken cancellationToken);
}