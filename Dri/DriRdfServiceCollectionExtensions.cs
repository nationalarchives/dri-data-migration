using Api;
using Dri;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriRdfServiceCollectionExtensions
{
    public static IServiceCollection AddDriExport(this IServiceCollection services)
    {
        services.AddHttpClient<IDriSparqlClient, DriSparqlClient>();
        services.AddSingleton<IDriRdfExporter, RdfExporter>();
        services.AddSingleton<IDriSqlExporter, SqlExporter>();

        return services;
    }
}
