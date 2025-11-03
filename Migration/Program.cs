using Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Migration;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddProgramCommandLine(args);

builder.Logging.AddOpenTelemetry(configure =>
{
    configure.IncludeScopes = true;
    configure.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(configure => configure.AddService("DRI Migration"))
    .WithMetrics(configure => configure.AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution"))
    .WithTracing(configure => configure.AddHttpClientInstrumentation())
    .UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri("http://localhost:4317/"));

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
