namespace ContentMirror.Core.Configs;

public class SiteConfig
{
    public required Dictionary<string, string> Headers { get; init; }
    public required AuthConfig AuthConfig { get; init; }
}

public class AuthConfig
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}