using Api;
using Microsoft.Extensions.DependencyInjection;
using Staging;

namespace Staging;

public static class StagingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddStagingIngest()
        {
            services.AddMemoryCache();
            services.AddHttpClient<ISparqlClient, StagingSparqlClient>();
            services.AddSingleton<ICacheClient, CacheClient>();
            services.AddTransient<IStagingIngest<DriAccessCondition>, AccessConditionIngest>();
            services.AddTransient<IStagingIngest<DriLegislation>, LegislationIngest>();
            services.AddTransient<IStagingIngest<DriGroundForRetention>, GroundForRetentionIngest>();
            services.AddTransient<IStagingIngest<DriSubset>, SubsetIngest>();
            services.AddTransient<IStagingIngest<DriAsset>, AssetIngest>();
            services.AddTransient<IStagingIngest<DriAssetDeliverableUnit>, AssetDeliverableUnitIngest>();
            services.AddTransient<IStagingIngest<DriWo409SubsetDeliverableUnit>, Wo409SubsetDeliverableUnitIngest>();
            services.AddTransient<IStagingIngest<DriVariation>, VariationIngest>();
            services.AddTransient<IStagingIngest<DriVariationFile>, VariationFileIngest>();
            services.AddTransient<IStagingIngest<DriSensitivityReview>, SensitivityReviewIngest>();
            services.AddTransient<IStagingIngest<DriChange>, ChangeIngest>();

            return services;
        }
    }
}
