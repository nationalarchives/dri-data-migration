using Api;
using DriSql;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriSqlServiceCollectionExtensions
{
    public static IServiceCollection AddDriSqlExport(this IServiceCollection services)
    {
        services.AddSingleton<IDriSqlExporter, DriExporter>();

        return services;
    }
}
