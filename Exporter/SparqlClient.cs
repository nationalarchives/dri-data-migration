using Api;
using Microsoft.Extensions.Options;
using Rdf;
using System.Net.Http;

namespace Exporter;

internal class SparqlClient(HttpClient httpClient, IOptions<ExportSettings> settings) : SparqlClientReadOnly(httpClient, settings.Value.SparqlConnectionString), IExportSparqlClient
{
}
