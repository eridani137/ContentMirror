using ContentMirror.Application.Parsers.Abstractions;
using ContentMirror.Core.Configs;
using ContentMirror.Core.Entities;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ParserExtension;

namespace ContentMirror.Application.Parsers;

public class TravelqParser(IOptions<ParsingConfig> parsingConfig, ILogger<TravelqParser> logger) : ISiteParser
{
    public string SiteUrl => "https://travelq.ru";
    public CookieJar Cookies { get; set; } = new();

    public string GetPaginationUrl(int number)
    {
        return $"{SiteUrl}/page/{number}/";
    }

    public async Task ParsePage(int page)
    {
        var url = GetPaginationUrl(page);
        logger.LogInformation("Получение новостей со страницы {PageUrl}", url);

        var parse = await url
            .WithHeaders(parsingConfig.Value.Headers)
            .WithCookies(Cookies)
            .GetStringAsync()
            .GetParse();

        if (parse is null)
        {
            logger.LogError("Ошибка получения страницы {PageUrl}", url);
            return;
        }

        var newsXpaths = parse.GetXPaths("//main[@id='genesis-content']/div/article");
        foreach (var newsXpath in newsXpaths)
        {
            try
            {
                var preview = ParsePreview(parse, newsXpath);
                if (preview is null)
                {
                    logger.LogError("Не удалось разобрать HTML: {OuterHtml}", parse.GetOuterHtml(newsXpath));
                    continue;
                }
                
                // TODO: check is contains
                
                logger.LogInformation("Парсинг {Title} [{Url}]", preview.Title, preview.Url);
                
                var newsEntity = new NewsEntity()
                {
                    Preview = preview
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки списка новостей");
            }
        }
    }

    public PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath)
    {
        const string a = "//h2[@class='entry-title']/a";
        
        var title = parse.GetInnerText($"{xpath}{a}");
        var url = parse.GetAttributeValue($"{xpath}{a}");
        var description = parse.GetInnerText($"{xpath}//div[@class='entry-content']/p");

        if (string.IsNullOrEmpty(title) ||
            string.IsNullOrEmpty(url) ||
            string.IsNullOrEmpty(description))
        {
            return null;
        }

        return new PreviewNewsEntity()
        {
            Url = url,
            Title = title,
            Description = description
        };
    }
}