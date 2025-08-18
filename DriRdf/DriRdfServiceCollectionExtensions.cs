using Api;
using DriRdf;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriRdfServiceCollectionExtensions
{
    public static IServiceCollection AddDriExport(this IServiceCollection services)
    {
        services.AddHttpClient<IDriSparqlClient, DriSparqlClient>();
        services.AddSingleton<IDriRdfExporter, DriExporter>();

        return services;
    }
}
