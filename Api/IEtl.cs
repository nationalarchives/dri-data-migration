using System.Threading;
using System.Threading.Tasks;

namespace Api;

public interface IEtl
{
    Task RunAsync(int offset, CancellationToken cancellationToken);
    EtlStageType StageType { get; }
}