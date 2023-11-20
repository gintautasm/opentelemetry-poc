using Confluent.Kafka;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public static class Telemetry
{
    public static string serviceName = System.Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "webapi";
    public static readonly ActivitySource SearchHandlerActivitySource = new(serviceName);
}
public class KafkaConsumer : BackgroundService
{
    ConsumerConfig consumerConfig = null;
    ProducerConfig producerConfig = null;
    ILogger<KafkaConsumer> logger = null;
    Random random = new Random();
    public KafkaConsumer(ILogger<KafkaConsumer> logger)
    {
        this.logger = logger;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = System.Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
            GroupId = "search-handler-gr",
            AutoOffsetReset = AutoOffsetReset.Latest,
            AllowAutoCreateTopics = true,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = System.Environment.GetEnvironmentVariable("KAFKA_API_KEY"),
            SaslPassword = System.Environment.GetEnvironmentVariable("KAFKA_API_SECRET")
        };

        producerConfig = new ProducerConfig
        {
            BootstrapServers = System.Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
            AllowAutoCreateTopics = true,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = System.Environment.GetEnvironmentVariable("KAFKA_API_KEY"),
            SaslPassword = System.Environment.GetEnvironmentVariable("KAFKA_API_SECRET")
        };
    }

    public async Task ConsumeMessages(string topic = "start-search")
    {
        using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
        {
            consumer.Subscribe(topic);
            var cts = new CancellationTokenSource();
            try
            {
                await Task.Delay(5000);
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    var traceparent = cr.Message.Headers.FirstOrDefault(h => h.Key == "traceparent");
                    var spanLink = new List<ActivityLink>();
                    if (traceparent != null)
                    {
                        var value = System.Text.Encoding.UTF8.GetString(traceparent.GetValueBytes());
                        ActivityContext.TryParse(value, null, true, out var traceparentContext);
                        spanLink.Add(new ActivityLink(traceparentContext));
                    }
                    using var TriggerSearch = Telemetry.SearchHandlerActivitySource.StartActivity(
                        ActivityKind.Consumer, name: "TriggerSearch", links: spanLink);
                    this.logger.LogInformation($"Consumed event from topic {topic}: key = {cr.Message.Key,-10} value = {cr.Message.Value}");
                    await Task.Delay(50);

                    await ProduceSearchResults();
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl-C was pressed.
            }
            finally
            {
                consumer.Close();
            }

            consumer.Close();
        }
    }

    public async Task ProduceSearchResults(string topic = "search-results")
    {
        using var ProduceSearchResults = Telemetry.SearchHandlerActivitySource.StartActivity(ActivityKind.Producer, name: "ProduceSearchResults");
        using (var producer = new ProducerBuilder<string, string>(producerConfig).Build())
        {
            var headers = new Headers
            {
                { "Header1", Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()) },
                { "traceparent", Encoding.UTF8.GetBytes(ProduceSearchResults?.Id ?? "") },
            };
            // get data from db, produce into topic
            // check if traces works
            var partitionVal = random.NextInt64(0, 5);
            this.logger.LogInformation($"producing search results Partition={partitionVal}");
            await producer.ProduceAsync(topic, new Message<string, string>
            {
                Headers = headers,
                Key = $"Key {partitionVal}",
                Value = JsonSerializer.Serialize(new { prop = "val" })
            });
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConsumeMessages();
    }
}
