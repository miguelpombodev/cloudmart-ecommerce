using Testcontainers.RabbitMq;

namespace Identity.Tests.Api.Common;

public class RabbitMqContainerWrapper : IAsyncDisposable
{
  private readonly RabbitMqContainer _container = new RabbitMqBuilder()
    .WithImage("rabbitmq:4-management")
    .WithUsername("test")
    .WithPassword("test-password")
    .Build();

  public async ValueTask DisposeAsync() =>
    await _container.DisposeAsync();

  public string RetrieveHostName() =>
    $"amqp://teste:teste-password@{_container.Hostname}:{_container.GetMappedPublicPort(5672)}";

  public Task StartAsync() =>
    _container.StartAsync();
}
