using VDS.RDF;

namespace Exporter;

public interface IRecordRetrieval
{
    Task<IEnumerable<IUriNode>?> GetListAsync(string code, CancellationToken cancellationToken);
    Task<IEnumerable<RecordOutput>> GetRecordAsync(IUriNode id, CancellationToken cancellationToken);
    Task<IEnumerable<XmlWrapper>> GetXmlAsync(IUriNode id, CancellationToken cancellationToken);
}