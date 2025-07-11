using ContentMirror.Core.Entities;
using Flurl.Http;
using ParserExtension;

namespace ContentMirror.Application.Parsers.Abstractions;

public interface ISiteParser
{
    public string SiteUrl { get; }
    public CookieJar Cookies { get; set; }
    string GetPaginationUrl(int number);
    Task ParsePage(int page);
    PreviewNewsEntity? ParsePreview(ParserWrapper parse, string xpath);
}