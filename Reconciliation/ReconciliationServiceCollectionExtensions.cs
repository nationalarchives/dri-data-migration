using Api;
using Microsoft.Extensions.DependencyInjection;
using Reconciliation;

namespace Reconciliation;

public static class ReconciliationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddReconciliation()
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
}
