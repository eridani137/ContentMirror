using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ContentMirror.Application.Parsers.Abstractions;
using ContentMirror.Core.Configs;
using ContentMirror.Core.Entities;
using Flurl.Http;
using Microsoft.Extensions.Options;
using ParserExtension;

namespace ContentMirror.Application.Parsers;

public partial class TravelqParser(IOptions<ParsingConfig> parsingConfig, ILogger<TravelqParser> logger) : ISiteParser
{
    public string SiteUrl => "https://travelq.ru";
    public CookieJar Cookies { get; set; } = new();

    public string GetPaginationUrl(int number)
    {
        return $"{SiteUrl}/page/{number}/";
    }

    public async IAsyncEnumerable<NewsEntity> ParseNews(List<string> existPosts, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var page = await GetLastPage(ct);
        while (!ct.IsCancellationRequested && page > 0)
        {
            await foreach (var news in ParsePage(page, existPosts, ct))
            {
                yield return news;
            }
            
            page--;
        }
    }

    public async IAsyncEnumerable<NewsEntity> ParsePage(int page, List<string> existPosts, [EnumeratorCancellation] CancellationToken ct = default)
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
            yield break;
        }

        var newsXpaths = parse.GetXPaths("//main[@id='genesis-content']/div/article");
        newsXpaths.Reverse();
        foreach (var newsXpath in newsXpaths)
        {
            if (ct.IsCancellationRequested) yield break;

            NewsEntity? newsEntity = null;
            try
            {
                var preview = ParsePreview(parse, newsXpath);
                if (preview is null)
                {
                    logger.LogError("Не удалось разобрать HTML: {OuterHtml}", parse.GetOuterHtml(newsXpath));
                    continue;
                }

                if (existPosts.Contains(preview.Title))
                {
                    logger.LogInformation("Пропускаю новость: {Title} [{Url}]", preview.Title, preview.Url);
                    continue;
                }

                newsEntity = await ParseFullPage(preview, ct);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в цикле обработки новостей на странице {Page}", page);
            }

            if (newsEntity is not null) yield return newsEntity;
        }
    }

    public PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath)
    {
        const string a = "//h2[@class='entry-title']/a";

        var title = parse.GetInnerText($"{xpath}{a}");
        var url = parse.GetAttributeValue($"{xpath}{a}");
        var description = parse.GetInnerText($"{xpath}//div[@class='entry-content']/p");
        var dateString = parse.GetInnerText($"{xpath}//p[@class='entry-meta']");
        var img = parse.GetHighestQualityFromSourceSimple($"{xpath}//header[@class='entry-header']/a/picture/source");

        if (string.IsNullOrEmpty(title) ||
            string.IsNullOrEmpty(url) ||
            string.IsNullOrEmpty(description) ||
            string.IsNullOrEmpty(dateString) ||
            img is null)
        {
            return null;
        }

        if (!DateTime.TryParse(dateString, out var date)) return null;

        return new PreviewNewsEntity()
        {
            Url = url,
            Title = title,
            Description = description,
            Date = date,
            Image = img.Url
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

        const string rootXpath =
            "//main[@id='genesis-content']/article/div[@class='entry-content' and not(ancestor::footer)]";

        var sb = new StringBuilder();

        var articleNodes =
            parse.GetNodesByXPath($"{rootXpath}/*[not(self::p and starts-with(normalize-space(.), 'По теме:') and a)]");
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

    public async Task<int> GetLastPage(CancellationToken ct = default)
    {
        var parse = await SiteUrl
            .WithHeaders(parsingConfig.Value.Headers)
            .WithCookies(Cookies)
            .GetStringAsync(cancellationToken: ct)
            .GetParse();

        if (parse is null) throw new ApplicationException($"Ошибка получения главной страницы {SiteUrl}");

        var hrefs = parse.GetAttributeValues("//div[@role='navigation']/ul/li/a");

        var pagesCount = PageNumberRegex().GetLastPageNumber(hrefs);

        logger.LogInformation("Последняя страница {PagesCount}", pagesCount);
        
        return pagesCount;
    }

    

    [GeneratedRegex(@"/page/(\d+)/?")]
    private static partial Regex PageNumberRegex();
}