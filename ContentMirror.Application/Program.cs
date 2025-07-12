using ContentMirror.Application;
using ContentMirror.Application.Configuration;
using ContentMirror.Application.Services;
using ContentMirror.Core.Configs;
using Serilog;
using Spectre.Console;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    var otlpConfig = builder.Configuration.GetSection(nameof(OpenTelemetryConfig)).Get<OpenTelemetryConfig>();
    if (otlpConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки OpenTelemetry");
    }

    ConfigureLogging.Configure(otlpConfig);
    OpenTelemetryConfiguration.Configure(builder, otlpConfig);
    
    builder.Host.UseSerilog(Log.Logger);
    
    Log.Information("OTLP Endpoint: {Endpoint}", otlpConfig.Endpoint);
    Log.Information("OTLP Token: {Token}", otlpConfig.Token);

    builder.Services.Configure<ParsingConfig>(builder.Configuration.GetSection(nameof(ParsingConfig)));
    var parsingConfig = builder.Configuration.GetSection(nameof(ParsingConfig)).Get<ParsingConfig>();
    if (parsingConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки парсинга");
    }

    builder.Services.AddParsers();

    builder.Services.AddHostedService<GatewayHost>();

    var app = builder.Build();

    app.Run();
}
catch (Exception e)
{
    if (ConfigureLogging.IsConfigured)
    {
        Log.ForContext<Program>().Fatal(e, "Ошибка инициализации сервиса");
    }
    else
    {
        AnsiConsole.WriteException(e);
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}