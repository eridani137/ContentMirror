using ContentMirror.Application.Parsers.Abstractions;

namespace ContentMirror.Application.Parsers;

public class ParsersFactory(IServiceProvider serviceProvider)
{
    public List<ISiteParser> GetParsers()
    {
        return serviceProvider.GetServices<ISiteParser>().ToList();
    }
}