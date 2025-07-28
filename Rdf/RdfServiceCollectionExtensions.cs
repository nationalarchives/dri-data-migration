using Api;
using Rdf;

namespace Microsoft.Extensions.DependencyInjection;

public static class RdfServiceCollectionExtensions
{
    public static IServiceCollection AddDriExport(this IServiceCollection services)
    {
        services.AddHttpClient<ISparqlClientReadOnly, DriSparqlClient>();
        services.AddSingleton<IDriExporter, DriExporter>();

        return services;
    }

    public static IServiceCollection AddStagingIngest(this IServiceCollection services)
    {
        services.AddHttpClient<ISparqlClient, StagingSparqlClient>();
        services.AddSingleton<IStagingIngest<DriAccessCondition>, AccessConditionIngest>();
        services.AddSingleton<IStagingIngest<DriLegislation>, LegislationIngest>();
        services.AddSingleton<IStagingIngest<DriGroundForRetention>, GroundForRetentionIngest>();
        services.AddSingleton<IStagingIngest<DriSubset>, SubsetIngest>();
        services.AddSingleton<IStagingIngest<DriAsset>, AssetIngest>();
        services.AddSingleton<IStagingIngest<DriVariation>, VariationIngest>();
        services.AddSingleton<IStagingIngest<DriSensitivityReview>, SensitivityReviewIngest>();

        return services;
    }

    public static IServiceCollection AddReconciliationClient(this IServiceCollection services)
    {
        services.AddHttpClient<ISparqlClientReadOnly, ReconciliationSparqlClient>();
        services.AddSingleton<IStagingReconciliationClient, StagingReconciliationClient>();

        return services;
    }
}
