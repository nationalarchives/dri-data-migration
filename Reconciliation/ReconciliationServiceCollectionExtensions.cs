using Api;
using Reconciliation;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReconciliationServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services)
    {
        services.AddHttpClient<IReconciliationSparqlClient, ReconciliationSparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(3));
        services.AddSingleton<IStagingReconciliationClient, StagingReconciliationClient>();
        services.AddHttpClient<IReconciliationSource, DiscoverySource>();
        services.AddSingleton<IReconciliationSource, ClosureSource>();
        services.AddSingleton<IReconciliationSource, MetadataSource>();
        services.AddSingleton<IDataComparison, DataComparison>();

        return services;
    }
}
