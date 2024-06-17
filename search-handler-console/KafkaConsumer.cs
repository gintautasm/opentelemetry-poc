using Confluent.Kafka;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

public class KafkaConsumer //: BackgroundService
{
    ConsumerConfig consumerConfig = null;
    ProducerConfig producerConfig = null;
    ILogger logger = null;
    Random random = new Random();
    public KafkaConsumer(ILogger logger)
    {
        this.logger = logger;
        consumerConfig = new ConsumerConfig
        {
            BootstrapServers = System.Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
            GroupId = "webapi-gr",
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

    public async Task ProcessSearchResults(string topic = "search-results")
    {

        using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
        {
            consumer.Subscribe(topic);
            var cts = new CancellationTokenSource();
            try
            {
                //await Task.Delay(5000);
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
                    using var ProcessSearch = search_handler.Telemetry.SearchHandlerActivitySource.StartActivity(
                        ActivityKind.Consumer, name: "ProcessSearch", links: spanLink);
                    this.logger.Information($"Consumed event from topic {topic}: key = {cr.Message.Key,-10} value = {cr.Message.Value}");
                    var searchResultsReceived = JsonSerializer.Deserialize<SearchResultMessage>(cr.Message.Value);
                    search_handler.Telemetry.searchResultsReceived.Add(searchResultsReceived.Result.Count,
                         new KeyValuePair<string, object?>("Query", searchResultsReceived.Query));
                    await Task.Delay(50);
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

    public async Task TriggerSearch(string topic = "start-search", string searchVal = "pandas")
    {
        using var TriggerSearch = search_handler.Telemetry.SearchHandlerActivitySource.StartActivity(ActivityKind.Producer, name: "TriggerSearch");
        using (var producer = new ProducerBuilder<string, string>(producerConfig).Build())
        {
            var headers = new Headers
            {
                { "Header1", Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()) },
                { "traceparent", Encoding.UTF8.GetBytes(TriggerSearch.Id.ToString()) },
            };
            // get data from db, produce into topic
            // check if traces works
            var partitionVal = random.NextInt64(0, 5);
            this.logger.Information($"producing search results Partition={partitionVal}");
            await producer.ProduceAsync(topic, new Message<string, string>
            {
                Headers = headers,
                Key = $"Key {partitionVal}",
                Value = JsonSerializer.Serialize(new { SearchTrigger = $"search-{searchVal}" })
            });
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessSearchResults();
    }

    public class SearchResultMessage
    {
        public List<int> Result { get; set; }
        public string Query { get; set; }
    }
}
