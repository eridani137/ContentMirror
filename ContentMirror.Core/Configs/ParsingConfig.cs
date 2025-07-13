namespace ContentMirror.Core.Configs;

public class ParsingConfig
{
    public required Dictionary<string, string> Headers { get; init; }
    public required List<ParsingSiteConfig> Sites { get; init; }
    public required TimeSpan Delay { get; init; }

    public ParsingSiteConfig? IsExistAndEnabled(string siteUrl)
    {
        if (Sites.FirstOrDefault(s => s.Url == siteUrl) is not { } siteConfig)
        {
            return null;
        }

        if (!siteConfig.IsEnabled)
        {
            return null;
        }

        return siteConfig;
    }
}

public class ParsingSiteConfig
{
    public required string Url { get; set; } 
    public required bool IsEnabled { get; set; }
    public required TimeSpan MaxCreatedAt { get; set; }
    public required TimeSpan RemovalBy { get; set; }
}