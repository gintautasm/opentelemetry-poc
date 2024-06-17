using System.Diagnostics;

namespace search_handler;

public static class Telemetry
{
    public static string serviceName = System.Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "webapi";
    public static readonly ActivitySource SearchHandlerActivitySource = new("custom.webapi.resource");
}
