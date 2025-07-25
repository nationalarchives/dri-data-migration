using System.Threading.Tasks;

namespace Orchestration;

public interface IEtl
{
    Task RunAsync(string code, int limit);
}