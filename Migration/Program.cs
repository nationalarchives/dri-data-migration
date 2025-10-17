using Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Migration;
using Serilog;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddProgramCommandLine(args);

builder.Services.AddSerilog((_, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddOptions<StagingSettings>().BindConfiguration(StagingSettings.Prefix);
builder.Services.AddOptions<DriSettings>().BindConfiguration(DriSettings.Prefix);
builder.Services.AddOptions<ReconciliationSettings>().BindConfiguration(ReconciliationSettings.Prefix);
builder.Services.AddOptions<ExportSettings>().BindConfiguration(ExportSettings.Prefix);

builder.Services.AddDriExport();
builder.Services.AddStagingIngest();
builder.Services.AddMigration();
builder.Services.AddReconciliation();
builder.Services.AddExporter();

builder.Services.AddHostedService<ProgramHostedService>();

var host = builder.Build();

await host.RunAsync();
