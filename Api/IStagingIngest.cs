using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IStagingIngest<T> where T : IDriRecord
{
    Task<bool> SetAsync(T record, CancellationToken cancellationToken);
}
