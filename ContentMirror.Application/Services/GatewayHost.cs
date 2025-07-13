using ContentMirror.Application.Parsers;
using ContentMirror.Core;
using ContentMirror.Core.Configs;
using ContentMirror.Infrastructure;
using Microsoft.Extensions.Options;

namespace ContentMirror.Application.Services;

public class GatewayHost(
    ParsersFactory parsersFactory,
    PostsRepository postsRepository,
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
                var existTitlePosts = await postsRepository.GetPostTitles();
                
                logger.LogInformation("Начало обработки сайтов");

                var parsers = parsersFactory.GetParsers();
                var now = DateTime.Now;
                foreach (var parser in parsers)
                {
                    if (parsingConfig.Value.Sites.FirstOrDefault(s => s.Url == parser.SiteUrl) is not { } siteConfig)
                    {
                        logger.LogWarning(
                            "Обработка {Url} пропущена, так как сайт отсутствует в конфигурации",
                            parser.SiteUrl);
                        continue;
                    }

                    if (!siteConfig.IsEnabled)
                    {
                        logger.LogWarning(
                            "Обработка {Url} пропущена, так как сайт отключен в конфигурации",
                            parser.SiteUrl);
                        continue;
                    }

                    logger.LogInformation("Обработка {Url}, максимальная дата новостей {MaxCreatedAt}", parser.SiteUrl,
                        now.Add(-siteConfig.MaxCreatedAt).Date.ToString(StaticData.DateFormat));

                    await foreach (var news in parser.ParseNews(existTitlePosts))
                    {
                        await postsRepository.AddPost(news);
                    }
                }

                logger.LogInformation("Обработка всех сайтов завершена, следующая {DateTime}",
                    now.Add(_delay).ToString(StaticData.DatetimeFormat));
                await Task.Delay(_delay, lifetime.ApplicationStopping);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки");
            }
        }
    }
}