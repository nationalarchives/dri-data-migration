using Api;
using Microsoft.Extensions.Options;
using Rdf;
using System.Net.Http;

namespace Reconciliation;

public class ReconciliationSparqlClient(HttpClient httpClient, IOptions<ReconciliationSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IReconciliationSparqlClient
{
}
