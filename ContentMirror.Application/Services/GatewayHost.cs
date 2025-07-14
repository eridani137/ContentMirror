using ContentMirror.Application.Parsers;
using ContentMirror.Core;
using ContentMirror.Core.Configs;
using ContentMirror.Infrastructure;
using LiteDB;
using Microsoft.Extensions.Options;

namespace ContentMirror.Application.Services;

public class GatewayHost(
    ParsersFactory parsersFactory,
    PostsRepository postsRepository,
    IOptions<ParsingConfig> parsingConfig,
    LiteContext liteContext,
    ILogger<GatewayHost> logger,
    IHostApplicationLifetime lifetime)
    : IHostedService
{
    private Task _worker = null!;

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
                var now = DateTime.Now;
                var parsers = parsersFactory.GetParsers();

                logger.LogInformation("Начало подчистки записей");

                foreach (var parser in parsers)
                {
                    if (parsingConfig.Value.IsExistAndEnabled(parser.SiteUrl) is not { } siteConfig)
                    {
                        logger.LogWarning(
                            "Обработка {Url} пропущена, так как сайт отсутствует или выключен в конфигурации",
                            parser.SiteUrl);
                        continue;
                    }

                    var expireBefore = DateTimeOffset.UtcNow.Subtract(siteConfig.RemovalBy).ToUnixTimeSeconds();
                    var expiredPosts = await postsRepository.GetExpiredPostsByFeedId(parser.FeedId, expireBefore);

                    if (expiredPosts.Count > 0)
                    {
                        logger.LogInformation("Подчистка {ExpiredCount} записей", expiredPosts.Count);

                        foreach (var expiredPost in expiredPosts)
                        {
                            try
                            {
                                await postsRepository.DeletePost(expiredPost.PostId, parser.FeedId);
                                liteContext.RemovedNews.Insert(new NewsEntry()
                                {
                                    Id = ObjectId.NewObjectId(),
                                    Url = expiredPost.PostSource
                                });
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "Ошибка подчистки поста {PostId}", expiredPost.PostId);
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("Подчистка не требуется");
                    }
                }

                var existTitlePosts = await postsRepository.GetPostTitles();
                var removedPosts = liteContext.RemovedNews.FindAll().ToList();

                logger.LogInformation("Начало обработки сайтов");

                foreach (var parser in parsers)
                {
                    if (parsingConfig.Value.IsExistAndEnabled(parser.SiteUrl) is not { } siteConfig)
                    {
                        logger.LogWarning(
                            "Обработка {Url} пропущена, так как сайт отсутствует или выключен в конфигурации",
                            parser.SiteUrl);
                        continue;
                    }

                    logger.LogInformation("Обработка {Url}, максимальная дата новостей {MaxCreatedAt}", parser.SiteUrl,
                        now.Add(-siteConfig.MaxCreatedAt).Date.ToString(StaticData.DateFormat));

                    await foreach (var news in parser.ParseNews(existTitlePosts, removedPosts,
                                       lifetime.ApplicationStopping))
                    {
                        try
                        {
                            await postsRepository.AddPost(news);
                            logger.LogInformation("Новость добавлена: {Title}", news.Preview.Title);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Ошибка при добавлении новости: {SourceUrl}", news.Preview.Url);
                        }
                    }
                }

                logger.LogInformation("Обработка всех сайтов завершена, следующая {DateTime}",
                    now.Add(parsingConfig.Value.Delay).ToString(StaticData.DatetimeFormat));
                await Task.Delay(parsingConfig.Value.Delay, lifetime.ApplicationStopping);
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