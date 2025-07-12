using ContentMirror.Application.Parsers;
using ContentMirror.Core.Configs;
using Microsoft.Extensions.Options;

namespace ContentMirror.Application.Services;

public class GatewayHost(
    ParsersFactory parsersFactory,
    IOptions<ParsingConfig> parsingConfig,
    ILogger<GatewayHost> logger,
    IHostApplicationLifetime lifetime)
    : IHostedService
{
    private Task _worker = null!;
    private readonly TimeSpan _delay = TimeSpan.FromHours(1);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _worker = Worker();
            logger.LogInformation("Сервис запущен");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка запуска сервиса");
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.WhenAny(_worker, Task.Delay(Timeout.Infinite, cancellationToken));
            logger.LogInformation("Сервис остановлен");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка остановки сервиса");
        }
        finally
        {
            _worker.Dispose();
        }
    }

    private async Task Worker()
    {
        await Task.Delay(3000, lifetime.ApplicationStopping);

        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Начало обработки");
                var parsers = parsersFactory.GetParsers();
                foreach (var parser in parsers)
                {
                    if (!parsingConfig.Value.Sites.TryGetValue(parser.SiteUrl, out var isEnabled) || !isEnabled)
                    {
                        logger.LogWarning(
                            "Обработка {Url} пропущена, так как сайт выключен или отстутствует в конфигурации",
                            parser.SiteUrl);
                        continue;
                    }

                    logger.LogInformation("Обработка {Url}", parser.SiteUrl);
                    await parser.ParsePage(1);
                }

                logger.LogInformation("Обработка всех парсеров завершена, следующая {DateTime}",
                    DateTime.Now.Add(_delay));
                await Task.Delay(_delay, lifetime.ApplicationStopping);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки");
            }
        }
    }
}