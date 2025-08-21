using Api;
using Dri;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriServiceCollectionExtensions
{
    public static IServiceCollection AddDriExport(this IServiceCollection services)
    {
        services.AddHttpClient<IDriSparqlClient, SparqlClient>();
        services.AddSingleton<IDriRdfExporter, RdfExporter>();
        services.AddSingleton<IDriSqlExporter, SqlExporter>();

        return services;
    }
}
