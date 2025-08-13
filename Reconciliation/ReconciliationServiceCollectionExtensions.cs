using Api;
using Reconciliation;

namespace Microsoft.Extensions.DependencyInjection;

public static class ReconciliationServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliation(this IServiceCollection services)
    {
        services.AddHttpClient<IReconciliationSource, DiscoveryRecord>();
        services.AddSingleton<IReconciliationSource, PreservicaClosure>();
        services.AddSingleton<IReconciliationSource, PreservicaMetadata>();
        services.AddSingleton<IReconciliation, Comparer>();

        return services;
    }
}
