using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IEtl
{
    Task RunAsync(CancellationToken cancellationToken);
}