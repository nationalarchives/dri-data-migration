using Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Migration;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddProgramCommandLine(args);

builder.Services.AddOptions<StagingSettings>().BindConfiguration(StagingSettings.Prefix);
builder.Services.AddOptions<DriSettings>().BindConfiguration(DriSettings.Prefix);
builder.Services.AddOptions<ReconciliationSettings>().BindConfiguration(ReconciliationSettings.Prefix);

builder.Services.AddDriRdfExport();
builder.Services.AddDriSqlExport();
builder.Services.AddStagingIngest();
builder.Services.AddMigration();
builder.Services.AddReconciliation();

builder.Services.AddHostedService<ProgramHostedService>();

var host = builder.Build();

await host.RunAsync();
