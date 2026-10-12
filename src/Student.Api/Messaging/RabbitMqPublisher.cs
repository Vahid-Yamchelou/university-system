using System.Text.Json;
using RabbitMQ.Client;

namespace Student.Api.Messaging;

public class RabbitMqPublisher : IEventPublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(IConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] ?? "localhost",
            UserName = "guest",
            Password = "guest"
        };
    }

    public async Task PublishAsync<T>(string queue, T message)
    {
        await using var conn = await _factory.CreateConnectionAsync();
        await using var channel = await conn.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        await channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
    }
}