namespace ContentMirror.Application.Parsers.Abstractions;

public interface ISiteParser
{
    public string SiteUrl { get; }
    string GetPaginationSegment(int number);
    Task ParsePage(int page);
}