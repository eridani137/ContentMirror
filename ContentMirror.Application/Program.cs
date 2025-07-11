using ContentMirror.Application.Configuration;
using ContentMirror.Core.Configs;
using Serilog;

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
    
    Log.Information("OTLP Endpoint: {Endpoint}", otlpConfig.Endpoint);
    Log.Information("OTLP Token: {Token}", otlpConfig.Token);
    
    builder.Host.UseSerilog(Log.Logger);

    var app = builder.Build();

    app.Run();
}
catch (Exception e)
{
    Log.ForContext<Program>().Fatal(e, "Ошибка инициализации сервиса");
}
finally
{
    await Log.CloseAndFlushAsync();
}