using System.Text;
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

    public async Task<List<NewsEntity>> ParseNews(CancellationToken ct = default)
    {
        var result = new List<NewsEntity>();

        var i = 1;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var pageResult = await ParsePage(i, ct);
                if (pageResult.Count == 0) break;
                result.AddRange(pageResult);
                break; // TODO
            }
            catch
            {
                break;
            }

            i++;
        }

        return result;
    }

    public async Task<List<NewsEntity>> ParsePage(int page, CancellationToken ct = default)
    {
        var url = GetPaginationUrl(page);
        logger.LogInformation("Получение новостей со страницы {Page} [{PageUrl}]", page, url);

        var parse = await url
            .WithHeaders(parsingConfig.Value.Headers)
            .WithCookies(Cookies)
            .GetStringAsync(cancellationToken: ct)
            .GetParse();

        if (parse is null)
        {
            logger.LogError("Ошибка получения страницы {PageUrl}", url);
            return [];
        }

        var result = new List<NewsEntity>();

        var newsXpaths = parse.GetXPaths("//main[@id='genesis-content']/div/article");
        foreach (var newsXpath in newsXpaths)
        {
            try
            {
                if (ct.IsCancellationRequested) break;
                
                var preview = ParsePreview(parse, newsXpath);
                if (preview is null)
                {
                    logger.LogError("Не удалось разобрать HTML: {OuterHtml}", parse.GetOuterHtml(newsXpath));
                    continue;
                }

                // TODO: check is contains

                var newsEntity = await ParseFullPage(preview, ct);
                
                result.Add(newsEntity);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки списка новостей");
            }
        }

        return result;
    }

    public PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath)
    {
        const string a = "//h2[@class='entry-title']/a";

        var title = parse.GetInnerText($"{xpath}{a}");
        var url = parse.GetAttributeValue($"{xpath}{a}");
        var description = parse.GetInnerText($"{xpath}//div[@class='entry-content']/p");
        var dateString = parse.GetInnerText($"{xpath}//p[@class='entry-meta']");

        if (string.IsNullOrEmpty(title) ||
            string.IsNullOrEmpty(url) ||
            string.IsNullOrEmpty(description) ||
            string.IsNullOrEmpty(dateString))
        {
            return null;
        }

        if (!DateTime.TryParse(dateString, out var date)) return null;

        return new PreviewNewsEntity()
        {
            Url = url,
            Title = title,
            Description = description,
            Date = date
        };
    }

    public async Task<NewsEntity> ParseFullPage(PreviewNewsEntity preview, CancellationToken ct = default)
    {
        logger.LogInformation("Парсинг {Title} [{Url}]", preview.Title, preview.Url);

        var parse = await preview.Url
            .WithHeaders(parsingConfig.Value.Headers)
            .WithCookies(Cookies)
            .GetStringAsync(cancellationToken: ct)
            .GetParse();

        if (parse is null)
        {
            throw new NullReferenceException("parse is null");
        }
        
        const string rootXpath = "//main[@id='genesis-content']/article/div[@class='entry-content' and not(ancestor::footer)]";

        var sb = new StringBuilder();
        
        var articleNodes = parse.GetNodesByXPath($"{rootXpath}/*[not(self::p and starts-with(normalize-space(.), 'По теме:') and a)]");
        foreach (var articleNode in articleNodes)
        {
            sb.AppendLine(articleNode.OuterHtml);
        }

        var result = new NewsEntity()
        {
            Preview = preview,
            Article = sb.ToString()
        };

        return result;
    }
}