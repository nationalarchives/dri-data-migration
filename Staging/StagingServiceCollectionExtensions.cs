using Api;
using Staging;

namespace Microsoft.Extensions.DependencyInjection;

public static class StagingServiceCollectionExtensions
{
    public static IServiceCollection AddStagingIngest(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient<ISparqlClient, StagingSparqlClient>();
        services.AddSingleton<ICacheClient, CacheClient>();
        services.AddSingleton<IStagingIngest<DriAccessCondition>, AccessConditionIngest>();
        services.AddSingleton<IStagingIngest<DriLegislation>, LegislationIngest>();
        services.AddSingleton<IStagingIngest<DriGroundForRetention>, GroundForRetentionIngest>();
        services.AddSingleton<IStagingIngest<DriSubset>, SubsetIngest>();
        services.AddSingleton<IStagingIngest<DriAsset>, AssetIngest>();
        services.AddSingleton<IStagingIngest<DriAssetDeliverableUnit>, AssetDeliverableUnitIngest>();
        services.AddSingleton<IStagingIngest<DriWo409SubsetDeliverableUnit>, Wo409SubsetDeliverableUnitIngest>();
        services.AddSingleton<IStagingIngest<DriVariation>, VariationIngest>();
        services.AddSingleton<IStagingIngest<DriVariationFile>, VariationFileIngest>();
        services.AddSingleton<IStagingIngest<DriSensitivityReview>, SensitivityReviewIngest>();
        services.AddSingleton<IStagingIngest<DriChange>, ChangeIngest>();

        return services;
    }
}
