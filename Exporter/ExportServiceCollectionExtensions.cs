using Api;
using Exporter;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExportServiceCollectionExtensions
{
    public static IServiceCollection AddExporter(this IServiceCollection services)
    {
        services.AddHttpClient<IExportSparqlClient, SparqlClient>();
        services.AddSingleton<IRecordRetrieval, RecordRetrieval>();
        services.AddSingleton<IOutputGenerator, OutputGenerator>();

        return services;
    }
}
