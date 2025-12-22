using Api;
using Exporter;
using Microsoft.Extensions.DependencyInjection;

namespace Exporter;

public static class ExportServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddExporter()
        {
            services.AddHttpClient<IExportSparqlClient, SparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(10));
            services.AddTransient<IRecordRetrieval, RecordRetrieval>();
            services.AddTransient<IOutputGenerator, OutputGenerator>();

            return services;
        }
    }
}
