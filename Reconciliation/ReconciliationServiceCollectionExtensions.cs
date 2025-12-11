using Api;
using Reconciliation;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReconciliationServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services)
    {
        services.AddHttpClient<IReconciliationSparqlClient, ReconciliationSparqlClient>(h => h.Timeout = TimeSpan.FromMinutes(10));
        services.AddTransient<IStagingReconciliationClient, StagingReconciliationClient>();
        services.AddHttpClient<IReconciliationSource, DiscoverySource>();
        services.AddTransient<IReconciliationSource, ClosureSource>();
        services.AddTransient<IReconciliationSource, MetadataSource>();
        services.AddTransient<IDataComparison, DataComparison>();

        return services;
    }
}
