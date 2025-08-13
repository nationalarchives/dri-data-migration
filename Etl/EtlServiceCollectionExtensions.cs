using Api;
using Etl;

namespace Microsoft.Extensions.DependencyInjection;

public static class EtlServiceCollectionExtensions
{
    public static IServiceCollection AddMigration(this IServiceCollection services)
    {
        services.AddSingleton<IEtl, EtlAccessCondition>();
        services.AddSingleton<IEtl, EtlLegislation>();
        services.AddSingleton<IEtl, EtlGroundForRetention>();
        services.AddSingleton<IEtl, EtlSubset>();
        services.AddSingleton<IEtl, EtlAsset>();
        services.AddSingleton<IEtl, EtlVariation>();
        services.AddSingleton<IEtl, EtlSensitivityReview>();
        services.AddSingleton<IMigration, Migration>();

        return services;
    }
}
