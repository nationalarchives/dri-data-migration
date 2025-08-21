using Api;
using Microsoft.Extensions.Options;
using Rdf;
using System.Net.Http;

namespace Dri;

public class DriSparqlClient(HttpClient httpClient, IOptions<DriSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IDriSparqlClient
{
}
