using System.Collections.Generic;
using Confluent.Kafka;
using System.Threading;
using System.Text;
using System.Text.Json;


public class KafkaConsumer : BackgroundService
{


    ConsumerConfig config = null;
    ILogger<KafkaConsumer> logger = null;

    Random random = new Random();

    public KafkaConsumer(ILogger<KafkaConsumer> logger)
    {
        this.logger = logger;
        config = new ConsumerConfig
        {
            BootstrapServers = System.Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
            GroupId = "webapi-gr",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = System.Environment.GetEnvironmentVariable("KAFKA_API_KEY"),
            SaslPassword = System.Environment.GetEnvironmentVariable("KAFKA_API_SECRET")
        };
    }

    public async Task ConsumeMessages(string topic = "start-search")
    {
        using (var consumer = new ConsumerBuilder<string, string>(config).Build())
        {
            consumer.Subscribe(topic);
            var cts = new CancellationTokenSource();
            try
            {
                await Task.Delay(5000);
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
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
        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            var headers = new Headers
            {
                { "Header1", Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()) }
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
