using ContentMirror.Core.Configs;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ParserExtension;

namespace ContentMirror.Application.Services;

public class SiteService(IOptions<SiteConfig> siteConfig)
{
    private readonly CookieJar _cookies = new();
    private const string SiteUrl = "https://newstravel.online";
    
    public async Task Authorization()
    {
        var token = await GetCsrfToken();
        var result = await $"{SiteUrl}/dashboard/account/signin?"
            .WithHeaders(siteConfig.Value.Headers)
            .WithCookies(_cookies)
            .PostAsync(new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["csrf_token"] = token,
                ["email"] = siteConfig.Value.AuthConfig.Email,
                ["password"] = siteConfig.Value.AuthConfig.Password,
                ["remember_me"] = "1"
            }))
            .ReceiveString();
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