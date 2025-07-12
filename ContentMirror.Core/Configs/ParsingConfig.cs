namespace ContentMirror.Core.Configs;

public class ParsingConfig
{
    public const string DatetimeFormat = "dd-MM-yyyy HH:mm:ss";
    public const string DateFormat = "dd-MM-yyyy";
    public required Dictionary<string, string> Headers { get; init; }
    public required List<SiteConfig> Sites { get; init; }
}

public class SiteConfig
{
    public required string Url { get; set; } 
    public required bool IsEnabled { get; set; }
    public required TimeSpan MaxCreatedAt { get; set; }
}