namespace ContentMirror.Core.Configs;

public class ParsingConfig
{
    public required Dictionary<string, string> Headers { get; init; }
    public required List<ParsingSiteConfig> Sites { get; init; }
}

public class ParsingSiteConfig
{
    public required string Url { get; set; } 
    public required bool IsEnabled { get; set; }
    public required TimeSpan MaxCreatedAt { get; set; }
}