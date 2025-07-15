using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ContentMirror.Application.Configuration;

public static class OpenTelemetryConfiguration
{
    public static void Configure(WebApplicationBuilder builder)
    {
        if (!EnvironmentHelper.IsOtelEnabled())
        {
            return;
        }

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(EnvironmentHelper.GetServiceName(), EnvironmentHelper.GetServiceVersion())
            .AddEnvironmentVariableDetector();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.body.size", request.ContentLength);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.body.size", response.ContentLength);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method.ToString());
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                        };
                    })
                    .AddOtlpExporter(options =>
                    {
                        ConfigureOtlpExporter(options, "traces");
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        ConfigureOtlpExporter(options, "metrics");
                    });
            });
    }

    private static void ConfigureOtlpExporter(OtlpExporterOptions options, string signalType)
    {
        var endpoint = EnvironmentHelper.GetOtlpEndpoint();
        var protocol = EnvironmentHelper.GetOtlpProtocol();
        var headers = EnvironmentHelper.GetOtlpHeaders();
        
        var otlpProtocol = protocol.ToLower() switch
        {
            "http" => OtlpExportProtocol.HttpProtobuf,
            _ => OtlpExportProtocol.Grpc
        };

        var isSeq = endpoint.Contains("seq") || endpoint.Contains("5341");
        
        if (isSeq)
        {
            options.Endpoint = signalType switch
            {
                "traces" => new Uri($"{endpoint}/ingest/otlp/v1/traces"),
                "metrics" => new Uri($"{endpoint}/ingest/otlp/v1/metrics"),
                "logs" => new Uri($"{endpoint}/ingest/otlp/v1/logs"),
                _ => new Uri($"{endpoint}/ingest/otlp/v1/traces")
            };
        }
        else
        {
            options.Endpoint = new Uri(endpoint);
        }

        options.Protocol = otlpProtocol;
        
        if (!string.IsNullOrEmpty(headers))
        {
            options.Headers = headers;
        }

        var timeout = EnvironmentHelper.GetOtlpTimeout();
        options.TimeoutMilliseconds = timeout;
    }
}