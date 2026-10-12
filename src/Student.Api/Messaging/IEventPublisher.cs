namespace Student.Api.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string queue, T message);
}