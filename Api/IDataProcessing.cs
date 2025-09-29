using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IDataProcessing
{
    Task EtlAsync(CancellationToken cancellationToken);
}