using Api;
using Rdf;

namespace Microsoft.Extensions.DependencyInjection;

public static class StagingIngestServiceCollectionExtensions
{
    public static IServiceCollection AddStagingIngest(this IServiceCollection services)
    {
        services.AddSingleton<IStagingIngest<DriAccessCondition>, AccessConditionIngest>();
        services.AddSingleton<IStagingIngest<DriLegislation>, LegislationIngest>();
        services.AddSingleton<IStagingIngest<DriGroundForRetention>, GroundForRetentionIngest>();
        services.AddSingleton<IStagingIngest<DriSubset>, SubsetIngest>();
        services.AddSingleton<IStagingIngest<DriAsset>, AssetIngest>();
        services.AddSingleton<IStagingIngest<DriVariation>, VariationIngest>();
        services.AddSingleton<IStagingIngest<DriSensitivityReview>, SensitivityReviewIngest>();

        return services;
    }
}
