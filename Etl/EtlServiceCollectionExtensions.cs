using Api;
using Etl;

namespace Microsoft.Extensions.DependencyInjection;

public static class EtlServiceCollectionExtensions
{
    public static IServiceCollection AddMigration(this IServiceCollection services)
    {
        //Order of execution is dictated by EtlStageType
        services.AddSingleton<IEtl, EtlAccessCondition>();
        services.AddSingleton<IEtl, EtlLegislation>();
        services.AddSingleton<IEtl, EtlGroundForRetention>();
        services.AddSingleton<IEtl, EtlSubset>();
        services.AddSingleton<IEtl, EtlAsset>();
        services.AddSingleton<IEtl, EtlVariation>();
        services.AddSingleton<IEtl, EtlAssetDeliverableUnit>();
        services.AddSingleton<IEtl, EtlWo409SubsetDeliverableUnit>();
        services.AddSingleton<IEtl, EtlVariationFile>();
        services.AddSingleton<IEtl, EtlSensitivityReview>();
        services.AddSingleton<IEtl, EtlChange>();
        services.AddSingleton<IDataProcessing, DataProcessing>();

        return services;
    }
}
