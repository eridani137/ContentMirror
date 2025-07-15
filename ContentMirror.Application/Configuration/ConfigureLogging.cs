using ContentMirror.Core.Configs;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Spectre;

namespace ContentMirror.Application.Configuration;

public static class ConfigureLogging
{
    public static bool IsConfigured { get; set; }
    public static void Configure()
    {
        const string logs = "logs";
        var logsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logs));

        if (!Directory.Exists(logsPath))
        {
            Directory.CreateDirectory(logsPath);
        }

        const string outputTemplate =
            "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        var levelSwitch = new LoggingLevelSwitch();
        var seqEndpoint = EnvironmentHelper.GetSeqEndpoint();
        var seqApiKey = EnvironmentHelper.GetSeqApiKey();
        var serviceName = EnvironmentHelper.GetServiceName();
        var environment = EnvironmentHelper.GetEnvironment();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("LuckyPennySoftware.AutoMapper.License", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithExceptionDetails()
            .Enrich.WithSpan()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(outputTemplate: outputTemplate, levelSwitch: levelSwitch)
            .WriteTo.File($"{logsPath}/.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate,
                levelSwitch: levelSwitch)
            .WriteTo.Seq(serverUrl: seqEndpoint, apiKey: seqApiKey, controlLevelSwitch: levelSwitch)
            .CreateLogger();
    }
}