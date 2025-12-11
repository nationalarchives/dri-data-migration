using Api;
using Etl;

namespace Microsoft.Extensions.DependencyInjection;

public static class EtlServiceCollectionExtensions
{
    public static IServiceCollection AddMigration(this IServiceCollection services)
    {
        //Order of execution is dictated by EtlStageType
        services.AddTransient<IEtl, EtlAccessCondition>();
        services.AddTransient<IEtl, EtlLegislation>();
        services.AddTransient<IEtl, EtlGroundForRetention>();
        services.AddTransient<IEtl, EtlSubset>();
        services.AddTransient<IEtl, EtlAsset>();
        services.AddTransient<IEtl, EtlVariation>();
        services.AddTransient<IEtl, EtlAssetDeliverableUnit>();
        services.AddTransient<IEtl, EtlWo409SubsetDeliverableUnit>();
        services.AddTransient<IEtl, EtlVariationFile>();
        services.AddTransient<IEtl, EtlSensitivityReview>();
        services.AddTransient<IEtl, EtlChange>();
        services.AddTransient<IDataProcessing, DataProcessing>();

        return services;
    }
}
