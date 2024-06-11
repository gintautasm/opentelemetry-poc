using Amazon.Lambda.Core;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Instrumentation.AWSLambda;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace search_handler_lambda;

public class Function
{
    public static TracerProvider tracerProvider;
    public Function()
    {
        tracerProvider = Sdk.CreateTracerProviderBuilder()
        // add other instrumentations
            .AddSource(Telemetry.serviceName)
            .ConfigureResource(resource =>
                {
                    resource.AddService(
                        serviceName: Telemetry.serviceName,
                        serviceVersion: "1.0");
                })
            .AddOtlpExporter()
            .AddAWSLambdaConfigurations()
            .Build();

        Log.Logger = new LoggerConfiguration()
                .WriteTo
                .OpenTelemetry(options =>
                {
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = Telemetry.serviceName,
                    };
                })
                .WriteTo.Console()
                .CreateLogger();
    }
    /// <summary>
    /// A simple function that takes a string and returns both the upper and lower case version of the string.
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public Casing FunctionHandler(string input, ILambdaContext context)
    {
        using var ProcessSearch = Telemetry.SearchHandlerActivitySource.StartActivity(
                        ActivityKind.Consumer, name: "FunctionHandler");
        return AWSLambdaWrapper.Trace(tracerProvider, (input, context) => {
            Log.Information("=========> received input "+ input);
            //Telemetry.searchResultsReceived.Add(1);
            var result = new Casing(input.ToLower(), input.ToUpper());
            Log.Information("<========= output "+ System.Text.Json.JsonSerializer.Serialize(result));
            return result;
        }, input, context, ProcessSearch.Context);
    }
}

public record Casing(string Lower, string Upper);

public static class Telemetry
{
    public static string serviceName = System.Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "search-handler";
    public static readonly ActivitySource SearchHandlerActivitySource = new(serviceName);
    public static Meter SearchHandlerMeter = new(serviceName);
    public static UpDownCounter<int> searchResultsReceived = SearchHandlerMeter.CreateUpDownCounter<int>("search_handler.search_result_received");
}