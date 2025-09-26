using Api;
using Microsoft.Extensions.Options;
using Rdf;

namespace Reconciliation;

public class ReconciliationSparqlClient(HttpClient httpClient, IOptions<ReconciliationSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IReconciliationSparqlClient
{
}
