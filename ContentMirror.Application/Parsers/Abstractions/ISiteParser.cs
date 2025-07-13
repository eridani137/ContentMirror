using ContentMirror.Core.Entities;
using Flurl.Http;
using ParserExtension;

namespace ContentMirror.Application.Parsers.Abstractions;

public interface ISiteParser
{
    public string SiteUrl { get; }
    public CookieJar Cookies { get; set; }
    string GetPaginationUrl(int number);
    Task<List<NewsEntity>> ParseNews(List<string> existPosts, CancellationToken ct = default);
    Task<List<NewsEntity>> ParsePage(int page, List<string> existPosts, CancellationToken ct = default);
    PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath);
    Task<NewsEntity> ParseFullPage(PreviewNewsEntity preview, CancellationToken ct = default);
}