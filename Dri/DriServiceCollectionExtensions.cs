using Api;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dri;

public static class DriServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDriExport()
        {
            services.AddHttpClient<IDriSparqlClient, SparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(10));
            services.AddTransient<IDriRdfExporter, RdfExporter>();
            services.AddTransient<IDriSqlExporter, SqlExporter>();

            return services;
        }
    }
}
