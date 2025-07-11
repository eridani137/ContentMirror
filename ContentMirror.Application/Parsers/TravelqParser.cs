using ContentMirror.Application.Parsers.Abstractions;

namespace ContentMirror.Application.Parsers;

public class TravelqParser : ISiteParser
{
    public string SiteUrl { get; } = "https://travelq.ru";

    public string GetPaginationSegment(int number)
    {
        return $"{SiteUrl}/page/{number}/";
    }

    public Task ParsePage(int page)
    {
        throw new NotImplementedException();
    }
}