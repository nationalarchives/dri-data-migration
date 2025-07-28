using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IEtl
{
    Task RunAsync(string code, int limit, CancellationToken cancellationToken);
}