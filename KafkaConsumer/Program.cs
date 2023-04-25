using Confluent.Kafka;
using Serilog;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var config = new ConsumerConfig
{
    BootstrapServers = "kafka:9092",
    GroupId = "csharp-consumer",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("time");

while (true)
{
    var consumeResult = consumer.Consume();
    if (consumeResult.IsPartitionEOF)
        continue;
    logger.Information("Received message {0}", consumeResult.Message.Value);
    await Task.Delay(TimeSpan.FromSeconds(1));
}
