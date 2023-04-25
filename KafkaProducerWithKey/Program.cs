using Confluent.Kafka;
using Serilog;
using Shared;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var config = new ProducerConfig { BootstrapServers = "kafka:9092" };

using var producer = new ProducerBuilder<int, Poslog>(config)
    .SetValueSerializer(new JsonSerializer<Poslog>())
    .Build();
var random = new Random();
while (true)
{
    var poslog = new Poslog
    {
        StoreId = random.Next(1, 501),
        DateTime = DateTime.Now,
        Content = "Poslog content",
    };
    try
    {
        await producer.ProduceAsync("poslogs", new()
        {
            Key = poslog.StoreId, 
            Value = poslog,
        });
        logger.Information("Message sent with key {0}.", poslog.StoreId);
    }
    catch (Exception exception)
    {
        logger.Error(exception, "Could not send message.");
    }

    await Task.Delay(TimeSpan.FromSeconds(0.12));
}