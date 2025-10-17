using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IOutputGenerator
{
    Task GenerateOutputAsync(CancellationToken cancellationToken);
}