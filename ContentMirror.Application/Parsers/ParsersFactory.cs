using ContentMirror.Application.Parsers.Abstractions;

namespace ContentMirror.Application.Parsers;

public class ParsersFactory(IServiceProvider serviceProvider)
{
    public IEnumerable<ISiteParser> GetParsers()
    {
        return serviceProvider.GetServices<ISiteParser>();
    }
}