using System.Threading.Tasks;

namespace Api;

public interface IEtl
{
    Task RunAsync(string code, int limit);
}