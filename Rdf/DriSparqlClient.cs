using Api;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Rdf;

public class DriSparqlClient(HttpClient httpClient, IOptions<DriSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IDriSparqlClient
{
}
