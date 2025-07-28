using Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Migration;

var builder = Host.CreateApplicationBuilder();

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Logging.AddSimpleConsole(configure =>
{
    configure.SingleLine = true;
    configure.IncludeScopes = false;
    configure.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff ";
});

builder.Configuration.AddProgramCommandLine(args);

builder.Services.AddMemoryCache();

builder.Services.AddOptions<StagingSettings>().BindConfiguration(StagingSettings.Prefix);
builder.Services.AddOptions<DriSettings>().BindConfiguration(DriSettings.Prefix);
builder.Services.AddOptions<ReconciliationSettings>().BindConfiguration(ReconciliationSettings.Prefix);

builder.Services.AddDriExport();
builder.Services.AddStagingIngest();
builder.Services.AddMigration();
builder.Services.AddReconciliationClient();
builder.Services.AddReconciliation();

builder.Services.AddHostedService<ProgramHostedService>();

var host = builder.Build();

await host.RunAsync();
