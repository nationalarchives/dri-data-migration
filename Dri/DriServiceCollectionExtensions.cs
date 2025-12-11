using Api;
using Dri;
using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriServiceCollectionExtensions
{
    public static IServiceCollection AddDriExport(this IServiceCollection services)
    {
        services.AddHttpClient<IDriSparqlClient, SparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(10));
        services.AddTransient<IDriRdfExporter, RdfExporter>();
        services.AddTransient<IDriSqlExporter, SqlExporter>();

        return services;
    }
}
