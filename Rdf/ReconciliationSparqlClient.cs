using Api;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Rdf;

public class ReconciliationSparqlClient(HttpClient httpClient, IOptions<ReconciliationSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IReconciliationSparqlClient
{
}
