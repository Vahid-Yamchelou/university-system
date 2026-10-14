using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:Host"] ?? "localhost",
            UserName = "guest",
            Password = "guest"
        };

        await using var conn = await factory.CreateConnectionAsync(ct);
        await using var channel = await conn.CreateChannelAsync(cancellationToken: ct);

        foreach (var queue in new[] { "student.created", "enrollment.created" })
        {
            await channel.QueueDeclareAsync(queue, durable: true,
                exclusive: false, autoDelete: false, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("[{Queue}] E-Mail wird gesendet: {Json}", queue, json);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer, ct);
        }

        await Task.Delay(Timeout.Infinite, ct);
    }
}