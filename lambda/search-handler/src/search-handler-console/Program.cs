using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace search_handler;

public class ILambdaContext
{

}

public static class Telemetry
{
    public static string serviceName = System.Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "search-handler";
    public static readonly ActivitySource SearchHandlerActivitySource = new(serviceName);
    public static Meter SearchHandlerMeter = new(serviceName);
    public static UpDownCounter<int> searchResultsReceived = SearchHandlerMeter.CreateUpDownCounter<int>("search_handler.search_result_received");
}

public class Function
{
    private static async Task Main()
    {
        var connectionString = System.Environment.GetEnvironmentVariable("DB_CONNECTION");

        TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
        // add other instrumentations
            .AddSource(Telemetry.serviceName)
            .ConfigureResource(resource =>
                {
                    resource.AddService(
                        serviceName: Telemetry.serviceName,
                        serviceVersion: "1.0");
                })
            .AddOtlpExporter(
                options =>
                {
                    // most likely should be local host as
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;//OtlpProtocol.HttpProtobuf;
                    options.HttpClientFactory = () =>
                    {
                        HttpClient client = new HttpClient();
                        //client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value");
                        return client;
                    };
                }
            )
        //.AddAWSLambdaConfigurations()
        .Build();

        using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(Telemetry.serviceName)
                .AddOtlpExporter(
                    options =>
                {
                    // most likely should be local host as
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;//OtlpProtocol.HttpProtobuf;
                    options.HttpClientFactory = () =>
                    {
                        HttpClient client = new HttpClient();
                        //client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value");
                        return client;
                    };
                }
                )
                .Build();

        using var log = new LoggerConfiguration()
                .WriteTo
                .OpenTelemetry(options =>
                {
                    // most likely should be local host as
                    options.Endpoint = System.Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4318/v1/logs";
                    options.Protocol = OtlpProtocol.HttpProtobuf;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = Telemetry.serviceName,
                    };
                })
                .WriteTo.Console()
                .CreateLogger();

        Log.Logger = log;

        var kafkaConsumer = new KafkaConsumer(Log.Logger);

        Task.Run(() => kafkaConsumer.ProcessSearchResults());


        Func<string, ILambdaContext, string> handler = FunctionHandler;

        //Task.Run(()=>Con)
        Log.Information("Ctrl+c or Enter to quit");

        while (true)
        {

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;

            if (key.Key == ConsoleKey.Spacebar)
            {
                using var InitSearch = Telemetry.SearchHandlerActivitySource.StartActivity("DebugSearch");
                Log.Information("doing long running job {input}", "debug");
                await kafkaConsumer.TriggerSearch("search-results", searchVal: "debug");
                Log.Information("job done, awaiting command");
            }
            else
            {
                //Do other stuff
                using var InitSearch = Telemetry.SearchHandlerActivitySource.StartActivity("InitSearch");
                Log.Information("doing long running job {input}", key.Key);

                await kafkaConsumer.TriggerSearch(searchVal: key.Key.ToString());
                Log.Information("job done, awaiting command");
            }
        }

    }

    public static string FunctionHandler(string input, ILambdaContext context)
    {

        Log.Information("input={input}", input);

        return input.ToUpper();
    }

}
