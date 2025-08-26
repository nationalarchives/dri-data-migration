using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;

namespace Api;

public interface ISparqlClient : ISparqlClientReadOnly
{
    Task ApplyDiffAsync(GraphDiffReport diffReport, CancellationToken cancellationToken);
    Task UpdateAsync(Triple triple, CancellationToken cancellationToken);
}
