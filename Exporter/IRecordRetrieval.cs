using Api;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Exporter;

public interface IRecordRetrieval
{
    Task<IEnumerable<RecordOutput>> GetAsync(int offset, CancellationToken cancellationToken);
}