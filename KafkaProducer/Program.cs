using System.Globalization;
using Confluent.Kafka;
using Serilog;

using var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var config = new ProducerConfig { BootstrapServers = "kafka:9092" };

using var producer = new ProducerBuilder<Null, string>(config).Build();
while (true)
{
    try
    {
        await producer.ProduceAsync("time", new() { Value = DateTime.Now.ToString(CultureInfo.InvariantCulture) });
        logger.Information("Message sent."); 
    }
    catch (Exception exception)
    {
        logger.Error(exception, "Could not send message.");
    }

    await Task.Delay(TimeSpan.FromSeconds(10));
}