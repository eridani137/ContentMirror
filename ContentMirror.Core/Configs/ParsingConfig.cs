namespace ContentMirror.Core.Configs;

public class ParsingConfig
{
    public required Dictionary<string, string> Headers { get; init; }
    public required Dictionary<string, bool> Sites { get; init; }
}