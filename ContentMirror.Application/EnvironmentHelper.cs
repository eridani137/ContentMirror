namespace ContentMirror.Application;

public static class EnvironmentHelper
{
    public static string GetOtlpEndpoint() =>
        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";

    public static string GetOtlpProtocol() =>
        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL") ?? "grpc";

    public static string? GetOtlpHeaders() =>
        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");

    public static bool IsOtelEnabled() =>
        !bool.TryParse(Environment.GetEnvironmentVariable("OTEL_ENABLED"), out var enabled) || enabled;

    public static string GetServiceName() =>
        Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "StreamKey.Api";

    public static string GetServiceVersion() =>
        Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION") ?? "1.0.0";

    public static int GetOtlpTimeout() =>
        int.TryParse(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TIMEOUT"), out var timeout)
            ? timeout
            : 30000;
    
    public static string GetEnvironment() => 
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    
    public static string GetSeqEndpoint() => 
        Environment.GetEnvironmentVariable("SEQ_ENDPOINT") ?? 
        Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")?.Replace(":4317", ":5341") ?? 
        "http://localhost:5341";
    
    public static string? GetSeqApiKey() => ExtractApiKeyFromHeaders();

    private static string? ExtractApiKeyFromHeaders()
    {
        var headers = GetOtlpHeaders();
        if (string.IsNullOrEmpty(headers)) return null;
        
        var headerParts = headers.Split('=', 2);
        return headerParts is ["X-Seq-ApiKey", _] ? headerParts[1] : null;
    }
}