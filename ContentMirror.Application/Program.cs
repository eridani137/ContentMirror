using ContentMirror.Application;
using ContentMirror.Application.Configuration;
using ContentMirror.Application.Mappings;
using ContentMirror.Application.Services;
using ContentMirror.Core.Configs;
using ContentMirror.Infrastructure;
using Serilog;
using Spectre.Console;

try
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureLogging.Configure();

    builder.Host.UseSerilog(Log.Logger);

    builder.Services.Configure<ParsingConfig>(builder.Configuration.GetSection(nameof(ParsingConfig)));
    var parsingConfig = builder.Configuration.GetSection(nameof(ParsingConfig)).Get<ParsingConfig>();
    if (parsingConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки парсинга");
    }

    builder.Services.Configure<SiteConfig>(builder.Configuration.GetSection(nameof(SiteConfig)));
    var siteConfig = builder.Configuration.GetSection(nameof(SiteConfig)).Get<SiteConfig>();
    if (siteConfig is null)
    {
        throw new ApplicationException("Нужно указать настройки основного сайта");
    }

    builder.Services.AddParsers();
    builder.Services.AddAutoMapper(configuration =>
    {
        configuration.AddProfile<PostProfile>();
    });
    builder.Services.AddSingleton<LiteContext>();
    builder.Services.AddSingleton<ConnectionFactory>();
    builder.Services.AddSingleton<PostsRepository>();
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