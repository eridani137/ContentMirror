using ContentMirror.Core.Entities;
using ContentMirror.Infrastructure;
using Flurl.Http;
using ParserExtension;

namespace ContentMirror.Application.Parsers.Abstractions;

public interface ISiteParser
{
    public int FeedId { get; }
    public string SiteUrl { get; }
    public CookieJar Cookies { get; set; }
    string GetPaginationUrl(int number);
    IAsyncEnumerable<NewsEntity> ParseNews(List<string> existPosts, List<NewsEntry> removedPosts, CancellationToken ct = default);
    IAsyncEnumerable<NewsEntity> ParsePage(int page, List<string> existPosts, List<NewsEntry> removedPosts, CancellationToken ct = default);
    PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath);
    Task<NewsEntity> ParseFullPage(PreviewNewsEntity preview, CancellationToken ct = default);
    Task<int> GetLastPage(CancellationToken ct = default);
}