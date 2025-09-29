using Api;
using Reconciliation;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReconciliationServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services)
    {
        services.AddHttpClient<IReconciliationSparqlClient, ReconciliationSparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(3));
        services.AddSingleton<IStagingReconciliationClient, StagingReconciliationClient>();
        services.AddHttpClient<IReconciliationSource, DiscoveryRecord>();
        services.AddSingleton<IReconciliationSource, PreservicaClosure>();
        services.AddSingleton<IReconciliationSource, PreservicaMetadata>();
        services.AddSingleton<IDataComparison, DataComparison>();

        return services;
    }
}
