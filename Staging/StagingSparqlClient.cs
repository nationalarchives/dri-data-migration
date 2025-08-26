using Api;
using Microsoft.Extensions.Options;
using Rdf;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Update;

namespace Staging;

public class StagingSparqlClient(HttpClient httpClient, IOptions<StagingSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), ISparqlClient
{
    private readonly SparqlUpdateClient updateClient = new(httpClient, settings.Value.SparqlUpdateConnectionString);

    public async Task ApplyDiffAsync(GraphDiffReport diffReport, CancellationToken cancellationToken) =>
        await updateClient.UpdateAsync(diffReport.AsUpdate().ToString(), cancellationToken);

    public async Task UpdateAsync(Triple triple, CancellationToken cancellationToken)
    {
        var diff = new GraphDiff();
        var graph = new NonIndexedGraph();
        graph.Assert(triple);
        var report = diff.Difference(new Graph(), graph);
        await updateClient.UpdateAsync(report.AsUpdate().ToString(), cancellationToken);
    }
}
