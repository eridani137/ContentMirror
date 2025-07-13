using ContentMirror.Core.Configs;
using ContentMirror.Infrastructure;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ParserExtension;

namespace ContentMirror.Application.Services;

public class SiteService(PostsRepository postsRepository, IOptions<SiteConfig> siteConfig, ILogger<SiteService> logger)
{
    private readonly CookieJar _cookies = new();
    private const string SiteUrl = "https://newstravel.online";
    

    public async Task Authorization(CancellationToken ct = default)
    {
        logger.LogInformation("Авторизация на основном сайте...");

        var token = await GetCsrfToken();
        var response = await $"{SiteUrl}/dashboard/account/signin?"
            .WithHeaders(siteConfig.Value.Headers)
            .WithCookies(_cookies)
            .PostAsync(new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["csrf_token"] = token,
                ["email"] = siteConfig.Value.AuthConfig.Email,
                ["password"] = siteConfig.Value.AuthConfig.Password,
                ["remember_me"] = "1"
            }), cancellationToken: ct);

        if (response.StatusCode != 200)
        {
            throw new ApplicationException("Не удалось авторизироваться на основном сайте, проверьте конфигурацию");
        }

        logger.LogInformation("Авторизация прошла успешно");
    }

    public async Task<List<string>> GetPosts()
    {
        logger.LogInformation("Получение уже опубликованных постов...");

        var titles = await postsRepository.GetPostTitles();
        
        logger.LogInformation("Получено {PostsCount} уже опубликованных заголовков", titles.Count);
        
        return titles;
    }
    
    public async Task CreatePost(CancellationToken ct = default)
    {
    }

    private async Task<string> GetCsrfToken()
    {
        var parse = await $"{SiteUrl}/dashboard/account/signin"
            .WithHeaders(siteConfig.Value.Headers)
            .WithCookies(_cookies)
            .GetStringAsync()
            .GetParse();

        if (parse is null) throw new NullReferenceException("Не получилось получить страницу авторизации");

        var token = parse.GetAttributeValue("//input[@name='csrf_token' and @type='hidden' and @value]", "value");
        if (string.IsNullOrEmpty(token)) throw new NullReferenceException("Не удалось получить csrf_token");

        return token;
    }
}