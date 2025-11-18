namespace Exporter;

public interface IRecordRetrieval
{
    Task<IEnumerable<RecordOutput>> GetRecordAsync(int offset, CancellationToken cancellationToken);
    Task<IEnumerable<XmlWrapper>> GetXmlAsync(int offset, CancellationToken cancellationToken);
}