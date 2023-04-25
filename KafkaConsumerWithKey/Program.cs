using Confluent.Kafka;
using Serilog;
using Shared;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var config = new ConsumerConfig
{
    BootstrapServers = "kafka:9092",
    GroupId = "csharp-consumer-key",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

using var consumer = new ConsumerBuilder<int, Poslog>(config)
    .SetValueDeserializer(new JsonSerializer<Poslog>())
    .Build();
consumer.Subscribe("poslogs");

while (true)
{
    var consumeResult = consumer.Consume();
    if (consumeResult.IsPartitionEOF)
        continue;
    logger.Information(
        "Received message with key: {0}, value: {1}", 
        consumeResult.Message.Key, 
        consumeResult.Message.Value);
    await Task.Delay(TimeSpan.FromSeconds(0.12));
}
